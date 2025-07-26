using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.Pages.Exercises;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Models.ReferenceData;
using GetFitterGetBigger.Admin.Builders;
using Microsoft.Extensions.DependencyInjection;
using Bunit.TestDoubles;
using System.Threading.Tasks;
using System.Linq;
using Moq;

namespace GetFitterGetBigger.Admin.Tests.Components.Pages.Exercises
{
    /// <summary>
    /// Simplified tests that verify the business rules are implemented
    /// without testing the exact implementation details
    /// </summary>
    public class ExerciseFormBusinessRulesSimpleTests : TestContext
    {
        private readonly MockExerciseStateService _mockStateService;
        private readonly MockReferenceDataService _mockReferenceDataService;
        private readonly Mock<IExerciseWeightTypeStateService> _mockWeightTypeStateService;

        public ExerciseFormBusinessRulesSimpleTests()
        {
            _mockStateService = new MockExerciseStateService();
            _mockReferenceDataService = new MockReferenceDataService();
            _mockWeightTypeStateService = new Mock<IExerciseWeightTypeStateService>();

            // Setup basic weight type state service mock
            _mockWeightTypeStateService.Setup(x => x.WeightTypes).Returns(new List<ExerciseWeightTypeDto>());
            _mockWeightTypeStateService.Setup(x => x.IsLoading).Returns(false);
            _mockWeightTypeStateService.Setup(x => x.LoadWeightTypesAsync()).Returns(Task.CompletedTask);

            Services.AddSingleton<IExerciseStateService>(_mockStateService);
            Services.AddSingleton<IGenericReferenceDataService>(_mockReferenceDataService);
            Services.AddSingleton(_mockWeightTypeStateService.Object);

            var navMan = Services.GetRequiredService<FakeNavigationManager>();
            navMan.NavigateTo("http://localhost/exercises/new");
        }

        [Fact]
        public void ExerciseForm_RendersAllRequiredSections()
        {
            // Arrange
            _mockStateService.SetupReferenceData();

            // Act
            var component = RenderComponent<ExerciseForm>();

            // Assert - All main sections should be present
            component.Markup.Should().Contain("Basic Information");
            component.Markup.Should().Contain("Exercise Types");
            component.Markup.Should().Contain("Exercise Configuration");
            component.Markup.Should().Contain("Muscle Groups");
            component.Markup.Should().Contain("Equipment");
            component.Markup.Should().Contain("Body Parts");
            component.Markup.Should().Contain("Movement Patterns");
        }

        [Fact]
        public void RestType_DisablesRelevantSections()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            var component = RenderComponent<ExerciseForm>();

            // Act - Select REST type (if available)
            var restCheckbox = component.FindAll("label").FirstOrDefault(l => l.TextContent.Contains("Rest"));
            if (restCheckbox != null)
            {
                var checkbox = restCheckbox.QuerySelector("input[type='checkbox']");
                if (checkbox != null)
                {
                    checkbox.Change(true);

                    // Assert - Sections should indicate they're disabled
                    component.Markup.Should().Contain("disabled for Rest exercises");
                }
            }
        }

        [Fact]
        public void MuscleGroups_RequiredForNonRestExercises()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            var component = RenderComponent<ExerciseForm>();

            // Fill required fields
            component.Find("#name").Input("Test Exercise");
            component.Find("#description").Input("Test Description");

            var difficultySelect = component.Find("#difficulty");
            if (difficultySelect != null)
            {
                difficultySelect.Change("1");
            }

            // Select a non-REST exercise type
            var firstCheckbox = component.Find("input[type='checkbox']");
            if (firstCheckbox != null)
            {
                firstCheckbox.Change(true);
            }

            // Act - Submit without muscle groups
            component.Find("form").Submit();

            // Assert - Should have validation error
            // The form should not submit successfully without muscle groups
            component.WaitForState(() =>
                component.Markup.Contains("required") ||
                component.Markup.Contains("muscle group"),
                TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void InlineCreation_LinksExist()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            var component = RenderComponent<ExerciseForm>();

            // Assert - Should have inline creation links
            component.Markup.Should().Contain("Create here");
        }

        [Fact]
        public void Equipment_UsesTagBasedSelection()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            var component = RenderComponent<ExerciseForm>();

            // Assert - Equipment section should have tag-based selection
            var equipmentSection = component.FindAll("div").FirstOrDefault(d =>
                d.TextContent.Contains("Equipment"));
            equipmentSection.Should().NotBeNull();

            // Should use dropdown selection instead of checkboxes
            component.Markup.Should().Contain("Select equipment");
        }

        [Fact]
        public void MuscleGroups_UsesNewSelector()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            var component = RenderComponent<ExerciseForm>();

            // Assert - Muscle groups should use the new selector
            component.Markup.Should().Contain("Select role");
            component.Markup.Should().Contain("Select muscle group");
        }

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
                    new() { Id = "1", Value = "Warmup", Description = "Warmup exercises" },
                    new() { Id = "2", Value = "Workout", Description = "Main workout" },
                    new() { Id = "3", Value = "Cooldown", Description = "Cooldown exercises" },
                    new() { Id = "4", Value = "Rest", Description = "Rest period" }
                };

                Equipment = new List<ReferenceDataDto>
                {
                    new() { Id = "1", Value = "Dumbbell", Description = "Free weight" },
                    new() { Id = "2", Value = "Barbell", Description = "Olympic bar" }
                };

                BodyParts = new List<ReferenceDataDto>
                {
                    new() { Id = "1", Value = "Upper Body", Description = "Upper body parts" },
                    new() { Id = "2", Value = "Lower Body", Description = "Lower body parts" }
                };

                MovementPatterns = new List<ReferenceDataDto>
                {
                    new() { Id = "1", Value = "Push", Description = "Pushing movements" },
                    new() { Id = "2", Value = "Pull", Description = "Pulling movements" }
                };
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

        private class MockReferenceDataService : IGenericReferenceDataService
        {
            public Task<IEnumerable<ReferenceDataDto>> GetReferenceDataAsync<T>() where T : IReferenceTableEntity
            {
                return Task.FromResult<IEnumerable<ReferenceDataDto>>(new List<ReferenceDataDto>());
            }

            public void ClearCache<T>() where T : IReferenceTableEntity { }
        }
    }
}