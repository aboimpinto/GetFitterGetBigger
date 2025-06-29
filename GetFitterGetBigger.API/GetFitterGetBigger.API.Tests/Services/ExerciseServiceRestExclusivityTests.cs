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
    private readonly Mock<IExerciseTypeRepository> _exerciseTypeRepositoryMock;
    private readonly ExerciseService _exerciseService;
    
    public ExerciseServiceRestExclusivityTests()
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
                "exercisetype-11111111-1111-1111-1111-111111111111",
                "exercisetype-22222222-2222-2222-2222-222222222222"
            },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>(),
            EquipmentIds = new List<string>(),
            MovementPatternIds = new List<string>(),
            BodyPartIds = new List<string>()
        };
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), null))
            .ReturnsAsync(false);
        
        // Mock ExerciseTypes: one Rest and one non-Rest
        // Create entities with IDs that match the string IDs in the request
        var restTypeId = ExerciseTypeId.From(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        var warmupTypeId = ExerciseTypeId.From(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        
        var restType = ExerciseType.Handler.Create(restTypeId, "Rest", "Rest period", 1, true);
        var warmupType = ExerciseType.Handler.Create(warmupTypeId, "Warmup", "Warmup exercise", 2, true);
        
        _exerciseTypeRepositoryMock.Setup(r => r.GetByIdAsync(restTypeId))
            .ReturnsAsync(restType);
        
        _exerciseTypeRepositoryMock.Setup(r => r.GetByIdAsync(warmupTypeId))
            .ReturnsAsync(warmupType);
        
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
                "exercisetype-11111111-1111-1111-1111-111111111111",
                "exercisetype-33333333-3333-3333-3333-333333333333"
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
        
        // Mock ExerciseTypes for the update test: one Rest and one Cooldown
        var restTypeId = ExerciseTypeId.From(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        var cooldownTypeId = ExerciseTypeId.From(Guid.Parse("33333333-3333-3333-3333-333333333333"));
        
        var restType = ExerciseType.Handler.Create(restTypeId, "Rest", "Rest period", 1, true);
        var cooldownType = ExerciseType.Handler.Create(cooldownTypeId, "Cooldown", "Cooldown exercise", 3, true);
        
        _exerciseTypeRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<ExerciseTypeId>()))
            .Returns<ExerciseTypeId>(id =>
            {
                if (ExerciseTypeId.TryParse("exercisetype-11111111-1111-1111-1111-111111111111", out var parsedRestId) && id == parsedRestId)
                    return Task.FromResult<ExerciseType?>(restType);
                if (ExerciseTypeId.TryParse("exercisetype-33333333-3333-3333-3333-333333333333", out var parsedCooldownId) && id == parsedCooldownId)
                    return Task.FromResult<ExerciseType?>(cooldownType);
                return Task.FromResult<ExerciseType?>(null);
            });
        
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
                "exercisetype-11111111-1111-1111-1111-111111111111"
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
        
        // Mock ExerciseType for the single Rest type test
        var restTypeId = ExerciseTypeId.From(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        var restType = ExerciseType.Handler.Create(restTypeId, "Rest", "Rest period", 1, true);
        
        _exerciseTypeRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<ExerciseTypeId>()))
            .Returns<ExerciseTypeId>(id =>
            {
                if (ExerciseTypeId.TryParse("exercisetype-11111111-1111-1111-1111-111111111111", out var parsedRestId) && id == parsedRestId)
                    return Task.FromResult<ExerciseType?>(restType);
                return Task.FromResult<ExerciseType?>(null);
            });
        
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
                "exercisetype-11223344-5566-7788-99aa-bbccddeeff00", // Warmup
                "exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e", // Workout
                "exercisetype-33445566-7788-99aa-bbcc-ddeeff001122"  // Cooldown
            },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>
            {
                new() { MuscleGroupId = "musclegroup-chest-123", MuscleRoleId = "musclerole-primary-456" }
            },
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
        
        // Mock ExerciseTypes for the multiple non-rest types
        var warmupTypeId = ExerciseTypeId.From(Guid.Parse("11223344-5566-7788-99aa-bbccddeeff00"));
        var workoutTypeId = ExerciseTypeId.From(Guid.Parse("b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e"));
        var cooldownTypeId = ExerciseTypeId.From(Guid.Parse("33445566-7788-99aa-bbcc-ddeeff001122"));
        
        var warmupType = ExerciseType.Handler.Create(warmupTypeId, "Warmup", "Warmup exercise", 1, true);
        var workoutType = ExerciseType.Handler.Create(workoutTypeId, "Workout", "Workout exercise", 2, true);
        var cooldownType = ExerciseType.Handler.Create(cooldownTypeId, "Cooldown", "Cooldown exercise", 3, true);
        
        _exerciseTypeRepositoryMock.Setup(r => r.GetByIdAsync(warmupTypeId))
            .ReturnsAsync(warmupType);
        _exerciseTypeRepositoryMock.Setup(r => r.GetByIdAsync(workoutTypeId))
            .ReturnsAsync(workoutType);
        _exerciseTypeRepositoryMock.Setup(r => r.GetByIdAsync(cooldownTypeId))
            .ReturnsAsync(cooldownType);
        
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
            MuscleGroups = new List<MuscleGroupWithRoleRequest>
            {
                new() { MuscleGroupId = "musclegroup-chest-123", MuscleRoleId = "musclerole-primary-456" }
            },
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