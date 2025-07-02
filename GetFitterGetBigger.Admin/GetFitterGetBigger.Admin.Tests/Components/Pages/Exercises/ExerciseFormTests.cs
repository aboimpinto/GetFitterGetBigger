using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.Pages.Exercises;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Builders;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Bunit.TestDoubles;
using System.Threading.Tasks;
using System.Linq;

namespace GetFitterGetBigger.Admin.Tests.Components.Pages.Exercises
{
    public class ExerciseFormTests : TestContext
    {
        private readonly MockExerciseStateService _mockStateService;
        private readonly MockReferenceDataService _mockReferenceDataService;

        public ExerciseFormTests()
        {
            _mockStateService = new MockExerciseStateService();
            _mockReferenceDataService = new MockReferenceDataService();

            Services.AddSingleton<IExerciseStateService>(_mockStateService);
            Services.AddSingleton<IReferenceDataService>(_mockReferenceDataService);
            var navMan = Services.GetRequiredService<FakeNavigationManager>();
            navMan.NavigateTo("http://localhost/exercises/new");
        }








        [Fact]
        public void ExerciseForm_IntegratesExerciseTypeSelector_Successfully()
        {
            // Task 4.5: Test ExerciseTypeSelector integration in ExerciseForm

            // Arrange
            _mockStateService.SetupReferenceData();
            var component = RenderComponent<ExerciseForm>();

            // Act - Find the ExerciseTypeSelector section
            var exerciseTypesHeader = component.FindAll("h3").FirstOrDefault(h => h.TextContent.Contains("Exercise Types"));
            exerciseTypesHeader.Should().NotBeNull();

            // Should have 4 exercise type checkboxes
            var checkboxes = component.FindAll("input[type='checkbox']").Take(4).ToList();
            checkboxes.Should().HaveCount(4);

            // Should have correct labels
            var labels = new[] { "Warmup", "Workout", "Cooldown", "Rest" };
            for (int i = 0; i < 4; i++)
            {
                var label = checkboxes[i].Parent!.TextContent;
                label.Should().Contain(labels[i]);
            }

            // Verify default state - none checked
            checkboxes.All(cb => !cb.IsChecked()).Should().BeTrue();
        }








        private class MockExerciseStateService : IExerciseStateService
        {
            public event Action? OnChange { add { } remove { } }
            public Func<ExerciseCreateDto, Task>? OnCreateExercise { get; set; }
            public Action<string, ExerciseUpdateDto>? OnUpdateExercise { get; set; }

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

            public Task UpdateExerciseAsync(string id, ExerciseUpdateDto exercise)
            {
                OnUpdateExercise?.Invoke(id, exercise);
                return Task.CompletedTask;
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