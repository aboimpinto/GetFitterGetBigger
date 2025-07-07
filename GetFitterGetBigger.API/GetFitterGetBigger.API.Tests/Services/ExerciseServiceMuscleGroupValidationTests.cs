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

public class ExerciseServiceMuscleGroupValidationTests
{
    private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _unitOfWorkProviderMock;
    private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _readOnlyUnitOfWorkMock;
    private readonly Mock<IWritableUnitOfWork<FitnessDbContext>> _writableUnitOfWorkMock;
    private readonly Mock<IExerciseRepository> _exerciseRepositoryMock;
    private readonly Mock<IExerciseTypeRepository> _exerciseTypeRepositoryMock;
    private readonly ExerciseService _exerciseService;
    
    public ExerciseServiceMuscleGroupValidationTests()
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
    public async Task CreateAsync_RestExerciseWithoutMuscleGroups_CreatesSuccessfully()
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
            MuscleGroups = new List<MuscleGroupWithRoleRequest>(), // Empty muscle groups for REST
            EquipmentIds = new List<string>(),
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
        Assert.Empty(result.MuscleGroups);
        _exerciseRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Exercise>()), Times.Once);
    }
    
    [Fact]
    public async Task CreateAsync_NonRestExerciseWithoutMuscleGroups_ThrowsInvalidOperationException()
    {
        // Arrange
        var workoutTypeId = ExerciseTypeId.New();
        var workoutType = ExerciseType.Handler.Create(workoutTypeId, "Workout", "Main workout", 1, false);
        
        var request = new CreateExerciseRequest
        {
            Name = "Push Up",
            Description = "Upper body exercise",
            DifficultyId = DifficultyLevelId.New().ToString(),
            ExerciseTypeIds = new List<string> { workoutTypeId.ToString() },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>(), // Empty muscle groups for non-REST
            EquipmentIds = new List<string>(),
            MovementPatternIds = new List<string>(),
            BodyPartIds = new List<string>(),
            KineticChainId = KineticChainTypeId.New().ToString() // Non-REST exercise
        };
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), null))
            .ReturnsAsync(false);
        
        _exerciseTypeRepositoryMock.Setup(r => r.GetByIdAsync(workoutTypeId))
            .ReturnsAsync(workoutType);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _exerciseService.CreateAsync(request));
        
        Assert.Equal("At least one muscle group must be specified for non-REST exercises.", exception.Message);
        _exerciseRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Exercise>()), Times.Never);
    }
    
    [Fact]
    public async Task CreateAsync_RestExerciseWithMuscleGroups_CreatesSuccessfully()
    {
        // Arrange - REST exercises CAN have muscle groups, they're just not required
        var restTypeId = ExerciseTypeId.New();
        var restType = ExerciseType.Handler.Create(restTypeId, "Rest", "Rest period", 1, true);
        
        var request = new CreateExerciseRequest
        {
            Name = "Active Rest",
            Description = "Light movement during rest",
            DifficultyId = DifficultyLevelId.New().ToString(),
            ExerciseTypeIds = new List<string> { restTypeId.ToString() },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>
            {
                new() { MuscleGroupId = MuscleGroupId.New().ToString(), MuscleRoleId = MuscleRoleId.New().ToString() }
            },
            EquipmentIds = new List<string>(),
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
        Assert.Equal("Active Rest", result.Name);
        Assert.Single(result.MuscleGroups);
        _exerciseRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Exercise>()), Times.Once);
    }
    
    [Fact]
    public async Task UpdateAsync_RestExerciseWithoutMuscleGroups_UpdatesSuccessfully()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var restTypeId = ExerciseTypeId.New();
        var restType = ExerciseType.Handler.Create(restTypeId, "Rest", "Rest period", 1, true);
        
        var existingExercise = Exercise.Handler.CreateNew(
            "Old Exercise",
            "Old Description",
            null,
            null,
            false,
            DifficultyLevelId.New());
        
        var request = new UpdateExerciseRequest
        {
            Name = "Rest Period Updated",
            Description = "Updated recovery time",
            DifficultyId = DifficultyLevelId.New().ToString(),
            ExerciseTypeIds = new List<string> { restTypeId.ToString() },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>(), // Empty muscle groups for REST
            EquipmentIds = new List<string>(),
            MovementPatternIds = new List<string>(),
            BodyPartIds = new List<string>()
        };
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<ExerciseId>()))
            .ReturnsAsync(false);
        
        _exerciseRepositoryMock.Setup(r => r.GetByIdAsync(exerciseId))
            .ReturnsAsync(existingExercise);
        
        _exerciseTypeRepositoryMock.Setup(r => r.GetByIdAsync(restTypeId))
            .ReturnsAsync(restType);
        
        _exerciseRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Exercise>()))
            .ReturnsAsync((Exercise e) => e);
        
        _writableUnitOfWorkMock.Setup(uow => uow.CommitAsync())
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _exerciseService.UpdateAsync(exerciseId.ToString(), request);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Rest Period Updated", result.Name);
        Assert.Empty(result.MuscleGroups);
        _exerciseRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Exercise>()), Times.Once);
    }
    
    [Fact]
    public async Task UpdateAsync_NonRestExerciseWithoutMuscleGroups_ThrowsInvalidOperationException()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var workoutTypeId = ExerciseTypeId.New();
        var workoutType = ExerciseType.Handler.Create(workoutTypeId, "Workout", "Main workout", 1, false);
        
        var existingExercise = Exercise.Handler.CreateNew(
            "Old Exercise",
            "Old Description",
            null,
            null,
            false,
            DifficultyLevelId.New());
        
        var request = new UpdateExerciseRequest
        {
            Name = "Push Up Updated",
            Description = "Updated upper body exercise",
            DifficultyId = DifficultyLevelId.New().ToString(),
            ExerciseTypeIds = new List<string> { workoutTypeId.ToString() },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>(), // Empty muscle groups for non-REST
            EquipmentIds = new List<string>(),
            MovementPatternIds = new List<string>(),
            BodyPartIds = new List<string>()
        };
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<ExerciseId>()))
            .ReturnsAsync(false);
        
        _exerciseRepositoryMock.Setup(r => r.GetByIdAsync(exerciseId))
            .ReturnsAsync(existingExercise);
        
        _exerciseTypeRepositoryMock.Setup(r => r.GetByIdAsync(workoutTypeId))
            .ReturnsAsync(workoutType);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _exerciseService.UpdateAsync(exerciseId.ToString(), request));
        
        Assert.Equal("At least one muscle group must be specified for non-REST exercises.", exception.Message);
        _exerciseRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Exercise>()), Times.Never);
    }
    
    [Theory]
    [InlineData("rest")]
    [InlineData("Rest")]
    [InlineData("REST")]
    [InlineData("rEsT")]
    public async Task CreateAsync_RestExerciseCaseInsensitive_WithoutMuscleGroups_CreatesSuccessfully(string restValue)
    {
        // Arrange
        var restTypeId = ExerciseTypeId.New();
        var restType = ExerciseType.Handler.Create(restTypeId, restValue, "Rest period", 1, true);
        
        var request = new CreateExerciseRequest
        {
            Name = "Rest Period",
            Description = "Recovery time",
            DifficultyId = DifficultyLevelId.New().ToString(),
            ExerciseTypeIds = new List<string> { restTypeId.ToString() },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>(), // Empty muscle groups
            EquipmentIds = new List<string>(),
            MovementPatternIds = new List<string>(),
            BodyPartIds = new List<string>()
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
        Assert.Empty(result.MuscleGroups);
    }
    
    [Fact]
    public async Task CreateAsync_NoExerciseTypes_WithoutMuscleGroups_ThrowsInvalidOperationException()
    {
        // Arrange - No exercise types means it's not a REST exercise
        var request = new CreateExerciseRequest
        {
            Name = "Generic Exercise",
            Description = "Some exercise",
            DifficultyId = DifficultyLevelId.New().ToString(),
            ExerciseTypeIds = new List<string>(),
            MuscleGroups = new List<MuscleGroupWithRoleRequest>(), // Empty muscle groups
            EquipmentIds = new List<string>(),
            MovementPatternIds = new List<string>(),
            BodyPartIds = new List<string>(),
            KineticChainId = KineticChainTypeId.New().ToString() // No exercise types, so non-REST
        };
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), null))
            .ReturnsAsync(false);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _exerciseService.CreateAsync(request));
        
        Assert.Equal("At least one muscle group must be specified for non-REST exercises.", exception.Message);
    }
    
    [Fact]
    public async Task CreateAsync_MultipleTypesIncludingRest_WithoutMuscleGroups_ThrowsRestExclusivityError()
    {
        // Arrange - REST can't be combined with other types (REST exclusivity rule)
        var restTypeId = ExerciseTypeId.New();
        var workoutTypeId = ExerciseTypeId.New();
        var restType = ExerciseType.Handler.Create(restTypeId, "Rest", "Rest period", 1, true);
        var workoutType = ExerciseType.Handler.Create(workoutTypeId, "Workout", "Main workout", 2, false);
        
        var request = new CreateExerciseRequest
        {
            Name = "Invalid Exercise",
            Description = "Can't be both REST and Workout",
            DifficultyId = DifficultyLevelId.New().ToString(),
            ExerciseTypeIds = new List<string> { restTypeId.ToString(), workoutTypeId.ToString() },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>(), // Empty muscle groups
            EquipmentIds = new List<string>(),
            MovementPatternIds = new List<string>(),
            BodyPartIds = new List<string>(),
            KineticChainId = KineticChainTypeId.New().ToString() // Mixed types
        };
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), null))
            .ReturnsAsync(false);
        
        _exerciseTypeRepositoryMock.Setup(r => r.GetByIdAsync(restTypeId))
            .ReturnsAsync(restType);
        _exerciseTypeRepositoryMock.Setup(r => r.GetByIdAsync(workoutTypeId))
            .ReturnsAsync(workoutType);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _exerciseService.CreateAsync(request));
        
        // This should fail on REST exclusivity, not muscle group validation
        Assert.Equal("Exercise type 'Rest' cannot be combined with other exercise types.", exception.Message);
    }
}