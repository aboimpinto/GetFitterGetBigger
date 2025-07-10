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
                new() { Id = "type-1", Value = "Warmup" },
                new() { Id = "type-2", Value = "Cooldown" }
            }
        };

        // Act
        var result = _validationService.ValidateExerciseTypeCompatibility(exercise);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Only exercises of type 'Workout' can have links", result.ErrorMessage);
        Assert.Contains("Warmup, Cooldown", result.ErrorMessage);
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

        _mockLinkService.Setup(x => x.GetLinksAsync("exercise-2", null, false))
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

        _mockLinkService.Setup(x => x.GetLinksAsync("exercise-2", null, false))
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
        _mockLinkService.Setup(x => x.GetLinksAsync("exercise-2", null, false))
            .ThrowsAsync(new Exception("API Error"));

        // Act
        var result = await _validationService.ValidateCircularReference("exercise-1", "exercise-2", ExerciseLinkType.Warmup);

        // Assert
        Assert.True(result.IsValid); // Allow operation when we can't check
    }

    #endregion

    #region ValidateMaximumLinks Tests

    [Fact]
    public void ValidateMaximumLinks_ReturnsFailure_WhenAtMaximumWarmupLinks()
    {
        // Act
        var result = _validationService.ValidateMaximumLinks(10, ExerciseLinkType.Warmup);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Maximum number of warmup links (10) has been reached", result.ErrorMessage);
        Assert.Equal("MAX_LINKS_REACHED", result.ErrorCode);
    }

    [Fact]
    public void ValidateMaximumLinks_ReturnsFailure_WhenAtMaximumCooldownLinks()
    {
        // Act
        var result = _validationService.ValidateMaximumLinks(10, ExerciseLinkType.Cooldown);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Maximum number of cooldown links (10) has been reached", result.ErrorMessage);
        Assert.Equal("MAX_LINKS_REACHED", result.ErrorCode);
    }

    [Fact]
    public void ValidateMaximumLinks_ReturnsSuccess_WhenBelowMaximum()
    {
        // Act
        var result = _validationService.ValidateMaximumLinks(9, ExerciseLinkType.Warmup);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateMaximumLinks_ReturnsFailure_WhenAboveMaximum()
    {
        // Act
        var result = _validationService.ValidateMaximumLinks(11, ExerciseLinkType.Warmup);

        // Assert
        Assert.False(result.IsValid);
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
            ExerciseTypes = new List<ExerciseTypeDto> { new() { Value = "Cooldown" } }
        };

        // Act
        var result = await _validationService.ValidateCreateLink(exercise, "exercise-2", ExerciseLinkType.Warmup, new List<ExerciseLinkDto>());

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("INVALID_EXERCISE_TYPE", result.ErrorCode);
    }

    [Fact]
    public async Task ValidateCreateLink_ReturnsFailure_WhenMaxLinksReached()
    {
        // Arrange
        var exercise = new ExerciseDto
        {
            Id = "exercise-1",
            ExerciseTypes = new List<ExerciseTypeDto> { new() { Value = "Workout" } }
        };

        var existingLinks = Enumerable.Range(1, 10).Select(i => new ExerciseLinkDto
        {
            TargetExerciseId = $"exercise-{i}",
            LinkType = "Warmup",
            IsActive = true
        }).ToList();

        // Act
        var result = await _validationService.ValidateCreateLink(exercise, "exercise-11", ExerciseLinkType.Warmup, existingLinks);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("MAX_LINKS_REACHED", result.ErrorCode);
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

        _mockLinkService.Setup(x => x.GetLinksAsync("exercise-2", null, false))
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

        _mockLinkService.Setup(x => x.GetLinksAsync("exercise-2", null, false))
            .ReturnsAsync(targetLinks);

        // Act
        var result = await _validationService.ValidateCreateLink(exercise, "exercise-2", ExerciseLinkType.Warmup, existingLinks);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
        Assert.Null(result.ErrorCode);
    }

    #endregion
}