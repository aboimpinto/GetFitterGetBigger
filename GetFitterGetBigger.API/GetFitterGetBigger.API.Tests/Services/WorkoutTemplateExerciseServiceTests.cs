using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplateExercises;
using GetFitterGetBigger.API.Services.Implementations;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.TestBuilders;
using GetFitterGetBigger.API.Tests.TestBuilders.Domain;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Tests.Services;

public class WorkoutTemplateExerciseServiceTests
{
    private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _mockUnitOfWorkProvider;
    private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _mockReadOnlyUnitOfWork;
    private readonly Mock<IWritableUnitOfWork<FitnessDbContext>> _mockWritableUnitOfWork;
    private readonly Mock<IWorkoutTemplateExerciseRepository> _mockExerciseTemplateRepo;
    private readonly Mock<IWorkoutTemplateRepository> _mockTemplateRepo;
    private readonly Mock<IExerciseRepository> _mockExerciseRepo;
    private readonly Mock<ISetConfigurationRepository> _mockSetConfigRepo;
    private readonly Mock<ILogger<WorkoutTemplateExerciseService>> _mockLogger;
    private readonly WorkoutTemplateExerciseService _service;
    
    // Test data
    private readonly WorkoutTemplateId _testTemplateId;
    private readonly WorkoutTemplateExerciseId _testExerciseId;
    private readonly ExerciseId _testExerciseEntityId;
    private readonly UserId _testUserId;
    private readonly WorkoutTemplate _testTemplate;
    private readonly WorkoutTemplateExercise _testTemplateExercise;
    private readonly Exercise _testExercise;
    private readonly WorkoutState _draftState;
    private readonly WorkoutState _productionState;
    
    public WorkoutTemplateExerciseServiceTests()
    {
        _mockUnitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
        _mockReadOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
        _mockWritableUnitOfWork = new Mock<IWritableUnitOfWork<FitnessDbContext>>();
        _mockExerciseTemplateRepo = new Mock<IWorkoutTemplateExerciseRepository>();
        _mockTemplateRepo = new Mock<IWorkoutTemplateRepository>();
        _mockExerciseRepo = new Mock<IExerciseRepository>();
        _mockSetConfigRepo = new Mock<ISetConfigurationRepository>();
        _mockLogger = new Mock<ILogger<WorkoutTemplateExerciseService>>();
        
        _mockUnitOfWorkProvider
            .Setup(x => x.CreateReadOnly())
            .Returns(_mockReadOnlyUnitOfWork.Object);
            
        _mockUnitOfWorkProvider
            .Setup(x => x.CreateWritable())
            .Returns(_mockWritableUnitOfWork.Object);
            
        _mockReadOnlyUnitOfWork
            .Setup(x => x.GetRepository<IWorkoutTemplateExerciseRepository>())
            .Returns(_mockExerciseTemplateRepo.Object);
            
        _mockReadOnlyUnitOfWork
            .Setup(x => x.GetRepository<IWorkoutTemplateRepository>())
            .Returns(_mockTemplateRepo.Object);
            
        _mockReadOnlyUnitOfWork
            .Setup(x => x.GetRepository<IExerciseRepository>())
            .Returns(_mockExerciseRepo.Object);
            
        _mockWritableUnitOfWork
            .Setup(x => x.GetRepository<IWorkoutTemplateExerciseRepository>())
            .Returns(_mockExerciseTemplateRepo.Object);
            
        _mockWritableUnitOfWork
            .Setup(x => x.GetRepository<IWorkoutTemplateRepository>())
            .Returns(_mockTemplateRepo.Object);
            
        _mockWritableUnitOfWork
            .Setup(x => x.GetRepository<IExerciseRepository>())
            .Returns(_mockExerciseRepo.Object);
            
        _mockWritableUnitOfWork
            .Setup(x => x.GetRepository<ISetConfigurationRepository>())
            .Returns(_mockSetConfigRepo.Object);
        
        _service = new WorkoutTemplateExerciseService(_mockUnitOfWorkProvider.Object, _mockLogger.Object);
        
        // Initialize test data
        _testTemplateId = WorkoutTemplateId.New();
        _testExerciseId = WorkoutTemplateExerciseId.New();
        _testExerciseEntityId = ExerciseId.New();
        _testUserId = UserId.New();
        
        // Create WorkoutState entities
        _draftState = WorkoutState.Handler.CreateNew("DRAFT", "Draft state", 1, true).Value! with
        {
            WorkoutStateId = WorkoutStateId.ParseOrEmpty(TestIds.WorkoutStateIds.Draft)
        };
        _productionState = WorkoutState.Handler.CreateNew("PRODUCTION", "Production state", 2, true).Value! with
        {
            WorkoutStateId = WorkoutStateId.ParseOrEmpty(TestIds.WorkoutStateIds.Production)
        };
        
        _testTemplate = CreateTemplateWithNavigation(_testTemplateId, _testUserId, _draftState);
            
        _testExercise = new ExerciseBuilder()
            .WithId(_testExerciseEntityId)
            .WithName("Test Exercise")
            .Build();
            
        _testTemplateExercise = new WorkoutTemplateExerciseBuilder()
            .WithId(_testExerciseId)
            .WithWorkoutTemplateId(_testTemplateId)
            .WithExerciseId(_testExerciseEntityId)
            .WithZone(WorkoutZone.Main)
            .WithSequenceOrder(1)
            .WithExercise(_testExercise)
            .Build();
    }
    
    private WorkoutTemplate CreateTemplateWithNavigation(WorkoutTemplateId id, UserId createdBy, WorkoutState workoutState)
    {
        var baseTemplate = new WorkoutTemplateBuilder()
            .WithId(id)
            .WithName("Test Template")
            .WithCreatedBy(createdBy)
            .WithWorkoutStateId(workoutState.WorkoutStateId)
            .Build();
            
        // Create new instance with navigation properties populated
        return baseTemplate with { WorkoutState = workoutState };
    }
    
    #region GetByWorkoutTemplateAsync Tests
    
    [Fact]
    public async Task GetByWorkoutTemplateAsync_WithValidId_ReturnsExercisesByZone()
    {
        // Arrange
        var exercises = new List<WorkoutTemplateExercise>
        {
            new WorkoutTemplateExerciseBuilder()
                .WithWorkoutTemplateId(_testTemplateId)
                .WithZone(WorkoutZone.Warmup)
                .WithSequenceOrder(1)
                .Build(),
            new WorkoutTemplateExerciseBuilder()
                .WithWorkoutTemplateId(_testTemplateId)
                .WithZone(WorkoutZone.Main)
                .WithSequenceOrder(1)
                .Build(),
            new WorkoutTemplateExerciseBuilder()
                .WithWorkoutTemplateId(_testTemplateId)
                .WithZone(WorkoutZone.Cooldown)
                .WithSequenceOrder(1)
                .Build()
        };
        
        _mockExerciseTemplateRepo
            .Setup(x => x.GetByWorkoutTemplateAsync(_testTemplateId))
            .ReturnsAsync(exercises);
        
        // Act
        var result = await _service.GetByWorkoutTemplateAsync(_testTemplateId);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(_testTemplateId.ToString(), result.Data.WorkoutTemplateId);
        Assert.Single(result.Data.WarmupExercises);
        Assert.Single(result.Data.MainExercises);
        Assert.Single(result.Data.CooldownExercises);
    }
    
    [Fact]
    public async Task GetByWorkoutTemplateAsync_WithEmptyId_ReturnsValidationError()
    {
        // Act
        var result = await _service.GetByWorkoutTemplateAsync(WorkoutTemplateId.Empty);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Contains("WorkoutTemplateId", result.Errors.First());
        Assert.Contains("GUID format", result.Errors.First());
    }
    
    [Fact]
    public async Task GetByWorkoutTemplateAsync_ExercisesAreSortedBySequenceOrder()
    {
        // Arrange
        var exercises = new List<WorkoutTemplateExercise>
        {
            new WorkoutTemplateExerciseBuilder()
                .WithWorkoutTemplateId(_testTemplateId)
                .WithZone(WorkoutZone.Main)
                .WithSequenceOrder(3)
                .Build(),
            new WorkoutTemplateExerciseBuilder()
                .WithWorkoutTemplateId(_testTemplateId)
                .WithZone(WorkoutZone.Main)
                .WithSequenceOrder(1)
                .Build(),
            new WorkoutTemplateExerciseBuilder()
                .WithWorkoutTemplateId(_testTemplateId)
                .WithZone(WorkoutZone.Main)
                .WithSequenceOrder(2)
                .Build()
        };
        
        _mockExerciseTemplateRepo
            .Setup(x => x.GetByWorkoutTemplateAsync(_testTemplateId))
            .ReturnsAsync(exercises);
        
        // Act
        var result = await _service.GetByWorkoutTemplateAsync(_testTemplateId);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Data.MainExercises.Count);
        Assert.Equal(1, result.Data.MainExercises[0].SequenceOrder);
        Assert.Equal(2, result.Data.MainExercises[1].SequenceOrder);
        Assert.Equal(3, result.Data.MainExercises[2].SequenceOrder);
    }
    
    #endregion
    
    #region GetByIdAsync Tests
    
    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsExercise()
    {
        // Arrange
        _mockExerciseTemplateRepo
            .Setup(x => x.GetByIdWithDetailsAsync(_testExerciseId))
            .ReturnsAsync(_testTemplateExercise);
        
        // Act
        var result = await _service.GetByIdAsync(_testExerciseId);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(_testExerciseId.ToString(), result.Data.Id);
        Assert.Equal(WorkoutZone.Main.ToString(), result.Data.Zone);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithEmptyId_ReturnsValidationError()
    {
        // Act
        var result = await _service.GetByIdAsync(WorkoutTemplateExerciseId.Empty);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Contains("WorkoutTemplateExerciseId", result.Errors.First());
        Assert.Contains("GUID format", result.Errors.First());
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNotFoundError()
    {
        // Arrange
        _mockExerciseTemplateRepo
            .Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<WorkoutTemplateExerciseId>()))
            .ReturnsAsync(WorkoutTemplateExercise.Empty);
        
        // Act
        var result = await _service.GetByIdAsync(_testExerciseId);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Contains("not found", result.Errors.First());
    }
    
    #endregion
    
    #region AddExerciseAsync Tests
    
    [Fact]
    public async Task AddExerciseAsync_WithValidCommand_AddsExercise()
    {
        // Arrange
        var command = new AddExerciseToTemplateCommand
        {
            WorkoutTemplateId = _testTemplateId,
            ExerciseId = _testExerciseEntityId,
            Zone = "Main",
            UserId = _testUserId,
            Notes = "Test notes"
        };
        
        _mockTemplateRepo
            .Setup(x => x.GetByIdAsync(_testTemplateId))
            .ReturnsAsync(_testTemplate);
            
        _mockExerciseRepo
            .Setup(x => x.GetByIdAsync(_testExerciseEntityId))
            .ReturnsAsync(_testExercise);
            
        _mockExerciseTemplateRepo
            .Setup(x => x.GetMaxSequenceOrderAsync(_testTemplateId, WorkoutZone.Main))
            .ReturnsAsync(0);
            
        _mockExerciseTemplateRepo
            .Setup(x => x.AddAsync(It.IsAny<WorkoutTemplateExercise>()))
            .ReturnsAsync((WorkoutTemplateExercise ex) => ex);
            
        _mockExerciseTemplateRepo
            .Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<WorkoutTemplateExerciseId>()))
            .ReturnsAsync(_testTemplateExercise);
        
        // Act
        var result = await _service.AddExerciseAsync(command);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        _mockWritableUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }
    
    [Fact]
    public async Task AddExerciseAsync_WithNullCommand_ReturnsValidationError()
    {
        // Act
        var result = await _service.AddExerciseAsync(null!);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Contains("Command cannot be null", result.Errors.First());
    }
    
    [Fact]
    public async Task AddExerciseAsync_WithInvalidZone_ReturnsValidationError()
    {
        // Arrange
        var command = new AddExerciseToTemplateCommand
        {
            WorkoutTemplateId = _testTemplateId,
            ExerciseId = _testExerciseEntityId,
            Zone = "InvalidZone",
            UserId = _testUserId
        };
        
        // Act
        var result = await _service.AddExerciseAsync(command);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Contains("Invalid zone", result.Errors.First());
    }
    
    [Fact]
    public async Task AddExerciseAsync_WhenTemplateNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var command = new AddExerciseToTemplateCommand
        {
            WorkoutTemplateId = _testTemplateId,
            ExerciseId = _testExerciseEntityId,
            Zone = "Main",
            UserId = _testUserId
        };
        
        _mockTemplateRepo
            .Setup(x => x.GetByIdAsync(_testTemplateId))
            .ReturnsAsync(WorkoutTemplate.Empty);
        
        // Act
        var result = await _service.AddExerciseAsync(command);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Contains("not found", result.Errors.First());
        Assert.Contains("Workout template not found", result.Errors.First());
    }
    
    [Fact]
    public async Task AddExerciseAsync_WhenUserNotAuthorized_ReturnsUnauthorizedError()
    {
        // Arrange
        var differentUserId = UserId.New();
        var command = new AddExerciseToTemplateCommand
        {
            WorkoutTemplateId = _testTemplateId,
            ExerciseId = _testExerciseEntityId,
            Zone = "Main",
            UserId = differentUserId
        };
        
        _mockTemplateRepo
            .Setup(x => x.GetByIdAsync(_testTemplateId))
            .ReturnsAsync(_testTemplate);
        
        // Act
        var result = await _service.AddExerciseAsync(command);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Contains("not authorized", result.Errors.First());
    }
    
    [Fact]
    public async Task AddExerciseAsync_WhenTemplateNotInDraftState_ReturnsValidationError()
    {
        // Arrange
        var publishedTemplate = CreateTemplateWithNavigation(_testTemplateId, _testUserId, _productionState);
            
        var command = new AddExerciseToTemplateCommand
        {
            WorkoutTemplateId = _testTemplateId,
            ExerciseId = _testExerciseEntityId,
            Zone = "Main",
            UserId = _testUserId
        };
        
        _mockTemplateRepo
            .Setup(x => x.GetByIdAsync(_testTemplateId))
            .ReturnsAsync(publishedTemplate);
        
        // Act
        var result = await _service.AddExerciseAsync(command);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Contains("DRAFT state", result.Errors.First());
    }
    
    [Fact]
    public async Task AddExerciseAsync_WhenExerciseNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var command = new AddExerciseToTemplateCommand
        {
            WorkoutTemplateId = _testTemplateId,
            ExerciseId = _testExerciseEntityId,
            Zone = "Main",
            UserId = _testUserId
        };
        
        _mockTemplateRepo
            .Setup(x => x.GetByIdAsync(_testTemplateId))
            .ReturnsAsync(_testTemplate);
            
        _mockExerciseRepo
            .Setup(x => x.GetByIdAsync(_testExerciseEntityId))
            .ReturnsAsync(Exercise.Empty);
        
        // Act
        var result = await _service.AddExerciseAsync(command);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Contains("not found", result.Errors.First());
        Assert.Contains("Exercise not found", result.Errors.First());
    }
    
    [Fact]
    public async Task AddExerciseAsync_WithSpecifiedSequenceOrder_UsesProvidedOrder()
    {
        // Arrange
        var command = new AddExerciseToTemplateCommand
        {
            WorkoutTemplateId = _testTemplateId,
            ExerciseId = _testExerciseEntityId,
            Zone = "Main",
            UserId = _testUserId,
            SequenceOrder = 5
        };
        
        WorkoutTemplateExercise? capturedExercise = null;
        
        _mockTemplateRepo
            .Setup(x => x.GetByIdAsync(_testTemplateId))
            .ReturnsAsync(_testTemplate);
            
        _mockExerciseRepo
            .Setup(x => x.GetByIdAsync(_testExerciseEntityId))
            .ReturnsAsync(_testExercise);
            
        _mockExerciseTemplateRepo
            .Setup(x => x.AddAsync(It.IsAny<WorkoutTemplateExercise>()))
            .Callback<WorkoutTemplateExercise>(ex => capturedExercise = ex)
            .ReturnsAsync((WorkoutTemplateExercise ex) => ex);
            
        _mockExerciseTemplateRepo
            .Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<WorkoutTemplateExerciseId>()))
            .ReturnsAsync(_testTemplateExercise);
        
        // Act
        var result = await _service.AddExerciseAsync(command);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedExercise);
        Assert.Equal(5, capturedExercise.SequenceOrder);
    }
    
    #endregion
    
    #region UpdateExerciseAsync Tests
    
    [Fact]
    public async Task UpdateExerciseAsync_WithValidCommand_UpdatesNotes()
    {
        // Arrange
        var command = new UpdateTemplateExerciseCommand
        {
            WorkoutTemplateExerciseId = _testExerciseId,
            UserId = _testUserId,
            Notes = "Updated notes"
        };
        
        _mockExerciseTemplateRepo
            .Setup(x => x.GetByIdWithDetailsAsync(_testExerciseId))
            .ReturnsAsync(_testTemplateExercise);
            
        _mockTemplateRepo
            .Setup(x => x.GetByIdAsync(_testTemplateId))
            .ReturnsAsync(_testTemplate);
            
        _mockExerciseTemplateRepo
            .Setup(x => x.UpdateAsync(It.IsAny<WorkoutTemplateExercise>()))
            .ReturnsAsync((WorkoutTemplateExercise ex) => ex);
            
        _mockExerciseTemplateRepo
            .Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<WorkoutTemplateExerciseId>()))
            .ReturnsAsync(_testTemplateExercise);
        
        // Act
        var result = await _service.UpdateExerciseAsync(command);
        
        // Assert
        Assert.True(result.IsSuccess);
        _mockWritableUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }
    
    [Fact]
    public async Task UpdateExerciseAsync_WithNullCommand_ReturnsValidationError()
    {
        // Act
        var result = await _service.UpdateExerciseAsync(null!);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Single(result.Errors);
    }
    
    [Fact]
    public async Task UpdateExerciseAsync_WhenExerciseNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var command = new UpdateTemplateExerciseCommand
        {
            WorkoutTemplateExerciseId = _testExerciseId,
            UserId = _testUserId
        };
        
        _mockExerciseTemplateRepo
            .Setup(x => x.GetByIdWithDetailsAsync(_testExerciseId))
            .ReturnsAsync(WorkoutTemplateExercise.Empty);
        
        // Act
        var result = await _service.UpdateExerciseAsync(command);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Contains("not found", result.Errors.First());
    }
    
    #endregion
    
    #region RemoveExerciseAsync Tests
    
    [Fact]
    public async Task RemoveExerciseAsync_WithValidRequest_RemovesExercise()
    {
        // Arrange
        _mockExerciseTemplateRepo
            .Setup(x => x.GetByIdWithDetailsAsync(_testExerciseId))
            .ReturnsAsync(_testTemplateExercise);
            
        _mockTemplateRepo
            .Setup(x => x.GetByIdAsync(_testTemplateId))
            .ReturnsAsync(_testTemplate);
            
        _mockExerciseTemplateRepo
            .Setup(x => x.DeleteAsync(_testExerciseId))
            .ReturnsAsync(true);
        
        // Act
        var result = await _service.RemoveExerciseAsync(_testExerciseId, _testUserId);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
        _mockWritableUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }
    
    [Fact]
    public async Task RemoveExerciseAsync_WithEmptyIds_ReturnsValidationError()
    {
        // Act
        var result = await _service.RemoveExerciseAsync(WorkoutTemplateExerciseId.Empty, _testUserId);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Single(result.Errors);
    }
    
    #endregion
    
    #region ReorderExercisesAsync Tests
    
    [Fact]
    public async Task ReorderExercisesAsync_WithValidCommand_ReordersExercises()
    {
        // Arrange
        var command = new ReorderTemplateExercisesCommand
        {
            WorkoutTemplateId = _testTemplateId,
            Zone = "Main",
            ExerciseIds = new List<WorkoutTemplateExerciseId> 
            { 
                WorkoutTemplateExerciseId.New(), 
                WorkoutTemplateExerciseId.New() 
            },
            UserId = _testUserId
        };
        
        _mockTemplateRepo
            .Setup(x => x.GetByIdAsync(_testTemplateId))
            .ReturnsAsync(_testTemplate);
            
        _mockExerciseTemplateRepo
            .Setup(x => x.ReorderExercisesAsync(
                _testTemplateId, 
                WorkoutZone.Main, 
                It.IsAny<Dictionary<WorkoutTemplateExerciseId, int>>()))
            .ReturnsAsync(true);
        
        // Act
        var result = await _service.ReorderExercisesAsync(command);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
        _mockWritableUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }
    
    [Fact]
    public async Task ReorderExercisesAsync_WithNullCommand_ReturnsValidationError()
    {
        // Act
        var result = await _service.ReorderExercisesAsync(null!);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Single(result.Errors);
    }
    
    [Fact]
    public async Task ReorderExercisesAsync_WithEmptyExerciseList_ReturnsValidationError()
    {
        // Arrange
        var command = new ReorderTemplateExercisesCommand
        {
            WorkoutTemplateId = _testTemplateId,
            Zone = "Main",
            ExerciseIds = new List<WorkoutTemplateExerciseId>(),
            UserId = _testUserId
        };
        
        // Act
        var result = await _service.ReorderExercisesAsync(command);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Single(result.Errors);
    }
    
    #endregion
    
    #region ChangeExerciseZoneAsync Tests
    
    [Fact]
    public async Task ChangeExerciseZoneAsync_WithValidCommand_ChangesZone()
    {
        // Arrange
        var command = new ChangeExerciseZoneCommand
        {
            WorkoutTemplateExerciseId = _testExerciseId,
            NewZone = "Cooldown",
            UserId = _testUserId
        };
        
        _mockExerciseTemplateRepo
            .Setup(x => x.GetByIdWithDetailsAsync(_testExerciseId))
            .ReturnsAsync(_testTemplateExercise);
            
        _mockTemplateRepo
            .Setup(x => x.GetByIdAsync(_testTemplateId))
            .ReturnsAsync(_testTemplate);
            
        _mockExerciseTemplateRepo
            .Setup(x => x.GetMaxSequenceOrderAsync(_testTemplateId, WorkoutZone.Cooldown))
            .ReturnsAsync(0);
            
        _mockExerciseTemplateRepo
            .Setup(x => x.UpdateAsync(It.IsAny<WorkoutTemplateExercise>()))
            .ReturnsAsync((WorkoutTemplateExercise ex) => ex);
            
        _mockExerciseTemplateRepo
            .Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<WorkoutTemplateExerciseId>()))
            .ReturnsAsync(_testTemplateExercise);
        
        // Act
        var result = await _service.ChangeExerciseZoneAsync(command);
        
        // Assert
        Assert.True(result.IsSuccess);
        _mockWritableUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }
    
    [Fact]
    public async Task ChangeExerciseZoneAsync_WithInvalidZone_ReturnsValidationError()
    {
        // Arrange
        var command = new ChangeExerciseZoneCommand
        {
            WorkoutTemplateExerciseId = _testExerciseId,
            NewZone = "InvalidZone",
            UserId = _testUserId
        };
        
        // Act
        var result = await _service.ChangeExerciseZoneAsync(command);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Contains("Invalid zone", result.Errors.First());
    }
    
    #endregion
    
    #region DuplicateExercisesAsync Tests
    
    [Fact]
    public async Task DuplicateExercisesAsync_WithValidCommand_DuplicatesExercises()
    {
        // Arrange
        var sourceTemplateId = WorkoutTemplateId.New();
        var targetTemplateId = WorkoutTemplateId.New();
        
        var sourceTemplate = CreateTemplateWithNavigation(sourceTemplateId, UserId.New(), _draftState);
            
        var targetTemplate = CreateTemplateWithNavigation(targetTemplateId, _testUserId, _draftState);
            
        var sourceExercises = new List<WorkoutTemplateExercise>
        {
            new WorkoutTemplateExerciseBuilder()
                .WithWorkoutTemplateId(sourceTemplateId)
                .WithZone(WorkoutZone.Main)
                .Build()
        };
        
        var command = new DuplicateTemplateExercisesCommand
        {
            SourceTemplateId = sourceTemplateId,
            TargetTemplateId = targetTemplateId,
            UserId = _testUserId,
            IncludeSetConfigurations = false
        };
        
        _mockTemplateRepo
            .Setup(x => x.GetByIdAsync(sourceTemplateId))
            .ReturnsAsync(sourceTemplate);
            
        _mockTemplateRepo
            .Setup(x => x.GetByIdAsync(targetTemplateId))
            .ReturnsAsync(targetTemplate);
            
        _mockExerciseTemplateRepo
            .Setup(x => x.GetByWorkoutTemplateAsync(sourceTemplateId))
            .ReturnsAsync(sourceExercises);
            
        _mockExerciseTemplateRepo
            .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<WorkoutTemplateExercise>>()))
            .ReturnsAsync((IEnumerable<WorkoutTemplateExercise> exercises) => exercises);
        
        // Act
        var result = await _service.DuplicateExercisesAsync(command);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Data);
        _mockWritableUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }
    
    [Fact]
    public async Task DuplicateExercisesAsync_WhenSourceTemplateNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var command = new DuplicateTemplateExercisesCommand
        {
            SourceTemplateId = WorkoutTemplateId.New(),
            TargetTemplateId = WorkoutTemplateId.New(),
            UserId = _testUserId
        };
        
        _mockTemplateRepo
            .Setup(x => x.GetByIdAsync(command.SourceTemplateId))
            .ReturnsAsync(WorkoutTemplate.Empty);
        
        // Act
        var result = await _service.DuplicateExercisesAsync(command);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Contains("not found", result.Errors.First());
        Assert.Contains("Source template not found", result.Errors.First());
    }
    
    [Fact]
    public async Task DuplicateExercisesAsync_WhenTargetNotOwned_ReturnsUnauthorizedError()
    {
        // Arrange
        var sourceTemplateId = WorkoutTemplateId.New();
        var targetTemplateId = WorkoutTemplateId.New();
        
        var sourceTemplate = CreateTemplateWithNavigation(sourceTemplateId, UserId.New(), _draftState);
            
        var targetTemplate = CreateTemplateWithNavigation(targetTemplateId, UserId.New(), _draftState);
        
        var command = new DuplicateTemplateExercisesCommand
        {
            SourceTemplateId = sourceTemplateId,
            TargetTemplateId = targetTemplateId,
            UserId = _testUserId
        };
        
        _mockTemplateRepo
            .Setup(x => x.GetByIdAsync(sourceTemplateId))
            .ReturnsAsync(sourceTemplate);
            
        _mockTemplateRepo
            .Setup(x => x.GetByIdAsync(targetTemplateId))
            .ReturnsAsync(targetTemplate);
        
        // Act
        var result = await _service.DuplicateExercisesAsync(command);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Contains("not authorized", result.Errors.First());
    }
    
    #endregion
    
    #region GetExerciseSuggestionsAsync Tests
    
    [Fact]
    public async Task GetExerciseSuggestionsAsync_WithValidRequest_ReturnsEmptyList()
    {
        // Act
        var result = await _service.GetExerciseSuggestionsAsync(_testTemplateId, "Main");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }
    
    [Fact]
    public async Task GetExerciseSuggestionsAsync_WithEmptyTemplateId_ReturnsValidationError()
    {
        // Act
        var result = await _service.GetExerciseSuggestionsAsync(WorkoutTemplateId.Empty, "Main");
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Single(result.Errors);
    }
    
    #endregion
    
    #region ValidateExercisesAsync Tests
    
    [Fact]
    public async Task ValidateExercisesAsync_WithValidExercises_ReturnsSuccess()
    {
        // Arrange
        var exerciseIds = new List<ExerciseId> { _testExerciseEntityId };
        
        _mockTemplateRepo
            .Setup(x => x.GetByIdAsync(_testTemplateId))
            .ReturnsAsync(_testTemplate);
            
        _mockExerciseRepo
            .Setup(x => x.GetByIdAsync(_testExerciseEntityId))
            .ReturnsAsync(_testExercise);
        
        // Act
        var result = await _service.ValidateExercisesAsync(_testTemplateId, exerciseIds);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
    }
    
    [Fact]
    public async Task ValidateExercisesAsync_WhenTemplateNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var exerciseIds = new List<ExerciseId> { _testExerciseEntityId };
        
        _mockTemplateRepo
            .Setup(x => x.GetByIdAsync(_testTemplateId))
            .ReturnsAsync(WorkoutTemplate.Empty);
        
        // Act
        var result = await _service.ValidateExercisesAsync(_testTemplateId, exerciseIds);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Contains("not found", result.Errors.First());
        Assert.Contains("Workout template not found", result.Errors.First());
    }
    
    [Fact]
    public async Task ValidateExercisesAsync_WhenExerciseNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var exerciseIds = new List<ExerciseId> { _testExerciseEntityId };
        
        _mockTemplateRepo
            .Setup(x => x.GetByIdAsync(_testTemplateId))
            .ReturnsAsync(_testTemplate);
            
        _mockExerciseRepo
            .Setup(x => x.GetByIdAsync(_testExerciseEntityId))
            .ReturnsAsync(Exercise.Empty);
        
        // Act
        var result = await _service.ValidateExercisesAsync(_testTemplateId, exerciseIds);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Contains("not found", result.Errors.First());
        Assert.Contains($"Exercise {_testExerciseEntityId} not found", result.Errors.First());
    }
    
    [Fact]
    public async Task ValidateExercisesAsync_WithEmptyExerciseList_ReturnsValidationError()
    {
        // Act
        var result = await _service.ValidateExercisesAsync(_testTemplateId, new List<ExerciseId>());
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Single(result.Errors);
    }
    
    #endregion
}