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

public class ExerciseServiceRestExclusivityTests
{
    private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _unitOfWorkProviderMock;
    private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _readOnlyUnitOfWorkMock;
    private readonly Mock<IWritableUnitOfWork<FitnessDbContext>> _writableUnitOfWorkMock;
    private readonly Mock<IExerciseRepository> _exerciseRepositoryMock;
    private readonly ExerciseService _exerciseService;
    
    public ExerciseServiceRestExclusivityTests()
    {
        _unitOfWorkProviderMock = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
        _readOnlyUnitOfWorkMock = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
        _writableUnitOfWorkMock = new Mock<IWritableUnitOfWork<FitnessDbContext>>();
        _exerciseRepositoryMock = new Mock<IExerciseRepository>();
        
        _readOnlyUnitOfWorkMock.Setup(uow => uow.GetRepository<IExerciseRepository>())
            .Returns(_exerciseRepositoryMock.Object);
        
        _writableUnitOfWorkMock.Setup(uow => uow.GetRepository<IExerciseRepository>())
            .Returns(_exerciseRepositoryMock.Object);
        
        _unitOfWorkProviderMock.Setup(p => p.CreateReadOnly())
            .Returns(_readOnlyUnitOfWorkMock.Object);
        
        _unitOfWorkProviderMock.Setup(p => p.CreateWritable())
            .Returns(_writableUnitOfWorkMock.Object);
        
        _exerciseService = new ExerciseService(_unitOfWorkProviderMock.Object);
    }
    
    [Fact]
    public async Task CreateAsync_WithRestTypeAndOtherTypes_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new CreateExerciseRequest
        {
            Name = "Test Exercise",
            Description = "Test Description",
            DifficultyId = DifficultyLevelId.New().ToString(),
            ExerciseTypeIds = new List<string>
            {
                "exercisetype-rest-12345678-1234-1234-1234-123456789012",
                "exercisetype-warmup-12345678-1234-1234-1234-123456789012"
            },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>(),
            EquipmentIds = new List<string>(),
            MovementPatternIds = new List<string>(),
            BodyPartIds = new List<string>()
        };
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), null))
            .ReturnsAsync(false);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _exerciseService.CreateAsync(request));
        
        Assert.Equal("Exercise type 'Rest' cannot be combined with other exercise types.", exception.Message);
    }
    
    [Fact]
    public async Task UpdateAsync_WithRestTypeAndOtherTypes_ThrowsInvalidOperationException()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var exercise = Exercise.Handler.CreateNew(
            "Existing Exercise",
            "Description",
            null,
            null,
            false,
            DifficultyLevelId.New());
        
        var request = new UpdateExerciseRequest
        {
            Name = "Updated Exercise",
            Description = "Updated Description",
            DifficultyId = DifficultyLevelId.New().ToString(),
            ExerciseTypeIds = new List<string>
            {
                "exercisetype-rest-12345678-1234-1234-1234-123456789012",
                "exercisetype-cooldown-12345678-1234-1234-1234-123456789012"
            },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>(),
            EquipmentIds = new List<string>(),
            MovementPatternIds = new List<string>(),
            BodyPartIds = new List<string>()
        };
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<ExerciseId>()))
            .ReturnsAsync(false);
        
        _exerciseRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<ExerciseId>()))
            .ReturnsAsync(exercise);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _exerciseService.UpdateAsync(exerciseId.ToString(), request));
        
        Assert.Equal("Exercise type 'Rest' cannot be combined with other exercise types.", exception.Message);
    }
    
    [Fact]
    public async Task CreateAsync_WithOnlyRestType_DoesNotThrow()
    {
        // Arrange
        var request = new CreateExerciseRequest
        {
            Name = "Rest Exercise",
            Description = "Rest Description",
            DifficultyId = DifficultyLevelId.New().ToString(),
            ExerciseTypeIds = new List<string>
            {
                "exercisetype-rest-12345678-1234-1234-1234-123456789012"
            },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>(),
            EquipmentIds = new List<string>(),
            MovementPatternIds = new List<string>(),
            BodyPartIds = new List<string>()
        };
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), null))
            .ReturnsAsync(false);
        
        _exerciseRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Exercise>()))
            .ReturnsAsync((Exercise e) => e);
        
        _writableUnitOfWorkMock.Setup(uow => uow.CommitAsync())
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _exerciseService.CreateAsync(request);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Rest Exercise", result.Name);
    }
    
    [Fact]
    public async Task CreateAsync_WithMultipleNonRestTypes_DoesNotThrow()
    {
        // Arrange
        var request = new CreateExerciseRequest
        {
            Name = "Complex Exercise",
            Description = "Complex Description",
            DifficultyId = DifficultyLevelId.New().ToString(),
            ExerciseTypeIds = new List<string>
            {
                ExerciseTypeId.New().ToString(),
                ExerciseTypeId.New().ToString(),
                ExerciseTypeId.New().ToString()
            },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>(),
            EquipmentIds = new List<string>(),
            MovementPatternIds = new List<string>(),
            BodyPartIds = new List<string>()
        };
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), null))
            .ReturnsAsync(false);
        
        _exerciseRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Exercise>()))
            .ReturnsAsync((Exercise e) => e);
        
        _writableUnitOfWorkMock.Setup(uow => uow.CommitAsync())
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _exerciseService.CreateAsync(request);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Complex Exercise", result.Name);
        Assert.Equal(3, result.ExerciseTypes.Count);
    }
    
    [Fact]
    public async Task CreateAsync_WithEmptyExerciseTypes_DoesNotThrow()
    {
        // Arrange
        var request = new CreateExerciseRequest
        {
            Name = "No Type Exercise",
            Description = "No Type Description",
            DifficultyId = DifficultyLevelId.New().ToString(),
            ExerciseTypeIds = new List<string>(),
            MuscleGroups = new List<MuscleGroupWithRoleRequest>(),
            EquipmentIds = new List<string>(),
            MovementPatternIds = new List<string>(),
            BodyPartIds = new List<string>()
        };
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), null))
            .ReturnsAsync(false);
        
        _exerciseRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Exercise>()))
            .ReturnsAsync((Exercise e) => e);
        
        _writableUnitOfWorkMock.Setup(uow => uow.CommitAsync())
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _exerciseService.CreateAsync(request);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("No Type Exercise", result.Name);
        Assert.Empty(result.ExerciseTypes);
    }
}