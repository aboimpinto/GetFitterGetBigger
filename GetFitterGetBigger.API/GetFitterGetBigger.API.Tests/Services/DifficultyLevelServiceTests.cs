using FluentAssertions;
using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.ReferenceTables.DifficultyLevel;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.Services.Builders;
using GetFitterGetBigger.API.Tests.Services.Extensions;
using Moq.AutoMock;

namespace GetFitterGetBigger.API.Tests.Services;

/// <summary>
/// Unit tests for DifficultyLevelService using the modern DataService architecture
/// Tests focus on service layer behavior, validation, and caching integration
/// </summary>
public class DifficultyLevelServiceTests
{
    [Fact]
    public async Task ExistsAsync_WithValidId_WhenDifficultyLevelExists_ReturnsTrue()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<DifficultyLevelService>();

        var difficultyLevelId = DifficultyLevelId.New();
        var difficultyLevelDto = ReferenceDataDtoBuilder.ForIntermediate()
            .WithId(difficultyLevelId)
            .Build();

        automocker
            .SetupReferenceDataCacheMiss()
            .SetupDifficultyLevelDataServiceGetById(difficultyLevelId, difficultyLevelDto);

        // Act
        var result = await testee.ExistsAsync(difficultyLevelId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeTrue();

        automocker
            .VerifyReferenceDataCacheGetOnce()
            .VerifyDifficultyLevelDataServiceGetByIdOnce(difficultyLevelId);
    }

    [Fact]
    public async Task ExistsAsync_WithValidId_WhenDifficultyLevelDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<DifficultyLevelService>();

        var difficultyLevelId = DifficultyLevelId.New();

        automocker
            .SetupReferenceDataCacheMiss()
            .SetupDifficultyLevelDataServiceGetByIdNotFound(difficultyLevelId);

        // Act
        var result = await testee.ExistsAsync(difficultyLevelId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeFalse();

        automocker
            .VerifyReferenceDataCacheGetOnce()
            .VerifyDifficultyLevelDataServiceGetByIdOnce(difficultyLevelId);
    }

    [Fact]
    public async Task ExistsAsync_WithEmptyId_ReturnsValidationFailure()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<DifficultyLevelService>();

        var emptyId = DifficultyLevelId.Empty;

        // Act
        var result = await testee.ExistsAsync(emptyId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(DifficultyLevelErrorMessages.InvalidIdFormat);

        // Verify DataService was not called for validation failures
        automocker.VerifyDifficultyLevelDataServiceGetByIdNeverCalled();
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_WhenCacheMiss_LoadsFromDataService()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<DifficultyLevelService>();

        var difficultyLevelId = DifficultyLevelId.New();
        var difficultyLevelDto = ReferenceDataDtoBuilder.ForIntermediate()
            .WithId(difficultyLevelId)
            .Build();

        automocker
            .SetupReferenceDataCacheMiss()
            .SetupDifficultyLevelDataServiceGetById(difficultyLevelId, difficultyLevelDto);

        // Act
        var result = await testee.GetByIdAsync(difficultyLevelId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().Be("Intermediate");
        result.Data.Description.Should().Be("For intermediate users");

        automocker
            .VerifyReferenceDataCacheGetOnce()
            .VerifyDifficultyLevelDataServiceGetByIdOnce(difficultyLevelId)
            .VerifyReferenceDataCacheSetOnce();
    }

    [Fact]
    public async Task GetByIdAsync_WithValidStringId_LoadsFromDataService()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<DifficultyLevelService>();

        var difficultyLevelId = DifficultyLevelId.New();
        var difficultyLevelIdString = difficultyLevelId.ToString();
        var difficultyLevelDto = ReferenceDataDtoBuilder.ForAdvanced()
            .WithId(difficultyLevelId)
            .Build();

        automocker
            .SetupReferenceDataCacheMiss()
            .SetupDifficultyLevelDataServiceGetById(difficultyLevelId, difficultyLevelDto);

        // Act
        var result = await testee.GetByIdAsync(difficultyLevelIdString);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().Be("Advanced");
        result.Data.Id.Should().Be(difficultyLevelIdString);

        automocker
            .VerifyReferenceDataCacheGetOnce()
            .VerifyDifficultyLevelDataServiceGetByIdOnce(difficultyLevelId);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_WhenCacheHit_ReturnsFromCache()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<DifficultyLevelService>();

        var difficultyLevelId = DifficultyLevelId.New();
        var cachedDto = ReferenceDataDtoBuilder.ForAdvanced()
            .WithId(difficultyLevelId)
            .Build();

        automocker.SetupReferenceDataCacheHit(cachedDto);

        // Act
        var result = await testee.GetByIdAsync(difficultyLevelId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().Be("Advanced");

        // Verify cache was checked but DataService was NOT called
        automocker
            .VerifyReferenceDataCacheGetOnce()
            .VerifyDifficultyLevelDataServiceGetByIdNeverCalled();
    }

    [Fact]
    public async Task GetByIdAsync_WithEmptyId_ReturnsValidationFailure()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<DifficultyLevelService>();

        var emptyId = DifficultyLevelId.Empty;

        // Act
        var result = await testee.GetByIdAsync(emptyId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(DifficultyLevelErrorMessages.InvalidIdFormat);

        automocker.VerifyDifficultyLevelDataServiceGetByIdNeverCalled();
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<DifficultyLevelService>();

        var difficultyLevelId = DifficultyLevelId.New();

        automocker
            .SetupReferenceDataCacheMiss()
            .SetupDifficultyLevelDataServiceGetByIdNotFound(difficultyLevelId);

        // Act
        var result = await testee.GetByIdAsync(difficultyLevelId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.NotFound);
        result.Data.Should().NotBeNull();

        automocker
            .VerifyReferenceDataCacheGetOnce()
            .VerifyDifficultyLevelDataServiceGetByIdOnce(difficultyLevelId);
    }

    [Fact]
    public async Task GetByValueAsync_WithEmptyValue_ReturnsValidationFailure()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<DifficultyLevelService>();

        const string emptyValue = "";

        // Act
        var result = await testee.GetByValueAsync(emptyValue);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(DifficultyLevelErrorMessages.ValueCannotBeEmpty);

        automocker.VerifyDifficultyLevelDataServiceGetByValueNeverCalled();
    }

    [Fact]
    public async Task GetByValueAsync_WithValidValue_ReturnsSuccess()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<DifficultyLevelService>();

        const string value = "Beginner";
        var difficultyLevelDto = ReferenceDataDtoBuilder.ForBeginner()
            .WithValue(value)
            .Build();

        automocker
            .SetupReferenceDataCacheMiss()
            .SetupDifficultyLevelDataServiceGetByValue(value, difficultyLevelDto);

        // Act
        var result = await testee.GetByValueAsync(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().Be(value);

        automocker
            .VerifyReferenceDataCacheGetOnce()
            .VerifyDifficultyLevelDataServiceGetByValueOnce(value);
    }

    [Fact]
    public async Task GetByValueAsync_WithNonExistentValue_ReturnsNotFound()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<DifficultyLevelService>();

        const string value = "NonExistent";

        automocker
            .SetupReferenceDataCacheMiss()
            .SetupDifficultyLevelDataServiceGetByValueNotFound(value);

        // Act
        var result = await testee.GetByValueAsync(value);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.NotFound);

        automocker
            .VerifyReferenceDataCacheGetOnce()
            .VerifyDifficultyLevelDataServiceGetByValueOnce(value);
    }

    [Fact]
    public async Task GetAllActiveAsync_WhenCacheMiss_LoadsFromDataService()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<DifficultyLevelService>();

        var difficultyLevels = new List<ReferenceDataDto>
        {
            ReferenceDataDtoBuilder.ForBeginner().Build(),
            ReferenceDataDtoBuilder.ForIntermediate().Build(),
            ReferenceDataDtoBuilder.ForAdvanced().Build()
        };

        automocker
            .SetupReferenceDataCacheMissList()
            .SetupDifficultyLevelDataServiceGetAllActive(difficultyLevels);

        // Act
        var result = await testee.GetAllActiveAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        var items = result.Data.ToList();
        items.Should().HaveCount(3);
        
        var values = items.Select(d => d.Value);
        values.Should().Contain("Beginner");
        values.Should().Contain("Intermediate");
        values.Should().Contain("Advanced");

        automocker
            .VerifyReferenceDataCacheGetListOnce()
            .VerifyDifficultyLevelDataServiceGetAllActiveOnce()
            .VerifyReferenceDataCacheSetListOnce();
    }

    [Fact]
    public async Task GetAllActiveAsync_WhenCacheHit_ReturnsFromCache()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<DifficultyLevelService>();

        var cachedLevels = new List<ReferenceDataDto>
        {
            ReferenceDataDtoBuilder.ForBeginner().Build(),
            ReferenceDataDtoBuilder.ForIntermediate().Build()
        };

        automocker.SetupReferenceDataCacheHitList(cachedLevels);

        // Act
        var result = await testee.GetAllActiveAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Count().Should().Be(2);

        // Verify cache was checked but DataService was NOT called
        automocker
            .VerifyReferenceDataCacheGetListOnce()
            .VerifyDifficultyLevelDataServiceGetAllActiveNeverCalled();
    }

    [Fact]
    public async Task GetAllActiveAsync_OnlyReturnsActiveOnes()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<DifficultyLevelService>();

        // DataService should only return active items (this is ensured at DataService level)
        var activeLevels = new List<ReferenceDataDto>
        {
            ReferenceDataDtoBuilder.ForBeginner().Build(),
            ReferenceDataDtoBuilder.ForIntermediate().Build()
        };

        automocker
            .SetupReferenceDataCacheMissList()
            .SetupDifficultyLevelDataServiceGetAllActive(activeLevels);

        // Act
        var result = await testee.GetAllActiveAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Count().Should().Be(2);

        automocker.VerifyDifficultyLevelDataServiceGetAllActiveOnce();
    }

    [Fact]
    public async Task GetByIdAsync_WithNullString_ReturnsValidationFailure()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<DifficultyLevelService>();

        string? nullId = null;

        // Act
        var result = await testee.GetByIdAsync(nullId!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(DifficultyLevelErrorMessages.InvalidIdFormat);

        automocker.VerifyDifficultyLevelDataServiceGetByIdNeverCalled();
    }

    [Fact]
    public async Task GetByIdAsync_WithEmptyString_ReturnsValidationFailure()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<DifficultyLevelService>();

        const string emptyId = "";

        // Act
        var result = await testee.GetByIdAsync(emptyId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(DifficultyLevelErrorMessages.InvalidIdFormat);

        automocker.VerifyDifficultyLevelDataServiceGetByIdNeverCalled();
    }
}