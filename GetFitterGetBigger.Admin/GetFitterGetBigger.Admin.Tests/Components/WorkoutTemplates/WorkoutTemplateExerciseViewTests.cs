using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.WorkoutTemplates;
using GetFitterGetBigger.Admin.Models.Dtos;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace GetFitterGetBigger.Admin.Tests.Components.WorkoutTemplates
{
    public class WorkoutTemplateExerciseViewTests : TestContext
    {
        private readonly List<WorkoutTemplateExerciseDto> _testExercises = new()
        {
            new WorkoutTemplateExerciseDto
            {
                ExerciseId = "exercise-1",
                ExerciseName = "Push-ups",
                OrderIndex = 1,
                Sets = 3,
                TargetReps = "15",
                RestSeconds = 60,
                Notes = "Keep core tight"
            },
            new WorkoutTemplateExerciseDto
            {
                ExerciseId = "exercise-2",
                ExerciseName = "Pull-ups",
                OrderIndex = 2,
                Sets = 4,
                TargetReps = "10",
                RestSeconds = 90,
                Notes = null
            },
            new WorkoutTemplateExerciseDto
            {
                ExerciseId = "exercise-3",
                ExerciseName = "Squats",
                OrderIndex = 3,
                Sets = 5,
                TargetReps = "20",
                RestSeconds = 120,
                Notes = "Go below parallel"
            }
        };

        #region Rendering Tests

        [Fact]
        public void Component_RendersExercisesCorrectly()
        {
            // Act
            var component = RenderComponent<WorkoutTemplateExerciseView>(parameters => parameters
                .Add(p => p.Exercises, _testExercises));

            // Assert
            component.Find("[data-testid='workout-template-exercise-view']").Should().NotBeNull();
            
            // Verify all exercises are rendered
            var exerciseItems = component.FindAll("[data-testid^='exercise-item-']");
            exerciseItems.Should().HaveCount(3);

            // Verify first exercise details
            var firstExerciseHeader = component.Find("[data-testid='exercise-header-0']");
            firstExerciseHeader.Should().NotBeNull();
            
            component.Find("[data-testid='exercise-order-0']").TextContent.Trim()
                .Should().Be("1");
            component.Find("[data-testid='exercise-name-0']").TextContent.Trim()
                .Should().Be("Push-ups");
            component.Find("[data-testid='exercise-sets-summary-0']").TextContent
                .Should().Contain("3").And.Contain("sets");
            component.Find("[data-testid='exercise-reps-summary-0']").TextContent
                .Should().Contain("15").And.Contain("reps");
        }

        [Fact]
        public void Component_EmptyState_DisplaysNoExercisesMessage()
        {
            // Act
            var component = RenderComponent<WorkoutTemplateExerciseView>(parameters => parameters
                .Add(p => p.Exercises, new List<WorkoutTemplateExerciseDto>()));

            // Assert
            component.Find("[data-testid='no-exercises']").Should().NotBeNull();
            component.Find("[data-testid='no-exercises']").TextContent
                .Should().Contain("No exercises added to this workout template yet");
            
            // Ensure no exercise items are rendered
            component.FindAll("[data-testid^='exercise-item-']").Should().BeEmpty();
        }

        [Fact]
        public void Component_NullExercises_DisplaysEmptyState()
        {
            // Act
            var component = RenderComponent<WorkoutTemplateExerciseView>(parameters => parameters
                .Add(p => p.Exercises, null));

            // Assert
            component.Find("[data-testid='no-exercises']").Should().NotBeNull();
        }

        #endregion

        #region Expand/Collapse Functionality Tests

        [Fact]
        public async Task Component_ToggleExercise_ExpandsAndCollapsesDetails()
        {
            // Arrange
            var component = RenderComponent<WorkoutTemplateExerciseView>(parameters => parameters
                .Add(p => p.Exercises, _testExercises));

            // Initially collapsed - details should not be visible
            component.FindAll("[data-testid='exercise-details-0']").Should().BeEmpty();

            // Act - Click to expand
            await component.InvokeAsync(() => 
                component.Find("[data-testid='exercise-header-0']").Click());

            // Assert - Details should be visible
            var details = component.Find("[data-testid='exercise-details-0']");
            details.Should().NotBeNull();
            
            // Verify detailed information is shown
            component.Find("[data-testid='exercise-sets-detail-0']").TextContent
                .Should().Be("3");
            component.Find("[data-testid='exercise-reps-detail-0']").TextContent
                .Should().Be("15");
            component.Find("[data-testid='exercise-rest-detail-0']").TextContent
                .Should().Be("1min");
            component.Find("[data-testid='exercise-id-0']").TextContent
                .Should().Be("exercise-1");

            // Act - Click to collapse
            await component.InvokeAsync(() => 
                component.Find("[data-testid='exercise-header-0']").Click());

            // Assert - Details should be hidden again
            component.FindAll("[data-testid='exercise-details-0']").Should().BeEmpty();
        }

        [Fact]
        public async Task Component_ExpandAllByDefault_ShowsAllExerciseDetails()
        {
            // Act
            var component = RenderComponent<WorkoutTemplateExerciseView>(parameters => parameters
                .Add(p => p.Exercises, _testExercises)
                .Add(p => p.ExpandAllByDefault, true));

            // Assert - All exercises should be expanded
            component.FindAll("[data-testid^='exercise-details-']").Should().HaveCount(3);
            component.Find("[data-testid='exercise-details-0']").Should().NotBeNull();
            component.Find("[data-testid='exercise-details-1']").Should().NotBeNull();
            component.Find("[data-testid='exercise-details-2']").Should().NotBeNull();
        }

        [Fact]
        public void Component_Notes_DisplayedOnlyWhenPresent()
        {
            // Arrange
            var component = RenderComponent<WorkoutTemplateExerciseView>(parameters => parameters
                .Add(p => p.Exercises, _testExercises)
                .Add(p => p.ExpandAllByDefault, true));

            // Assert - First exercise has notes
            var firstNotes = component.Find("[data-testid='exercise-notes-0']");
            firstNotes.Should().NotBeNull();
            firstNotes.TextContent.Should().Contain("Keep core tight");

            // Second exercise has no notes - section should not exist
            component.FindAll("[data-testid='exercise-notes-1']").Should().BeEmpty();

            // Third exercise has notes
            var thirdNotes = component.Find("[data-testid='exercise-notes-2']");
            thirdNotes.Should().NotBeNull();
            thirdNotes.TextContent.Should().Contain("Go below parallel");
        }

        #endregion

        #region Summary Footer Tests

        [Fact]
        public void Component_ShowSummary_DisplaysSummaryFooter()
        {
            // Act
            var component = RenderComponent<WorkoutTemplateExerciseView>(parameters => parameters
                .Add(p => p.Exercises, _testExercises)
                .Add(p => p.ShowSummary, true));

            // Assert
            var summary = component.Find("[data-testid='exercises-summary']");
            summary.Should().NotBeNull();
            summary.TextContent.Should().Contain("Total Exercises: 3");
            summary.TextContent.Should().Contain("Estimated Duration:");
        }

        [Fact]
        public void Component_HideSummary_DoesNotDisplayFooter()
        {
            // Act
            var component = RenderComponent<WorkoutTemplateExerciseView>(parameters => parameters
                .Add(p => p.Exercises, _testExercises)
                .Add(p => p.ShowSummary, false));

            // Assert
            component.FindAll("[data-testid='exercises-summary']").Should().BeEmpty();
        }

        #endregion

        #region OnExerciseClick Callback Tests

        [Fact]
        public void Component_OnExerciseClick_NotImplementedInCurrentVersion()
        {
            // Arrange
            WorkoutTemplateExerciseDto? clickedExercise = null;
            var component = RenderComponent<WorkoutTemplateExerciseView>(parameters => parameters
                .Add(p => p.Exercises, _testExercises)
                .Add(p => p.OnExerciseClick, EventCallback.Factory.Create<WorkoutTemplateExerciseDto>(
                    this, exercise => clickedExercise = exercise)));

            // Act - Click on exercise header (synchronously)
            component.Find("[data-testid='exercise-header-0']").Click();

            // Assert - Current implementation only toggles expand/collapse
            // OnExerciseClick is not triggered by header click
            clickedExercise.Should().BeNull();
        }

        #endregion

        #region Public Methods Tests

        [Fact]
        public async Task Component_ExpandAll_ExpandsAllExercises()
        {
            // Arrange
            var component = RenderComponent<WorkoutTemplateExerciseView>(parameters => parameters
                .Add(p => p.Exercises, _testExercises));
            var instance = component.Instance;

            // Initially all should be collapsed
            component.FindAll("[data-testid^='exercise-details-']").Should().BeEmpty();

            // Act
            await component.InvokeAsync(() => instance.ExpandAll());

            // Assert - All exercises should be expanded
            component.FindAll("[data-testid^='exercise-details-']").Should().HaveCount(3);
        }

        [Fact]
        public async Task Component_CollapseAll_CollapsesAllExercises()
        {
            // Arrange
            var component = RenderComponent<WorkoutTemplateExerciseView>(parameters => parameters
                .Add(p => p.Exercises, _testExercises)
                .Add(p => p.ExpandAllByDefault, true));
            var instance = component.Instance;

            // Initially all should be expanded
            component.FindAll("[data-testid^='exercise-details-']").Should().HaveCount(3);

            // Act
            await component.InvokeAsync(() => instance.CollapseAll());

            // Assert - All exercises should be collapsed
            component.FindAll("[data-testid^='exercise-details-']").Should().BeEmpty();
        }

        [Fact]
        public async Task Component_ExpandAll_WithEmptyList_DoesNotThrow()
        {
            // Arrange
            var component = RenderComponent<WorkoutTemplateExerciseView>(parameters => parameters
                .Add(p => p.Exercises, new List<WorkoutTemplateExerciseDto>()));
            var instance = component.Instance;

            // Act & Assert - Should not throw
            await component.InvokeAsync(() => instance.ExpandAll());
        }

        #endregion

        #region Format Helpers Tests

        [Theory]
        [InlineData(30, "30s")]
        [InlineData(45, "45s")]
        [InlineData(60, "1min")]
        [InlineData(90, "1min 30s")]
        [InlineData(120, "2min")]
        [InlineData(150, "2min 30s")]
        [InlineData(180, "3min")]
        [InlineData(0, "0s")]
        public void Component_FormatRestTime_FormatsCorrectly(int seconds, string expected)
        {
            // Arrange
            var component = RenderComponent<WorkoutTemplateExerciseView>(parameters => parameters
                .Add(p => p.Exercises, new List<WorkoutTemplateExerciseDto>()));
            var instance = component.Instance;

            // Act
            var result = instance.FormatRestTime(seconds);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void Component_CalculateEstimatedDuration_CalculatesCorrectly()
        {
            // Arrange
            var component = RenderComponent<WorkoutTemplateExerciseView>(parameters => parameters
                .Add(p => p.Exercises, _testExercises));
            var instance = component.Instance;

            // Act
            var result = instance.CalculateEstimatedDuration();

            // Assert
            // Calculation: 
            // Exercise 1: 3 sets * 45s + 2 * 60s rest = 255s
            // Exercise 2: 4 sets * 45s + 3 * 90s rest = 450s
            // Exercise 3: 5 sets * 45s + 4 * 120s rest = 705s
            // Total: 1410s + 2 * 30s transition = 1470s = 24.5min ≈ 24min
            result.Should().Be("24min");
        }

        [Fact]
        public void Component_CalculateEstimatedDuration_WithEmptyList_ReturnsZero()
        {
            // Arrange
            var component = RenderComponent<WorkoutTemplateExerciseView>(parameters => parameters
                .Add(p => p.Exercises, new List<WorkoutTemplateExerciseDto>()));
            var instance = component.Instance;

            // Act
            var result = instance.CalculateEstimatedDuration();

            // Assert
            result.Should().Be("0min");
        }

        #endregion

        #region UI Interaction Tests

        [Fact]
        public async Task Component_ToggleButton_UpdatesAriaLabel()
        {
            // Arrange
            var component = RenderComponent<WorkoutTemplateExerciseView>(parameters => parameters
                .Add(p => p.Exercises, _testExercises));

            // Initially collapsed
            var toggleButton = component.Find("[data-testid='exercise-toggle-0']");
            toggleButton.GetAttribute("aria-label").Should().Be("Expand exercise details");

            // Act - Expand
            await component.InvokeAsync(() => 
                component.Find("[data-testid='exercise-header-0']").Click());

            // Assert - Aria label should update
            toggleButton = component.Find("[data-testid='exercise-toggle-0']");
            toggleButton.GetAttribute("aria-label").Should().Be("Collapse exercise details");
        }

        [Fact]
        public void Component_HoverEffects_AppliedToHeaders()
        {
            // Arrange & Act
            var component = RenderComponent<WorkoutTemplateExerciseView>(parameters => parameters
                .Add(p => p.Exercises, _testExercises));

            // Assert - Headers should have hover classes
            var header = component.Find("[data-testid='exercise-header-0']");
            var classes = header.GetAttribute("class") ?? "";
            classes.Should().Contain("hover:bg-gray-50");
            classes.Should().Contain("cursor-pointer");
        }

        #endregion

        #region Edge Cases and Error Handling

        [Fact]
        public void Component_LargeExerciseList_RendersAllItems()
        {
            // Arrange
            var largeList = Enumerable.Range(1, 20).Select(i => new WorkoutTemplateExerciseDto
            {
                ExerciseId = $"exercise-{i}",
                ExerciseName = $"Exercise {i}",
                OrderIndex = i,
                Sets = 3,
                TargetReps = "10",
                RestSeconds = 60
            }).ToList();

            // Act
            var component = RenderComponent<WorkoutTemplateExerciseView>(parameters => parameters
                .Add(p => p.Exercises, largeList));

            // Assert
            component.FindAll("[data-testid^='exercise-item-']").Should().HaveCount(20);
            component.Find("[data-testid='exercise-name-19']").TextContent.Should().Contain("Exercise 20");
        }

        [Fact]
        public void Component_ExerciseWithLongName_DisplaysCorrectly()
        {
            // Arrange
            var exercises = new List<WorkoutTemplateExerciseDto>
            {
                new WorkoutTemplateExerciseDto
                {
                    ExerciseId = "long-name",
                    ExerciseName = "Single Arm Dumbbell Row with Resistance Band and Pause at Top Position",
                    OrderIndex = 1,
                    Sets = 3,
                    TargetReps = "12",
                    RestSeconds = 60
                }
            };

            // Act
            var component = RenderComponent<WorkoutTemplateExerciseView>(parameters => parameters
                .Add(p => p.Exercises, exercises));

            // Assert
            component.Find("[data-testid='exercise-name-0']").TextContent
                .Should().Contain("Single Arm Dumbbell Row with Resistance Band");
        }

        #endregion

        #region State Management Tests

        [Fact]
        public async Task Component_IndependentExpansionState_MaintainsCorrectly()
        {
            // Arrange
            var component = RenderComponent<WorkoutTemplateExerciseView>(parameters => parameters
                .Add(p => p.Exercises, _testExercises));

            // Act - Expand first and third exercises
            await component.InvokeAsync(() => 
                component.Find("[data-testid='exercise-header-0']").Click());
            await component.InvokeAsync(() => 
                component.Find("[data-testid='exercise-header-2']").Click());

            // Assert - First and third expanded, second collapsed
            component.FindAll("[data-testid='exercise-details-0']").Should().HaveCount(1);
            component.FindAll("[data-testid='exercise-details-1']").Should().BeEmpty();
            component.FindAll("[data-testid='exercise-details-2']").Should().HaveCount(1);
        }

        #endregion
    }
}