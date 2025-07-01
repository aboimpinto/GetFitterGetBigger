using System;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Controllers;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Controllers
{
    public class EquipmentControllerCrudTests
    {
        private readonly EquipmentController _controller;
        private readonly Mock<IEquipmentService> _mockEquipmentService;
        private readonly Mock<ILogger<EquipmentController>> _mockLogger;

        public EquipmentControllerCrudTests()
        {
            _mockEquipmentService = new Mock<IEquipmentService>();
            _mockLogger = new Mock<ILogger<EquipmentController>>();

            _controller = new EquipmentController(
                _mockEquipmentService.Object,
                _mockLogger.Object);
        }

        #region Create Tests

        [Fact]
        public async Task Create_WithValidData_ReturnsCreatedResult()
        {
            // Arrange
            var request = new CreateEquipmentDto { Name = "Barbell" };
            var createdDto = new EquipmentDto 
            { 
                Id = "equipment-123",
                Name = "Barbell",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            _mockEquipmentService.Setup(x => x.CreateEquipmentAsync(request))
                .ReturnsAsync(createdDto);

            // Act
            var result = await _controller.Create(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var dto = Assert.IsType<EquipmentDto>(createdResult.Value);
            Assert.Equal("Barbell", dto.Name);
            Assert.True(dto.IsActive);
        }

        [Fact]
        public async Task Create_WithDuplicateName_ReturnsConflict()
        {
            // Arrange
            var request = new CreateEquipmentDto { Name = "Barbell" };
            
            _mockEquipmentService.Setup(x => x.CreateEquipmentAsync(request))
                .ThrowsAsync(new InvalidOperationException("Equipment with the name 'Barbell' already exists"));

            // Act
            var result = await _controller.Create(request);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Contains("already exists", conflictResult.Value?.ToString());
        }

        [Fact]
        public async Task Create_WithNullRequest_ReturnsBadRequest()
        {
            // Arrange
            CreateEquipmentDto? nullRequest = null;
            
            _mockEquipmentService.Setup(x => x.CreateEquipmentAsync(It.IsAny<CreateEquipmentDto>()))
                .ThrowsAsync(new ArgumentNullException());

            // Act
            var result = await _controller.Create(nullRequest!);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task Update_WithValidData_ReturnsOk()
        {
            // Arrange
            var id = "equipment-123";
            var request = new UpdateEquipmentDto { Name = "Updated Barbell" };
            var updatedDto = new EquipmentDto 
            { 
                Id = id,
                Name = "Updated Barbell",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow
            };
            
            _mockEquipmentService.Setup(x => x.UpdateEquipmentAsync(id, request))
                .ReturnsAsync(updatedDto);

            // Act
            var result = await _controller.Update(id, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<EquipmentDto>(okResult.Value);
            Assert.Equal("Updated Barbell", dto.Name);
            Assert.NotNull(dto.UpdatedAt);
        }

        [Fact]
        public async Task Update_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var id = "equipment-999";
            var request = new UpdateEquipmentDto { Name = "Updated Barbell" };
            
            _mockEquipmentService.Setup(x => x.UpdateEquipmentAsync(id, request))
                .ThrowsAsync(new InvalidOperationException("Entity with ID 'equipment-999' not found"));

            // Act
            var result = await _controller.Update(id, request);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Update_WithDuplicateName_ReturnsConflict()
        {
            // Arrange
            var id = "equipment-123";
            var request = new UpdateEquipmentDto { Name = "Dumbbell" };
            
            _mockEquipmentService.Setup(x => x.UpdateEquipmentAsync(id, request))
                .ThrowsAsync(new InvalidOperationException("Equipment with the name 'Dumbbell' already exists"));

            // Act
            var result = await _controller.Update(id, request);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Contains("already exists", conflictResult.Value?.ToString());
        }

        [Fact]
        public async Task Update_WithInvalidIdFormat_ReturnsBadRequest()
        {
            // Arrange
            var id = "invalid-id";
            var request = new UpdateEquipmentDto { Name = "Updated Barbell" };
            
            _mockEquipmentService.Setup(x => x.UpdateEquipmentAsync(id, request))
                .ThrowsAsync(new ArgumentException("Invalid ID format"));

            // Act
            var result = await _controller.Update(id, request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var id = "equipment-123";
            
            _mockEquipmentService.Setup(x => x.DeactivateAsync(id))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var id = "equipment-999";
            
            _mockEquipmentService.Setup(x => x.DeactivateAsync(id))
                .ThrowsAsync(new InvalidOperationException("Equipment with ID 'equipment-999' not found"));

            // Act
            var result = await _controller.Delete(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_WithEquipmentInUse_ReturnsConflict()
        {
            // Arrange
            var id = "equipment-123";
            
            _mockEquipmentService.Setup(x => x.DeactivateAsync(id))
                .ThrowsAsync(new InvalidOperationException("Cannot deactivate equipment that is in use"));

            // Act
            var result = await _controller.Delete(id);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Contains("in use", conflictResult.Value?.ToString());
        }

        [Fact]
        public async Task Delete_WithInvalidIdFormat_ReturnsBadRequest()
        {
            // Arrange
            var id = "invalid-id";
            
            _mockEquipmentService.Setup(x => x.DeactivateAsync(id))
                .ThrowsAsync(new ArgumentException("Invalid ID format"));

            // Act
            var result = await _controller.Delete(id);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion
    }
}