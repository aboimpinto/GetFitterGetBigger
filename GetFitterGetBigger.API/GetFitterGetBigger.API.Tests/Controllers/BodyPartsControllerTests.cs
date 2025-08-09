using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.Controllers;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Controllers
{
    public class BodyPartsControllerTests
    {
        private readonly Mock<IBodyPartService> _mockBodyPartService;
        private readonly Mock<ILogger<BodyPartsController>> _mockLogger;
        private readonly BodyPartsController _controller;

        public BodyPartsControllerTests()
        {
            _mockBodyPartService = new Mock<IBodyPartService>();
            _mockLogger = new Mock<ILogger<BodyPartsController>>();
            _controller = new BodyPartsController(
                _mockBodyPartService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task GetBodyParts_ReturnsOkWithBodyParts()
        {
            // Arrange
            var bodyParts = new List<BodyPartDto>
            {
                new() { Id = "bodypart-123", Value = "Chest", Description = "Chest muscles" },
                new() { Id = "bodypart-456", Value = "Back", Description = "Back muscles" }
            };
            var serviceResult = ServiceResult<IEnumerable<BodyPartDto>>.Success(bodyParts);

            _mockBodyPartService
                .Setup(x => x.GetAllActiveAsync())
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetBodyParts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedBodyParts = Assert.IsAssignableFrom<IEnumerable<BodyPartDto>>(okResult.Value);
            Assert.Equal(2, returnedBodyParts.Count());
            _mockBodyPartService.Verify(x => x.GetAllActiveAsync(), Times.Once);
        }

        [Fact]
        public async Task GetBodyPartById_WithValidId_ReturnsOkWithBodyPart()
        {
            // Arrange
            var id = "bodypart-12345678-1234-1234-1234-123456789012";
            var bodyPart = new BodyPartDto
            {
                Id = id,
                Value = "Chest",
                Description = "Chest muscles"
            };
            var serviceResult = ServiceResult<BodyPartDto>.Success(bodyPart);

            _mockBodyPartService
                .Setup(x => x.GetByIdAsync(It.Is<BodyPartId>(bpId => bpId.ToString() == id)))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetBodyPartById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedBodyPart = Assert.IsType<BodyPartDto>(okResult.Value);
            Assert.Equal(id, returnedBodyPart.Id);
            Assert.Equal("Chest", returnedBodyPart.Value);
            _mockBodyPartService.Verify(x => x.GetByIdAsync(It.Is<BodyPartId>(bpId => bpId.ToString() == id)), Times.Once);
        }

        [Fact]
        public async Task GetBodyPartById_WithInvalidIdFormat_ReturnsBadRequest()
        {
            // Arrange
            var invalidId = "invalid-format";
            var serviceResult = ServiceResult<BodyPartDto>.Failure(
                new BodyPartDto(),
                ServiceError.ValidationFailed(BodyPartErrorMessages.InvalidIdFormat));

            _mockBodyPartService
                .Setup(x => x.GetByIdAsync(BodyPartId.Empty))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetBodyPartById(invalidId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
            _mockBodyPartService.Verify(x => x.GetByIdAsync(BodyPartId.Empty), Times.Once);
        }

        [Fact]
        public async Task GetBodyPartById_WhenNotFound_ReturnsNotFound()
        {
            // Arrange
            var id = "bodypart-00000000-0000-0000-0000-000000000999";
            var serviceResult = ServiceResult<BodyPartDto>.Failure(
                new BodyPartDto(),
                ServiceError.NotFound(BodyPartErrorMessages.NotFound));

            _mockBodyPartService
                .Setup(x => x.GetByIdAsync(It.Is<BodyPartId>(bpId => bpId.ToString() == id)))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetBodyPartById(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            _mockBodyPartService.Verify(x => x.GetByIdAsync(It.Is<BodyPartId>(bpId => bpId.ToString() == id)), Times.Once);
        }

        [Fact]
        public async Task GetBodyPartByValue_WithExistingValue_ReturnsOk()
        {
            // Arrange
            var value = "Chest";
            var bodyPart = new BodyPartDto
                {
                Id = "bodypart-123",
                Value = value,
                Description = "Chest muscles"
            };
            var serviceResult = ServiceResult<BodyPartDto>.Success(bodyPart);

            _mockBodyPartService
                .Setup(x => x.GetByValueAsync(value))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetBodyPartByValue(value);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedBodyPart = Assert.IsType<BodyPartDto>(okResult.Value);
            Assert.Equal(value, returnedBodyPart.Value);
            _mockBodyPartService.Verify(x => x.GetByValueAsync(value), Times.Once);
        }

        [Fact]
        public async Task GetBodyPartByValue_WhenNotFound_ReturnsNotFound()
        {
            // Arrange
            var value = "NonExistent";
            var serviceResult = ServiceResult<BodyPartDto>.Failure(
                new BodyPartDto(),
                ServiceError.NotFound(BodyPartErrorMessages.NotFound));

            _mockBodyPartService
                .Setup(x => x.GetByValueAsync(value))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetBodyPartByValue(value);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            _mockBodyPartService.Verify(x => x.GetByValueAsync(value), Times.Once);
        }

        [Fact]
        public async Task GetBodyPartByValue_WhenServiceFails_ReturnsBadRequest()
        {
            // Arrange
            var value = "";
            var serviceResult = ServiceResult<BodyPartDto>.Failure(
                new BodyPartDto(),
                ServiceError.ValidationFailed(BodyPartErrorMessages.ValueCannotBeEmpty));

            _mockBodyPartService
                .Setup(x => x.GetByValueAsync(value))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetBodyPartByValue(value);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
            _mockBodyPartService.Verify(x => x.GetByValueAsync(value), Times.Once);
        }
    }
}