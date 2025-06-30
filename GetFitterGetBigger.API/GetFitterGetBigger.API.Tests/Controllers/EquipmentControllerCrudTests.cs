using System;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Controllers;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Configuration;
using GetFitterGetBigger.API.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Olimpo.EntityFramework.Persistency;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Controllers
{
    public class EquipmentControllerCrudTests
    {
        private readonly EquipmentController _controller;
        private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _mockUnitOfWorkProvider;
        private readonly Mock<IWritableUnitOfWork<FitnessDbContext>> _mockUnitOfWork;
        private readonly Mock<IEquipmentRepository> _mockRepository;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<ILogger<EquipmentController>> _mockLogger;

        public EquipmentControllerCrudTests()
        {
            _mockUnitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
            _mockUnitOfWork = new Mock<IWritableUnitOfWork<FitnessDbContext>>();
            _mockRepository = new Mock<IEquipmentRepository>();
            _mockCacheService = new Mock<ICacheService>();
            _mockLogger = new Mock<ILogger<EquipmentController>>();

            var cacheConfig = new CacheConfiguration
            {
                DynamicTables = new TableCacheConfiguration
                {
                    DurationInHours = 1,
                    Tables = new List<string> { "Equipment" }
                }
            };
            var mockCacheOptions = new Mock<IOptions<CacheConfiguration>>();
            mockCacheOptions.Setup(x => x.Value).Returns(cacheConfig);

            _mockUnitOfWorkProvider.Setup(x => x.CreateWritable()).Returns(_mockUnitOfWork.Object);
            _mockUnitOfWork.Setup(x => x.GetRepository<IEquipmentRepository>()).Returns(_mockRepository.Object);

            _controller = new EquipmentController(
                _mockUnitOfWorkProvider.Object,
                _mockCacheService.Object,
                mockCacheOptions.Object,
                _mockLogger.Object);
        }

        #region Create Tests

        [Fact]
        public async Task Create_WithValidData_ReturnsCreatedResult()
        {
            // Arrange
            var request = new CreateEquipmentDto { Name = "Barbell" };
            var createdEquipment = Equipment.Handler.CreateNew("Barbell");
            
            _mockRepository.Setup(x => x.ExistsAsync("Barbell", null))
                .ReturnsAsync(false);
            _mockRepository.Setup(x => x.CreateAsync(It.IsAny<Equipment>()))
                .ReturnsAsync(createdEquipment);

            // Act
            var result = await _controller.Create(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var dto = Assert.IsType<EquipmentDto>(createdResult.Value);
            Assert.Equal("Barbell", dto.Name);
            Assert.True(dto.IsActive);
            
            // Verify cache invalidation
            _mockCacheService.Verify(x => x.RemoveAsync(It.Is<string>(k => k.Contains("GetAll"))), Times.Once);
            // Repository handles saving, no need to verify CommitAsync
        }

        [Fact]
        public async Task Create_WithDuplicateName_ReturnsConflict()
        {
            // Arrange
            var request = new CreateEquipmentDto { Name = "Barbell" };
            
            _mockRepository.Setup(x => x.ExistsAsync("Barbell", null))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Create(request);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal("Equipment with the name 'Barbell' already exists", conflictResult.Value);
            
            _mockRepository.Verify(x => x.CreateAsync(It.IsAny<Equipment>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task Create_TrimsWhitespace()
        {
            // Arrange
            var request = new CreateEquipmentDto { Name = "  Barbell  " };
            var createdEquipment = Equipment.Handler.CreateNew("Barbell");
            
            _mockRepository.Setup(x => x.ExistsAsync("  Barbell  ", null))
                .ReturnsAsync(false);
            _mockRepository.Setup(x => x.CreateAsync(It.Is<Equipment>(e => e.Name == "Barbell")))
                .ReturnsAsync(createdEquipment);

            // Act
            var result = await _controller.Create(request);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result);
            _mockRepository.Verify(x => x.CreateAsync(It.Is<Equipment>(e => e.Name == "Barbell")), Times.Once);
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task Update_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var equipmentId = EquipmentId.New();
            var existingEquipment = Equipment.Handler.Create(equipmentId, "Barbell");
            var request = new UpdateEquipmentDto { Name = "Olympic Barbell" };
            var updatedEquipment = Equipment.Handler.Update(existingEquipment, "Olympic Barbell");
            
            _mockRepository.Setup(x => x.GetByIdAsync(equipmentId))
                .ReturnsAsync(existingEquipment);
            _mockRepository.Setup(x => x.ExistsAsync("Olympic Barbell", equipmentId))
                .ReturnsAsync(false);
            _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<Equipment>()))
                .ReturnsAsync(updatedEquipment);

            // Act
            var result = await _controller.Update(equipmentId.ToString(), request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<EquipmentDto>(okResult.Value);
            Assert.Equal("Olympic Barbell", dto.Name);
            Assert.NotNull(dto.UpdatedAt);
            
            // Verify cache invalidation
            _mockCacheService.Verify(x => x.RemoveAsync(It.Is<string>(k => k.Contains("GetAll"))), Times.Once);
            _mockCacheService.Verify(x => x.RemoveAsync(It.Is<string>(k => k.Contains("GetById"))), Times.Once);
            _mockCacheService.Verify(x => x.RemoveAsync(It.Is<string>(k => k.Contains("barbell"))), Times.AtLeastOnce);
            // Repository handles saving, no need to verify CommitAsync
        }

        [Fact]
        public async Task Update_WithInvalidIdFormat_ReturnsBadRequest()
        {
            // Arrange
            var request = new UpdateEquipmentDto { Name = "Olympic Barbell" };

            // Act
            var result = await _controller.Update("invalid-id", request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Invalid ID format", badRequestResult.Value?.ToString() ?? string.Empty);
        }

        [Fact]
        public async Task Update_WhenNotExists_ReturnsNotFound()
        {
            // Arrange
            var equipmentId = EquipmentId.New();
            var request = new UpdateEquipmentDto { Name = "Olympic Barbell" };
            
            _mockRepository.Setup(x => x.GetByIdAsync(equipmentId))
                .ReturnsAsync((Equipment?)null);

            // Act
            var result = await _controller.Update(equipmentId.ToString(), request);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Equipment>()), Times.Never);
        }

        [Fact]
        public async Task Update_WithDuplicateName_ReturnsConflict()
        {
            // Arrange
            var equipmentId = EquipmentId.New();
            var existingEquipment = Equipment.Handler.Create(equipmentId, "Barbell");
            var request = new UpdateEquipmentDto { Name = "Dumbbell" };
            
            _mockRepository.Setup(x => x.GetByIdAsync(equipmentId))
                .ReturnsAsync(existingEquipment);
            _mockRepository.Setup(x => x.ExistsAsync("Dumbbell", equipmentId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Update(equipmentId.ToString(), request);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal("Equipment with the name 'Dumbbell' already exists", conflictResult.Value);
            _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Equipment>()), Times.Never);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_WhenNotInUse_ReturnsNoContent()
        {
            // Arrange
            var equipmentId = EquipmentId.New();
            var existingEquipment = Equipment.Handler.Create(equipmentId, "Barbell");
            
            _mockRepository.Setup(x => x.GetByIdAsync(equipmentId))
                .ReturnsAsync(existingEquipment);
            _mockRepository.Setup(x => x.IsInUseAsync(equipmentId))
                .ReturnsAsync(false);
            _mockRepository.Setup(x => x.DeactivateAsync(equipmentId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(equipmentId.ToString());

            // Assert
            Assert.IsType<NoContentResult>(result);
            
            // Verify cache invalidation
            _mockCacheService.Verify(x => x.RemoveAsync(It.Is<string>(k => k.Contains("GetAll"))), Times.Once);
            _mockCacheService.Verify(x => x.RemoveAsync(It.Is<string>(k => k.Contains("GetById"))), Times.Once);
            _mockCacheService.Verify(x => x.RemoveAsync(It.Is<string>(k => k.Contains("barbell"))), Times.Once);
            // Repository handles saving, no need to verify CommitAsync
        }

        [Fact]
        public async Task Delete_WithInvalidIdFormat_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Delete("invalid-id");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Invalid ID format", badRequestResult.Value?.ToString() ?? string.Empty);
        }

        [Fact]
        public async Task Delete_WhenNotExists_ReturnsNotFound()
        {
            // Arrange
            var equipmentId = EquipmentId.New();
            
            _mockRepository.Setup(x => x.GetByIdAsync(equipmentId))
                .ReturnsAsync((Equipment?)null);

            // Act
            var result = await _controller.Delete(equipmentId.ToString());

            // Assert
            Assert.IsType<NotFoundResult>(result);
            _mockRepository.Verify(x => x.IsInUseAsync(It.IsAny<EquipmentId>()), Times.Never);
            _mockRepository.Verify(x => x.DeactivateAsync(It.IsAny<EquipmentId>()), Times.Never);
        }

        [Fact]
        public async Task Delete_WhenInUse_ReturnsConflict()
        {
            // Arrange
            var equipmentId = EquipmentId.New();
            var existingEquipment = Equipment.Handler.Create(equipmentId, "Barbell");
            
            _mockRepository.Setup(x => x.GetByIdAsync(equipmentId))
                .ReturnsAsync(existingEquipment);
            _mockRepository.Setup(x => x.IsInUseAsync(equipmentId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(equipmentId.ToString());

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal("Cannot deactivate equipment that is in use by exercises", conflictResult.Value);
            _mockRepository.Verify(x => x.DeactivateAsync(It.IsAny<EquipmentId>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task Delete_WhenDeactivateFails_ReturnsNotFound()
        {
            // Arrange
            var equipmentId = EquipmentId.New();
            var existingEquipment = Equipment.Handler.Create(equipmentId, "Barbell");
            
            _mockRepository.Setup(x => x.GetByIdAsync(equipmentId))
                .ReturnsAsync(existingEquipment);
            _mockRepository.Setup(x => x.IsInUseAsync(equipmentId))
                .ReturnsAsync(false);
            _mockRepository.Setup(x => x.DeactivateAsync(equipmentId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Delete(equipmentId.ToString());

            // Assert
            Assert.IsType<NotFoundResult>(result);
            _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Never);
        }

        #endregion
    }
}