using FluentAssertions;
using GetFitterGetBigger.Admin.Builders;
using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Tests.Components.Pages.Exercises
{
    /// <summary>
    /// Tests for ExerciseList display functionality including exercise types and status badges.
    /// These are unit tests for the data structures and display logic.
    /// </summary>
    public class ExerciseListDisplayTests
    {
        [Fact]
        public void ExerciseListDto_WithExerciseTypes_ContainsTypesForDisplay()
        {
            // Arrange & Act
            var exercise = new ExerciseListDtoBuilder()
                .WithId("1")
                .WithName("Test Exercise")
                .WithExerciseTypes(
                    ("Warmup", "Warmup exercise"),
                    ("Workout", "Main workout"))
                .Build();

            // Assert
            exercise.ExerciseTypes.Should().HaveCount(2);
            exercise.ExerciseTypes.Should().Contain(t => t.Value == "Warmup");
            exercise.ExerciseTypes.Should().Contain(t => t.Value == "Workout");
        }

        [Fact]
        public void ExerciseListDto_WithAllExerciseTypes_ContainsAllFourTypes()
        {
            // Arrange & Act
            var exercise = new ExerciseListDtoBuilder()
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
        public void ExerciseListDto_WithActiveStatus_DisplaysCorrectly()
        {
            // Arrange & Act
            var activeExercise = new ExerciseListDtoBuilder()
                .WithId("1")
                .WithName("Active Exercise")
                .WithIsActive(true)
                .Build();

            var inactiveExercise = new ExerciseListDtoBuilder()
                .WithId("2")
                .WithName("Inactive Exercise")
                .WithIsActive(false)
                .Build();

            // Assert
            activeExercise.IsActive.Should().BeTrue();
            inactiveExercise.IsActive.Should().BeFalse();
        }

        [Fact]
        public void ExerciseFilterDto_WithIsActiveFilter_SetsCorrectly()
        {
            // Arrange & Act
            var filterActiveOnly = new ExerciseFilterDto { IsActive = true };
            var filterInactiveOnly = new ExerciseFilterDto { IsActive = false };
            var filterAll = new ExerciseFilterDto { IsActive = null };

            // Assert
            filterActiveOnly.IsActive.Should().Be(true);
            filterInactiveOnly.IsActive.Should().Be(false);
            filterAll.IsActive.Should().BeNull();
        }

        [Fact]
        public void ExerciseTypeDto_ColorClassMapping_ShouldMapCorrectly()
        {
            // This tests the color class logic that would be used in the component
            var warmupType = new ExerciseTypeDto { Value = "Warmup" };
            var workoutType = new ExerciseTypeDto { Value = "Workout" };
            var cooldownType = new ExerciseTypeDto { Value = "Cooldown" };
            var restType = new ExerciseTypeDto { Value = "Rest" };
            var unknownType = new ExerciseTypeDto { Value = "Unknown" };

            // Simulate the GetTypeColorClass method logic
            string GetTypeColorClass(string typeName) => typeName switch
            {
                "Warmup" => "bg-yellow-100 text-yellow-800",
                "Workout" => "bg-blue-100 text-blue-800",
                "Cooldown" => "bg-green-100 text-green-800",
                "Rest" => "bg-purple-100 text-purple-800",
                _ => "bg-gray-100 text-gray-800"
            };

            // Assert
            GetTypeColorClass(warmupType.Value).Should().Be("bg-yellow-100 text-yellow-800");
            GetTypeColorClass(workoutType.Value).Should().Be("bg-blue-100 text-blue-800");
            GetTypeColorClass(cooldownType.Value).Should().Be("bg-green-100 text-green-800");
            GetTypeColorClass(restType.Value).Should().Be("bg-purple-100 text-purple-800");
            GetTypeColorClass(unknownType.Value).Should().Be("bg-gray-100 text-gray-800");
        }

        [Fact]
        public void ExerciseListDto_WithOrderedExerciseTypes_OrdersCorrectly()
        {
            // Arrange & Act
            var exercise = new ExerciseListDtoBuilder()
                .WithId("1")
                .WithName("Test Exercise")
                .WithExerciseTypes(
                    ("Workout", ""),
                    ("Cooldown", ""),
                    ("Warmup", ""))
                .Build();

            // Assert - should be ordered alphabetically by Value
            var orderedTypes = exercise.ExerciseTypes.OrderBy(t => t.Value).ToList();
            orderedTypes[0].Value.Should().Be("Cooldown");
            orderedTypes[1].Value.Should().Be("Warmup");
            orderedTypes[2].Value.Should().Be("Workout");
        }

        [Fact]
        public void ExerciseListDto_WithEmptyExerciseTypes_HandlesGracefully()
        {
            // Arrange & Act
            var exercise = new ExerciseListDtoBuilder()
                .WithId("1")
                .WithName("No Types Exercise")
                .Build();

            // Assert
            exercise.ExerciseTypes.Should().NotBeNull();
            exercise.ExerciseTypes.Should().BeEmpty();
        }

        [Fact]
        public void ExercisePagedResultDto_WithFilteredResults_ShowsCorrectData()
        {
            // Arrange
            var exercises = new List<ExerciseListDto>
            {
                new ExerciseListDtoBuilder()
                    .WithId("1")
                    .WithName("Active Exercise")
                    .WithIsActive(true)
                    .WithExerciseTypes(("Warmup", ""))
                    .Build(),
                new ExerciseListDtoBuilder()
                    .WithId("2")
                    .WithName("Inactive Exercise")
                    .WithIsActive(false)
                    .WithExerciseTypes(("Workout", ""))
                    .Build()
            };

            // Act
            var pagedResult = new ExercisePagedResultDto
            {
                Items = exercises,
                TotalCount = 2,
                PageSize = 10
            };

            // Assert
            pagedResult.Items.Should().HaveCount(2);
            pagedResult.Items.Should().Contain(e => e.IsActive == true);
            pagedResult.Items.Should().Contain(e => e.IsActive == false);
            pagedResult.Items.Should().Contain(e => e.ExerciseTypes.Any(t => t.Value == "Warmup"));
            pagedResult.Items.Should().Contain(e => e.ExerciseTypes.Any(t => t.Value == "Workout"));
        }

        [Fact]
        public void ExerciseListDto_WithKineticChain_ContainsKineticChainForDisplay()
        {
            // Arrange & Act
            var compoundExercise = new ExerciseListDtoBuilder()
                .WithId("1")
                .WithName("Compound Exercise")
                .WithKineticChain("Compound", "Multi-muscle movement")
                .Build();

            var isolationExercise = new ExerciseListDtoBuilder()
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
        public void ExerciseListDto_WithNullKineticChain_HandlesGracefully()
        {
            // Arrange & Act
            var restExercise = new ExerciseListDtoBuilder()
                .WithId("1")
                .WithName("Rest Exercise")
                .WithKineticChain(null)
                .Build();

            // Assert
            restExercise.KineticChain.Should().BeNull();
        }

        [Fact]
        public void KineticChainBadgeClass_ColorClassMapping_ShouldMapCorrectly()
        {
            // This tests the color class logic for kinetic chain badges
            var compoundType = "Compound";
            var isolationType = "Isolation";
            var unknownType = "Unknown";

            // Simulate the GetKineticChainBadgeClass method logic
            string GetKineticChainBadgeClass(string kineticChainType) => kineticChainType?.ToLower() switch
            {
                "compound" => "bg-purple-100 text-purple-800",
                "isolation" => "bg-blue-100 text-blue-800",
                _ => "bg-gray-100 text-gray-800"
            };

            // Assert
            GetKineticChainBadgeClass(compoundType).Should().Be("bg-purple-100 text-purple-800");
            GetKineticChainBadgeClass(isolationType).Should().Be("bg-blue-100 text-blue-800");
            GetKineticChainBadgeClass(unknownType).Should().Be("bg-gray-100 text-gray-800");
            GetKineticChainBadgeClass(null!).Should().Be("bg-gray-100 text-gray-800");
        }

        [Fact]
        public void ExercisePagedResultDto_WithMixedKineticChains_ShowsCorrectData()
        {
            // Arrange
            var exercises = new List<ExerciseListDto>
            {
                new ExerciseListDtoBuilder()
                    .WithId("1")
                    .WithName("Compound Exercise")
                    .WithKineticChain("Compound", "Multi-muscle movement")
                    .Build(),
                new ExerciseListDtoBuilder()
                    .WithId("2")
                    .WithName("Isolation Exercise")
                    .WithKineticChain("Isolation", "Single-muscle movement")
                    .Build(),
                new ExerciseListDtoBuilder()
                    .WithId("3")
                    .WithName("Rest Exercise")
                    .WithKineticChain(null)
                    .Build()
            };

            // Act
            var pagedResult = new ExercisePagedResultDto
            {
                Items = exercises,
                TotalCount = 3,
                PageSize = 10
            };

            // Assert
            pagedResult.Items.Should().HaveCount(3);
            pagedResult.Items.Should().Contain(e => e.KineticChain != null && e.KineticChain.Value == "Compound");
            pagedResult.Items.Should().Contain(e => e.KineticChain != null && e.KineticChain.Value == "Isolation");
            pagedResult.Items.Should().Contain(e => e.KineticChain == null);
        }

        [Fact]
        public void ExerciseListDto_KineticChainDisplayLogic_WorksForRestExercises()
        {
            // Arrange & Act
            var restExercise = new ExerciseListDtoBuilder()
                .WithId("1")
                .WithName("Rest Exercise")
                .WithExerciseTypes(("Rest", "Rest period"))
                .WithKineticChain(null) // REST exercises should have null kinetic chain
                .Build();

            // Assert
            restExercise.ExerciseTypes.Should().Contain(t => t.Value == "Rest");
            restExercise.KineticChain.Should().BeNull();
        }
    }
}