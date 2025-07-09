using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.Pages.Exercises;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Builders;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetFitterGetBigger.Admin.Tests.Components.Pages.Exercises
{
    public class ExerciseListWithLinksTests : TestContext
    {
        private readonly Mock<IExerciseStateService> _stateServiceMock;
        private readonly Mock<NavigationManager> _navigationManagerMock;
        private readonly List<ReferenceDataDto> _difficultyLevels;
        private readonly List<ReferenceDataDto> _muscleGroups;

        public ExerciseListWithLinksTests()
        {
            _stateServiceMock = new Mock<IExerciseStateService>();
            _navigationManagerMock = new Mock<NavigationManager>();

            _difficultyLevels = new List<ReferenceDataDto>
            {
                new() { Id = "1", Value = "Beginner" },
                new() { Id = "2", Value = "Intermediate" }
            };

            _muscleGroups = new List<ReferenceDataDto>
            {
                new() { Id = "1", Value = "Chest" },
                new() { Id = "2", Value = "Back" }
            };

            // Setup default state
            _stateServiceMock.SetupGet(x => x.DifficultyLevels).Returns(_difficultyLevels);
            _stateServiceMock.SetupGet(x => x.MuscleGroups).Returns(_muscleGroups);
            _stateServiceMock.SetupGet(x => x.IsLoading).Returns(false);
            _stateServiceMock.SetupGet(x => x.ErrorMessage).Returns((string?)null);
            _stateServiceMock.SetupGet(x => x.HasStoredPage).Returns(false);
            _stateServiceMock.SetupGet(x => x.CurrentFilter).Returns(new ExerciseFilterDto { Page = 1, PageSize = 10 });
        }

        [Fact]
        public void ExerciseList_ShowsLinkIndicator_WhenExerciseHasLinks()
        {
            // Arrange
            var exerciseWithLinks = new ExerciseListDtoBuilder()
                .WithId("ex1")
                .WithName("Barbell Squat")
                .WithWarmupLinkCount(3)
                .WithCooldownLinkCount(2)
                .Build();

            var exerciseWithoutLinks = new ExerciseListDtoBuilder()
                .WithId("ex2")
                .WithName("Bench Press")
                .WithWarmupLinkCount(0)
                .WithCooldownLinkCount(0)
                .Build();

            var pagedResult = new ExercisePagedResultDtoBuilder()
                .WithExercises(exerciseWithLinks, exerciseWithoutLinks)
                .WithTotalCount(2)
                .Build();

            _stateServiceMock.SetupGet(x => x.CurrentPage).Returns(pagedResult);

            Services.AddSingleton(_stateServiceMock.Object);
            Services.AddSingleton(_navigationManagerMock.Object);

            // Act
            var component = RenderComponent<ExerciseList>();

            // Assert
            var linkIndicators = component.FindAll("[data-testid='link-indicator']");
            linkIndicators.Should().HaveCount(1);
            
            var linkIcon = component.Find("[data-testid='link-icon']");
            linkIcon.Should().NotBeNull();
            
            var linkCounts = component.Find("[data-testid='link-counts']");
            linkCounts.TextContent.Should().Be("3/2");
        }

        [Fact]
        public void ExerciseList_ShowsCorrectLinkCount_ForDifferentLinkTypes()
        {
            // Arrange
            var warmupOnlyExercise = new ExerciseListDtoBuilder()
                .WithId("ex1")
                .WithName("Leg Swings")
                .WithWarmupLinkCount(5)
                .WithCooldownLinkCount(0)
                .Build();

            var cooldownOnlyExercise = new ExerciseListDtoBuilder()
                .WithId("ex2")
                .WithName("Quad Stretch")
                .WithWarmupLinkCount(0)
                .WithCooldownLinkCount(3)
                .Build();

            var pagedResult = new ExercisePagedResultDtoBuilder()
                .WithExercises(warmupOnlyExercise, cooldownOnlyExercise)
                .WithTotalCount(2)
                .Build();

            _stateServiceMock.SetupGet(x => x.CurrentPage).Returns(pagedResult);

            Services.AddSingleton(_stateServiceMock.Object);
            Services.AddSingleton(_navigationManagerMock.Object);

            // Act
            var component = RenderComponent<ExerciseList>();

            // Assert
            var linkCounts = component.FindAll("[data-testid='link-counts']");
            linkCounts.Should().HaveCount(2);
            linkCounts[0].TextContent.Should().Be("5 W");
            linkCounts[1].TextContent.Should().Be("3 C");
        }

        [Fact]
        public void ExerciseList_ShowsTooltip_OnLinkIndicatorHover()
        {
            // Arrange
            var exercise = new ExerciseListDtoBuilder()
                .WithId("ex1")
                .WithName("Barbell Squat")
                .WithWarmupLinkCount(3)
                .WithCooldownLinkCount(2)
                .Build();

            var pagedResult = new ExercisePagedResultDtoBuilder()
                .WithExercises(exercise)
                .WithTotalCount(1)
                .Build();

            _stateServiceMock.SetupGet(x => x.CurrentPage).Returns(pagedResult);

            Services.AddSingleton(_stateServiceMock.Object);
            Services.AddSingleton(_navigationManagerMock.Object);

            // Act
            var component = RenderComponent<ExerciseList>();

            // Assert
            var tooltip = component.Find("[data-testid='link-tooltip']");
            tooltip.Should().NotBeNull();
            tooltip.InnerHtml.Should().Contain("3 warmup exercises");
            tooltip.InnerHtml.Should().Contain("2 cooldown exercises");
        }

        [Fact]
        public void ExerciseList_ShowsHasLinksFilter_InFilterSection()
        {
            // Arrange
            SetupEmptyPagedResult();
            
            Services.AddSingleton(_stateServiceMock.Object);
            Services.AddSingleton(_navigationManagerMock.Object);

            // Act
            var component = RenderComponent<ExerciseList>();

            // Assert
            var filterLabel = component.FindAll("label").FirstOrDefault(l => l.TextContent == "Links");
            filterLabel.Should().NotBeNull();
            
            var filterSelect = filterLabel!.NextElementSibling;
            filterSelect.Should().NotBeNull();
            
            var options = filterSelect!.QuerySelectorAll("option");
            options.Should().HaveCount(3);
            options[0].TextContent.Should().Be("All Exercises");
            options[1].TextContent.Should().Be("Has Links");
            options[2].TextContent.Should().Be("No Links");
        }

        [Fact]
        public async Task ExerciseList_AppliesHasLinksFilter_WhenFilterIsSelected()
        {
            // Arrange
            SetupEmptyPagedResult();
            
            Services.AddSingleton(_stateServiceMock.Object);
            Services.AddSingleton(_navigationManagerMock.Object);

            var component = RenderComponent<ExerciseList>();

            // Act
            var filterSelect = component.FindAll("select").FirstOrDefault(s => s.InnerHtml.Contains("Has Links"));
            filterSelect.Should().NotBeNull();
            
            await filterSelect!.ChangeAsync(new ChangeEventArgs { Value = "true" });
            
            var applyButton = component.FindAll("button").First(b => b.TextContent.Contains("Apply Filter"));
            await applyButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

            // Assert
            _stateServiceMock.Verify(x => x.LoadExercisesAsync(It.Is<ExerciseFilterDto>(f => 
                f.HasLinks == true
            )), Times.Once);
        }

        [Fact]
        public async Task ExerciseList_ClearsHasLinksFilter_WhenClearIsClicked()
        {
            // Arrange
            SetupEmptyPagedResult();
            
            Services.AddSingleton(_stateServiceMock.Object);
            Services.AddSingleton(_navigationManagerMock.Object);

            var component = RenderComponent<ExerciseList>();

            // Set filter
            var filterSelect = component.FindAll("select").FirstOrDefault(s => s.InnerHtml.Contains("Has Links"));
            await filterSelect!.ChangeAsync(new ChangeEventArgs { Value = "true" });

            // Act
            var clearButton = component.FindAll("button").First(b => b.TextContent.Contains("Clear"));
            await clearButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

            // Assert
            _stateServiceMock.Verify(x => x.LoadExercisesAsync(It.Is<ExerciseFilterDto>(f => 
                f.HasLinks == null
            )), Times.Once);
            
            // Verify UI is cleared
            filterSelect!.GetAttribute("value").Should().Be("");
        }

        [Fact]
        public void ExerciseList_RestoresHasLinksFilter_WhenReturningFromExerciseForm()
        {
            // Arrange
            var filter = new ExerciseFilterDto
            {
                Page = 1,
                PageSize = 10,
                HasLinks = true
            };

            _stateServiceMock.SetupGet(x => x.HasStoredPage).Returns(true);
            _stateServiceMock.SetupGet(x => x.CurrentFilter).Returns(filter);
            
            SetupEmptyPagedResult();
            
            Services.AddSingleton(_stateServiceMock.Object);
            Services.AddSingleton(_navigationManagerMock.Object);

            // Act
            var component = RenderComponent<ExerciseList>();

            // Assert
            var filterSelect = component.FindAll("select").FirstOrDefault(s => s.InnerHtml.Contains("Has Links"));
            filterSelect!.GetAttribute("value").Should().Be("true");
        }

        [Fact]
        public void ExerciseList_ShowsCorrectTooltipText_ForSingularExercise()
        {
            // Arrange
            var exercise = new ExerciseListDtoBuilder()
                .WithId("ex1")
                .WithName("Plank")
                .WithWarmupLinkCount(1)
                .WithCooldownLinkCount(1)
                .Build();

            var pagedResult = new ExercisePagedResultDtoBuilder()
                .WithExercises(exercise)
                .WithTotalCount(1)
                .Build();

            _stateServiceMock.SetupGet(x => x.CurrentPage).Returns(pagedResult);

            Services.AddSingleton(_stateServiceMock.Object);
            Services.AddSingleton(_navigationManagerMock.Object);

            // Act
            var component = RenderComponent<ExerciseList>();

            // Assert
            var tooltip = component.Find("[data-testid='link-tooltip']");
            tooltip.InnerHtml.Should().Contain("1 warmup exercise");
            tooltip.InnerHtml.Should().Contain("1 cooldown exercise");
            tooltip.InnerHtml.Should().NotContain("exercises");
        }

        [Fact]
        public void ExerciseList_DoesNotShowLinkIndicator_ForExercisesWithoutLinks()
        {
            // Arrange
            var exercise = new ExerciseListDtoBuilder()
                .WithId("ex1")
                .WithName("Bicep Curl")
                .WithWarmupLinkCount(0)
                .WithCooldownLinkCount(0)
                .Build();

            var pagedResult = new ExercisePagedResultDtoBuilder()
                .WithExercises(exercise)
                .WithTotalCount(1)
                .Build();

            _stateServiceMock.SetupGet(x => x.CurrentPage).Returns(pagedResult);

            Services.AddSingleton(_stateServiceMock.Object);
            Services.AddSingleton(_navigationManagerMock.Object);

            // Act
            var component = RenderComponent<ExerciseList>();

            // Assert
            var linkIndicators = component.FindAll("[data-testid='link-indicator']");
            linkIndicators.Should().BeEmpty();
        }

        private void SetupEmptyPagedResult()
        {
            var pagedResult = new ExercisePagedResultDto
            {
                Items = new List<ExerciseListDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            };
            
            _stateServiceMock.SetupGet(x => x.CurrentPage).Returns(pagedResult);
        }
    }
}