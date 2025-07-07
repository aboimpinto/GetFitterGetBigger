using FluentAssertions;
using GetFitterGetBigger.Admin.Builders;
using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Tests.Components.Pages.Exercises
{
    /// <summary>
    /// Tests for ExerciseDetail display functionality including coach notes and exercise types.
    /// These are unit tests for the data structures and display logic.
    /// </summary>
    public class ExerciseDetailDisplayTests
    {
        [Fact]
        public void ExerciseDto_WithCoachNotes_ContainsOrderedNotes()
        {
            // Arrange & Act
            var exercise = new ExerciseDtoBuilder()
                .WithId("1")
                .WithName("Test Exercise")
                .WithCoachNotes(
                    ("Step 1: Start position", 0),
                    ("Step 2: Movement", 1),
                    ("Step 3: End position", 2))
                .Build();

            // Assert
            exercise.CoachNotes.Should().HaveCount(3);

            var orderedNotes = exercise.CoachNotes.OrderBy(cn => cn.Order).ToList();
            orderedNotes[0].Text.Should().Be("Step 1: Start position");
            orderedNotes[0].Order.Should().Be(0);
            orderedNotes[1].Text.Should().Be("Step 2: Movement");
            orderedNotes[1].Order.Should().Be(1);
            orderedNotes[2].Text.Should().Be("Step 3: End position");
            orderedNotes[2].Order.Should().Be(2);
        }

        [Fact]
        public void ExerciseDto_WithUnorderedCoachNotes_OrdersCorrectly()
        {
            // Arrange & Act
            var exercise = new ExerciseDtoBuilder()
                .WithId("1")
                .WithName("Test Exercise")
                .WithCoachNotes(
                    ("Third step", 2),
                    ("First step", 0),
                    ("Second step", 1))
                .Build();

            // Assert
            var orderedNotes = exercise.CoachNotes.OrderBy(cn => cn.Order).Select(cn => cn.Text).ToList();
            orderedNotes[0].Should().Be("First step");
            orderedNotes[1].Should().Be("Second step");
            orderedNotes[2].Should().Be("Third step");
        }

        [Fact]
        public void ExerciseDto_WithEmptyCoachNotes_HandlesGracefully()
        {
            // Arrange & Act
            var exercise = new ExerciseDtoBuilder()
                .WithId("1")
                .WithName("Test Exercise")
                .Build();

            // Assert
            exercise.CoachNotes.Should().NotBeNull();
            exercise.CoachNotes.Should().BeEmpty();
        }

        [Fact]
        public void ExerciseDto_WithExerciseTypes_DisplaysCorrectly()
        {
            // Arrange & Act
            var exercise = new ExerciseDtoBuilder()
                .WithId("1")
                .WithName("Test Exercise")
                .WithExerciseTypes(
                    ("Warmup", ""),
                    ("Workout", ""))
                .Build();

            // Assert
            exercise.ExerciseTypes.Should().HaveCount(2);
            exercise.ExerciseTypes.Should().Contain(t => t.Value == "Warmup");
            exercise.ExerciseTypes.Should().Contain(t => t.Value == "Workout");
        }

        [Fact]
        public void ExerciseDto_WithAllExerciseTypes_DisplaysAllTypes()
        {
            // Arrange & Act
            var exercise = new ExerciseDtoBuilder()
                .WithId("1")
                .WithName("Full Exercise")
                .WithExerciseTypes(
                    ("Warmup", ""),
                    ("Workout", ""),
                    ("Cooldown", ""),
                    ("Rest", ""))
                .Build();

            // Assert
            exercise.ExerciseTypes.Should().HaveCount(4);
            var typeValues = exercise.ExerciseTypes.Select(t => t.Value).ToList();
            typeValues.Should().Contain("Warmup");
            typeValues.Should().Contain("Workout");
            typeValues.Should().Contain("Cooldown");
            typeValues.Should().Contain("Rest");
        }

        [Fact]
        public void ExerciseDto_ExerciseTypeColorMapping_ReturnsCorrectClasses()
        {
            // This tests the color class logic that would be used in the component
            string GetTypeColorClass(string typeName) => typeName switch
            {
                "Warmup" => "bg-yellow-100 text-yellow-800",
                "Workout" => "bg-blue-100 text-blue-800",
                "Cooldown" => "bg-green-100 text-green-800",
                "Rest" => "bg-purple-100 text-purple-800",
                _ => "bg-gray-100 text-gray-800"
            };

            // Assert
            GetTypeColorClass("Warmup").Should().Be("bg-yellow-100 text-yellow-800");
            GetTypeColorClass("Workout").Should().Be("bg-blue-100 text-blue-800");
            GetTypeColorClass("Cooldown").Should().Be("bg-green-100 text-green-800");
            GetTypeColorClass("Rest").Should().Be("bg-purple-100 text-purple-800");
            GetTypeColorClass("Unknown").Should().Be("bg-gray-100 text-gray-800");
        }

        [Fact]
        public void ExerciseDto_WithOrderedExerciseTypes_OrdersAlphabetically()
        {
            // Arrange & Act
            var exercise = new ExerciseDtoBuilder()
                .WithId("1")
                .WithName("Test Exercise")
                .WithExerciseTypes(
                    ("Workout", ""),
                    ("Cooldown", ""),
                    ("Warmup", ""))
                .Build();

            // Assert - should be ordered alphabetically by Value for display
            var orderedTypes = exercise.ExerciseTypes.OrderBy(t => t.Value).ToList();
            orderedTypes[0].Value.Should().Be("Cooldown");
            orderedTypes[1].Value.Should().Be("Warmup");
            orderedTypes[2].Value.Should().Be("Workout");
        }

        [Fact]
        public void ExerciseDto_WithEmptyExerciseTypes_HandlesGracefully()
        {
            // Arrange & Act
            var exercise = new ExerciseDtoBuilder()
                .WithId("1")
                .WithName("No Types Exercise")
                .Build();

            // Assert
            exercise.ExerciseTypes.Should().NotBeNull();
            exercise.ExerciseTypes.Should().BeEmpty();
        }

        [Fact]
        public void ExerciseDto_ReplacesInstructionsWithCoachNotes_CorrectStructure()
        {
            // Arrange & Act
            var exercise = new ExerciseDtoBuilder()
                .WithId("1")
                .WithName("Test Exercise")
                .WithInstructions("Old instructions text")
                .WithCoachNotes(
                    ("New coach note 1", 0),
                    ("New coach note 2", 1))
                .Build();

            // Assert
            // Instructions field still exists but coach notes are the primary display
            exercise.Instructions.Should().Be("Old instructions text");
            exercise.CoachNotes.Should().HaveCount(2);
            exercise.CoachNotes.Should().Contain(cn => cn.Text == "New coach note 1");
            exercise.CoachNotes.Should().Contain(cn => cn.Text == "New coach note 2");
        }

        [Fact]
        public void CoachNoteDto_WithTextAndOrder_HasCorrectProperties()
        {
            // Arrange & Act
            var coachNote = new CoachNoteDto
            {
                Id = "note-1",
                Text = "Step 1: Setup",
                Order = 0
            };

            // Assert
            coachNote.Id.Should().Be("note-1");
            coachNote.Text.Should().Be("Step 1: Setup");
            coachNote.Order.Should().Be(0);
        }

        [Fact]
        public void ExerciseDto_WithLongCoachNotes_HandlesCorrectly()
        {
            // Arrange & Act
            var longText = new string('a', 1000); // 1000 character text
            var exercise = new ExerciseDtoBuilder()
                .WithId("1")
                .WithName("Test Exercise")
                .WithCoachNotes((longText, 0))
                .Build();

            // Assert
            exercise.CoachNotes.Should().HaveCount(1);
            exercise.CoachNotes[0].Text.Should().Be(longText);
            exercise.CoachNotes[0].Text.Length.Should().Be(1000);
        }

        [Fact]
        public void ExerciseDto_WithKineticChain_ContainsKineticChainForDetailDisplay()
        {
            // Arrange & Act
            var compoundExercise = new ExerciseDtoBuilder()
                .WithId("1")
                .WithName("Compound Exercise")
                .WithKineticChain("Compound", "Multi-muscle movement")
                .Build();

            var isolationExercise = new ExerciseDtoBuilder()
                .WithId("2")
                .WithName("Isolation Exercise")
                .WithKineticChain("Isolation", "Single-muscle movement")
                .Build();

            // Assert
            compoundExercise.KineticChain.Should().NotBeNull();
            compoundExercise.KineticChain!.Value.Should().Be("Compound");
            compoundExercise.KineticChain.Description.Should().Be("Multi-muscle movement");

            isolationExercise.KineticChain.Should().NotBeNull();
            isolationExercise.KineticChain!.Value.Should().Be("Isolation");
            isolationExercise.KineticChain.Description.Should().Be("Single-muscle movement");
        }

        [Fact]
        public void ExerciseDto_WithNullKineticChain_HandlesGracefullyInDetailView()
        {
            // Arrange & Act
            var restExercise = new ExerciseDtoBuilder()
                .WithId("1")
                .WithName("Rest Exercise")
                .WithKineticChain(null)
                .Build();

            // Assert
            restExercise.KineticChain.Should().BeNull();
        }

        [Fact]
        public void ExerciseDto_KineticChainBadgeColorMapping_ReturnsCorrectClasses()
        {
            // This tests the color class logic for kinetic chain badges in detail view
            string GetKineticChainBadgeClass(string kineticChainType) => kineticChainType?.ToLower() switch
            {
                "compound" => "bg-purple-100 text-purple-800",
                "isolation" => "bg-blue-100 text-blue-800",
                _ => "bg-gray-100 text-gray-800"
            };

            // Assert
            GetKineticChainBadgeClass("Compound").Should().Be("bg-purple-100 text-purple-800");
            GetKineticChainBadgeClass("Isolation").Should().Be("bg-blue-100 text-blue-800");
            GetKineticChainBadgeClass("Unknown").Should().Be("bg-gray-100 text-gray-800");
            GetKineticChainBadgeClass(null!).Should().Be("bg-gray-100 text-gray-800");
        }

        [Fact]
        public void ExerciseDto_WithKineticChainTooltip_DisplaysBothValueAndDescription()
        {
            // Arrange & Act
            var exercise = new ExerciseDtoBuilder()
                .WithId("1")
                .WithName("Test Exercise")
                .WithKineticChain("Compound", "Multi-muscle movement pattern")
                .Build();

            // Assert - for tooltip display functionality
            exercise.KineticChain.Should().NotBeNull();
            exercise.KineticChain!.Value.Should().Be("Compound"); // Badge text
            exercise.KineticChain.Description.Should().Be("Multi-muscle movement pattern"); // Tooltip text
        }

        [Fact]
        public void ExerciseDto_KineticChainDisplayLogic_WorksForRestExercisesInDetailView()
        {
            // Arrange & Act
            var restExercise = new ExerciseDtoBuilder()
                .WithId("1")
                .WithName("Rest Exercise")
                .WithExerciseTypes(("Rest", "Rest period"))
                .WithKineticChain(null) // REST exercises should have null kinetic chain
                .Build();

            // Assert
            restExercise.ExerciseTypes.Should().Contain(t => t.Value == "Rest");
            restExercise.KineticChain.Should().BeNull();
        }

        [Fact]
        public void ExerciseDto_WithBothDifficultyAndKineticChain_DisplaysCorrectly()
        {
            // Arrange & Act
            var exercise = new ExerciseDtoBuilder()
                .WithId("1")
                .WithName("Complex Exercise")
                .WithDifficulty(new ReferenceDataDto { Id = "advanced", Value = "Advanced", Description = "Advanced level" })
                .WithKineticChain("Compound", "Multi-muscle movement")
                .Build();

            // Assert - should have both difficulty and kinetic chain for display
            exercise.Difficulty.Should().NotBeNull();
            exercise.Difficulty!.Value.Should().Be("Advanced");
            exercise.KineticChain.Should().NotBeNull();
            exercise.KineticChain!.Value.Should().Be("Compound");
        }
    }
}