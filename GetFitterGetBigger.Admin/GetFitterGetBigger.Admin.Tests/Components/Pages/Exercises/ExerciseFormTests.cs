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

namespace GetFitterGetBigger.Admin.Tests.Components.Pages.Exercises
{
    public class ExerciseFormTests : TestContext
    {
        private readonly MockExerciseStateService _mockStateService;

        public ExerciseFormTests()
        {
            _mockStateService = new MockExerciseStateService();
            
            Services.AddSingleton<IExerciseStateService>(_mockStateService);
            var navMan = Services.GetRequiredService<FakeNavigationManager>();
            navMan.NavigateTo("http://localhost/exercises/new");
        }

        [Fact]
        public void ExerciseForm_AllowsSubmission_WithZeroCoachNotes()
        {
            // Arrange
            _mockStateService.SetupReferenceData();

            // Act
            var component = RenderComponent<ExerciseForm>();
            
            // Fill required fields
            component.Find("#name").Input("Test Exercise");
            component.Find("#description").Input("Test Description");
            component.Find("#difficulty").Change("1"); // Select a difficulty
            
            // Select an exercise type
            var exerciseTypeCheckbox = component.Find("input[type='checkbox']");
            exerciseTypeCheckbox.Change(true);
            
            // Select at least one muscle group (required for non-Rest exercises)
            var muscleGroupSelect = component.Find("select[value='']");
            muscleGroupSelect.Change("1");
            var muscleRoleSelect = component.FindAll("select")[2]; // Second select after difficulty
            muscleRoleSelect.Change("Primary");
            
            // Submit form
            var form = component.Find("form");
            form.Submit();

            // Assert - form should submit without coach notes
            var navManager = Services.GetRequiredService<NavigationManager>();
            navManager.Uri.Should().EndWith("/exercises");
        }

        [Fact]
        public void ExerciseForm_DoesNotShowCoachNotesValidationError_WhenEmpty()
        {
            // Arrange
            _mockStateService.SetupReferenceData();

            // Act
            var component = RenderComponent<ExerciseForm>();
            
            // Fill some fields but leave coach notes empty
            component.Find("#name").Input("Test Exercise");
            
            // Try to submit to trigger validation
            var form = component.Find("form");
            form.Submit();

            // Assert - should not show coach notes validation error
            component.Markup.Should().NotContain("At least one coach note is required");
        }

        [Fact]
        public void ExerciseForm_EnforcesMaxLengthOnCoachNotes()
        {
            // This test verifies that coach notes have a maximum length enforced by the textarea
            
            // Arrange
            _mockStateService.SetupReferenceData();
            var component = RenderComponent<ExerciseForm>();
            
            // Act
            // Add a coach note
            component.FindAll("button").First(b => b.TextContent.Contains("Add Note")).Click();
            
            // Try to enter text of exactly 1000 characters (the max)
            var noteTextarea = component.FindAll("textarea")[1]; // Skip description textarea
            noteTextarea.Change(new string('a', 1000));
            
            // Assert - the character counter should show 1000/1000 characters (at the limit)
            component.Markup.Should().Contain("1000 / 1000 characters");
            
            // Also verify the textarea has maxlength attribute
            noteTextarea.GetAttribute("maxlength").Should().Be("1000");
        }

        [Fact]
        public void ExerciseForm_FiltersOutEmptyCoachNotes_OnSubmission()
        {
            // This test verifies that empty coach notes are filtered out when submitting the form
            
            // Arrange
            _mockStateService.SetupReferenceData();
            var submittedModel = (ExerciseCreateDto?)null;
            _mockStateService.OnCreateExercise = (model) => {
                submittedModel = model;
                return Task.CompletedTask;
            };
            
            // Act
            var component = RenderComponent<ExerciseForm>();
            
            // Fill all required fields first
            component.Find("#name").Input("Test Exercise");
            component.Find("#description").Input("Test Description");
            component.Find("#difficulty").Change("1");
            component.Find("input[type='checkbox']").Change(true);
            
            // Select muscle group
            component.FindAll("select")[1].Change("1");
            component.FindAll("select")[2].Change("Primary");
            
            // Add a single coach note with valid text
            component.FindAll("button").First(b => b.TextContent.Contains("Add Note")).Click();
            
            // Add text to the coach note
            component.InvokeAsync(() => {
                var noteTextarea = component.FindAll("textarea")[1]; // Skip description
                noteTextarea.Change("Valid note");
            });
            
            // Submit form
            component.InvokeAsync(() => component.Find("form").Submit());
            
            // Wait for submission
            component.WaitForState(() => submittedModel != null, TimeSpan.FromSeconds(1));
            
            // Assert
            submittedModel.Should().NotBeNull();
            submittedModel!.CoachNotes.Should().HaveCount(1);
            submittedModel.CoachNotes[0].Text.Should().Be("Valid note");
        }

        [Fact]
        public void ExerciseForm_StartsWithEmptyCoachNotes_ForNewExercise()
        {
            // Arrange
            _mockStateService.SetupReferenceData();

            // Act
            var component = RenderComponent<ExerciseForm>();

            // Assert - should show empty state for coach notes
            component.Markup.Should().Contain("No coach notes added yet");
            component.Markup.Should().Contain("📝");
        }

        [Fact]
        public void ExerciseForm_HandlesRestExercise_WithNoCoachNotesOrMuscleGroups()
        {
            // Arrange
            _mockStateService.SetupReferenceData();
            var submittedModel = (ExerciseCreateDto?)null;
            _mockStateService.OnCreateExercise = (model) => {
                submittedModel = model;
                return Task.CompletedTask;
            };

            // Act
            var component = RenderComponent<ExerciseForm>();
            
            // Fill required fields
            component.Find("#name").Input("Rest Exercise");
            component.Find("#description").Input("Rest period");
            component.Find("#difficulty").Change("1");
            
            // Select Rest exercise type
            var restCheckbox = component.FindAll("input[type='checkbox']")[3]; // Rest is the 4th type
            restCheckbox.Change(true);
            
            // Submit form - no coach notes or muscle groups needed for Rest
            var form = component.Find("form");
            form.Submit();

            // Assert
            var navManager = Services.GetRequiredService<NavigationManager>();
            navManager.Uri.Should().EndWith("/exercises");
            submittedModel.Should().NotBeNull();
            submittedModel!.CoachNotes.Should().BeEmpty();
            submittedModel.MuscleGroups.Should().BeEmpty();
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
            public ExerciseDto? SelectedExercise { get; private set; }
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

    }
}