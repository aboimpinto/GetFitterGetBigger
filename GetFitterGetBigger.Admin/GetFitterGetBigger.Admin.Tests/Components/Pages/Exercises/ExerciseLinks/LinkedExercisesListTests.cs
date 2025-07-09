using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.Pages.Exercises.ExerciseLinks;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Builders;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace GetFitterGetBigger.Admin.Tests.Components.Pages.Exercises.ExerciseLinks
{
    public class LinkedExercisesListTests : TestContext
    {
        private readonly Mock<IExerciseLinkStateService> _stateServiceMock;

        public LinkedExercisesListTests()
        {
            _stateServiceMock = new Mock<IExerciseLinkStateService>();
        }

        [Fact]
        public void LinkedExercisesList_RendersWarmupAndCooldownSections()
        {
            // Arrange
            SetupEmptyState();

            // Act
            var component = RenderComponent<LinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _stateServiceMock.Object));

            // Assert
            component.Find("[data-testid='warmup-section']").Should().NotBeNull();
            component.Find("[data-testid='cooldown-section']").Should().NotBeNull();
            component.Markup.Should().Contain("Warmup Exercises");
            component.Markup.Should().Contain("Cooldown Exercises");
        }

        [Fact]
        public void LinkedExercisesList_DisplaysLinkCounts()
        {
            // Arrange
            var warmupLinks = new List<ExerciseLinkDto>
            {
                new ExerciseLinkDtoBuilder().AsWarmup().Build(),
                new ExerciseLinkDtoBuilder().AsWarmup().Build()
            };
            var cooldownLinks = new List<ExerciseLinkDto>
            {
                new ExerciseLinkDtoBuilder().AsCooldown().Build()
            };

            SetupStateWithLinks(warmupLinks, cooldownLinks);

            // Act
            var component = RenderComponent<LinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _stateServiceMock.Object));

            // Assert
            component.Find("[data-testid='warmup-count']").TextContent.Should().Contain("2 / 10");
            component.Find("[data-testid='cooldown-count']").TextContent.Should().Contain("1 / 10");
        }

        [Fact]
        public void LinkedExercisesList_ShowsEmptyStateWhenNoLinks()
        {
            // Arrange
            SetupEmptyState();

            // Act
            var component = RenderComponent<LinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _stateServiceMock.Object));

            // Assert
            component.Find("[data-testid='warmup-empty-state']").Should().NotBeNull();
            component.Find("[data-testid='cooldown-empty-state']").Should().NotBeNull();
            component.Markup.Should().Contain("No warmup exercises linked yet");
            component.Markup.Should().Contain("No cooldown exercises linked yet");
        }

        [Fact]
        public void LinkedExercisesList_RendersExerciseLinkCards()
        {
            // Arrange
            var warmupLinks = new List<ExerciseLinkDto>
            {
                new ExerciseLinkDtoBuilder().WithId("warmup-1").AsWarmup().Build(),
                new ExerciseLinkDtoBuilder().WithId("warmup-2").AsWarmup().Build()
            };
            var cooldownLinks = new List<ExerciseLinkDto>
            {
                new ExerciseLinkDtoBuilder().WithId("cooldown-1").AsCooldown().Build()
            };

            SetupStateWithLinks(warmupLinks, cooldownLinks);

            // Act
            var component = RenderComponent<LinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _stateServiceMock.Object));

            // Assert
            var warmupContainer = component.Find("[data-testid='warmup-links-container']");
            var cooldownContainer = component.Find("[data-testid='cooldown-links-container']");
            
            component.FindComponents<ExerciseLinkCard>().Should().HaveCount(3);
            warmupContainer.QuerySelectorAll("[data-testid='exercise-link-card']").Should().HaveCount(2);
            cooldownContainer.QuerySelectorAll("[data-testid='exercise-link-card']").Should().HaveCount(1);
        }

        [Fact]
        public void LinkedExercisesList_ShowsAddButtonsWhenNotAtMaxCapacity()
        {
            // Arrange
            SetupEmptyState();

            // Act
            var component = RenderComponent<LinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _stateServiceMock.Object)
                .Add(p => p.OnAddLink, EventCallback.Factory.Create<string>(this, _ => { })));

            // Assert
            component.Find("[data-testid='add-warmup-button']").Should().NotBeNull();
            component.Find("[data-testid='add-cooldown-button']").Should().NotBeNull();
        }

        [Fact]
        public void LinkedExercisesList_HidesAddButtonWhenAtMaxCapacity()
        {
            // Arrange
            var warmupLinks = Enumerable.Range(0, 10)
                .Select(i => new ExerciseLinkDtoBuilder().WithId($"warmup-{i}").AsWarmup().Build())
                .ToList();

            SetupStateWithLinks(warmupLinks, new List<ExerciseLinkDto>());

            // Act
            var component = RenderComponent<LinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _stateServiceMock.Object)
                .Add(p => p.OnAddLink, EventCallback.Factory.Create<string>(this, _ => { })));

            // Assert
            component.FindAll("[data-testid='add-warmup-button']").Should().BeEmpty();
            component.Find("[data-testid='add-cooldown-button']").Should().NotBeNull();
        }

        [Fact]
        public void LinkedExercisesList_CallsOnAddLinkWhenAddButtonClicked()
        {
            // Arrange
            SetupEmptyState();
            string? linkTypeReceived = null;

            var component = RenderComponent<LinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _stateServiceMock.Object)
                .Add(p => p.OnAddLink, EventCallback.Factory.Create<string>(this, type => linkTypeReceived = type)));

            // Act
            component.Find("[data-testid='add-warmup-button']").Click();

            // Assert
            linkTypeReceived.Should().Be("Warmup");

            // Act
            component.Find("[data-testid='add-cooldown-button']").Click();

            // Assert
            linkTypeReceived.Should().Be("Cooldown");
        }

        [Fact]
        public void LinkedExercisesList_DisablesInteractionsWhenDisabled()
        {
            // Arrange
            SetupEmptyState();

            // Act
            var component = RenderComponent<LinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _stateServiceMock.Object)
                .Add(p => p.Disabled, true)
                .Add(p => p.OnAddLink, EventCallback.Factory.Create<string>(this, _ => { })));

            // Assert
            component.FindAll("[data-testid='add-warmup-button']").Should().BeEmpty();
            component.FindAll("[data-testid='add-cooldown-button']").Should().BeEmpty();
            component.Markup.Should().NotContain("Click \"Add Warmup\" to link exercises");
            component.Markup.Should().NotContain("Click \"Add Cooldown\" to link exercises");
        }

        [Fact]
        public void LinkedExercisesList_PassesOnRemoveLinkToCards()
        {
            // Arrange
            var link = new ExerciseLinkDtoBuilder().WithId("link-1").AsWarmup().Build();
            SetupStateWithLinks(new List<ExerciseLinkDto> { link }, new List<ExerciseLinkDto>());

            ExerciseLinkDto? removedLink = null;
            var component = RenderComponent<LinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _stateServiceMock.Object)
                .Add(p => p.OnRemoveLink, EventCallback.Factory.Create<ExerciseLinkDto>(this, l => removedLink = l)));

            // Act
            var linkCard = component.FindComponent<ExerciseLinkCard>();
            linkCard.Find("[data-testid='remove-button']").Click();

            // Assert
            removedLink.Should().NotBeNull();
            removedLink!.Id.Should().Be("link-1");
        }

        [Fact]
        public void LinkedExercisesList_SubscribesToStateChanges()
        {
            // Arrange
            var initialLinks = new List<ExerciseLinkDto>();
            SetupStateWithLinks(initialLinks, initialLinks);
            
            var component = RenderComponent<LinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _stateServiceMock.Object));

            // Verify subscription
            _stateServiceMock.VerifyAdd(x => x.OnChange += It.IsAny<Action>(), Times.Once);

            // Act - Simulate state change by updating the mock
            var newWarmupLinks = new List<ExerciseLinkDto> 
            { 
                new ExerciseLinkDtoBuilder().AsWarmup().Build() 
            };
            _stateServiceMock.SetupGet(x => x.WarmupLinks).Returns(newWarmupLinks);
            _stateServiceMock.SetupGet(x => x.WarmupLinkCount).Returns(1);
            
            // Trigger the OnChange event within the renderer's synchronization context
            component.InvokeAsync(() => _stateServiceMock.Raise(x => x.OnChange += null));

            // Assert - Component should re-render with new state
            component.WaitForAssertion(() => 
                component.Find("[data-testid='warmup-count']").TextContent.Should().Contain("1 / 10"));
        }

        [Fact]
        public void LinkedExercisesList_UnsubscribesOnDispose()
        {
            // Arrange
            SetupEmptyState();
            var component = RenderComponent<LinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _stateServiceMock.Object));

            // Get the component instance to call Dispose directly
            var componentInstance = component.Instance;

            // Act
            componentInstance.Dispose();

            // Assert
            _stateServiceMock.VerifyRemove(x => x.OnChange -= It.IsAny<Action>(), Times.Once);
        }

        [Fact]
        public void LinkedExercisesList_HandlesDragAndDrop()
        {
            // Arrange
            var link1 = new ExerciseLinkDtoBuilder().WithId("link-1").AsWarmup().WithDisplayOrder(0).Build();
            var link2 = new ExerciseLinkDtoBuilder().WithId("link-2").AsWarmup().WithDisplayOrder(1).Build();
            SetupStateWithLinks(new List<ExerciseLinkDto> { link1, link2 }, new List<ExerciseLinkDto>());

            (string linkType, Dictionary<string, int> reorderMap)? reorderCall = null;
            var component = RenderComponent<LinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _stateServiceMock.Object)
                .Add(p => p.OnReorderLinks, EventCallback.Factory.Create<(string, Dictionary<string, int>)>(
                    this, args => reorderCall = args)));

            var warmupContainer = component.Find("[data-testid='warmup-links-container']");
            var firstCard = component.FindComponents<ExerciseLinkCard>().First();

            // Act - Start drag
            firstCard.Find("[data-testid='exercise-link-card']").TriggerEvent("ondragstart", new DragEventArgs());

            // Act - Drop
            warmupContainer.TriggerEvent("ondrop", new DragEventArgs());

            // Assert
            reorderCall.Should().NotBeNull();
            reorderCall!.Value.linkType.Should().Be("Warmup");
            reorderCall!.Value.reorderMap.Should().ContainKey("link-1");
            reorderCall!.Value.reorderMap.Should().ContainKey("link-2");
        }

        [Fact]
        public void LinkedExercisesList_ShowsReorderProgressOverlay_WhenIsSavingIsTrue()
        {
            // Arrange
            SetupEmptyState();
            _stateServiceMock.SetupGet(x => x.IsSaving).Returns(true);

            // Act
            var component = RenderComponent<LinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _stateServiceMock.Object));

            // Assert
            var overlay = component.Find("[data-testid='reorder-progress-overlay']");
            overlay.Should().NotBeNull();
            overlay.TextContent.Should().Contain("Reordering exercises...");
            overlay.QuerySelector(".animate-spin").Should().NotBeNull();
        }

        [Fact]
        public void LinkedExercisesList_HidesReorderProgressOverlay_WhenIsSavingIsFalse()
        {
            // Arrange
            SetupEmptyState();
            _stateServiceMock.SetupGet(x => x.IsSaving).Returns(false);

            // Act
            var component = RenderComponent<LinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _stateServiceMock.Object));

            // Assert
            component.FindAll("[data-testid='reorder-progress-overlay']").Should().BeEmpty();
        }

        [Fact]
        public void LinkedExercisesList_ShowsDropZoneHighlight_DuringDrag()
        {
            // Arrange
            var link = new ExerciseLinkDtoBuilder().WithId("link-1").AsWarmup().Build();
            SetupStateWithLinks(new List<ExerciseLinkDto> { link }, new List<ExerciseLinkDto>());

            var component = RenderComponent<LinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _stateServiceMock.Object));

            var warmupContainer = component.Find("[data-testid='warmup-links-container']");
            var firstCard = component.FindComponents<ExerciseLinkCard>().First();

            // Act - Start drag
            firstCard.Find("[data-testid='exercise-link-card']").TriggerEvent("ondragstart", new DragEventArgs());

            // Act - Enter drop zone
            warmupContainer.TriggerEvent("ondragenter", new DragEventArgs());

            // Assert
            var containerClass = warmupContainer.GetAttribute("class");
            containerClass.Should().Contain("border-2");
            containerClass.Should().Contain("border-dashed");
            containerClass.Should().Contain("border-blue-400");
            containerClass.Should().Contain("bg-blue-50");

            // Act - Leave drop zone
            warmupContainer.TriggerEvent("ondragleave", new DragEventArgs());

            // Assert - highlight should be removed
            var updatedClass = component.Find("[data-testid='warmup-links-container']").GetAttribute("class");
            updatedClass.Should().NotContain("border-2");
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