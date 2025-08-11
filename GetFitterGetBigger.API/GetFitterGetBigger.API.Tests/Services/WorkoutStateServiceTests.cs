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
    public class WorkoutStateServiceTests
    {
        private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _mockUnitOfWorkProvider;
        private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _mockReadOnlyUnitOfWork;
        private readonly Mock<IWorkoutStateRepository> _mockWorkoutStateRepository;
        private readonly Mock<IEternalCacheService> _mockCacheService;
        private readonly Mock<ILogger<WorkoutStateService>> _mockLogger;
        private readonly WorkoutStateService _workoutStateService;

        public WorkoutStateServiceTests()
        {
            _mockUnitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
            _mockReadOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
            _mockWorkoutStateRepository = new Mock<IWorkoutStateRepository>();
            _mockCacheService = new Mock<IEternalCacheService>();
            _mockLogger = new Mock<ILogger<WorkoutStateService>>();

            _mockUnitOfWorkProvider
                .Setup(x => x.CreateReadOnly())
                .Returns(_mockReadOnlyUnitOfWork.Object);

            _mockReadOnlyUnitOfWork
                .Setup(x => x.GetRepository<IWorkoutStateRepository>())
                .Returns(_mockWorkoutStateRepository.Object);

            _workoutStateService = new WorkoutStateService(
                _mockUnitOfWorkProvider.Object,
                _mockCacheService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task ExistsAsync_WithWorkoutStateId_WhenWorkoutStateExists_ReturnsTrue()
        {
            // Arrange
            var workoutStateId = WorkoutStateId.New();
            var createResult = WorkoutState.Handler.Create(workoutStateId, "DRAFT", "Template under construction", 1, true);
            Assert.True(createResult.IsSuccess, "WorkoutState creation should succeed");
            var workoutState = createResult.Value;

            // Setup cache miss so it goes to database
            _mockCacheService
                .Setup(x => x.GetAsync<WorkoutStateDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<WorkoutStateDto>.Miss());

            _mockWorkoutStateRepository
                .Setup(x => x.GetByIdAsync(It.IsAny<WorkoutStateId>()))
                .ReturnsAsync(workoutState);

            // Act
            var result = await _workoutStateService.ExistsAsync(workoutStateId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Data.Value);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            _mockWorkoutStateRepository.Verify(x => x.GetByIdAsync(It.IsAny<WorkoutStateId>()), Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_WithWorkoutStateId_WhenWorkoutStateDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var workoutStateId = WorkoutStateId.New();

            // Setup cache miss so it goes to database
            _mockCacheService
                .Setup(x => x.GetAsync<WorkoutStateDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<WorkoutStateDto>.Miss());

            _mockWorkoutStateRepository
                .Setup(x => x.GetByIdAsync(It.IsAny<WorkoutStateId>()))
                .ReturnsAsync(WorkoutState.Empty);

            // Act
            var result = await _workoutStateService.ExistsAsync(workoutStateId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.Data.Value);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            _mockWorkoutStateRepository.Verify(x => x.GetByIdAsync(It.IsAny<WorkoutStateId>()), Times.Once);
        }

        [Fact]
        public async Task GetAllActiveAsync_ReturnsSuccessWithWorkoutStates()
        {
            // Arrange
            var draftResult = WorkoutState.Handler.Create(WorkoutStateId.New(), "DRAFT", "Template under construction", 1, true);
            var productionResult = WorkoutState.Handler.Create(WorkoutStateId.New(), "PRODUCTION", "Active template for use", 2, true);
            
            Assert.True(draftResult.IsSuccess, "DRAFT WorkoutState creation should succeed");
            Assert.True(productionResult.IsSuccess, "PRODUCTION WorkoutState creation should succeed");
            
            var workoutStates = new List<WorkoutState>
            {
                draftResult.Value,
                productionResult.Value
            };

            // Setup cache miss so it goes to database
            _mockCacheService
                .Setup(x => x.GetAsync<IEnumerable<WorkoutStateDto>>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<IEnumerable<WorkoutStateDto>>.Miss());

            _mockWorkoutStateRepository
                .Setup(x => x.GetAllActiveAsync())
                .ReturnsAsync(workoutStates);

            // Act
            var result = await _workoutStateService.GetAllActiveAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Data.Count());
            
            var resultArray = result.Data.ToArray();
            Assert.Equal("DRAFT", resultArray[0].Value);
            Assert.Equal("PRODUCTION", resultArray[1].Value);
            
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            _mockWorkoutStateRepository.Verify(x => x.GetAllActiveAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_CallsGetAllActiveAsync()
        {
            // Arrange
            var workoutStates = new List<WorkoutState>();

            // Setup cache miss so it goes to database
            _mockCacheService
                .Setup(x => x.GetAsync<IEnumerable<WorkoutStateDto>>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<IEnumerable<WorkoutStateDto>>.Miss());

            _mockWorkoutStateRepository
                .Setup(x => x.GetAllActiveAsync())
                .ReturnsAsync(workoutStates);

            // Act
            var result = await _workoutStateService.GetAllAsync();

            // Assert
            Assert.True(result.IsSuccess);
            _mockWorkoutStateRepository.Verify(x => x.GetAllActiveAsync(), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WithWorkoutStateId_WhenExists_ReturnsWorkoutState()
        {
            // Arrange
            var workoutStateId = WorkoutStateId.New();
            var createResult = WorkoutState.Handler.Create(workoutStateId, "DRAFT", "Template under construction", 1, true);
            Assert.True(createResult.IsSuccess, "WorkoutState creation should succeed");
            var workoutState = createResult.Value;

            // Setup cache miss so it goes to database
            _mockCacheService
                .Setup(x => x.GetAsync<WorkoutStateDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<WorkoutStateDto>.Miss());

            _mockWorkoutStateRepository
                .Setup(x => x.GetByIdAsync(It.IsAny<WorkoutStateId>()))
                .ReturnsAsync(workoutState);

            // Act
            var result = await _workoutStateService.GetByIdAsync(workoutStateId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(workoutStateId.ToString(), result.Data.Id);
            Assert.Equal("DRAFT", result.Data.Value);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            _mockWorkoutStateRepository.Verify(x => x.GetByIdAsync(It.IsAny<WorkoutStateId>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WithWorkoutStateId_WhenDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var workoutStateId = WorkoutStateId.New();

            // Setup cache miss so it goes to database
            _mockCacheService
                .Setup(x => x.GetAsync<WorkoutStateDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<WorkoutStateDto>.Miss());

            _mockWorkoutStateRepository
                .Setup(x => x.GetByIdAsync(It.IsAny<WorkoutStateId>()))
                .ReturnsAsync(WorkoutState.Empty);

            // Act
            var result = await _workoutStateService.GetByIdAsync(workoutStateId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceErrorCode.NotFound, result.PrimaryErrorCode);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            _mockWorkoutStateRepository.Verify(x => x.GetByIdAsync(It.IsAny<WorkoutStateId>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WithStringId_CallsWorkoutStateIdOverload()
        {
            // Arrange
            var workoutStateId = WorkoutStateId.New();
            var workoutStateIdString = workoutStateId.ToString();
            var createResult = WorkoutState.Handler.Create(workoutStateId, "DRAFT", "Template under construction", 1, true);
            Assert.True(createResult.IsSuccess, "WorkoutState creation should succeed");
            var workoutState = createResult.Value;

            // Setup cache miss so it goes to database
            _mockCacheService
                .Setup(x => x.GetAsync<WorkoutStateDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<WorkoutStateDto>.Miss());

            _mockWorkoutStateRepository
                .Setup(x => x.GetByIdAsync(It.IsAny<WorkoutStateId>()))
                .ReturnsAsync(workoutState);

            // Act
            var result = await _workoutStateService.GetByIdAsync(workoutStateIdString);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(workoutStateId.ToString(), result.Data.Id);
            Assert.Equal("DRAFT", result.Data.Value);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            _mockWorkoutStateRepository.Verify(x => x.GetByIdAsync(It.IsAny<WorkoutStateId>()), Times.Once);
        }

        [Fact]
        public async Task GetByValueAsync_WhenExists_ReturnsWorkoutState()
        {
            // Arrange
            var workoutStateId = WorkoutStateId.New();
            var createResult = WorkoutState.Handler.Create(workoutStateId, "DRAFT", "Template under construction", 1, true);
            Assert.True(createResult.IsSuccess, "WorkoutState creation should succeed");
            var workoutState = createResult.Value;

            // Setup cache miss so it goes to database
            _mockCacheService
                .Setup(x => x.GetAsync<WorkoutStateDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<WorkoutStateDto>.Miss());

            _mockWorkoutStateRepository
                .Setup(x => x.GetByValueAsync(It.IsAny<string>()))
                .ReturnsAsync(workoutState);

            // Act
            var result = await _workoutStateService.GetByValueAsync("DRAFT");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("DRAFT", result.Data.Value);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            _mockWorkoutStateRepository.Verify(x => x.GetByValueAsync("DRAFT"), Times.Once);
        }

        [Fact]
        public async Task GetByValueAsync_WhenDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            // Setup cache miss so it goes to database
            _mockCacheService
                .Setup(x => x.GetAsync<WorkoutStateDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<WorkoutStateDto>.Miss());

            _mockWorkoutStateRepository
                .Setup(x => x.GetByValueAsync(It.IsAny<string>()))
                .ReturnsAsync(WorkoutState.Empty);

            // Act
            var result = await _workoutStateService.GetByValueAsync("NONEXISTENT");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceErrorCode.NotFound, result.PrimaryErrorCode);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            _mockWorkoutStateRepository.Verify(x => x.GetByValueAsync("NONEXISTENT"), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WithEmptyId_ReturnsValidationError()
        {
            // Act
            var result = await _workoutStateService.GetByIdAsync(WorkoutStateId.Empty);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Never);
        }

        [Fact]
        public async Task GetByValueAsync_WithWhitespaceValue_ReturnsValidationError()
        {
            // Act
            var result = await _workoutStateService.GetByValueAsync("   ");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Never);
        }

        [Fact]
        public async Task ExistsAsync_WithEmptyId_ReturnsValidationError()
        {
            // Act
            var result = await _workoutStateService.ExistsAsync(WorkoutStateId.Empty);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_WhenCacheHit_DoesNotHitDatabase()
        {
            // Arrange
            var workoutStateId = WorkoutStateId.New();
            var cachedDto = new WorkoutStateDto
            {
                Id = workoutStateId.ToString(),
                Value = "DRAFT",
                Description = "Template under construction"
            };

            // Setup cache hit
            _mockCacheService
                .Setup(x => x.GetAsync<WorkoutStateDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<WorkoutStateDto>.Hit(cachedDto));

            // Act
            var result = await _workoutStateService.GetByIdAsync(workoutStateId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("DRAFT", result.Data.Value);
            
            // Should not hit database when cache hits
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Never);
            _mockWorkoutStateRepository.Verify(x => x.GetByIdAsync(It.IsAny<WorkoutStateId>()), Times.Never);
        }

        [Fact]
        public async Task GetAllActiveAsync_WhenCacheHit_DoesNotHitDatabase()
        {
            // Arrange
            var cachedDtos = new List<WorkoutStateDto>
            {
                new() { Id = WorkoutStateId.New().ToString(), Value = "DRAFT", Description = "Template under construction" },
                new() { Id = WorkoutStateId.New().ToString(), Value = "PRODUCTION", Description = "Active template for use" }
            };

            // Setup cache hit
            _mockCacheService
                .Setup(x => x.GetAsync<IEnumerable<WorkoutStateDto>>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<IEnumerable<WorkoutStateDto>>.Hit(cachedDtos));

            // Act
            var result = await _workoutStateService.GetAllActiveAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Data.Count());
            
            // Should not hit database when cache hits
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Never);
            _mockWorkoutStateRepository.Verify(x => x.GetAllActiveAsync(), Times.Never);
        }
    }
}