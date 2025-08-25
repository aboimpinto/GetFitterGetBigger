using System.Linq;
using FluentAssertions;
using GetFitterGetBigger.API.Constants.ErrorMessages;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplateExercises;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise;
using GetFitterGetBigger.API.Tests.TestBuilders.Domain;
using GetFitterGetBigger.API.Tests.TestBuilders.DTOs;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using Olimpo.EntityFramework.Persistency;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services.WorkoutTemplate.Features.Exercise;

public class WorkoutTemplateExerciseServiceTests
{
    private readonly AutoMocker _autoMocker;
    private readonly WorkoutTemplateExerciseService _testee;
    private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _unitOfWorkProviderMock;
    private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _readOnlyUnitOfWorkMock;
    private readonly Mock<IWritableUnitOfWork<FitnessDbContext>> _writableUnitOfWorkMock;
    private readonly Mock<IWorkoutTemplateExerciseRepository> _workoutTemplateExerciseRepositoryMock;
    private readonly Mock<IWorkoutTemplateRepository> _workoutTemplateRepositoryMock;
    private readonly Mock<IExerciseRepository> _exerciseRepositoryMock;
    private readonly Mock<ISetConfigurationRepository> _setConfigurationRepositoryMock;

    public WorkoutTemplateExerciseServiceTests()
    {
        _autoMocker = new AutoMocker();
        _unitOfWorkProviderMock = _autoMocker.GetMock<IUnitOfWorkProvider<FitnessDbContext>>();
        _readOnlyUnitOfWorkMock = _autoMocker.GetMock<IReadOnlyUnitOfWork<FitnessDbContext>>();
        _writableUnitOfWorkMock = _autoMocker.GetMock<IWritableUnitOfWork<FitnessDbContext>>();
        _workoutTemplateExerciseRepositoryMock = _autoMocker.GetMock<IWorkoutTemplateExerciseRepository>();
        _workoutTemplateRepositoryMock = _autoMocker.GetMock<IWorkoutTemplateRepository>();
        _exerciseRepositoryMock = _autoMocker.GetMock<IExerciseRepository>();
        _setConfigurationRepositoryMock = _autoMocker.GetMock<ISetConfigurationRepository>();
        
        SetupUnitOfWork();
        _testee = _autoMocker.CreateInstance<WorkoutTemplateExerciseService>();
    }

    private void SetupUnitOfWork()
    {
        _unitOfWorkProviderMock
            .Setup(x => x.CreateReadOnly())
            .Returns(_readOnlyUnitOfWorkMock.Object);
            
        _unitOfWorkProviderMock
            .Setup(x => x.CreateWritable())
            .Returns(_writableUnitOfWorkMock.Object);

        _readOnlyUnitOfWorkMock
            .Setup(x => x.GetRepository<IWorkoutTemplateExerciseRepository>())
            .Returns(_workoutTemplateExerciseRepositoryMock.Object);
            
        _readOnlyUnitOfWorkMock
            .Setup(x => x.GetRepository<IWorkoutTemplateRepository>())
            .Returns(_workoutTemplateRepositoryMock.Object);
            
        _readOnlyUnitOfWorkMock
            .Setup(x => x.GetRepository<IExerciseRepository>())
            .Returns(_exerciseRepositoryMock.Object);

        _writableUnitOfWorkMock
            .Setup(x => x.GetRepository<IWorkoutTemplateExerciseRepository>())
            .Returns(_workoutTemplateExerciseRepositoryMock.Object);
            
        _writableUnitOfWorkMock
            .Setup(x => x.GetRepository<IWorkoutTemplateRepository>())
            .Returns(_workoutTemplateRepositoryMock.Object);
            
        _writableUnitOfWorkMock
            .Setup(x => x.GetRepository<IExerciseRepository>())
            .Returns(_exerciseRepositoryMock.Object);
            
        _writableUnitOfWorkMock
            .Setup(x => x.GetRepository<ISetConfigurationRepository>())
            .Returns(_setConfigurationRepositoryMock.Object);
    }

    #region GetByWorkoutTemplateAsync Tests

    [Fact]
    public async Task GetByWorkoutTemplateAsync_WhenWorkoutTemplateIdIsEmpty_ShouldReturnFailure()
    {
        // Arrange
        var emptyId = WorkoutTemplateId.Empty;

        // Act
        var result = await _testee.GetByWorkoutTemplateAsync(emptyId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.InvalidFormat);
        result.Errors.First().Should().Contain("WorkoutTemplateId");
    }

    [Fact]
    public async Task GetByWorkoutTemplateAsync_WhenValidId_ShouldReturnExercisesGroupedByZone()
    {
        // Arrange
        var workoutTemplateId = WorkoutTemplateId.From(Guid.NewGuid());
        var warmupExercise = WorkoutTemplateExerciseBuilder.AWarmupExercise()
            .WithZone(WorkoutZone.Warmup)
            .WithSequenceOrder(1)
            .Build();
        var mainExercise1 = WorkoutTemplateExerciseBuilder.AMainExercise()
            .WithZone(WorkoutZone.Main)
            .WithSequenceOrder(1)
            .Build();
        var mainExercise2 = WorkoutTemplateExerciseBuilder.AMainExercise()
            .WithZone(WorkoutZone.Main)
            .WithSequenceOrder(2)
            .Build();
        var cooldownExercise = WorkoutTemplateExerciseBuilder.ACooldownExercise()
            .WithZone(WorkoutZone.Cooldown)
            .WithSequenceOrder(1)
            .Build();

        var exercises = new List<WorkoutTemplateExercise> 
        { 
            warmupExercise, 
            mainExercise2, // Out of order to test sorting
            mainExercise1, 
            cooldownExercise 
        };

        _workoutTemplateExerciseRepositoryMock
            .Setup(x => x.GetByWorkoutTemplateAsync(workoutTemplateId))
            .ReturnsAsync(exercises);

        // Act
        var result = await _testee.GetByWorkoutTemplateAsync(workoutTemplateId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.WorkoutTemplateId.Should().Be(workoutTemplateId.ToString());
        
        result.Data.WarmupExercises.Should().HaveCount(1);
        result.Data.WarmupExercises[0].Zone.Should().Be("Warmup");
        
        result.Data.MainExercises.Should().HaveCount(2);
        result.Data.MainExercises[0].SequenceOrder.Should().Be(1);
        result.Data.MainExercises[1].SequenceOrder.Should().Be(2);
        
        result.Data.CooldownExercises.Should().HaveCount(1);
        result.Data.CooldownExercises[0].Zone.Should().Be("Cooldown");
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenExerciseIdIsEmpty_ShouldReturnFailure()
    {
        // Arrange
        var emptyId = WorkoutTemplateExerciseId.Empty;

        // Act
        var result = await _testee.GetByIdAsync(emptyId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.InvalidFormat);
        result.Errors.First().Should().Contain("WorkoutTemplateExerciseId");
    }

    [Fact]
    public async Task GetByIdAsync_WhenExerciseNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var exerciseId = WorkoutTemplateExerciseId.From(Guid.NewGuid());
        
        _workoutTemplateExerciseRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(exerciseId))
            .ReturnsAsync(WorkoutTemplateExercise.Empty);

        // Act
        var result = await _testee.GetByIdAsync(exerciseId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.NotFound);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.ExerciseNotFound + " not found");
    }

    [Fact]
    public async Task GetByIdAsync_WhenExerciseExists_ShouldReturnExercise()
    {
        // Arrange
        var exerciseId = WorkoutTemplateExerciseId.From(Guid.NewGuid());
        var exercise = WorkoutTemplateExerciseBuilder.AMainExercise()
            .WithId(exerciseId)
            .WithNotes("Test notes")
            .Build();
        
        _workoutTemplateExerciseRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(exerciseId))
            .ReturnsAsync(exercise);

        // Act
        var result = await _testee.GetByIdAsync(exerciseId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(exerciseId.ToString());
        result.Data.Notes.Should().Be("Test notes");
    }

    #endregion

    #region AddExerciseAsync Tests

    [Fact]
    public async Task AddExerciseAsync_WhenCommandIsNull_ShouldReturnFailure()
    {
        // Arrange
        AddExerciseToTemplateCommand? command = null;

        // Act
        var result = await _testee.AddExerciseAsync(command!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.CommandCannotBeNull);
    }

    [Fact]
    public async Task AddExerciseAsync_WhenWorkoutTemplateIdIsEmpty_ShouldReturnFailure()
    {
        // Arrange
        var command = new AddExerciseToTemplateCommand
        {
            WorkoutTemplateId = WorkoutTemplateId.Empty,
            ExerciseId = ExerciseId.From(Guid.NewGuid()),
            Zone = "Main",
            UserId = UserId.From(Guid.NewGuid())
        };

        // Act
        var result = await _testee.AddExerciseAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.InvalidCommandParameters);
    }

    [Fact]
    public async Task AddExerciseAsync_WhenZoneIsInvalid_ShouldReturnFailure()
    {
        // Arrange
        var command = new AddExerciseToTemplateCommand
        {
            WorkoutTemplateId = WorkoutTemplateId.From(Guid.NewGuid()),
            ExerciseId = ExerciseId.From(Guid.NewGuid()),
            Zone = "InvalidZone",
            UserId = UserId.From(Guid.NewGuid())
        };

        // Act
        var result = await _testee.AddExerciseAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.First().Should().Contain("InvalidZone");
    }

    [Fact]
    public async Task AddExerciseAsync_WhenTemplateNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var command = new AddExerciseToTemplateCommand
        {
            WorkoutTemplateId = WorkoutTemplateId.From(Guid.NewGuid()),
            ExerciseId = ExerciseId.From(Guid.NewGuid()),
            Zone = "Main",
            UserId = UserId.From(Guid.NewGuid())
        };

        _workoutTemplateRepositoryMock
            .Setup(x => x.GetByIdAsync(command.WorkoutTemplateId))
            .ReturnsAsync(API.Models.Entities.WorkoutTemplate.Empty);

        // Act
        var result = await _testee.AddExerciseAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.NotFound);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.WorkoutTemplateNotFound + " not found");
    }

    [Fact]
    public async Task AddExerciseAsync_WhenTemplateNotInDraftState_ShouldReturnFailure()
    {
        // Arrange
        var command = new AddExerciseToTemplateCommand
        {
            WorkoutTemplateId = WorkoutTemplateId.From(Guid.NewGuid()),
            ExerciseId = ExerciseId.From(Guid.NewGuid()),
            Zone = "Main",
            UserId = UserId.From(Guid.NewGuid())
        };

        var template = WorkoutTemplateBuilder.AProductionWorkoutTemplate()
            .WithWorkoutState(WorkoutState.Handler.Create(
                WorkoutStateId.From(Guid.NewGuid()),
                "PRODUCTION",
                "Production state",
                2,
                true).Value)
            .Build();

        _workoutTemplateRepositoryMock
            .Setup(x => x.GetByIdAsync(command.WorkoutTemplateId))
            .ReturnsAsync(template);

        // Act
        var result = await _testee.AddExerciseAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.CanOnlyAddExercisesToDraftTemplates);
    }

    [Fact]
    public async Task AddExerciseAsync_WhenValidCommand_ShouldCreateExercise()
    {
        // Arrange
        var command = new AddExerciseToTemplateCommand
        {
            WorkoutTemplateId = WorkoutTemplateId.From(Guid.NewGuid()),
            ExerciseId = ExerciseId.From(Guid.NewGuid()),
            Zone = "Main",
            UserId = UserId.From(Guid.NewGuid()),
            Notes = "Test notes",
            SequenceOrder = null
        };

        var template = WorkoutTemplateBuilder.ADraftWorkoutTemplate()
            .WithWorkoutState(WorkoutState.Handler.Create(
                WorkoutStateId.From(Guid.NewGuid()),
                "DRAFT",
                "Draft state",
                1,
                true).Value)
            .Build();

        var exercise = ExerciseBuilder.AWorkoutExercise().Build();

        _workoutTemplateRepositoryMock
            .Setup(x => x.GetByIdAsync(command.WorkoutTemplateId))
            .ReturnsAsync(template);

        _exerciseRepositoryMock
            .Setup(x => x.GetByIdAsync(command.ExerciseId))
            .ReturnsAsync(exercise);

        _workoutTemplateExerciseRepositoryMock
            .Setup(x => x.GetMaxSequenceOrderAsync(command.WorkoutTemplateId, WorkoutZone.Main))
            .ReturnsAsync(2);

        var createdExercise = WorkoutTemplateExerciseBuilder.AMainExercise()
            .WithExercise(exercise)
            .WithNotes("Test notes")
            .WithSequenceOrder(3)
            .Build();

        _workoutTemplateExerciseRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<WorkoutTemplateExerciseId>()))
            .ReturnsAsync(createdExercise);

        // Act
        var result = await _testee.AddExerciseAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Notes.Should().Be("Test notes");
        result.Data.SequenceOrder.Should().Be(3);
        
        _workoutTemplateExerciseRepositoryMock.Verify(x => 
            x.AddAsync(It.IsAny<WorkoutTemplateExercise>()), Times.Once);
        _writableUnitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
    }

    #endregion

    #region RemoveExerciseAsync Tests

    [Fact]
    public async Task RemoveExerciseAsync_WhenExerciseIdIsEmpty_ShouldReturnFailure()
    {
        // Arrange
        var emptyId = WorkoutTemplateExerciseId.Empty;

        // Act
        var result = await _testee.RemoveExerciseAsync(emptyId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.InvalidExerciseId);
    }

    [Fact]
    public async Task RemoveExerciseAsync_WhenExerciseNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var exerciseId = WorkoutTemplateExerciseId.From(Guid.NewGuid());

        _workoutTemplateExerciseRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(exerciseId))
            .ReturnsAsync(WorkoutTemplateExercise.Empty);

        // Act
        var result = await _testee.RemoveExerciseAsync(exerciseId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.NotFound);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.TemplateExerciseNotFound + " not found");
    }

    [Fact]
    public async Task RemoveExerciseAsync_WhenValidRequest_ShouldRemoveExercise()
    {
        // Arrange
        var exerciseId = WorkoutTemplateExerciseId.From(Guid.NewGuid());
        var workoutTemplateId = WorkoutTemplateId.From(Guid.NewGuid());
        
        var exerciseTemplate = WorkoutTemplateExerciseBuilder.AMainExercise()
            .WithWorkoutTemplateId(workoutTemplateId)
            .Build();

        var template = WorkoutTemplateBuilder.ADraftWorkoutTemplate()
            .WithWorkoutState(WorkoutState.Handler.Create(
                WorkoutStateId.From(Guid.NewGuid()),
                "DRAFT",
                "Draft state",
                1,
                true).Value)
            .Build();

        _workoutTemplateExerciseRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(exerciseId))
            .ReturnsAsync(exerciseTemplate);

        _workoutTemplateRepositoryMock
            .Setup(x => x.GetByIdAsync(workoutTemplateId))
            .ReturnsAsync(template);

        // Act
        var result = await _testee.RemoveExerciseAsync(exerciseId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeTrue();
        
        _workoutTemplateExerciseRepositoryMock.Verify(x => 
            x.DeleteAsync(exerciseId), Times.Once);
        _writableUnitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
    }

    #endregion

    #region UpdateExerciseAsync Tests

    [Fact]
    public async Task UpdateExerciseAsync_WhenCommandIsNull_ShouldReturnFailure()
    {
        // Arrange
        UpdateTemplateExerciseCommand? command = null;

        // Act
        var result = await _testee.UpdateExerciseAsync(command!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.CommandCannotBeNull);
    }

    [Fact]
    public async Task UpdateExerciseAsync_WhenWorkoutTemplateExerciseIdIsEmpty_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdateTemplateExerciseCommand
        {
            WorkoutTemplateExerciseId = WorkoutTemplateExerciseId.Empty,
            UserId = UserId.From(Guid.NewGuid()),
            Notes = "Updated notes"
        };

        // Act
        var result = await _testee.UpdateExerciseAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.InvalidCommandParameters);
    }

    [Fact]
    public async Task UpdateExerciseAsync_WhenUserIdIsEmpty_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdateTemplateExerciseCommand
        {
            WorkoutTemplateExerciseId = WorkoutTemplateExerciseId.From(Guid.NewGuid()),
            UserId = UserId.Empty,
            Notes = "Updated notes"
        };

        // Act
        var result = await _testee.UpdateExerciseAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.InvalidCommandParameters);
    }

    [Fact]
    public async Task UpdateExerciseAsync_WhenExerciseNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var command = new UpdateTemplateExerciseCommand
        {
            WorkoutTemplateExerciseId = WorkoutTemplateExerciseId.From(Guid.NewGuid()),
            UserId = UserId.From(Guid.NewGuid()),
            Notes = "Updated notes"
        };

        _workoutTemplateExerciseRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(command.WorkoutTemplateExerciseId))
            .ReturnsAsync(WorkoutTemplateExercise.Empty);

        // Act
        var result = await _testee.UpdateExerciseAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.NotFound);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.TemplateExerciseNotFound + " not found");
    }

    [Fact]
    public async Task UpdateExerciseAsync_WhenTemplateNotInDraftState_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdateTemplateExerciseCommand
        {
            WorkoutTemplateExerciseId = WorkoutTemplateExerciseId.From(Guid.NewGuid()),
            UserId = UserId.From(Guid.NewGuid()),
            Notes = "Updated notes"
        };

        var workoutTemplateId = WorkoutTemplateId.From(Guid.NewGuid());
        var exerciseTemplate = WorkoutTemplateExerciseBuilder.AMainExercise()
            .WithWorkoutTemplateId(workoutTemplateId)
            .Build();

        var template = WorkoutTemplateBuilder.AProductionWorkoutTemplate()
            .WithWorkoutState(WorkoutState.Handler.Create(
                WorkoutStateId.From(Guid.NewGuid()),
                "PRODUCTION",
                "Production state",
                2,
                true).Value)
            .Build();

        _workoutTemplateExerciseRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(command.WorkoutTemplateExerciseId))
            .ReturnsAsync(exerciseTemplate);

        _workoutTemplateRepositoryMock
            .Setup(x => x.GetByIdAsync(workoutTemplateId))
            .ReturnsAsync(template);

        // Act
        var result = await _testee.UpdateExerciseAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.CanOnlyUpdateExercisesInDraftTemplates);
    }

    [Fact]
    public async Task UpdateExerciseAsync_WhenValidCommand_ShouldUpdateExercise()
    {
        // Arrange
        var command = new UpdateTemplateExerciseCommand
        {
            WorkoutTemplateExerciseId = WorkoutTemplateExerciseId.From(Guid.NewGuid()),
            UserId = UserId.From(Guid.NewGuid()),
            Notes = "Updated notes"
        };

        var workoutTemplateId = WorkoutTemplateId.From(Guid.NewGuid());
        var exerciseTemplate = WorkoutTemplateExerciseBuilder.AMainExercise()
            .WithWorkoutTemplateId(workoutTemplateId)
            .WithNotes("Original notes")
            .Build();

        var template = WorkoutTemplateBuilder.ADraftWorkoutTemplate()
            .WithWorkoutState(WorkoutState.Handler.Create(
                WorkoutStateId.From(Guid.NewGuid()),
                "DRAFT",
                "Draft state",
                1,
                true).Value)
            .Build();

        var updatedExercise = WorkoutTemplateExerciseBuilder.AMainExercise()
            .WithWorkoutTemplateId(workoutTemplateId)
            .WithNotes("Updated notes")
            .Build();

        _workoutTemplateExerciseRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(command.WorkoutTemplateExerciseId))
            .ReturnsAsync(exerciseTemplate);

        _workoutTemplateRepositoryMock
            .Setup(x => x.GetByIdAsync(workoutTemplateId))
            .ReturnsAsync(template);

        _workoutTemplateExerciseRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<WorkoutTemplateExerciseId>()))
            .ReturnsAsync(updatedExercise);

        // Act
        var result = await _testee.UpdateExerciseAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Notes.Should().Be("Updated notes");

        _workoutTemplateExerciseRepositoryMock.Verify(x =>
            x.UpdateAsync(It.IsAny<WorkoutTemplateExercise>()), Times.Once);
        _writableUnitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
    }

    #endregion

    #region ReorderExercisesAsync Tests

    [Fact]
    public async Task ReorderExercisesAsync_WhenCommandIsNull_ShouldReturnFailure()
    {
        // Arrange
        ReorderTemplateExercisesCommand? command = null;

        // Act
        var result = await _testee.ReorderExercisesAsync(command!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.CommandCannotBeNull);
    }

    [Fact]
    public async Task ReorderExercisesAsync_WhenWorkoutTemplateIdIsEmpty_ShouldReturnFailure()
    {
        // Arrange
        var command = new ReorderTemplateExercisesCommand
        {
            WorkoutTemplateId = WorkoutTemplateId.Empty,
            Zone = "Main",
            ExerciseIds = new List<WorkoutTemplateExerciseId> { WorkoutTemplateExerciseId.From(Guid.NewGuid()) },
            UserId = UserId.From(Guid.NewGuid())
        };

        // Act
        var result = await _testee.ReorderExercisesAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.InvalidCommandParameters);
    }

    [Fact]
    public async Task ReorderExercisesAsync_WhenZoneIsEmpty_ShouldReturnFailure()
    {
        // Arrange
        var command = new ReorderTemplateExercisesCommand
        {
            WorkoutTemplateId = WorkoutTemplateId.From(Guid.NewGuid()),
            Zone = "",
            ExerciseIds = new List<WorkoutTemplateExerciseId> { WorkoutTemplateExerciseId.From(Guid.NewGuid()) },
            UserId = UserId.From(Guid.NewGuid())
        };

        // Act
        var result = await _testee.ReorderExercisesAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.InvalidCommandParameters);
    }

    [Fact]
    public async Task ReorderExercisesAsync_WhenExerciseIdsIsEmpty_ShouldReturnFailure()
    {
        // Arrange
        var command = new ReorderTemplateExercisesCommand
        {
            WorkoutTemplateId = WorkoutTemplateId.From(Guid.NewGuid()),
            Zone = "Main",
            ExerciseIds = new List<WorkoutTemplateExerciseId>(),
            UserId = UserId.From(Guid.NewGuid())
        };

        // Act
        var result = await _testee.ReorderExercisesAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.InvalidCommandParameters);
    }

    [Fact]
    public async Task ReorderExercisesAsync_WhenZoneIsInvalid_ShouldReturnFailure()
    {
        // Arrange
        var command = new ReorderTemplateExercisesCommand
        {
            WorkoutTemplateId = WorkoutTemplateId.From(Guid.NewGuid()),
            Zone = "InvalidZone",
            ExerciseIds = new List<WorkoutTemplateExerciseId> { WorkoutTemplateExerciseId.From(Guid.NewGuid()) },
            UserId = UserId.From(Guid.NewGuid())
        };

        // Act
        var result = await _testee.ReorderExercisesAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.First().Should().Contain("InvalidZone");
    }

    [Fact]
    public async Task ReorderExercisesAsync_WhenTemplateNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var command = new ReorderTemplateExercisesCommand
        {
            WorkoutTemplateId = WorkoutTemplateId.From(Guid.NewGuid()),
            Zone = "Main",
            ExerciseIds = new List<WorkoutTemplateExerciseId> { WorkoutTemplateExerciseId.From(Guid.NewGuid()) },
            UserId = UserId.From(Guid.NewGuid())
        };

        _workoutTemplateRepositoryMock
            .Setup(x => x.GetByIdAsync(command.WorkoutTemplateId))
            .ReturnsAsync(API.Models.Entities.WorkoutTemplate.Empty);

        // Act
        var result = await _testee.ReorderExercisesAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.NotFound);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.WorkoutTemplateNotFound + " not found");
    }

    [Fact]
    public async Task ReorderExercisesAsync_WhenTemplateNotInDraftState_ShouldReturnFailure()
    {
        // Arrange
        var command = new ReorderTemplateExercisesCommand
        {
            WorkoutTemplateId = WorkoutTemplateId.From(Guid.NewGuid()),
            Zone = "Main",
            ExerciseIds = new List<WorkoutTemplateExerciseId> { WorkoutTemplateExerciseId.From(Guid.NewGuid()) },
            UserId = UserId.From(Guid.NewGuid())
        };

        var template = WorkoutTemplateBuilder.AProductionWorkoutTemplate()
            .WithWorkoutState(WorkoutState.Handler.Create(
                WorkoutStateId.From(Guid.NewGuid()),
                "PRODUCTION",
                "Production state",
                2,
                true).Value)
            .Build();

        _workoutTemplateRepositoryMock
            .Setup(x => x.GetByIdAsync(command.WorkoutTemplateId))
            .ReturnsAsync(template);

        // Act
        var result = await _testee.ReorderExercisesAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.CanOnlyReorderExercisesInDraftTemplates);
    }

    [Fact]
    public async Task ReorderExercisesAsync_WhenValidCommand_ShouldReorderExercises()
    {
        // Arrange
        var exerciseId1 = WorkoutTemplateExerciseId.From(Guid.NewGuid());
        var exerciseId2 = WorkoutTemplateExerciseId.From(Guid.NewGuid());
        var command = new ReorderTemplateExercisesCommand
        {
            WorkoutTemplateId = WorkoutTemplateId.From(Guid.NewGuid()),
            Zone = "Main",
            ExerciseIds = new List<WorkoutTemplateExerciseId> { exerciseId1, exerciseId2 },
            UserId = UserId.From(Guid.NewGuid())
        };

        var template = WorkoutTemplateBuilder.ADraftWorkoutTemplate()
            .WithWorkoutState(WorkoutState.Handler.Create(
                WorkoutStateId.From(Guid.NewGuid()),
                "DRAFT",
                "Draft state",
                1,
                true).Value)
            .Build();

        _workoutTemplateRepositoryMock
            .Setup(x => x.GetByIdAsync(command.WorkoutTemplateId))
            .ReturnsAsync(template);

        // Act
        var result = await _testee.ReorderExercisesAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeTrue();

        _workoutTemplateExerciseRepositoryMock.Verify(x =>
            x.ReorderExercisesAsync(
                command.WorkoutTemplateId,
                WorkoutZone.Main,
                It.IsAny<Dictionary<WorkoutTemplateExerciseId, int>>()), Times.Once);
        _writableUnitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
    }

    #endregion

    #region ChangeExerciseZoneAsync Tests

    [Fact]
    public async Task ChangeExerciseZoneAsync_WhenCommandIsNull_ShouldReturnFailure()
    {
        // Arrange
        ChangeExerciseZoneCommand? command = null;

        // Act
        var result = await _testee.ChangeExerciseZoneAsync(command!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.CommandCannotBeNull);
    }

    [Fact]
    public async Task ChangeExerciseZoneAsync_WhenWorkoutTemplateExerciseIdIsEmpty_ShouldReturnFailure()
    {
        // Arrange
        var command = new ChangeExerciseZoneCommand
        {
            WorkoutTemplateExerciseId = WorkoutTemplateExerciseId.Empty,
            NewZone = "Main",
            UserId = UserId.From(Guid.NewGuid())
        };

        // Act
        var result = await _testee.ChangeExerciseZoneAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.InvalidCommandParameters);
    }

    [Fact]
    public async Task ChangeExerciseZoneAsync_WhenNewZoneIsEmpty_ShouldReturnFailure()
    {
        // Arrange
        var command = new ChangeExerciseZoneCommand
        {
            WorkoutTemplateExerciseId = WorkoutTemplateExerciseId.From(Guid.NewGuid()),
            NewZone = "",
            UserId = UserId.From(Guid.NewGuid())
        };

        // Act
        var result = await _testee.ChangeExerciseZoneAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.InvalidCommandParameters);
    }

    [Fact]
    public async Task ChangeExerciseZoneAsync_WhenNewZoneIsInvalid_ShouldReturnFailure()
    {
        // Arrange
        var command = new ChangeExerciseZoneCommand
        {
            WorkoutTemplateExerciseId = WorkoutTemplateExerciseId.From(Guid.NewGuid()),
            NewZone = "InvalidZone",
            UserId = UserId.From(Guid.NewGuid())
        };

        // Act
        var result = await _testee.ChangeExerciseZoneAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.First().Should().Contain("InvalidZone");
    }

    [Fact]
    public async Task ChangeExerciseZoneAsync_WhenExerciseNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var command = new ChangeExerciseZoneCommand
        {
            WorkoutTemplateExerciseId = WorkoutTemplateExerciseId.From(Guid.NewGuid()),
            NewZone = "Main",
            UserId = UserId.From(Guid.NewGuid())
        };

        _workoutTemplateExerciseRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(command.WorkoutTemplateExerciseId))
            .ReturnsAsync(WorkoutTemplateExercise.Empty);

        // Act
        var result = await _testee.ChangeExerciseZoneAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.NotFound);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.TemplateExerciseNotFound + " not found");
    }

    [Fact]
    public async Task ChangeExerciseZoneAsync_WhenTemplateNotInDraftState_ShouldReturnFailure()
    {
        // Arrange
        var command = new ChangeExerciseZoneCommand
        {
            WorkoutTemplateExerciseId = WorkoutTemplateExerciseId.From(Guid.NewGuid()),
            NewZone = "Main",
            UserId = UserId.From(Guid.NewGuid())
        };

        var workoutTemplateId = WorkoutTemplateId.From(Guid.NewGuid());
        var exerciseTemplate = WorkoutTemplateExerciseBuilder.AWarmupExercise()
            .WithWorkoutTemplateId(workoutTemplateId)
            .Build();

        var template = WorkoutTemplateBuilder.AProductionWorkoutTemplate()
            .WithWorkoutState(WorkoutState.Handler.Create(
                WorkoutStateId.From(Guid.NewGuid()),
                "PRODUCTION",
                "Production state",
                2,
                true).Value)
            .Build();

        _workoutTemplateExerciseRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(command.WorkoutTemplateExerciseId))
            .ReturnsAsync(exerciseTemplate);

        _workoutTemplateRepositoryMock
            .Setup(x => x.GetByIdAsync(workoutTemplateId))
            .ReturnsAsync(template);

        // Act
        var result = await _testee.ChangeExerciseZoneAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.CanOnlyChangeZonesInDraftTemplates);
    }

    [Fact]
    public async Task ChangeExerciseZoneAsync_WhenValidCommand_ShouldChangeZone()
    {
        // Arrange
        var command = new ChangeExerciseZoneCommand
        {
            WorkoutTemplateExerciseId = WorkoutTemplateExerciseId.From(Guid.NewGuid()),
            NewZone = "Main",
            UserId = UserId.From(Guid.NewGuid()),
            NewSequenceOrder = 2
        };

        var workoutTemplateId = WorkoutTemplateId.From(Guid.NewGuid());
        var exerciseTemplate = WorkoutTemplateExerciseBuilder.AWarmupExercise()
            .WithWorkoutTemplateId(workoutTemplateId)
            .Build();

        var template = WorkoutTemplateBuilder.ADraftWorkoutTemplate()
            .WithWorkoutState(WorkoutState.Handler.Create(
                WorkoutStateId.From(Guid.NewGuid()),
                "DRAFT",
                "Draft state",
                1,
                true).Value)
            .Build();

        var updatedExercise = WorkoutTemplateExerciseBuilder.AMainExercise()
            .WithWorkoutTemplateId(workoutTemplateId)
            .WithSequenceOrder(2)
            .Build();

        _workoutTemplateExerciseRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(command.WorkoutTemplateExerciseId))
            .ReturnsAsync(exerciseTemplate);

        _workoutTemplateRepositoryMock
            .Setup(x => x.GetByIdAsync(workoutTemplateId))
            .ReturnsAsync(template);

        _workoutTemplateExerciseRepositoryMock
            .Setup(x => x.GetMaxSequenceOrderAsync(workoutTemplateId, WorkoutZone.Main))
            .ReturnsAsync(1);

        _workoutTemplateExerciseRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<WorkoutTemplateExerciseId>()))
            .ReturnsAsync(updatedExercise);

        // Act
        var result = await _testee.ChangeExerciseZoneAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Zone.Should().Be("Main");
        result.Data.SequenceOrder.Should().Be(2);

        _workoutTemplateExerciseRepositoryMock.Verify(x =>
            x.UpdateAsync(It.IsAny<WorkoutTemplateExercise>()), Times.Once);
        _writableUnitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
    }

    #endregion

    #region DuplicateExercisesAsync Tests

    [Fact]
    public async Task DuplicateExercisesAsync_WhenCommandIsNull_ShouldReturnFailure()
    {
        // Arrange
        DuplicateTemplateExercisesCommand? command = null;

        // Act
        var result = await _testee.DuplicateExercisesAsync(command!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.CommandCannotBeNull);
    }

    [Fact]
    public async Task DuplicateExercisesAsync_WhenSourceTemplateIdIsEmpty_ShouldReturnFailure()
    {
        // Arrange
        var command = new DuplicateTemplateExercisesCommand
        {
            SourceTemplateId = WorkoutTemplateId.Empty,
            TargetTemplateId = WorkoutTemplateId.From(Guid.NewGuid()),
            UserId = UserId.From(Guid.NewGuid())
        };

        // Act
        var result = await _testee.DuplicateExercisesAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.InvalidCommandParameters);
    }

    [Fact]
    public async Task DuplicateExercisesAsync_WhenTargetTemplateIdIsEmpty_ShouldReturnFailure()
    {
        // Arrange
        var command = new DuplicateTemplateExercisesCommand
        {
            SourceTemplateId = WorkoutTemplateId.From(Guid.NewGuid()),
            TargetTemplateId = WorkoutTemplateId.Empty,
            UserId = UserId.From(Guid.NewGuid())
        };

        // Act
        var result = await _testee.DuplicateExercisesAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.InvalidCommandParameters);
    }

    [Fact]
    public async Task DuplicateExercisesAsync_WhenSourceTemplateNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var command = new DuplicateTemplateExercisesCommand
        {
            SourceTemplateId = WorkoutTemplateId.From(Guid.NewGuid()),
            TargetTemplateId = WorkoutTemplateId.From(Guid.NewGuid()),
            UserId = UserId.From(Guid.NewGuid())
        };

        _workoutTemplateRepositoryMock
            .Setup(x => x.GetByIdAsync(command.SourceTemplateId))
            .ReturnsAsync(API.Models.Entities.WorkoutTemplate.Empty);

        // Act
        var result = await _testee.DuplicateExercisesAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.NotFound);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.SourceTemplateNotFound + " not found");
    }

    [Fact]
    public async Task DuplicateExercisesAsync_WhenTargetTemplateNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var command = new DuplicateTemplateExercisesCommand
        {
            SourceTemplateId = WorkoutTemplateId.From(Guid.NewGuid()),
            TargetTemplateId = WorkoutTemplateId.From(Guid.NewGuid()),
            UserId = UserId.From(Guid.NewGuid())
        };

        var sourceTemplate = WorkoutTemplateBuilder.ADraftWorkoutTemplate().Build();

        _workoutTemplateRepositoryMock
            .Setup(x => x.GetByIdAsync(command.SourceTemplateId))
            .ReturnsAsync(sourceTemplate);

        _workoutTemplateRepositoryMock
            .Setup(x => x.GetByIdAsync(command.TargetTemplateId))
            .ReturnsAsync(API.Models.Entities.WorkoutTemplate.Empty);

        // Act
        var result = await _testee.DuplicateExercisesAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.NotFound);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.TargetTemplateNotFound + " not found");
    }

    [Fact]
    public async Task DuplicateExercisesAsync_WhenTargetTemplateNotInDraftState_ShouldReturnFailure()
    {
        // Arrange
        var command = new DuplicateTemplateExercisesCommand
        {
            SourceTemplateId = WorkoutTemplateId.From(Guid.NewGuid()),
            TargetTemplateId = WorkoutTemplateId.From(Guid.NewGuid()),
            UserId = UserId.From(Guid.NewGuid())
        };

        var sourceTemplate = WorkoutTemplateBuilder.ADraftWorkoutTemplate().Build();
        var targetTemplate = WorkoutTemplateBuilder.AProductionWorkoutTemplate()
            .WithWorkoutState(WorkoutState.Handler.Create(
                WorkoutStateId.From(Guid.NewGuid()),
                "PRODUCTION",
                "Production state",
                2,
                true).Value)
            .Build();

        _workoutTemplateRepositoryMock
            .Setup(x => x.GetByIdAsync(command.SourceTemplateId))
            .ReturnsAsync(sourceTemplate);

        _workoutTemplateRepositoryMock
            .Setup(x => x.GetByIdAsync(command.TargetTemplateId))
            .ReturnsAsync(targetTemplate);

        // Act
        var result = await _testee.DuplicateExercisesAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.CanOnlyDuplicateExercisesToDraftTemplates);
    }

    [Fact]
    public async Task DuplicateExercisesAsync_WhenValidCommandWithoutSetConfigurations_ShouldDuplicateExercises()
    {
        // Arrange
        var command = new DuplicateTemplateExercisesCommand
        {
            SourceTemplateId = WorkoutTemplateId.From(Guid.NewGuid()),
            TargetTemplateId = WorkoutTemplateId.From(Guid.NewGuid()),
            UserId = UserId.From(Guid.NewGuid()),
            IncludeSetConfigurations = false
        };

        var sourceTemplate = WorkoutTemplateBuilder.ADraftWorkoutTemplate().Build();
        var targetTemplate = WorkoutTemplateBuilder.ADraftWorkoutTemplate()
            .WithWorkoutState(WorkoutState.Handler.Create(
                WorkoutStateId.From(Guid.NewGuid()),
                "DRAFT",
                "Draft state",
                1,
                true).Value)
            .Build();

        var sourceExercises = new List<WorkoutTemplateExercise>
        {
            WorkoutTemplateExerciseBuilder.AMainExercise()
                .WithWorkoutTemplateId(command.SourceTemplateId)
                .Build(),
            WorkoutTemplateExerciseBuilder.AWarmupExercise()
                .WithWorkoutTemplateId(command.SourceTemplateId)
                .Build()
        };

        _workoutTemplateRepositoryMock
            .Setup(x => x.GetByIdAsync(command.SourceTemplateId))
            .ReturnsAsync(sourceTemplate);

        _workoutTemplateRepositoryMock
            .Setup(x => x.GetByIdAsync(command.TargetTemplateId))
            .ReturnsAsync(targetTemplate);

        _workoutTemplateExerciseRepositoryMock
            .Setup(x => x.GetByWorkoutTemplateAsync(command.SourceTemplateId))
            .ReturnsAsync(sourceExercises);

        // Act
        var result = await _testee.DuplicateExercisesAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(2);

        _workoutTemplateExerciseRepositoryMock.Verify(x =>
            x.AddRangeAsync(It.Is<List<WorkoutTemplateExercise>>(list => list.Count == 2)), Times.Once);
        _setConfigurationRepositoryMock.Verify(x =>
            x.AddRangeAsync(It.IsAny<IEnumerable<SetConfiguration>>()), Times.Never);
        _writableUnitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task DuplicateExercisesAsync_WhenValidCommandWithSetConfigurations_ShouldDuplicateExercisesAndConfigurations()
    {
        // Arrange
        var command = new DuplicateTemplateExercisesCommand
        {
            SourceTemplateId = WorkoutTemplateId.From(Guid.NewGuid()),
            TargetTemplateId = WorkoutTemplateId.From(Guid.NewGuid()),
            UserId = UserId.From(Guid.NewGuid()),
            IncludeSetConfigurations = true
        };

        var sourceTemplate = WorkoutTemplateBuilder.ADraftWorkoutTemplate().Build();
        var targetTemplate = WorkoutTemplateBuilder.ADraftWorkoutTemplate()
            .WithWorkoutState(WorkoutState.Handler.Create(
                WorkoutStateId.From(Guid.NewGuid()),
                "DRAFT",
                "Draft state",
                1,
                true).Value)
            .Build();

        var setConfig = SetConfiguration.Handler.CreateNew(
            WorkoutTemplateExerciseId.From(Guid.NewGuid()),
            1,
            "10",
            50.0m,
            60,
            60).Value;

        var sourceExercise = WorkoutTemplateExerciseBuilder.AMainExercise()
            .WithWorkoutTemplateId(command.SourceTemplateId)
            .Build();

        // Manually set configurations since builder pattern may not persist them
        var sourceExerciseWithConfigs = sourceExercise with { Configurations = new List<SetConfiguration> { setConfig } };
        var sourceExercises = new List<WorkoutTemplateExercise> { sourceExerciseWithConfigs };

        _workoutTemplateRepositoryMock
            .Setup(x => x.GetByIdAsync(command.SourceTemplateId))
            .ReturnsAsync(sourceTemplate);

        _workoutTemplateRepositoryMock
            .Setup(x => x.GetByIdAsync(command.TargetTemplateId))
            .ReturnsAsync(targetTemplate);

        _workoutTemplateExerciseRepositoryMock
            .Setup(x => x.GetByWorkoutTemplateAsync(command.SourceTemplateId))
            .ReturnsAsync(sourceExercises);

        // Act
        var result = await _testee.DuplicateExercisesAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(1);

        _workoutTemplateExerciseRepositoryMock.Verify(x =>
            x.AddRangeAsync(It.Is<List<WorkoutTemplateExercise>>(list => list.Count == 1)), Times.Once);
        // Set configurations should be duplicated when includeSetConfigurations is true
        // and source exercise has configurations
        _setConfigurationRepositoryMock.Verify(x =>
            x.AddRangeAsync(It.IsAny<IEnumerable<SetConfiguration>>()), Times.Once);
        _writableUnitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
    }

    #endregion

    #region GetExerciseSuggestionsAsync Tests

    [Fact]
    public async Task GetExerciseSuggestionsAsync_WhenWorkoutTemplateIdIsEmpty_ShouldReturnFailure()
    {
        // Arrange
        var emptyId = WorkoutTemplateId.Empty;
        const string zone = "Main";

        // Act
        var result = await _testee.GetExerciseSuggestionsAsync(emptyId, zone);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.InvalidTemplateIdOrZone);
    }

    [Fact]
    public async Task GetExerciseSuggestionsAsync_WhenZoneIsEmpty_ShouldReturnFailure()
    {
        // Arrange
        var templateId = WorkoutTemplateId.From(Guid.NewGuid());
        const string zone = "";

        // Act
        var result = await _testee.GetExerciseSuggestionsAsync(templateId, zone);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.InvalidTemplateIdOrZone);
    }

    [Fact]
    public async Task GetExerciseSuggestionsAsync_WhenValidParameters_ShouldReturnEmptyList()
    {
        // Arrange
        var templateId = WorkoutTemplateId.From(Guid.NewGuid());
        const string zone = "Main";
        const int maxSuggestions = 3;

        // Act
        var result = await _testee.GetExerciseSuggestionsAsync(templateId, zone, maxSuggestions);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEmpty(); // As per TODO comment, returns empty list
    }

    #endregion

    #region ValidateExercisesAsync Tests

    [Fact]
    public async Task ValidateExercisesAsync_WhenWorkoutTemplateIdIsEmpty_ShouldReturnFailure()
    {
        // Arrange
        var emptyId = WorkoutTemplateId.Empty;
        var exerciseIds = new List<ExerciseId> { ExerciseId.From(Guid.NewGuid()) };

        // Act
        var result = await _testee.ValidateExercisesAsync(emptyId, exerciseIds);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.InvalidTemplateIdOrExerciseList);
    }

    [Fact]
    public async Task ValidateExercisesAsync_WhenExerciseIdsIsEmpty_ShouldReturnFailure()
    {
        // Arrange
        var templateId = WorkoutTemplateId.From(Guid.NewGuid());
        var exerciseIds = new List<ExerciseId>();

        // Act
        var result = await _testee.ValidateExercisesAsync(templateId, exerciseIds);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.InvalidTemplateIdOrExerciseList);
    }

    [Fact]
    public async Task ValidateExercisesAsync_WhenTemplateNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var templateId = WorkoutTemplateId.From(Guid.NewGuid());
        var exerciseIds = new List<ExerciseId> { ExerciseId.From(Guid.NewGuid()) };

        _workoutTemplateRepositoryMock
            .Setup(x => x.GetByIdAsync(templateId))
            .ReturnsAsync(API.Models.Entities.WorkoutTemplate.Empty);

        // Act
        var result = await _testee.ValidateExercisesAsync(templateId, exerciseIds);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.NotFound);
        result.Errors.First().Should().Be(WorkoutTemplateExerciseErrorMessages.WorkoutTemplateNotFound + " not found");
    }

    [Fact]
    public async Task ValidateExercisesAsync_WhenExerciseNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var templateId = WorkoutTemplateId.From(Guid.NewGuid());
        var exerciseId = ExerciseId.From(Guid.NewGuid());
        var exerciseIds = new List<ExerciseId> { exerciseId };

        var template = WorkoutTemplateBuilder.ADraftWorkoutTemplate().Build();

        _workoutTemplateRepositoryMock
            .Setup(x => x.GetByIdAsync(templateId))
            .ReturnsAsync(template);

        _exerciseRepositoryMock
            .Setup(x => x.GetByIdAsync(exerciseId))
            .ReturnsAsync(ExerciseBuilder.AnExercise().WithId(ExerciseId.Empty).Build());

        // Act
        var result = await _testee.ValidateExercisesAsync(templateId, exerciseIds);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.NotFound);
        result.Errors.First().Should().Contain(exerciseId.ToString());
    }

    [Fact]
    public async Task ValidateExercisesAsync_WhenAllExercisesValid_ShouldReturnSuccess()
    {
        // Arrange
        var templateId = WorkoutTemplateId.From(Guid.NewGuid());
        var exerciseId1 = ExerciseId.From(Guid.NewGuid());
        var exerciseId2 = ExerciseId.From(Guid.NewGuid());
        var exerciseIds = new List<ExerciseId> { exerciseId1, exerciseId2 };

        var template = WorkoutTemplateBuilder.ADraftWorkoutTemplate().Build();
        var exercise1 = ExerciseBuilder.AWorkoutExercise().Build();
        var exercise2 = ExerciseBuilder.AWorkoutExercise().Build();

        _workoutTemplateRepositoryMock
            .Setup(x => x.GetByIdAsync(templateId))
            .ReturnsAsync(template);

        _exerciseRepositoryMock
            .Setup(x => x.GetByIdAsync(exerciseId1))
            .ReturnsAsync(exercise1);

        _exerciseRepositoryMock
            .Setup(x => x.GetByIdAsync(exerciseId2))
            .ReturnsAsync(exercise2);

        // Act
        var result = await _testee.ValidateExercisesAsync(templateId, exerciseIds);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeTrue();
    }

    #endregion
}