using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.Pages.Exercises;
using GetFitterGetBigger.Admin.Components.Pages.Exercises.ExerciseLinks;
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
    public class ExerciseDetailWithLinksTests : TestContext
    {
        private readonly Mock<IExerciseStateService> _exerciseStateServiceMock;
        private readonly Mock<IExerciseLinkStateService> _linkStateServiceMock;
        private readonly Mock<IExerciseService> _exerciseServiceMock;
        private readonly Mock<IExerciseLinkValidationService> _validationServiceMock;
        private readonly Mock<NavigationManager> _navigationManagerMock;
        private readonly ExerciseDto _workoutExercise;
        private readonly ExerciseDto _nonWorkoutExercise;
        private readonly List<ExerciseTypeDto> _exerciseTypes;

        public ExerciseDetailWithLinksTests()
        {
            _exerciseStateServiceMock = new Mock<IExerciseStateService>();
            _validationServiceMock = new Mock<IExerciseLinkValidationService>();
            _linkStateServiceMock = new Mock<IExerciseLinkStateService>();
            _exerciseServiceMock = new Mock<IExerciseService>();
            _navigationManagerMock = new Mock<NavigationManager>();

            _exerciseTypes = new List<ExerciseTypeDto>
            {
                new() { Id = "1", Value = "Warmup", Description = "Warmup exercises" },
                new() { Id = "2", Value = "Workout", Description = "Main workout" },
                new() { Id = "3", Value = "Cooldown", Description = "Cooldown exercises" }
            };

            _workoutExercise = new ExerciseDtoBuilder()
                .WithId("ex1")
                .WithName("Barbell Squat")
                .WithDescription("A compound leg exercise")
                .WithDifficulty("Intermediate")
                .WithExerciseTypes(("Workout", "Main workout"))
                .WithKineticChain("Compound", "Multi-joint movement")
                .Build();

            _nonWorkoutExercise = new ExerciseDtoBuilder()
                .WithId("ex2")
                .WithName("Arm Circles")
                .WithDescription("A warmup exercise")
                .WithDifficulty("Beginner")
                .WithExerciseTypes(("Warmup", "Warmup exercise"))
                .Build();

            // Setup default state service behavior
            _exerciseStateServiceMock.SetupGet(x => x.ExerciseTypes).Returns(_exerciseTypes);
            _exerciseStateServiceMock.SetupGet(x => x.IsLoadingExercise).Returns(false);
            _exerciseStateServiceMock.SetupGet(x => x.ErrorMessage).Returns((string?)null);

            // Register the validation service
            Services.AddSingleton(_validationServiceMock.Object);
        }

        [Fact]
        public void ExerciseDetail_ShowsExerciseLinkManager_ForWorkoutType()
        {
            // Arrange
            _exerciseStateServiceMock.SetupGet(x => x.SelectedExercise).Returns(_workoutExercise);
            SetupLinkStateWithNoLinks();

            Services.AddSingleton(_exerciseStateServiceMock.Object);
            Services.AddSingleton(_linkStateServiceMock.Object);
            Services.AddSingleton(_exerciseServiceMock.Object);
            Services.AddSingleton(_validationServiceMock.Object);
            Services.AddSingleton(_navigationManagerMock.Object);

            // Act
            var component = RenderComponent<ExerciseDetail>(parameters => parameters
                .Add(p => p.Id, "ex1"));

            // Assert
            component.FindComponent<FourWayExerciseLinkManager>().Should().NotBeNull();
            // Verify component exists through data-testid, not magic strings
            component.Find("[data-testid='four-way-exercise-link-manager']").Should().NotBeNull();
        }

        [Fact]
        public void ExerciseDetail_ShowsExerciseLinkManager_ForAllExerciseTypes()
        {
            // Arrange
            _exerciseStateServiceMock.SetupGet(x => x.SelectedExercise).Returns(_nonWorkoutExercise);
            SetupLinkStateWithNoLinks();

            Services.AddSingleton(_exerciseStateServiceMock.Object);
            Services.AddSingleton(_linkStateServiceMock.Object);
            Services.AddSingleton(_exerciseServiceMock.Object);
            Services.AddSingleton(_validationServiceMock.Object);
            Services.AddSingleton(_navigationManagerMock.Object);

            // Act
            var component = RenderComponent<ExerciseDetail>(parameters => parameters
                .Add(p => p.Id, "ex2"));

            // Assert - Now all exercise types show the FourWayExerciseLinkManager
            component.FindComponent<FourWayExerciseLinkManager>().Should().NotBeNull();
            // Verify component exists through data-testid, not magic strings
            component.Find("[data-testid='four-way-exercise-link-manager']").Should().NotBeNull();
        }

        [Fact]
        public async Task ExerciseDetail_InitializesLinkStateService_ForWorkoutType()
        {
            // Arrange
            _exerciseStateServiceMock.SetupGet(x => x.SelectedExercise).Returns(_workoutExercise);
            SetupLinkStateWithNoLinks();

            Services.AddSingleton(_exerciseStateServiceMock.Object);
            Services.AddSingleton(_linkStateServiceMock.Object);
            Services.AddSingleton(_exerciseServiceMock.Object);
            Services.AddSingleton(_validationServiceMock.Object);
            Services.AddSingleton(_navigationManagerMock.Object);

            // Act
            var component = RenderComponent<ExerciseDetail>(parameters => parameters
                .Add(p => p.Id, "ex1"));

            // Wait for async initialization
            await Task.Delay(50);

            // Assert
            _linkStateServiceMock.Verify(x => x.InitializeForExerciseAsync("ex1", "Barbell Squat"), Times.Once);
        }

        [Fact]
        public void ExerciseDetail_DisplaysLinkedExercises_WhenPresent()
        {
            // Arrange
            _exerciseStateServiceMock.SetupGet(x => x.SelectedExercise).Returns(_workoutExercise);

            var warmupLink = new ExerciseLinkDtoBuilder()
                .WithId("link1")
                .WithTargetExercise("Leg Swings", "ex3")
                .AsWarmup()
                .Build();

            var cooldownLink = new ExerciseLinkDtoBuilder()
                .WithId("link2")
                .WithTargetExercise("Quad Stretch", "ex4")
                .AsCooldown()
                .Build();

            SetupLinkStateWithLinks(new List<ExerciseLinkDto> { warmupLink }, new List<ExerciseLinkDto> { cooldownLink });

            Services.AddSingleton(_exerciseStateServiceMock.Object);
            Services.AddSingleton(_linkStateServiceMock.Object);
            Services.AddSingleton(_exerciseServiceMock.Object);
            Services.AddSingleton(_validationServiceMock.Object);
            Services.AddSingleton(_navigationManagerMock.Object);

            // Act
            var component = RenderComponent<ExerciseDetail>(parameters => parameters
                .Add(p => p.Id, "ex1"));

            // Assert
            // Verify sections exist through data-testid
            component.Find("[data-testid='warmup-section']").Should().NotBeNull();
            component.Find("[data-testid='cooldown-section']").Should().NotBeNull();
            
            // Verify links are present through structure
            var warmupLinks = component.FindAll("[data-testid='warmup-links-container'] [role='listitem']");
            var cooldownLinks = component.FindAll("[data-testid='cooldown-links-container'] [role='listitem']");
            warmupLinks.Should().HaveCountGreaterThan(0);
            cooldownLinks.Should().HaveCountGreaterThan(0);
        }

        [Fact]
        public void ExerciseDetail_ShowsEmptyStates_WhenNoLinks()
        {
            // Arrange
            _exerciseStateServiceMock.SetupGet(x => x.SelectedExercise).Returns(_workoutExercise);
            SetupLinkStateWithNoLinks();

            Services.AddSingleton(_exerciseStateServiceMock.Object);
            Services.AddSingleton(_linkStateServiceMock.Object);
            Services.AddSingleton(_exerciseServiceMock.Object);
            Services.AddSingleton(_validationServiceMock.Object);
            Services.AddSingleton(_navigationManagerMock.Object);

            // Act
            var component = RenderComponent<ExerciseDetail>(parameters => parameters
                .Add(p => p.Id, "ex1"));

            // Assert
            // Verify empty state is shown through data-testid
            component.Find("[data-testid='warmup-empty-state']").Should().NotBeNull();
            component.Find("[data-testid='cooldown-empty-state']").Should().NotBeNull();
        }

        [Fact]
        public void ExerciseDetail_ShowsLoadingState_WhenLinksLoading()
        {
            // Arrange
            _exerciseStateServiceMock.SetupGet(x => x.SelectedExercise).Returns(_workoutExercise);
            SetupLinkStateWithNoLinks();
            // Override IsLoading after setup
            _linkStateServiceMock.SetupGet(x => x.IsLoading).Returns(true);

            Services.AddSingleton(_exerciseStateServiceMock.Object);
            Services.AddSingleton(_linkStateServiceMock.Object);
            Services.AddSingleton(_exerciseServiceMock.Object);
            Services.AddSingleton(_validationServiceMock.Object);
            Services.AddSingleton(_navigationManagerMock.Object);

            // Act
            var component = RenderComponent<ExerciseDetail>(parameters => parameters
                .Add(p => p.Id, "ex1"));

            // Assert
            component.Find("[data-testid='loading-spinner']").Should().NotBeNull();
        }

        [Fact]
        public void ExerciseDetail_ShowsAddButtons_WhenUnderMaxCapacity()
        {
            // Arrange
            _exerciseStateServiceMock.SetupGet(x => x.SelectedExercise).Returns(_workoutExercise);
            SetupLinkStateWithNoLinks();

            Services.AddSingleton(_exerciseStateServiceMock.Object);
            Services.AddSingleton(_linkStateServiceMock.Object);
            Services.AddSingleton(_exerciseServiceMock.Object);
            Services.AddSingleton(_validationServiceMock.Object);
            Services.AddSingleton(_navigationManagerMock.Object);

            // Act
            var component = RenderComponent<ExerciseDetail>(parameters => parameters
                .Add(p => p.Id, "ex1"));

            // Assert
            component.Find("[data-testid='add-warmup-button']").Should().NotBeNull();
            component.Find("[data-testid='add-cooldown-button']").Should().NotBeNull();
        }

        [Fact]
        public void ExerciseDetail_ShowsAddButtons_WithoutCapacityLimits()
        {
            // Arrange
            _exerciseStateServiceMock.SetupGet(x => x.SelectedExercise).Returns(_workoutExercise);

            // Create 15 warmup links (previously would be over max capacity, now unlimited)
            var warmupLinks = Enumerable.Range(0, 15)
                .Select(i => new ExerciseLinkDtoBuilder()
                    .WithId($"warmup-{i}")
                    .AsWarmup()
                    .Build())
                .ToList();

            SetupLinkStateWithLinks(warmupLinks, new List<ExerciseLinkDto>());

            Services.AddSingleton(_exerciseStateServiceMock.Object);
            Services.AddSingleton(_linkStateServiceMock.Object);
            Services.AddSingleton(_exerciseServiceMock.Object);
            Services.AddSingleton(_validationServiceMock.Object);
            Services.AddSingleton(_navigationManagerMock.Object);

            // Act
            var component = RenderComponent<ExerciseDetail>(parameters => parameters
                .Add(p => p.Id, "ex1"));

            // Assert - Add buttons should still be available (no capacity limits)
            component.Find("[data-testid='add-warmup-button']").Should().NotBeNull();
            component.Find("[data-testid='add-cooldown-button']").Should().NotBeNull();
            // Should not contain capacity indicators like "15 / 10"
            component.Markup.Should().NotContain(" / ");
        }

        [Fact]
        public void ExerciseDetail_UpdatesWhenLinkStateChanges()
        {
            // Arrange
            _exerciseStateServiceMock.SetupGet(x => x.SelectedExercise).Returns(_workoutExercise);
            SetupLinkStateWithNoLinks();

            Services.AddSingleton(_exerciseStateServiceMock.Object);
            Services.AddSingleton(_linkStateServiceMock.Object);
            Services.AddSingleton(_exerciseServiceMock.Object);
            Services.AddSingleton(_validationServiceMock.Object);
            Services.AddSingleton(_navigationManagerMock.Object);

            var component = RenderComponent<ExerciseDetail>(parameters => parameters
                .Add(p => p.Id, "ex1"));

            // Initial state - no links (no capacity display)
            // Verify empty state is shown through data-testid
            component.Find("[data-testid='warmup-empty-state']").Should().NotBeNull();

            // Act - Simulate adding a link by updating state
            var newLink = new ExerciseLinkDtoBuilder()
                .WithId("new-link")
                .AsWarmup()
                .Build();

            _linkStateServiceMock.SetupGet(x => x.WarmupLinks).Returns(new List<ExerciseLinkDto> { newLink });
            _linkStateServiceMock.SetupGet(x => x.WarmupLinkCount).Returns(1);

            // Trigger state change
            component.InvokeAsync(() => _linkStateServiceMock.Raise(x => x.OnChange += null));

            // Assert
            component.WaitForAssertion(() =>
            {
                // Should show the linked exercise without capacity display
                component.Markup.Should().NotContain("No warmup exercises linked yet");
            });
        }

        [Fact]
        public void ExerciseDetail_RemembersLinkState_WhenNavigatingBack()
        {
            // Arrange
            _exerciseStateServiceMock.SetupGet(x => x.SelectedExercise).Returns(_workoutExercise);

            // Simulate state already loaded with links
            var existingLink = new ExerciseLinkDtoBuilder()
                .WithId("existing")
                .AsWarmup()
                .Build();

            SetupLinkStateWithLinks(new List<ExerciseLinkDto> { existingLink }, new List<ExerciseLinkDto>());

            Services.AddSingleton(_exerciseStateServiceMock.Object);
            Services.AddSingleton(_linkStateServiceMock.Object);
            Services.AddSingleton(_exerciseServiceMock.Object);
            Services.AddSingleton(_validationServiceMock.Object);
            Services.AddSingleton(_navigationManagerMock.Object);

            // Act
            var component = RenderComponent<ExerciseDetail>(parameters => parameters
                .Add(p => p.Id, "ex1"));

            // Assert - Should show existing link without re-initializing (no capacity display)
            component.Markup.Should().NotContain("No warmup exercises linked yet");
            _linkStateServiceMock.Verify(x => x.InitializeForExerciseAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        private void SetupLinkStateWithNoLinks()
        {
            _linkStateServiceMock.SetupGet(x => x.IsLoading).Returns(false);
            _linkStateServiceMock.SetupGet(x => x.ErrorMessage).Returns((string?)null);
            _linkStateServiceMock.SetupGet(x => x.WarmupLinks).Returns(Enumerable.Empty<ExerciseLinkDto>());
            _linkStateServiceMock.SetupGet(x => x.CooldownLinks).Returns(Enumerable.Empty<ExerciseLinkDto>());
            _linkStateServiceMock.SetupGet(x => x.AlternativeLinks).Returns(Enumerable.Empty<ExerciseLinkDto>());
            _linkStateServiceMock.SetupGet(x => x.WarmupLinkCount).Returns(0);
            _linkStateServiceMock.SetupGet(x => x.CooldownLinkCount).Returns(0);
            _linkStateServiceMock.SetupGet(x => x.AlternativeLinkCount).Returns(0);
            _linkStateServiceMock.SetupGet(x => x.ActiveContext).Returns("Workout");
            _linkStateServiceMock.SetupGet(x => x.CurrentLinks).Returns(new ExerciseLinksResponseDto
            {
                ExerciseId = "ex1",
                ExerciseName = "Barbell Squat",
                Links = new List<ExerciseLinkDto>()
            });
        }

        private void SetupLinkStateWithLinks(List<ExerciseLinkDto> warmupLinks, List<ExerciseLinkDto> cooldownLinks)
        {
            _linkStateServiceMock.SetupGet(x => x.IsLoading).Returns(false);
            _linkStateServiceMock.SetupGet(x => x.ErrorMessage).Returns((string?)null);
            _linkStateServiceMock.SetupGet(x => x.WarmupLinks).Returns(warmupLinks);
            _linkStateServiceMock.SetupGet(x => x.CooldownLinks).Returns(cooldownLinks);
            _linkStateServiceMock.SetupGet(x => x.AlternativeLinks).Returns(Enumerable.Empty<ExerciseLinkDto>());
            _linkStateServiceMock.SetupGet(x => x.WarmupLinkCount).Returns(warmupLinks.Count);
            _linkStateServiceMock.SetupGet(x => x.CooldownLinkCount).Returns(cooldownLinks.Count);
            _linkStateServiceMock.SetupGet(x => x.AlternativeLinkCount).Returns(0);
            _linkStateServiceMock.SetupGet(x => x.ActiveContext).Returns("Workout");

            var allLinks = new List<ExerciseLinkDto>();
            allLinks.AddRange(warmupLinks);
            allLinks.AddRange(cooldownLinks);

            _linkStateServiceMock.SetupGet(x => x.CurrentLinks).Returns(new ExerciseLinksResponseDto
            {
                ExerciseId = "ex1",
                ExerciseName = "Barbell Squat",
                Links = allLinks
            });
        }
    }
}