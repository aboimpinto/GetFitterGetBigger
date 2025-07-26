using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.Pages.Exercises;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Builders;
using GetFitterGetBigger.Admin.Tests.TestHelpers;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Bunit.TestDoubles;
using System.Threading.Tasks;
using System.Linq;
using Moq;

namespace GetFitterGetBigger.Admin.Tests.Components.Pages.Exercises
{
    public class ExerciseFormTests : TestContext
    {
        private readonly MockExerciseStateService _mockStateService;
        private readonly MockGenericReferenceDataService _mockReferenceDataService;
        private readonly Mock<IExerciseWeightTypeStateService> _mockWeightTypeStateService;

        public ExerciseFormTests()
        {
            _mockStateService = new MockExerciseStateService();
            _mockReferenceDataService = new MockGenericReferenceDataService();
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

        [Fact]
        public void ExerciseForm_KineticChainDropdown_RendersCorrectly()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            var component = RenderComponent<ExerciseForm>();

            // Act
            var kineticChainSelect = component.Find("select#kineticChain");
            var options = kineticChainSelect.QuerySelectorAll("option");

            // Assert
            kineticChainSelect.Should().NotBeNull();
            options.Should().HaveCount(3); // Empty option + 2 kinetic chain types
            options[0].TextContent.Should().Be("Select kinetic chain type");
            options[1].TextContent.Should().Be("Compound");
            options[2].TextContent.Should().Be("Isolation");
        }

        [Fact]
        public void ExerciseForm_KineticChainDropdown_RequiredForNonRestExercises()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            var component = RenderComponent<ExerciseForm>();

            // Act
            var kineticChainSelect = component.Find("select#kineticChain");

            // Assert - should be required by default (when no exercise type is selected, it defaults to requiring kinetic chain)
            // This is actually correct behavior since in the actual form, kinetic chain is required unless REST is selected
            kineticChainSelect.HasAttribute("required").Should().BeTrue();
            kineticChainSelect.IsDisabled().Should().BeFalse();
        }

        [Fact]
        public void ExerciseForm_KineticChainDropdown_DisabledForRestExercises()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            var component = RenderComponent<ExerciseForm>();

            // Act - Select Rest exercise type
            var restCheckbox = component.FindAll("input[type='checkbox']").First(cb =>
                cb.Parent!.TextContent.Contains("Rest"));
            restCheckbox.Change(true);

            // Assert
            var kineticChainSelect = component.Find("select#kineticChain");
            kineticChainSelect.IsDisabled().Should().BeTrue();
            kineticChainSelect.HasAttribute("required").Should().BeFalse();
            kineticChainSelect.GetAttribute("class").Should().Contain("bg-gray-100");
        }

        [Fact]
        public void ExerciseForm_KineticChainDropdown_RequiredForWorkoutExercises()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            var component = RenderComponent<ExerciseForm>();

            // Act - Select Workout exercise type
            var workoutCheckbox = component.FindAll("input[type='checkbox']").First(cb =>
                cb.Parent!.TextContent.Contains("Workout"));
            workoutCheckbox.Change(true);

            // Assert
            var kineticChainSelect = component.Find("select#kineticChain");
            kineticChainSelect.IsDisabled().Should().BeFalse();
            kineticChainSelect.HasAttribute("required").Should().BeTrue();
        }

        [Fact]
        public void ExerciseForm_KineticChainValidation_ShowsErrorForMissingKineticChain()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            var component = RenderComponent<ExerciseForm>();

            // Act - Fill required fields but leave kinetic chain empty for workout exercise
            component.Find("input#name").Input("Test Exercise");
            component.Find("textarea#description").Input("Test Description");
            component.Find("select#difficulty").Change("1");

            var workoutCheckbox = component.FindAll("input[type='checkbox']").First(cb =>
                cb.Parent!.TextContent.Contains("Workout"));
            workoutCheckbox.Change(true);

            // Try to submit the form
            var submitButton = component.Find("button[data-testid='floating-save-button']");
            submitButton.Click();

            // Assert
            var errorMessage = component.Find("p.text-red-600");
            errorMessage.TextContent.Should().Be("Kinetic chain type is required for non-REST exercises");
        }

        [Fact]
        public void ExerciseForm_KineticChainValidation_ShowsErrorForRestExerciseWithKineticChain()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            var component = RenderComponent<ExerciseForm>();

            // Act - Fill required fields and select kinetic chain
            component.Find("input#name").Input("Rest Exercise");
            component.Find("textarea#description").Input("Rest Description");
            component.Find("select#difficulty").Change("1");
            component.Find("select#kineticChain").Change("kineticchain-1");

            // Then select Rest exercise type (this should clear kinetic chain)
            var restCheckbox = component.FindAll("input[type='checkbox']").First(cb =>
                cb.Parent!.TextContent.Contains("Rest"));
            restCheckbox.Change(true);

            // Try to submit the form
            var submitButton = component.Find("button[data-testid='floating-save-button']");
            submitButton.Click();

            // Assert - Should not show error because kinetic chain should be cleared automatically
            var kineticChainSelect = component.Find("select#kineticChain");
            var value = kineticChainSelect.GetAttribute("value");
            (value == null || value == "").Should().BeTrue();
        }

        [Fact]
        public void ExerciseForm_KineticChainSelection_ClearsWhenRestTypeSelected()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            var component = RenderComponent<ExerciseForm>();

            // Act - First select a kinetic chain type
            var kineticChainSelect = component.Find("select#kineticChain");
            kineticChainSelect.Change("kineticchain-1");

            // Verify it's selected
            kineticChainSelect.GetAttribute("value").Should().Be("kineticchain-1");

            // Then select Rest exercise type
            var restCheckbox = component.FindAll("input[type='checkbox']").First(cb =>
                cb.Parent!.TextContent.Contains("Rest"));
            restCheckbox.Change(true);

            // Assert - kinetic chain should be cleared
            var value = kineticChainSelect.GetAttribute("value");
            (value == null || value == "").Should().BeTrue();
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

        // Mock class removed - using shared MockGenericReferenceDataService from TestHelpers

    }
}