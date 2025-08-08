using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Implementations;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.TestConstants;
using Microsoft.Extensions.Logging;
using Moq;
using Olimpo.EntityFramework.Persistency;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services
{
    public class MetricTypeServiceTests
    {
        private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _mockUnitOfWorkProvider;
        private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _mockReadOnlyUnitOfWork;
        private readonly Mock<IMetricTypeRepository> _mockMetricTypeRepository;
        private readonly Mock<IEternalCacheService> _mockCacheService;
        private readonly Mock<ILogger<MetricTypeService>> _mockLogger;
        private readonly MetricTypeService _metricTypeService;

        public MetricTypeServiceTests()
        {
            _mockUnitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
            _mockReadOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
            _mockMetricTypeRepository = new Mock<IMetricTypeRepository>();
            _mockCacheService = new Mock<IEternalCacheService>();
            _mockLogger = new Mock<ILogger<MetricTypeService>>();

            _mockUnitOfWorkProvider
                .Setup(x => x.CreateReadOnly())
                .Returns(_mockReadOnlyUnitOfWork.Object);

            _mockReadOnlyUnitOfWork
                .Setup(x => x.GetRepository<IMetricTypeRepository>())
                .Returns(_mockMetricTypeRepository.Object);

            _metricTypeService = new MetricTypeService(
                _mockUnitOfWorkProvider.Object,
                _mockCacheService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task ExistsAsync_WithMetricTypeId_WhenMetricTypeExists_ReturnsTrue()
        {
            // Arrange
            var metricTypeId = MetricTypeId.New();

            _mockMetricTypeRepository
                .Setup(x => x.ExistsAsync(It.IsAny<MetricTypeId>()))
                .ReturnsAsync(true);

            // Act
            var result = await _metricTypeService.ExistsAsync(metricTypeId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Data);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            _mockMetricTypeRepository.Verify(x => x.ExistsAsync(It.IsAny<MetricTypeId>()), Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_WithMetricTypeId_WhenMetricTypeDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var metricTypeId = MetricTypeId.New();

            _mockMetricTypeRepository
                .Setup(x => x.ExistsAsync(It.IsAny<MetricTypeId>()))
                .ReturnsAsync(false);

            // Act
            var result = await _metricTypeService.ExistsAsync(metricTypeId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.Data);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            _mockMetricTypeRepository.Verify(x => x.ExistsAsync(It.IsAny<MetricTypeId>()), Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_WithEmptyMetricTypeId_ReturnsFalse()
        {
            // Arrange
            var emptyId = MetricTypeId.Empty;

            // Act
            var result = await _metricTypeService.ExistsAsync(emptyId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceErrorCode.InvalidFormat, result.PrimaryErrorCode);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Never);
            _mockMetricTypeRepository.Verify(x => x.GetByIdAsync(It.IsAny<MetricTypeId>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_WhenMetricTypeExists_ReturnsSuccess()
        {
            // Arrange
            var metricTypeId = MetricTypeId.New();
            var metricType = MetricType.Handler.Create(
                metricTypeId,
                "Weight",
                "Weight measurement in kg or lbs",
                1,
                true).Value;

            _mockCacheService
                .Setup(x => x.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());

            _mockMetricTypeRepository
                .Setup(x => x.GetByIdAsync(metricTypeId))
                .ReturnsAsync(metricType);

            _mockCacheService
                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<ReferenceDataDto>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _metricTypeService.GetByIdAsync(metricTypeId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(metricType.Id, result.Data.Id);
            Assert.Equal(metricType.Value, result.Data.Value);
            Assert.Equal(metricType.Description, result.Data.Description);
        }

        [Fact]
        public async Task GetByIdAsync_WithEmptyId_ReturnsValidationFailed()
        {
            // Arrange
            var emptyId = MetricTypeId.Empty;

            // Act
            var result = await _metricTypeService.GetByIdAsync(emptyId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
            Assert.Contains(MetricTypeErrorMessages.InvalidIdFormat, result.StructuredErrors.Select(e => e.Message));
        }

        [Fact]
        public async Task GetByValueAsync_WithValidValue_WhenMetricTypeExists_ReturnsSuccess()
        {
            // Arrange
            var value = "Weight";
            var metricType = MetricType.Handler.Create(
                MetricTypeId.New(),
                value,
                "Weight measurement in kg or lbs",
                1,
                true).Value;

            _mockCacheService
                .Setup(x => x.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());

            _mockMetricTypeRepository
                .Setup(x => x.GetByValueAsync(value))
                .ReturnsAsync(metricType);

            _mockCacheService
                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<ReferenceDataDto>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _metricTypeService.GetByValueAsync(value);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(metricType.Id, result.Data.Id);
            Assert.Equal(metricType.Value, result.Data.Value);
            Assert.Equal(metricType.Description, result.Data.Description);
        }

        [Fact]
        public async Task GetByValueAsync_WithEmptyValue_ReturnsValidationFailed()
        {
            // Arrange
            var emptyValue = string.Empty;

            // Act
            var result = await _metricTypeService.GetByValueAsync(emptyValue);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
            Assert.Contains(MetricTypeErrorMessages.ValueCannotBeEmpty, result.StructuredErrors.Select(e => e.Message));
        }

        [Fact]
        public async Task GetAllActiveAsync_ReturnsAllActiveMetricTypes()
        {
            // Arrange
            var metricTypes = new List<MetricType>
            {
                MetricType.Handler.Create(MetricTypeId.New(), "Weight", "Weight measurement", 1, true).Value,
                MetricType.Handler.Create(MetricTypeId.New(), "Repetitions", "Number of reps", 2, true).Value,
                MetricType.Handler.Create(MetricTypeId.New(), "Time", "Duration in seconds", 3, true).Value
            };

            _mockCacheService
                .Setup(x => x.GetAsync<IEnumerable<ReferenceDataDto>>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<IEnumerable<ReferenceDataDto>>.Miss());

            _mockMetricTypeRepository
                .Setup(x => x.GetAllActiveAsync())
                .ReturnsAsync(metricTypes);

            _mockCacheService
                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ReferenceDataDto>>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _metricTypeService.GetAllActiveAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(3, result.Data.Count());
            Assert.All(result.Data, dto => Assert.NotNull(dto.Value));
        }

        [Fact]
        public async Task GetByIdAsync_WithCacheHit_ReturnsCachedData()
        {
            // Arrange
            var metricTypeId = MetricTypeId.New();
            var cachedDto = new ReferenceDataDto
            {
                Id = metricTypeId.ToString(),
                Value = "Weight",
                Description = "Weight measurement in kg or lbs"
            };

            _mockCacheService
                .Setup(x => x.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<ReferenceDataDto>.Hit(cachedDto));

            // Act
            var result = await _metricTypeService.GetByIdAsync(metricTypeId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(cachedDto.Id, result.Data.Id);
            Assert.Equal(cachedDto.Value, result.Data.Value);
            Assert.Equal(cachedDto.Description, result.Data.Description);
            _mockMetricTypeRepository.Verify(x => x.GetByIdAsync(It.IsAny<MetricTypeId>()), Times.Never);
        }
    }
}