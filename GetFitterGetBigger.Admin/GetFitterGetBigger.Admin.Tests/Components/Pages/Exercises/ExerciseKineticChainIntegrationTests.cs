using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.Pages.Exercises;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Builders;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Bunit.TestDoubles;
using System.Threading.Tasks;

namespace GetFitterGetBigger.Admin.Tests.Components.Pages.Exercises
{
    /// <summary>
    /// Integration tests for kinetic chain functionality in the exercise form.
    /// Tests the complete flow from form interaction to service calls.
    /// </summary>
    public class ExerciseKineticChainIntegrationTests : TestContext
    {
        private readonly MockExerciseStateService _mockStateService;
        private readonly MockReferenceDataService _mockReferenceDataService;

        public ExerciseKineticChainIntegrationTests()
        {
            _mockStateService = new MockExerciseStateService();
            _mockReferenceDataService = new MockReferenceDataService();

            Services.AddSingleton<IExerciseStateService>(_mockStateService);
            Services.AddSingleton<IReferenceDataService>(_mockReferenceDataService);
            var navMan = Services.GetRequiredService<FakeNavigationManager>();
            navMan.NavigateTo("http://localhost/exercises/new");
        }

        [Fact]
        public void IntegrationTest_CreateExerciseWithCompoundKineticChain_FormDisplaysCorrectly()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            var component = RenderComponent<ExerciseForm>();

            // Act - Fill form with compound exercise
            component.Find("input#name").Input("Bench Press");
            component.Find("textarea#description").Input("Compound upper body exercise");
            component.Find("select#difficulty").Change("2"); // Intermediate
            component.Find("select#kineticChain").Change("kineticchain-1"); // Compound

            // Select Workout exercise type
            var workoutCheckbox = component.FindAll("input[type='checkbox']").First(cb => 
                cb.Parent!.TextContent.Contains("Workout"));
            workoutCheckbox.Change(true);

            // Assert - Check that form displays kinetic chain correctly
            var kineticChainSelect = component.Find("select#kineticChain");
            kineticChainSelect.GetAttribute("value").Should().Be("kineticchain-1");
            kineticChainSelect.HasAttribute("required").Should().BeTrue();
            kineticChainSelect.HasAttribute("disabled").Should().BeFalse();

            // Check that the correct option is selected
            var selectedOption = kineticChainSelect.QuerySelector("option[value='kineticchain-1']");
            selectedOption.Should().NotBeNull();
            selectedOption!.TextContent.Should().Be("Compound");
        }

        [Fact]
        public void IntegrationTest_CreateExerciseWithIsolationKineticChain_FormDisplaysCorrectly()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            var component = RenderComponent<ExerciseForm>();

            // Act - Fill form with isolation exercise
            component.Find("input#name").Input("Bicep Curls");
            component.Find("textarea#description").Input("Isolation arm exercise");
            component.Find("select#difficulty").Change("1"); // Beginner
            component.Find("select#kineticChain").Change("kineticchain-2"); // Isolation

            // Select Workout exercise type
            var workoutCheckbox = component.FindAll("input[type='checkbox']").First(cb => 
                cb.Parent!.TextContent.Contains("Workout"));
            workoutCheckbox.Change(true);

            // Assert - Check that form displays kinetic chain correctly
            var kineticChainSelect = component.Find("select#kineticChain");
            kineticChainSelect.GetAttribute("value").Should().Be("kineticchain-2");
            kineticChainSelect.HasAttribute("required").Should().BeTrue();
            kineticChainSelect.HasAttribute("disabled").Should().BeFalse();

            // Check that the correct option is selected
            var selectedOption = kineticChainSelect.QuerySelector("option[value='kineticchain-2']");
            selectedOption.Should().NotBeNull();
            selectedOption!.TextContent.Should().Be("Isolation");
        }

        [Fact]
        public void IntegrationTest_CreateRestExercise_KineticChainIsNull()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            ExerciseCreateDto? capturedDto = null;
            _mockStateService.OnCreateExercise = dto =>
            {
                capturedDto = dto;
                return Task.CompletedTask;
            };

            var component = RenderComponent<ExerciseForm>();

            // Act - Fill form and select REST type first (this should disable kinetic chain)
            component.Find("input#name").Input("Rest Period");
            component.Find("textarea#description").Input("Rest between exercises");
            component.Find("select#difficulty").Change("1"); // Beginner

            // Select Rest exercise type
            var restCheckbox = component.FindAll("input[type='checkbox']").First(cb => 
                cb.Parent!.TextContent.Contains("Rest"));
            restCheckbox.Change(true);

            // Verify kinetic chain dropdown is disabled
            var kineticChainSelect = component.Find("select#kineticChain");
            kineticChainSelect.HasAttribute("disabled").Should().BeTrue();

            // Submit form
            var submitButton = component.Find("button[data-testid='floating-save-button']");
            submitButton.Click();

            // Assert
            capturedDto.Should().NotBeNull();
            capturedDto!.Name.Should().Be("Rest Period");
            capturedDto.Description.Should().Be("Rest between exercises");
            capturedDto.KineticChainId.Should().BeNull();
            capturedDto.ExerciseTypeIds.Should().Contain("4"); // Rest type
        }

        [Fact]
        public void IntegrationTest_RestExerciseValidation_RejectsKineticChain()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            var component = RenderComponent<ExerciseForm>();

            // Act - Try to set kinetic chain first, then select REST type
            component.Find("input#name").Input("Invalid Rest");
            component.Find("textarea#description").Input("Should not have kinetic chain");
            component.Find("select#difficulty").Change("1");
            component.Find("select#kineticChain").Change("kineticchain-1"); // Try to set compound

            // Then select Rest exercise type (this should clear kinetic chain)
            var restCheckbox = component.FindAll("input[type='checkbox']").First(cb => 
                cb.Parent!.TextContent.Contains("Rest"));
            restCheckbox.Change(true);

            // Assert - kinetic chain should be cleared and disabled
            var kineticChainSelect = component.Find("select#kineticChain");
            kineticChainSelect.HasAttribute("disabled").Should().BeTrue();
            var value = kineticChainSelect.GetAttribute("value");
            (value == null || value == "").Should().BeTrue();
        }

        [Fact]
        public void IntegrationTest_NonRestExerciseValidation_RequiresKineticChain()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            var component = RenderComponent<ExerciseForm>();

            // Act - Fill form but leave kinetic chain empty for workout exercise
            component.Find("input#name").Input("Push Ups");
            component.Find("textarea#description").Input("Bodyweight exercise");
            component.Find("select#difficulty").Change("2");
            // Intentionally leave kinetic chain empty

            // Select Workout exercise type
            var workoutCheckbox = component.FindAll("input[type='checkbox']").First(cb => 
                cb.Parent!.TextContent.Contains("Workout"));
            workoutCheckbox.Change(true);

            // Try to submit form
            var submitButton = component.Find("button[data-testid='floating-save-button']");
            submitButton.Click();

            // Assert - should show validation error
            var errorMessages = component.FindAll("p.text-red-600");
            errorMessages.Should().Contain(e => e.TextContent.Contains("Kinetic chain type is required"));
        }

        [Fact]
        public void IntegrationTest_EditExercise_ChangeKineticChainFromCompoundToIsolation()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            
            // Setup existing exercise with compound kinetic chain
            _mockStateService.SelectedExercise = new ExerciseDtoBuilder()
                .WithId("existing-id")
                .WithName("Existing Exercise")
                .WithDescription("Existing description")
                .WithKineticChain("Compound", "Multi-muscle movement")
                .WithExerciseTypes(("Workout", "Main workout"))
                .Build();

            var navMan = Services.GetRequiredService<FakeNavigationManager>();
            navMan.NavigateTo("http://localhost/exercises/existing-id/edit");

            var component = RenderComponent<ExerciseForm>(parameters => parameters
                .Add(p => p.Id, "existing-id"));

            // Assert - Verify the existing compound kinetic chain is loaded
            var kineticChainSelect = component.Find("select#kineticChain");
            kineticChainSelect.GetAttribute("value").Should().Be("kineticchain-compound");

            // Act - Change kinetic chain from Compound to Isolation
            kineticChainSelect.Change("kineticchain-2"); // Isolation

            // Assert - Verify the change
            kineticChainSelect.GetAttribute("value").Should().Be("kineticchain-2");
        }

        [Fact]
        public void IntegrationTest_EditExercise_AddKineticChainToExistingExercise()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            
            // Setup existing exercise without kinetic chain
            _mockStateService.SelectedExercise = new ExerciseDtoBuilder()
                .WithId("existing-id")
                .WithName("Old Exercise")
                .WithDescription("Exercise without kinetic chain")
                .WithKineticChain(null) // No kinetic chain initially
                .WithExerciseTypes(("Workout", "Main workout"))
                .Build();

            var navMan = Services.GetRequiredService<FakeNavigationManager>();
            navMan.NavigateTo("http://localhost/exercises/existing-id/edit");

            var component = RenderComponent<ExerciseForm>(parameters => parameters
                .Add(p => p.Id, "existing-id"));

            // Assert - Verify kinetic chain field is initially empty
            var kineticChainSelect = component.Find("select#kineticChain");
            var initialValue = kineticChainSelect.GetAttribute("value");
            (initialValue == null || initialValue == "").Should().BeTrue();

            // Act - Add kinetic chain to existing exercise
            kineticChainSelect.Change("kineticchain-1"); // Compound

            // Assert - Verify the kinetic chain was added
            kineticChainSelect.GetAttribute("value").Should().Be("kineticchain-1");
            
            // Verify the form allows submission (no validation errors)
            kineticChainSelect.HasAttribute("required").Should().BeTrue();
            kineticChainSelect.HasAttribute("disabled").Should().BeFalse();
        }

        private class MockExerciseStateService : IExerciseStateService
        {
            public event Action? OnChange { add { } remove { } }
            public Func<ExerciseCreateDto, Task>? OnCreateExercise { get; set; }
            public Func<string, ExerciseUpdateDto, Task>? OnUpdateExercise { get; set; }

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
                    new() { Id = "kineticchain-1", Value = "Compound", Description = "Multi-muscle movement" },
                    new() { Id = "kineticchain-2", Value = "Isolation", Description = "Single-muscle movement" }
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

            public async Task UpdateExerciseAsync(string id, ExerciseUpdateDto exercise)
            {
                if (OnUpdateExercise != null)
                {
                    await OnUpdateExercise(id, exercise);
                }
            }

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