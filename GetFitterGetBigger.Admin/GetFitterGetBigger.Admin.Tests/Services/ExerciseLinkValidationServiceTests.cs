using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using Moq;
using Xunit;

namespace GetFitterGetBigger.Admin.Tests.Services;

public class ExerciseLinkValidationServiceTests
{
    private readonly Mock<IExerciseLinkService> _mockLinkService;
    private readonly ExerciseLinkValidationService _validationService;

    public ExerciseLinkValidationServiceTests()
    {
        _mockLinkService = new Mock<IExerciseLinkService>();
        _validationService = new ExerciseLinkValidationService(_mockLinkService.Object);
    }

    #region ValidateExerciseTypeCompatibility Tests

    [Fact]
    public void ValidateExerciseTypeCompatibility_ReturnsFailure_WhenExerciseIsNull()
    {
        // Act
        var result = _validationService.ValidateExerciseTypeCompatibility(null!);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Exercise cannot be null", result.ErrorMessage);
        Assert.Equal("EXERCISE_NULL", result.ErrorCode);
    }

    [Fact]
    public void ValidateExerciseTypeCompatibility_ReturnsFailure_WhenNotWorkoutType()
    {
        // Arrange
        var exercise = new ExerciseDto
        {
            Id = "exercise-1",
            Name = "Test Exercise",
            ExerciseTypes = new List<ExerciseTypeDto>
            {
                new() { Id = "type-1", Value = "Strength" },
                new() { Id = "type-2", Value = "Cardio" }
            }
        };

        // Act
        var result = _validationService.ValidateExerciseTypeCompatibility(exercise);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Only exercises of type 'Workout', 'Warmup', or 'Cooldown' can have links", result.ErrorMessage);
        Assert.Contains("Strength, Cardio", result.ErrorMessage);
        Assert.Equal("INVALID_EXERCISE_TYPE", result.ErrorCode);
    }

    [Fact]
    public void ValidateExerciseTypeCompatibility_ReturnsSuccess_WhenIsWorkoutType()
    {
        // Arrange
        var exercise = new ExerciseDto
        {
            Id = "exercise-1",
            Name = "Test Exercise",
            ExerciseTypes = new List<ExerciseTypeDto>
            {
                new() { Id = "type-1", Value = "Workout" },
                new() { Id = "type-2", Value = "Strength" }
            }
        };

        // Act
        var result = _validationService.ValidateExerciseTypeCompatibility(exercise);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
        Assert.Null(result.ErrorCode);
    }

    [Fact]
    public void ValidateExerciseTypeCompatibility_ReturnsSuccess_WhenWorkoutTypeInMixedCase()
    {
        // Arrange
        var exercise = new ExerciseDto
        {
            Id = "exercise-1",
            Name = "Test Exercise",
            ExerciseTypes = new List<ExerciseTypeDto>
            {
                new() { Id = "type-1", Value = "WORKOUT" }
            }
        };

        // Act
        var result = _validationService.ValidateExerciseTypeCompatibility(exercise);

        // Assert
        Assert.True(result.IsValid);
    }

    #endregion

    #region ValidateCircularReference Tests

    [Fact]
    public async Task ValidateCircularReference_ReturnsFailure_WhenIdsAreNull()
    {
        // Act
        var result = await _validationService.ValidateCircularReference(null!, "target", ExerciseLinkType.Warmup);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Exercise IDs cannot be null or empty", result.ErrorMessage);
        Assert.Equal("INVALID_EXERCISE_ID", result.ErrorCode);
    }

    [Fact]
    public async Task ValidateCircularReference_ReturnsFailure_WhenIdsAreEmpty()
    {
        // Act
        var result = await _validationService.ValidateCircularReference("", "target", ExerciseLinkType.Warmup);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Exercise IDs cannot be null or empty", result.ErrorMessage);
        Assert.Equal("INVALID_EXERCISE_ID", result.ErrorCode);
    }

    [Fact]
    public async Task ValidateCircularReference_ReturnsFailure_WhenSelfReference()
    {
        // Act
        var result = await _validationService.ValidateCircularReference("exercise-1", "exercise-1", ExerciseLinkType.Warmup);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("An exercise cannot be linked to itself", result.ErrorMessage);
        Assert.Equal("SELF_REFERENCE", result.ErrorCode);
    }

    [Fact]
    public async Task ValidateCircularReference_ReturnsFailure_WhenCircularReferenceExists()
    {
        // Arrange
        var targetLinks = new ExerciseLinksResponseDto
        {
            Links = new List<ExerciseLinkDto>
            {
                new() { Id = "link-1", TargetExerciseId = "exercise-1", LinkType = "Warmup" }
            }
        };

        _mockLinkService.Setup(x => x.GetLinksAsync("exercise-2", It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(targetLinks);

        // Act
        var result = await _validationService.ValidateCircularReference("exercise-1", "exercise-2", ExerciseLinkType.Warmup);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("circular reference", result.ErrorMessage);
        Assert.Equal("CIRCULAR_REFERENCE", result.ErrorCode);
    }

    [Fact]
    public async Task ValidateCircularReference_ReturnsSuccess_WhenNoCircularReference()
    {
        // Arrange
        var targetLinks = new ExerciseLinksResponseDto
        {
            Links = new List<ExerciseLinkDto>
            {
                new() { Id = "link-1", TargetExerciseId = "exercise-3", LinkType = "Warmup" }
            }
        };

        _mockLinkService.Setup(x => x.GetLinksAsync("exercise-2", It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(targetLinks);

        // Act
        var result = await _validationService.ValidateCircularReference("exercise-1", "exercise-2", ExerciseLinkType.Warmup);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateCircularReference_ReturnsSuccess_WhenApiCallFails()
    {
        // Arrange
        _mockLinkService.Setup(x => x.GetLinksAsync("exercise-2", It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ThrowsAsync(new Exception("API Error"));

        // Act
        var result = await _validationService.ValidateCircularReference("exercise-1", "exercise-2", ExerciseLinkType.Warmup);

        // Assert
        Assert.True(result.IsValid); // Allow operation when we can't check
    }

    #endregion


    #region ValidateDuplicateLink Tests

    [Fact]
    public void ValidateDuplicateLink_ReturnsSuccess_WhenExistingLinksIsNull()
    {
        // Act
        var result = _validationService.ValidateDuplicateLink(null!, "exercise-2", ExerciseLinkType.Warmup);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateDuplicateLink_ReturnsSuccess_WhenNoDuplicateExists()
    {
        // Arrange
        var existingLinks = new List<ExerciseLinkDto>
        {
            new() { TargetExerciseId = "exercise-3", LinkType = "Warmup", IsActive = true },
            new() { TargetExerciseId = "exercise-4", LinkType = "Cooldown", IsActive = true }
        };

        // Act
        var result = _validationService.ValidateDuplicateLink(existingLinks, "exercise-2", ExerciseLinkType.Warmup);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateDuplicateLink_ReturnsFailure_WhenDuplicateWarmupExists()
    {
        // Arrange
        var existingLinks = new List<ExerciseLinkDto>
        {
            new() { TargetExerciseId = "exercise-2", LinkType = "Warmup", IsActive = true }
        };

        // Act
        var result = _validationService.ValidateDuplicateLink(existingLinks, "exercise-2", ExerciseLinkType.Warmup);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("This exercise is already linked as a warmup exercise", result.ErrorMessage);
        Assert.Equal("DUPLICATE_LINK", result.ErrorCode);
    }

    [Fact]
    public void ValidateDuplicateLink_ReturnsFailure_WhenDuplicateCooldownExists()
    {
        // Arrange
        var existingLinks = new List<ExerciseLinkDto>
        {
            new() { TargetExerciseId = "exercise-2", LinkType = "Cooldown", IsActive = true }
        };

        // Act
        var result = _validationService.ValidateDuplicateLink(existingLinks, "exercise-2", ExerciseLinkType.Cooldown);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("This exercise is already linked as a cooldown exercise", result.ErrorMessage);
        Assert.Equal("DUPLICATE_LINK", result.ErrorCode);
    }

    [Fact]
    public void ValidateDuplicateLink_ReturnsSuccess_WhenDuplicateIsInactive()
    {
        // Arrange
        var existingLinks = new List<ExerciseLinkDto>
        {
            new() { TargetExerciseId = "exercise-2", LinkType = "Warmup", IsActive = false }
        };

        // Act
        var result = _validationService.ValidateDuplicateLink(existingLinks, "exercise-2", ExerciseLinkType.Warmup);

        // Assert
        Assert.True(result.IsValid);
    }

    #endregion

    #region ValidateCreateLink Tests

    [Fact]
    public async Task ValidateCreateLink_ReturnsFailure_WhenExerciseTypeInvalid()
    {
        // Arrange
        var exercise = new ExerciseDto
        {
            Id = "exercise-1",
            ExerciseTypes = new List<ExerciseTypeDto> { new() { Value = "Strength" } }
        };

        // Act
        var result = await _validationService.ValidateCreateLink(exercise, "exercise-2", ExerciseLinkType.Warmup, new List<ExerciseLinkDto>());

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("INVALID_EXERCISE_TYPE", result.ErrorCode);
    }


    [Fact]
    public async Task ValidateCreateLink_ReturnsFailure_WhenDuplicateExists()
    {
        // Arrange
        var exercise = new ExerciseDto
        {
            Id = "exercise-1",
            ExerciseTypes = new List<ExerciseTypeDto> { new() { Value = "Workout" } }
        };

        var existingLinks = new List<ExerciseLinkDto>
        {
            new() { TargetExerciseId = "exercise-2", LinkType = "Warmup", IsActive = true }
        };

        // Act
        var result = await _validationService.ValidateCreateLink(exercise, "exercise-2", ExerciseLinkType.Warmup, existingLinks);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("DUPLICATE_LINK", result.ErrorCode);
    }

    [Fact]
    public async Task ValidateCreateLink_ReturnsFailure_WhenCircularReference()
    {
        // Arrange
        var exercise = new ExerciseDto
        {
            Id = "exercise-1",
            ExerciseTypes = new List<ExerciseTypeDto> { new() { Value = "Workout" } }
        };

        var targetLinks = new ExerciseLinksResponseDto
        {
            Links = new List<ExerciseLinkDto>
            {
                new() { TargetExerciseId = "exercise-1", LinkType = "Warmup" }
            }
        };

        _mockLinkService.Setup(x => x.GetLinksAsync("exercise-2", It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(targetLinks);

        // Act
        var result = await _validationService.ValidateCreateLink(exercise, "exercise-2", ExerciseLinkType.Warmup, new List<ExerciseLinkDto>());

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("CIRCULAR_REFERENCE", result.ErrorCode);
    }

    [Fact]
    public async Task ValidateCreateLink_ReturnsSuccess_WhenAllValidationsPass()
    {
        // Arrange
        var exercise = new ExerciseDto
        {
            Id = "exercise-1",
            ExerciseTypes = new List<ExerciseTypeDto> { new() { Value = "Workout" } }
        };

        var existingLinks = new List<ExerciseLinkDto>
        {
            new() { TargetExerciseId = "exercise-3", LinkType = "Warmup", IsActive = true }
        };

        var targetLinks = new ExerciseLinksResponseDto
        {
            Links = new List<ExerciseLinkDto>
            {
                new() { TargetExerciseId = "exercise-4", LinkType = "Warmup" }
            }
        };

        _mockLinkService.Setup(x => x.GetLinksAsync("exercise-2", It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(targetLinks);

        // Act
        var result = await _validationService.ValidateCreateLink(exercise, "exercise-2", ExerciseLinkType.Warmup, existingLinks);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
        Assert.Null(result.ErrorCode);
    }

    #endregion

    #region CanAddLinkType Tests

    [Theory]
    [InlineData("Workout", "Warmup", true)]
    [InlineData("Workout", "Cooldown", true)]
    [InlineData("Workout", "Alternative", true)]
    [InlineData("workout", "warmup", true)] // Case insensitive
    [InlineData("WORKOUT", "COOLDOWN", true)] // Mixed case
    public void CanAddLinkType_ReturnsSuccess_WhenWorkoutContext(string context, string linkType, bool shouldBeValid)
    {
        // Act
        var result = _validationService.CanAddLinkType(context, linkType);

        // Assert
        Assert.True(result.IsValid == shouldBeValid);
        if (shouldBeValid)
        {
            Assert.Null(result.ErrorMessage);
            Assert.Null(result.ErrorCode);
        }
    }

    [Theory]
    [InlineData("Warmup", "Workout", true)]
    [InlineData("Warmup", "Alternative", true)]
    [InlineData("warmup", "workout", true)] // Case insensitive
    [InlineData("WARMUP", "ALTERNATIVE", true)] // Mixed case
    public void CanAddLinkType_ReturnsSuccess_WhenWarmupContextWithAllowedTypes(string context, string linkType, bool shouldBeValid)
    {
        // Act
        var result = _validationService.CanAddLinkType(context, linkType);

        // Assert
        Assert.True(result.IsValid == shouldBeValid);
        if (shouldBeValid)
        {
            Assert.Null(result.ErrorMessage);
            Assert.Null(result.ErrorCode);
        }
    }

    [Theory]
    [InlineData("Warmup", "Warmup")]
    [InlineData("Warmup", "Cooldown")]
    [InlineData("warmup", "cooldown")] // Case insensitive
    public void CanAddLinkType_ReturnsFailure_WhenWarmupContextWithRestrictedTypes(string context, string linkType)
    {
        // Act
        var result = _validationService.CanAddLinkType(context, linkType);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Warmup exercises can only link to Workout and Alternative exercises", result.ErrorMessage);
        Assert.Equal("WARMUP_LINK_RESTRICTION", result.ErrorCode);
    }

    [Theory]
    [InlineData("Cooldown", "Workout", true)]
    [InlineData("Cooldown", "Alternative", true)]
    [InlineData("cooldown", "workout", true)] // Case insensitive
    [InlineData("COOLDOWN", "ALTERNATIVE", true)] // Mixed case
    public void CanAddLinkType_ReturnsSuccess_WhenCooldownContextWithAllowedTypes(string context, string linkType, bool shouldBeValid)
    {
        // Act
        var result = _validationService.CanAddLinkType(context, linkType);

        // Assert
        Assert.True(result.IsValid == shouldBeValid);
        if (shouldBeValid)
        {
            Assert.Null(result.ErrorMessage);
            Assert.Null(result.ErrorCode);
        }
    }

    [Theory]
    [InlineData("Cooldown", "Warmup")]
    [InlineData("Cooldown", "Cooldown")]
    [InlineData("cooldown", "warmup")] // Case insensitive
    public void CanAddLinkType_ReturnsFailure_WhenCooldownContextWithRestrictedTypes(string context, string linkType)
    {
        // Act
        var result = _validationService.CanAddLinkType(context, linkType);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Cooldown exercises can only link to Workout and Alternative exercises", result.ErrorMessage);
        Assert.Equal("COOLDOWN_LINK_RESTRICTION", result.ErrorCode);
    }

    [Fact]
    public void CanAddLinkType_ReturnsFailure_WhenContextIsNull()
    {
        // Act
        var result = _validationService.CanAddLinkType(null!, "Warmup");

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Exercise context cannot be null or empty", result.ErrorMessage);
        Assert.Equal("INVALID_CONTEXT", result.ErrorCode);
    }

    [Fact]
    public void CanAddLinkType_ReturnsFailure_WhenContextIsEmpty()
    {
        // Act
        var result = _validationService.CanAddLinkType("", "Warmup");

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Exercise context cannot be null or empty", result.ErrorMessage);
        Assert.Equal("INVALID_CONTEXT", result.ErrorCode);
    }

    [Fact]
    public void CanAddLinkType_ReturnsFailure_WhenLinkTypeIsNull()
    {
        // Act
        var result = _validationService.CanAddLinkType("Workout", null!);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Link type cannot be null or empty", result.ErrorMessage);
        Assert.Equal("INVALID_LINK_TYPE", result.ErrorCode);
    }

    [Fact]
    public void CanAddLinkType_ReturnsFailure_WhenLinkTypeIsEmpty()
    {
        // Act
        var result = _validationService.CanAddLinkType("Workout", "");

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Link type cannot be null or empty", result.ErrorMessage);
        Assert.Equal("INVALID_LINK_TYPE", result.ErrorCode);
    }

    [Theory]
    [InlineData("UnknownContext")]
    [InlineData("InvalidType")]
    [InlineData("Rest")] // REST exercises shouldn't reach this point, but test unknown contexts
    public void CanAddLinkType_ReturnsFailure_WhenUnknownContext(string context)
    {
        // Act
        var result = _validationService.CanAddLinkType(context, "Warmup");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains($"Unknown exercise context '{context}'", result.ErrorMessage);
        Assert.Equal("UNKNOWN_CONTEXT", result.ErrorCode);
    }

    [Theory]
    [InlineData("Workout", "InvalidType")]
    [InlineData("Workout", "Rest")]
    [InlineData("Workout", "Strength")]
    public void CanAddLinkType_ReturnsFailure_WhenWorkoutContextWithInvalidTypes(string context, string linkType)
    {
        // Act
        var result = _validationService.CanAddLinkType(context, linkType);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains($"Invalid link type '{linkType}' for Workout exercises", result.ErrorMessage);
        Assert.Equal("WORKOUT_INVALID_LINK_TYPE", result.ErrorCode);
    }

    #endregion
}