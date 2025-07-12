using System;
using System.Collections.Generic;
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

namespace GetFitterGetBigger.API.Tests.Services;

public class ExerciseServiceEquipmentValidationTests
{
    private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _unitOfWorkProviderMock;
    private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _readOnlyUnitOfWorkMock;
    private readonly Mock<IWritableUnitOfWork<FitnessDbContext>> _writableUnitOfWorkMock;
    private readonly Mock<IExerciseRepository> _exerciseRepositoryMock;
    private readonly Mock<IExerciseTypeService> _mockExerciseTypeService;
    private readonly IExerciseService _exerciseService;
    
    public ExerciseServiceEquipmentValidationTests()
    {
        _unitOfWorkProviderMock = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
        _readOnlyUnitOfWorkMock = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
        _writableUnitOfWorkMock = new Mock<IWritableUnitOfWork<FitnessDbContext>>();
        _exerciseRepositoryMock = new Mock<IExerciseRepository>();
        _mockExerciseTypeService = new Mock<IExerciseTypeService>();
        
        _readOnlyUnitOfWorkMock.Setup(uow => uow.GetRepository<IExerciseRepository>())
            .Returns(_exerciseRepositoryMock.Object);
        
        _writableUnitOfWorkMock.Setup(uow => uow.GetRepository<IExerciseRepository>())
            .Returns(_exerciseRepositoryMock.Object);
        
        _unitOfWorkProviderMock.Setup(p => p.CreateReadOnly())
            .Returns(_readOnlyUnitOfWorkMock.Object);
        
        _unitOfWorkProviderMock.Setup(p => p.CreateWritable())
            .Returns(_writableUnitOfWorkMock.Object);
        
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
        
        _exerciseService = new ExerciseService(_unitOfWorkProviderMock.Object, _mockExerciseTypeService.Object);
    }
    
    [Fact]
    public async Task CreateAsync_RestExerciseWithoutEquipment_CreatesSuccessfully()
    {
        // Arrange
        var restTypeId = ExerciseTypeId.New();
        var restType = ExerciseType.Handler.Create(restTypeId, "Rest", "Rest period", 1, true);
        
        var request = CreateExerciseRequestBuilder.ForRestExercise()
            .WithName("Rest Period")
            .WithDescription("Recovery time between sets")
            .WithExerciseTypes(restTypeId.ToString())
            .Build();
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), null))
            .ReturnsAsync(false);
        
        // Override the default mock to return true for REST type
        _mockExerciseTypeService
            .Setup(s => s.AnyIsRestTypeAsync(It.Is<IEnumerable<string>>(ids => 
                ids.Contains(restTypeId.ToString()))))
            .ReturnsAsync(true);
        
        _exerciseRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Exercise>()))
            .ReturnsAsync((Exercise e) => e);
        
        _writableUnitOfWorkMock.Setup(uow => uow.CommitAsync())
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _exerciseService.CreateAsync(request.ToCommand());
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess, $"Create failed with errors: {string.Join(", ", result.Errors)}");
        Assert.Equal("Rest Period", result.Data.Name);
        Assert.Empty(result.Data.Equipment); // Equipment is empty and that's OK
        _exerciseRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Exercise>()), Times.Once);
    }
    
    [Fact]
    public async Task CreateAsync_NonRestExerciseWithoutEquipment_CreatesSuccessfully()
    {
        // Arrange
        var workoutTypeId = ExerciseTypeId.New();
        var workoutType = ExerciseType.Handler.Create(workoutTypeId, "Workout", "Main workout", 1, false);
        
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Push Up")
            .WithDescription("Bodyweight upper body exercise")
            .WithExerciseTypes(workoutTypeId.ToString())
            .WithMuscleGroups((MuscleGroupId.New().ToString(), MuscleRoleId.New().ToString()))
            .WithEquipmentIds() // Empty equipment for bodyweight exercise
            .Build();
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), null))
            .ReturnsAsync(false);
        
        // Override the default mock to return false for non-REST type
        _mockExerciseTypeService
            .Setup(s => s.AnyIsRestTypeAsync(It.Is<IEnumerable<string>>(ids => 
                ids.Contains(workoutTypeId.ToString()))))
            .ReturnsAsync(false);
        
        _exerciseRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Exercise>()))
            .ReturnsAsync((Exercise e) => e);
        
        _writableUnitOfWorkMock.Setup(uow => uow.CommitAsync())
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _exerciseService.CreateAsync(request.ToCommand());
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess, $"Create failed with errors: {string.Join(", ", result.Errors)}");
        Assert.Equal("Push Up", result.Data.Name);
        Assert.Empty(result.Data.Equipment); // Equipment is empty and that's OK for bodyweight exercises
        _exerciseRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Exercise>()), Times.Once);
    }
    
    [Fact]
    public async Task CreateAsync_ExerciseWithEquipment_CreatesSuccessfully()
    {
        // Arrange
        var workoutTypeId = ExerciseTypeId.New();
        var workoutType = ExerciseType.Handler.Create(workoutTypeId, "Workout", "Main workout", 1, false);
        var equipmentId = EquipmentId.New();
        
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Bench Press")
            .WithDescription("Chest exercise with barbell")
            .WithExerciseTypes(workoutTypeId.ToString())
            .WithMuscleGroups((MuscleGroupId.New().ToString(), MuscleRoleId.New().ToString()))
            .Build();
        
        // Add equipment manually for this specific test
        request.EquipmentIds = new List<string> { equipmentId.ToString() };
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), null))
            .ReturnsAsync(false);
        
        // Override the default mock to return false for non-REST type
        _mockExerciseTypeService
            .Setup(s => s.AnyIsRestTypeAsync(It.Is<IEnumerable<string>>(ids => 
                ids.Contains(workoutTypeId.ToString()))))
            .ReturnsAsync(false);
        
        Exercise? capturedExercise = null;
        _exerciseRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Exercise>()))
            .Callback<Exercise>(e => capturedExercise = e)
            .ReturnsAsync((Exercise e) => e);
        
        _writableUnitOfWorkMock.Setup(uow => uow.CommitAsync())
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _exerciseService.CreateAsync(request.ToCommand());
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess, $"Create failed with errors: {string.Join(", ", result.Errors)}");
        
        // Verify the entity was created with equipment
        Assert.NotNull(capturedExercise);
        Assert.Single(capturedExercise.ExerciseEquipment);
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
        var request = UpdateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Push Up")
            .WithDescription("Changed to bodyweight exercise")
            .WithExerciseTypes(workoutTypeId.ToString())
            .WithMuscleGroups((MuscleGroupId.New().ToString(), MuscleRoleId.New().ToString()))
            .Build();
        
        // Remove all equipment for this specific test
        request.EquipmentIds = new List<string>();
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<ExerciseId>()))
            .ReturnsAsync(false);
        
        _exerciseRepositoryMock.Setup(r => r.GetByIdAsync(exerciseId))
            .ReturnsAsync(existingExercise);
        
        // Override the default mock to return false for non-REST type
        _mockExerciseTypeService
            .Setup(s => s.AnyIsRestTypeAsync(It.Is<IEnumerable<string>>(ids => 
                ids.Contains(workoutTypeId.ToString()))))
            .ReturnsAsync(false);
        
        Exercise? capturedExercise = null;
        _exerciseRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Exercise>()))
            .Callback<Exercise>(e => capturedExercise = e)
            .ReturnsAsync((Exercise e) => e);
        
        _writableUnitOfWorkMock.Setup(uow => uow.CommitAsync())
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _exerciseService.UpdateAsync(ExerciseId.ParseOrEmpty(exerciseId.ToString()), request.ToCommand());
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess, $"Update failed with errors: {string.Join(", ", result.Errors)}");
        
        // Verify the entity was updated with equipment removed
        Assert.NotNull(capturedExercise);
        Assert.Empty(capturedExercise.ExerciseEquipment);
        _exerciseRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Exercise>()), Times.Once);
    }
}