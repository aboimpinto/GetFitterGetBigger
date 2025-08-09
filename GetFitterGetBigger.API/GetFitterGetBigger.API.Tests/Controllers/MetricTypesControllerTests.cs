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
    public class MetricTypesControllerTests
    {
        private readonly Mock<IMetricTypeService> _mockMetricTypeService;
        private readonly Mock<ILogger<MetricTypesController>> _mockLogger;
        private readonly MetricTypesController _controller;

        public MetricTypesControllerTests()
        {
            _mockMetricTypeService = new Mock<IMetricTypeService>();
            _mockLogger = new Mock<ILogger<MetricTypesController>>();
            _controller = new MetricTypesController(
                _mockMetricTypeService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task GetMetricTypes_ReturnsOkWithMetricTypes()
        {
            // Arrange
            var metricTypes = new List<ReferenceDataDto>
            {
                new() { Id = "metrictype-123", Value = "Weight", Description = "Weight measurement" },
                new() { Id = "metrictype-456", Value = "Repetitions", Description = "Number of reps" }
            };
            var serviceResult = ServiceResult<IEnumerable<ReferenceDataDto>>.Success(metricTypes);

            _mockMetricTypeService
                .Setup(x => x.GetAllActiveAsync())
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetMetricTypes();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedMetricTypes = Assert.IsAssignableFrom<IEnumerable<ReferenceDataDto>>(okResult.Value);
            Assert.Equal(2, returnedMetricTypes.Count());
            _mockMetricTypeService.Verify(x => x.GetAllActiveAsync(), Times.Once);
        }

        [Fact]
        public async Task GetMetricTypeById_WithValidId_ReturnsOkWithMetricType()
        {
            // Arrange
            var id = "metrictype-12345678-1234-1234-1234-123456789012";
            var metricType = new ReferenceDataDto
                {
                Id = id,
                Value = "Weight",
                Description = "Weight measurement"
            };
            var serviceResult = ServiceResult<ReferenceDataDto>.Success(metricType);

            _mockMetricTypeService
                .Setup(x => x.GetByIdAsync(It.Is<MetricTypeId>(mtId => mtId.ToString() == id)))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetMetricTypeById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedMetricType = Assert.IsType<ReferenceDataDto>(okResult.Value);
            Assert.Equal(id, returnedMetricType.Id);
            Assert.Equal("Weight", returnedMetricType.Value);
            _mockMetricTypeService.Verify(x => x.GetByIdAsync(It.Is<MetricTypeId>(mtId => mtId.ToString() == id)), Times.Once);
        }

        [Fact]
        public async Task GetMetricTypeById_WithInvalidIdFormat_ReturnsBadRequest()
        {
            // Arrange
            var invalidId = "invalid-format";
            var serviceResult = ServiceResult<ReferenceDataDto>.Failure(
                new ReferenceDataDto(),
                ServiceError.ValidationFailed(MetricTypeErrorMessages.InvalidIdFormat));

            _mockMetricTypeService
                .Setup(x => x.GetByIdAsync(MetricTypeId.Empty))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetMetricTypeById(invalidId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = badRequestResult.Value;
            Assert.NotNull(errorResponse);
            _mockMetricTypeService.Verify(x => x.GetByIdAsync(MetricTypeId.Empty), Times.Once);
        }

        [Fact]
        public async Task GetMetricTypeById_WithNotFoundId_ReturnsNotFound()
        {
            // Arrange
            var id = "metrictype-12345678-1234-1234-1234-123456789012";
            var serviceResult = ServiceResult<ReferenceDataDto>.Failure(
                new ReferenceDataDto(),
                ServiceError.NotFound(MetricTypeErrorMessages.NotFound, id));

            _mockMetricTypeService
                .Setup(x => x.GetByIdAsync(It.Is<MetricTypeId>(mtId => mtId.ToString() == id)))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetMetricTypeById(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            _mockMetricTypeService.Verify(x => x.GetByIdAsync(It.Is<MetricTypeId>(mtId => mtId.ToString() == id)), Times.Once);
        }

        [Fact]
        public async Task GetMetricTypeByValue_WithValidValue_ReturnsOkWithMetricType()
        {
            // Arrange
            var value = "Weight";
            var metricType = new ReferenceDataDto
                {
                Id = "metrictype-123",
                Value = value,
                Description = "Weight measurement"
            };
            var serviceResult = ServiceResult<ReferenceDataDto>.Success(metricType);

            _mockMetricTypeService
                .Setup(x => x.GetByValueAsync(value))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetMetricTypeByValue(value);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedMetricType = Assert.IsType<ReferenceDataDto>(okResult.Value);
            Assert.Equal(value, returnedMetricType.Value);
            _mockMetricTypeService.Verify(x => x.GetByValueAsync(value), Times.Once);
        }

        [Fact]
        public async Task GetMetricTypeByValue_WithEmptyValue_ReturnsBadRequest()
        {
            // Arrange
            var emptyValue = "";
            var serviceResult = ServiceResult<ReferenceDataDto>.Failure(
                new ReferenceDataDto(),
                ServiceError.ValidationFailed(MetricTypeErrorMessages.ValueCannotBeEmpty));

            _mockMetricTypeService
                .Setup(x => x.GetByValueAsync(emptyValue))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetMetricTypeByValue(emptyValue);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = badRequestResult.Value;
            Assert.NotNull(errorResponse);
            _mockMetricTypeService.Verify(x => x.GetByValueAsync(emptyValue), Times.Once);
        }

        [Fact]
        public async Task GetMetricTypeByValue_WithNotFoundValue_ReturnsNotFound()
        {
            // Arrange
            var value = "NonExistent";
            var serviceResult = ServiceResult<ReferenceDataDto>.Failure(
                new ReferenceDataDto(),
                ServiceError.NotFound(MetricTypeErrorMessages.NotFound, value));

            _mockMetricTypeService
                .Setup(x => x.GetByValueAsync(value))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetMetricTypeByValue(value);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            _mockMetricTypeService.Verify(x => x.GetByValueAsync(value), Times.Once);
        }
    }
}