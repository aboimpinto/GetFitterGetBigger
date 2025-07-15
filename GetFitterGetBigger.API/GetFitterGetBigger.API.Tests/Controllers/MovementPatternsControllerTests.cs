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
using GetFitterGetBigger.API.Tests.TestConstants;
using GetFitterGetBigger.API.Tests.TestBuilders;

namespace GetFitterGetBigger.API.Tests.Controllers
{
    public class MovementPatternsControllerTests
    {
        private readonly Mock<IMovementPatternService> _mockMovementPatternService;
        private readonly Mock<ILogger<MovementPatternsController>> _mockLogger;
        private readonly MovementPatternsController _controller;

        public MovementPatternsControllerTests()
        {
            _mockMovementPatternService = new Mock<IMovementPatternService>();
            _mockLogger = new Mock<ILogger<MovementPatternsController>>();
            _controller = new MovementPatternsController(
                _mockMovementPatternService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithMovementPatterns()
        {
            // Arrange
            var movementPatterns = new List<ReferenceDataDto>
            {
                new() { Id = TestIds.MovementPatternIds.Push, Value = MovementPatternTestConstants.HorizontalPushName, Description = MovementPatternTestConstants.PushingForwardDescription },
                new() { Id = TestIds.MovementPatternIds.Pull, Value = MovementPatternTestConstants.VerticalPullName, Description = MovementPatternTestConstants.PullingDownwardDescription }
            };
            var serviceResult = ServiceResult<IEnumerable<ReferenceDataDto>>.Success(movementPatterns);

            _mockMovementPatternService
                .Setup(x => x.GetAllActiveAsync())
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedMovementPatterns = Assert.IsAssignableFrom<IEnumerable<ReferenceDataDto>>(okResult.Value);
            Assert.Equal(2, returnedMovementPatterns.Count());
            _mockMovementPatternService.Verify(x => x.GetAllActiveAsync(), Times.Once);
        }

        [Fact]
        public async Task GetById_WithValidId_ReturnsOkWithMovementPattern()
        {
            // Arrange
            var id = TestIds.MovementPatternIds.Push;
            var movementPattern = new ReferenceDataDto
            {
                Id = id,
                Value = MovementPatternTestConstants.HorizontalPushName,
                Description = MovementPatternTestConstants.PushingForwardDescription
            };
            var serviceResult = ServiceResult<ReferenceDataDto>.Success(movementPattern);

            _mockMovementPatternService
                .Setup(x => x.GetByIdAsync(It.Is<MovementPatternId>(mpId => mpId.ToString() == id)))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedMovementPattern = Assert.IsType<ReferenceDataDto>(okResult.Value);
            Assert.Equal(id, returnedMovementPattern.Id);
            Assert.Equal(MovementPatternTestConstants.HorizontalPushName, returnedMovementPattern.Value);
            _mockMovementPatternService.Verify(x => x.GetByIdAsync(It.IsAny<MovementPatternId>()), Times.Once);
        }

        [Fact]
        public async Task GetById_WithInvalidIdFormat_ReturnsBadRequest()
        {
            // Arrange
            var invalidId = MovementPatternTestConstants.InvalidFormatId;
            var dto = new ReferenceDataDto();
            _mockMovementPatternService
                .Setup(x => x.GetByIdAsync(It.IsAny<MovementPatternId>()))
                .ReturnsAsync(ServiceResult<ReferenceDataDto>.Failure(dto, ServiceError.ValidationFailed(MovementPatternErrorMessages.InvalidIdFormat)));

            // Act
            var result = await _controller.GetById(invalidId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
            
            // Verify the service WAS called since we now let the service handle validation
            _mockMovementPatternService.Verify(x => x.GetByIdAsync(It.IsAny<MovementPatternId>()), Times.Once);
        }

        [Fact]
        public async Task GetById_WhenNotFound_ReturnsNotFound()
        {
            // Arrange
            var id = MovementPatternTestConstants.NonExistentId;
            var serviceResult = ServiceResult<ReferenceDataDto>.Failure(
                new ReferenceDataDto(),
                ServiceError.NotFound(MovementPatternErrorMessages.NotFound));

            _mockMovementPatternService
                .Setup(x => x.GetByIdAsync(It.Is<MovementPatternId>(mpId => mpId.ToString() == id)))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            _mockMovementPatternService.Verify(x => x.GetByIdAsync(It.IsAny<MovementPatternId>()), Times.Once);
        }

        [Fact]
        public async Task GetByName_WithValidName_ReturnsOkWithMovementPattern()
        {
            // Arrange
            var name = MovementPatternTestConstants.HorizontalPushName;
            var movementPattern = new ReferenceDataDto
            {
                Id = TestIds.MovementPatternIds.Push,
                Value = name,
                Description = MovementPatternTestConstants.PushingForwardDescription
            };
            var serviceResult = ServiceResult<ReferenceDataDto>.Success(movementPattern);

            _mockMovementPatternService
                .Setup(x => x.GetByValueAsync(name))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetByName(name);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedMovementPattern = Assert.IsType<ReferenceDataDto>(okResult.Value);
            Assert.Equal(name, returnedMovementPattern.Value);
            _mockMovementPatternService.Verify(x => x.GetByValueAsync(name), Times.Once);
        }

        [Fact]
        public async Task GetByName_WhenNotFound_ReturnsNotFound()
        {
            // Arrange
            var name = "NonExistent";
            var serviceResult = ServiceResult<ReferenceDataDto>.Failure(
                new ReferenceDataDto(),
                ServiceError.NotFound(MovementPatternErrorMessages.NotFound));

            _mockMovementPatternService
                .Setup(x => x.GetByValueAsync(name))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetByName(name);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            _mockMovementPatternService.Verify(x => x.GetByValueAsync(name), Times.Once);
        }

        [Fact]
        public async Task GetByValue_WithValidValue_ReturnsOkWithMovementPattern()
        {
            // Arrange
            var value = MovementPatternTestConstants.VerticalPullName;
            var movementPattern = new ReferenceDataDto
            {
                Id = TestIds.MovementPatternIds.Pull,
                Value = value,
                Description = MovementPatternTestConstants.PullingDownwardDescription
            };
            var serviceResult = ServiceResult<ReferenceDataDto>.Success(movementPattern);

            _mockMovementPatternService
                .Setup(x => x.GetByValueAsync(value))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetByValue(value);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedMovementPattern = Assert.IsType<ReferenceDataDto>(okResult.Value);
            Assert.Equal(value, returnedMovementPattern.Value);
            _mockMovementPatternService.Verify(x => x.GetByValueAsync(value), Times.Once);
        }

        [Fact]
        public async Task GetByValue_WhenNotFound_ReturnsNotFound()
        {
            // Arrange
            var value = "NonExistent";
            var serviceResult = ServiceResult<ReferenceDataDto>.Failure(
                new ReferenceDataDto(),
                ServiceError.NotFound(MovementPatternErrorMessages.NotFound));

            _mockMovementPatternService
                .Setup(x => x.GetByValueAsync(value))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetByValue(value);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            _mockMovementPatternService.Verify(x => x.GetByValueAsync(value), Times.Once);
        }
    }
}