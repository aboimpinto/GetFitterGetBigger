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
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Commands;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.TestBuilders;
using GetFitterGetBigger.API.Mappers;
using GetFitterGetBigger.API.Tests.TestBuilders.Domain;
using GetFitterGetBigger.API.Constants;
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
        private readonly Mock<IExerciseTypeService> _mockExerciseTypeService;
        private readonly IExerciseService _service;

        public ExerciseServiceTests()
        {
            _mockUnitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
            _mockReadOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
            _mockWritableUnitOfWork = new Mock<IWritableUnitOfWork<FitnessDbContext>>();
            _mockExerciseRepository = new Mock<IExerciseRepository>();
            _mockExerciseTypeService = new Mock<IExerciseTypeService>();

            _mockReadOnlyUnitOfWork
                .Setup(uow => uow.GetRepository<IExerciseRepository>())
                .Returns(_mockExerciseRepository.Object);

            _mockWritableUnitOfWork
                .Setup(uow => uow.GetRepository<IExerciseRepository>())
                .Returns(_mockExerciseRepository.Object);

            _mockUnitOfWorkProvider
                .Setup(p => p.CreateReadOnly())
                .Returns(_mockReadOnlyUnitOfWork.Object);

            _mockUnitOfWorkProvider
                .Setup(p => p.CreateWritable())
                .Returns(_mockWritableUnitOfWork.Object);

            // Setup default mock behaviors for ExerciseTypeService
            _mockExerciseTypeService
                .Setup(s => s.AllExistAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(true);
                
            _mockExerciseTypeService
                .Setup(s => s.AnyIsRestTypeAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync((IEnumerable<string> ids) => 
                    ids.Any(id => id == TestIds.ExerciseTypeIds.Rest || 
                                  id.ToLowerInvariant().Contains("rest")));

            // Default behavior: all exercise types exist
            _mockExerciseTypeService
                .Setup(s => s.ExistsAsync(It.IsAny<ExerciseTypeId>()))
                .ReturnsAsync(true);

            _service = new ExerciseService(_mockUnitOfWorkProvider.Object, _mockExerciseTypeService.Object);
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
            var difficulty = DifficultyLevel.Handler.Create(difficultyId, "Beginner", "For beginners", 1, true).Value;
            
            var exercises = new List<Exercise>
            {
                ExerciseBuilder.AWorkoutExercise()
                    .WithName("Bench Press")
                    .WithDifficultyId(difficultyId)
                    .Build(),
                ExerciseBuilder.AWorkoutExercise()
                    .WithName("Overhead Press")
                    .WithDifficultyId(difficultyId)
                    .Build()
            };

            _mockExerciseRepository
                .Setup(r => r.GetPagedAsync(
                    It.IsAny<int>(), 
                    It.IsAny<int>(), 
                    It.IsAny<string>(),
                    It.IsAny<DifficultyLevelId>(),
                    It.IsAny<IEnumerable<MuscleGroupId>>(),
                    It.IsAny<IEnumerable<EquipmentId>>(),
                    It.IsAny<IEnumerable<MovementPatternId>>(),
                    It.IsAny<IEnumerable<BodyPartId>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((exercises, 2));

            // Act
            var result = await _service.GetPagedAsync(filterParams.ToCommand());

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
            var difficulty = DifficultyLevel.Handler.Create(difficultyId, "Beginner", "For beginners", 1, true).Value;
            var exercise = CreateTestExercise("Test Exercise", difficultyId, difficulty);

            _mockExerciseRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ExerciseId>()))
                .ReturnsAsync(exercise);

            // Act
            var result = await _service.GetByIdAsync(ExerciseId.ParseOrEmpty(exerciseId.ToString()));

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Exercise", result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ReturnsEmpty()
        {
            // Arrange
            var invalidId = "invalid-id";

            // Act
            var result = await _service.GetByIdAsync(ExerciseId.ParseOrEmpty(invalidId));

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsEmpty);
        }

        [Fact]
        public async Task CreateAsync_WithValidRequest_CreatesExercise()
        {
            // Arrange
            var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
                .WithName("New Exercise")
                .WithDescription("Description")
                .WithKineticChain(KineticChainTypeTestBuilder.Compound())
                .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
                .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
                .Build();

            var difficultyId = DifficultyLevelId.New();
            var difficulty = DifficultyLevel.Handler.Create(difficultyId, "Beginner", "For beginners", 1, true).Value;
            var createdExercise = CreateTestExercise("New Exercise", difficultyId, difficulty);

            _mockExerciseRepository
                .Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<ExerciseId?>()))
                .ReturnsAsync(false);

            _mockExerciseRepository
                .Setup(r => r.AddAsync(It.IsAny<Exercise>()))
                .ReturnsAsync(createdExercise);

            // Mock exercise type service to validate all types exist
            _mockExerciseTypeService
                .Setup(s => s.AllExistAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.CreateAsync(request.ToCommand());

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal("New Exercise", result.Data.Name);
            _mockWritableUnitOfWork.Verify(uow => uow.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithDuplicateName_ReturnsFailure()
        {
            // Arrange
            var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
                .WithName("Duplicate Exercise")
                .WithKineticChain(KineticChainTypeTestBuilder.Compound())
                .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
                .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
                .Build();

            _mockExerciseRepository
                .Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<ExerciseId?>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.CreateAsync(request.ToCommand());

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.Data.IsEmpty);
            Assert.Contains("already exists", result.Errors.First()); // Partial match for DuplicateNameFormat
        }

        [Fact]
        public async Task UpdateAsync_WithValidRequest_UpdatesExercise()
        {
            // Arrange
            var exerciseId = ExerciseId.New();
            var request = UpdateExerciseRequestBuilder.ForWorkoutExercise()
                .WithName("Updated Exercise")
                .WithDescription("Updated Description")
                .WithKineticChain(KineticChainTypeTestBuilder.Compound())
                .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
                .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
                .WithIsActive(true)
                .Build();

            var difficultyId = DifficultyLevelId.New();
            var difficulty = DifficultyLevel.Handler.Create(difficultyId, "Beginner", "For beginners", 1, true).Value;
            
            // Create existing exercise with the same ID we're updating
            var existingExercise = ExerciseBuilder.AWorkoutExercise()
                .WithId(exerciseId)
                .WithName("Original Exercise")
                .WithDescription("Original Description")
                .WithDifficultyId(difficultyId)
                .AsInactive()
                .Build();
            
            // Set up the navigation property
            var existingExerciseWithDifficulty = existingExercise with { Difficulty = difficulty };
            
            var updatedExercise = ExerciseBuilder.AWorkoutExercise()
                .WithId(exerciseId)
                .WithName("Updated Exercise")
                .WithDescription("Updated Description")
                .WithDifficultyId(difficultyId)
                .AsActive()
                .Build();
                
            var updatedExerciseWithDifficulty = updatedExercise with { Difficulty = difficulty };

            // First call returns existing, second call (after update) returns updated
            var getByIdCallCount = 0;
            _mockExerciseRepository
                .Setup(r => r.GetByIdAsync(exerciseId))
                .ReturnsAsync(() => 
                {
                    getByIdCallCount++;
                    return getByIdCallCount == 1 ? existingExerciseWithDifficulty : updatedExerciseWithDifficulty;
                });

            _mockExerciseRepository
                .Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<ExerciseId?>()))
                .ReturnsAsync(false);

            _mockExerciseRepository
                .Setup(r => r.UpdateAsync(It.IsAny<Exercise>()))
                .ReturnsAsync(updatedExerciseWithDifficulty);

            // Mock exercise type service to validate all types exist
            _mockExerciseTypeService
                .Setup(s => s.AllExistAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.UpdateAsync(ExerciseId.ParseOrEmpty(exerciseId.ToString()), request.ToCommand());

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal("Updated Exercise", result.Data.Name);
            _mockWritableUnitOfWork.Verify(uow => uow.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithValidId_PerformsSoftDelete()
        {
            // Arrange
            var exerciseId = ExerciseId.New();
            var difficultyId = DifficultyLevelId.New();
            var difficulty = DifficultyLevel.Handler.Create(difficultyId, "Beginner", "For beginners", 1, true).Value;
            var exercise = CreateTestExercise("To Delete", difficultyId, difficulty);

            _mockExerciseRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ExerciseId>()))
                .ReturnsAsync(exercise);

            _mockExerciseRepository
                .Setup(r => r.SoftDeleteAsync(It.IsAny<ExerciseId>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.DeleteAsync(ExerciseId.ParseOrEmpty(exerciseId.ToString()));

            // Assert
            Assert.True(result.IsSuccess);
            _mockExerciseRepository.Verify(r => r.SoftDeleteAsync(It.IsAny<ExerciseId>()), Times.Once);
            _mockExerciseRepository.Verify(r => r.UpdateAsync(It.IsAny<Exercise>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WithInvalidId_ReturnsFailure()
        {
            // Arrange
            var exerciseId = ExerciseId.New();

            _mockExerciseRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ExerciseId>()))
                .ReturnsAsync(Exercise.Empty);

            // Act
            var result = await _service.DeleteAsync(ExerciseId.ParseOrEmpty(exerciseId.ToString()));

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(ExerciseErrorMessages.ExerciseNotFound, result.Errors);
            _mockExerciseRepository.Verify(r => r.SoftDeleteAsync(It.IsAny<ExerciseId>()), Times.Never);
        }

        private Exercise CreateTestExercise(string name, DifficultyLevelId difficultyId, DifficultyLevel difficulty)
        {
            return ExerciseBuilder.AWorkoutExercise()
                .WithName(name)
                .WithDifficultyId(difficultyId)
                .WithKineticChainId(KineticChainTypeId.New())
                .Build();
        }

        [Fact]
        public async Task CreateAsync_WithRestExerciseAndWeightType_ReturnsFailure()
        {
            // Arrange
            var request = CreateExerciseRequestBuilder.ForRestExercise()
                .WithName("Rest Exercise")
                .WithDescription("Rest period")
                .WithExerciseWeightTypeId(TestIds.ExerciseWeightTypeIds.WeightRequired) // Should not be allowed
                .Build();

            // Mock REST type detection
            _mockExerciseTypeService
                .Setup(s => s.AnyIsRestTypeAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.CreateAsync(request.ToCommand());
            
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(ExerciseErrorMessages.RestExerciseCannotHaveWeightType, result.Errors);
        }

        [Fact]
        public async Task CreateAsync_WithNonRestExerciseWithoutWeightType_ReturnsFailure()
        {
            // Arrange
            var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
                .WithName("Workout Exercise")
                .WithDescription("Workout description")
                .WithExerciseWeightTypeId(null) // Should be required
                .AddMuscleGroup(
                    MuscleGroupTestBuilder.Chest(), 
                    MuscleRoleTestBuilder.Primary())
                .Build();

            // Mock non-REST type detection
            _mockExerciseTypeService
                .Setup(s => s.AnyIsRestTypeAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(false);

            // Act
            var result = await _service.CreateAsync(request.ToCommand());
            
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(ExerciseErrorMessages.NonRestExerciseMustHaveWeightType, result.Errors);
        }

        [Fact]
        public async Task CreateAsync_WithNonRestExerciseWithWeightType_Succeeds()
        {
            // Arrange
            var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
                .WithName("Workout Exercise")
                .WithDescription("Strength exercise")
                .WithDifficultyId("difficultylevel-" + Guid.NewGuid())
                .WithKineticChainId("kineticchaintype-" + Guid.NewGuid())
                .WithExerciseTypes("exercisetype-" + Guid.NewGuid())
                .WithExerciseWeightTypeId("exerciseweighttype-" + Guid.NewGuid())
                .AddMuscleGroup(
                    "musclegroup-" + Guid.NewGuid(),
                    "musclerole-" + Guid.NewGuid())
                .Build();

            var difficultyId = DifficultyLevelId.New();
            var difficulty = DifficultyLevel.Handler.Create(difficultyId, "Beginner", "For beginners", 1, true).Value;
            var createdExercise = CreateTestExercise("Workout Exercise", difficultyId, difficulty);

            _mockExerciseRepository
                .Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<ExerciseId?>()))
                .ReturnsAsync(false);

            _mockExerciseRepository
                .Setup(r => r.AddAsync(It.IsAny<Exercise>()))
                .ReturnsAsync(createdExercise);

            // Mock non-REST type detection
            _mockExerciseTypeService
                .Setup(s => s.AnyIsRestTypeAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(false);

            // Act
            var result = await _service.CreateAsync(request.ToCommand());

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal("Workout Exercise", result.Data.Name);
            _mockWritableUnitOfWork.Verify(uow => uow.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithRestExerciseAndWeightType_ReturnsFailure()
        {
            // Arrange
            var exerciseId = ExerciseId.New();
            var request = UpdateExerciseRequestBuilder.ForRestExercise()
                .WithName("Updated Rest Exercise")
                .WithDescription("Updated rest")
                .WithDifficultyId("difficultylevel-" + Guid.NewGuid())
                .WithExerciseTypes(TestIds.ExerciseTypeIds.Rest) // REST type
                .WithExerciseWeightTypeId("exerciseweighttype-" + Guid.NewGuid()) // Should not be allowed
                .Build();

            var difficultyId = DifficultyLevelId.New();
            var difficulty = DifficultyLevel.Handler.Create(difficultyId, "Beginner", "For beginners", 1, true).Value;
            var existingExercise = CreateTestExercise("Rest Exercise", difficultyId, difficulty);

            _mockExerciseRepository
                .Setup(r => r.GetByIdAsync(exerciseId))
                .ReturnsAsync(existingExercise);

            _mockExerciseRepository
                .Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<ExerciseId?>()))
                .ReturnsAsync(false);

            // Mock REST type detection
            _mockExerciseTypeService
                .Setup(s => s.AnyIsRestTypeAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.UpdateAsync(ExerciseId.ParseOrEmpty(exerciseId.ToString()), request.ToCommand());
            
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(ExerciseErrorMessages.RestExerciseCannotHaveWeightType, result.Errors);
        }

        [Fact]
        public async Task UpdateAsync_WithNonRestExerciseWithoutWeightType_ReturnsFailure()
        {
            // Arrange
            var exerciseId = ExerciseId.New();
            var request = UpdateExerciseRequestBuilder.ForWorkoutExercise()
                .WithName("Updated Workout Exercise")
                .WithDescription("Updated strength exercise")
                .WithDifficultyId("difficultylevel-" + Guid.NewGuid())
                .WithKineticChainId("kineticchaintype-" + Guid.NewGuid())
                .WithExerciseTypes("exercisetype-" + Guid.NewGuid())
                .WithExerciseWeightTypeId(null) // Should be required
                .AddMuscleGroup(
                    "musclegroup-" + Guid.NewGuid(),
                    "musclerole-" + Guid.NewGuid())
                .Build();

            var difficultyId = DifficultyLevelId.New();
            var difficulty = DifficultyLevel.Handler.Create(difficultyId, "Beginner", "For beginners", 1, true).Value;
            var existingExercise = CreateTestExercise("Workout Exercise", difficultyId, difficulty);

            _mockExerciseRepository
                .Setup(r => r.GetByIdAsync(exerciseId))
                .ReturnsAsync(existingExercise);

            _mockExerciseRepository
                .Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<ExerciseId?>()))
                .ReturnsAsync(false);

            // Mock non-REST type detection
            _mockExerciseTypeService
                .Setup(s => s.AnyIsRestTypeAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(false);

            // Act
            var result = await _service.UpdateAsync(ExerciseId.ParseOrEmpty(exerciseId.ToString()), request.ToCommand());
            
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(ExerciseErrorMessages.NonRestExerciseMustHaveWeightType, result.Errors);
        }
    }
}