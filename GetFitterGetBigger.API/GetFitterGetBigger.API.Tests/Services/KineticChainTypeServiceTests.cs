using FluentAssertions;
using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.ReferenceTables.KineticChainType;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.Services.Builders;
using GetFitterGetBigger.API.Tests.Services.Extensions;
using Moq.AutoMock;

namespace GetFitterGetBigger.API.Tests.Services;

/// <summary>
/// Unit tests for KineticChainTypeService using the modern DataService architecture
/// Tests focus on service layer behavior, validation, and caching integration
/// </summary>
public class KineticChainTypeServiceTests
{
    [Fact]
    public async Task ExistsAsync_WithValidId_WhenKineticChainTypeExists_ReturnsTrue()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<KineticChainTypeService>();

        var kineticChainTypeId = KineticChainTypeId.New();
        var kineticChainTypeDto = ReferenceDataDtoBuilder.ForOpenChain()
            .WithId(kineticChainTypeId)
            .Build();

        automocker
            .SetupReferenceDataCacheMiss()
            .SetupKineticChainTypeDataServiceGetById(kineticChainTypeId, kineticChainTypeDto);

        // Act
        var result = await testee.ExistsAsync(kineticChainTypeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithValidId_WhenKineticChainTypeDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<KineticChainTypeService>();

        var kineticChainTypeId = KineticChainTypeId.New();

        automocker
            .SetupReferenceDataCacheMiss()
            .SetupKineticChainTypeDataServiceGetByIdNotFound(kineticChainTypeId);

        // Act
        var result = await testee.ExistsAsync(kineticChainTypeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WithEmptyId_ReturnsValidationFailure()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<KineticChainTypeService>();

        var emptyId = KineticChainTypeId.Empty;

        // Act
        var result = await testee.ExistsAsync(emptyId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(KineticChainTypeErrorMessages.InvalidIdFormat);

        automocker.VerifyKineticChainTypeDataServiceGetByIdNeverCalled();
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_WhenCacheMiss_LoadsFromDataService()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<KineticChainTypeService>();

        var kineticChainTypeId = KineticChainTypeId.New();
        var kineticChainTypeDto = ReferenceDataDtoBuilder.ForClosedChain()
            .WithId(kineticChainTypeId)
            .Build();

        automocker
            .SetupReferenceDataCacheMiss()
            .SetupKineticChainTypeDataServiceGetById(kineticChainTypeId, kineticChainTypeDto);

        // Act
        var result = await testee.GetByIdAsync(kineticChainTypeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().Be("Closed Chain");
        result.Data.Description.Should().Be("Closed kinetic chain movement");
    }

    [Fact]
    public async Task GetByIdAsync_WithValidStringId_LoadsFromDataService()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<KineticChainTypeService>();

        var kineticChainTypeId = KineticChainTypeId.New();
        var kineticChainTypeIdString = kineticChainTypeId.ToString();
        var kineticChainTypeDto = ReferenceDataDtoBuilder.ForOpenChain()
            .WithId(kineticChainTypeId)
            .Build();

        automocker
            .SetupReferenceDataCacheMiss()
            .SetupKineticChainTypeDataServiceGetById(kineticChainTypeId, kineticChainTypeDto);

        // Act
        var result = await testee.GetByIdAsync(kineticChainTypeIdString);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().Be("Open Chain");
        result.Data.Id.Should().Be(kineticChainTypeIdString);
    }

    [Fact]
    public async Task GetByIdAsync_WithEmptyId_ReturnsValidationFailure()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<KineticChainTypeService>();

        var emptyId = KineticChainTypeId.Empty;

        // Act
        var result = await testee.GetByIdAsync(emptyId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(KineticChainTypeErrorMessages.InvalidIdFormat);

        automocker.VerifyKineticChainTypeDataServiceGetByIdNeverCalled();
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<KineticChainTypeService>();

        var kineticChainTypeId = KineticChainTypeId.New();

        automocker
            .SetupReferenceDataCacheMiss()
            .SetupKineticChainTypeDataServiceGetByIdNotFound(kineticChainTypeId);

        // Act
        var result = await testee.GetByIdAsync(kineticChainTypeId);

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
        var testee = automocker.CreateInstance<KineticChainTypeService>();

        const string emptyValue = "";

        // Act
        var result = await testee.GetByValueAsync(emptyValue);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(KineticChainTypeErrorMessages.ValueCannotBeEmpty);
    }

    [Fact]
    public async Task GetAllActiveAsync_WhenCacheMiss_LoadsFromDataService()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<KineticChainTypeService>();

        var kineticChainTypes = new List<ReferenceDataDto>
        {
            ReferenceDataDtoBuilder.ForOpenChain().Build(),
            ReferenceDataDtoBuilder.ForClosedChain().Build()
        };

        automocker
            .SetupReferenceDataCacheMissList()
            .SetupKineticChainTypeDataServiceGetAllActive(kineticChainTypes);

        // Act
        var result = await testee.GetAllActiveAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        var items = result.Data.ToList();
        items.Should().HaveCount(2);
        
        var values = items.Select(d => d.Value);
        values.Should().Contain("Open Chain");
        values.Should().Contain("Closed Chain");
    }

    [Fact]
    public async Task GetByIdAsync_WithNullString_ReturnsValidationFailure()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<KineticChainTypeService>();

        string? nullId = null;

        // Act
        var result = await testee.GetByIdAsync(nullId!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(KineticChainTypeErrorMessages.InvalidIdFormat);

        automocker.VerifyKineticChainTypeDataServiceGetByIdNeverCalled();
    }

    [Fact]
    public async Task GetByIdAsync_WithEmptyString_ReturnsValidationFailure()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<KineticChainTypeService>();

        const string emptyId = "";

        // Act
        var result = await testee.GetByIdAsync(emptyId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(KineticChainTypeErrorMessages.InvalidIdFormat);

        automocker.VerifyKineticChainTypeDataServiceGetByIdNeverCalled();
    }
}