using FluentAssertions;
using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.ReferenceTables.MuscleRole;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.Services.Builders;
using GetFitterGetBigger.API.Tests.Services.Extensions;
using Moq.AutoMock;

namespace GetFitterGetBigger.API.Tests.Services;

/// <summary>
/// Unit tests for MuscleRoleService using the modern DataService architecture
/// Tests focus on service layer behavior, validation, and caching integration
/// </summary>
public class MuscleRoleServiceTests
{
    [Fact]
    public async Task ExistsAsync_WithValidId_WhenMuscleRoleExists_ReturnsTrue()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<MuscleRoleService>();

        var muscleRoleId = MuscleRoleId.New();
        var muscleRoleDto = ReferenceDataDtoBuilder.ForAgonist()
            .WithId(muscleRoleId)
            .Build();

        automocker
            .SetupReferenceDataCacheMiss()
            .SetupMuscleRoleDataServiceGetById(muscleRoleId, muscleRoleDto);

        // Act
        var result = await testee.ExistsAsync(muscleRoleId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithValidId_WhenMuscleRoleDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<MuscleRoleService>();

        var muscleRoleId = MuscleRoleId.New();

        automocker
            .SetupReferenceDataCacheMiss()
            .SetupMuscleRoleDataServiceGetByIdNotFound(muscleRoleId);

        // Act
        var result = await testee.ExistsAsync(muscleRoleId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WithEmptyId_ReturnsValidationFailure()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<MuscleRoleService>();

        var emptyId = MuscleRoleId.Empty;

        // Act
        var result = await testee.ExistsAsync(emptyId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(MuscleRoleErrorMessages.InvalidIdFormat);

        automocker.VerifyMuscleRoleDataServiceGetByIdNeverCalled();
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_WhenCacheMiss_LoadsFromDataService()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<MuscleRoleService>();

        var muscleRoleId = MuscleRoleId.New();
        var muscleRoleDto = ReferenceDataDtoBuilder.ForAntagonist()
            .WithId(muscleRoleId)
            .Build();

        automocker
            .SetupReferenceDataCacheMiss()
            .SetupMuscleRoleDataServiceGetById(muscleRoleId, muscleRoleDto);

        // Act
        var result = await testee.GetByIdAsync(muscleRoleId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().Be("Antagonist");
        result.Data.Description.Should().Be("Opposing muscle");
    }

    [Fact]
    public async Task GetByIdAsync_WithValidStringId_LoadsFromDataService()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<MuscleRoleService>();

        var muscleRoleId = MuscleRoleId.New();
        var muscleRoleIdString = muscleRoleId.ToString();
        var muscleRoleDto = ReferenceDataDtoBuilder.ForSynergist()
            .WithId(muscleRoleId)
            .Build();

        automocker
            .SetupReferenceDataCacheMiss()
            .SetupMuscleRoleDataServiceGetById(muscleRoleId, muscleRoleDto);

        // Act
        var result = await testee.GetByIdAsync(muscleRoleIdString);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().Be("Synergist");
        result.Data.Id.Should().Be(muscleRoleIdString);
    }

    [Fact]
    public async Task GetByIdAsync_WithEmptyId_ReturnsValidationFailure()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<MuscleRoleService>();

        var emptyId = MuscleRoleId.Empty;

        // Act
        var result = await testee.GetByIdAsync(emptyId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(MuscleRoleErrorMessages.InvalidIdFormat);

        automocker.VerifyMuscleRoleDataServiceGetByIdNeverCalled();
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<MuscleRoleService>();

        var muscleRoleId = MuscleRoleId.New();

        automocker
            .SetupReferenceDataCacheMiss()
            .SetupMuscleRoleDataServiceGetByIdNotFound(muscleRoleId);

        // Act
        var result = await testee.GetByIdAsync(muscleRoleId);

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
        var testee = automocker.CreateInstance<MuscleRoleService>();

        const string emptyValue = "";

        // Act
        var result = await testee.GetByValueAsync(emptyValue);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(MuscleRoleErrorMessages.ValueCannotBeEmpty);
    }

    [Fact]
    public async Task GetAllActiveAsync_WhenCacheMiss_LoadsFromDataService()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<MuscleRoleService>();

        var muscleRoles = new List<ReferenceDataDto>
        {
            ReferenceDataDtoBuilder.ForAgonist().Build(),
            ReferenceDataDtoBuilder.ForAntagonist().Build(),
            ReferenceDataDtoBuilder.ForSynergist().Build()
        };

        automocker
            .SetupReferenceDataCacheMissList()
            .SetupMuscleRoleDataServiceGetAllActive(muscleRoles);

        // Act
        var result = await testee.GetAllActiveAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        var items = result.Data.ToList();
        items.Should().HaveCount(3);
        
        var values = items.Select(d => d.Value);
        values.Should().Contain("Agonist");
        values.Should().Contain("Antagonist");
        values.Should().Contain("Synergist");
    }

    [Fact]
    public async Task GetByIdAsync_WithNullString_ReturnsValidationFailure()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<MuscleRoleService>();

        string? nullId = null;

        // Act
        var result = await testee.GetByIdAsync(nullId!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(MuscleRoleErrorMessages.InvalidIdFormat);

        automocker.VerifyMuscleRoleDataServiceGetByIdNeverCalled();
    }

    [Fact]
    public async Task GetByIdAsync_WithEmptyString_ReturnsValidationFailure()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<MuscleRoleService>();

        const string emptyId = "";

        // Act
        var result = await testee.GetByIdAsync(emptyId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(MuscleRoleErrorMessages.InvalidIdFormat);

        automocker.VerifyMuscleRoleDataServiceGetByIdNeverCalled();
    }
}