using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Models.Results;
using GetFitterGetBigger.Admin.Services;
using Moq;
using Xunit;

namespace GetFitterGetBigger.Admin.Tests.Services
{
    /// <summary>
    /// Tests for AlternativeExerciseLinkValidationService
    /// </summary>
    public class AlternativeExerciseLinkValidationServiceTests
    {
        private readonly AlternativeExerciseLinkValidationService _service;

        public AlternativeExerciseLinkValidationServiceTests()
        {
            _service = new AlternativeExerciseLinkValidationService();
        }

        // Tests now focus on the direct validation methods since service is simplified

        [Fact]
        public void ValidateExerciseTypes_Should_Return_Error_For_Null_Source()
        {
            // Act
            var result = _service.ValidateExerciseTypes(null!, new ExerciseDto());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("SOURCE_EXERCISE_NULL", result.ErrorCode);
        }

        [Fact]
        public void ValidateExerciseTypes_Should_Return_Error_For_Null_Target()
        {
            // Act
            var result = _service.ValidateExerciseTypes(new ExerciseDto(), null!);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("TARGET_EXERCISE_NULL", result.ErrorCode);
        }

        [Fact]
        public void ValidateExerciseTypes_Should_Return_Error_For_Source_Without_Types()
        {
            // Arrange
            var sourceExercise = new ExerciseDto
            {
                Id = "1",
                ExerciseTypes = null
            };

            var targetExercise = new ExerciseDto
            {
                Id = "2",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new() { Value = "Workout" }
                }
            };

            // Act
            var result = _service.ValidateExerciseTypes(sourceExercise, targetExercise);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("MISSING_SOURCE_TYPES", result.ErrorCode);
        }

        [Fact]
        public void ValidateExerciseTypes_Should_Return_Error_For_Target_Without_Types()
        {
            // Arrange
            var sourceExercise = new ExerciseDto
            {
                Id = "1",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new() { Value = "Workout" }
                }
            };

            var targetExercise = new ExerciseDto
            {
                Id = "2",
                ExerciseTypes = null
            };

            // Act
            var result = _service.ValidateExerciseTypes(sourceExercise, targetExercise);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("MISSING_TARGET_TYPES", result.ErrorCode);
        }

        [Fact]
        public void ValidateExerciseTypes_Should_Return_Error_For_No_Shared_Types()
        {
            // Arrange
            var sourceExercise = new ExerciseDto
            {
                Id = "1",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new() { Value = "Workout" }
                }
            };

            var targetExercise = new ExerciseDto
            {
                Id = "2",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new() { Value = "Warmup" }
                }
            };

            // Act
            var result = _service.ValidateExerciseTypes(sourceExercise, targetExercise);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("NO_SHARED_TYPES", result.ErrorCode);
            Assert.Contains("Source types: Workout", result.ErrorMessage);
            Assert.Contains("Target types: Warmup", result.ErrorMessage);
        }

        [Fact]
        public void ValidateExerciseTypes_Should_Return_Success_For_Shared_Types()
        {
            // Arrange
            var sourceExercise = new ExerciseDto
            {
                Id = "1",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new() { Value = "Workout" },
                    new() { Value = "Warmup" }
                }
            };

            var targetExercise = new ExerciseDto
            {
                Id = "2",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new() { Value = "Workout" }
                }
            };

            // Act
            var result = _service.ValidateExerciseTypes(sourceExercise, targetExercise);

            // Assert
            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(0, 0, 0)] // No muscle groups
        [InlineData(1, 1, 60)] // 1 primary match out of 1 max = 60%
        [InlineData(2, 1, 30)] // 1 primary match out of 2 max = 30%
        [InlineData(1, 0, 0)] // No matches = 0%
        public void CalculateMuscleGroupOverlap_Should_Calculate_Correct_Percentage(
            int sourcePrimaryCount, int sharedPrimaryCount, int expectedPercentage)
        {
            // Arrange
            var sourceExercise = CreateExerciseWithMuscleGroups(sourcePrimaryCount, 0, "Source");
            var targetExercise = CreateExerciseWithMuscleGroups(sharedPrimaryCount, 0, "Target", useSameGroups: true);

            // Act
            var result = _service.CalculateMuscleGroupOverlap(sourceExercise, targetExercise);

            // Assert
            Assert.Equal(expectedPercentage, result);
        }

        [Fact]
        public void CalculateMuscleGroupOverlap_Should_Return_Zero_For_Null_Muscle_Groups()
        {
            // Arrange
            var sourceExercise = new ExerciseDto
            {
                Id = "1",
                MuscleGroups = null
            };

            var targetExercise = new ExerciseDto
            {
                Id = "2",
                MuscleGroups = null
            };

            // Act
            var result = _service.CalculateMuscleGroupOverlap(sourceExercise, targetExercise);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void CalculateMuscleGroupOverlap_Should_Handle_Mixed_Primary_And_Secondary()
        {
            // Arrange
            var sourceExercise = new ExerciseDto
            {
                Id = "1",
                MuscleGroups = new List<ExerciseMuscleGroupDto>
                {
                    new() 
                    { 
                        MuscleGroup = new MuscleGroupDto { Id = "1", Value = "Chest" },
                        Role = new MuscleGroupRoleDto { Id = "1", Value = "Primary" }
                    },
                    new() 
                    { 
                        MuscleGroup = new MuscleGroupDto { Id = "2", Value = "Triceps" },
                        Role = new MuscleGroupRoleDto { Id = "2", Value = "Secondary" }
                    }
                }
            };

            var targetExercise = new ExerciseDto
            {
                Id = "2",
                MuscleGroups = new List<ExerciseMuscleGroupDto>
                {
                    new() 
                    { 
                        MuscleGroup = new MuscleGroupDto { Id = "1", Value = "Chest" },
                        Role = new MuscleGroupRoleDto { Id = "1", Value = "Primary" }
                    },
                    new() 
                    { 
                        MuscleGroup = new MuscleGroupDto { Id = "2", Value = "Triceps" },
                        Role = new MuscleGroupRoleDto { Id = "2", Value = "Secondary" }
                    }
                }
            };

            // Act
            var result = _service.CalculateMuscleGroupOverlap(sourceExercise, targetExercise);

            // Assert
            Assert.True(result > 0); // Should have overlap
            Assert.True(result <= 100); // Should not exceed 100%
        }

        // Removed async tests since service is simplified

        private ExerciseDto CreateExerciseWithMuscleGroups(int primaryCount, int secondaryCount, string namePrefix, bool useSameGroups = false)
        {
            var muscleGroups = new List<ExerciseMuscleGroupDto>();

            for (int i = 0; i < primaryCount; i++)
            {
                var groupName = useSameGroups ? "Chest" : $"{namePrefix}Primary{i}";
                muscleGroups.Add(new ExerciseMuscleGroupDto
                {
                    MuscleGroup = new MuscleGroupDto { Id = $"{i + 1}", Value = groupName },
                    Role = new MuscleGroupRoleDto { Id = "1", Value = "Primary" }
                });
            }

            for (int i = 0; i < secondaryCount; i++)
            {
                var groupName = useSameGroups ? "Triceps" : $"{namePrefix}Secondary{i}";
                muscleGroups.Add(new ExerciseMuscleGroupDto
                {
                    MuscleGroup = new MuscleGroupDto { Id = $"{primaryCount + i + 1}", Value = groupName },
                    Role = new MuscleGroupRoleDto { Id = "2", Value = "Secondary" }
                });
            }

            return new ExerciseDto
            {
                Id = "1",
                Name = $"{namePrefix} Exercise",
                MuscleGroups = muscleGroups
            };
        }
    }
}