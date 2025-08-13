using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.Equipment;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.ReferenceTables.Equipment;
using GetFitterGetBigger.API.Services.ReferenceTables.Equipment.DataServices;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.Services.Extensions;
using GetFitterGetBigger.API.Tests.TestBuilders.DTOs;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services;

/// <summary>
/// Unit tests for EquipmentService
/// Tests the Equipment service layer with proper mocking and isolation
/// </summary>
public class EquipmentServiceTests
{
    // Note: Equipment is CRUD-enabled and doesn't use caching, so cache tests have been removed

    [Fact]
    public async Task GetByIdAsync_EmptyId_ReturnsValidationError()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<EquipmentService>();
        
        var emptyId = EquipmentId.Empty;

        // Act
        var result = await testee.GetByIdAsync(emptyId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        autoMocker.GetMock<IEquipmentDataService>()
            .Verify(x => x.GetByIdAsync(It.IsAny<EquipmentId>()), Times.Never);
    }

    [Fact]
    public async Task ExistsAsync_ValidId_ReturnsTrue()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<EquipmentService>();
        
        var equipmentId = EquipmentId.New();
        var equipment = EquipmentDtoTestBuilder.Barbell().WithId(equipmentId).Build();

        autoMocker.GetMock<IEquipmentDataService>()
            .Setup(x => x.GetByIdAsync(equipmentId))
            .ReturnsAsync(ServiceResult<EquipmentDto>.Success(equipment));

        // Act
        var result = await testee.ExistsAsync(equipmentId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeTrue();
        autoMocker.GetMock<IEquipmentDataService>()
            .Verify(x => x.GetByIdAsync(equipmentId), Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_NonExistentId_ReturnsFalse()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<EquipmentService>();
        
        var equipmentId = EquipmentId.New();

        autoMocker.GetMock<IEquipmentDataService>()
            .Setup(x => x.GetByIdAsync(equipmentId))
            .ReturnsAsync(ServiceResult<EquipmentDto>.Success(EquipmentDto.Empty));

        // Act
        var result = await testee.ExistsAsync(equipmentId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeFalse();
        autoMocker.GetMock<IEquipmentDataService>()
            .Verify(x => x.GetByIdAsync(equipmentId), Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_EmptyId_ReturnsValidationError()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<EquipmentService>();
        
        var emptyId = EquipmentId.Empty;

        // Act
        var result = await testee.ExistsAsync(emptyId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        autoMocker.GetMock<IEquipmentDataService>()
            .Verify(x => x.GetByIdAsync(It.IsAny<EquipmentId>()), Times.Never);
    }
}