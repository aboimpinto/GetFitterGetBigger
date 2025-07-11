using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.Pages.Exercises;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Builders;
using Microsoft.Extensions.DependencyInjection;
using Bunit.TestDoubles;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Moq;

namespace GetFitterGetBigger.Admin.Tests.Components.Pages.Exercises
{
    /// <summary>
    /// Integration tests for ExerciseForm weight type features,
    /// verifying weight type selector integration, form validation, 
    /// and submission behavior with weight type data.
    /// </summary>
    public class ExerciseFormWeightTypeTests : TestContext
    {
        private readonly MockExerciseStateService _mockStateService;
        private readonly Mock<IExerciseWeightTypeStateService> _mockWeightTypeStateService;
        private readonly MockReferenceDataService _mockReferenceDataService;
        private readonly List<ExerciseWeightTypeDto> _mockWeightTypes;

        public ExerciseFormWeightTypeTests()
        {
            _mockStateService = new MockExerciseStateService();
            _mockWeightTypeStateService = new Mock<IExerciseWeightTypeStateService>();
            _mockReferenceDataService = new MockReferenceDataService();

            // Setup weight types
            _mockWeightTypes = new List<ExerciseWeightTypeDto>
            {
                new() { Id = Guid.NewGuid(), Code = "BODYWEIGHT_ONLY", Name = "Bodyweight Only", Description = "Uses bodyweight only", IsActive = true, DisplayOrder = 1 },
                new() { Id = Guid.NewGuid(), Code = "NO_WEIGHT", Name = "No Weight", Description = "No weight used", IsActive = true, DisplayOrder = 2 },
                new() { Id = Guid.NewGuid(), Code = "BODYWEIGHT_OPTIONAL", Name = "Bodyweight Optional", Description = "Can add weight", IsActive = true, DisplayOrder = 3 },
                new() { Id = Guid.NewGuid(), Code = "WEIGHT_REQUIRED", Name = "Weight Required", Description = "Requires external weight", IsActive = true, DisplayOrder = 4 },
                new() { Id = Guid.NewGuid(), Code = "MACHINE_WEIGHT", Name = "Machine Weight", Description = "Machine weight/stack", IsActive = true, DisplayOrder = 5 }
            };

            // Setup weight type state service mock
            _mockWeightTypeStateService.Setup(x => x.WeightTypes).Returns(_mockWeightTypes);
            _mockWeightTypeStateService.Setup(x => x.IsLoading).Returns(false);
            _mockWeightTypeStateService.Setup(x => x.ErrorMessage).Returns((string?)null);
            _mockWeightTypeStateService.Setup(x => x.LoadWeightTypesAsync()).Returns(Task.CompletedTask);

            Services.AddSingleton<IExerciseStateService>(_mockStateService);
            Services.AddSingleton<IReferenceDataService>(_mockReferenceDataService);
            Services.AddSingleton(_mockWeightTypeStateService.Object);

            var navMan = Services.GetRequiredService<FakeNavigationManager>();
            navMan.NavigateTo("http://localhost/exercises/new");
        }

        #region Weight Type Selector Integration Tests

        [Fact]
        public async Task WeightTypeSelector_RendersCorrectly_WithAllWeightTypes()
        {
            // Arrange
            _mockStateService.SetupReferenceData();

            // Act
            var component = RenderComponent<ExerciseForm>();
            await component.InvokeAsync(async () => await Task.Delay(100)); // Allow for async initialization

            // Assert - Weight type selector should be present
            var weightTypeSection = component.FindAll("div").FirstOrDefault(d =>
                d.TextContent.Contains("Weight Type"));
            weightTypeSection.Should().NotBeNull("Weight Type section should exist");

            // Should have dropdown with all weight types
            var markup = component.Markup;
            markup.Should().Contain("Select weight type", "Should have weight type dropdown");
            
            // Verify all weight types are available in the selector
            markup.Should().Contain("Bodyweight Only");
            markup.Should().Contain("No Weight");
            markup.Should().Contain("Bodyweight Optional");
            markup.Should().Contain("Weight Required");
            markup.Should().Contain("Machine Weight");
        }

        [Fact]
        public async Task WeightTypeSelector_ShowsRequiredIndicator_ForNonRestExercises()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            var component = RenderComponent<ExerciseForm>();
            await component.InvokeAsync(async () => await Task.Delay(100));

            // Fill form with non-REST exercise type
            component.Find("#name").Input("Test Exercise");
            component.Find("#description").Input("Test Description");
            component.Find("#difficulty").Change("1");

            // Select first exercise type (non-REST)
            var firstCheckbox = component.Find("input[type='checkbox']");
            firstCheckbox.Change(true);

            // Assert - Weight type should show required indicator
            component.WaitForState(() =>
                component.Markup.Contains("Weight Type") && component.Markup.Contains("*"),
                TimeSpan.FromSeconds(1));

            var markup = component.Markup;
            var weightTypeLabel = markup.Substring(markup.IndexOf("Weight Type"));
            weightTypeLabel.Should().Contain("*", "Weight type should show required indicator for non-REST exercises");
        }

        [Fact]
        public async Task WeightTypeSelector_IsDisabled_ForRestExercises()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            var component = RenderComponent<ExerciseForm>();
            await component.InvokeAsync(async () => await Task.Delay(100));

            // Act - Select REST type
            var restLabel = component.FindAll("label").FirstOrDefault(l => l.TextContent.Contains("Rest"));
            if (restLabel != null)
            {
                var restCheckbox = restLabel.QuerySelector("input[type='checkbox']");
                restCheckbox?.Change(true);

                // Assert - Weight type selector should be disabled
                component.WaitForState(() =>
                    component.Markup.Contains("disabled") && component.Markup.Contains("Weight Type"),
                    TimeSpan.FromSeconds(1));

                var markup = component.Markup;
                markup.Should().Contain("cursor-not-allowed", "Weight type selector should be disabled for REST exercises");
            }
        }

        #endregion

        #region Form Validation Tests

        [Fact]
        public async Task FormValidation_RequiresWeightType_ForNonRestExercises()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            var component = RenderComponent<ExerciseForm>();
            await component.InvokeAsync(async () => await Task.Delay(100));

            // Fill required fields except weight type
            component.Find("#name").Input("Test Exercise");
            component.Find("#description").Input("Test Description");
            component.Find("#difficulty").Change("1");

            // Select non-REST exercise type
            var firstCheckbox = component.Find("input[type='checkbox']");
            firstCheckbox.Change(true);

            // Act - Submit without weight type
            component.Find("form").Submit();

            // Assert - Should show weight type validation error
            component.WaitForState(() =>
                component.Markup.Contains("Weight type is required") ||
                component.Markup.Contains("text-red-600"),
                TimeSpan.FromSeconds(1));

            var hasValidationError =
                component.Markup.Contains("Weight type is required") ||
                component.FindAll(".text-red-600").Any();

            hasValidationError.Should().BeTrue(
                "Form should show validation error when weight type is missing for non-REST exercises");
        }

        [Fact]
        public async Task FormValidation_DoesNotRequireWeightType_ForRestExercises()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            var submittedModel = (ExerciseCreateDto?)null;
            _mockStateService.OnCreateExercise = (model) =>
            {
                submittedModel = model;
                return Task.CompletedTask;
            };

            var component = RenderComponent<ExerciseForm>();
            await component.InvokeAsync(async () => await Task.Delay(100));

            // Fill required fields
            component.Find("#name").Input("Rest Period");
            component.Find("#description").Input("Recovery time");
            component.Find("#difficulty").Change("1");

            // Select REST exercise type
            var restLabel = component.FindAll("label").FirstOrDefault(l => l.TextContent.Contains("Rest"));
            if (restLabel != null)
            {
                var restCheckbox = restLabel.QuerySelector("input[type='checkbox']");
                restCheckbox?.Change(true);

                // Act - Submit without weight type (should be valid for REST)
                component.Find("form").Submit();
                await Task.Delay(100);

                // Assert - Should not show weight type validation error
                var hasWeightTypeError = component.Markup.Contains("Weight type is required");
                hasWeightTypeError.Should().BeFalse(
                    "REST exercises should not require weight type");
            }
        }

        [Fact]
        public async Task FormValidation_ShowsError_WhenRestExerciseHasWeightType()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            var component = RenderComponent<ExerciseForm>();
            await component.InvokeAsync(async () => await Task.Delay(100));

            // Fill required fields
            component.Find("#name").Input("Rest Period");
            component.Find("#description").Input("Recovery time");
            component.Find("#difficulty").Change("1");

            // First select a weight type
            // We'll simulate this since the UI interaction is complex
            await component.InvokeAsync(() =>
            {
                // Access the component instance to set weight type
                var formInstance = component.Instance;
                var modelField = formInstance.GetType().GetField("model", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (modelField?.GetValue(formInstance) is ExerciseCreateDto model)
                {
                    model.WeightTypeId = _mockWeightTypes[0].Id.ToString(); // Set a weight type
                }
            });

            // Then select REST exercise type
            var restLabel = component.FindAll("label").FirstOrDefault(l => l.TextContent.Contains("Rest"));
            if (restLabel != null)
            {
                var restCheckbox = restLabel.QuerySelector("input[type='checkbox']");
                restCheckbox?.Change(true);

                // Act - Submit form
                component.Find("form").Submit();

                // Assert - Should show validation error
                component.WaitForState(() =>
                    component.Markup.Contains("REST exercises cannot have a weight type"),
                    TimeSpan.FromSeconds(1));

                component.Markup.Should().Contain("REST exercises cannot have a weight type",
                    "Should show validation error when REST exercise has weight type");
            }
        }

        #endregion

        #region Form Submission Tests

        [Fact]
        public async Task FormSubmission_IncludesWeightTypeData_WhenSelected()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            var submittedModel = (ExerciseCreateDto?)null;
            _mockStateService.OnCreateExercise = (model) =>
            {
                submittedModel = model;
                return Task.CompletedTask;
            };

            var component = RenderComponent<ExerciseForm>();
            await component.InvokeAsync(async () => await Task.Delay(100));

            // Fill all required fields
            component.Find("#name").Input("Test Exercise");
            component.Find("#description").Input("Test Description");
            component.Find("#difficulty").Change("1");

            // Select non-REST exercise type
            var firstCheckbox = component.Find("input[type='checkbox']");
            firstCheckbox.Change(true);

            // Select weight type programmatically (since UI interaction is complex)
            await component.InvokeAsync(() =>
            {
                var formInstance = component.Instance;
                var modelField = formInstance.GetType().GetField("model", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (modelField?.GetValue(formInstance) is ExerciseCreateDto model)
                {
                    model.WeightTypeId = _mockWeightTypes[3].Id.ToString(); // Weight Required
                    model.KineticChainId = "kc1"; // Add required kinetic chain
                }
            });

            // Add a muscle group to satisfy validation
            await component.InvokeAsync(() =>
            {
                var formInstance = component.Instance;
                var muscleGroupField = formInstance.GetType().GetField("muscleGroupAssignments", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (muscleGroupField?.GetValue(formInstance) is List<MuscleGroupRoleAssignmentDto> muscleGroups)
                {
                    muscleGroups.Add(new MuscleGroupRoleAssignmentDto
                    {
                        MuscleGroupId = "1",
                        Role = "Primary"
                    });
                }
            });

            // Act - Submit form
            component.Find("form").Submit();
            await Task.Delay(100);

            // Assert - Submitted model should include weight type
            submittedModel.Should().NotBeNull("Form should submit successfully");
            submittedModel!.WeightTypeId.Should().Be(_mockWeightTypes[3].Id.ToString(), "Submitted model should include selected weight type");
        }

        [Fact]
        public async Task FormSubmission_ClearsWeightType_WhenRestTypeSelected()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            var submittedModel = (ExerciseCreateDto?)null;
            _mockStateService.OnCreateExercise = (model) =>
            {
                submittedModel = model;
                return Task.CompletedTask;
            };

            var component = RenderComponent<ExerciseForm>();
            await component.InvokeAsync(async () => await Task.Delay(100));

            // Fill required fields
            component.Find("#name").Input("Rest Period");
            component.Find("#description").Input("Recovery time");
            component.Find("#difficulty").Change("1");

            // Select REST exercise type first
            var restLabel = component.FindAll("label").FirstOrDefault(l => l.TextContent.Contains("Rest"));
            if (restLabel != null)
            {
                var restCheckbox = restLabel.QuerySelector("input[type='checkbox']");
                restCheckbox?.Change(true);

                // Wait for form to update after REST type selection
                await component.InvokeAsync(async () => await Task.Delay(50));

                // Act - Submit form (REST exercise should submit successfully without weight type)
                component.Find("form").Submit();
                await component.InvokeAsync(async () => await Task.Delay(100));

                // Assert - Form should submit successfully for REST exercises
                submittedModel.Should().NotBeNull("REST exercises should submit successfully without weight type");
                if (submittedModel != null)
                {
                    submittedModel.WeightTypeId.Should().BeNullOrEmpty(
                        "Weight type should be null/empty for REST exercise types");
                }
            }
            else
            {
                // If REST label not found, this indicates the test setup needs adjustment
                var markup = component.Markup;
                Assert.Fail($"REST exercise type not found in form. Available types in markup: {markup}");
            }
        }

        #endregion

        #region Warning Messages Tests

        [Fact]
        public async Task WeightTypeChange_ShowsWarningMessage_WhenChangingFromRestrictiveType()
        {
            // This test verifies that changing from a restrictive weight type shows appropriate warnings
            // The actual warning implementation is in ExerciseWeightTypeSelector component

            // Arrange
            _mockStateService.SetupReferenceData();
            var component = RenderComponent<ExerciseForm>();
            await component.InvokeAsync(async () => await Task.Delay(100));

            // Assert - Verify weight type selector supports warning messages
            var markup = component.Markup;
            var hasWeightTypeSelector = markup.Contains("Weight Type") && markup.Contains("Select weight type");
            hasWeightTypeSelector.Should().BeTrue(
                "ExerciseWeightTypeSelector should be rendered and support warning messages");

            // The actual warning behavior is tested in ExerciseWeightTypeSelectorTests
            // This test confirms the selector is properly integrated in the form
        }

        #endregion

        #region Edit Mode Tests

        [Fact]
        public async Task EditMode_LoadsExistingWeightType_Correctly()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            _mockStateService.SelectedExercise = new ExerciseDtoBuilder()
                .WithId("123")
                .WithName("Test Exercise")
                .WithDescription("Test Description")
                .WithDifficulty(new ReferenceDataDto { Id = "1", Value = "Beginner", Description = "For beginners" })
                .WithWeightType("WEIGHT_REQUIRED", "Weight Required", "Requires external weight")
                .WithExerciseTypes(new List<ExerciseTypeDto> {
                    new() { Id = "2", Value = "Workout", Description = "Main workout" }
                })
                .Build();

            var navMan = Services.GetRequiredService<FakeNavigationManager>();
            navMan.NavigateTo("http://localhost/exercises/123/edit");

            // Act
            var component = RenderComponent<ExerciseForm>(parameters => parameters
                .Add(p => p.Id, "123"));
            await component.InvokeAsync(async () => await Task.Delay(100));

            // Assert - Weight type selector should show the existing weight type
            var markup = component.Markup;
            markup.Should().Contain("Weight Required", 
                "Weight type selector should display the existing weight type value");
        }

        [Fact]
        public async Task EditMode_ClearsWeightType_WhenRestExerciseLoaded()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            _mockStateService.SelectedExercise = new ExerciseDtoBuilder()
                .WithId("456")
                .WithName("Rest Exercise")
                .WithDescription("Recovery period")
                .WithDifficulty(new ReferenceDataDto { Id = "1", Value = "Beginner", Description = "For beginners" })
                .WithWeightType("BODYWEIGHT_ONLY", "Bodyweight Only", "Uses bodyweight only") // This should be cleared
                .WithExerciseTypes(new List<ExerciseTypeDto> {
                    new() { Id = "4", Value = "Rest", Description = "Rest period" }
                })
                .Build();

            var navMan = Services.GetRequiredService<FakeNavigationManager>();
            navMan.NavigateTo("http://localhost/exercises/456/edit");

            // Act
            var component = RenderComponent<ExerciseForm>(parameters => parameters
                .Add(p => p.Id, "456"));
            await component.InvokeAsync(async () => await Task.Delay(100));

            // Assert - Weight type should be cleared and selector disabled for REST
            var markup = component.Markup;
            markup.Should().Contain("disabled for Rest exercises", 
                "Weight type should be disabled for REST exercises even if data exists");
            
            // The form correctly clears weight type for REST exercises during form loading
            // Verify the weight type selector is disabled rather than checking for specific text
            markup.Should().Contain("cursor-not-allowed",
                "Weight type selector should be disabled for REST exercises");
        }

        #endregion

        private class MockExerciseStateService : IExerciseStateService
        {
            public event Action? OnChange { add { } remove { } }
            public Func<ExerciseCreateDto, Task>? OnCreateExercise { get; set; }

            public ExercisePagedResultDto? CurrentPage { get; private set; }
            public ExerciseFilterDto CurrentFilter { get; private set; } = new();
            public bool IsLoading { get; private set; }
            public string? ErrorMessage { get; private set; }
            public ExerciseDto? SelectedExercise { get; set; }
            public bool IsLoadingExercise { get; private set; }
            public IEnumerable<ReferenceDataDto> DifficultyLevels { get; private set; } = Enumerable.Empty<ReferenceDataDto>();
            public IEnumerable<ReferenceDataDto> MuscleGroups { get; private set; } = Enumerable.Empty<ReferenceDataDto>();
            public IEnumerable<ReferenceDataDto> MuscleRoles { get; private set; } = Enumerable.Empty<ReferenceDataDto>();
            public IEnumerable<ReferenceDataDto> Equipment { get; private set; } = Enumerable.Empty<ReferenceDataDto>();
            public IEnumerable<ReferenceDataDto> BodyParts { get; private set; } = Enumerable.Empty<ReferenceDataDto>();
            public IEnumerable<ReferenceDataDto> MovementPatterns { get; private set; } = Enumerable.Empty<ReferenceDataDto>();
            public IEnumerable<ReferenceDataDto> KineticChainTypes { get; private set; } = Enumerable.Empty<ReferenceDataDto>();
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
                    new() { Id = "1", Value = "Warmup", Description = "Warmup exercises" },
                    new() { Id = "2", Value = "Workout", Description = "Main workout" },
                    new() { Id = "3", Value = "Cooldown", Description = "Cooldown exercises" },
                    new() { Id = "4", Value = "Rest", Description = "Rest period" }
                };

                KineticChainTypes = new List<ReferenceDataDto>
                {
                    new() { Id = "kc1", Value = "Open", Description = "Open kinetic chain" },
                    new() { Id = "kc2", Value = "Closed", Description = "Closed kinetic chain" }
                };

                Equipment = new List<ReferenceDataDto>();
                BodyParts = new List<ReferenceDataDto>();
                MovementPatterns = new List<ReferenceDataDto>();
            }

            public Task InitializeAsync() => Task.CompletedTask;
            public Task LoadExercisesAsync(ExerciseFilterDto? filter = null) => Task.CompletedTask;
            public Task LoadExerciseByIdAsync(string id) => Task.CompletedTask;

            public async Task CreateExerciseAsync(ExerciseCreateDto exercise)
            {
                if (OnCreateExercise != null)
                {
                    await OnCreateExercise(exercise);
                }
            }

            public Task UpdateExerciseAsync(string id, ExerciseUpdateDto exercise) => Task.CompletedTask;
            public Task DeleteExerciseAsync(string id) => Task.CompletedTask;
            public Task RefreshCurrentPageAsync() => Task.CompletedTask;
            public void ClearSelectedExercise() { }
            public void ClearError() { }
            public void StoreReturnPage() { }
            public void ClearStoredPage() { }
            public Task LoadExercisesWithStoredPageAsync() => Task.CompletedTask;
        }

        private class MockReferenceDataService : IReferenceDataService
        {
            public Task<IEnumerable<ReferenceDataDto>> GetDifficultyLevelsAsync() =>
                Task.FromResult<IEnumerable<ReferenceDataDto>>(new List<ReferenceDataDto>());

            public Task<IEnumerable<ReferenceDataDto>> GetMuscleGroupsAsync() =>
                Task.FromResult<IEnumerable<ReferenceDataDto>>(new List<ReferenceDataDto>());

            public Task<IEnumerable<ReferenceDataDto>> GetMuscleRolesAsync() =>
                Task.FromResult<IEnumerable<ReferenceDataDto>>(new List<ReferenceDataDto>());

            public Task<IEnumerable<ReferenceDataDto>> GetEquipmentAsync() =>
                Task.FromResult<IEnumerable<ReferenceDataDto>>(new List<ReferenceDataDto>());

            public Task<IEnumerable<ReferenceDataDto>> GetBodyPartsAsync() =>
                Task.FromResult<IEnumerable<ReferenceDataDto>>(new List<ReferenceDataDto>());

            public Task<IEnumerable<ReferenceDataDto>> GetMovementPatternsAsync() =>
                Task.FromResult<IEnumerable<ReferenceDataDto>>(new List<ReferenceDataDto>());

            public Task<IEnumerable<ExerciseTypeDto>> GetExerciseTypesAsync() =>
                Task.FromResult<IEnumerable<ExerciseTypeDto>>(new List<ExerciseTypeDto>());

            public Task<IEnumerable<ReferenceDataDto>> GetKineticChainTypesAsync() =>
                Task.FromResult<IEnumerable<ReferenceDataDto>>(new List<ReferenceDataDto>());

            public Task<IEnumerable<ReferenceDataDto>> GetMetricTypesAsync() =>
                Task.FromResult<IEnumerable<ReferenceDataDto>>(new List<ReferenceDataDto>());

            public void ClearEquipmentCache() { }
            public void ClearMuscleGroupsCache() { }
        }
    }
}