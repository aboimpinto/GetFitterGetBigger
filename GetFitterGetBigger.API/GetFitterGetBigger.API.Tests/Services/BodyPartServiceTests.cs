using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Implementations;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Olimpo.EntityFramework.Persistency;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services
{
    public class BodyPartServiceTests
    {
        private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _mockUnitOfWorkProvider;
        private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _mockReadOnlyUnitOfWork;
        private readonly Mock<IBodyPartRepository> _mockBodyPartRepository;
        private readonly Mock<IEmptyEnabledCacheService> _mockCacheService;
        private readonly Mock<ILogger<BodyPartService>> _mockLogger;
        private readonly BodyPartService _bodyPartService;

        public BodyPartServiceTests()
        {
            _mockUnitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
            _mockReadOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
            _mockBodyPartRepository = new Mock<IBodyPartRepository>();
            _mockCacheService = new Mock<IEmptyEnabledCacheService>();
            _mockLogger = new Mock<ILogger<BodyPartService>>();

            _mockUnitOfWorkProvider
                .Setup(x => x.CreateReadOnly())
                .Returns(_mockReadOnlyUnitOfWork.Object);

            _mockReadOnlyUnitOfWork
                .Setup(x => x.GetRepository<IBodyPartRepository>())
                .Returns(_mockBodyPartRepository.Object);

            _bodyPartService = new BodyPartService(
                _mockUnitOfWorkProvider.Object,
                _mockCacheService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task ExistsAsync_WithBodyPartId_WhenBodyPartExists_ReturnsTrue()
        {
            // Arrange
            var bodyPartId = BodyPartId.New();
            var bodyPart = BodyPart.Handler.Create(
                bodyPartId,
                "Chest",
                "Chest muscles",
                1,
                true);

            _mockCacheService
                .Setup(x => x.GetAsync<BodyPartDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<BodyPartDto>.Miss());

            _mockBodyPartRepository
                .Setup(x => x.GetByIdAsync(bodyPartId))
                .ReturnsAsync(bodyPart);

            // Act
            var result = await _bodyPartService.ExistsAsync(bodyPartId);

            // Assert
            Assert.True(result);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            _mockBodyPartRepository.Verify(x => x.GetByIdAsync(bodyPartId), Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_WithBodyPartId_WhenBodyPartDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var bodyPartId = BodyPartId.New();

            _mockCacheService
                .Setup(x => x.GetAsync<BodyPartDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<BodyPartDto>.Miss());

            _mockBodyPartRepository
                .Setup(x => x.GetByIdAsync(bodyPartId))
                .ReturnsAsync(BodyPart.Empty);

            // Act
            var result = await _bodyPartService.ExistsAsync(bodyPartId);

            // Assert
            Assert.False(result);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            _mockBodyPartRepository.Verify(x => x.GetByIdAsync(bodyPartId), Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_WithStringId_WhenBodyPartExists_ReturnsTrue()
        {
            // Arrange
            var bodyPartId = BodyPartId.New();
            var bodyPartIdString = bodyPartId.ToString();
            var bodyPartDto = new BodyPartDto
            {
                Id = bodyPartIdString,
                Value = "Chest",
                Description = "Chest muscles"
            };

            _mockCacheService
                .Setup(x => x.GetAsync<BodyPartDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<BodyPartDto>.Hit(bodyPartDto));

            // Act
            var result = await _bodyPartService.ExistsAsync(bodyPartIdString);

            // Assert
            Assert.True(result);
            _mockCacheService.Verify(x => x.GetAsync<BodyPartDto>(It.IsAny<string>()), Times.Once);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Never);
        }

        [Fact]
        public async Task GetAllActiveAsync_ReturnsSuccessWithBodyParts()
        {
            // Arrange
            var bodyParts = new List<BodyPart>
            {
                BodyPart.Handler.Create(BodyPartId.New(), "Chest", "Chest muscles", 1, true),
                BodyPart.Handler.Create(BodyPartId.New(), "Back", "Back muscles", 2, true)
            };

            _mockCacheService
                .Setup(x => x.GetAsync<IEnumerable<BodyPartDto>>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<IEnumerable<BodyPartDto>>.Miss());

            _mockBodyPartRepository
                .Setup(x => x.GetAllActiveAsync())
                .ReturnsAsync(bodyParts);

            _mockCacheService
                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<IEnumerable<BodyPartDto>>(), It.IsAny<TimeSpan>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _bodyPartService.GetAllActiveAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count());
            Assert.Empty(result.Errors);
            _mockBodyPartRepository.Verify(x => x.GetAllActiveAsync(), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsSuccessWithBodyPart()
        {
            // Arrange
            var bodyPartId = BodyPartId.New();
            var bodyPartIdString = bodyPartId.ToString();
            var bodyPart = BodyPart.Handler.Create(
                bodyPartId,
                "Chest",
                "Chest muscles",
                1,
                true);

            _mockCacheService
                .Setup(x => x.GetAsync<BodyPartDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<BodyPartDto>.Miss());

            _mockBodyPartRepository
                .Setup(x => x.GetByIdAsync(bodyPartId))
                .ReturnsAsync(bodyPart);

            _mockCacheService
                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<BodyPartDto>(), It.IsAny<TimeSpan>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _bodyPartService.GetByIdAsync(bodyPartIdString);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(bodyPartIdString, result.Data.Id);
            Assert.Equal("Chest", result.Data.Value);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task GetByIdAsync_WithBodyPartId_ReturnsSuccessWithBodyPart()
        {
            // Arrange
            var bodyPartId = BodyPartId.New();
            var bodyPart = BodyPart.Handler.Create(
                bodyPartId,
                "Back",
                "Back muscles",
                2,
                true);

            _mockCacheService
                .Setup(x => x.GetAsync<BodyPartDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<BodyPartDto>.Miss());

            _mockBodyPartRepository
                .Setup(x => x.GetByIdAsync(bodyPartId))
                .ReturnsAsync(bodyPart);

            _mockCacheService
                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<BodyPartDto>(), It.IsAny<TimeSpan>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _bodyPartService.GetByIdAsync(bodyPartId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(bodyPartId.ToString(), result.Data.Id);
            Assert.Equal("Back", result.Data.Value);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task GetByIdAsync_WithEmptyBodyPartId_ReturnsValidationFailure()
        {
            // Arrange
            var emptyBodyPartId = BodyPartId.Empty;
            // No need to setup repository - service returns ValidationFailed immediately for empty IDs

            // Act
            var result = await _bodyPartService.GetByIdAsync(emptyBodyPartId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
            Assert.Contains("Invalid body part ID format", result.Errors);
            // Verify the repository was NOT called (optimization - empty IDs are rejected immediately)
            _mockBodyPartRepository.Verify(x => x.GetByIdAsync(It.IsAny<BodyPartId>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_WithEmptyString_ReturnsValidationFailure()
        {
            // Arrange
            var emptyId = "";
            // Note: The service only validates for null/empty. Format validation 
            // is handled by the controller and BodyPartId.ParseOrEmpty()

            // Act
            var result = await _bodyPartService.GetByIdAsync(emptyId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
            Assert.Contains("ID cannot be empty", result.Errors);
            _mockBodyPartRepository.Verify(x => x.GetByIdAsync(It.IsAny<BodyPartId>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_WithNullString_ReturnsValidationFailure()
        {
            // Arrange
            string nullId = null;

            // Act
            var result = await _bodyPartService.GetByIdAsync(nullId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
            Assert.Contains("ID cannot be empty", result.Errors);
            _mockBodyPartRepository.Verify(x => x.GetByIdAsync(It.IsAny<BodyPartId>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_WithInactiveBodyPart_ReturnsNotFound()
        {
            // Arrange
            var bodyPartId = BodyPartId.New();
            var inactiveBodyPart = BodyPart.Handler.Create(
                bodyPartId,
                "Chest",
                "Chest muscles",
                1,
                false); // IsActive = false

            _mockCacheService
                .Setup(x => x.GetAsync<BodyPartDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<BodyPartDto>.Miss());

            _mockBodyPartRepository
                .Setup(x => x.GetByIdAsync(bodyPartId))
                .ReturnsAsync(inactiveBodyPart);

            // Act
            var result = await _bodyPartService.GetByIdAsync(bodyPartId.ToString());

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.NotEmpty(result.Errors);
            Assert.Contains("not found", result.Errors[0]);
            _mockBodyPartRepository.Verify(x => x.GetByIdAsync(bodyPartId), Times.Once);
        }

        [Fact]
        public async Task GetByValueAsync_WithExistingValue_ReturnsSuccess()
        {
            // Arrange
            var value = "Chest";
            var bodyPartId = BodyPartId.New();
            var bodyPart = BodyPart.Handler.Create(
                bodyPartId,
                value,
                "Chest muscles",
                1,
                true);

            _mockCacheService
                .Setup(x => x.GetAsync<BodyPartDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<BodyPartDto>.Miss());

            _mockBodyPartRepository
                .Setup(x => x.GetByValueAsync(value))
                .ReturnsAsync(bodyPart);

            _mockCacheService
                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<BodyPartDto>(), It.IsAny<TimeSpan>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _bodyPartService.GetByValueAsync(value);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(value, result.Data.Value);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task GetByValueAsync_WithNonExistingValue_ReturnsFailure()
        {
            // Arrange
            var value = "NonExistent";

            _mockCacheService
                .Setup(x => x.GetAsync<BodyPartDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<BodyPartDto>.Miss());

            _mockBodyPartRepository
                .Setup(x => x.GetByValueAsync(value))
                .ReturnsAsync(BodyPart.Empty);

            // Act
            var result = await _bodyPartService.GetByValueAsync(value);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.NotEmpty(result.Errors);
            Assert.Contains("not found", result.Errors[0]);
        }

        [Fact]
        public async Task GetByValueAsync_WithInactiveBodyPart_ReturnsNotFound()
        {
            // Arrange
            var value = "InactiveBodyPart";
            var inactiveBodyPart = BodyPart.Handler.Create(
                BodyPartId.New(),
                value,
                "Inactive body part",
                1,
                false); // IsActive = false

            _mockCacheService
                .Setup(x => x.GetAsync<BodyPartDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<BodyPartDto>.Miss());

            _mockBodyPartRepository
                .Setup(x => x.GetByValueAsync(value))
                .ReturnsAsync(inactiveBodyPart);

            // Act
            var result = await _bodyPartService.GetByValueAsync(value);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.NotEmpty(result.Errors);
            Assert.Contains("not found", result.Errors[0]);
            _mockBodyPartRepository.Verify(x => x.GetByValueAsync(value), Times.Once);
        }
    }
}