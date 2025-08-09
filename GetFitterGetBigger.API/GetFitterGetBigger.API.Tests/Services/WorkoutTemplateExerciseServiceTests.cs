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
            .Setup(x => x.CreateReadOnly())
            .Returns(_mockReadOnlyUnitOfWork.Object);
            
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
        
        _testTemplate = CreateTemplateWithNavigation(_testTemplateId, _draftState);
            
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
    
    private WorkoutTemplate CreateTemplateWithNavigation(WorkoutTemplateId id, WorkoutState workoutState)
    {
        var baseTemplate = new WorkoutTemplateBuilder()
            .WithId(id)
            .WithName("Test Template")
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
        
        _mockExerciseTemplateRepo.Setup(x => x.GetByWorkoutTemplateAsync(_testTemplateId))
            .ReturnsAsync(exercises);
        
        // Act
        var result = await _service.GetByWorkoutTemplateAsync(_testTemplateId);
        
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
        
        _mockExerciseTemplateRepo.Setup(x => x.GetByWorkoutTemplateAsync(_testTemplateId))
            .ReturnsAsync(exercises);
        
        // Act
        var result = await _service.GetByWorkoutTemplateAsync(_testTemplateId);
        
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
        _mockExerciseTemplateRepo.Setup(x => x.GetByIdWithDetailsAsync(_testExerciseId))
            .ReturnsAsync(_testTemplateExercise);
        
        // Act
        var result = await _service.GetByIdAsync(_testExerciseId);
        
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
        
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Contains("WorkoutTemplateExerciseId", result.Errors.First());
        Assert.Contains("GUID format", result.Errors.First());
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNotFoundError()
    {
        // Arrange
        _mockExerciseTemplateRepo.Setup(x => x.GetByIdWithDetailsAsync(_testExerciseId))
            .ReturnsAsync((WorkoutTemplateExercise?)null);
        
        // Act
        var result = await _service.GetByIdAsync(_testExerciseId);
        
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Contains("not found", result.Errors.First());
    }
    
    #endregion
    
    #region GetExerciseSuggestionsAsync Tests
    
    [Fact]
    public async Task GetExerciseSuggestionsAsync_WithValidRequest_ReturnsEmptyList()
    {
        // Act
        var result = await _service.GetExerciseSuggestionsAsync(_testTemplateId, "Main");
        
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }
    
    [Fact]
    public async Task GetExerciseSuggestionsAsync_WithEmptyTemplateId_ReturnsValidationError()
    {
        // Act
        var result = await _service.GetExerciseSuggestionsAsync(WorkoutTemplateId.Empty, "Main");
        
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
            .Setup(x => x.GetByIdWithDetailsAsync(_testTemplateId))
            .ReturnsAsync(_testTemplate);
            
        _mockExerciseRepo
            .Setup(x => x.GetByIdAsync(_testExerciseEntityId))
            .ReturnsAsync(ExerciseBuilder.AWorkoutExercise().Build());
        
        // Act
        var result = await _service.ValidateExercisesAsync(_testTemplateId, exerciseIds);
        
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
    }
    
    [Fact]
    public async Task ValidateExercisesAsync_WhenTemplateNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var exerciseIds = new List<ExerciseId> { _testExerciseEntityId };
        
        _mockTemplateRepo
            .Setup(x => x.GetByIdWithDetailsAsync(_testTemplateId))
            .ReturnsAsync((WorkoutTemplate?)null);
        
        // Act
        var result = await _service.ValidateExercisesAsync(_testTemplateId, exerciseIds);
        
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
            .Setup(x => x.GetByIdWithDetailsAsync(_testTemplateId))
            .ReturnsAsync(_testTemplate);
            
        _mockExerciseRepo
            .Setup(x => x.GetByIdAsync(_testExerciseEntityId))
            .ReturnsAsync(Exercise.Empty);
        
        // Act
        var result = await _service.ValidateExercisesAsync(_testTemplateId, exerciseIds);
        
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
        
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Single(result.Errors);
    }
    
    #endregion
}