using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Implementations;
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
            var ids = new List<ExerciseTypeId>
            {
                ExerciseTypeId.New(),
                ExerciseTypeId.New(),
                ExerciseTypeId.New()
            };

            var exerciseTypes = ids.Select((id, index) => 
                ExerciseType.Handler.Create(
                    id,
                    $"Type{index}",
                    $"Description{index}",
                    index,
                    true)
            ).ToList();

            foreach (var pair in ids.Zip(exerciseTypes, (id, type) => new { id, type }))
            {
                _mockExerciseTypeRepository
                    .Setup(x => x.GetByIdAsync(pair.id))
                    .ReturnsAsync(pair.type);
            }

            // Act
            var result = await _exerciseTypeService.AllExistAsync(ids);

            // Assert
            Assert.True(result);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            foreach (var id in ids)
            {
                _mockExerciseTypeRepository.Verify(x => x.GetByIdAsync(id), Times.Once);
            }
            _mockReadOnlyUnitOfWork.Verify(x => x.Dispose(), Times.Once);
        }

        [Fact]
        public async Task AllExistAsync_WhenSomeExerciseTypesDoNotExist_ReturnsFalse()
        {
            // Arrange
            var ids = new List<ExerciseTypeId>
            {
                ExerciseTypeId.New(),
                ExerciseTypeId.New(),
                ExerciseTypeId.New()
            };

            // First two exist, third does not
            _mockExerciseTypeRepository
                .Setup(x => x.GetByIdAsync(ids[0]))
                .ReturnsAsync(ExerciseType.Handler.Create(ids[0], "Type1", "Desc1", 1, true));

            _mockExerciseTypeRepository
                .Setup(x => x.GetByIdAsync(ids[1]))
                .ReturnsAsync(ExerciseType.Handler.Create(ids[1], "Type2", "Desc2", 2, true));

            _mockExerciseTypeRepository
                .Setup(x => x.GetByIdAsync(ids[2]))
                .ReturnsAsync((ExerciseType?)null);

            // Act
            var result = await _exerciseTypeService.AllExistAsync(ids);

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
            var ids = new List<ExerciseTypeId>();

            // Act
            var result = await _exerciseTypeService.AllExistAsync(ids);

            // Assert
            Assert.True(result);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            _mockExerciseTypeRepository.Verify(x => x.GetByIdAsync(It.IsAny<ExerciseTypeId>()), Times.Never);
            _mockReadOnlyUnitOfWork.Verify(x => x.Dispose(), Times.Once);
        }
    }
}