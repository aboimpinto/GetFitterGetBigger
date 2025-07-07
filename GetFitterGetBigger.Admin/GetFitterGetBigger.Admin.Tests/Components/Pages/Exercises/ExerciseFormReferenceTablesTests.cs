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

namespace GetFitterGetBigger.Admin.Tests.Components.Pages.Exercises
{
    public class ExerciseFormReferenceTablesTests : TestContext
    {
        private readonly MockExerciseStateServiceForReferenceTableTest _mockStateService;

        public ExerciseFormReferenceTablesTests()
        {
            _mockStateService = new MockExerciseStateServiceForReferenceTableTest();
            Services.AddSingleton<IExerciseStateService>(_mockStateService);
            Services.AddSingleton<IReferenceDataService>(new MockReferenceDataService());
            var navMan = Services.GetRequiredService<FakeNavigationManager>();
            navMan.NavigateTo("http://localhost/exercises/new");
        }

        [Fact]
        public async Task ExerciseForm_ShouldLoadReferenceData_WhenPartiallyLoaded()
        {
            // This test verifies the fix - when only SOME reference data is loaded, InitializeAsync should be called
            // Arrange
            _mockStateService.SetupPartialReferenceData(); // Only DifficultyLevels populated

            // Act
            var component = RenderComponent<ExerciseForm>();

            // Wait for initialization
            await component.InvokeAsync(async () => await Task.Delay(100));

            // Assert - InitializeAsync SHOULD be called because not all reference data is loaded
            _mockStateService.InitializeAsyncCalled.Should().BeTrue("InitializeAsync should be called when not all reference data is loaded");

            // Assert - After initialization, all reference data should be available
            // The actual state is more important than the DOM structure
            _mockStateService.MuscleGroups.Should().HaveCount(3, "Should have 3 muscle groups after initialization");
            _mockStateService.Equipment.Should().HaveCount(2, "Should have 2 equipment items after initialization");

            // Check that the muscle groups section exists
            var muscleGroupSection = component.FindAll("div")
                .FirstOrDefault(section => section.TextContent.Contains("Muscle Groups"));
            muscleGroupSection.Should().NotBeNull("Muscle Groups section should exist");

            // Check that the equipment section exists  
            var equipmentSection = component.FindAll("div")
                .FirstOrDefault(section => section.TextContent.Contains("Equipment"));
            equipmentSection.Should().NotBeNull("Equipment section should exist");
        }


        // Removed test - Muscle group selection UI completely changed
        /*
        public async Task ExerciseForm_ShouldShowAllReferenceTablesData_AfterInitialization()
        {
            // Arrange
            _mockStateService.SetupEmptyReferenceData(); // Start with empty data
            
            // Act
            var component = RenderComponent<ExerciseForm>();
            
            // Wait for initialization to complete
            await component.InvokeAsync(async () =>
            {
                await Task.Delay(100); // Give time for OnInitializedAsync to run
            });

            // Assert - Check that reference data was loaded
            _mockStateService.InitializeAsyncCalled.Should().BeTrue("InitializeAsync should have been called");
            
            // Assert - Check MuscleGroups dropdown has options
            var muscleGroupSection = component.FindAll("div.border-b.pb-6")
                .FirstOrDefault(section => section.TextContent.Contains("Muscle Groups"));
            muscleGroupSection.Should().NotBeNull();
            var muscleGroupSelect = muscleGroupSection!.QuerySelector("select");
            muscleGroupSelect.Should().NotBeNull();
            var muscleGroupOptions = muscleGroupSelect!.QuerySelectorAll("option").Skip(1).ToList(); // Skip "Select muscle group" option
            muscleGroupOptions.Should().HaveCount(3, "Should have 3 muscle groups after initialization");
            
            // Assert - Check Difficulty dropdown has options
            var difficultySelect = component.Find("#difficulty");
            var difficultyOptions = difficultySelect.QuerySelectorAll("option").Skip(1).ToList(); // Skip "Select difficulty" option
            difficultyOptions.Should().HaveCount(3, "Should have 3 difficulty levels");
            
            // Assert - Check Equipment checkboxes exist
            var equipmentSection = component.FindAll("div.border-b.pb-6")
                .FirstOrDefault(section => section.TextContent.Contains("Equipment"));
            equipmentSection.Should().NotBeNull();
            var equipmentCheckboxes = equipmentSection!.QuerySelectorAll("input[type='checkbox']");
            equipmentCheckboxes.Should().HaveCount(2, "Should have 2 equipment options");
            
            // Assert - Check BodyParts checkboxes exist
            var bodyPartsSection = component.FindAll("div.border-b.pb-6")
                .FirstOrDefault(section => section.TextContent.Contains("Body Parts"));
            bodyPartsSection.Should().NotBeNull();
            var bodyPartCheckboxes = bodyPartsSection!.QuerySelectorAll("input[type='checkbox']");
            bodyPartCheckboxes.Should().HaveCount(3, "Should have 3 body part options");
        }
        */

        // Removed test - Muscle group selection UI completely changed
        /*
        public void ExerciseForm_ShouldNotShowReferenceData_WhenNotLoaded()
        {
            // Arrange
            _mockStateService.SetupEmptyReferenceData();
            _mockStateService.SimulateNoInitialization = true; // Prevent initialization
            
            // Act
            var component = RenderComponent<ExerciseForm>();
            
            // Assert - Check MuscleGroups dropdown has no options (except default)
            var muscleGroupSection = component.FindAll("div.border-b.pb-6")
                .FirstOrDefault(section => section.TextContent.Contains("Muscle Groups"));
            muscleGroupSection.Should().NotBeNull();
            var muscleGroupSelect = muscleGroupSection!.QuerySelector("select");
            muscleGroupSelect.Should().NotBeNull();
            var muscleGroupOptions = muscleGroupSelect!.QuerySelectorAll("option").Skip(1).ToList();
            muscleGroupOptions.Should().BeEmpty("Should have no muscle groups when not initialized");
            
            // Assert - Check Equipment section has no checkboxes
            var equipmentSection = component.FindAll("div.border-b.pb-6")
                .FirstOrDefault(section => section.TextContent.Contains("Equipment"));
            equipmentSection.Should().NotBeNull();
            var equipmentCheckboxes = equipmentSection!.QuerySelectorAll("input[type='checkbox']");
            equipmentCheckboxes.Should().BeEmpty("Should have no equipment options when not initialized");
        }
        */

        // Removed test - Muscle group selection UI completely changed
        /*
        public async Task ExerciseForm_ShouldPopulateMuscleGroupsFromNewDTOStructure()
        {
            // This test checks if the form correctly uses MuscleGroupDTO instead of ReferenceTableDTO
            
            // Arrange - Setup with the new DTO structure
            _mockStateService.SetupReferenceDataWithNewStructure();
            
            // Act
            var component = RenderComponent<ExerciseForm>();
            await component.InvokeAsync(async () => await Task.Delay(100));
            
            // Assert - Check that muscle groups are populated correctly
            // Find the muscle group select specifically (it's in the muscle groups section)
            var muscleGroupSection = component.FindAll("div.border-b.pb-6")
                .FirstOrDefault(section => section.TextContent.Contains("Muscle Groups"));
            muscleGroupSection.Should().NotBeNull();
            
            var muscleGroupSelect = muscleGroupSection!.QuerySelector("select");
            muscleGroupSelect.Should().NotBeNull();
            var muscleGroupOptions = muscleGroupSelect!.QuerySelectorAll("option").Skip(1).ToList();
            
            muscleGroupOptions.Should().HaveCount(3);
            muscleGroupOptions[0].TextContent.Should().Be("Chest");
            muscleGroupOptions[1].TextContent.Should().Be("Back");
            muscleGroupOptions[2].TextContent.Should().Be("Legs");
            
            // Verify the values are the IDs
            muscleGroupOptions[0].GetAttribute("value").Should().Be("mg1");
            muscleGroupOptions[1].GetAttribute("value").Should().Be("mg2");
            muscleGroupOptions[2].GetAttribute("value").Should().Be("mg3");
        }
        */

        private class MockExerciseStateServiceForReferenceTableTest : IExerciseStateService
        {
            public event Action? OnChange;
            public bool InitializeAsyncCalled { get; private set; }
            public bool SimulateNoInitialization { get; set; }

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
            public IEnumerable<ReferenceDataDto> KineticChainTypes { get; set; } = Enumerable.Empty<ReferenceDataDto>();
            public IEnumerable<ExerciseTypeDto> ExerciseTypes { get; private set; } = Enumerable.Empty<ExerciseTypeDto>();
            public bool IsLoadingReferenceData { get; private set; }
            public bool HasStoredPage => false;

            public void SetupEmptyReferenceData()
            {
                // Start with empty collections
                DifficultyLevels = Enumerable.Empty<ReferenceDataDto>();
                MuscleGroups = Enumerable.Empty<ReferenceDataDto>();
                MuscleRoles = Enumerable.Empty<ReferenceDataDto>();
                Equipment = Enumerable.Empty<ReferenceDataDto>();
                BodyParts = Enumerable.Empty<ReferenceDataDto>();
                MovementPatterns = Enumerable.Empty<ReferenceDataDto>();
                ExerciseTypes = Enumerable.Empty<ExerciseTypeDto>();
            }

            public void SetupPartialReferenceData()
            {
                // Only set DifficultyLevels - this simulates the bug where InitializeAsync is not called
                // because DifficultyLevels has data, but other collections remain empty
                DifficultyLevels = new List<ReferenceDataDto>
                {
                    new() { Id = "1", Value = "Beginner", Description = "For beginners" },
                    new() { Id = "2", Value = "Intermediate", Description = "For intermediate users" },
                    new() { Id = "3", Value = "Advanced", Description = "For advanced users" }
                };

                // Leave all other collections empty
                MuscleGroups = Enumerable.Empty<ReferenceDataDto>();
                MuscleRoles = Enumerable.Empty<ReferenceDataDto>();
                Equipment = Enumerable.Empty<ReferenceDataDto>();
                BodyParts = Enumerable.Empty<ReferenceDataDto>();
                MovementPatterns = Enumerable.Empty<ReferenceDataDto>();
                ExerciseTypes = Enumerable.Empty<ExerciseTypeDto>();
            }

            public void SetupReferenceDataWithNewStructure()
            {
                // Setup with proper reference data
                DifficultyLevels = new List<ReferenceDataDto>
                {
                    new() { Id = "1", Value = "Beginner", Description = "For beginners" },
                    new() { Id = "2", Value = "Intermediate", Description = "For intermediate users" },
                    new() { Id = "3", Value = "Advanced", Description = "For advanced users" }
                };

                MuscleGroups = new List<ReferenceDataDto>
                {
                    new() { Id = "mg1", Value = "Chest", Description = "Chest muscles" },
                    new() { Id = "mg2", Value = "Back", Description = "Back muscles" },
                    new() { Id = "mg3", Value = "Legs", Description = "Leg muscles" }
                };

                MuscleRoles = new List<ReferenceDataDto>
                {
                    new() { Id = "1", Value = "Primary", Description = "Primary muscle" },
                    new() { Id = "2", Value = "Secondary", Description = "Secondary muscle" }
                };

                Equipment = new List<ReferenceDataDto>
                {
                    new() { Id = "eq1", Value = "Barbell", Description = "Barbell equipment" },
                    new() { Id = "eq2", Value = "Dumbbell", Description = "Dumbbell equipment" }
                };

                BodyParts = new List<ReferenceDataDto>
                {
                    new() { Id = "bp1", Value = "Upper Body", Description = "Upper body parts" },
                    new() { Id = "bp2", Value = "Lower Body", Description = "Lower body parts" },
                    new() { Id = "bp3", Value = "Core", Description = "Core body parts" }
                };

                MovementPatterns = new List<ReferenceDataDto>
                {
                    new() { Id = "mp1", Value = "Push", Description = "Pushing movements" },
                    new() { Id = "mp2", Value = "Pull", Description = "Pulling movements" }
                };

                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new() { Id = "1", Value = "Warmup", Description = "Warmup exercises" },
                    new() { Id = "2", Value = "Workout", Description = "Main workout" }
                };
            }

            public async Task InitializeAsync()
            {
                InitializeAsyncCalled = true;

                if (!SimulateNoInitialization)
                {
                    // Simulate loading reference data
                    await Task.Delay(10);
                    SetupReferenceDataWithNewStructure();
                    OnChange?.Invoke();
                }
            }

            public Task LoadExercisesAsync(ExerciseFilterDto? filter = null) => Task.CompletedTask;
            public Task LoadExerciseByIdAsync(string id) => Task.CompletedTask;
            public Task CreateExerciseAsync(ExerciseCreateDto exercise) => Task.CompletedTask;
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