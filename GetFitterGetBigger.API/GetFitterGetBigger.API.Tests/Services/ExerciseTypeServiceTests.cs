using FluentAssertions;
using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.ReferenceTables.ExerciseType;
using GetFitterGetBigger.API.Services.ReferenceTables.ExerciseType.DataServices;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.Services.Builders;
using GetFitterGetBigger.API.Tests.Services.Extensions;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services;

/// <summary>
/// Unit tests for ExerciseTypeService using the modern DataService architecture
/// Tests focus on service layer behavior, validation, and caching integration
/// </summary>
public class ExerciseTypeServiceTests
{
    [Fact]
    public async Task ExistsAsync_WithValidId_WhenExerciseTypeExists_ReturnsTrue()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<ExerciseTypeService>();

        var exerciseTypeId = ExerciseTypeId.New();
        var exerciseTypeDto = new ExerciseTypeDto
        {
            Id = exerciseTypeId.ToString(),
            Value = "Strength",
            Description = "Strength training exercise"
        };

        automocker
            .SetupExerciseTypeCacheMiss()
            .SetupExerciseTypeDataServiceGetById(exerciseTypeId, exerciseTypeDto);

        // Act
        var result = await testee.ExistsAsync(exerciseTypeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithValidId_WhenExerciseTypeDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<ExerciseTypeService>();

        var exerciseTypeId = ExerciseTypeId.New();

        automocker
            .SetupExerciseTypeCacheMiss()
            .SetupExerciseTypeDataServiceGetByIdNotFound(exerciseTypeId);

        // Act
        var result = await testee.ExistsAsync(exerciseTypeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WithEmptyId_ReturnsValidationFailure()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<ExerciseTypeService>();

        var emptyId = ExerciseTypeId.Empty;

        // Act
        var result = await testee.ExistsAsync(emptyId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(ExerciseTypeErrorMessages.InvalidIdFormat);

        automocker.GetMock<IExerciseTypeDataService>().Verify(x => x.GetByIdAsync(It.IsAny<ExerciseTypeId>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_WhenCacheMiss_LoadsFromDataService()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<ExerciseTypeService>();

        var exerciseTypeId = ExerciseTypeId.New();
        var exerciseTypeDto = new ExerciseTypeDto
        {
            Id = exerciseTypeId.ToString(),
            Value = "Cardio",
            Description = "Cardiovascular exercise"
        };

        automocker
            .SetupExerciseTypeCacheMiss()
            .SetupExerciseTypeDataServiceGetById(exerciseTypeId, exerciseTypeDto);

        // Act
        var result = await testee.GetByIdAsync(exerciseTypeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().Be("Cardio");
        result.Data.Description.Should().Be("Cardiovascular exercise");
    }

    [Fact]
    public async Task GetByIdAsync_WithValidStringId_LoadsFromDataService()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<ExerciseTypeService>();

        var exerciseTypeId = ExerciseTypeId.New();
        var exerciseTypeIdString = exerciseTypeId.ToString();
        var exerciseTypeDto = new ExerciseTypeDto
        {
            Id = exerciseTypeId.ToString(),
            Value = "Rest",
            Description = "Rest period"
        };

        automocker
            .SetupExerciseTypeCacheMiss()
            .SetupExerciseTypeDataServiceGetById(exerciseTypeId, exerciseTypeDto);

        // Act
        var result = await testee.GetByIdAsync(exerciseTypeIdString);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().Be("Rest");
        result.Data.Id.Should().Be(exerciseTypeIdString);
    }

    [Fact]
    public async Task GetByIdAsync_WithEmptyId_ReturnsValidationFailure()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<ExerciseTypeService>();

        var emptyId = ExerciseTypeId.Empty;

        // Act
        var result = await testee.GetByIdAsync(emptyId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(ExerciseTypeErrorMessages.InvalidIdFormat);

        automocker.GetMock<IExerciseTypeDataService>().Verify(x => x.GetByIdAsync(It.IsAny<ExerciseTypeId>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<ExerciseTypeService>();

        var exerciseTypeId = ExerciseTypeId.New();

        automocker
            .SetupExerciseTypeCacheMiss()
            .SetupExerciseTypeDataServiceGetByIdNotFound(exerciseTypeId);

        // Act
        var result = await testee.GetByIdAsync(exerciseTypeId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.NotFound);
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByValueAsync_WithEmptyValue_ReturnsValidationFailure()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<ExerciseTypeService>();

        const string emptyValue = "";

        // Act
        var result = await testee.GetByValueAsync(emptyValue);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(ExerciseTypeErrorMessages.ValueCannotBeEmpty);
    }

    [Fact]
    public async Task GetAllActiveAsync_WhenCacheMiss_LoadsFromDataService()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<ExerciseTypeService>();

        var exerciseTypes = new List<ExerciseTypeDto>
        {
            new ExerciseTypeDto { Id = ExerciseTypeId.New().ToString(), Value = "Strength", Description = "Strength training exercise" },
            new ExerciseTypeDto { Id = ExerciseTypeId.New().ToString(), Value = "Cardio", Description = "Cardiovascular exercise" },
            new ExerciseTypeDto { Id = ExerciseTypeId.New().ToString(), Value = "Rest", Description = "Rest period" }
        };

        automocker
            .SetupExerciseTypeCacheMiss()
            .SetupExerciseTypeDataServiceGetAllActive(exerciseTypes);

        // Act
        var result = await testee.GetAllActiveAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        var items = result.Data.ToList();
        items.Should().HaveCount(3);
        
        var values = items.Select(d => d.Value);
        values.Should().Contain("Strength");
        values.Should().Contain("Cardio");
        values.Should().Contain("Rest");
    }

    [Fact]
    public async Task GetByIdAsync_WithNullString_ReturnsValidationFailure()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<ExerciseTypeService>();

        string? nullId = null;

        // Act
        var result = await testee.GetByIdAsync(nullId!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(ExerciseTypeErrorMessages.InvalidIdFormat);

        automocker.GetMock<IExerciseTypeDataService>().Verify(x => x.GetByIdAsync(It.IsAny<ExerciseTypeId>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WithEmptyString_ReturnsValidationFailure()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<ExerciseTypeService>();

        const string emptyId = "";

        // Act
        var result = await testee.GetByIdAsync(emptyId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(ExerciseTypeErrorMessages.InvalidIdFormat);

        automocker.GetMock<IExerciseTypeDataService>().Verify(x => x.GetByIdAsync(It.IsAny<ExerciseTypeId>()), Times.Never);
    }
}