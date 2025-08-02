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
using GetFitterGetBigger.API.Tests.TestConstants;
using GetFitterGetBigger.API.Tests.TestBuilders;

namespace GetFitterGetBigger.API.Tests.Services
{
    public class MovementPatternServiceTests
    {
        private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _mockUnitOfWorkProvider;
        private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _mockReadOnlyUnitOfWork;
        private readonly Mock<IMovementPatternRepository> _mockMovementPatternRepository;
        private readonly Mock<IEternalCacheService> _mockCacheService;
        private readonly Mock<ILogger<MovementPatternService>> _mockLogger;
        private readonly MovementPatternService _movementPatternService;

        public MovementPatternServiceTests()
        {
            _mockUnitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
            _mockReadOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
            _mockMovementPatternRepository = new Mock<IMovementPatternRepository>();
            _mockCacheService = new Mock<IEternalCacheService>();
            _mockLogger = new Mock<ILogger<MovementPatternService>>();

            _mockUnitOfWorkProvider
                .Setup(x => x.CreateReadOnly())
                .Returns(_mockReadOnlyUnitOfWork.Object);

            _mockReadOnlyUnitOfWork
                .Setup(x => x.GetRepository<IMovementPatternRepository>())
                .Returns(_mockMovementPatternRepository.Object);

            _movementPatternService = new MovementPatternService(
                _mockUnitOfWorkProvider.Object,
                _mockCacheService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllActiveAsync_WithCacheMiss_LoadsFromRepositoryAndCaches()
        {
            // Arrange
            var movementPatterns = new List<MovementPattern>
            {
                MovementPattern.Handler.Create(
                    MovementPatternId.New(),
                    MovementPatternTestConstants.HorizontalPushName,
                    MovementPatternTestConstants.PushingForwardDescription,
                    MovementPatternTestConstants.TestDisplayOrder,
                    true).Value,
                MovementPattern.Handler.Create(
                    MovementPatternId.New(),
                    MovementPatternTestConstants.VerticalPullName,
                    MovementPatternTestConstants.PullingDownwardDescription,
                    MovementPatternTestConstants.UpdatedDisplayOrder,
                    true).Value
            };

            _mockCacheService
                .Setup(x => x.GetAsync<IEnumerable<ReferenceDataDto>>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<IEnumerable<ReferenceDataDto>>.Miss());

            _mockMovementPatternRepository
                .Setup(x => x.GetAllActiveAsync())
                .ReturnsAsync(movementPatterns);

            // Act
            var result = await _movementPatternService.GetAllActiveAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count());
            
            _mockCacheService.Verify(x => x.SetAsync(
                It.IsAny<string>(), 
                It.IsAny<IEnumerable<ReferenceDataDto>>()), Times.Once);
            
            _mockMovementPatternRepository.Verify(x => x.GetAllActiveAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllActiveAsync_WithCacheHit_ReturnsFromCache()
        {
            // Arrange
            var cachedDtos = new List<ReferenceDataDto>
            {
                new() { Id = TestIds.MovementPatternIds.Push, Value = MovementPatternTestConstants.HorizontalPushName, Description = MovementPatternTestConstants.PushingForwardDescription },
                new() { Id = TestIds.MovementPatternIds.Pull, Value = MovementPatternTestConstants.VerticalPullName, Description = MovementPatternTestConstants.PullingDownwardDescription }
            };

            _mockCacheService
                .Setup(x => x.GetAsync<IEnumerable<ReferenceDataDto>>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<IEnumerable<ReferenceDataDto>>.Hit(cachedDtos));

            // Act
            var result = await _movementPatternService.GetAllActiveAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count());
            
            _mockMovementPatternRepository.Verify(x => x.GetAllActiveAsync(), Times.Never);
            _mockCacheService.Verify(x => x.SetAsync(
                It.IsAny<string>(), 
                It.IsAny<IEnumerable<ReferenceDataDto>>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsMovementPattern()
        {
            // Arrange
            var movementPatternId = MovementPatternId.New();
            var movementPattern = MovementPattern.Handler.Create(
                movementPatternId,
                MovementPatternTestConstants.HorizontalPushName,
                MovementPatternTestConstants.PushingForwardDescription,
                MovementPatternTestConstants.TestDisplayOrder,
                true).Value;

            _mockCacheService
                .Setup(x => x.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());

            _mockMovementPatternRepository
                .Setup(x => x.GetByIdAsync(movementPatternId))
                .ReturnsAsync(movementPattern);

            // Act
            var result = await _movementPatternService.GetByIdAsync(movementPatternId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(MovementPatternTestConstants.HorizontalPushName, result.Data.Value);
            Assert.Equal(MovementPatternTestConstants.PushingForwardDescription, result.Data.Description);
            
            _mockMovementPatternRepository.Verify(x => x.GetByIdAsync(movementPatternId), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WithEmptyId_ReturnsValidationFailure()
        {
            // Arrange
            var emptyId = MovementPatternId.Empty;
            // No need to setup repository - service returns ValidationFailed immediately for empty IDs

            // Act
            var result = await _movementPatternService.GetByIdAsync(emptyId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
            Assert.Contains(MovementPatternErrorMessages.InvalidIdFormat, result.Errors);
            // Verify the repository was NOT called (optimization - empty IDs are rejected immediately)
            _mockMovementPatternRepository.Verify(x => x.GetByIdAsync(It.IsAny<MovementPatternId>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_WithEmptyString_ReturnsValidationFailure()
        {
            // Arrange
            var emptyId = MovementPatternTestConstants.EmptyString;
            // Note: The service only validates for null/empty. Format validation 
            // is handled by the controller and MovementPatternId.ParseOrEmpty()

            // Act
            var result = await _movementPatternService.GetByIdAsync(emptyId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
            Assert.Contains("Movement pattern ID cannot be empty", result.Errors);
            _mockMovementPatternRepository.Verify(x => x.GetByIdAsync(It.IsAny<MovementPatternId>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_WithNullString_ReturnsValidationFailure()
        {
            // Arrange
            string? nullId = null;

            // Act
            var result = await _movementPatternService.GetByIdAsync(nullId!);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
            Assert.Contains("Movement pattern ID cannot be empty", result.Errors);
            _mockMovementPatternRepository.Verify(x => x.GetByIdAsync(It.IsAny<MovementPatternId>()), Times.Never);
        }

        [Fact]
        public async Task GetByValueAsync_WithValidValue_ReturnsMovementPattern()
        {
            // Arrange
            var value = MovementPatternTestConstants.HorizontalPushName;
            var movementPattern = MovementPattern.Handler.Create(
                MovementPatternId.New(),
                value,
                MovementPatternTestConstants.PushingForwardDescription,
                MovementPatternTestConstants.TestDisplayOrder,
                true).Value;

            _mockCacheService
                .Setup(x => x.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());

            _mockMovementPatternRepository
                .Setup(x => x.GetByValueAsync(value))
                .ReturnsAsync(movementPattern);

            // Act
            var result = await _movementPatternService.GetByValueAsync(value);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(value, result.Data.Value);
            
            _mockMovementPatternRepository.Verify(x => x.GetByValueAsync(value), Times.Once);
        }

        [Fact]
        public async Task GetByValueAsync_WithEmptyValue_ReturnsFailure()
        {
            // Act
            var result = await _movementPatternService.GetByValueAsync(MovementPatternTestConstants.EmptyString);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(MovementPatternErrorMessages.ValueCannotBeEmpty, string.Join(", ", result.Errors));
            
            _mockMovementPatternRepository.Verify(x => x.GetByValueAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetByValueAsync_WhenNotFound_ReturnsFailure()
        {
            // Arrange
            var value = "NonExistent";

            _mockCacheService
                .Setup(x => x.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());

            _mockMovementPatternRepository
                .Setup(x => x.GetByValueAsync(value))
                .ReturnsAsync(MovementPattern.Empty);

            // Act
            var result = await _movementPatternService.GetByValueAsync(value);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains($"Movement pattern with value '{value}' not found", string.Join(", ", result.Errors));
        }

        [Fact]
        public async Task ExistsAsync_WithMovementPatternId_WhenExists_ReturnsTrue()
        {
            // Arrange
            var movementPatternId = MovementPatternId.New();
            var movementPattern = MovementPattern.Handler.Create(
                movementPatternId,
                MovementPatternTestConstants.HorizontalPushName,
                MovementPatternTestConstants.PushingForwardDescription,
                MovementPatternTestConstants.TestDisplayOrder,
                true).Value;

            _mockCacheService
                .Setup(x => x.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());

            _mockMovementPatternRepository
                .Setup(x => x.GetByIdAsync(movementPatternId))
                .ReturnsAsync(movementPattern);

            // Act
            var result = await _movementPatternService.ExistsAsync(movementPatternId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WithMovementPatternId_WhenDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var movementPatternId = MovementPatternId.New();

            _mockCacheService
                .Setup(x => x.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());

            _mockMovementPatternRepository
                .Setup(x => x.GetByIdAsync(movementPatternId))
                .ReturnsAsync(MovementPattern.Empty);

            // Act
            var result = await _movementPatternService.ExistsAsync(movementPatternId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_WithStringId_WhenValid_ReturnsTrue()
        {
            // Arrange
            var movementPatternId = MovementPatternId.New();
            var stringId = movementPatternId.ToString();
            var movementPattern = MovementPattern.Handler.Create(
                movementPatternId,
                MovementPatternTestConstants.HorizontalPushName,
                MovementPatternTestConstants.PushingForwardDescription,
                MovementPatternTestConstants.TestDisplayOrder,
                true).Value;

            _mockCacheService
                .Setup(x => x.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());

            _mockMovementPatternRepository
                .Setup(x => x.GetByIdAsync(movementPatternId))
                .ReturnsAsync(movementPattern);

            // Act
            var result = await _movementPatternService.ExistsAsync(stringId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WithNonEmptyInvalidFormat_ReturnsFalse()
        {
            // Arrange
            var invalidFormatId = MovementPatternTestConstants.InvalidFormatId;
            // Note: Service allows any non-empty string to pass validation.
            // MovementPatternId.ParseOrEmpty() will convert this to Empty,
            // causing ExistsAsync to return false.

            // Act
            var result = await _movementPatternService.ExistsAsync(invalidFormatId);

            // Assert
            Assert.False(result);
        }
    }
}