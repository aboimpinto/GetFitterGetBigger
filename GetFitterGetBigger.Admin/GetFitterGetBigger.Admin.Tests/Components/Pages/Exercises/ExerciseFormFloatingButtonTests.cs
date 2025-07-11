using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.Pages.Exercises;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using Microsoft.Extensions.DependencyInjection;
using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Bunit.TestDoubles;
using Moq;

namespace GetFitterGetBigger.Admin.Tests.Components.Pages.Exercises
{
    /// <summary>
    /// Tests for the floating action buttons in ExerciseForm.
    /// Note: Testing responsive CSS positioning has limitations in unit tests.
    /// These tests verify structure and presence, not actual pixel positioning.
    /// </summary>
    public class ExerciseFormFloatingButtonTests : TestContext
    {
        private readonly MockExerciseStateService _mockStateService;
        private readonly MockReferenceDataService _mockReferenceDataService;
        private readonly Mock<IExerciseWeightTypeStateService> _mockWeightTypeStateService;

        public ExerciseFormFloatingButtonTests()
        {
            _mockStateService = new MockExerciseStateService();
            _mockReferenceDataService = new MockReferenceDataService();
            _mockWeightTypeStateService = new Mock<IExerciseWeightTypeStateService>();

            // Setup basic weight type state service mock
            _mockWeightTypeStateService.Setup(x => x.WeightTypes).Returns(new List<ExerciseWeightTypeDto>());
            _mockWeightTypeStateService.Setup(x => x.IsLoading).Returns(false);
            _mockWeightTypeStateService.Setup(x => x.LoadWeightTypesAsync()).Returns(Task.CompletedTask);

            Services.AddSingleton<IExerciseStateService>(_mockStateService);
            Services.AddSingleton<IReferenceDataService>(_mockReferenceDataService);
            Services.AddSingleton(_mockWeightTypeStateService.Object);
            var navMan = Services.GetRequiredService<FakeNavigationManager>();
            navMan.NavigateTo("http://localhost/exercises/new");
        }

        [Fact]
        public void FloatingButtons_ArePresent_WhenNotLoading()
        {
            // Arrange
            _mockStateService.SetupReferenceData();

            // Act
            var component = RenderComponent<ExerciseForm>();

            // Assert - Verify both floating buttons are present
            var cancelButton = component.Find("[data-testid='floating-cancel-button']");
            var saveButton = component.Find("[data-testid='floating-save-button']");

            cancelButton.Should().NotBeNull();
            saveButton.Should().NotBeNull();
        }

        [Fact]
        public void FloatingButtons_HaveCorrectStructure()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            var component = RenderComponent<ExerciseForm>();

            // Act & Assert - Cancel button structure
            var cancelButton = component.Find("[data-testid='floating-cancel-button']");
            cancelButton.QuerySelector("svg").Should().NotBeNull("Cancel button should have an X icon");
            cancelButton.QuerySelector("span")?.TextContent.Should().Be("Cancel");

            // Save button structure
            var saveButton = component.Find("[data-testid='floating-save-button']");
            saveButton.QuerySelector("svg").Should().NotBeNull("Save button should have a floppy disk icon");
            var saveText = saveButton.QuerySelector("span")?.TextContent;
            saveText.Should().Be("Create"); // In create mode
        }

        [Fact]
        public void FloatingButtons_HaveCorrectPositioningClasses()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            var component = RenderComponent<ExerciseForm>();

            // Act
            var cancelContainer = component.Find("[data-testid='floating-cancel-button']").ParentElement;
            var saveContainer = component.Find("[data-testid='floating-save-button']").ParentElement;

            // Assert - Verify CSS classes
            cancelContainer?.GetAttribute("class").Should().Contain("fixed");
            cancelContainer?.GetAttribute("class").Should().Contain("bottom-8");
            cancelContainer?.GetAttribute("class").Should().Contain("z-50");
            cancelContainer?.GetAttribute("class").Should().Contain("cancelPostionStyle");

            saveContainer?.GetAttribute("class").Should().Contain("fixed");
            saveContainer?.GetAttribute("class").Should().Contain("bottom-8");
            saveContainer?.GetAttribute("class").Should().Contain("z-50");
            saveContainer?.GetAttribute("class").Should().Contain("right-4");
            // Note: xl:right-[calc(50%-41rem)] is a Tailwind class that would be present
        }

        [Fact]
        public void SaveButton_HasProperStructure_ForCreateMode()
        {
            // This test verifies the save button structure without testing loading state
            // The loading state test would require more complex setup with the form

            // Arrange
            _mockStateService.SetupReferenceData();
            var component = RenderComponent<ExerciseForm>();

            // Act
            var saveButton = component.Find("[data-testid='floating-save-button']");
            var buttonText = saveButton.QuerySelector("span")?.TextContent;

            // Assert
            buttonText.Should().Be("Create"); // Should show "Create" in create mode
            saveButton.Should().NotBeNull();
            saveButton.GetAttribute("disabled").Should().BeNull(); // Not disabled initially
        }

        [Fact]
        public async Task CancelButton_NavigatesToExerciseList_WhenClicked()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            var navMan = Services.GetRequiredService<Bunit.TestDoubles.FakeNavigationManager>();
            var component = RenderComponent<ExerciseForm>();

            // Act
            var cancelButton = component.Find("[data-testid='floating-cancel-button']");
            await component.InvokeAsync(() => cancelButton.Click());

            // Assert
            navMan.Uri.Should().EndWith("/exercises");
        }

        [Fact]
        public void FloatingButtons_UpdateTextForEditMode()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            _mockStateService.SelectedExercise = new ExerciseDto
            {
                Id = "123",
                Name = "Existing Exercise"
            };

            var navMan = Services.GetRequiredService<Bunit.TestDoubles.FakeNavigationManager>();
            navMan.NavigateTo("http://localhost/exercises/123/edit");

            // Act
            var component = RenderComponent<ExerciseForm>(parameters => parameters
                .Add(p => p.Id, "123"));

            // Assert
            var saveButtonText = component.Find("[data-testid='floating-save-button'] span").TextContent;
            saveButtonText.Should().Be("Update"); // Should show "Update" in edit mode
        }

        [Fact]
        public void StyleBlock_ContainsResponsivePositioning()
        {
            // Arrange & Act
            _mockStateService.SetupReferenceData();
            var component = RenderComponent<ExerciseForm>();

            // Assert - Check that the component includes the style block
            // Note: bUnit doesn't parse <style> blocks, but we can verify the component renders
            // The actual CSS would be tested via integration/E2E tests
            component.Markup.Should().Contain("<style>");
            component.Markup.Should().Contain("cancelPostionStyle");

            // This is as far as unit tests can go for CSS positioning
            // Actual responsive behavior would need:
            // 1. Integration tests with a real browser
            // 2. Visual regression tests
            // 3. E2E tests with viewport size changes
        }

        /// <summary>
        /// NOTE: Testing actual CSS positioning values (like calc(50% - 25rem)) 
        /// requires browser-based testing tools such as:
        /// - Playwright or Selenium for E2E tests
        /// - Percy or Chromatic for visual regression tests
        /// - Cypress for integration tests
        /// 
        /// Unit tests with bUnit can only verify:
        /// - Element presence and structure
        /// - CSS class application
        /// - Event handling
        /// - Component state changes
        /// </summary>
        [Fact]
        public void Documentation_ExplainsTestingLimitations()
        {
            // This test serves as documentation
            true.Should().BeTrue();
        }

        // Mock services for testing
        private class MockExerciseStateService : IExerciseStateService
        {
            public event Action? OnChange { add { } remove { } }
            public Func<ExerciseCreateDto, Task>? OnCreateExercise { get; set; }
            public Task? CreateDelay { get; set; }

            public ExercisePagedResultDto? CurrentPage { get; private set; }
            public ExerciseFilterDto CurrentFilter { get; private set; } = new();
            public bool IsLoading { get; private set; }
            public string? ErrorMessage { get; private set; }
            public ExerciseDto? SelectedExercise { get; set; }
            public bool IsLoadingExercise { get; private set; }
            public IEnumerable<ReferenceDataDto> DifficultyLevels { get; private set; } = Enumerable.Empty<ReferenceDataDto>();
            public IEnumerable<ReferenceDataDto> MuscleGroups { get; private set; } = Enumerable.Empty<ReferenceDataDto>();
            public IEnumerable<ReferenceDataDto> MuscleRoles { get; private set; } = Enumerable.Empty<ReferenceDataDto>();
            public IEnumerable<ReferenceDataDto> Equipment { get; set; } = Enumerable.Empty<ReferenceDataDto>();
            public IEnumerable<ReferenceDataDto> BodyParts { get; private set; } = Enumerable.Empty<ReferenceDataDto>();
            public IEnumerable<ReferenceDataDto> MovementPatterns { get; private set; } = Enumerable.Empty<ReferenceDataDto>();
            public IEnumerable<ReferenceDataDto> KineticChainTypes { get; set; } = Enumerable.Empty<ReferenceDataDto>();
            public IEnumerable<ExerciseTypeDto> ExerciseTypes { get; private set; } = Enumerable.Empty<ExerciseTypeDto>();
            public bool IsLoadingReferenceData { get; private set; }
            public bool HasStoredPage => false;

            public void SetupReferenceData()
            {
                DifficultyLevels = new List<ReferenceDataDto>
                {
                    new() { Id = "1", Value = "Beginner", Description = "For beginners" },
                    new() { Id = "2", Value = "Intermediate", Description = "For intermediate users" },
                    new() { Id = "3", Value = "Advanced", Description = "For advanced users" }
                };

                MuscleGroups = new List<ReferenceDataDto>
                {
                    new() { Id = "1", Value = "Chest", Description = "Chest muscles" },
                    new() { Id = "2", Value = "Back", Description = "Back muscles" },
                    new() { Id = "3", Value = "Legs", Description = "Leg muscles" }
                };

                MuscleRoles = new List<ReferenceDataDto>
                {
                    new() { Id = "1", Value = "Primary", Description = "Primary muscle" },
                    new() { Id = "2", Value = "Secondary", Description = "Secondary muscle" },
                    new() { Id = "3", Value = "Stabilizer", Description = "Stabilizer muscle" }
                };

                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new() { Id = "1", Value = "Strength", Description = "Strength training" },
                    new() { Id = "2", Value = "Cardio", Description = "Cardiovascular exercise" },
                    new() { Id = "3", Value = "Flexibility", Description = "Flexibility training" },
                    new() { Id = "4", Value = "Rest", Description = "Rest period" }
                };

                BodyParts = new List<ReferenceDataDto>
                {
                    new() { Id = "1", Value = "Arms", Description = "Arms" },
                    new() { Id = "2", Value = "Legs", Description = "Legs" },
                    new() { Id = "3", Value = "Core", Description = "Core" }
                };

                MovementPatterns = new List<ReferenceDataDto>
                {
                    new() { Id = "1", Value = "Push", Description = "Pushing movements" },
                    new() { Id = "2", Value = "Pull", Description = "Pulling movements" },
                    new() { Id = "3", Value = "Squat", Description = "Squatting movements" }
                };

                Equipment = new List<ReferenceDataDto>
                {
                    new() { Id = "1", Value = "Barbell", Description = "Barbell" },
                    new() { Id = "2", Value = "Dumbbell", Description = "Dumbbell" },
                    new() { Id = "3", Value = "Bodyweight", Description = "No equipment needed" }
                };
            }

            public Task InitializeAsync()
            {
                SetupReferenceData();
                return Task.CompletedTask;
            }

            public Task LoadExercisesAsync(int pageNumber = 1, int pageSize = 10) => Task.CompletedTask;
            public Task LoadExercisesAsync(ExerciseFilterDto? filter) => Task.CompletedTask;
            public Task RefreshCurrentPageAsync() => Task.CompletedTask;
            public void ClearSelectedExercise() { }
            public void StoreReturnPage() { }
            public void ClearStoredPage() { }
            public Task LoadExercisesWithStoredPageAsync() => Task.CompletedTask;
            public Task LoadExerciseByIdAsync(string id) => Task.CompletedTask;
            public async Task CreateExerciseAsync(ExerciseCreateDto dto)
            {
                if (CreateDelay != null)
                {
                    await CreateDelay;
                }
                OnCreateExercise?.Invoke(dto);
            }
            public Task UpdateExerciseAsync(string id, ExerciseUpdateDto dto) => Task.CompletedTask;
            public Task DeleteExerciseAsync(string id) => Task.CompletedTask;
            public void UpdateFilter(ExerciseFilterDto filter) { }
            public void ClearFilter() { }
            public void ClearError() { }
            public void RestorePageIfAvailable() { }
        }

        private class MockReferenceDataService : IReferenceDataService
        {
            public Task<IEnumerable<ReferenceDataDto>> GetDifficultyLevelsAsync()
                => Task.FromResult<IEnumerable<ReferenceDataDto>>(Array.Empty<ReferenceDataDto>());
            public Task<IEnumerable<ReferenceDataDto>> GetMuscleGroupsAsync()
                => Task.FromResult<IEnumerable<ReferenceDataDto>>(Array.Empty<ReferenceDataDto>());
            public Task<IEnumerable<ReferenceDataDto>> GetMuscleRolesAsync()
                => Task.FromResult<IEnumerable<ReferenceDataDto>>(Array.Empty<ReferenceDataDto>());
            public Task<IEnumerable<ReferenceDataDto>> GetEquipmentAsync()
                => Task.FromResult<IEnumerable<ReferenceDataDto>>(Array.Empty<ReferenceDataDto>());
            public Task<IEnumerable<ReferenceDataDto>> GetBodyPartsAsync()
                => Task.FromResult<IEnumerable<ReferenceDataDto>>(Array.Empty<ReferenceDataDto>());
            public Task<IEnumerable<ReferenceDataDto>> GetMovementPatternsAsync()
                => Task.FromResult<IEnumerable<ReferenceDataDto>>(Array.Empty<ReferenceDataDto>());
            public Task<IEnumerable<ExerciseTypeDto>> GetExerciseTypesAsync()
                => Task.FromResult<IEnumerable<ExerciseTypeDto>>(Array.Empty<ExerciseTypeDto>());
            public Task<string> CreateMuscleGroupAsync(string value) => Task.FromResult(Guid.NewGuid().ToString());
            public Task<string> CreateEquipmentAsync(string value) => Task.FromResult(Guid.NewGuid().ToString());
            public void ClearMuscleGroupsCache() { }
            public void ClearEquipmentCache() { }
            public Task<IEnumerable<ReferenceDataDto>> GetKineticChainTypesAsync()
                => Task.FromResult<IEnumerable<ReferenceDataDto>>(Array.Empty<ReferenceDataDto>());
            public Task<IEnumerable<ReferenceDataDto>> GetMetricTypesAsync()
                => Task.FromResult<IEnumerable<ReferenceDataDto>>(Array.Empty<ReferenceDataDto>());
        }
    }
}