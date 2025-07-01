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
    public class ExerciseCoachNotesIntegrationTests : TestContext
    {
        private readonly MockExerciseStateService _mockStateService;

        public ExerciseCoachNotesIntegrationTests()
        {
            _mockStateService = new MockExerciseStateService();

            Services.AddSingleton<IExerciseStateService>(_mockStateService);
            var navMan = Services.GetRequiredService<FakeNavigationManager>();
            navMan.NavigateTo("http://localhost/exercises/new");
        }

        [Fact]
        public async Task Task9_1_CreateExerciseWithMultipleCoachNotes_SavesInCorrectOrder()
        {
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
            component.Find("#name").Input("Bench Press");
            component.Find("#description").Input("Classic chest exercise");
            component.Find("#difficulty").Change("2");

            // Select exercise types
            component.FindAll("input[type='checkbox']")[0].Change(true); // Warmup
            component.FindAll("input[type='checkbox']")[1].Change(true); // Workout

            // Select muscle groups
            component.FindAll("select")[1].Change("1"); // Chest
            component.FindAll("select")[2].Change("Primary");

            // Add 3 coach notes
            for (int i = 0; i < 3; i++)
            {
                await component.InvokeAsync(() =>
                {
                    component.FindAll("button").First(b => b.TextContent.Contains("Add Note")).Click();
                });
            }

            // Wait for notes to be added
            component.WaitForState(() => component.FindAll("textarea").Count >= 4);

            // Fill coach notes
            await component.InvokeAsync(() => component.FindAll("textarea")[1].Change("Keep your back flat on the bench"));
            await component.InvokeAsync(() => component.FindAll("textarea")[2].Change("Lower the bar to chest level"));
            await component.InvokeAsync(() => component.FindAll("textarea")[3].Change("Press up explosively"));

            // Submit form
            component.Find("form").Submit();

            // Wait for submission
            component.WaitForState(() => createdExercise != null, TimeSpan.FromSeconds(1));

            // Assert
            createdExercise.Should().NotBeNull();
            createdExercise!.Name.Should().Be("Bench Press");
            createdExercise.CoachNotes.Should().HaveCount(3);
            createdExercise.CoachNotes[0].Text.Should().Be("Keep your back flat on the bench");
            createdExercise.CoachNotes[0].Order.Should().Be(0);
            createdExercise.CoachNotes[1].Text.Should().Be("Lower the bar to chest level");
            createdExercise.CoachNotes[1].Order.Should().Be(1);
            createdExercise.CoachNotes[2].Text.Should().Be("Press up explosively");
            createdExercise.CoachNotes[2].Order.Should().Be(2);
            createdExercise.ExerciseTypeIds.Should().HaveCount(2);
            createdExercise.ExerciseTypeIds.Should().Contain("1"); // Warmup
            createdExercise.ExerciseTypeIds.Should().Contain("2"); // Workout
        }

        [Fact]
        public async Task Task9_3_EditExerciseWithReorderingCoachNotes_UpdatesOrderCorrectly()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            _mockStateService.SelectedExercise = new ExerciseDtoBuilder()
                .WithId("123")
                .WithName("Pull-ups")
                .WithDescription("Back exercise")
                .WithDifficulty(new ReferenceDataDto { Id = "2", Value = "Intermediate", Description = "For intermediate users" })
                .WithExerciseTypes(new List<ExerciseTypeDto> {
                    new() { Id = "2", Value = "Workout", Description = "Main workout" }
                })
                .WithMuscleGroups(new List<MuscleGroupWithRoleDto> {
                    new() {
                        MuscleGroup = new ReferenceDataDto { Id = "2", Value = "Back", Description = "Back muscles" },
                        Role = new ReferenceDataDto { Id = "1", Value = "Primary", Description = "Primary muscle" }
                    }
                })
                .WithCoachNotes(new List<CoachNoteDto> {
                    new() { Id = "1", Text = "Hang from bar with overhand grip", Order = 0 },
                    new() { Id = "2", Text = "Pull up until chin clears bar", Order = 1 },
                    new() { Id = "3", Text = "Lower with control", Order = 2 }
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

            // Wait for form to load with coach notes
            component.WaitForState(() => component.FindAll("textarea").Count >= 4);

            // Click the down arrow on the first note to move it down
            await component.InvokeAsync(() =>
            {
                var moveDownButtons = component.FindAll("button").Where(b => b.TextContent.Contains("↓")).ToList();
                moveDownButtons[0].Click(); // Move first note down
            });

            // Add a delay to ensure state updates
            await Task.Delay(100);

            // Submit form
            component.Find("form").Submit();

            // Wait for submission
            component.WaitForState(() => updatedExercise != null, TimeSpan.FromSeconds(1));

            // Assert - the first and second notes should have swapped positions
            updatedExercise.Should().NotBeNull();
            updatedExercise!.CoachNotes.Should().HaveCount(3);
            updatedExercise.CoachNotes[0].Text.Should().Be("Pull up until chin clears bar");
            updatedExercise.CoachNotes[0].Order.Should().Be(0);
            updatedExercise.CoachNotes[1].Text.Should().Be("Hang from bar with overhand grip");
            updatedExercise.CoachNotes[1].Order.Should().Be(1);
            updatedExercise.CoachNotes[2].Text.Should().Be("Lower with control");
            updatedExercise.CoachNotes[2].Order.Should().Be(2);
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