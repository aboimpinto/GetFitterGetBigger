using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Implementations;
using Moq;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Tests.Services
{
    public class ExerciseTypeServiceTests
    {
        private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _mockUnitOfWorkProvider;
        private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _mockReadOnlyUnitOfWork;
        private readonly Mock<IExerciseTypeRepository> _mockExerciseTypeRepository;
        private readonly ExerciseTypeService _exerciseTypeService;

        public ExerciseTypeServiceTests()
        {
            _mockUnitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
            _mockReadOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
            _mockExerciseTypeRepository = new Mock<IExerciseTypeRepository>();

            _mockUnitOfWorkProvider
                .Setup(x => x.CreateReadOnly())
                .Returns(_mockReadOnlyUnitOfWork.Object);

            _mockReadOnlyUnitOfWork
                .Setup(x => x.GetRepository<IExerciseTypeRepository>())
                .Returns(_mockExerciseTypeRepository.Object);

            _exerciseTypeService = new ExerciseTypeService(
                _mockUnitOfWorkProvider.Object);
        }

        [Fact]
        public async Task ExistsAsync_WhenExerciseTypeExists_ReturnsTrue()
        {
            // Arrange
            var exerciseTypeId = ExerciseTypeId.New();
            var exerciseType = ExerciseType.Handler.Create(
                exerciseTypeId,
                "Strength",
                "Strength training exercises",
                1,
                true);

            _mockExerciseTypeRepository
                .Setup(x => x.GetByIdAsync(exerciseTypeId))
                .ReturnsAsync(exerciseType);

            // Act
            var result = await _exerciseTypeService.ExistsAsync(exerciseTypeId);

            // Assert
            Assert.True(result);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            _mockExerciseTypeRepository.Verify(x => x.GetByIdAsync(exerciseTypeId), Times.Once);
            _mockReadOnlyUnitOfWork.Verify(x => x.Dispose(), Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_WhenExerciseTypeDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var exerciseTypeId = ExerciseTypeId.New();

            _mockExerciseTypeRepository
                .Setup(x => x.GetByIdAsync(exerciseTypeId))
                .ReturnsAsync((ExerciseType?)null);

            // Act
            var result = await _exerciseTypeService.ExistsAsync(exerciseTypeId);

            // Assert
            Assert.False(result);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            _mockExerciseTypeRepository.Verify(x => x.GetByIdAsync(exerciseTypeId), Times.Once);
            _mockReadOnlyUnitOfWork.Verify(x => x.Dispose(), Times.Once);
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

            var exerciseTypes = typeIds.Select((id, index) => 
                ExerciseType.Handler.Create(
                    id,
                    $"Type{index}",
                    $"Description{index}",
                    index,
                    true)
            ).ToList();

            foreach (var pair in typeIds.Zip(exerciseTypes, (id, type) => new { id, type }))
            {
                _mockExerciseTypeRepository
                    .Setup(x => x.GetByIdAsync(pair.id))
                    .ReturnsAsync(pair.type);
            }

            // Act
            var result = await _exerciseTypeService.AllExistAsync(idStrings);

            // Assert
            Assert.True(result);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            foreach (var id in typeIds)
            {
                _mockExerciseTypeRepository.Verify(x => x.GetByIdAsync(id), Times.Once);
            }
            _mockReadOnlyUnitOfWork.Verify(x => x.Dispose(), Times.Once);
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
                .ReturnsAsync(ExerciseType.Handler.Create(typeIds[0], "Type1", "Desc1", 1, true));

            _mockExerciseTypeRepository
                .Setup(x => x.GetByIdAsync(typeIds[1]))
                .ReturnsAsync(ExerciseType.Handler.Create(typeIds[1], "Type2", "Desc2", 2, true));

            _mockExerciseTypeRepository
                .Setup(x => x.GetByIdAsync(typeIds[2]))
                .ReturnsAsync((ExerciseType?)null);

            // Act
            var result = await _exerciseTypeService.AllExistAsync(idStrings);

            // Assert
            Assert.False(result);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            // Should stop checking after finding the first non-existent one
            _mockExerciseTypeRepository.Verify(x => x.GetByIdAsync(It.IsAny<ExerciseTypeId>()), Times.Exactly(3));
            _mockReadOnlyUnitOfWork.Verify(x => x.Dispose(), Times.Once);
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
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            _mockExerciseTypeRepository.Verify(x => x.GetByIdAsync(It.IsAny<ExerciseTypeId>()), Times.Never);
            _mockReadOnlyUnitOfWork.Verify(x => x.Dispose(), Times.Once);
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
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            _mockExerciseTypeRepository.Verify(x => x.GetByIdAsync(It.IsAny<ExerciseTypeId>()), Times.Never);
            _mockReadOnlyUnitOfWork.Verify(x => x.Dispose(), Times.Once);
        }
        
        [Fact]
        public async Task AnyIsRestTypeAsync_WithRestType_ReturnsTrue()
        {
            // Arrange
            var restTypeId = ExerciseTypeId.New();
            var nonRestTypeId = ExerciseTypeId.New();
            var ids = new List<string> { restTypeId.ToString(), nonRestTypeId.ToString() };
            
            var restType = ExerciseType.Handler.Create(restTypeId, "Rest", "Rest period", 1, true);
            var nonRestType = ExerciseType.Handler.Create(nonRestTypeId, "Strength", "Strength training", 2, true);
            
            _mockExerciseTypeRepository
                .Setup(x => x.GetByIdAsync(restTypeId))
                .ReturnsAsync(restType);
                
            _mockExerciseTypeRepository
                .Setup(x => x.GetByIdAsync(nonRestTypeId))
                .ReturnsAsync(nonRestType);

            // Act
            var result = await _exerciseTypeService.AnyIsRestTypeAsync(ids);

            // Assert
            Assert.True(result);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            _mockExerciseTypeRepository.Verify(x => x.GetByIdAsync(restTypeId), Times.Once);
            // Should stop checking after finding REST type
            _mockReadOnlyUnitOfWork.Verify(x => x.Dispose(), Times.Once);
        }
        
        [Fact]
        public async Task AnyIsRestTypeAsync_WithoutRestType_ReturnsFalse()
        {
            // Arrange
            var typeId1 = ExerciseTypeId.New();
            var typeId2 = ExerciseTypeId.New();
            var ids = new List<string> { typeId1.ToString(), typeId2.ToString() };
            
            var type1 = ExerciseType.Handler.Create(typeId1, "Strength", "Strength training", 1, true);
            var type2 = ExerciseType.Handler.Create(typeId2, "Cardio", "Cardio training", 2, true);
            
            _mockExerciseTypeRepository
                .Setup(x => x.GetByIdAsync(typeId1))
                .ReturnsAsync(type1);
                
            _mockExerciseTypeRepository
                .Setup(x => x.GetByIdAsync(typeId2))
                .ReturnsAsync(type2);

            // Act
            var result = await _exerciseTypeService.AnyIsRestTypeAsync(ids);

            // Assert
            Assert.False(result);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            _mockExerciseTypeRepository.Verify(x => x.GetByIdAsync(It.IsAny<ExerciseTypeId>()), Times.Exactly(2));
            _mockReadOnlyUnitOfWork.Verify(x => x.Dispose(), Times.Once);
        }
    }
}