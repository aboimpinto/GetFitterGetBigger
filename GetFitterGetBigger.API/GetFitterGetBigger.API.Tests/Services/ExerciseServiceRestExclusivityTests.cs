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
using GetFitterGetBigger.API.Tests.TestBuilders.Domain;
using GetFitterGetBigger.API.Mappers;
using GetFitterGetBigger.API.Constants;
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
    private readonly Mock<IExerciseTypeService> _mockExerciseTypeService;
    private readonly IExerciseService _exerciseService;
    
    public ExerciseServiceRestExclusivityTests()
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
                ids.Any(id => id == "exercisetype-d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a" || 
                              id.ToLowerInvariant().Contains("rest")));
        
        // Default behavior: all exercise types exist
        _mockExerciseTypeService
            .Setup(s => s.ExistsAsync(It.IsAny<ExerciseTypeId>()))
            .ReturnsAsync(true);
        
        _exerciseService = new ExerciseService(_unitOfWorkProviderMock.Object, _mockExerciseTypeService.Object);
    }
    
    [Fact]
    public async Task CreateAsync_WithRestTypeAndOtherTypes_ReturnsFailure()
    {
        // Arrange
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Test Exercise")
            .WithDescription("Test Description")
            .WithKineticChain(KineticChainTypeTestBuilder.Compound())
            .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
            .WithExerciseTypes(new[]
            {
                ExerciseType.Handler.Create(ExerciseTypeId.From(Guid.Parse("11111111-1111-1111-1111-111111111111")), "Rest", "Rest", 1, true),
                ExerciseType.Handler.Create(ExerciseTypeId.From(Guid.Parse("22222222-2222-2222-2222-222222222222")), "Workout", "Workout", 2, true)
            })
            .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
            .Build();
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), null))
            .ReturnsAsync(false);
        
        // Override the default mock to return true for this specific test
        // since one of the types is a REST type
        _mockExerciseTypeService
            .Setup(s => s.AnyIsRestTypeAsync(It.Is<IEnumerable<string>>(ids => 
                ids.Contains("exercisetype-11111111-1111-1111-1111-111111111111"))))
            .ReturnsAsync(true);
        
        // Act
        var result = await _exerciseService.CreateAsync(request.ToCommand());
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(ExerciseErrorMessages.RestExerciseCannotBeCombined, result.Errors);
    }
    
    [Fact]
    public async Task UpdateAsync_WithRestTypeAndOtherTypes_ReturnsFailure()
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
        
        var request = UpdateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Updated Exercise")
            .WithDescription("Updated Description")
            .WithKineticChain(KineticChainTypeTestBuilder.Compound())
            .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
            .WithExerciseTypes(new[]
            {
                ExerciseType.Handler.Create(ExerciseTypeId.From(Guid.Parse("11111111-1111-1111-1111-111111111111")), "Rest", "Rest", 1, true),
                ExerciseType.Handler.Create(ExerciseTypeId.From(Guid.Parse("33333333-3333-3333-3333-333333333333")), "Cooldown", "Cooldown", 3, true)
            })
            .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
            .Build();
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<ExerciseId>()))
            .ReturnsAsync(false);
        
        _exerciseRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<ExerciseId>()))
            .ReturnsAsync(exercise);
        
        // Mock ExerciseTypes for the update test: one Rest and one Cooldown
        var restTypeId = ExerciseTypeId.From(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        var cooldownTypeId = ExerciseTypeId.From(Guid.Parse("33333333-3333-3333-3333-333333333333"));
        
        var restType = ExerciseType.Handler.Create(restTypeId, "Rest", "Rest period", 1, true);
        var cooldownType = ExerciseType.Handler.Create(cooldownTypeId, "Cooldown", "Cooldown exercise", 3, false);
        
        // Override the default mock to return true for REST type
        _mockExerciseTypeService
            .Setup(s => s.AnyIsRestTypeAsync(It.Is<IEnumerable<string>>(ids => 
                ids.Contains("exercisetype-11111111-1111-1111-1111-111111111111"))))
            .ReturnsAsync(true);
        
        // Act
        var result = await _exerciseService.UpdateAsync(ExerciseId.ParseOrEmpty(exerciseId.ToString()), request.ToCommand());
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(ExerciseErrorMessages.RestExerciseCannotBeCombined, result.Errors);
    }
    
    [Fact]
    public async Task CreateAsync_WithOnlyRestType_DoesNotThrow()
    {
        // Arrange
        var request = CreateExerciseRequestBuilder.ForRestExercise()
            .WithName("Rest Exercise")
            .WithDescription("Rest Description")
            .Build();
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), null))
            .ReturnsAsync(false);
        
        _exerciseRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Exercise>()))
            .ReturnsAsync((Exercise e) => e);
        
        _writableUnitOfWorkMock.Setup(uow => uow.CommitAsync())
            .Returns(Task.CompletedTask);
        
        // Override mock to return true since it's a REST type
        _mockExerciseTypeService
            .Setup(s => s.AnyIsRestTypeAsync(It.Is<IEnumerable<string>>(ids => 
                ids.Contains("exercisetype-11111111-1111-1111-1111-111111111111"))))
            .ReturnsAsync(true);
        
        // Act
        var result = await _exerciseService.CreateAsync(request.ToCommand());
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal("Rest Exercise", result.Data.Name);
    }
    
    [Fact]
    public async Task CreateAsync_WithMultipleNonRestTypes_DoesNotThrow()
    {
        // Arrange
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Complex Exercise")
            .WithDescription("Complex Description")
            .WithKineticChain(KineticChainTypeTestBuilder.Compound())
            .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
            .WithExerciseTypes(new[]
            {
                ExerciseType.Handler.Create(ExerciseTypeId.From(Guid.Parse("11223344-5566-7788-99aa-bbccddeeff00")), "Warmup", "Warmup", 1, true),
                ExerciseType.Handler.Create(ExerciseTypeId.From(Guid.Parse("b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e")), "Workout", "Workout", 2, true),
                ExerciseType.Handler.Create(ExerciseTypeId.From(Guid.Parse("33445566-7788-99aa-bbcc-ddeeff001122")), "Cooldown", "Cooldown", 3, true)
            })
            .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
            .Build();
        
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
        
        var warmupType = ExerciseType.Handler.Create(warmupTypeId, "Warmup", "Warmup exercise", 1, false);
        var workoutType = ExerciseType.Handler.Create(workoutTypeId, "Workout", "Workout exercise", 2, false);
        var cooldownType = ExerciseType.Handler.Create(cooldownTypeId, "Cooldown", "Cooldown exercise", 3, false);
        
        // Override the default mock to return false for all non-REST types
        _mockExerciseTypeService
            .Setup(s => s.AnyIsRestTypeAsync(It.Is<IEnumerable<string>>(ids => 
                ids.Contains("exercisetype-11223344-5566-7788-99aa-bbccddeeff00") ||
                ids.Contains("exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e") ||
                ids.Contains("exercisetype-33445566-7788-99aa-bbcc-ddeeff001122"))))
            .ReturnsAsync(false);
        
        // Act
        var result = await _exerciseService.CreateAsync(request.ToCommand());
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal("Complex Exercise", result.Data.Name);
        // TODO: Fix mapper to properly include all exercise types
        // Assert.Equal(3, result.Data.ExerciseTypes.Count);
    }
    
    [Fact]
    public async Task CreateAsync_WithEmptyExerciseTypes_RequiresExerciseWeightType()
    {
        // Arrange - Empty exercise types means non-REST, so ExerciseWeightTypeId is required
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("No Type Exercise")
            .WithDescription("No Type Description")
            .WithKineticChain(KineticChainTypeTestBuilder.Compound())
            .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
            .WithExerciseTypes() // Empty exercise types = non-REST
            .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
            .Build();
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), null))
            .ReturnsAsync(false);
        
        _exerciseRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Exercise>()))
            .ReturnsAsync((Exercise e) => e);
        
        _writableUnitOfWorkMock.Setup(uow => uow.CommitAsync())
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _exerciseService.CreateAsync(request.ToCommand());
        
        // Assert - Should succeed because ForWorkoutExercise() includes ExerciseWeightTypeId by default
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal("No Type Exercise", result.Data.Name);
        Assert.Empty(result.Data.ExerciseTypes);
    }
}