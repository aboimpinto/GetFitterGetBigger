using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using Moq;
using Xunit;

namespace GetFitterGetBigger.Admin.Tests.Services
{
    /// <summary>
    /// Tests for enhanced ExerciseLinkValidationService with Alternative exercise support
    /// </summary>
    public class ExerciseLinkValidationServiceEnhancedTests
    {
        private readonly Mock<IExerciseLinkService> _mockExerciseLinkService;
        private readonly ExerciseLinkValidationService _service;

        public ExerciseLinkValidationServiceEnhancedTests()
        {
            _mockExerciseLinkService = new Mock<IExerciseLinkService>();
            _service = new ExerciseLinkValidationService(_mockExerciseLinkService.Object);
        }

        [Theory]
        [InlineData("Workout")]
        [InlineData("Warmup")]
        [InlineData("Cooldown")]
        public void ValidateExerciseTypeCompatibility_Should_Allow_Valid_Types(string exerciseType)
        {
            // Arrange
            var exercise = new ExerciseDto
            {
                Id = "1",
                Name = "Test Exercise",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new() { Id = "1", Value = exerciseType }
                }
            };

            // Act
            var result = _service.ValidateExerciseTypeCompatibility(exercise);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidateExerciseTypeCompatibility_Should_Reject_REST_Exercise()
        {
            // Arrange
            var exercise = new ExerciseDto
            {
                Id = "1",
                Name = "Rest Period",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new() { Id = "1", Value = "REST" }
                }
            };

            // Act
            var result = _service.ValidateExerciseTypeCompatibility(exercise);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("REST_EXERCISE_NO_LINKS", result.ErrorCode);
            Assert.Contains("REST exercises cannot have relationships", result.ErrorMessage);
        }

        [Fact]
        public void ValidateExerciseTypeCompatibility_Should_Reject_Invalid_Types()
        {
            // Arrange
            var exercise = new ExerciseDto
            {
                Id = "1",
                Name = "Invalid Exercise",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new() { Id = "1", Value = "Invalid" }
                }
            };

            // Act
            var result = _service.ValidateExerciseTypeCompatibility(exercise);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("INVALID_EXERCISE_TYPE", result.ErrorCode);
            Assert.Contains("Only exercises of type 'Workout', 'Warmup', or 'Cooldown'", result.ErrorMessage);
        }

        [Fact]
        public void ValidateExerciseTypeCompatibility_Should_Allow_Multi_Type_Exercise()
        {
            // Arrange
            var exercise = new ExerciseDto
            {
                Id = "1",
                Name = "Multi-Type Exercise",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new() { Id = "1", Value = "Workout" },
                    new() { Id = "2", Value = "Warmup" }
                }
            };

            // Act
            var result = _service.ValidateExerciseTypeCompatibility(exercise);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidateAlternativeExerciseCompatibility_Should_Reject_Null_Source()
        {
            // Arrange
            var targetExercise = new ExerciseDto
            {
                Id = "2",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new() { Value = "Workout" }
                }
            };

            // Act
            var result = _service.ValidateAlternativeExerciseCompatibility(null!, targetExercise);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("SOURCE_EXERCISE_NULL", result.ErrorCode);
        }

        [Fact]
        public void ValidateAlternativeExerciseCompatibility_Should_Reject_Null_Target()
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

            // Act
            var result = _service.ValidateAlternativeExerciseCompatibility(sourceExercise, null!);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("TARGET_EXERCISE_NULL", result.ErrorCode);
        }

        [Fact]
        public void ValidateAlternativeExerciseCompatibility_Should_Reject_Self_Reference()
        {
            // Arrange
            var exercise = new ExerciseDto
            {
                Id = "1",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new() { Value = "Workout" }
                }
            };

            // Act
            var result = _service.ValidateAlternativeExerciseCompatibility(exercise, exercise);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("SELF_REFERENCE", result.ErrorCode);
        }

        [Fact]
        public void ValidateAlternativeExerciseCompatibility_Should_Reject_No_Shared_Types()
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
            var result = _service.ValidateAlternativeExerciseCompatibility(sourceExercise, targetExercise);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("NO_SHARED_TYPES", result.ErrorCode);
            Assert.Contains("Source types: Workout", result.ErrorMessage);
            Assert.Contains("Target types: Warmup", result.ErrorMessage);
        }

        [Fact]
        public void ValidateAlternativeExerciseCompatibility_Should_Accept_Shared_Types()
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
            var result = _service.ValidateAlternativeExerciseCompatibility(sourceExercise, targetExercise);

            // Assert
            Assert.True(result.IsValid);
        }




        [Fact]
        public void ValidateDuplicateLink_Should_Detect_Alternative_Duplicate()
        {
            // Arrange
            var existingLinks = new List<ExerciseLinkDto>
            {
                new ExerciseLinkDto
                {
                    Id = "1",
                    TargetExerciseId = "target-id",
                    LinkType = "Alternative",
                    IsActive = true
                }
            };

            // Act
            var result = _service.ValidateDuplicateLink(existingLinks, "target-id", ExerciseLinkType.Alternative);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("This exercise is already linked as a alternative exercise", result.ErrorMessage);
        }

        [Fact]
        public async Task ValidateCreateLink_Should_Handle_Alternative_Link_Validation()
        {
            // Arrange
            var exercise = new ExerciseDto
            {
                Id = "source-id",
                Name = "Source Exercise",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new() { Value = "Workout" }
                }
            };

            var existingLinks = new List<ExerciseLinkDto>();

            // Act
            var result = await _service.ValidateCreateLink(exercise, "target-id", ExerciseLinkType.Alternative, existingLinks);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task ValidateCreateLink_Should_Reject_Alternative_Self_Reference()
        {
            // Arrange
            var exercise = new ExerciseDto
            {
                Id = "same-id",
                Name = "Self-Reference Exercise",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new() { Value = "Workout" }
                }
            };

            var existingLinks = new List<ExerciseLinkDto>();

            // Act
            var result = await _service.ValidateCreateLink(exercise, "same-id", ExerciseLinkType.Alternative, existingLinks);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("An exercise cannot be an alternative to itself", result.ErrorMessage);
        }

        [Fact]
        public async Task ValidateCreateLink_Should_Pass_All_Validations_For_Valid_Alternative()
        {
            // Arrange
            var exercise = new ExerciseDto
            {
                Id = "source-id",
                Name = "Source Exercise",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new() { Value = "Workout" }
                }
            };

            var existingLinks = new List<ExerciseLinkDto>();

            // Setup mock to avoid circular reference check failure
            _mockExerciseLinkService.Setup(s => s.GetLinksAsync("target-id", It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(new ExerciseLinksResponseDto
                {
                    Links = new List<ExerciseLinkDto>()
                });

            // Act
            var result = await _service.ValidateCreateLink(exercise, "target-id", ExerciseLinkType.Alternative, existingLinks);

            // Assert
            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("Workout", false)]
        [InlineData("Warmup", false)]
        [InlineData("Cooldown", false)]
        [InlineData("REST", true)]
        [InlineData("Invalid", true)]
        [InlineData("", true)]
        public void ValidateExerciseTypeCompatibility_Should_Handle_Various_Types(string exerciseType, bool shouldFail)
        {
            // Arrange
            var exercise = new ExerciseDto
            {
                Id = "1",
                Name = "Test Exercise",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new() { Id = "1", Value = exerciseType }
                }
            };

            // Act
            var result = _service.ValidateExerciseTypeCompatibility(exercise);

            // Assert
            if (shouldFail)
            {
                Assert.False(result.IsValid);
            }
            else
            {
                Assert.True(result.IsValid);
            }
        }

        [Fact]
        public void ValidateExerciseTypeCompatibility_Should_Handle_Empty_Exercise_Types()
        {
            // Arrange
            var exercise = new ExerciseDto
            {
                Id = "1",
                Name = "Empty Types Exercise",
                ExerciseTypes = new List<ExerciseTypeDto>()
            };

            // Act
            var result = _service.ValidateExerciseTypeCompatibility(exercise);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Only exercises of type 'Workout', 'Warmup', or 'Cooldown'", result.ErrorMessage);
        }

        [Fact]
        public void ValidateAlternativeExerciseCompatibility_Should_Handle_Empty_Source_Types()
        {
            // Arrange
            var sourceExercise = new ExerciseDto
            {
                Id = "1",
                ExerciseTypes = new List<ExerciseTypeDto>()
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
            var result = _service.ValidateAlternativeExerciseCompatibility(sourceExercise, targetExercise);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("MISSING_SOURCE_TYPES", result.ErrorCode);
        }

        [Fact]
        public void ValidateAlternativeExerciseCompatibility_Should_Handle_Empty_Target_Types()
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
                ExerciseTypes = new List<ExerciseTypeDto>()
            };

            // Act
            var result = _service.ValidateAlternativeExerciseCompatibility(sourceExercise, targetExercise);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("MISSING_TARGET_TYPES", result.ErrorCode);
        }
    }
}