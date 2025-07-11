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
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.TestBuilders;
using GetFitterGetBigger.API.Mappers;
using Moq;
using Olimpo.EntityFramework.Persistency;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services
{
    public class ExerciseServiceKineticChainValidationTests
    {
        private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _mockUnitOfWorkProvider;
        private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _mockReadOnlyUnitOfWork;
        private readonly Mock<IWritableUnitOfWork<FitnessDbContext>> _mockWritableUnitOfWork;
        private readonly Mock<IExerciseRepository> _mockExerciseRepository;
        private readonly Mock<IExerciseTypeService> _mockExerciseTypeService;
        private readonly IExerciseService _service;
        
        // Reference data IDs
        private readonly DifficultyLevelId _difficultyId = DifficultyLevelId.New();
        private readonly ExerciseTypeId _restTypeId = ExerciseTypeId.New();
        private readonly ExerciseTypeId _strengthTypeId = ExerciseTypeId.New();
        private readonly KineticChainTypeId _kineticChainId = KineticChainTypeId.New();
        
        // Reference data entities
        private readonly ExerciseType _restType;
        private readonly ExerciseType _strengthType;

        public ExerciseServiceKineticChainValidationTests()
        {
            _mockUnitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
            _mockReadOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
            _mockWritableUnitOfWork = new Mock<IWritableUnitOfWork<FitnessDbContext>>();
            _mockExerciseRepository = new Mock<IExerciseRepository>();
            _mockExerciseTypeService = new Mock<IExerciseTypeService>();
            
            // Create reference data entities
            _restType = ExerciseType.Handler.Create(_restTypeId, "Rest", "Rest period between exercises", 1);
            _strengthType = ExerciseType.Handler.Create(_strengthTypeId, "Strength", "Strength training exercise", 2);
            
            // Setup default mock behaviors for ExerciseTypeService
            _mockExerciseTypeService
                .Setup(s => s.AllExistAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(true);
                
            _mockExerciseTypeService
                .Setup(s => s.AnyIsRestTypeAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync((IEnumerable<string> ids) => 
                    ids.Any(id => id == "exercisetype-d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a" || 
                                  id == _restTypeId.ToString() ||
                                  id.ToLowerInvariant().Contains("rest")));
            
            // Default behavior: all exercise types exist
            _mockExerciseTypeService
                .Setup(s => s.ExistsAsync(It.IsAny<ExerciseTypeId>()))
                .ReturnsAsync(true);
            
            _service = new ExerciseService(_mockUnitOfWorkProvider.Object, _mockExerciseTypeService.Object);
        }

        private void SetupMocks()
        {
            _mockUnitOfWorkProvider.Setup(x => x.CreateReadOnly())
                .Returns(_mockReadOnlyUnitOfWork.Object);
            _mockUnitOfWorkProvider.Setup(x => x.CreateWritable())
                .Returns(_mockWritableUnitOfWork.Object);
                
            _mockReadOnlyUnitOfWork.Setup(x => x.GetRepository<IExerciseRepository>())
                .Returns(_mockExerciseRepository.Object);
                
            _mockWritableUnitOfWork.Setup(x => x.GetRepository<IExerciseRepository>())
                .Returns(_mockExerciseRepository.Object);
                
            // Override the default mock to return true for REST type
            _mockExerciseTypeService
                .Setup(s => s.AnyIsRestTypeAsync(It.Is<IEnumerable<string>>(ids => 
                    ids.Contains(_restTypeId.ToString()))))
                .ReturnsAsync(true);
                
            // Override the default mock to return false for non-REST type
            _mockExerciseTypeService
                .Setup(s => s.AnyIsRestTypeAsync(It.Is<IEnumerable<string>>(ids => 
                    ids.Contains(_strengthTypeId.ToString()))))
                .ReturnsAsync(false);
                
            // Default: exercise name doesn't exist
            _mockExerciseRepository.Setup(x => x.ExistsAsync(It.IsAny<string>(), It.IsAny<ExerciseId?>()))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task CreateAsync_NonRestExerciseWithoutKineticChain_ShouldThrowException()
        {
            // Arrange
            SetupMocks();
            
            var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
                .WithName("Test Exercise")
                .WithDescription("Test Description")
                .WithDifficultyId(_difficultyId.ToString())
                .WithExerciseTypes(_strengthTypeId.ToString())
                .WithKineticChainId(null) // No kinetic chain provided
                .WithMuscleGroups((MuscleGroupId.New().ToString(), MuscleRoleId.New().ToString()))
                .Build();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateAsync(request.ToCommand()));
            Assert.Equal("Kinetic chain type must be specified for non-REST exercises.", exception.Message);
        }

        [Fact]
        public async Task CreateAsync_RestExerciseWithKineticChain_ShouldThrowException()
        {
            // Arrange
            SetupMocks();
            
            var request = CreateExerciseRequestBuilder.ForRestExercise()
                .WithName("Rest Period")
                .WithDescription("Rest between sets")
                .WithDifficultyId(_difficultyId.ToString())
                .WithExerciseTypes(_restTypeId.ToString())
                .WithKineticChainId(_kineticChainId.ToString()) // Kinetic chain provided for rest
                .Build();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateAsync(request.ToCommand()));
            Assert.Equal("Kinetic chain type must not be specified for REST exercises.", exception.Message);
        }

        [Fact]
        public async Task CreateAsync_NonRestExerciseWithKineticChain_ShouldSucceed()
        {
            // Arrange
            SetupMocks();
            
            var createdExercise = Exercise.Handler.CreateNew(
                "Test Exercise",
                "Test Description",
                null,
                null,
                false,
                _difficultyId,
                _kineticChainId);
                
            _mockExerciseRepository.Setup(x => x.AddAsync(It.IsAny<Exercise>()))
                .ReturnsAsync(createdExercise);
            
            var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
                .WithName("Test Exercise")
                .WithDescription("Test Description")
                .WithDifficultyId(_difficultyId.ToString())
                .WithExerciseTypes(_strengthTypeId.ToString())
                .WithKineticChainId(_kineticChainId.ToString())
                .WithMuscleGroups((MuscleGroupId.New().ToString(), MuscleRoleId.New().ToString()))
                .Build();

            // Act
            var result = await _service.CreateAsync(request.ToCommand());

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            _mockExerciseRepository.Verify(x => x.AddAsync(It.Is<Exercise>(e => 
                e.KineticChainId == _kineticChainId)), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_RestExerciseWithoutKineticChain_ShouldSucceed()
        {
            // Arrange
            SetupMocks();
            
            var createdExercise = Exercise.Handler.CreateNew(
                "Rest Period",
                "Rest between sets",
                null,
                null,
                false,
                _difficultyId,
                null); // No kinetic chain
                
            _mockExerciseRepository.Setup(x => x.AddAsync(It.IsAny<Exercise>()))
                .ReturnsAsync(createdExercise);
            
            var request = CreateExerciseRequestBuilder.ForRestExercise()
                .WithName("Rest Period")
                .WithDescription("Rest between sets")
                .WithDifficultyId(_difficultyId.ToString())
                .WithExerciseTypes(_restTypeId.ToString())
                .Build();

            // Act
            var result = await _service.CreateAsync(request.ToCommand());

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            _mockExerciseRepository.Verify(x => x.AddAsync(It.Is<Exercise>(e => 
                e.KineticChainId == null)), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_NonRestExerciseWithoutKineticChain_ShouldThrowException()
        {
            // Arrange
            SetupMocks();
            
            var exerciseId = ExerciseId.New();
            var existingExercise = Exercise.Handler.Create(
                exerciseId,
                "Test Exercise",
                "Test Description",
                null,
                null,
                false,
                true,
                _difficultyId,
                _kineticChainId);
                
            _mockExerciseRepository.Setup(x => x.GetByIdAsync(exerciseId))
                .ReturnsAsync(existingExercise);
            
            var request = UpdateExerciseRequestBuilder.ForWorkoutExercise()
                .WithName("Updated Exercise")
                .WithDescription("Updated Description")
                .WithDifficultyId(_difficultyId.ToString())
                .WithExerciseTypes(_strengthTypeId.ToString())
                .WithKineticChainId(null) // No kinetic chain provided
                .WithMuscleGroups((MuscleGroupId.New().ToString(), MuscleRoleId.New().ToString()))
                .Build();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.UpdateAsync(ExerciseId.ParseOrEmpty(exerciseId.ToString()), request.ToCommand()));
            Assert.Equal("Kinetic chain type must be specified for non-REST exercises.", exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_RestExerciseWithKineticChain_ShouldThrowException()
        {
            // Arrange
            SetupMocks();
            
            var exerciseId = ExerciseId.New();
            var existingExercise = Exercise.Handler.Create(
                exerciseId,
                "Rest Period",
                "Rest between sets",
                null,
                null,
                false,
                true,
                _difficultyId,
                null);
                
            _mockExerciseRepository.Setup(x => x.GetByIdAsync(exerciseId))
                .ReturnsAsync(existingExercise);
            
            var request = UpdateExerciseRequestBuilder.ForRestExercise()
                .WithName("Updated Rest")
                .WithDescription("Updated rest period")
                .WithDifficultyId(_difficultyId.ToString())
                .WithExerciseTypes(_restTypeId.ToString())
                .WithKineticChainId(_kineticChainId.ToString()) // Kinetic chain provided for rest
                .Build();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.UpdateAsync(ExerciseId.ParseOrEmpty(exerciseId.ToString()), request.ToCommand()));
            Assert.Equal("Kinetic chain type must not be specified for REST exercises.", exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_NonRestExerciseWithKineticChain_ShouldSucceed()
        {
            // Arrange
            SetupMocks();
            
            var exerciseId = ExerciseId.New();
            var existingExercise = Exercise.Handler.Create(
                exerciseId,
                "Test Exercise",
                "Test Description",
                null,
                null,
                false,
                true,
                _difficultyId,
                _kineticChainId);
                
            _mockExerciseRepository.Setup(x => x.GetByIdAsync(exerciseId))
                .ReturnsAsync(existingExercise);
                
            var updatedExercise = Exercise.Handler.Create(
                exerciseId,
                "Updated Exercise",
                "Updated Description",
                null,
                null,
                false,
                true,
                _difficultyId,
                _kineticChainId);
                
            _mockExerciseRepository.Setup(x => x.UpdateAsync(It.IsAny<Exercise>()))
                .ReturnsAsync(updatedExercise);
            
            var request = UpdateExerciseRequestBuilder.ForWorkoutExercise()
                .WithName("Updated Exercise")
                .WithDescription("Updated Description")
                .WithDifficultyId(_difficultyId.ToString())
                .WithExerciseTypes(_strengthTypeId.ToString())
                .WithKineticChainId(_kineticChainId.ToString())
                .WithMuscleGroups((MuscleGroupId.New().ToString(), MuscleRoleId.New().ToString()))
                .Build();

            // Act
            var result = await _service.UpdateAsync(ExerciseId.ParseOrEmpty(exerciseId.ToString()), request.ToCommand());

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            _mockExerciseRepository.Verify(x => x.UpdateAsync(It.Is<Exercise>(e => 
                e.KineticChainId == _kineticChainId)), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_InvalidKineticChainId_ShouldThrowException()
        {
            // Arrange
            SetupMocks();
            
            var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
                .WithName("Test Exercise")
                .WithDescription("Test Description")
                .WithDifficultyId(_difficultyId.ToString())
                .WithExerciseTypes(_strengthTypeId.ToString())
                .WithKineticChainId("invalid-id-format")
                .WithMuscleGroups((MuscleGroupId.New().ToString(), MuscleRoleId.New().ToString()))
                .Build();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreateAsync(request.ToCommand()));
            Assert.Equal("Invalid kinetic chain ID: invalid-id-format", exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_InvalidKineticChainId_ShouldThrowException()
        {
            // Arrange
            SetupMocks();
            
            var exerciseId = ExerciseId.New();
            var existingExercise = Exercise.Handler.Create(
                exerciseId,
                "Test Exercise",
                "Test Description",
                null,
                null,
                false,
                true,
                _difficultyId);
                
            _mockExerciseRepository.Setup(x => x.GetByIdAsync(exerciseId))
                .ReturnsAsync(existingExercise);
            
            var request = UpdateExerciseRequestBuilder.ForWorkoutExercise()
                .WithName("Updated Exercise")
                .WithDescription("Updated Description")
                .WithDifficultyId(_difficultyId.ToString())
                .WithExerciseTypes(_strengthTypeId.ToString())
                .WithKineticChainId("invalid-id-format")
                .WithMuscleGroups((MuscleGroupId.New().ToString(), MuscleRoleId.New().ToString()))
                .Build();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.UpdateAsync(ExerciseId.ParseOrEmpty(exerciseId.ToString()), request.ToCommand()));
            Assert.Equal("Invalid kinetic chain ID: invalid-id-format", exception.Message);
        }
    }
}