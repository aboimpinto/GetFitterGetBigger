using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.Shared;
using GetFitterGetBigger.Admin.Models.Dtos;
using Microsoft.AspNetCore.Components;
using AngleSharp.Dom;

namespace GetFitterGetBigger.Admin.Tests.Components
{
    public class ExerciseTypeSelectorTests : TestContext
    {
        private List<ExerciseTypeDto> GetTestExerciseTypes()
        {
            return new List<ExerciseTypeDto>
            {
                new() { Id = "1", Value = "Warmup", Description = "Warmup exercises" },
                new() { Id = "2", Value = "Workout", Description = "Main workout exercises" },
                new() { Id = "3", Value = "Cooldown", Description = "Cooldown exercises" },
                new() { Id = "4", Value = "Rest", Description = "Rest period" }
            };
        }

        [Fact]
        public void ExerciseTypeSelector_DisplaysAllAvailableTypes()
        {
            // Arrange
            var availableTypes = GetTestExerciseTypes();

            // Act
            var component = RenderComponent<ExerciseTypeSelector>(parameters => parameters
                .Add(p => p.AvailableTypes, availableTypes)
                .Add(p => p.SelectedTypeIds, new List<string>())
            );

            // Assert
            var checkboxes = component.FindAll("input[type='checkbox']");
            checkboxes.Should().HaveCount(4);

            var labels = component.FindAll("label");
            labels[0].TextContent.Should().Contain("Warmup");
            labels[1].TextContent.Should().Contain("Workout");
            labels[2].TextContent.Should().Contain("Cooldown");
            labels[3].TextContent.Should().Contain("Rest");
        }

        [Fact]
        public void ExerciseTypeSelector_ShowsSelectedTypes()
        {
            // Arrange
            var availableTypes = GetTestExerciseTypes();
            var selectedIds = new List<string> { "1", "2" }; // Warmup and Workout

            // Act
            var component = RenderComponent<ExerciseTypeSelector>(parameters => parameters
                .Add(p => p.AvailableTypes, availableTypes)
                .Add(p => p.SelectedTypeIds, selectedIds)
            );

            // Assert
            var checkboxes = component.FindAll("input[type='checkbox']");
            checkboxes[0].HasAttribute("checked").Should().BeTrue(); // Warmup
            checkboxes[1].HasAttribute("checked").Should().BeTrue(); // Workout
            checkboxes[2].HasAttribute("checked").Should().BeFalse(); // Cooldown
            checkboxes[3].HasAttribute("checked").Should().BeFalse(); // Rest
        }

        [Fact]
        public void SelectingType_AddsToSelectedList()
        {
            // Arrange
            var availableTypes = GetTestExerciseTypes();
            var selectedIds = new List<string> { "1" }; // Start with Warmup
            var updatedIds = new List<string>();

            var component = RenderComponent<ExerciseTypeSelector>(parameters => parameters
                .Add(p => p.AvailableTypes, availableTypes)
                .Add(p => p.SelectedTypeIds, selectedIds)
                .Add(p => p.SelectedTypeIdsChanged, EventCallback.Factory.Create<List<string>>(this, (list) => updatedIds = list))
            );

            // Act - Select Workout
            var workoutCheckbox = component.FindAll("input[type='checkbox']")[1];
            workoutCheckbox.Change(true);

            // Assert
            selectedIds.Should().Contain("1");
            selectedIds.Should().Contain("2");
            selectedIds.Should().HaveCount(2);
        }

        [Fact]
        public void DeselectingType_RemovesFromSelectedList()
        {
            // Arrange
            var availableTypes = GetTestExerciseTypes();
            var selectedIds = new List<string> { "1", "2" }; // Warmup and Workout

            var component = RenderComponent<ExerciseTypeSelector>(parameters => parameters
                .Add(p => p.AvailableTypes, availableTypes)
                .Add(p => p.SelectedTypeIds, selectedIds)
            );

            // Act - Deselect Warmup
            var warmupCheckbox = component.FindAll("input[type='checkbox']")[0];
            warmupCheckbox.Change(false);

            // Assert
            selectedIds.Should().NotContain("1");
            selectedIds.Should().Contain("2");
            selectedIds.Should().HaveCount(1);
        }

        [Fact]
        public void RestType_WhenSelected_DisablesOtherTypes()
        {
            // Arrange
            var availableTypes = GetTestExerciseTypes();
            var selectedIds = new List<string> { "4" }; // Rest selected

            // Act
            var component = RenderComponent<ExerciseTypeSelector>(parameters => parameters
                .Add(p => p.AvailableTypes, availableTypes)
                .Add(p => p.SelectedTypeIds, selectedIds)
            );

            // Assert
            var checkboxes = component.FindAll("input[type='checkbox']");
            checkboxes[0].HasAttribute("disabled").Should().BeTrue(); // Warmup disabled
            checkboxes[1].HasAttribute("disabled").Should().BeTrue(); // Workout disabled
            checkboxes[2].HasAttribute("disabled").Should().BeTrue(); // Cooldown disabled
            checkboxes[3].HasAttribute("disabled").Should().BeTrue(); // Rest disabled (cannot uncheck last item)

            // Should show warning messages
            component.Markup.Should().Contain("Cannot be selected when Rest is selected");
        }

        [Fact]
        public void OtherTypes_WhenSelected_DisablesRestType()
        {
            // Arrange
            var availableTypes = GetTestExerciseTypes();
            var selectedIds = new List<string> { "1", "2" }; // Warmup and Workout

            // Act
            var component = RenderComponent<ExerciseTypeSelector>(parameters => parameters
                .Add(p => p.AvailableTypes, availableTypes)
                .Add(p => p.SelectedTypeIds, selectedIds)
            );

            // Assert
            var restCheckbox = component.FindAll("input[type='checkbox']")[3];
            restCheckbox.HasAttribute("disabled").Should().BeTrue();

            // Should show warning message for Rest
            component.Markup.Should().Contain("Rest type cannot be combined with other types");
        }

        [Fact]
        public void CannotDeselectLastSelectedType()
        {
            // Arrange
            var availableTypes = GetTestExerciseTypes();
            var selectedIds = new List<string> { "1" }; // Only Warmup selected

            var component = RenderComponent<ExerciseTypeSelector>(parameters => parameters
                .Add(p => p.AvailableTypes, availableTypes)
                .Add(p => p.SelectedTypeIds, selectedIds)
            );

            // Act & Assert
            var warmupCheckbox = component.FindAll("input[type='checkbox']")[0];
            warmupCheckbox.HasAttribute("disabled").Should().BeTrue();
        }

        [Fact]
        public void CannotSelectAllFourTypes_ThreeSelected_FourthDisabled()
        {
            // Arrange
            var availableTypes = GetTestExerciseTypes();
            var selectedIds = new List<string> { "1", "2", "3" }; // Three types selected

            // Act
            var component = RenderComponent<ExerciseTypeSelector>(parameters => parameters
                .Add(p => p.AvailableTypes, availableTypes)
                .Add(p => p.SelectedTypeIds, selectedIds)
            );

            // Assert
            var restCheckbox = component.FindAll("input[type='checkbox']")[3];
            restCheckbox.HasAttribute("disabled").Should().BeTrue(); // Fourth type disabled
        }

        [Fact]
        public void SelectingRestType_IsPreventedWhenOthersSelected()
        {
            // Arrange
            var availableTypes = GetTestExerciseTypes();
            var selectedIds = new List<string> { "1", "2" }; // Start with Warmup and Workout

            var component = RenderComponent<ExerciseTypeSelector>(parameters => parameters
                .Add(p => p.AvailableTypes, availableTypes)
                .Add(p => p.SelectedTypeIds, selectedIds)
            );

            // Act & Assert - Rest checkbox should be disabled
            var restCheckbox = component.FindAll("input[type='checkbox']")[3];
            restCheckbox.HasAttribute("disabled").Should().BeTrue();
            
            // Verify the warning message is shown
            component.Markup.Should().Contain("Rest type cannot be combined with other types");
        }

        [Fact]
        public void ValidationMessage_DisplayedWhenRulesViolated()
        {
            // Arrange
            var availableTypes = GetTestExerciseTypes();
            var selectedIds = new List<string> { "4" }; // Rest selected

            var component = RenderComponent<ExerciseTypeSelector>(parameters => parameters
                .Add(p => p.AvailableTypes, availableTypes)
                .Add(p => p.SelectedTypeIds, selectedIds)
            );

            // Try to uncheck Rest (which would leave no selection)
            var restCheckbox = component.FindAll("input[type='checkbox']")[3];
            
            // Act - This should be prevented due to validation
            restCheckbox.Change(false);

            // Assert - Rest should still be selected due to validation
            selectedIds.Should().Contain("4");
            selectedIds.Should().HaveCount(1);
        }

        [Fact]
        public void RulesText_AlwaysVisible()
        {
            // Arrange & Act
            var component = RenderComponent<ExerciseTypeSelector>(parameters => parameters
                .Add(p => p.AvailableTypes, GetTestExerciseTypes())
                .Add(p => p.SelectedTypeIds, new List<string> { "1" })
            );

            // Assert
            component.Markup.Should().Contain("Rules: At least one type must be selected. Rest type is exclusive. Cannot select all four types.");
        }

        [Fact]
        public void MultipleSelectionChanges_MaintainValidState()
        {
            // Arrange
            var availableTypes = GetTestExerciseTypes();
            var selectedIds = new List<string> { "1" }; // Start with Warmup

            var component = RenderComponent<ExerciseTypeSelector>(parameters => parameters
                .Add(p => p.AvailableTypes, availableTypes)
                .Add(p => p.SelectedTypeIds, selectedIds)
            );

            // Act - Multiple operations
            // Add Workout
            component.FindAll("input[type='checkbox']")[1].Change(true);
            selectedIds.Should().HaveCount(2);

            // Add Cooldown
            component.FindAll("input[type='checkbox']")[2].Change(true);
            selectedIds.Should().HaveCount(3);

            // Try to add Rest (should not work due to validation)
            var restCheckbox = component.FindAll("input[type='checkbox']")[3];
            restCheckbox.HasAttribute("disabled").Should().BeTrue();

            // Remove Warmup
            component.FindAll("input[type='checkbox']")[0].Change(false);
            selectedIds.Should().HaveCount(2);

            // Assert final state
            selectedIds.Should().Contain("2"); // Workout
            selectedIds.Should().Contain("3"); // Cooldown
            selectedIds.Should().NotContain("1"); // Warmup removed
            selectedIds.Should().NotContain("4"); // Rest never added
        }
    }
}