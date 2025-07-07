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
    public class ExerciseServiceKineticChainValidationTests
    {
        private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _mockUnitOfWorkProvider;
        private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _mockReadOnlyUnitOfWork;
        private readonly Mock<IWritableUnitOfWork<FitnessDbContext>> _mockWritableUnitOfWork;
        private readonly Mock<IExerciseRepository> _mockExerciseRepository;
        private readonly Mock<IExerciseTypeRepository> _mockExerciseTypeRepository;
        private readonly ExerciseService _service;
        
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
            _mockExerciseTypeRepository = new Mock<IExerciseTypeRepository>();
            
            // Create reference data entities
            _restType = ExerciseType.Handler.Create(_restTypeId, "Rest", "Rest period between exercises", 1);
            _strengthType = ExerciseType.Handler.Create(_strengthTypeId, "Strength", "Strength training exercise", 2);
            
            _service = new ExerciseService(_mockUnitOfWorkProvider.Object);
        }

        private void SetupMocks()
        {
            _mockUnitOfWorkProvider.Setup(x => x.CreateReadOnly())
                .Returns(_mockReadOnlyUnitOfWork.Object);
            _mockUnitOfWorkProvider.Setup(x => x.CreateWritable())
                .Returns(_mockWritableUnitOfWork.Object);
                
            _mockReadOnlyUnitOfWork.Setup(x => x.GetRepository<IExerciseRepository>())
                .Returns(_mockExerciseRepository.Object);
            _mockReadOnlyUnitOfWork.Setup(x => x.GetRepository<IExerciseTypeRepository>())
                .Returns(_mockExerciseTypeRepository.Object);
                
            _mockWritableUnitOfWork.Setup(x => x.GetRepository<IExerciseRepository>())
                .Returns(_mockExerciseRepository.Object);
                
            // Set up exercise type repository to return our test types
            _mockExerciseTypeRepository.Setup(x => x.GetByIdAsync(_restTypeId))
                .ReturnsAsync(_restType);
            _mockExerciseTypeRepository.Setup(x => x.GetByIdAsync(_strengthTypeId))
                .ReturnsAsync(_strengthType);
                
            // Default: exercise name doesn't exist
            _mockExerciseRepository.Setup(x => x.ExistsAsync(It.IsAny<string>(), It.IsAny<ExerciseId?>()))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task CreateAsync_NonRestExerciseWithoutKineticChain_ShouldThrowException()
        {
            // Arrange
            SetupMocks();
            
            var request = new CreateExerciseRequest
            {
                Name = "Test Exercise",
                Description = "Test Description",
                DifficultyId = _difficultyId.ToString(),
                ExerciseTypeIds = new List<string> { _strengthTypeId.ToString() },
                MuscleGroups = new List<MuscleGroupWithRoleRequest> 
                { 
                    new MuscleGroupWithRoleRequest 
                    { 
                        MuscleGroupId = MuscleGroupId.New().ToString(), 
                        MuscleRoleId = MuscleRoleId.New().ToString() 
                    } 
                },
                EquipmentIds = new List<string>(),
                MovementPatternIds = new List<string>(),
                BodyPartIds = new List<string>(),
                KineticChainId = null // No kinetic chain provided
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateAsync(request));
            Assert.Equal("Kinetic chain type must be specified for non-REST exercises.", exception.Message);
        }

        [Fact]
        public async Task CreateAsync_RestExerciseWithKineticChain_ShouldThrowException()
        {
            // Arrange
            SetupMocks();
            
            var request = new CreateExerciseRequest
            {
                Name = "Rest Period",
                Description = "Rest between sets",
                DifficultyId = _difficultyId.ToString(),
                ExerciseTypeIds = new List<string> { _restTypeId.ToString() },
                MuscleGroups = new List<MuscleGroupWithRoleRequest>(),
                EquipmentIds = new List<string>(),
                MovementPatternIds = new List<string>(),
                BodyPartIds = new List<string>(),
                KineticChainId = _kineticChainId.ToString() // Kinetic chain provided for rest
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateAsync(request));
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
            
            var request = new CreateExerciseRequest
            {
                Name = "Test Exercise",
                Description = "Test Description",
                DifficultyId = _difficultyId.ToString(),
                ExerciseTypeIds = new List<string> { _strengthTypeId.ToString() },
                MuscleGroups = new List<MuscleGroupWithRoleRequest> 
                { 
                    new MuscleGroupWithRoleRequest 
                    { 
                        MuscleGroupId = MuscleGroupId.New().ToString(), 
                        MuscleRoleId = MuscleRoleId.New().ToString() 
                    } 
                },
                EquipmentIds = new List<string>(),
                MovementPatternIds = new List<string>(),
                BodyPartIds = new List<string>(),
                KineticChainId = _kineticChainId.ToString()
            };

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.NotNull(result);
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
            
            var request = new CreateExerciseRequest
            {
                Name = "Rest Period",
                Description = "Rest between sets",
                DifficultyId = _difficultyId.ToString(),
                ExerciseTypeIds = new List<string> { _restTypeId.ToString() },
                MuscleGroups = new List<MuscleGroupWithRoleRequest>(),
                EquipmentIds = new List<string>(),
                MovementPatternIds = new List<string>(),
                BodyPartIds = new List<string>(),
                KineticChainId = null
            };

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.NotNull(result);
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
            
            var request = new UpdateExerciseRequest
            {
                Name = "Updated Exercise",
                Description = "Updated Description",
                DifficultyId = _difficultyId.ToString(),
                ExerciseTypeIds = new List<string> { _strengthTypeId.ToString() },
                MuscleGroups = new List<MuscleGroupWithRoleRequest> 
                { 
                    new MuscleGroupWithRoleRequest 
                    { 
                        MuscleGroupId = MuscleGroupId.New().ToString(), 
                        MuscleRoleId = MuscleRoleId.New().ToString() 
                    } 
                },
                EquipmentIds = new List<string>(),
                MovementPatternIds = new List<string>(),
                BodyPartIds = new List<string>(),
                KineticChainId = null // No kinetic chain provided
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.UpdateAsync(exerciseId.ToString(), request));
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
            
            var request = new UpdateExerciseRequest
            {
                Name = "Updated Rest",
                Description = "Updated rest period",
                DifficultyId = _difficultyId.ToString(),
                ExerciseTypeIds = new List<string> { _restTypeId.ToString() },
                MuscleGroups = new List<MuscleGroupWithRoleRequest>(),
                EquipmentIds = new List<string>(),
                MovementPatternIds = new List<string>(),
                BodyPartIds = new List<string>(),
                KineticChainId = _kineticChainId.ToString() // Kinetic chain provided for rest
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.UpdateAsync(exerciseId.ToString(), request));
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
            
            var request = new UpdateExerciseRequest
            {
                Name = "Updated Exercise",
                Description = "Updated Description",
                DifficultyId = _difficultyId.ToString(),
                ExerciseTypeIds = new List<string> { _strengthTypeId.ToString() },
                MuscleGroups = new List<MuscleGroupWithRoleRequest> 
                { 
                    new MuscleGroupWithRoleRequest 
                    { 
                        MuscleGroupId = MuscleGroupId.New().ToString(), 
                        MuscleRoleId = MuscleRoleId.New().ToString() 
                    } 
                },
                EquipmentIds = new List<string>(),
                MovementPatternIds = new List<string>(),
                BodyPartIds = new List<string>(),
                KineticChainId = _kineticChainId.ToString()
            };

            // Act
            var result = await _service.UpdateAsync(exerciseId.ToString(), request);

            // Assert
            Assert.NotNull(result);
            _mockExerciseRepository.Verify(x => x.UpdateAsync(It.Is<Exercise>(e => 
                e.KineticChainId == _kineticChainId)), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_InvalidKineticChainId_ShouldThrowException()
        {
            // Arrange
            SetupMocks();
            
            var request = new CreateExerciseRequest
            {
                Name = "Test Exercise",
                Description = "Test Description",
                DifficultyId = _difficultyId.ToString(),
                ExerciseTypeIds = new List<string> { _strengthTypeId.ToString() },
                MuscleGroups = new List<MuscleGroupWithRoleRequest> 
                { 
                    new MuscleGroupWithRoleRequest 
                    { 
                        MuscleGroupId = MuscleGroupId.New().ToString(), 
                        MuscleRoleId = MuscleRoleId.New().ToString() 
                    } 
                },
                EquipmentIds = new List<string>(),
                MovementPatternIds = new List<string>(),
                BodyPartIds = new List<string>(),
                KineticChainId = "invalid-id-format"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreateAsync(request));
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
            
            var request = new UpdateExerciseRequest
            {
                Name = "Updated Exercise",
                Description = "Updated Description",
                DifficultyId = _difficultyId.ToString(),
                ExerciseTypeIds = new List<string> { _strengthTypeId.ToString() },
                MuscleGroups = new List<MuscleGroupWithRoleRequest> 
                { 
                    new MuscleGroupWithRoleRequest 
                    { 
                        MuscleGroupId = MuscleGroupId.New().ToString(), 
                        MuscleRoleId = MuscleRoleId.New().ToString() 
                    } 
                },
                EquipmentIds = new List<string>(),
                MovementPatternIds = new List<string>(),
                BodyPartIds = new List<string>(),
                KineticChainId = "invalid-id-format"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.UpdateAsync(exerciseId.ToString(), request));
            Assert.Equal("Invalid kinetic chain ID: invalid-id-format", exception.Message);
        }
    }
}