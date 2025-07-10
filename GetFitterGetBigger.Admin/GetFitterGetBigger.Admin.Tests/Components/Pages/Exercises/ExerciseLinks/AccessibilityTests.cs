using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.Pages.Exercises.ExerciseLinks;
using GetFitterGetBigger.Admin.Components.Shared;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Builders;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace GetFitterGetBigger.Admin.Tests.Components.Pages.Exercises.ExerciseLinks
{
    public class AccessibilityTests : TestContext
    {
        private readonly Mock<IExerciseLinkStateService> _stateServiceMock;
        private readonly Mock<IExerciseService> _exerciseServiceMock;
        private readonly List<ExerciseTypeDto> _exerciseTypes;

        public AccessibilityTests()
        {
            _stateServiceMock = new Mock<IExerciseLinkStateService>();
            _exerciseServiceMock = new Mock<IExerciseService>();
            _exerciseTypes = new List<ExerciseTypeDto>
            {
                new ExerciseTypeDto { Id = "1", Value = "Workout" }
            };
        }

        [Fact]
        public void ExerciseLinkCard_HasProperAriaLabels()
        {
            // Arrange
            var link = new ExerciseLinkDtoBuilder()
                .WithTargetExercise(new ExerciseListDtoBuilder()
                    .WithName("Push-ups")
                    .Build())
                .AsWarmup()
                .WithDisplayOrder(0)
                .Build();

            // Act
            var component = RenderComponent<ExerciseLinkCard>(parameters => parameters
                .Add(p => p.Link, link)
                .Add(p => p.OnMoveUp, EventCallback.Factory.Create<ExerciseLinkDto>(this, _ => { })));

            // Assert
            var card = component.Find("[data-testid='exercise-link-card']");
            card.GetAttribute("role").Should().Be("listitem");
            card.GetAttribute("aria-label").Should().Be("Warmup exercise: Push-ups, position 1");
        }

        [Fact]
        public void ExerciseLinkCard_RemoveButton_HasAriaLabel()
        {
            // Arrange
            var link = new ExerciseLinkDtoBuilder()
                .WithTargetExercise(new ExerciseListDtoBuilder()
                    .WithName("Squats")
                    .Build())
                .Build();

            // Act
            var component = RenderComponent<ExerciseLinkCard>(parameters => parameters
                .Add(p => p.Link, link)
                .Add(p => p.OnRemove, EventCallback.Factory.Create<ExerciseLinkDto>(this, _ => { })));

            // Assert
            var removeButton = component.Find("[data-testid='remove-button']");
            removeButton.GetAttribute("aria-label").Should().Be("Remove Squats");
        }

        [Fact]
        public void LinkedExercisesList_HasProperRegionLabels()
        {
            // Arrange
            SetupEmptyState();

            // Act
            var component = RenderComponent<LinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _stateServiceMock.Object));

            // Assert
            var warmupSection = component.Find("[data-testid='warmup-section']");
            warmupSection.GetAttribute("role").Should().Be("region");
            warmupSection.GetAttribute("aria-label").Should().Be("Warmup exercises section");

            var cooldownSection = component.Find("[data-testid='cooldown-section']");
            cooldownSection.GetAttribute("role").Should().Be("region");
            cooldownSection.GetAttribute("aria-label").Should().Be("Cooldown exercises section");
        }

        [Fact]
        public void LinkedExercisesList_ListContainers_HaveAriaLive()
        {
            // Arrange
            var link = new ExerciseLinkDtoBuilder().AsWarmup().Build();
            SetupStateWithLinks(new List<ExerciseLinkDto> { link }, new List<ExerciseLinkDto>());

            // Act
            var component = RenderComponent<LinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _stateServiceMock.Object));

            // Assert
            var warmupContainer = component.Find("[data-testid='warmup-links-container']");
            warmupContainer.GetAttribute("role").Should().Be("list");
            warmupContainer.GetAttribute("aria-live").Should().Be("polite");
            warmupContainer.GetAttribute("aria-relevant").Should().Be("additions removals");
        }

        [Fact]
        public void AddExerciseLinkModal_HasProperDialogAttributes()
        {
            // Arrange & Act
            var component = RenderComponent<AddExerciseLinkModal>(parameters => parameters
                .Add(p => p.IsOpen, true)
                .Add(p => p.LinkType, "Warmup")
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExistingLinks, new List<ExerciseLinkDto>())
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Assert
            var modal = component.Find("[data-testid='add-link-modal']");
            modal.GetAttribute("role").Should().Be("dialog");
            modal.GetAttribute("aria-modal").Should().Be("true");
            modal.GetAttribute("aria-labelledby").Should().Be("modal-title");
        }

        [Fact]
        public void ExerciseLinkCard_MoveButtonsHaveAccessibleLabels()
        {
            // Arrange
            var link = new ExerciseLinkDtoBuilder()
                .WithTargetExercise(new ExerciseListDtoBuilder()
                    .WithName("Push-ups")
                    .Build())
                .Build();

            var component = RenderComponent<ExerciseLinkCard>(parameters => parameters
                .Add(p => p.Link, link)
                .Add(p => p.CanMoveUp, true)
                .Add(p => p.CanMoveDown, true)
                .Add(p => p.OnMoveUp, EventCallback.Factory.Create<ExerciseLinkDto>(this, _ => { }))
                .Add(p => p.OnMoveDown, EventCallback.Factory.Create<ExerciseLinkDto>(this, _ => { })));

            // Assert
            var moveUpButton = component.Find("[data-testid='move-up-button']");
            moveUpButton.GetAttribute("aria-label").Should().Be("Move Push-ups up in order");

            var moveDownButton = component.Find("[data-testid='move-down-button']");
            moveDownButton.GetAttribute("aria-label").Should().Be("Move Push-ups down in order");
        }

        [Fact]
        public void AriaLiveRegion_RendersScreenReaderAnnouncements()
        {
            // Arrange & Act
            var component = RenderComponent<AriaLiveRegion>(parameters => parameters
                .Add(p => p.Message, "Exercise link added successfully")
                .Add(p => p.AriaLive, "assertive"));

            // Assert
            var liveRegion = component.Find(".sr-only");
            liveRegion.GetAttribute("aria-live").Should().Be("assertive");
            liveRegion.GetAttribute("aria-atomic").Should().Be("true");
            liveRegion.GetAttribute("role").Should().Be("status");
            liveRegion.TextContent.Should().Be("Exercise link added successfully");
        }

        [Fact]
        public void LinkedExercisesList_ButtonsHaveAriaLabels()
        {
            // Arrange
            SetupEmptyState();

            // Act
            var component = RenderComponent<LinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _stateServiceMock.Object)
                .Add(p => p.OnAddLink, EventCallback.Factory.Create<string>(this, _ => { })));

            // Assert
            var addWarmupButton = component.Find("[data-testid='add-warmup-button']");
            addWarmupButton.GetAttribute("aria-label").Should().Be("Add warmup exercise");

            var addCooldownButton = component.Find("[data-testid='add-cooldown-button']");
            addCooldownButton.GetAttribute("aria-label").Should().Be("Add cooldown exercise");
        }

        [Fact]
        public void DeleteConfirmationDialog_HasProperAriaAttributes()
        {
            // Arrange
            var workoutExercise = new ExerciseDtoBuilder()
                .WithId("ex1")
                .WithName("Test Exercise")
                .WithExerciseTypes(("Workout", "Main workout"))
                .Build();

            var link = new ExerciseLinkDtoBuilder()
                .WithId("link1")
                .WithTargetExercise(new ExerciseListDtoBuilder()
                    .WithName("Target Exercise")
                    .Build())
                .AsWarmup()
                .Build();

            SetupStateWithLinks(new List<ExerciseLinkDto> { link }, new List<ExerciseLinkDto>());

            var validationServiceMock = new Mock<IExerciseLinkValidationService>();
            Services.AddSingleton(validationServiceMock.Object);

            var component = RenderComponent<ExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, workoutExercise)
                .Add(p => p.StateService, _stateServiceMock.Object)
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Act - Open delete confirmation
            var linkedList = component.FindComponent<LinkedExercisesList>();
            linkedList.Find("[data-testid='remove-button']").Click();

            // Assert
            var dialog = component.Find("[data-testid='delete-confirmation-dialog']");
            dialog.GetAttribute("role").Should().Be("dialog");
            dialog.GetAttribute("aria-modal").Should().Be("true");
            dialog.GetAttribute("aria-labelledby").Should().Be("delete-dialog-title");
        }

        private void SetupEmptyState()
        {
            _stateServiceMock.SetupGet(x => x.WarmupLinks).Returns(Enumerable.Empty<ExerciseLinkDto>());
            _stateServiceMock.SetupGet(x => x.CooldownLinks).Returns(Enumerable.Empty<ExerciseLinkDto>());
            _stateServiceMock.SetupGet(x => x.WarmupLinkCount).Returns(0);
            _stateServiceMock.SetupGet(x => x.CooldownLinkCount).Returns(0);
        }

        private void SetupStateWithLinks(List<ExerciseLinkDto> warmupLinks, List<ExerciseLinkDto> cooldownLinks)
        {
            _stateServiceMock.SetupGet(x => x.WarmupLinks).Returns(warmupLinks);
            _stateServiceMock.SetupGet(x => x.CooldownLinks).Returns(cooldownLinks);
            _stateServiceMock.SetupGet(x => x.WarmupLinkCount).Returns(warmupLinks.Count);
            _stateServiceMock.SetupGet(x => x.CooldownLinkCount).Returns(cooldownLinks.Count);
        }
    }
}