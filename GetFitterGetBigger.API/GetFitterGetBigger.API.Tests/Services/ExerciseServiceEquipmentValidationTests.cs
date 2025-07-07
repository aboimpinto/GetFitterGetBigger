using System;
using System.Collections.Generic;
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

namespace GetFitterGetBigger.API.Tests.Services;

public class ExerciseServiceEquipmentValidationTests
{
    private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _unitOfWorkProviderMock;
    private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _readOnlyUnitOfWorkMock;
    private readonly Mock<IWritableUnitOfWork<FitnessDbContext>> _writableUnitOfWorkMock;
    private readonly Mock<IExerciseRepository> _exerciseRepositoryMock;
    private readonly Mock<IExerciseTypeRepository> _exerciseTypeRepositoryMock;
    private readonly ExerciseService _exerciseService;
    
    public ExerciseServiceEquipmentValidationTests()
    {
        _unitOfWorkProviderMock = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
        _readOnlyUnitOfWorkMock = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
        _writableUnitOfWorkMock = new Mock<IWritableUnitOfWork<FitnessDbContext>>();
        _exerciseRepositoryMock = new Mock<IExerciseRepository>();
        _exerciseTypeRepositoryMock = new Mock<IExerciseTypeRepository>();
        
        _readOnlyUnitOfWorkMock.Setup(uow => uow.GetRepository<IExerciseRepository>())
            .Returns(_exerciseRepositoryMock.Object);
        
        _readOnlyUnitOfWorkMock.Setup(uow => uow.GetRepository<IExerciseTypeRepository>())
            .Returns(_exerciseTypeRepositoryMock.Object);
        
        _writableUnitOfWorkMock.Setup(uow => uow.GetRepository<IExerciseRepository>())
            .Returns(_exerciseRepositoryMock.Object);
        
        _unitOfWorkProviderMock.Setup(p => p.CreateReadOnly())
            .Returns(_readOnlyUnitOfWorkMock.Object);
        
        _unitOfWorkProviderMock.Setup(p => p.CreateWritable())
            .Returns(_writableUnitOfWorkMock.Object);
        
        _exerciseService = new ExerciseService(_unitOfWorkProviderMock.Object);
    }
    
    [Fact]
    public async Task CreateAsync_RestExerciseWithoutEquipment_CreatesSuccessfully()
    {
        // Arrange
        var restTypeId = ExerciseTypeId.New();
        var restType = ExerciseType.Handler.Create(restTypeId, "Rest", "Rest period", 1, true);
        
        var request = new CreateExerciseRequest
        {
            Name = "Rest Period",
            Description = "Recovery time between sets",
            DifficultyId = DifficultyLevelId.New().ToString(),
            ExerciseTypeIds = new List<string> { restTypeId.ToString() },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>(), // Empty for REST
            EquipmentIds = new List<string>(), // Empty equipment - this should be allowed
            MovementPatternIds = new List<string>(),
            BodyPartIds = new List<string>(),
            KineticChainId = null // REST exercise
        };
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), null))
            .ReturnsAsync(false);
        
        _exerciseTypeRepositoryMock.Setup(r => r.GetByIdAsync(restTypeId))
            .ReturnsAsync(restType);
        
        _exerciseRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Exercise>()))
            .ReturnsAsync((Exercise e) => e);
        
        _writableUnitOfWorkMock.Setup(uow => uow.CommitAsync())
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _exerciseService.CreateAsync(request);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Rest Period", result.Name);
        Assert.Empty(result.Equipment); // Equipment is empty and that's OK
        _exerciseRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Exercise>()), Times.Once);
    }
    
    [Fact]
    public async Task CreateAsync_NonRestExerciseWithoutEquipment_CreatesSuccessfully()
    {
        // Arrange
        var workoutTypeId = ExerciseTypeId.New();
        var workoutType = ExerciseType.Handler.Create(workoutTypeId, "Workout", "Main workout", 1, false);
        
        var request = new CreateExerciseRequest
        {
            Name = "Push Up",
            Description = "Bodyweight upper body exercise",
            DifficultyId = DifficultyLevelId.New().ToString(),
            ExerciseTypeIds = new List<string> { workoutTypeId.ToString() },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>
            {
                new() { MuscleGroupId = MuscleGroupId.New().ToString(), MuscleRoleId = MuscleRoleId.New().ToString() }
            },
            EquipmentIds = new List<string>(), // Empty equipment - this should be allowed for bodyweight exercises
            MovementPatternIds = new List<string>(),
            BodyPartIds = new List<string>(),
            KineticChainId = KineticChainTypeId.New().ToString() // Non-REST exercise
        };
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), null))
            .ReturnsAsync(false);
        
        _exerciseTypeRepositoryMock.Setup(r => r.GetByIdAsync(workoutTypeId))
            .ReturnsAsync(workoutType);
        
        _exerciseRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Exercise>()))
            .ReturnsAsync((Exercise e) => e);
        
        _writableUnitOfWorkMock.Setup(uow => uow.CommitAsync())
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _exerciseService.CreateAsync(request);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Push Up", result.Name);
        Assert.Empty(result.Equipment); // Equipment is empty and that's OK for bodyweight exercises
        _exerciseRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Exercise>()), Times.Once);
    }
    
    [Fact]
    public async Task CreateAsync_ExerciseWithEquipment_CreatesSuccessfully()
    {
        // Arrange
        var workoutTypeId = ExerciseTypeId.New();
        var workoutType = ExerciseType.Handler.Create(workoutTypeId, "Workout", "Main workout", 1, false);
        var equipmentId = EquipmentId.New();
        
        var request = new CreateExerciseRequest
        {
            Name = "Bench Press",
            Description = "Chest exercise with barbell",
            DifficultyId = DifficultyLevelId.New().ToString(),
            ExerciseTypeIds = new List<string> { workoutTypeId.ToString() },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>
            {
                new() { MuscleGroupId = MuscleGroupId.New().ToString(), MuscleRoleId = MuscleRoleId.New().ToString() }
            },
            EquipmentIds = new List<string> { equipmentId.ToString() }, // With equipment
            MovementPatternIds = new List<string>(),
            BodyPartIds = new List<string>(),
            KineticChainId = KineticChainTypeId.New().ToString() // Non-REST exercise
        };
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), null))
            .ReturnsAsync(false);
        
        _exerciseTypeRepositoryMock.Setup(r => r.GetByIdAsync(workoutTypeId))
            .ReturnsAsync(workoutType);
        
        _exerciseRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Exercise>()))
            .ReturnsAsync((Exercise e) => e);
        
        _writableUnitOfWorkMock.Setup(uow => uow.CommitAsync())
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _exerciseService.CreateAsync(request);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Bench Press", result.Name);
        Assert.Single(result.Equipment); // Has equipment
        _exerciseRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Exercise>()), Times.Once);
    }
    
    [Fact]
    public async Task UpdateAsync_RemoveAllEquipment_UpdatesSuccessfully()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var workoutTypeId = ExerciseTypeId.New();
        var workoutType = ExerciseType.Handler.Create(workoutTypeId, "Workout", "Main workout", 1, false);
        
        var existingExercise = Exercise.Handler.CreateNew(
            "Bench Press",
            "Chest exercise",
            null,
            null,
            false,
            DifficultyLevelId.New());
        
        // Update to remove all equipment
        var request = new UpdateExerciseRequest
        {
            Name = "Push Up",
            Description = "Changed to bodyweight exercise",
            DifficultyId = DifficultyLevelId.New().ToString(),
            ExerciseTypeIds = new List<string> { workoutTypeId.ToString() },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>
            {
                new() { MuscleGroupId = MuscleGroupId.New().ToString(), MuscleRoleId = MuscleRoleId.New().ToString() }
            },
            EquipmentIds = new List<string>(), // Remove all equipment
            MovementPatternIds = new List<string>(),
            BodyPartIds = new List<string>(),
            KineticChainId = KineticChainTypeId.New().ToString() // Non-REST exercise
        };
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<ExerciseId>()))
            .ReturnsAsync(false);
        
        _exerciseRepositoryMock.Setup(r => r.GetByIdAsync(exerciseId))
            .ReturnsAsync(existingExercise);
        
        _exerciseTypeRepositoryMock.Setup(r => r.GetByIdAsync(workoutTypeId))
            .ReturnsAsync(workoutType);
        
        _exerciseRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Exercise>()))
            .ReturnsAsync((Exercise e) => e);
        
        _writableUnitOfWorkMock.Setup(uow => uow.CommitAsync())
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _exerciseService.UpdateAsync(exerciseId.ToString(), request);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Push Up", result.Name);
        Assert.Empty(result.Equipment); // Equipment removed successfully
        _exerciseRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Exercise>()), Times.Once);
    }
}