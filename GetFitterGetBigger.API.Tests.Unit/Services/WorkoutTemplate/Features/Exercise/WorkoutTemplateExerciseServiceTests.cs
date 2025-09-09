using GetFitterGetBigger.API.Data;
using GetFitterGetBigger.API.Data.Providers;
using GetFitterGetBigger.API.Data.Repositories.Exercise;
using GetFitterGetBigger.API.Data.Repositories.WorkoutTemplate;
using GetFitterGetBigger.API.Entities.Exercise;
using GetFitterGetBigger.API.Entities.WorkoutTemplate;
using GetFitterGetBigger.API.Services.Exercise.Features.Links;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.Dto;
using GetFitterGetBigger.API.Services.Shared;
using GetFitterGetBigger.API.Services.Shared.Dto;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise.Dto;
using GetFitterGetBigger.API.Tests.Unit.Builders.Entities;
using GetFitterGetBigger.API.Tests.Unit.Builders.Services;
using GetFitterGetBigger.API.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Unit.Services.WorkoutTemplate.Features.Exercise;

public class WorkoutTemplateExerciseServiceTests
{
    private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _unitOfWorkProviderMock;
    private readonly Mock<IExerciseLinkQueryDataService> _exerciseLinkDataServiceMock;
    private readonly Mock<ILogger<WorkoutTemplateExerciseService>> _loggerMock;
    private readonly Mock<IUnitOfWork<FitnessDbContext>> _readOnlyUnitOfWorkMock;
    private readonly Mock<IUnitOfWork<FitnessDbContext>> _writableUnitOfWorkMock;
    private readonly Mock<IWorkoutTemplateExerciseRepository> _repositoryMock;
    private readonly Mock<IWorkoutTemplateRepository> _templateRepositoryMock;
    private readonly Mock<IExerciseRepository> _exerciseRepositoryMock;
    private readonly WorkoutTemplateExerciseService _service;

    public WorkoutTemplateExerciseServiceTests()
    {
        _unitOfWorkProviderMock = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
        _exerciseLinkDataServiceMock = new Mock<IExerciseLinkQueryDataService>();
        _loggerMock = new Mock<ILogger<WorkoutTemplateExerciseService>>();
        _readOnlyUnitOfWorkMock = new Mock<IUnitOfWork<FitnessDbContext>>();
        _writableUnitOfWorkMock = new Mock<IUnitOfWork<FitnessDbContext>>();
        _repositoryMock = new Mock<IWorkoutTemplateExerciseRepository>();
        _templateRepositoryMock = new Mock<IWorkoutTemplateRepository>();
        _exerciseRepositoryMock = new Mock<IExerciseRepository>();

        _service = new WorkoutTemplateExerciseService(
            _unitOfWorkProviderMock.Object,
            _exerciseLinkDataServiceMock.Object,
            _loggerMock.Object);
    }

    #region AddExerciseAsync Tests

    [Fact]
    public async Task AddExerciseAsync_WithValidWorkoutExercise_AddsMainAndAutoLinkedExercises()
    {
        // Arrange
        var templateId = new WorkoutTemplateId(Guid.NewGuid());
        var exerciseId = new ExerciseId(Guid.NewGuid());
        var warmupExerciseId = new ExerciseId(Guid.NewGuid());
        var cooldownExerciseId = new ExerciseId(Guid.NewGuid());
        
        var dto = new AddExerciseDto(exerciseId, "Workout", 1, "{}");
        
        var template = new WorkoutTemplateBuilder()
            .WithId(templateId)
            .WithWorkoutState("Draft")
            .Build();
        
        var exercise = new ExerciseBuilder()
            .WithId(exerciseId)
            .WithIsActive(true)
            .Build();

        // Setup readonly unit of work for validation
        _unitOfWorkProviderMock
            .Setup(p => p.CreateReadOnly())
            .Returns(_readOnlyUnitOfWorkMock.Object);
        
        _readOnlyUnitOfWorkMock
            .Setup(u => u.GetRepository<IWorkoutTemplateRepository>())
            .Returns(_templateRepositoryMock.Object);
        
        _readOnlyUnitOfWorkMock
            .Setup(u => u.GetRepository<IExerciseRepository>())
            .Returns(_exerciseRepositoryMock.Object);
        
        _templateRepositoryMock
            .Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(template);
        
        _exerciseRepositoryMock
            .Setup(r => r.GetByIdAsync(exerciseId))
            .ReturnsAsync(exercise);

        // Setup writable unit of work for adding
        _unitOfWorkProviderMock
            .Setup(p => p.CreateWritable())
            .Returns(_writableUnitOfWorkMock.Object);
        
        _writableUnitOfWorkMock
            .Setup(u => u.GetRepository<IWorkoutTemplateExerciseRepository>())
            .Returns(_repositoryMock.Object);
        
        _repositoryMock
            .Setup(r => r.GetMaxOrderInRoundAsync(templateId, "Workout", 1))
            .ReturnsAsync(0);

        // Setup auto-linking
        var warmupLink = new ExerciseLinkBuilder()
            .WithSourceExerciseId(exerciseId)
            .WithTargetExerciseId(warmupExerciseId)
            .WithLinkType(ExerciseLinkType.WARMUP)
            .BuildDto();
        
        var cooldownLink = new ExerciseLinkBuilder()
            .WithSourceExerciseId(exerciseId)
            .WithTargetExerciseId(cooldownExerciseId)
            .WithLinkType(ExerciseLinkType.COOLDOWN)
            .BuildDto();
        
        _exerciseLinkDataServiceMock
            .Setup(s => s.GetBySourceExerciseAsync(exerciseId, "WARMUP"))
            .ReturnsAsync(ServiceResult<List<ExerciseLinkDto>>.Success(new List<ExerciseLinkDto> { warmupLink }));
        
        _exerciseLinkDataServiceMock
            .Setup(s => s.GetBySourceExerciseAsync(exerciseId, "COOLDOWN"))
            .ReturnsAsync(ServiceResult<List<ExerciseLinkDto>>.Success(new List<ExerciseLinkDto> { cooldownLink }));
        
        _repositoryMock
            .Setup(r => r.ExistsInPhaseAsync(templateId, "Warmup", warmupExerciseId))
            .ReturnsAsync(false);
        
        _repositoryMock
            .Setup(r => r.ExistsInPhaseAsync(templateId, "Cooldown", cooldownExerciseId))
            .ReturnsAsync(false);
        
        _repositoryMock
            .Setup(r => r.GetMaxRoundNumberAsync(templateId, "Warmup"))
            .ReturnsAsync(0);
        
        _repositoryMock
            .Setup(r => r.GetMaxRoundNumberAsync(templateId, "Cooldown"))
            .ReturnsAsync(0);
        
        _repositoryMock
            .Setup(r => r.GetMaxOrderInRoundAsync(templateId, "Warmup", 1))
            .ReturnsAsync(0);
        
        _repositoryMock
            .Setup(r => r.GetMaxOrderInRoundAsync(templateId, "Cooldown", 1))
            .ReturnsAsync(0);

        // Act
        var result = await _service.AddExerciseAsync(templateId, dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Data.AddedExercises.Count);
        Assert.Contains("Successfully added 3 exercise(s)", result.Data.Message);
        
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<WorkoutTemplateExercise>()), Times.Exactly(3));
        _writableUnitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task AddExerciseAsync_WithInvalidTemplateId_ReturnsFailure()
    {
        // Arrange
        var templateId = WorkoutTemplateId.Empty;
        var dto = new AddExerciseDto(new ExerciseId(Guid.NewGuid()), "Workout", 1, "{}");

        // Act
        var result = await _service.AddExerciseAsync(templateId, dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(WorkoutTemplateExerciseErrorMessages.WorkoutTemplateNotFound, result.Error.Message);
    }

    [Fact]
    public async Task AddExerciseAsync_WithInvalidPhase_ReturnsFailure()
    {
        // Arrange
        var templateId = new WorkoutTemplateId(Guid.NewGuid());
        var dto = new AddExerciseDto(new ExerciseId(Guid.NewGuid()), "InvalidPhase", 1, "{}");

        // Act
        var result = await _service.AddExerciseAsync(templateId, dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(WorkoutTemplateExerciseErrorMessages.InvalidZoneWarmupMainCooldown, result.Error.Message);
    }

    [Fact]
    public async Task AddExerciseAsync_WithTemplateNotInDraftState_ReturnsFailure()
    {
        // Arrange
        var templateId = new WorkoutTemplateId(Guid.NewGuid());
        var dto = new AddExerciseDto(new ExerciseId(Guid.NewGuid()), "Workout", 1, "{}");
        
        var template = new WorkoutTemplateBuilder()
            .WithId(templateId)
            .WithWorkoutState("Published")
            .Build();

        _unitOfWorkProviderMock
            .Setup(p => p.CreateReadOnly())
            .Returns(_readOnlyUnitOfWorkMock.Object);
        
        _readOnlyUnitOfWorkMock
            .Setup(u => u.GetRepository<IWorkoutTemplateRepository>())
            .Returns(_templateRepositoryMock.Object);
        
        _templateRepositoryMock
            .Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(template);

        // Act
        var result = await _service.AddExerciseAsync(templateId, dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(WorkoutTemplateExerciseErrorMessages.CanOnlyAddExercisesToDraftTemplates, result.Error.Message);
    }

    #endregion

    #region RemoveExerciseAsync Tests

    [Fact]
    public async Task RemoveExerciseAsync_WithWorkoutExercise_RemovesMainAndOrphanedExercises()
    {
        // Arrange
        var templateId = new WorkoutTemplateId(Guid.NewGuid());
        var exerciseId = new WorkoutTemplateExerciseId(Guid.NewGuid());
        var workoutExerciseId = new ExerciseId(Guid.NewGuid());
        var warmupExerciseId = new ExerciseId(Guid.NewGuid());
        var cooldownExerciseId = new ExerciseId(Guid.NewGuid());

        var template = new WorkoutTemplateBuilder()
            .WithId(templateId)
            .WithWorkoutState("Draft")
            .Build();

        var workoutExercise = new WorkoutTemplateExerciseBuilder()
            .WithId(exerciseId)
            .WithWorkoutTemplateId(templateId)
            .WithExerciseId(workoutExerciseId)
            .WithPhase("Workout")
            .WithRoundNumber(1)
            .WithOrderInRound(1)
            .Build();

        var warmupExercise = new WorkoutTemplateExerciseBuilder()
            .WithExerciseId(warmupExerciseId)
            .WithPhase("Warmup")
            .Build();

        var cooldownExercise = new WorkoutTemplateExerciseBuilder()
            .WithExerciseId(cooldownExerciseId)
            .WithPhase("Cooldown")
            .Build();

        // Setup readonly unit of work
        _unitOfWorkProviderMock
            .Setup(p => p.CreateReadOnly())
            .Returns(_readOnlyUnitOfWorkMock.Object);
        
        _readOnlyUnitOfWorkMock
            .Setup(u => u.GetRepository<IWorkoutTemplateRepository>())
            .Returns(_templateRepositoryMock.Object);
        
        _readOnlyUnitOfWorkMock
            .Setup(u => u.GetRepository<IWorkoutTemplateExerciseRepository>())
            .Returns(_repositoryMock.Object);
        
        _templateRepositoryMock
            .Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(template);
        
        _repositoryMock
            .Setup(r => r.GetByIdAsync(exerciseId))
            .ReturnsAsync(workoutExercise);

        // Setup writable unit of work
        _unitOfWorkProviderMock
            .Setup(p => p.CreateWritable())
            .Returns(_writableUnitOfWorkMock.Object);
        
        _writableUnitOfWorkMock
            .Setup(u => u.GetRepository<IWorkoutTemplateExerciseRepository>())
            .Returns(_repositoryMock.Object);

        // Setup orphan detection
        var warmupLink = new ExerciseLinkBuilder()
            .WithSourceExerciseId(workoutExerciseId)
            .WithTargetExerciseId(warmupExerciseId)
            .WithLinkType(ExerciseLinkType.WARMUP)
            .BuildDto();
        
        var cooldownLink = new ExerciseLinkBuilder()
            .WithSourceExerciseId(workoutExerciseId)
            .WithTargetExerciseId(cooldownExerciseId)
            .WithLinkType(ExerciseLinkType.COOLDOWN)
            .BuildDto();
        
        _exerciseLinkDataServiceMock
            .Setup(s => s.GetBySourceExerciseAsync(workoutExerciseId, "WARMUP"))
            .ReturnsAsync(ServiceResult<List<ExerciseLinkDto>>.Success(new List<ExerciseLinkDto> { warmupLink }));
        
        _exerciseLinkDataServiceMock
            .Setup(s => s.GetBySourceExerciseAsync(workoutExerciseId, "COOLDOWN"))
            .ReturnsAsync(ServiceResult<List<ExerciseLinkDto>>.Success(new List<ExerciseLinkDto> { cooldownLink }));
        
        _repositoryMock
            .Setup(r => r.GetByTemplateAndPhaseAsync(templateId, "Workout"))
            .ReturnsAsync(new List<WorkoutTemplateExercise> { workoutExercise });
        
        _repositoryMock
            .Setup(r => r.GetByTemplateAndPhaseAsync(templateId, "Warmup"))
            .ReturnsAsync(new List<WorkoutTemplateExercise> { warmupExercise });
        
        _repositoryMock
            .Setup(r => r.GetByTemplateAndPhaseAsync(templateId, "Cooldown"))
            .ReturnsAsync(new List<WorkoutTemplateExercise> { cooldownExercise });

        // Act
        var result = await _service.RemoveExerciseAsync(templateId, exerciseId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Data.RemovedExercises.Count);
        Assert.Contains("Successfully removed 3 exercise(s)", result.Data.Message);
        
        _repositoryMock.Verify(r => r.DeleteRangeAsync(It.IsAny<List<WorkoutTemplateExerciseId>>()), Times.Once);
        _writableUnitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveExerciseAsync_WithExerciseNotInTemplate_ReturnsFailure()
    {
        // Arrange
        var templateId = new WorkoutTemplateId(Guid.NewGuid());
        var exerciseId = new WorkoutTemplateExerciseId(Guid.NewGuid());
        var otherTemplateId = new WorkoutTemplateId(Guid.NewGuid());

        var template = new WorkoutTemplateBuilder()
            .WithId(templateId)
            .WithWorkoutState("Draft")
            .Build();

        var exercise = new WorkoutTemplateExerciseBuilder()
            .WithId(exerciseId)
            .WithWorkoutTemplateId(otherTemplateId)
            .Build();

        _unitOfWorkProviderMock
            .Setup(p => p.CreateReadOnly())
            .Returns(_readOnlyUnitOfWorkMock.Object);
        
        _readOnlyUnitOfWorkMock
            .Setup(u => u.GetRepository<IWorkoutTemplateRepository>())
            .Returns(_templateRepositoryMock.Object);
        
        _readOnlyUnitOfWorkMock
            .Setup(u => u.GetRepository<IWorkoutTemplateExerciseRepository>())
            .Returns(_repositoryMock.Object);
        
        _templateRepositoryMock
            .Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(template);
        
        _repositoryMock
            .Setup(r => r.GetByIdAsync(exerciseId))
            .ReturnsAsync(exercise);

        // Act
        var result = await _service.RemoveExerciseAsync(templateId, exerciseId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(WorkoutTemplateExerciseErrorMessages.TemplateExerciseNotFound, result.Error.Message);
    }

    #endregion

    #region UpdateExerciseMetadataAsync Tests

    [Fact]
    public async Task UpdateExerciseMetadataAsync_WithValidData_UpdatesMetadata()
    {
        // Arrange
        var templateId = new WorkoutTemplateId(Guid.NewGuid());
        var exerciseId = new WorkoutTemplateExerciseId(Guid.NewGuid());
        var newMetadata = "{\"sets\": 3, \"reps\": 10}";

        var template = new WorkoutTemplateBuilder()
            .WithId(templateId)
            .WithWorkoutState("Draft")
            .Build();

        var exercise = new WorkoutTemplateExerciseBuilder()
            .WithId(exerciseId)
            .WithWorkoutTemplateId(templateId)
            .Build();

        _unitOfWorkProviderMock
            .Setup(p => p.CreateReadOnly())
            .Returns(_readOnlyUnitOfWorkMock.Object);
        
        _readOnlyUnitOfWorkMock
            .Setup(u => u.GetRepository<IWorkoutTemplateRepository>())
            .Returns(_templateRepositoryMock.Object);
        
        _readOnlyUnitOfWorkMock
            .Setup(u => u.GetRepository<IWorkoutTemplateExerciseRepository>())
            .Returns(_repositoryMock.Object);
        
        _templateRepositoryMock
            .Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(template);
        
        _repositoryMock
            .Setup(r => r.GetByIdAsync(exerciseId))
            .ReturnsAsync(exercise);

        _unitOfWorkProviderMock
            .Setup(p => p.CreateWritable())
            .Returns(_writableUnitOfWorkMock.Object);
        
        _writableUnitOfWorkMock
            .Setup(u => u.GetRepository<IWorkoutTemplateExerciseRepository>())
            .Returns(_repositoryMock.Object);

        // Act
        var result = await _service.UpdateExerciseMetadataAsync(templateId, exerciseId, newMetadata);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Metadata updated successfully", result.Data.Message);
        
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<WorkoutTemplateExercise>()), Times.Once);
        _writableUnitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    #endregion

    #region ReorderExerciseAsync Tests

    [Fact]
    public async Task ReorderExerciseAsync_MovingUp_ReordersCorrectly()
    {
        // Arrange
        var templateId = new WorkoutTemplateId(Guid.NewGuid());
        var exerciseId = new WorkoutTemplateExerciseId(Guid.NewGuid());
        var newOrder = 1;

        var template = new WorkoutTemplateBuilder()
            .WithId(templateId)
            .WithWorkoutState("Draft")
            .Build();

        var exercise = new WorkoutTemplateExerciseBuilder()
            .WithId(exerciseId)
            .WithWorkoutTemplateId(templateId)
            .WithOrderInRound(3)
            .Build();

        var otherExercise1 = new WorkoutTemplateExerciseBuilder()
            .WithOrderInRound(1)
            .Build();

        var otherExercise2 = new WorkoutTemplateExerciseBuilder()
            .WithOrderInRound(2)
            .Build();

        _unitOfWorkProviderMock
            .Setup(p => p.CreateReadOnly())
            .Returns(_readOnlyUnitOfWorkMock.Object);
        
        _readOnlyUnitOfWorkMock
            .Setup(u => u.GetRepository<IWorkoutTemplateRepository>())
            .Returns(_templateRepositoryMock.Object);
        
        _readOnlyUnitOfWorkMock
            .Setup(u => u.GetRepository<IWorkoutTemplateExerciseRepository>())
            .Returns(_repositoryMock.Object);
        
        _templateRepositoryMock
            .Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(template);
        
        _repositoryMock
            .Setup(r => r.GetByIdAsync(exerciseId))
            .ReturnsAsync(exercise);

        _unitOfWorkProviderMock
            .Setup(p => p.CreateWritable())
            .Returns(_writableUnitOfWorkMock.Object);
        
        _writableUnitOfWorkMock
            .Setup(u => u.GetRepository<IWorkoutTemplateExerciseRepository>())
            .Returns(_repositoryMock.Object);
        
        _repositoryMock
            .Setup(r => r.GetByRoundAsync(templateId, exercise.Phase, exercise.RoundNumber))
            .ReturnsAsync(new List<WorkoutTemplateExercise> { otherExercise1, otherExercise2, exercise });

        // Act
        var result = await _service.ReorderExerciseAsync(templateId, exerciseId, newOrder);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains($"Exercise reordered to position {newOrder}", result.Data.Message);
        
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<WorkoutTemplateExercise>()), Times.AtLeast(2));
        _writableUnitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    #endregion

    #region CopyRoundAsync Tests

    [Fact]
    public async Task CopyRoundAsync_WithExistingRound_CopiesAllExercises()
    {
        // Arrange
        var templateId = new WorkoutTemplateId(Guid.NewGuid());
        var dto = new CopyRoundDto("Workout", 1, "Workout", 2);

        var template = new WorkoutTemplateBuilder()
            .WithId(templateId)
            .WithWorkoutState("Draft")
            .Build();

        var exercise1 = new WorkoutTemplateExerciseBuilder()
            .WithPhase("Workout")
            .WithRoundNumber(1)
            .WithOrderInRound(1)
            .Build();

        var exercise2 = new WorkoutTemplateExerciseBuilder()
            .WithPhase("Workout")
            .WithRoundNumber(1)
            .WithOrderInRound(2)
            .Build();

        _unitOfWorkProviderMock
            .Setup(p => p.CreateReadOnly())
            .Returns(_readOnlyUnitOfWorkMock.Object);
        
        _readOnlyUnitOfWorkMock
            .Setup(u => u.GetRepository<IWorkoutTemplateRepository>())
            .Returns(_templateRepositoryMock.Object);
        
        _readOnlyUnitOfWorkMock
            .Setup(u => u.GetRepository<IWorkoutTemplateExerciseRepository>())
            .Returns(_repositoryMock.Object);
        
        _templateRepositoryMock
            .Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(template);
        
        _repositoryMock
            .Setup(r => r.GetByTemplateAndPhaseAsync(templateId, dto.SourcePhase))
            .ReturnsAsync(new List<WorkoutTemplateExercise> { exercise1, exercise2 });

        _unitOfWorkProviderMock
            .Setup(p => p.CreateWritable())
            .Returns(_writableUnitOfWorkMock.Object);
        
        _writableUnitOfWorkMock
            .Setup(u => u.GetRepository<IWorkoutTemplateExerciseRepository>())
            .Returns(_repositoryMock.Object);
        
        _repositoryMock
            .Setup(r => r.GetByRoundAsync(templateId, dto.SourcePhase, dto.SourceRoundNumber))
            .ReturnsAsync(new List<WorkoutTemplateExercise> { exercise1, exercise2 });
        
        _repositoryMock
            .Setup(r => r.GetMaxOrderInRoundAsync(templateId, dto.TargetPhase, dto.TargetRoundNumber))
            .ReturnsAsync(0);

        // Act
        var result = await _service.CopyRoundAsync(templateId, dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Data.CopiedExercises.Count);
        Assert.Contains("Successfully copied 2 exercise(s)", result.Data.Message);
        
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<WorkoutTemplateExercise>()), Times.Exactly(2));
        _writableUnitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    #endregion

    #region GetTemplateExercisesAsync Tests

    [Fact]
    public async Task GetTemplateExercisesAsync_WithExercises_ReturnsOrganizedByPhaseAndRound()
    {
        // Arrange
        var templateId = new WorkoutTemplateId(Guid.NewGuid());
        
        var template = new WorkoutTemplateBuilder()
            .WithId(templateId)
            .WithName("Test Template")
            .Build();

        var warmupExercise = new WorkoutTemplateExerciseBuilder()
            .WithPhase("Warmup")
            .WithRoundNumber(1)
            .WithOrderInRound(1)
            .Build();

        var workoutExercise1 = new WorkoutTemplateExerciseBuilder()
            .WithPhase("Workout")
            .WithRoundNumber(1)
            .WithOrderInRound(1)
            .Build();

        var workoutExercise2 = new WorkoutTemplateExerciseBuilder()
            .WithPhase("Workout")
            .WithRoundNumber(2)
            .WithOrderInRound(1)
            .Build();

        var cooldownExercise = new WorkoutTemplateExerciseBuilder()
            .WithPhase("Cooldown")
            .WithRoundNumber(1)
            .WithOrderInRound(1)
            .Build();

        _unitOfWorkProviderMock
            .Setup(p => p.CreateReadOnly())
            .Returns(_readOnlyUnitOfWorkMock.Object);
        
        _readOnlyUnitOfWorkMock
            .Setup(u => u.GetRepository<IWorkoutTemplateRepository>())
            .Returns(_templateRepositoryMock.Object);
        
        _readOnlyUnitOfWorkMock
            .Setup(u => u.GetRepository<IWorkoutTemplateExerciseRepository>())
            .Returns(_repositoryMock.Object);
        
        _templateRepositoryMock
            .Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(template);
        
        _repositoryMock
            .Setup(r => r.GetByWorkoutTemplateIdAsync(templateId))
            .ReturnsAsync(new List<WorkoutTemplateExercise> 
            { 
                warmupExercise, 
                workoutExercise1, 
                workoutExercise2, 
                cooldownExercise 
            });

        // Act
        var result = await _service.GetTemplateExercisesAsync(templateId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Test Template", result.Data.TemplateName);
        Assert.Single(result.Data.Warmup.Rounds);
        Assert.Equal(2, result.Data.Workout.Rounds.Count);
        Assert.Single(result.Data.Cooldown.Rounds);
    }

    #endregion

    #region ValidateExerciseMetadataAsync Tests

    [Fact]
    public async Task ValidateExerciseMetadataAsync_WithValidJson_ReturnsTrue()
    {
        // Arrange
        var exerciseId = new ExerciseId(Guid.NewGuid());
        var protocolId = new ExecutionProtocolId(Guid.NewGuid());
        var metadata = "{\"sets\": 3, \"reps\": 10}";

        // Act
        var result = await _service.ValidateExerciseMetadataAsync(exerciseId, protocolId, metadata);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Data.Value);
    }

    [Fact]
    public async Task ValidateExerciseMetadataAsync_WithInvalidJson_ReturnsFalse()
    {
        // Arrange
        var exerciseId = new ExerciseId(Guid.NewGuid());
        var protocolId = new ExecutionProtocolId(Guid.NewGuid());
        var metadata = "invalid json";

        // Act
        var result = await _service.ValidateExerciseMetadataAsync(exerciseId, protocolId, metadata);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Data.Value);
    }

    #endregion

    #region Backward Compatibility Tests

    [Fact]
    public async Task AddWorkoutTemplateExerciseAsync_BackwardCompatibility_Works()
    {
        // Arrange
        var templateId = new WorkoutTemplateId(Guid.NewGuid());
        var exerciseId = new ExerciseId(Guid.NewGuid());
        
        var createDto = new CreateWorkoutTemplateExerciseDto
        {
            WorkoutTemplateId = templateId,
            ExerciseId = exerciseId,
            Zone = "Workout"
        };

        var template = new WorkoutTemplateBuilder()
            .WithId(templateId)
            .WithWorkoutState("Draft")
            .Build();
        
        var exercise = new ExerciseBuilder()
            .WithId(exerciseId)
            .WithIsActive(true)
            .Build();

        _unitOfWorkProviderMock
            .Setup(p => p.CreateReadOnly())
            .Returns(_readOnlyUnitOfWorkMock.Object);
        
        _readOnlyUnitOfWorkMock
            .Setup(u => u.GetRepository<IWorkoutTemplateRepository>())
            .Returns(_templateRepositoryMock.Object);
        
        _readOnlyUnitOfWorkMock
            .Setup(u => u.GetRepository<IExerciseRepository>())
            .Returns(_exerciseRepositoryMock.Object);
        
        _templateRepositoryMock
            .Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(template);
        
        _exerciseRepositoryMock
            .Setup(r => r.GetByIdAsync(exerciseId))
            .ReturnsAsync(exercise);

        _unitOfWorkProviderMock
            .Setup(p => p.CreateWritable())
            .Returns(_writableUnitOfWorkMock.Object);
        
        _writableUnitOfWorkMock
            .Setup(u => u.GetRepository<IWorkoutTemplateExerciseRepository>())
            .Returns(_repositoryMock.Object);
        
        _repositoryMock
            .Setup(r => r.GetMaxOrderInRoundAsync(templateId, "Workout", 1))
            .ReturnsAsync(0);
        
        _exerciseLinkDataServiceMock
            .Setup(s => s.GetBySourceExerciseAsync(exerciseId, It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<List<ExerciseLinkDto>>.Success(new List<ExerciseLinkDto>()));

        // Act
        var result = await _service.AddWorkoutTemplateExerciseAsync(createDto);

        // Assert
        Assert.True(result.IsSuccess);
        
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<WorkoutTemplateExercise>()), Times.Once);
        _writableUnitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    #endregion
}