using FluentAssertions;
using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.ReferenceTables.BodyPart;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.Services.Builders;
using GetFitterGetBigger.API.Tests.Services.Extensions;
using Moq.AutoMock;

namespace GetFitterGetBigger.API.Tests.Services;

/// <summary>
/// Unit tests for BodyPartService using the modern DataService architecture
/// Tests focus on service layer behavior, validation, and caching integration
/// </summary>
public class BodyPartServiceTests
{
    [Fact]
    public async Task ExistsAsync_WithValidId_WhenBodyPartExists_ReturnsTrue()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<BodyPartService>();

        var bodyPartId = BodyPartId.New();
        var bodyPartDto = new BodyPartDtoBuilder()
            .WithId(bodyPartId)
            .Build();

        automocker
            .SetupBodyPartCacheMiss()
            .SetupBodyPartDataServiceGetById(bodyPartId, bodyPartDto);

        // Act
        var result = await testee.ExistsAsync(bodyPartId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeTrue();

        automocker
            .VerifyBodyPartCacheGetOnce()
            .VerifyBodyPartDataServiceGetByIdOnce(bodyPartId);
    }

    [Fact]
    public async Task ExistsAsync_WithValidId_WhenBodyPartDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<BodyPartService>();

        var bodyPartId = BodyPartId.New();

        automocker
            .SetupBodyPartCacheMiss()
            .SetupBodyPartDataServiceGetByIdNotFound(bodyPartId);

        // Act
        var result = await testee.ExistsAsync(bodyPartId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeFalse();

        automocker
            .VerifyBodyPartCacheGetOnce()
            .VerifyBodyPartDataServiceGetByIdOnce(bodyPartId);
    }

    [Fact]
    public async Task ExistsAsync_WithEmptyId_ReturnsValidationFailure()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<BodyPartService>();

        var emptyBodyPartId = BodyPartId.Empty;

        // Act
        var result = await testee.ExistsAsync(emptyBodyPartId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(BodyPartErrorMessages.InvalidIdFormat);

        // Verify DataService was not called for validation failures
        automocker.VerifyBodyPartDataServiceGetByIdNeverCalled();
    }

    [Fact]
    public async Task GetAllActiveAsync_ReturnsSuccessWithBodyParts()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<BodyPartService>();

        var bodyPartDtos = new List<BodyPartDto>
        {
            BodyPartDtoBuilder.ForChest().Build(),
            BodyPartDtoBuilder.ForBack().Build()
        };

        automocker
            .SetupBodyPartCacheMissList()
            .SetupBodyPartDataServiceGetAllActive(bodyPartDtos);

        // Act
        var result = await testee.GetAllActiveAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Count().Should().Be(2);
        result.Errors.Should().BeEmpty();

        automocker
            .VerifyBodyPartCacheGetListOnce()
            .VerifyBodyPartDataServiceGetAllActiveOnce();
    }

    [Fact]
    public async Task GetByIdAsync_WithValidStringId_ReturnsSuccessWithBodyPart()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<BodyPartService>();

        var bodyPartId = BodyPartId.New();
        var bodyPartIdString = bodyPartId.ToString();
        var bodyPartDto = BodyPartDtoBuilder.ForChest()
            .WithId(bodyPartId)
            .Build();

        automocker
            .SetupBodyPartCacheMiss()
            .SetupBodyPartDataServiceGetById(bodyPartId, bodyPartDto);

        // Act
        var result = await testee.GetByIdAsync(bodyPartIdString);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(bodyPartIdString);
        result.Data.Value.Should().Be("Chest");
        result.Errors.Should().BeEmpty();

        automocker
            .VerifyBodyPartCacheGetOnce()
            .VerifyBodyPartDataServiceGetByIdOnce(bodyPartId);
    }

    [Fact]
    public async Task GetByIdAsync_WithBodyPartId_ReturnsSuccessWithBodyPart()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<BodyPartService>();

        var bodyPartId = BodyPartId.New();
        var bodyPartDto = BodyPartDtoBuilder.ForBack()
            .WithId(bodyPartId)
            .Build();

        automocker
            .SetupBodyPartCacheMiss()
            .SetupBodyPartDataServiceGetById(bodyPartId, bodyPartDto);

        // Act
        var result = await testee.GetByIdAsync(bodyPartId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(bodyPartId.ToString());
        result.Data.Value.Should().Be("Back");
        result.Errors.Should().BeEmpty();

        automocker
            .VerifyBodyPartCacheGetOnce()
            .VerifyBodyPartDataServiceGetByIdOnce(bodyPartId);
    }

    [Fact]
    public async Task GetByIdAsync_WithEmptyBodyPartId_ReturnsValidationFailure()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<BodyPartService>();

        var emptyBodyPartId = BodyPartId.Empty;

        // Act
        var result = await testee.GetByIdAsync(emptyBodyPartId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().NotBeNull();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(BodyPartErrorMessages.InvalidIdFormat);

        // Verify DataService was not called for validation failures
        automocker.VerifyBodyPartDataServiceGetByIdNeverCalled();
    }

    [Fact]
    public async Task GetByIdAsync_WithEmptyString_ReturnsValidationFailure()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<BodyPartService>();

        const string emptyId = "";

        // Act
        var result = await testee.GetByIdAsync(emptyId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().NotBeNull();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(BodyPartErrorMessages.InvalidIdFormat);

        automocker.VerifyBodyPartDataServiceGetByIdNeverCalled();
    }

    [Fact]
    public async Task GetByIdAsync_WithNullString_ReturnsValidationFailure()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<BodyPartService>();

        string? nullId = null;

        // Act
        var result = await testee.GetByIdAsync(nullId!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().NotBeNull();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(BodyPartErrorMessages.InvalidIdFormat);

        automocker.VerifyBodyPartDataServiceGetByIdNeverCalled();
    }

    [Fact]
    public async Task GetByIdAsync_WhenBodyPartNotFound_ReturnsNotFound()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<BodyPartService>();

        var bodyPartId = BodyPartId.New();

        automocker
            .SetupBodyPartCacheMiss()
            .SetupBodyPartDataServiceGetByIdNotFound(bodyPartId);

        // Act
        var result = await testee.GetByIdAsync(bodyPartId.ToString());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().NotBeNull();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.NotFound);
        result.Errors.Should().NotBeEmpty();

        automocker
            .VerifyBodyPartCacheGetOnce()
            .VerifyBodyPartDataServiceGetByIdOnce(bodyPartId);
    }

    [Fact]
    public async Task GetByValueAsync_WithExistingValue_ReturnsSuccess()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<BodyPartService>();

        const string value = "Chest";
        var bodyPartDto = BodyPartDtoBuilder.ForChest()
            .WithValue(value)
            .Build();

        automocker
            .SetupBodyPartCacheMiss()
            .SetupBodyPartDataServiceGetByValue(value, bodyPartDto);

        // Act
        var result = await testee.GetByValueAsync(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Value.Should().Be(value);
        result.Errors.Should().BeEmpty();

        automocker
            .VerifyBodyPartCacheGetOnce()
            .VerifyBodyPartDataServiceGetByValueOnce(value);
    }

    [Fact]
    public async Task GetByValueAsync_WithNonExistingValue_ReturnsNotFound()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<BodyPartService>();

        const string value = "NonExistent";

        automocker
            .SetupBodyPartCacheMiss()
            .SetupBodyPartDataServiceGetByValueNotFound(value);

        // Act
        var result = await testee.GetByValueAsync(value);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().NotBeNull();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.NotFound);
        result.Errors.Should().NotBeEmpty();

        automocker
            .VerifyBodyPartCacheGetOnce()
            .VerifyBodyPartDataServiceGetByValueOnce(value);
    }

    [Fact]
    public async Task GetByValueAsync_WithEmptyValue_ReturnsValidationFailure()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<BodyPartService>();

        const string emptyValue = "";

        // Act
        var result = await testee.GetByValueAsync(emptyValue);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().NotBeNull();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(BodyPartErrorMessages.ValueCannotBeEmpty);

        // Verify DataService was not called for validation failures
        automocker.VerifyBodyPartDataServiceGetByValueNeverCalled();
    }

    [Fact]
    public async Task GetByValueAsync_WithWhitespaceValue_ReturnsValidationFailure()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<BodyPartService>();

        const string whitespaceValue = "   ";

        // Act
        var result = await testee.GetByValueAsync(whitespaceValue);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().NotBeNull();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(BodyPartErrorMessages.ValueCannotBeEmpty);

        // Verify DataService was not called for validation failures
        automocker.VerifyBodyPartDataServiceGetByValueNeverCalled();
    }
}