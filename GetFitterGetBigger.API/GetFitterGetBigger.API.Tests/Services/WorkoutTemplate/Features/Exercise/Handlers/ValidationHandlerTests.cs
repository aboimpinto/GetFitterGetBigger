using FluentAssertions;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise.Handlers;
using GetFitterGetBigger.API.Tests.TestBuilders.Domain;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using Olimpo.EntityFramework.Persistency;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services.WorkoutTemplate.Features.Exercise.Handlers;

public class ValidationHandlerTests
{
    // No shared state - each test creates its own AutoMocker instance

    #region IsTemplateInDraftStateAsync Tests

    [Fact]
    public async Task IsTemplateInDraftStateAsync_WhenTemplateIsInDraftState_ShouldReturnTrue()
    {
        // Arrange
        const string draftState = "Draft";
        
        var autoMocker = new AutoMocker();
        var templateId = WorkoutTemplateId.New();
        
        var template = new WorkoutTemplateBuilder()
            .WithId(templateId)
            .WithWorkoutState(WorkoutStateTestBuilder.Default().WithValue(draftState).Build())
            .Build();
            
        autoMocker.GetMock<IUnitOfWorkProvider<FitnessDbContext>>()
            .Setup(x => x.CreateReadOnly())
            .Returns(autoMocker.GetMock<IReadOnlyUnitOfWork<FitnessDbContext>>().Object);
            
        autoMocker.GetMock<IReadOnlyUnitOfWork<FitnessDbContext>>()
            .Setup(x => x.GetRepository<IWorkoutTemplateRepository>())
            .Returns(autoMocker.GetMock<IWorkoutTemplateRepository>().Object);
            
        autoMocker.GetMock<IWorkoutTemplateRepository>()
            .Setup(x => x.GetByIdAsync(templateId))
            .ReturnsAsync(template);

        var testee = autoMocker.CreateInstance<ValidationHandler>();

        // Act
        var result = await testee.IsTemplateInDraftStateAsync(templateId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsTemplateInDraftStateAsync_WhenTemplateIsNotInDraftState_ShouldReturnFalse()
    {
        // Arrange
        const string productionState = "Production";
        
        var autoMocker = new AutoMocker();
        var templateId = WorkoutTemplateId.New();
        
        var template = new WorkoutTemplateBuilder()
            .WithId(templateId)
            .WithWorkoutState(WorkoutStateTestBuilder.Default().WithValue(productionState).Build())
            .Build();
            
        autoMocker.GetMock<IUnitOfWorkProvider<FitnessDbContext>>()
            .Setup(x => x.CreateReadOnly())
            .Returns(autoMocker.GetMock<IReadOnlyUnitOfWork<FitnessDbContext>>().Object);
            
        autoMocker.GetMock<IReadOnlyUnitOfWork<FitnessDbContext>>()
            .Setup(x => x.GetRepository<IWorkoutTemplateRepository>())
            .Returns(autoMocker.GetMock<IWorkoutTemplateRepository>().Object);
            
        autoMocker.GetMock<IWorkoutTemplateRepository>()
            .Setup(x => x.GetByIdAsync(templateId))
            .ReturnsAsync(template);

        var testee = autoMocker.CreateInstance<ValidationHandler>();

        // Act
        var result = await testee.IsTemplateInDraftStateAsync(templateId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsTemplateInDraftStateAsync_WhenTemplateDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var templateId = WorkoutTemplateId.New();
        
        autoMocker.GetMock<IUnitOfWorkProvider<FitnessDbContext>>()
            .Setup(x => x.CreateReadOnly())
            .Returns(autoMocker.GetMock<IReadOnlyUnitOfWork<FitnessDbContext>>().Object);
            
        autoMocker.GetMock<IReadOnlyUnitOfWork<FitnessDbContext>>()
            .Setup(x => x.GetRepository<IWorkoutTemplateRepository>())
            .Returns(autoMocker.GetMock<IWorkoutTemplateRepository>().Object);
            
        autoMocker.GetMock<IWorkoutTemplateRepository>()
            .Setup(x => x.GetByIdAsync(templateId))
            .ReturnsAsync(GetFitterGetBigger.API.Models.Entities.WorkoutTemplate.Empty);

        var testee = autoMocker.CreateInstance<ValidationHandler>();

        // Act
        var result = await testee.IsTemplateInDraftStateAsync(templateId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region IsExerciseActiveAsync Tests

    [Fact]
    public async Task IsExerciseActiveAsync_WhenExerciseExists_ShouldReturnTrue()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var exerciseId = ExerciseId.New();
        
        var exercise = new ExerciseBuilder()
            .WithId(exerciseId)
            .Build();
            
        autoMocker.GetMock<IUnitOfWorkProvider<FitnessDbContext>>()
            .Setup(x => x.CreateReadOnly())
            .Returns(autoMocker.GetMock<IReadOnlyUnitOfWork<FitnessDbContext>>().Object);
            
        autoMocker.GetMock<IReadOnlyUnitOfWork<FitnessDbContext>>()
            .Setup(x => x.GetRepository<IExerciseRepository>())
            .Returns(autoMocker.GetMock<IExerciseRepository>().Object);
            
        autoMocker.GetMock<IExerciseRepository>()
            .Setup(x => x.GetByIdAsync(exerciseId))
            .ReturnsAsync(exercise);

        var testee = autoMocker.CreateInstance<ValidationHandler>();

        // Act
        var result = await testee.IsExerciseActiveAsync(exerciseId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsExerciseActiveAsync_WhenExerciseDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var exerciseId = ExerciseId.New();
        
        autoMocker.GetMock<IUnitOfWorkProvider<FitnessDbContext>>()
            .Setup(x => x.CreateReadOnly())
            .Returns(autoMocker.GetMock<IReadOnlyUnitOfWork<FitnessDbContext>>().Object);
            
        autoMocker.GetMock<IReadOnlyUnitOfWork<FitnessDbContext>>()
            .Setup(x => x.GetRepository<IExerciseRepository>())
            .Returns(autoMocker.GetMock<IExerciseRepository>().Object);
            
        autoMocker.GetMock<IExerciseRepository>()
            .Setup(x => x.GetByIdAsync(exerciseId))
            .ReturnsAsync(GetFitterGetBigger.API.Models.Entities.Exercise.Empty);

        var testee = autoMocker.CreateInstance<ValidationHandler>();

        // Act
        var result = await testee.IsExerciseActiveAsync(exerciseId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region DoesTemplateExistAsync Tests

    [Fact]
    public async Task DoesTemplateExistAsync_WhenTemplateExists_ShouldReturnTrue()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var templateId = WorkoutTemplateId.New();
        
        var template = new WorkoutTemplateBuilder()
            .WithId(templateId)
            .Build();
            
        autoMocker.GetMock<IUnitOfWorkProvider<FitnessDbContext>>()
            .Setup(x => x.CreateReadOnly())
            .Returns(autoMocker.GetMock<IReadOnlyUnitOfWork<FitnessDbContext>>().Object);
            
        autoMocker.GetMock<IReadOnlyUnitOfWork<FitnessDbContext>>()
            .Setup(x => x.GetRepository<IWorkoutTemplateRepository>())
            .Returns(autoMocker.GetMock<IWorkoutTemplateRepository>().Object);
            
        autoMocker.GetMock<IWorkoutTemplateRepository>()
            .Setup(x => x.GetByIdAsync(templateId))
            .ReturnsAsync(template);

        var testee = autoMocker.CreateInstance<ValidationHandler>();

        // Act
        var result = await testee.DoesTemplateExistAsync(templateId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DoesTemplateExistAsync_WhenTemplateDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var templateId = WorkoutTemplateId.New();
        
        autoMocker.GetMock<IUnitOfWorkProvider<FitnessDbContext>>()
            .Setup(x => x.CreateReadOnly())
            .Returns(autoMocker.GetMock<IReadOnlyUnitOfWork<FitnessDbContext>>().Object);
            
        autoMocker.GetMock<IReadOnlyUnitOfWork<FitnessDbContext>>()
            .Setup(x => x.GetRepository<IWorkoutTemplateRepository>())
            .Returns(autoMocker.GetMock<IWorkoutTemplateRepository>().Object);
            
        autoMocker.GetMock<IWorkoutTemplateRepository>()
            .Setup(x => x.GetByIdAsync(templateId))
            .ReturnsAsync(GetFitterGetBigger.API.Models.Entities.WorkoutTemplate.Empty);

        var testee = autoMocker.CreateInstance<ValidationHandler>();

        // Act
        var result = await testee.DoesTemplateExistAsync(templateId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region DoesExerciseExistInTemplateAsync Tests

    [Fact]
    public async Task DoesExerciseExistInTemplateAsync_WhenExerciseExistsInTemplate_ShouldReturnTrue()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var exerciseId = WorkoutTemplateExerciseId.New();
        var templateId = WorkoutTemplateId.New();
        
        var exercise = new WorkoutTemplateExerciseBuilder()
            .WithId(exerciseId)
            .WithWorkoutTemplateId(templateId)
            .Build();
            
        autoMocker.GetMock<IUnitOfWorkProvider<FitnessDbContext>>()
            .Setup(x => x.CreateReadOnly())
            .Returns(autoMocker.GetMock<IReadOnlyUnitOfWork<FitnessDbContext>>().Object);
            
        autoMocker.GetMock<IReadOnlyUnitOfWork<FitnessDbContext>>()
            .Setup(x => x.GetRepository<IWorkoutTemplateExerciseRepository>())
            .Returns(autoMocker.GetMock<IWorkoutTemplateExerciseRepository>().Object);
            
        autoMocker.GetMock<IWorkoutTemplateExerciseRepository>()
            .Setup(x => x.GetByIdWithDetailsAsync(exerciseId))
            .ReturnsAsync(exercise);

        var testee = autoMocker.CreateInstance<ValidationHandler>();

        // Act
        var result = await testee.DoesExerciseExistInTemplateAsync(exerciseId, templateId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DoesExerciseExistInTemplateAsync_WhenExerciseDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var exerciseId = WorkoutTemplateExerciseId.New();
        var templateId = WorkoutTemplateId.New();
        
        autoMocker.GetMock<IUnitOfWorkProvider<FitnessDbContext>>()
            .Setup(x => x.CreateReadOnly())
            .Returns(autoMocker.GetMock<IReadOnlyUnitOfWork<FitnessDbContext>>().Object);
            
        autoMocker.GetMock<IReadOnlyUnitOfWork<FitnessDbContext>>()
            .Setup(x => x.GetRepository<IWorkoutTemplateExerciseRepository>())
            .Returns(autoMocker.GetMock<IWorkoutTemplateExerciseRepository>().Object);
            
        autoMocker.GetMock<IWorkoutTemplateExerciseRepository>()
            .Setup(x => x.GetByIdWithDetailsAsync(exerciseId))
            .ReturnsAsync(WorkoutTemplateExercise.Empty);

        var testee = autoMocker.CreateInstance<ValidationHandler>();

        // Act
        var result = await testee.DoesExerciseExistInTemplateAsync(exerciseId, templateId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DoesExerciseExistInTemplateAsync_WhenExerciseBelongsToOtherTemplate_ShouldReturnFalse()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var exerciseId = WorkoutTemplateExerciseId.New();
        var templateId = WorkoutTemplateId.New();
        var otherTemplateId = WorkoutTemplateId.New();
        
        var exercise = new WorkoutTemplateExerciseBuilder()
            .WithId(exerciseId)
            .WithWorkoutTemplateId(otherTemplateId)
            .Build();
            
        autoMocker.GetMock<IUnitOfWorkProvider<FitnessDbContext>>()
            .Setup(x => x.CreateReadOnly())
            .Returns(autoMocker.GetMock<IReadOnlyUnitOfWork<FitnessDbContext>>().Object);
            
        autoMocker.GetMock<IReadOnlyUnitOfWork<FitnessDbContext>>()
            .Setup(x => x.GetRepository<IWorkoutTemplateExerciseRepository>())
            .Returns(autoMocker.GetMock<IWorkoutTemplateExerciseRepository>().Object);
            
        autoMocker.GetMock<IWorkoutTemplateExerciseRepository>()
            .Setup(x => x.GetByIdWithDetailsAsync(exerciseId))
            .ReturnsAsync(exercise);

        var testee = autoMocker.CreateInstance<ValidationHandler>();

        // Act
        var result = await testee.DoesExerciseExistInTemplateAsync(exerciseId, templateId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region DoesRoundExistAsync Tests

    [Fact]
    public async Task DoesRoundExistAsync_WhenRoundExists_ShouldReturnTrue()
    {
        // Arrange
        const string warmupPhase = "Warmup";
        const int roundNumber = 1;
        
        var autoMocker = new AutoMocker();
        var templateId = WorkoutTemplateId.New();
        
        var exercises = new List<WorkoutTemplateExercise>
        {
            new WorkoutTemplateExerciseBuilder()
                .WithWorkoutTemplateId(templateId)
                .WithPhase(warmupPhase)
                .WithRoundNumber(roundNumber)
                .Build()
        };
        
        autoMocker.GetMock<IUnitOfWorkProvider<FitnessDbContext>>()
            .Setup(x => x.CreateReadOnly())
            .Returns(autoMocker.GetMock<IReadOnlyUnitOfWork<FitnessDbContext>>().Object);
            
        autoMocker.GetMock<IReadOnlyUnitOfWork<FitnessDbContext>>()
            .Setup(x => x.GetRepository<IWorkoutTemplateExerciseRepository>())
            .Returns(autoMocker.GetMock<IWorkoutTemplateExerciseRepository>().Object);
            
        autoMocker.GetMock<IWorkoutTemplateExerciseRepository>()
            .Setup(x => x.GetByWorkoutTemplateAsync(templateId))
            .ReturnsAsync(exercises);

        var testee = autoMocker.CreateInstance<ValidationHandler>();

        // Act
        var result = await testee.DoesRoundExistAsync(templateId, warmupPhase, roundNumber);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DoesRoundExistAsync_WhenRoundDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        const string warmupPhase = "Warmup";
        const int roundNumber = 1;
        
        var autoMocker = new AutoMocker();
        var templateId = WorkoutTemplateId.New();
        
        autoMocker.GetMock<IUnitOfWorkProvider<FitnessDbContext>>()
            .Setup(x => x.CreateReadOnly())
            .Returns(autoMocker.GetMock<IReadOnlyUnitOfWork<FitnessDbContext>>().Object);
            
        autoMocker.GetMock<IReadOnlyUnitOfWork<FitnessDbContext>>()
            .Setup(x => x.GetRepository<IWorkoutTemplateExerciseRepository>())
            .Returns(autoMocker.GetMock<IWorkoutTemplateExerciseRepository>().Object);
            
        autoMocker.GetMock<IWorkoutTemplateExerciseRepository>()
            .Setup(x => x.GetByWorkoutTemplateAsync(templateId))
            .ReturnsAsync(new List<WorkoutTemplateExercise>());

        var testee = autoMocker.CreateInstance<ValidationHandler>();

        // Act
        var result = await testee.DoesRoundExistAsync(templateId, warmupPhase, roundNumber);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}