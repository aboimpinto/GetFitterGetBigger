using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
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
    public class ExerciseServiceTests
    {
        private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _mockUnitOfWorkProvider;
        private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _mockReadOnlyUnitOfWork;
        private readonly Mock<IWritableUnitOfWork<FitnessDbContext>> _mockWritableUnitOfWork;
        private readonly Mock<IExerciseRepository> _mockExerciseRepository;
        private readonly Mock<IExerciseTypeRepository> _mockExerciseTypeRepository;
        private readonly ExerciseService _service;

        public ExerciseServiceTests()
        {
            _mockUnitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
            _mockReadOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
            _mockWritableUnitOfWork = new Mock<IWritableUnitOfWork<FitnessDbContext>>();
            _mockExerciseRepository = new Mock<IExerciseRepository>();
            _mockExerciseTypeRepository = new Mock<IExerciseTypeRepository>();

            _mockReadOnlyUnitOfWork
                .Setup(uow => uow.GetRepository<IExerciseRepository>())
                .Returns(_mockExerciseRepository.Object);
            
            _mockReadOnlyUnitOfWork
                .Setup(uow => uow.GetRepository<IExerciseTypeRepository>())
                .Returns(_mockExerciseTypeRepository.Object);

            _mockWritableUnitOfWork
                .Setup(uow => uow.GetRepository<IExerciseRepository>())
                .Returns(_mockExerciseRepository.Object);

            _mockUnitOfWorkProvider
                .Setup(p => p.CreateReadOnly())
                .Returns(_mockReadOnlyUnitOfWork.Object);

            _mockUnitOfWorkProvider
                .Setup(p => p.CreateWritable())
                .Returns(_mockWritableUnitOfWork.Object);

            _service = new ExerciseService(_mockUnitOfWorkProvider.Object);
        }

        [Fact]
        public async Task GetPagedAsync_ReturnsPagedResponse()
        {
            // Arrange
            var filterParams = new ExerciseFilterParams
            {
                Page = 1,
                PageSize = 10,
                Name = "Press"
            };

            var difficultyId = DifficultyLevelId.New();
            var difficulty = DifficultyLevel.Handler.Create(difficultyId, "Beginner", "For beginners", 1, true);
            
            var exercises = new List<Exercise>
            {
                CreateTestExercise("Bench Press", difficultyId, difficulty),
                CreateTestExercise("Overhead Press", difficultyId, difficulty)
            };

            _mockExerciseRepository
                .Setup(r => r.GetPagedAsync(
                    It.IsAny<int>(), 
                    It.IsAny<int>(), 
                    It.IsAny<string>(),
                    It.IsAny<DifficultyLevelId?>(),
                    It.IsAny<IEnumerable<MuscleGroupId>>(),
                    It.IsAny<IEnumerable<EquipmentId>>(),
                    It.IsAny<IEnumerable<MovementPatternId>>(),
                    It.IsAny<IEnumerable<BodyPartId>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((exercises, 2));

            // Act
            var result = await _service.GetPagedAsync(filterParams);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Items.Count());
            Assert.All(result.Items, dto => Assert.Contains("Press", dto.Name));
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsExerciseDto()
        {
            // Arrange
            var exerciseId = ExerciseId.New();
            var difficultyId = DifficultyLevelId.New();
            var difficulty = DifficultyLevel.Handler.Create(difficultyId, "Beginner", "For beginners", 1, true);
            var exercise = CreateTestExercise("Test Exercise", difficultyId, difficulty);

            _mockExerciseRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ExerciseId>()))
                .ReturnsAsync(exercise);

            // Act
            var result = await _service.GetByIdAsync(exerciseId.ToString());

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Exercise", result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var invalidId = "invalid-id";

            // Act
            var result = await _service.GetByIdAsync(invalidId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateAsync_WithValidRequest_CreatesExercise()
        {
            // Arrange
            var request = new CreateExerciseRequest
            {
                Name = "New Exercise",
                Description = "Description",
                CoachNotes = new List<CoachNoteRequest> { new() { Text = "Instructions", Order = 0 } },
                IsUnilateral = false,
                DifficultyId = "difficultylevel-" + Guid.NewGuid(),
                MuscleGroups = new List<MuscleGroupWithRoleRequest>
                {
                    new MuscleGroupWithRoleRequest
                    {
                        MuscleGroupId = "musclegroup-" + Guid.NewGuid(),
                        MuscleRoleId = "musclerole-" + Guid.NewGuid()
                    }
                }
            };

            var difficultyId = DifficultyLevelId.New();
            var difficulty = DifficultyLevel.Handler.Create(difficultyId, "Beginner", "For beginners", 1, true);
            var createdExercise = CreateTestExercise("New Exercise", difficultyId, difficulty);

            _mockExerciseRepository
                .Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<ExerciseId?>()))
                .ReturnsAsync(false);

            _mockExerciseRepository
                .Setup(r => r.AddAsync(It.IsAny<Exercise>()))
                .ReturnsAsync(createdExercise);

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Exercise", result.Name);
            _mockWritableUnitOfWork.Verify(uow => uow.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithDuplicateName_ThrowsException()
        {
            // Arrange
            var request = new CreateExerciseRequest
            {
                Name = "Duplicate Exercise",
                Description = "Description",
                CoachNotes = new List<CoachNoteRequest> { new() { Text = "Instructions", Order = 0 } },
                DifficultyId = "difficultylevel-" + Guid.NewGuid(),
                MuscleGroups = new List<MuscleGroupWithRoleRequest>
                {
                    new MuscleGroupWithRoleRequest
                    {
                        MuscleGroupId = "musclegroup-" + Guid.NewGuid(),
                        MuscleRoleId = "musclerole-" + Guid.NewGuid()
                    }
                }
            };

            _mockExerciseRepository
                .Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<ExerciseId?>()))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateAsync(request));
        }

        [Fact]
        public async Task UpdateAsync_WithValidRequest_UpdatesExercise()
        {
            // Arrange
            var exerciseId = ExerciseId.New();
            var request = new UpdateExerciseRequest
            {
                Name = "Updated Exercise",
                Description = "Updated Description",
                CoachNotes = new List<CoachNoteRequest> { new() { Text = "Updated Instructions", Order = 0 } },
                IsActive = true,
                DifficultyId = "difficultylevel-" + Guid.NewGuid(),
                MuscleGroups = new List<MuscleGroupWithRoleRequest>
                {
                    new MuscleGroupWithRoleRequest
                    {
                        MuscleGroupId = "musclegroup-" + Guid.NewGuid(),
                        MuscleRoleId = "musclerole-" + Guid.NewGuid()
                    }
                }
            };

            var difficultyId = DifficultyLevelId.New();
            var difficulty = DifficultyLevel.Handler.Create(difficultyId, "Beginner", "For beginners", 1, true);
            
            // Create existing exercise with the same ID we're updating
            var existingExercise = Exercise.Handler.Create(
                exerciseId,
                "Original Exercise",
                "Original Description",
                null,  // videoUrl
                null,  // imageUrl
                false, // isUnilateral
                false, // isActive
                difficultyId);
            
            // Set up the navigation property
            var existingExerciseWithDifficulty = existingExercise with { Difficulty = difficulty };
            
            var updatedExercise = Exercise.Handler.Create(
                exerciseId,
                "Updated Exercise",
                "Updated Description",
                null,  // videoUrl
                null,  // imageUrl
                false, // isUnilateral
                true,  // isActive
                difficultyId);
                
            var updatedExerciseWithDifficulty = updatedExercise with { Difficulty = difficulty };

            _mockExerciseRepository
                .Setup(r => r.GetByIdAsync(exerciseId))
                .ReturnsAsync(existingExerciseWithDifficulty);

            _mockExerciseRepository
                .Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<ExerciseId?>()))
                .ReturnsAsync(false);

            _mockExerciseRepository
                .Setup(r => r.UpdateAsync(It.IsAny<Exercise>()))
                .ReturnsAsync(updatedExerciseWithDifficulty);

            // Act
            var result = await _service.UpdateAsync(exerciseId.ToString(), request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Exercise", result.Name);
            _mockWritableUnitOfWork.Verify(uow => uow.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithNoReferences_PerformsHardDelete()
        {
            // Arrange
            var exerciseId = ExerciseId.New();
            var difficultyId = DifficultyLevelId.New();
            var difficulty = DifficultyLevel.Handler.Create(difficultyId, "Beginner", "For beginners", 1, true);
            var exercise = CreateTestExercise("To Delete", difficultyId, difficulty);

            _mockExerciseRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ExerciseId>()))
                .ReturnsAsync(exercise);

            _mockExerciseRepository
                .Setup(r => r.HasReferencesAsync(It.IsAny<ExerciseId>()))
                .ReturnsAsync(false);

            _mockExerciseRepository
                .Setup(r => r.DeleteAsync(It.IsAny<ExerciseId>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteAsync(exerciseId.ToString());

            // Assert
            Assert.True(result);
            _mockExerciseRepository.Verify(r => r.DeleteAsync(It.IsAny<ExerciseId>()), Times.Once);
            _mockExerciseRepository.Verify(r => r.UpdateAsync(It.IsAny<Exercise>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WithReferences_PerformsSoftDelete()
        {
            // Arrange
            var exerciseId = ExerciseId.New();
            var difficultyId = DifficultyLevelId.New();
            var difficulty = DifficultyLevel.Handler.Create(difficultyId, "Beginner", "For beginners", 1, true);
            var exercise = CreateTestExercise("To Soft Delete", difficultyId, difficulty);

            _mockExerciseRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ExerciseId>()))
                .ReturnsAsync(exercise);

            _mockExerciseRepository
                .Setup(r => r.HasReferencesAsync(It.IsAny<ExerciseId>()))
                .ReturnsAsync(true);

            _mockExerciseRepository
                .Setup(r => r.UpdateAsync(It.IsAny<Exercise>()))
                .ReturnsAsync(exercise);

            // Act
            var result = await _service.DeleteAsync(exerciseId.ToString());

            // Assert
            Assert.True(result);
            _mockExerciseRepository.Verify(r => r.UpdateAsync(It.IsAny<Exercise>()), Times.Once);
            _mockExerciseRepository.Verify(r => r.DeleteAsync(It.IsAny<ExerciseId>()), Times.Never);
        }

        private Exercise CreateTestExercise(string name, DifficultyLevelId difficultyId, DifficultyLevel difficulty)
        {
            var exercise = Exercise.Handler.CreateNew(
                name,
                "Test Description",
                null,
                null,
                false,
                difficultyId);

            // Use reflection to set the navigation property
            var difficultyProperty = exercise.GetType().GetProperty("Difficulty");
            difficultyProperty?.SetValue(exercise, difficulty);

            return exercise;
        }
    }
}