using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.Controllers;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.ReferenceTables.DifficultyLevel;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace GetFitterGetBigger.API.Tests.Controllers
{
    public class DifficultyLevelsControllerTests
    {
        private readonly Mock<IDifficultyLevelService> _mockDifficultyLevelService;
        private readonly Mock<ILogger<DifficultyLevelsController>> _mockLogger;
        private readonly DifficultyLevelsController _controller;

        public DifficultyLevelsControllerTests()
        {
            _mockDifficultyLevelService = new Mock<IDifficultyLevelService>();
            _mockLogger = new Mock<ILogger<DifficultyLevelsController>>();
            _controller = new DifficultyLevelsController(
                _mockDifficultyLevelService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task GetDifficultyLevels_ReturnsOkWithDifficultyLevels()
        {
            // Arrange
            var difficultyLevels = new List<ReferenceDataDto>
            {
                new() { Id = "difficultylevel-123", Value = "Beginner", Description = "For beginners" },
                new() { Id = "difficultylevel-456", Value = "Intermediate", Description = "For intermediate users" }
            };
            var serviceResult = ServiceResult<IEnumerable<ReferenceDataDto>>.Success(difficultyLevels);

            _mockDifficultyLevelService
                .Setup(x => x.GetAllActiveAsync())
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetDifficultyLevels();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedDifficultyLevels = Assert.IsAssignableFrom<IEnumerable<ReferenceDataDto>>(okResult.Value);
            Assert.Equal(2, returnedDifficultyLevels.Count());
            _mockDifficultyLevelService.Verify(x => x.GetAllActiveAsync(), Times.Once);
        }

        [Fact]
        public async Task GetDifficultyLevelById_WithValidId_ReturnsOkWithDifficultyLevel()
        {
            // Arrange
            var id = "difficultylevel-12345678-1234-1234-1234-123456789012";
            var difficultyLevel = new ReferenceDataDto
                {
                Id = id,
                Value = "Advanced",
                Description = "For advanced users"
            };
            var serviceResult = ServiceResult<ReferenceDataDto>.Success(difficultyLevel);

            _mockDifficultyLevelService
                .Setup(x => x.GetByIdAsync(It.Is<DifficultyLevelId>(dlId => dlId.ToString() == id)))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetDifficultyLevelById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedDifficultyLevel = Assert.IsType<ReferenceDataDto>(okResult.Value);
            Assert.Equal(id, returnedDifficultyLevel.Id);
            Assert.Equal("Advanced", returnedDifficultyLevel.Value);
            _mockDifficultyLevelService.Verify(x => x.GetByIdAsync(It.IsAny<DifficultyLevelId>()), Times.Once);
        }

        [Fact]
        public async Task GetDifficultyLevelById_WithEmptyGuid_ReturnsBadRequest()
        {
            // Arrange
            var id = "difficultylevel-00000000-0000-0000-0000-000000000000";
            var serviceResult = ServiceResult<ReferenceDataDto>.Failure(
                new ReferenceDataDto(),
                ServiceError.ValidationFailed(DifficultyLevelErrorMessages.InvalidIdFormat));

            _mockDifficultyLevelService
                .Setup(x => x.GetByIdAsync(It.IsAny<DifficultyLevelId>()))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetDifficultyLevelById(id);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = badRequestResult.Value;
            Assert.NotNull(errorResponse);
            _mockDifficultyLevelService.Verify(x => x.GetByIdAsync(It.IsAny<DifficultyLevelId>()), Times.Once);
        }

        [Fact]
        public async Task GetDifficultyLevelById_WithInvalidFormat_ReturnsBadRequest()
        {
            // Arrange
            var id = "invalid-format";
            var serviceResult = ServiceResult<ReferenceDataDto>.Failure(
                new ReferenceDataDto(),
                ServiceError.ValidationFailed(DifficultyLevelErrorMessages.InvalidIdFormat));

            _mockDifficultyLevelService
                .Setup(x => x.GetByIdAsync(It.IsAny<DifficultyLevelId>()))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetDifficultyLevelById(id);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = badRequestResult.Value;
            Assert.NotNull(errorResponse);
            _mockDifficultyLevelService.Verify(x => x.GetByIdAsync(It.IsAny<DifficultyLevelId>()), Times.Once);
        }

        [Fact]
        public async Task GetDifficultyLevelById_WhenNotFound_ReturnsNotFound()
        {
            // Arrange
            var id = "difficultylevel-12345678-1234-1234-1234-123456789012";
            var serviceResult = ServiceResult<ReferenceDataDto>.Failure(
                new ReferenceDataDto(),
                ServiceError.NotFound("Difficulty level", id));

            _mockDifficultyLevelService
                .Setup(x => x.GetByIdAsync(It.IsAny<DifficultyLevelId>()))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetDifficultyLevelById(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            _mockDifficultyLevelService.Verify(x => x.GetByIdAsync(It.IsAny<DifficultyLevelId>()), Times.Once);
        }

        [Fact]
        public async Task GetDifficultyLevelByValue_WithValidValue_ReturnsOkWithDifficultyLevel()
        {
            // Arrange
            var value = "Expert";
            var difficultyLevel = new ReferenceDataDto
                {
                Id = "difficultylevel-789",
                Value = value,
                Description = "For experts only"
            };
            var serviceResult = ServiceResult<ReferenceDataDto>.Success(difficultyLevel);

            _mockDifficultyLevelService
                .Setup(x => x.GetByValueAsync(value))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetDifficultyLevelByValue(value);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedDifficultyLevel = Assert.IsType<ReferenceDataDto>(okResult.Value);
            Assert.Equal(value, returnedDifficultyLevel.Value);
            _mockDifficultyLevelService.Verify(x => x.GetByValueAsync(value), Times.Once);
        }

        [Fact]
        public async Task GetDifficultyLevelByValue_WithEmptyValue_ReturnsBadRequest()
        {
            // Arrange
            var value = "";
            var serviceResult = ServiceResult<ReferenceDataDto>.Failure(
                new ReferenceDataDto(),
                ServiceError.ValidationFailed(DifficultyLevelErrorMessages.ValueCannotBeEmpty));

            _mockDifficultyLevelService
                .Setup(x => x.GetByValueAsync(value))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetDifficultyLevelByValue(value);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = badRequestResult.Value;
            Assert.NotNull(errorResponse);
            _mockDifficultyLevelService.Verify(x => x.GetByValueAsync(value), Times.Once);
        }

        [Fact]
        public async Task GetDifficultyLevelByValue_WhenNotFound_ReturnsNotFound()
        {
            // Arrange
            var value = "NonExistent";
            var serviceResult = ServiceResult<ReferenceDataDto>.Failure(
                new ReferenceDataDto(),
                ServiceError.NotFound("Difficulty level", value));

            _mockDifficultyLevelService
                .Setup(x => x.GetByValueAsync(value))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetDifficultyLevelByValue(value);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            _mockDifficultyLevelService.Verify(x => x.GetByValueAsync(value), Times.Once);
        }

        [Fact]
        public async Task GetDifficultyLevels_WhenServiceFails_StillReturnsOk()
        {
            // Arrange
            var emptyList = new List<ReferenceDataDto>();
            var serviceResult = ServiceResult<IEnumerable<ReferenceDataDto>>.Failure(
                emptyList,
                ServiceError.InternalError("Database error"));

            _mockDifficultyLevelService
                .Setup(x => x.GetAllActiveAsync())
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetDifficultyLevels();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedDifficultyLevels = Assert.IsAssignableFrom<IEnumerable<ReferenceDataDto>>(okResult.Value);
            Assert.Empty(returnedDifficultyLevels);
            _mockDifficultyLevelService.Verify(x => x.GetAllActiveAsync(), Times.Once);
        }
    }
}