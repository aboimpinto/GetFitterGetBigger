using System;
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
using Microsoft.Extensions.Logging;
using Moq;
using Olimpo.EntityFramework.Persistency;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services
{
    public class DifficultyLevelServiceTests
    {
        private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _mockUnitOfWorkProvider;
        private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _mockReadOnlyUnitOfWork;
        private readonly Mock<IDifficultyLevelRepository> _mockDifficultyLevelRepository;
        private readonly Mock<IEternalCacheService> _mockCacheService;
        private readonly Mock<ILogger<DifficultyLevelService>> _mockLogger;
        private readonly DifficultyLevelService _difficultyLevelService;

        public DifficultyLevelServiceTests()
        {
            _mockUnitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
            _mockReadOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
            _mockDifficultyLevelRepository = new Mock<IDifficultyLevelRepository>();
            _mockCacheService = new Mock<IEternalCacheService>();
            _mockLogger = new Mock<ILogger<DifficultyLevelService>>();

            _mockUnitOfWorkProvider
                .Setup(x => x.CreateReadOnly())
                .Returns(_mockReadOnlyUnitOfWork.Object);

            _mockReadOnlyUnitOfWork
                .Setup(x => x.GetRepository<IDifficultyLevelRepository>())
                .Returns(_mockDifficultyLevelRepository.Object);

            _difficultyLevelService = new DifficultyLevelService(
                _mockUnitOfWorkProvider.Object,
                _mockCacheService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task ExistsAsync_WithDifficultyLevelId_WhenDifficultyLevelExists_ReturnsTrue()
        {
            // Arrange
            var difficultyLevelId = DifficultyLevelId.New();
            var difficultyLevel = DifficultyLevel.Handler.Create(
                difficultyLevelId,
                "Beginner",
                "Suitable for beginners",
                1,
                true).Value;

            _mockCacheService
                .Setup(x => x.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());

            _mockDifficultyLevelRepository
                .Setup(x => x.GetByIdAsync(difficultyLevelId))
                .ReturnsAsync(difficultyLevel);

            // Act
            var result = await _difficultyLevelService.ExistsAsync(difficultyLevelId);

            // Assert
            Assert.True(result);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            _mockDifficultyLevelRepository.Verify(x => x.GetByIdAsync(difficultyLevelId), Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_WithDifficultyLevelId_WhenDifficultyLevelDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var difficultyLevelId = DifficultyLevelId.New();

            _mockCacheService
                .Setup(x => x.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());

            _mockDifficultyLevelRepository
                .Setup(x => x.GetByIdAsync(difficultyLevelId))
                .ReturnsAsync(DifficultyLevel.Empty);

            // Act
            var result = await _difficultyLevelService.ExistsAsync(difficultyLevelId);

            // Assert
            Assert.False(result);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            _mockDifficultyLevelRepository.Verify(x => x.GetByIdAsync(difficultyLevelId), Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_WithEmptyDifficultyLevelId_ReturnsFalse()
        {
            // Arrange
            var emptyId = DifficultyLevelId.Empty;

            // Act
            var result = await _difficultyLevelService.ExistsAsync(emptyId);

            // Assert
            Assert.False(result);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Never);
            _mockDifficultyLevelRepository.Verify(x => x.GetByIdAsync(It.IsAny<DifficultyLevelId>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_WhenCacheMiss_LoadsFromDatabase()
        {
            // Arrange
            var difficultyLevelId = DifficultyLevelId.New();
            var difficultyLevel = DifficultyLevel.Handler.Create(
                difficultyLevelId,
                "Intermediate",
                "For intermediate users",
                2,
                true).Value;

            _mockCacheService
                .Setup(x => x.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());

            _mockDifficultyLevelRepository
                .Setup(x => x.GetByIdAsync(difficultyLevelId))
                .ReturnsAsync(difficultyLevel);

            _mockCacheService
                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<ReferenceDataDto>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _difficultyLevelService.GetByIdAsync(difficultyLevelId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Intermediate", result.Data.Value);
            Assert.Equal("For intermediate users", result.Data.Description);
            _mockCacheService.Verify(x => x.SetAsync<ReferenceDataDto>(It.IsAny<string>(), It.IsAny<ReferenceDataDto>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_WhenCacheHit_ReturnsFromCache()
        {
            // Arrange
            var difficultyLevelId = DifficultyLevelId.New();
            var cachedDto = new ReferenceDataDto
            {
                Id = difficultyLevelId.ToString(),
                Value = "Advanced",
                Description = "For advanced users"
            };

            _mockCacheService
                .Setup(x => x.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<ReferenceDataDto>.Hit(cachedDto));

            // Act
            var result = await _difficultyLevelService.GetByIdAsync(difficultyLevelId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Advanced", result.Data.Value);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Never);
            _mockDifficultyLevelRepository.Verify(x => x.GetByIdAsync(It.IsAny<DifficultyLevelId>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_WithEmptyId_ReturnsValidationFailure()
        {
            // Arrange
            var emptyId = DifficultyLevelId.Empty;

            // Act
            var result = await _difficultyLevelService.GetByIdAsync(emptyId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
            Assert.Contains(DifficultyLevelErrorMessages.InvalidIdFormat, result.Errors.First());
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Never);
        }

        [Fact]
        public async Task GetByValueAsync_WithValidValue_WhenExists_ReturnsSuccess()
        {
            // Arrange
            var value = "Expert";
            var difficultyLevel = DifficultyLevel.Handler.Create(
                DifficultyLevelId.New(),
                value,
                "For experts only",
                4,
                true).Value;

            _mockCacheService
                .Setup(x => x.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());

            _mockDifficultyLevelRepository
                .Setup(x => x.GetByValueAsync(value))
                .ReturnsAsync(difficultyLevel);

            _mockCacheService
                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<ReferenceDataDto>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _difficultyLevelService.GetByValueAsync(value);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(value, result.Data.Value);
            _mockDifficultyLevelRepository.Verify(x => x.GetByValueAsync(value), Times.Once);
        }

        [Fact]
        public async Task GetByValueAsync_WithEmptyValue_ReturnsValidationFailure()
        {
            // Arrange
            var emptyValue = "";

            // Act
            var result = await _difficultyLevelService.GetByValueAsync(emptyValue);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
            Assert.Contains(DifficultyLevelErrorMessages.ValueCannotBeEmpty, result.Errors.First());
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Never);
        }

        [Fact]
        public async Task GetByValueAsync_WithNonExistentValue_ReturnsNotFound()
        {
            // Arrange
            var value = "NonExistent";

            _mockCacheService
                .Setup(x => x.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());

            _mockDifficultyLevelRepository
                .Setup(x => x.GetByValueAsync(value))
                .ReturnsAsync(DifficultyLevel.Empty);

            // Act
            var result = await _difficultyLevelService.GetByValueAsync(value);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceErrorCode.NotFound, result.PrimaryErrorCode);
            _mockDifficultyLevelRepository.Verify(x => x.GetByValueAsync(value), Times.Once);
        }

        [Fact]
        public async Task GetAllActiveAsync_WhenCacheMiss_LoadsFromDatabase()
        {
            // Arrange
            var difficultyLevels = new List<DifficultyLevel>
            {
                DifficultyLevel.Handler.Create(DifficultyLevelId.New(), "Beginner", "For beginners", 1, true).Value,
                DifficultyLevel.Handler.Create(DifficultyLevelId.New(), "Intermediate", "For intermediate users", 2, true).Value,
                DifficultyLevel.Handler.Create(DifficultyLevelId.New(), "Advanced", "For advanced users", 3, true).Value
            };

            _mockCacheService
                .Setup(x => x.GetAsync<IEnumerable<ReferenceDataDto>>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<IEnumerable<ReferenceDataDto>>.Miss());

            _mockDifficultyLevelRepository
                .Setup(x => x.GetAllActiveAsync())
                .ReturnsAsync(difficultyLevels);

            _mockCacheService
                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ReferenceDataDto>>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _difficultyLevelService.GetAllActiveAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(3, result.Data.Count());
            var values = result.Data.Select(d => d.Value).ToList();
            Assert.Contains("Beginner", values);
            Assert.Contains("Intermediate", values);
            Assert.Contains("Advanced", values);
            _mockCacheService.Verify(x => x.SetAsync<List<ReferenceDataDto>>(It.IsAny<string>(), It.IsAny<List<ReferenceDataDto>>()), Times.Once);
        }

        [Fact]
        public async Task GetAllActiveAsync_WithInactiveLevel_OnlyReturnsActiveOnes()
        {
            // Arrange
            var difficultyLevels = new List<DifficultyLevel>
            {
                DifficultyLevel.Handler.Create(DifficultyLevelId.New(), "Beginner", "For beginners", 1, true).Value,
                DifficultyLevel.Handler.Create(DifficultyLevelId.New(), "Intermediate", "For intermediate users", 2, true).Value
            };

            _mockCacheService
                .Setup(x => x.GetAsync<IEnumerable<ReferenceDataDto>>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<IEnumerable<ReferenceDataDto>>.Miss());

            _mockDifficultyLevelRepository
                .Setup(x => x.GetAllActiveAsync())
                .ReturnsAsync(difficultyLevels);

            _mockCacheService
                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ReferenceDataDto>>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _difficultyLevelService.GetAllActiveAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Data.Count());
            _mockDifficultyLevelRepository.Verify(x => x.GetAllActiveAsync(), Times.Once);
        }
    }
}