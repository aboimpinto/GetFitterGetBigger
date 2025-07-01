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
    public class ExerciseFormIntegrationTests : TestContext
    {
        private readonly MockExerciseStateService _mockStateService;

        public ExerciseFormIntegrationTests()
        {
            _mockStateService = new MockExerciseStateService();

            Services.AddSingleton<IExerciseStateService>(_mockStateService);
            var navMan = Services.GetRequiredService<FakeNavigationManager>();
            navMan.NavigateTo("http://localhost/exercises/new");
        }

        [Fact]
        public void ExerciseForm_CompleteFlow_CreatesExerciseWithZeroCoachNotes()
        {
            // Task 4.1: Test complete exercise CRUD flow with zero coach notes

            // Arrange
            _mockStateService.SetupReferenceData();
            var createdExercise = (ExerciseCreateDto?)null;
            _mockStateService.OnCreateExercise = (model) =>
            {
                createdExercise = model;
                return Task.CompletedTask;
            };

            // Act
            var component = RenderComponent<ExerciseForm>();

            // Fill all required fields
            component.Find("#name").Input("Push-up");
            component.Find("#description").Input("A basic bodyweight exercise");
            component.Find("#difficulty").Change("1");

            // Select exercise type
            component.Find("input[type='checkbox']").Change(true);

            // Select muscle groups
            component.FindAll("select")[1].Change("1"); // Chest
            component.FindAll("select")[2].Change("Primary");

            // Submit form without adding any coach notes
            component.Find("form").Submit();

            // Wait for submission
            component.WaitForState(() => createdExercise != null, TimeSpan.FromSeconds(1));

            // Assert
            createdExercise.Should().NotBeNull();
            createdExercise!.Name.Should().Be("Push-up");
            createdExercise.Description.Should().Be("A basic bodyweight exercise");
            createdExercise.CoachNotes.Should().BeEmpty(); // No coach notes
            createdExercise.MuscleGroups.Should().HaveCount(1);
        }

        [Fact]
        public async Task ExerciseForm_EditMode_RemovesAllCoachNotes()
        {
            // Task 4.2: Test exercise editing to remove all coach notes

            // Arrange
            _mockStateService.SetupReferenceData();

            // Set up an existing exercise with coach notes
            _mockStateService.SelectedExercise = new ExerciseDtoBuilder()
                .WithId("123")
                .WithName("Squat")
                .WithDescription("Lower body exercise")
                .WithDifficulty(new ReferenceDataDto { Id = "1", Value = "Beginner", Description = "For beginners" })
                .WithExerciseTypes(new List<ExerciseTypeDto> {
                    new() { Id = "2", Value = "Workout", Description = "Main workout" }
                })
                .WithMuscleGroups(new List<MuscleGroupWithRoleDto> {
                    new() {
                        MuscleGroup = new ReferenceDataDto { Id = "3", Value = "Legs", Description = "Leg muscles" },
                        Role = new ReferenceDataDto { Id = "1", Value = "Primary", Description = "Primary muscle" }
                    }
                })
                .WithCoachNotes(new List<CoachNoteDto> {
                    new() { Id = "1", Text = "Keep back straight", Order = 0 },
                    new() { Id = "2", Text = "Knees over toes", Order = 1 }
                })
                .Build();

            var updatedExercise = (ExerciseUpdateDto?)null;
            _mockStateService.OnUpdateExercise = (id, model) =>
            {
                updatedExercise = model;
                return Task.CompletedTask;
            };

            // Act
            var component = RenderComponent<ExerciseForm>(parameters => parameters
                .Add(p => p.Id, "123"));

            // Wait for the form to load
            component.WaitForState(() => component.FindAll("textarea").Count > 2);

            // Clear all coach note textareas
            var textareaCount = component.FindAll("textarea").Count;
            for (int i = 1; i < textareaCount; i++)
            {
                await component.InvokeAsync(() =>
                {
                    var textarea = component.FindAll("textarea")[i];
                    textarea.Change("");
                });
            }

            // Submit form
            component.Find("form").Submit();

            // Wait for submission
            component.WaitForState(() => updatedExercise != null, TimeSpan.FromSeconds(1));

            // Assert
            updatedExercise.Should().NotBeNull();
            updatedExercise!.CoachNotes.Should().BeEmpty(); // All notes removed
        }

        [Fact]
        public async Task ExerciseForm_MixedCoachNotes_FiltersEmptyOnes()
        {
            // Task 4.4: Test edge case with mixed empty/filled notes

            // Arrange
            _mockStateService.SetupReferenceData();
            var createdExercise = (ExerciseCreateDto?)null;
            _mockStateService.OnCreateExercise = (model) =>
            {
                createdExercise = model;
                return Task.CompletedTask;
            };

            // Act
            var component = RenderComponent<ExerciseForm>();

            // Fill required fields
            component.Find("#name").Input("Plank");
            component.Find("#description").Input("Core stabilization exercise");
            component.Find("#difficulty").Change("2");
            component.Find("input[type='checkbox']").Change(true);
            component.FindAll("select")[1].Change("1");
            component.FindAll("select")[2].Change("Primary");

            // Add multiple coach notes and fill them
            for (int i = 0; i < 4; i++)
            {
                await component.InvokeAsync(() =>
                {
                    component.FindAll("button").First(b => b.TextContent.Contains("Add Note")).Click();
                });
            }

            // Wait for all notes to be added
            component.WaitForState(() => component.FindAll("textarea").Count >= 5);

            // Fill notes with mixed content - wrap each in InvokeAsync
            await component.InvokeAsync(() => component.FindAll("textarea").Skip(1).First().Change("")); // Empty
            await component.InvokeAsync(() => component.FindAll("textarea").Skip(1).ElementAt(1).Change("Keep core tight")); // Valid
            await component.InvokeAsync(() => component.FindAll("textarea").Skip(1).ElementAt(2).Change("   ")); // Whitespace
            await component.InvokeAsync(() => component.FindAll("textarea").Skip(1).ElementAt(3).Change("Breathe normally")); // Valid

            // Submit form
            component.Find("form").Submit();

            // Wait for submission
            component.WaitForState(() => createdExercise != null, TimeSpan.FromSeconds(1));

            // Assert
            createdExercise.Should().NotBeNull();
            createdExercise!.CoachNotes.Should().HaveCount(2); // Only valid notes
            createdExercise.CoachNotes[0].Text.Should().Be("Keep core tight");
            createdExercise.CoachNotes[0].Order.Should().Be(0);
            createdExercise.CoachNotes[1].Text.Should().Be("Breathe normally");
            createdExercise.CoachNotes[1].Order.Should().Be(1);
        }

        [Fact]
        public async Task ExerciseForm_CoachNotesFunctionality_WorksWhenProvided()
        {
            // Task 4.3: Verify coach notes functionality still works when notes are provided

            // Arrange
            _mockStateService.SetupReferenceData();
            var createdExercise = (ExerciseCreateDto?)null;
            _mockStateService.OnCreateExercise = (model) =>
            {
                createdExercise = model;
                return Task.CompletedTask;
            };

            // Act
            var component = RenderComponent<ExerciseForm>();

            // Fill required fields
            component.Find("#name").Input("Deadlift");
            component.Find("#description").Input("Full body compound exercise");
            component.Find("#difficulty").Change("3");
            component.Find("input[type='checkbox']").Change(true);
            component.FindAll("select")[1].Change("2"); // Back
            component.FindAll("select")[2].Change("Primary");

            // Add two coach notes
            await component.InvokeAsync(() =>
            {
                component.FindAll("button").First(b => b.TextContent.Contains("Add Note")).Click();
            });
            await component.InvokeAsync(() =>
            {
                component.FindAll("button").First(b => b.TextContent.Contains("Add Note")).Click();
            });

            // Wait for notes to be added
            component.WaitForState(() => component.FindAll("textarea").Count >= 3);

            // Fill notes - wrap each in InvokeAsync
            await component.InvokeAsync(() => component.FindAll("textarea").Skip(1).First().Change("Maintain neutral spine"));
            await component.InvokeAsync(() => component.FindAll("textarea").Skip(1).ElementAt(1).Change("Drive through heels"));

            // Submit form
            component.Find("form").Submit();

            // Wait for submission
            component.WaitForState(() => createdExercise != null, TimeSpan.FromSeconds(1));

            // Assert
            createdExercise.Should().NotBeNull();
            createdExercise!.CoachNotes.Should().HaveCount(2);
            createdExercise.CoachNotes[0].Text.Should().Be("Maintain neutral spine");
            createdExercise.CoachNotes[1].Text.Should().Be("Drive through heels");
        }

        [Fact]
        public void ExerciseForm_RestExercise_AllowsZeroCoachNotesAndMuscleGroups()
        {
            // Additional test for Rest exercises

            // Arrange
            _mockStateService.SetupReferenceData();
            var createdExercise = (ExerciseCreateDto?)null;
            _mockStateService.OnCreateExercise = (model) =>
            {
                createdExercise = model;
                return Task.CompletedTask;
            };

            // Act
            var component = RenderComponent<ExerciseForm>();

            // Fill required fields
            component.Find("#name").Input("Rest Period");
            component.Find("#description").Input("Recovery time between sets");
            component.Find("#difficulty").Change("1"); // Should auto-set to Beginner

            // Select Rest exercise type
            var checkboxes = component.FindAll("input[type='checkbox']");
            checkboxes[3].Change(true); // Rest is the 4th type

            // Submit form without coach notes or muscle groups
            component.Find("form").Submit();

            // Wait for submission
            component.WaitForState(() => createdExercise != null, TimeSpan.FromSeconds(1));

            // Assert
            createdExercise.Should().NotBeNull();
            createdExercise!.Name.Should().Be("Rest Period");
            createdExercise.CoachNotes.Should().BeEmpty();
            createdExercise.MuscleGroups.Should().BeEmpty();
            createdExercise.EquipmentIds.Should().BeEmpty();
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
    }
}