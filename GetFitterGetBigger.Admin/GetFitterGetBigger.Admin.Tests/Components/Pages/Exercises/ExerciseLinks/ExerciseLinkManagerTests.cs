using Bunit;
using FluentAssertions;
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

namespace GetFitterGetBigger.Admin.Tests.Components.Pages.Exercises.ExerciseLinks
{
    public class ExerciseLinkManagerTests : TestContext
    {
        private readonly Mock<IExerciseLinkStateService> _stateServiceMock;
        private readonly Mock<IExerciseService> _exerciseServiceMock;
        private readonly Mock<IExerciseLinkValidationService> _validationServiceMock;
        private readonly List<ExerciseTypeDto> _exerciseTypes;
        private readonly ExerciseDto _workoutExercise;
        private readonly ExerciseDto _nonWorkoutExercise;

        public ExerciseLinkManagerTests()
        {
            _stateServiceMock = new Mock<IExerciseLinkStateService>();
            _exerciseServiceMock = new Mock<IExerciseService>();
            _validationServiceMock = new Mock<IExerciseLinkValidationService>();

            // Register the validation service in the test context
            Services.AddSingleton(_validationServiceMock.Object);

            _exerciseTypes = new List<ExerciseTypeDto>
            {
                new() { Id = "1", Value = "Warmup", Description = "Warmup exercises" },
                new() { Id = "2", Value = "Workout", Description = "Main workout" },
                new() { Id = "3", Value = "Cooldown", Description = "Cooldown exercises" }
            };

            _workoutExercise = new ExerciseDtoBuilder()
                .WithId("ex1")
                .WithName("Squats")
                .WithExerciseTypes(("Workout", "Main workout"))
                .Build();

            _nonWorkoutExercise = new ExerciseDtoBuilder()
                .WithId("ex2")
                .WithName("Arm Circles")
                .WithExerciseTypes(("Warmup", "Warmup exercise"))
                .Build();
        }

        [Fact]
        public void ExerciseLinkManager_HidesForNonWorkoutExercises()
        {
            // Arrange & Act
            var component = RenderComponent<ExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, _nonWorkoutExercise)
                .Add(p => p.StateService, _stateServiceMock.Object)
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Assert
            // Component renders the outer div but no content when not a workout exercise
            var manager = component.Find("[data-testid='exercise-link-manager']");
            manager.Should().NotBeNull();
            manager.InnerHtml.Trim().Should().BeEmpty();
            _stateServiceMock.Verify(x => x.InitializeForExerciseAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExerciseLinkManager_ShowsForWorkoutExercises()
        {
            // Arrange
            SetupEmptyState();

            // Act
            var component = RenderComponent<ExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, _workoutExercise)
                .Add(p => p.StateService, _stateServiceMock.Object)
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Assert
            component.Find("[data-testid='exercise-link-manager']").Should().NotBeNull();
            component.Markup.Should().Contain("Linked Exercises");

            // Wait for async initialization
            await Task.Delay(50);
            _stateServiceMock.Verify(x => x.InitializeForExerciseAsync("ex1", "Squats"), Times.Once);
        }

        [Fact]
        public void ExerciseLinkManager_ShowsLoadingState()
        {
            // Arrange
            _stateServiceMock.SetupGet(x => x.IsLoading).Returns(true);

            // Act
            var component = RenderComponent<ExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, _workoutExercise)
                .Add(p => p.StateService, _stateServiceMock.Object)
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Assert
            component.Find("[data-testid='loading-spinner']").Should().NotBeNull();
            component.FindAll("[data-testid='linked-exercises-list']").Should().BeEmpty();
        }

        [Fact]
        public void ExerciseLinkManager_ShowsErrorState()
        {
            // Arrange
            _stateServiceMock.SetupGet(x => x.IsLoading).Returns(false);
            _stateServiceMock.SetupGet(x => x.ErrorMessage).Returns("Failed to load exercise links");

            // Act
            var component = RenderComponent<ExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, _workoutExercise)
                .Add(p => p.StateService, _stateServiceMock.Object)
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Assert
            var errorDiv = component.Find("[data-testid='error-message']");
            errorDiv.Should().NotBeNull();
            errorDiv.TextContent.Should().Contain("Failed to load exercise links");
            errorDiv.TextContent.Should().Contain("Try again");
        }

        [Fact]
        public async Task ExerciseLinkManager_RetryLoadsLinks()
        {
            // Arrange
            _stateServiceMock.SetupGet(x => x.IsLoading).Returns(false);
            _stateServiceMock.SetupGet(x => x.ErrorMessage).Returns("Failed to load");
            _stateServiceMock.Setup(x => x.LoadLinksAsync()).Returns(Task.CompletedTask);

            var component = RenderComponent<ExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, _workoutExercise)
                .Add(p => p.StateService, _stateServiceMock.Object)
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Act
            await component.InvokeAsync(() => component.Find("button:contains('Try again')").Click());

            // Assert
            _stateServiceMock.Verify(x => x.LoadLinksAsync(), Times.Once);
        }

        [Fact]
        public void ExerciseLinkManager_ShowsLinkedExercisesList()
        {
            // Arrange
            SetupStateWithLinks();

            // Act
            var component = RenderComponent<ExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, _workoutExercise)
                .Add(p => p.StateService, _stateServiceMock.Object)
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Assert
            component.FindComponent<LinkedExercisesList>().Should().NotBeNull();
        }

        [Fact]
        public async Task ExerciseLinkManager_OpensAddModalOnAddLink()
        {
            // Arrange
            SetupEmptyState();
            var component = RenderComponent<ExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, _workoutExercise)
                .Add(p => p.StateService, _stateServiceMock.Object)
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            var linkedList = component.FindComponent<LinkedExercisesList>();

            // Act
            await component.InvokeAsync(() => linkedList.Instance.OnAddLink.InvokeAsync("Warmup"));

            // Assert
            var modal = component.FindComponent<AddExerciseLinkModal>();
            modal.Instance.IsOpen.Should().BeTrue();
            modal.Instance.LinkType.Should().Be("Warmup");
        }

        [Fact]
        public async Task ExerciseLinkManager_CreatesLinkOnExerciseSelected()
        {
            // Arrange
            SetupEmptyState();
            _stateServiceMock.Setup(x => x.CreateLinkAsync(It.IsAny<CreateExerciseLinkDto>()))
                .Returns(Task.CompletedTask);

            // Setup validation to succeed
            _validationServiceMock.Setup(x => x.ValidateCreateLink(
                It.IsAny<ExerciseDto>(),
                It.IsAny<string>(),
                It.IsAny<ExerciseLinkType>(),
                It.IsAny<IEnumerable<ExerciseLinkDto>>()))
                .ReturnsAsync(ValidationResult.Success());

            var component = RenderComponent<ExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, _workoutExercise)
                .Add(p => p.StateService, _stateServiceMock.Object)
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Open modal
            var linkedList = component.FindComponent<LinkedExercisesList>();
            await component.InvokeAsync(() => linkedList.Instance.OnAddLink.InvokeAsync("Cooldown"));

            // Act - Select an exercise
            var modal = component.FindComponent<AddExerciseLinkModal>();
            var selectedExercise = new ExerciseListDtoBuilder()
                .WithId("ex3")
                .WithName("Stretching")
                .Build();

            await component.InvokeAsync(() => modal.Instance.OnAdd.InvokeAsync(selectedExercise));

            // Assert
            _stateServiceMock.Verify(x => x.CreateLinkAsync(It.Is<CreateExerciseLinkDto>(dto =>
                dto.SourceExerciseId == "ex1" &&
                dto.TargetExerciseId == "ex3" &&
                dto.LinkType == "Cooldown"
            )), Times.Once);
            modal.Instance.IsOpen.Should().BeFalse();
        }

        [Fact]
        public async Task ExerciseLinkManager_ShowsDeleteConfirmationDialog()
        {
            // Arrange
            var link = new ExerciseLinkDtoBuilder()
                .WithId("link1")
                .WithTargetExercise("Jumping Jacks", "ex4")
                .AsWarmup()
                .Build();

            SetupStateWithLinks(new List<ExerciseLinkDto> { link });

            var component = RenderComponent<ExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, _workoutExercise)
                .Add(p => p.StateService, _stateServiceMock.Object)
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            var linkedList = component.FindComponent<LinkedExercisesList>();

            // Act
            await component.InvokeAsync(() => linkedList.Instance.OnRemoveLink.InvokeAsync(link));

            // Assert
            var dialog = component.Find("[data-testid='delete-confirmation-dialog']");
            dialog.Should().NotBeNull();
            dialog.TextContent.Should().Contain("Remove Exercise Link");
            dialog.TextContent.Should().Contain("Jumping Jacks");
            dialog.TextContent.Should().Contain("warmup exercises");
        }

        [Fact]
        public async Task ExerciseLinkManager_DeletesLinkOnConfirmation()
        {
            // Arrange
            var link = new ExerciseLinkDtoBuilder()
                .WithId("link1")
                .WithTargetExercise("Push-ups", "ex5")
                .AsWarmup()
                .Build();

            SetupStateWithLinks(new List<ExerciseLinkDto> { link });
            _stateServiceMock.Setup(x => x.DeleteLinkAsync("link1")).Returns(Task.CompletedTask);

            var component = RenderComponent<ExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, _workoutExercise)
                .Add(p => p.StateService, _stateServiceMock.Object)
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Open confirmation dialog
            var linkedList = component.FindComponent<LinkedExercisesList>();
            await component.InvokeAsync(() => linkedList.Instance.OnRemoveLink.InvokeAsync(link));

            // Act - Confirm deletion
            await component.InvokeAsync(() => component.Find("[data-testid='confirm-delete-button']").Click());

            // Assert
            _stateServiceMock.Verify(x => x.DeleteLinkAsync("link1"), Times.Once);
            component.FindAll("[data-testid='delete-confirmation-dialog']").Should().BeEmpty();
        }

        [Fact]
        public async Task ExerciseLinkManager_CancelsDeleteOnCancel()
        {
            // Arrange
            var link = new ExerciseLinkDtoBuilder().WithId("link1").AsWarmup().Build();
            SetupStateWithLinks(new List<ExerciseLinkDto> { link });

            var component = RenderComponent<ExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, _workoutExercise)
                .Add(p => p.StateService, _stateServiceMock.Object)
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Open confirmation dialog
            var linkedList = component.FindComponent<LinkedExercisesList>();
            await component.InvokeAsync(() => linkedList.Instance.OnRemoveLink.InvokeAsync(link));

            // Act - Cancel deletion
            component.Find("[data-testid='cancel-delete-button']").Click();

            // Assert
            _stateServiceMock.Verify(x => x.DeleteLinkAsync(It.IsAny<string>()), Times.Never);
            component.FindAll("[data-testid='delete-confirmation-dialog']").Should().BeEmpty();
        }

        [Fact]
        public async Task ExerciseLinkManager_ReordersLinks()
        {
            // Arrange
            SetupStateWithLinks();
            _stateServiceMock.Setup(x => x.UpdateMultipleLinksAsync(It.IsAny<List<UpdateExerciseLinkDto>>()))
                .Returns(Task.CompletedTask);

            var component = RenderComponent<ExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, _workoutExercise)
                .Add(p => p.StateService, _stateServiceMock.Object)
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            var linkedList = component.FindComponent<LinkedExercisesList>();
            var reorderMap = new Dictionary<string, int>
            {
                { "link1", 1 },
                { "link2", 0 }
            };

            // Act
            await component.InvokeAsync(() => linkedList.Instance.OnReorderLinks.InvokeAsync(("Warmup", reorderMap)));

            // Assert
            _stateServiceMock.Verify(x => x.UpdateMultipleLinksAsync(
                It.Is<List<UpdateExerciseLinkDto>>(updates =>
                    updates.Count == 2 &&
                    updates.Any(u => u.Id == "link1" && u.DisplayOrder == 1) &&
                    updates.Any(u => u.Id == "link2" && u.DisplayOrder == 0)
                )), Times.Once);
        }

        [Fact]
        public async Task ExerciseLinkManager_ShowsSuccessNotification()
        {
            // Arrange
            SetupEmptyState();
            _stateServiceMock.SetupGet(x => x.ErrorMessage).Returns((string?)null);
            _stateServiceMock.Setup(x => x.CreateLinkAsync(It.IsAny<CreateExerciseLinkDto>()))
                .Returns(Task.CompletedTask);

            // Setup validation to succeed
            _validationServiceMock.Setup(x => x.ValidateCreateLink(
                It.IsAny<ExerciseDto>(),
                It.IsAny<string>(),
                It.IsAny<ExerciseLinkType>(),
                It.IsAny<IEnumerable<ExerciseLinkDto>>()))
                .ReturnsAsync(ValidationResult.Success());

            var component = RenderComponent<ExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, _workoutExercise)
                .Add(p => p.StateService, _stateServiceMock.Object)
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Open modal and select exercise
            var linkedList = component.FindComponent<LinkedExercisesList>();
            await component.InvokeAsync(() => linkedList.Instance.OnAddLink.InvokeAsync("Warmup"));

            var modal = component.FindComponent<AddExerciseLinkModal>();
            var selectedExercise = new ExerciseListDtoBuilder()
                .WithId("ex4")
                .WithName("Jumping Jacks")
                .Build();

            // Act
            await component.InvokeAsync(() => modal.Instance.OnAdd.InvokeAsync(selectedExercise));

            // Assert
            component.WaitForAssertion(() =>
            {
                var notification = component.Find("[data-testid='success-notification']");
                notification.TextContent.Should().Contain("Jumping Jacks added to warmup exercises");
            });
        }

        [Fact]
        public async Task ExerciseLinkManager_SuccessNotificationCanBeDismissed()
        {
            // Arrange
            SetupStateWithLinks();
            _stateServiceMock.Setup(x => x.CreateLinkAsync(It.IsAny<CreateExerciseLinkDto>()))
                .Returns(Task.CompletedTask);
            _validationServiceMock.Setup(x => x.ValidateCreateLink(
                It.IsAny<ExerciseDto>(),
                It.IsAny<string>(),
                It.IsAny<ExerciseLinkType>(),
                It.IsAny<IEnumerable<ExerciseLinkDto>>()))
                .ReturnsAsync(ValidationResult.Success());

            var component = RenderComponent<ExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, _workoutExercise)
                .Add(p => p.StateService, _stateServiceMock.Object)
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Open modal and select exercise
            var linkedList = component.FindComponent<LinkedExercisesList>();
            await component.InvokeAsync(() => linkedList.Instance.OnAddLink.InvokeAsync("Warmup"));

            var modal = component.FindComponent<AddExerciseLinkModal>();
            var selectedExercise = new ExerciseListDtoBuilder()
                .WithId("ex4")
                .WithName("Test Exercise")
                .Build();

            await component.InvokeAsync(() => modal.Instance.OnAdd.InvokeAsync(selectedExercise));

            // Wait for notification to appear
            component.WaitForAssertion(() =>
            {
                component.Find("[data-testid='success-notification']").Should().NotBeNull();
            });

            // Act - Click dismiss button
            component.Find("[data-testid='success-notification'] button").Click();

            // Assert - Notification should disappear
            component.WaitForAssertion(() =>
            {
                component.FindAll("[data-testid='success-notification']").Should().BeEmpty();
            });
        }

        [Fact]
        public void ExerciseLinkManager_SubscribesToStateChanges()
        {
            // Arrange
            SetupEmptyState();
            var component = RenderComponent<ExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, _workoutExercise)
                .Add(p => p.StateService, _stateServiceMock.Object)
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Assert - Verify subscription (both ExerciseLinkManager and LinkedExercisesList subscribe)
            _stateServiceMock.VerifyAdd(x => x.OnChange += It.IsAny<Action>(), Times.AtLeastOnce);

            // Act - Get the component instance and dispose it directly
            var managerInstance = component.Instance;
            managerInstance.Dispose();

            // Assert - Verify unsubscription (only the manager unsubscribes when disposed)
            _stateServiceMock.VerifyRemove(x => x.OnChange -= It.IsAny<Action>(), Times.Once);
        }

        private void SetupEmptyState()
        {
            _stateServiceMock.SetupGet(x => x.IsLoading).Returns(false);
            _stateServiceMock.SetupGet(x => x.ErrorMessage).Returns((string?)null);
            _stateServiceMock.SetupGet(x => x.WarmupLinks).Returns(Enumerable.Empty<ExerciseLinkDto>());
            _stateServiceMock.SetupGet(x => x.CooldownLinks).Returns(Enumerable.Empty<ExerciseLinkDto>());
            _stateServiceMock.SetupGet(x => x.WarmupLinkCount).Returns(0);
            _stateServiceMock.SetupGet(x => x.CooldownLinkCount).Returns(0);
            _stateServiceMock.SetupGet(x => x.CurrentLinks).Returns(new ExerciseLinksResponseDto
            {
                ExerciseId = "ex1",
                ExerciseName = "Squats",
                Links = new List<ExerciseLinkDto>()
            });
        }

        private void SetupStateWithLinks(List<ExerciseLinkDto>? links = null)
        {
            links ??= new List<ExerciseLinkDto>
            {
                new ExerciseLinkDtoBuilder().WithId("link1").AsWarmup().Build(),
                new ExerciseLinkDtoBuilder().WithId("link2").AsCooldown().Build()
            };

            var warmupLinks = links.Where(l => l.LinkType == "Warmup").ToList();
            var cooldownLinks = links.Where(l => l.LinkType == "Cooldown").ToList();

            _stateServiceMock.SetupGet(x => x.IsLoading).Returns(false);
            _stateServiceMock.SetupGet(x => x.ErrorMessage).Returns((string?)null);
            _stateServiceMock.SetupGet(x => x.WarmupLinks).Returns(warmupLinks);
            _stateServiceMock.SetupGet(x => x.CooldownLinks).Returns(cooldownLinks);
            _stateServiceMock.SetupGet(x => x.WarmupLinkCount).Returns(warmupLinks.Count);
            _stateServiceMock.SetupGet(x => x.CooldownLinkCount).Returns(cooldownLinks.Count);
            _stateServiceMock.SetupGet(x => x.CurrentLinks).Returns(new ExerciseLinksResponseDto
            {
                ExerciseId = "ex1",
                ExerciseName = "Squats",
                Links = links
            });
        }
    }
}