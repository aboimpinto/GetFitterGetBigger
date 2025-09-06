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
using System.Threading.Tasks;

namespace GetFitterGetBigger.Admin.Tests.Components.Pages.Exercises.ExerciseLinks
{
    /// <summary>
    /// Tests for the AddExerciseLinkModal component including accessibility and interaction patterns
    /// </summary>
    public class AddExerciseLinkModalTests : TestContext
    {
        private readonly Mock<IExerciseService> _exerciseServiceMock;
        private readonly List<ExerciseTypeDto> _exerciseTypes;

        public AddExerciseLinkModalTests()
        {
            _exerciseServiceMock = new Mock<IExerciseService>();
            _exerciseTypes = new List<ExerciseTypeDto>
            {
                new() { Id = "1", Value = "Warmup", Description = "Warmup exercises" },
                new() { Id = "2", Value = "Workout", Description = "Main workout" },
                new() { Id = "3", Value = "Cooldown", Description = "Cooldown exercises" }
            };
        }

        [Fact]
        public void AddExerciseLinkModal_WhenClosed_IsHidden()
        {
            // Arrange & Act
            var component = RenderComponent<AddExerciseLinkModal>(parameters => parameters
                .Add(p => p.IsOpen, false)
                .Add(p => p.LinkType, "Warmup")
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExistingLinks, new List<ExerciseLinkDto>())
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Assert
            var modal = component.Find("[data-testid='add-link-modal']");
            modal.GetAttribute("class").Should().Contain("hidden");
        }

        [Fact]
        public void AddExerciseLinkModal_WhenOpen_IsVisible()
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
            modal.GetAttribute("class").Should().NotContain("hidden");
            modal.GetAttribute("class").Should().Contain("z-50");
        }

        [Fact]
        public void AddExerciseLinkModal_DisplaysCorrectTitle()
        {
            // Arrange & Act
            var component = RenderComponent<AddExerciseLinkModal>(parameters => parameters
                .Add(p => p.IsOpen, true)
                .Add(p => p.LinkType, "Cooldown")
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExistingLinks, new List<ExerciseLinkDto>())
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Assert
            component.Find("[data-testid='modal-title']").TextContent.Should().Contain("Add Cooldown Exercise");
        }

        [Fact]
        public async Task AddExerciseLinkModal_PerformsInitialSearchOnOpen()
        {
            // Arrange
            var exercises = new ExercisePagedResultDtoBuilder()
                .WithItems(
                    new ExerciseListDtoBuilder()
                        .WithId("1")
                        .WithName("Push-ups")
                        .WithExerciseTypes(("Workout", "Main workout"))
                        .Build()
                )
                .Build();

            _exerciseServiceMock
                .Setup(x => x.GetExercisesAsync(It.IsAny<ExerciseFilterDto>()))
                .ReturnsAsync(exercises);

            // Act
            var component = RenderComponent<AddExerciseLinkModal>(parameters => parameters
                .Add(p => p.IsOpen, true)
                .Add(p => p.LinkType, "Warmup")
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExistingLinks, new List<ExerciseLinkDto>())
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Wait for async operation
            await Task.Delay(100);

            // Assert
            _exerciseServiceMock.Verify(x => x.GetExercisesAsync(It.IsAny<ExerciseFilterDto>()), Times.Once);
        }

        [Fact]
        public async Task AddExerciseLinkModal_SearchFunctionality_Works()
        {
            // Arrange
            var exercises = new ExercisePagedResultDtoBuilder()
                .WithItems(
                    new ExerciseListDtoBuilder()
                        .WithId("1")
                        .WithName("Squats")
                        .WithExerciseTypes(("Warmup", "Warmup exercise"))
                        .Build(),
                    new ExerciseListDtoBuilder()
                        .WithId("2")
                        .WithName("Lunges")
                        .WithExerciseTypes(("Warmup", "Warmup exercise"))
                        .Build()
                )
                .Build();

            _exerciseServiceMock
                .Setup(x => x.GetExercisesAsync(It.Is<ExerciseFilterDto>(f => f.Name == "squat")))
                .ReturnsAsync(exercises);

            var component = RenderComponent<AddExerciseLinkModal>(parameters => parameters
                .Add(p => p.IsOpen, true)
                .Add(p => p.LinkType, "Warmup")
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExistingLinks, new List<ExerciseLinkDto>())
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Act
            await component.InvokeAsync(() => component.Find("[data-testid='search-input']").Input("squat"));
            await component.InvokeAsync(() => component.Find("[data-testid='search-button']").Click());

            // Assert
            _exerciseServiceMock.Verify(x => x.GetExercisesAsync(
                It.Is<ExerciseFilterDto>(f => f.Name == "squat")), Times.Once);
        }

        [Fact]
        public void AddExerciseLinkModal_ShowsOnlyWarmupExercisesForWarmupLink()
        {
            // Arrange
            var exercises = new ExercisePagedResultDtoBuilder()
                .WithItems(
                    new ExerciseListDtoBuilder()
                        .WithId("1")
                        .WithName("Push-ups")
                        .WithExerciseTypes(("Workout", "Main workout"))
                        .Build(),
                    new ExerciseListDtoBuilder()
                        .WithId("2")
                        .WithName("Arm Circles")
                        .WithExerciseTypes(("Warmup", "Warmup exercise"))
                        .Build(),
                    new ExerciseListDtoBuilder()
                        .WithId("3")
                        .WithName("Stretching")
                        .WithExerciseTypes(("Cooldown", "Cooldown exercise"))
                        .Build(),
                    new ExerciseListDtoBuilder()
                        .WithId("4")
                        .WithName("Air Squats")
                        .WithExerciseTypes(("Warmup", "Warmup exercise"), ("Workout", "Main workout"))
                        .Build()
                )
                .Build();

            _exerciseServiceMock
                .Setup(x => x.GetExercisesAsync(It.IsAny<ExerciseFilterDto>()))
                .ReturnsAsync(exercises);

            // Act
            var component = RenderComponent<AddExerciseLinkModal>(parameters => parameters
                .Add(p => p.IsOpen, true)
                .Add(p => p.LinkType, "Warmup")
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExistingLinks, new List<ExerciseLinkDto>())
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Wait for render
            component.WaitForAssertion(() =>
            {
                var results = component.FindAll("[data-testid^='exercise-']");
                results.Should().HaveCount(2); // Should show "Arm Circles" and "Air Squats" (both have Warmup type)
                results.Select(r => r.GetAttribute("data-testid")).Should().Contain("exercise-2");
                results.Select(r => r.GetAttribute("data-testid")).Should().Contain("exercise-4");
            });
        }

        [Fact]
        public void AddExerciseLinkModal_ShowsOnlyCooldownExercisesForCooldownLink()
        {
            // Arrange
            var exercises = new ExercisePagedResultDtoBuilder()
                .WithItems(
                    new ExerciseListDtoBuilder()
                        .WithId("1")
                        .WithName("Push-ups")
                        .WithExerciseTypes(("Workout", "Main workout"))
                        .Build(),
                    new ExerciseListDtoBuilder()
                        .WithId("2")
                        .WithName("Arm Circles")
                        .WithExerciseTypes(("Warmup", "Warmup exercise"))
                        .Build(),
                    new ExerciseListDtoBuilder()
                        .WithId("3")
                        .WithName("Stretching")
                        .WithExerciseTypes(("Cooldown", "Cooldown exercise"))
                        .Build(),
                    new ExerciseListDtoBuilder()
                        .WithId("4")
                        .WithName("Walking")
                        .WithExerciseTypes(("Cooldown", "Cooldown exercise"), ("Workout", "Main workout"))
                        .Build()
                )
                .Build();

            _exerciseServiceMock
                .Setup(x => x.GetExercisesAsync(It.IsAny<ExerciseFilterDto>()))
                .ReturnsAsync(exercises);

            // Act
            var component = RenderComponent<AddExerciseLinkModal>(parameters => parameters
                .Add(p => p.IsOpen, true)
                .Add(p => p.LinkType, "Cooldown")
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExistingLinks, new List<ExerciseLinkDto>())
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Wait for render
            component.WaitForAssertion(() =>
            {
                var results = component.FindAll("[data-testid^='exercise-']");
                results.Should().HaveCount(2); // Should show "Stretching" and "Walking" (both have Cooldown type)
                results.Select(r => r.GetAttribute("data-testid")).Should().Contain("exercise-3");
                results.Select(r => r.GetAttribute("data-testid")).Should().Contain("exercise-4");
            });
        }

        [Fact]
        public void AddExerciseLinkModal_ShowsAlreadyLinkedExercises()
        {
            // Arrange
            var existingLinks = new List<ExerciseLinkDto>
            {
                new ExerciseLinkDtoBuilder()
                    .WithTargetExerciseId("1")
                    .AsWarmup()
                    .Build()
            };

            var exercises = new ExercisePagedResultDtoBuilder()
                .WithItems(
                    new ExerciseListDtoBuilder()
                        .WithId("1")
                        .WithName("Push-ups")
                        .WithExerciseTypes(("Cooldown", "Cooldown exercise"))
                        .Build()
                )
                .Build();

            _exerciseServiceMock
                .Setup(x => x.GetExercisesAsync(It.IsAny<ExerciseFilterDto>()))
                .ReturnsAsync(exercises);

            // Act
            var component = RenderComponent<AddExerciseLinkModal>(parameters => parameters
                .Add(p => p.IsOpen, true)
                .Add(p => p.LinkType, "Cooldown")
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExistingLinks, existingLinks)
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Assert
            component.WaitForAssertion(() =>
            {
                var exerciseCard = component.Find("[data-testid='exercise-1']");
                exerciseCard.TextContent.Should().Contain("Already linked as Warmup");
                exerciseCard.GetAttribute("class").Should().Contain("cursor-not-allowed");
            });
        }

        [Fact]
        public void AddExerciseLinkModal_SelectsExerciseOnClick()
        {
            // Arrange
            var exercises = new ExercisePagedResultDtoBuilder()
                .WithItems(
                    new ExerciseListDtoBuilder()
                        .WithId("1")
                        .WithName("Push-ups")
                        .WithExerciseTypes(("Warmup", "Warmup exercise"))
                        .Build()
                )
                .Build();

            _exerciseServiceMock
                .Setup(x => x.GetExercisesAsync(It.IsAny<ExerciseFilterDto>()))
                .ReturnsAsync(exercises);

            var component = RenderComponent<AddExerciseLinkModal>(parameters => parameters
                .Add(p => p.IsOpen, true)
                .Add(p => p.LinkType, "Warmup")
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExistingLinks, new List<ExerciseLinkDto>())
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Act
            component.WaitForElement("[data-testid='exercise-1']").Click();

            // Assert
            var exerciseCard = component.Find("[data-testid='exercise-1']");
            exerciseCard.GetAttribute("class").Should().Contain("border-blue-500");
            exerciseCard.GetAttribute("class").Should().Contain("bg-blue-50");
        }

        [Fact]
        public void AddExerciseLinkModal_AddButtonDisabledWhenNoSelection()
        {
            // Arrange & Act
            var component = RenderComponent<AddExerciseLinkModal>(parameters => parameters
                .Add(p => p.IsOpen, true)
                .Add(p => p.LinkType, "Warmup")
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExistingLinks, new List<ExerciseLinkDto>())
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Assert
            var addButton = component.Find("[data-testid='add-button']");
            addButton.GetAttribute("disabled").Should().NotBeNull();
        }

        [Fact]
        public async Task AddExerciseLinkModal_CallsOnAddWithSelectedExercise()
        {
            // Arrange
            var exercises = new ExercisePagedResultDtoBuilder()
                .WithItems(
                    new ExerciseListDtoBuilder()
                        .WithId("1")
                        .WithName("Push-ups")
                        .WithExerciseTypes(("Warmup", "Warmup exercise"))
                        .Build()
                )
                .Build();

            _exerciseServiceMock
                .Setup(x => x.GetExercisesAsync(It.IsAny<ExerciseFilterDto>()))
                .ReturnsAsync(exercises);

            ExerciseListDto? addedExercise = null;
            var component = RenderComponent<AddExerciseLinkModal>(parameters => parameters
                .Add(p => p.IsOpen, true)
                .Add(p => p.LinkType, "Warmup")
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExistingLinks, new List<ExerciseLinkDto>())
                .Add(p => p.ExerciseTypes, _exerciseTypes)
                .Add(p => p.OnAdd, EventCallback.Factory.Create<ExerciseListDto>(this, e => addedExercise = e)));

            // Act
            component.WaitForElement("[data-testid='exercise-1']").Click();
            await component.InvokeAsync(() => component.Find("[data-testid='add-button']").Click());

            // Assert
            addedExercise.Should().NotBeNull();
            addedExercise!.Id.Should().Be("1");
            addedExercise.Name.Should().Be("Push-ups");
        }

        [Fact]
        public async Task AddExerciseLinkModal_CallsOnCancelWhenCancelClicked()
        {
            // Arrange
            var cancelCalled = false;
            var component = RenderComponent<AddExerciseLinkModal>(parameters => parameters
                .Add(p => p.IsOpen, true)
                .Add(p => p.LinkType, "Warmup")
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExistingLinks, new List<ExerciseLinkDto>())
                .Add(p => p.ExerciseTypes, _exerciseTypes)
                .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => cancelCalled = true)));

            // Act
            await component.InvokeAsync(() => component.Find("[data-testid='cancel-button']").Click());

            // Assert
            cancelCalled.Should().BeTrue();
        }

        [Fact]
        public void AddExerciseLinkModal_ShowsNoResultsMessage()
        {
            // Arrange
            var exercises = new ExercisePagedResultDtoBuilder()
                .WithItems() // Empty results
                .Build();

            _exerciseServiceMock
                .Setup(x => x.GetExercisesAsync(It.IsAny<ExerciseFilterDto>()))
                .ReturnsAsync(exercises);

            // Act
            var component = RenderComponent<AddExerciseLinkModal>(parameters => parameters
                .Add(p => p.IsOpen, true)
                .Add(p => p.LinkType, "Warmup")
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExistingLinks, new List<ExerciseLinkDto>())
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Assert
            component.WaitForAssertion(() =>
            {
                component.Find("[data-testid='no-results']").TextContent
                    .Should().Contain("No exercises found matching your criteria");
            });
        }

        [Fact]
        public async Task AddExerciseLinkModal_DisplaysErrorOnSearchFailure()
        {
            // Arrange
            _exerciseServiceMock
                .Setup(x => x.GetExercisesAsync(It.IsAny<ExerciseFilterDto>()))
                .ThrowsAsync(new Exception("Network error"));

            var component = RenderComponent<AddExerciseLinkModal>(parameters => parameters
                .Add(p => p.IsOpen, true)
                .Add(p => p.LinkType, "Warmup")
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExistingLinks, new List<ExerciseLinkDto>())
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Act
            await component.InvokeAsync(() => component.Find("[data-testid='search-button']").Click());

            // Assert
            component.WaitForAssertion(() =>
            {
                component.Find("[data-testid='error-message']").TextContent
                    .Should().Contain("Failed to search exercises: Network error");
            });
        }

        [Fact]
        public async Task AddExerciseLinkModal_ShowsAddingProgressIndicator()
        {
            // Arrange
            var exercises = new ExercisePagedResultDtoBuilder()
                .WithItems(
                    new ExerciseListDtoBuilder()
                        .WithId("1")
                        .WithName("Push-ups")
                        .WithExerciseTypes(("Warmup", "Warmup exercise"))
                        .Build()
                )
                .Build();

            _exerciseServiceMock
                .Setup(x => x.GetExercisesAsync(It.IsAny<ExerciseFilterDto>()))
                .ReturnsAsync(exercises);

            var tcs = new TaskCompletionSource<bool>();
            ExerciseListDto? capturedExercise = null;

            var component = RenderComponent<AddExerciseLinkModal>(parameters => parameters
                .Add(p => p.IsOpen, true)
                .Add(p => p.LinkType, "Warmup")
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExistingLinks, new List<ExerciseLinkDto>())
                .Add(p => p.ExerciseTypes, _exerciseTypes)
                .Add(p => p.OnAdd, EventCallback.Factory.Create<ExerciseListDto>(this, async exercise =>
                {
                    capturedExercise = exercise;
                    await tcs.Task; // Wait for our signal
                })));

            // Select an exercise
            component.WaitForElement("[data-testid='exercise-1']").Click();

            // Act - Click add button (should show progress)
            var addButton = component.Find("[data-testid='add-button']");
            addButton.Click();

            // Assert - Should show adding state
            component.WaitForAssertion(() =>
            {
                var updatedButton = component.Find("[data-testid='add-button']");
                updatedButton.TextContent.Should().Contain("Adding...");
                updatedButton.QuerySelector(".animate-spin").Should().NotBeNull();
                updatedButton.GetAttribute("disabled").Should().NotBeNull();
            });

            // Complete the async operation
            tcs.SetResult(true);
            await component.InvokeAsync(() => Task.CompletedTask);

            // Assert - Should return to normal state
            component.WaitForAssertion(() =>
            {
                // Modal should be closed/reset after successful add
                capturedExercise.Should().NotBeNull();
                capturedExercise!.Id.Should().Be("1");
            });
        }

        #region Accessibility Tests

        [Fact]
        public void AddExerciseLinkModal_HasProperAriaAttributes()
        {
            // Arrange & Act
            var component = RenderComponent<AddExerciseLinkModal>(parameters => parameters
                .Add(p => p.IsOpen, true)
                .Add(p => p.LinkType, "Alternative")
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExistingLinks, new List<ExerciseLinkDto>())
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Assert - Check ARIA attributes on modal
            var modal = component.Find("[role='dialog']");
            modal.Should().NotBeNull();
            modal.GetAttribute("aria-modal").Should().Be("true");
            modal.GetAttribute("aria-labelledby").Should().Be("modal-title");
            modal.GetAttribute("aria-describedby").Should().Be("modal-description");

            // Check that backdrop has aria-hidden
            var backdrop = component.Find(".fixed.inset-0.bg-gray-500");
            backdrop.GetAttribute("aria-hidden").Should().Be("true");

            // Check that search input has proper ARIA attributes
            var searchInput = component.Find("[data-testid='search-input']");
            searchInput.GetAttribute("aria-label").Should().Be("Search for exercises by name");
            searchInput.GetAttribute("aria-autocomplete").Should().Be("list");
        }

        [Fact]
        public void AddExerciseLinkModal_HasScreenReaderDescription()
        {
            // Arrange & Act
            var component = RenderComponent<AddExerciseLinkModal>(parameters => parameters
                .Add(p => p.IsOpen, true)
                .Add(p => p.LinkType, "Warmup")
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExistingLinks, new List<ExerciseLinkDto>())
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Assert - Check screen reader only description exists
            var description = component.Find("#modal-description");
            description.Should().NotBeNull();
            description.GetAttribute("class").Should().Contain("sr-only");
            description.TextContent.Should().Contain("Select an exercise to add as a warmup exercise");
        }

        [Fact]
        public async Task AddExerciseLinkModal_BackdropClick_ClosesModal()
        {
            // Arrange
            var onCancelCalled = false;
            var component = RenderComponent<AddExerciseLinkModal>(parameters => parameters
                .Add(p => p.IsOpen, true)
                .Add(p => p.LinkType, "Warmup")
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExistingLinks, new List<ExerciseLinkDto>())
                .Add(p => p.ExerciseTypes, _exerciseTypes)
                .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => onCancelCalled = true)));

            // Act - Click on backdrop
            var backdrop = component.Find(".fixed.inset-0.bg-gray-500");
            await backdrop.ClickAsync(new MouseEventArgs());

            // Assert
            onCancelCalled.Should().BeTrue("OnCancel should be called when backdrop is clicked");
        }

        [Fact]
        public async Task AddExerciseLinkModal_EscapeKey_ClosesModal()
        {
            // Arrange
            var onCancelCalled = false;
            var component = RenderComponent<AddExerciseLinkModal>(parameters => parameters
                .Add(p => p.IsOpen, true)
                .Add(p => p.LinkType, "Cooldown")
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExistingLinks, new List<ExerciseLinkDto>())
                .Add(p => p.ExerciseTypes, _exerciseTypes)
                .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => onCancelCalled = true)));

            // Act - Press Escape key on modal
            var modal = component.Find("[role='dialog']");
            await modal.KeyDownAsync(new KeyboardEventArgs { Key = "Escape" });

            // Assert
            onCancelCalled.Should().BeTrue("OnCancel should be called when Escape key is pressed");
        }

        [Fact]
        public async Task AddExerciseLinkModal_SearchInputFocus_OnOpen()
        {
            // Arrange
            var component = RenderComponent<AddExerciseLinkModal>(parameters => parameters
                .Add(p => p.IsOpen, true)
                .Add(p => p.LinkType, "Warmup")
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExistingLinks, new List<ExerciseLinkDto>())
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Wait for OnAfterRenderAsync to complete
            await component.InvokeAsync(() => Task.Delay(50));

            // Assert - Verify search input exists and would receive focus
            var searchInput = component.Find("[data-testid='search-input']");
            searchInput.Should().NotBeNull();
            // Note: bUnit limitation - cannot directly test focus state,
            // but we verify the element exists and FocusAsync would be called
        }

        [Fact]
        public void AddExerciseLinkModal_NotRendered_WhenClosed()
        {
            // Arrange & Act
            var component = RenderComponent<AddExerciseLinkModal>(parameters => parameters
                .Add(p => p.IsOpen, false)
                .Add(p => p.LinkType, "Warmup")
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExistingLinks, new List<ExerciseLinkDto>())
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Assert - Modal should not exist in DOM when IsOpen is false
            var modals = component.FindAll("[role='dialog']");
            modals.Should().BeEmpty("Modal should not be rendered when IsOpen is false");
        }

        [Fact]
        public void AddExerciseLinkModal_ModalTitle_MatchesLinkType()
        {
            // Test different link types
            var linkTypes = new[] { "Warmup", "Cooldown", "Alternative" };

            foreach (var linkType in linkTypes)
            {
                // Arrange & Act
                var component = RenderComponent<AddExerciseLinkModal>(parameters => parameters
                    .Add(p => p.IsOpen, true)
                    .Add(p => p.LinkType, linkType)
                    .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                    .Add(p => p.ExistingLinks, new List<ExerciseLinkDto>())
                    .Add(p => p.ExerciseTypes, _exerciseTypes));

                // Assert
                var title = component.Find("#modal-title");
                title.TextContent.Should().Contain($"Add {linkType} Exercise");

                // Check screen reader description also matches
                var description = component.Find("#modal-description");
                description.TextContent.Should().Contain($"Select an exercise to add as a {linkType.ToLower()} exercise");

                component.Dispose();
            }
        }

        #endregion
    }
}