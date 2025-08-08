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
    public class ExerciseTypeServiceTests
    {
        private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _mockUnitOfWorkProvider;
        private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _mockReadOnlyUnitOfWork;
        private readonly Mock<IExerciseTypeRepository> _mockExerciseTypeRepository;
        private readonly Mock<IEternalCacheService> _mockCacheService;
        private readonly Mock<ILogger<ExerciseTypeService>> _mockLogger;
        private readonly ExerciseTypeService _exerciseTypeService;

        public ExerciseTypeServiceTests()
        {
            _mockUnitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
            _mockReadOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
            _mockExerciseTypeRepository = new Mock<IExerciseTypeRepository>();
            _mockCacheService = new Mock<IEternalCacheService>();
            _mockLogger = new Mock<ILogger<ExerciseTypeService>>();

            _mockUnitOfWorkProvider
                .Setup(x => x.CreateReadOnly())
                .Returns(_mockReadOnlyUnitOfWork.Object);

            _mockReadOnlyUnitOfWork
                .Setup(x => x.GetRepository<IExerciseTypeRepository>())
                .Returns(_mockExerciseTypeRepository.Object);

            _exerciseTypeService = new ExerciseTypeService(
                _mockUnitOfWorkProvider.Object,
                _mockCacheService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllActiveAsync_ReturnsAllActiveExerciseTypes()
        {
            // Arrange
            var exerciseTypes = new List<ExerciseType>
            {
                ExerciseType.Handler.Create(ExerciseTypeId.New(), "Warmup", "Warmup exercises", 1, true).Value,
                ExerciseType.Handler.Create(ExerciseTypeId.New(), "Workout", "Main workout", 2, true).Value,
                ExerciseType.Handler.Create(ExerciseTypeId.New(), "Rest", "Rest period", 3, true).Value
            };

            _mockExerciseTypeRepository
                .Setup(x => x.GetAllActiveAsync())
                .ReturnsAsync(exerciseTypes);

            _mockCacheService
                .Setup(x => x.GetAsync<IEnumerable<ReferenceDataDto>>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<IEnumerable<ReferenceDataDto>>.Miss());

            // Act
            var result = await _exerciseTypeService.GetAllActiveAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsExerciseType()
        {
            // Arrange
            var exerciseTypeId = ExerciseTypeId.New();
            var exerciseType = ExerciseType.Handler.Create(
                exerciseTypeId,
                "Strength",
                "Strength training exercises",
                1,
                true).Value;

            _mockExerciseTypeRepository
                .Setup(x => x.GetByIdAsync(exerciseTypeId))
                .ReturnsAsync(exerciseType);

            _mockCacheService
                .Setup(x => x.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());

            // Act
            var result = await _exerciseTypeService.GetByIdAsync(exerciseTypeId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(exerciseTypeId.ToString(), result.Data.Id);
            Assert.Equal("Strength", result.Data.Value);
        }

        [Fact]
        public async Task GetByIdAsync_WithEmptyId_ReturnsValidationError()
        {
            // Arrange
            var emptyId = ExerciseTypeId.Empty;

            // Act
            var result = await _exerciseTypeService.GetByIdAsync(emptyId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
        }

        [Fact]
        public async Task GetByValueAsync_WithValidValue_ReturnsExerciseType()
        {
            // Arrange
            var value = "Warmup";
            var exerciseType = ExerciseType.Handler.Create(
                ExerciseTypeId.New(),
                value,
                "Warmup exercises",
                1,
                true).Value;

            _mockExerciseTypeRepository
                .Setup(x => x.GetByValueAsync(value))
                .ReturnsAsync(exerciseType);

            _mockCacheService
                .Setup(x => x.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());

            // Act
            var result = await _exerciseTypeService.GetByValueAsync(value);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(value, result.Data.Value);
        }

        [Fact]
        public async Task GetByValueAsync_WithEmptyValue_ReturnsValidationError()
        {
            // Arrange
            var value = "";

            // Act
            var result = await _exerciseTypeService.GetByValueAsync(value);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
        }

        [Fact]
        public async Task ExistsAsync_WhenExerciseTypeExists_ReturnsTrue()
        {
            // Arrange
            var exerciseTypeId = ExerciseTypeId.New();

            _mockExerciseTypeRepository
                .Setup(x => x.ExistsAsync(It.IsAny<ExerciseTypeId>()))
                .ReturnsAsync(true);

            // Act
            var result = await _exerciseTypeService.ExistsAsync(exerciseTypeId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task ExistsAsync_WhenExerciseTypeDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var exerciseTypeId = ExerciseTypeId.New();

            _mockExerciseTypeRepository
                .Setup(x => x.ExistsAsync(It.IsAny<ExerciseTypeId>()))
                .ReturnsAsync(false);

            // Act
            var result = await _exerciseTypeService.ExistsAsync(exerciseTypeId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.Data);
        }

        [Fact]
        public async Task ExistsAsync_WithEmptyId_ReturnsFalse()
        {
            // Arrange
            var emptyId = ExerciseTypeId.Empty;

            // Act
            var result = await _exerciseTypeService.ExistsAsync(emptyId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceErrorCode.InvalidFormat, result.PrimaryErrorCode);
        }

        [Fact]
        public async Task AllExistAsync_WhenAllExerciseTypesExist_ReturnsTrue()
        {
            // Arrange
            var typeIds = new List<ExerciseTypeId>
            {
                ExerciseTypeId.New(),
                ExerciseTypeId.New(),
                ExerciseTypeId.New()
            };
            
            var idStrings = typeIds.Select(id => id.ToString()).ToList();

            foreach (var id in typeIds)
            {
                _mockExerciseTypeRepository
                    .Setup(x => x.ExistsAsync(It.IsAny<ExerciseTypeId>()))
                    .ReturnsAsync(true);
            }

            _mockCacheService
                .Setup(x => x.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());

            // Act
            var result = await _exerciseTypeService.AllExistAsync(idStrings);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task AllExistAsync_WhenSomeExerciseTypesDoNotExist_ReturnsFalse()
        {
            // Arrange
            var typeIds = new List<ExerciseTypeId>
            {
                ExerciseTypeId.New(),
                ExerciseTypeId.New(),
                ExerciseTypeId.New()
            };
            
            var idStrings = typeIds.Select(id => id.ToString()).ToList();

            // First two exist, third does not
            _mockExerciseTypeRepository
                .Setup(x => x.GetByIdAsync(typeIds[0]))
                .ReturnsAsync(ExerciseType.Handler.Create(typeIds[0], "Type1", "Desc1", 1, true).Value);

            _mockExerciseTypeRepository
                .Setup(x => x.GetByIdAsync(typeIds[1]))
                .ReturnsAsync(ExerciseType.Handler.Create(typeIds[1], "Type2", "Desc2", 2, true).Value);

            _mockExerciseTypeRepository
                .Setup(x => x.GetByIdAsync(typeIds[2]))
                .ReturnsAsync(ExerciseType.Empty);

            _mockCacheService
                .Setup(x => x.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());

            // Act
            var result = await _exerciseTypeService.AllExistAsync(idStrings);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task AllExistAsync_WithEmptyCollection_ReturnsTrue()
        {
            // Arrange
            var ids = new List<string>();

            // Act
            var result = await _exerciseTypeService.AllExistAsync(ids);

            // Assert
            Assert.True(result);
        }
        
        [Fact]
        public async Task AllExistAsync_WithInvalidIdFormat_ReturnsFalse()
        {
            // Arrange
            var ids = new List<string> { "invalid-id-format" };

            // Act
            var result = await _exerciseTypeService.AllExistAsync(ids);

            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public async Task AnyIsRestTypeAsync_WithRestType_ReturnsTrue()
        {
            // Arrange
            var restTypeId = ExerciseTypeId.New();
            var nonRestTypeId = ExerciseTypeId.New();
            var ids = new List<string> { restTypeId.ToString(), nonRestTypeId.ToString() };
            
            var restType = ExerciseType.Handler.Create(restTypeId, "Rest", "Rest period", 1, true).Value;
            
            _mockExerciseTypeRepository
                .Setup(x => x.GetByIdAsync(restTypeId))
                .ReturnsAsync(restType);

            _mockCacheService
                .Setup(x => x.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());

            // Act
            var result = await _exerciseTypeService.AnyIsRestTypeAsync(ids);

            // Assert
            Assert.True(result);
        }
        
        [Fact]
        public async Task AnyIsRestTypeAsync_WithoutRestType_ReturnsFalse()
        {
            // Arrange
            var typeId1 = ExerciseTypeId.New();
            var typeId2 = ExerciseTypeId.New();
            var ids = new List<string> { typeId1.ToString(), typeId2.ToString() };
            
            var type1 = ExerciseType.Handler.Create(typeId1, "Strength", "Strength training", 1, true).Value;
            var type2 = ExerciseType.Handler.Create(typeId2, "Cardio", "Cardio training", 2, true).Value;
            
            _mockExerciseTypeRepository
                .Setup(x => x.GetByIdAsync(typeId1))
                .ReturnsAsync(type1);
                
            _mockExerciseTypeRepository
                .Setup(x => x.GetByIdAsync(typeId2))
                .ReturnsAsync(type2);

            _mockCacheService
                .Setup(x => x.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());

            // Act
            var result = await _exerciseTypeService.AnyIsRestTypeAsync(ids);

            // Assert
            Assert.False(result);
        }
    }
}