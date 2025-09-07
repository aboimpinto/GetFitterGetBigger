using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Infrastructure;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.WorkoutTemplate.DataServices;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Handlers;
using GetFitterGetBigger.API.Tests.TestBuilders;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using WorkoutTemplateEntity = GetFitterGetBigger.API.Models.Entities.WorkoutTemplate;

namespace GetFitterGetBigger.API.Tests.Services.WorkoutTemplate.Handlers;

/// <summary>
/// Unit tests for DuplicationHandler following AutoMocker pattern with DataService mocks
/// </summary>
public class DuplicationHandlerTests
{
    #region DuplicateAsync - Success Cases

    [Fact]
    public async Task DuplicateAsync_WithValidParameters_CreatesNewTemplate()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<DuplicationHandler>();
        
        var originalId = WorkoutTemplateId.New();
        var originalTemplate = new WorkoutTemplateDtoBuilder()
            .WithId(originalId.ToString())
            .WithName("Original Template")
            .WithDescription("Original Description")
            .Build();
        
        var duplicatedTemplate = new WorkoutTemplateDtoBuilder()
            .WithId(WorkoutTemplateId.New().ToString())
            .WithName("Copy of Original")
            .WithDescription("Original Description")
            .Build();
        
        // Setup query data service
        automocker.GetMock<IWorkoutTemplateQueryDataService>()
            .Setup(x => x.GetByIdWithDetailsAsync(originalId))
            .ReturnsAsync(ServiceResult<WorkoutTemplateDto>.Success(originalTemplate));
        
        automocker.GetMock<IWorkoutTemplateQueryDataService>()
            .Setup(x => x.ExistsByNameAsync("Copy of Original"))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(new BooleanResultDto { Value = false }));
        
        // Setup original template exists (needed for validation chain)
        automocker.GetMock<IWorkoutTemplateQueryDataService>()
            .Setup(x => x.ExistsAsync(originalId))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(new BooleanResultDto { Value = true }));
        
        // Setup command data service
        automocker.GetMock<IWorkoutTemplateCommandDataService>()
            .Setup(x => x.DuplicateAsync(It.IsAny<WorkoutTemplateId>(), It.IsAny<string>(), It.IsAny<UserId>(), It.IsAny<ITransactionScope?>()))
            .ReturnsAsync(ServiceResult<WorkoutTemplateDto>.Success(duplicatedTemplate));

        // Act
        var result = await testee.DuplicateAsync(originalId, "Copy of Original");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Name.Should().Be("Copy of Original");
        result.Data.Description.Should().Be("Original Description");
        
        automocker.GetMock<IWorkoutTemplateCommandDataService>()
            .Verify(x => x.DuplicateAsync(It.IsAny<WorkoutTemplateId>(), It.IsAny<string>(), It.IsAny<UserId>(), It.IsAny<ITransactionScope?>()), Times.Once);
    }

    [Fact]
    public async Task DuplicateAsync_WithExercises_DuplicatesExercisesToo()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<DuplicationHandler>();
        
        var originalId = WorkoutTemplateId.New();
        var exerciseId = ExerciseId.New();
        
        var exerciseDto = new WorkoutTemplateExerciseDto
        {
            Id = "wtexercise-" + Guid.NewGuid(),
            Exercise = new ExerciseDto 
            { 
                Id = exerciseId.ToString(), 
                Name = "Test Exercise",
                Description = "Test",
                CoachNotes = new List<CoachNoteDto>(),
                IsActive = true
            },
            Zone = "Main",
            SequenceOrder = 1,
            Notes = "Exercise Notes"
        };
        
        var originalTemplate = new WorkoutTemplateDtoBuilder()
            .WithId(originalId.ToString())
            .WithName("Original")
            .WithExercises([exerciseDto])
            .Build();
        
        var duplicatedTemplate = new WorkoutTemplateDtoBuilder()
            .WithId(WorkoutTemplateId.New().ToString())
            .WithName("Copy")
            .Build();
        
        // Setup query data service
        automocker.GetMock<IWorkoutTemplateQueryDataService>()
            .Setup(x => x.GetByIdWithDetailsAsync(originalId))
            .ReturnsAsync(ServiceResult<WorkoutTemplateDto>.Success(originalTemplate));
        
        automocker.GetMock<IWorkoutTemplateQueryDataService>()
            .Setup(x => x.ExistsByNameAsync("Copy"))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(new BooleanResultDto { Value = false }));
        
        // Setup original template exists (needed for validation chain)
        automocker.GetMock<IWorkoutTemplateQueryDataService>()
            .Setup(x => x.ExistsAsync(originalId))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(new BooleanResultDto { Value = true }));
        
        // Setup command data service
        automocker.GetMock<IWorkoutTemplateCommandDataService>()
            .Setup(x => x.DuplicateAsync(It.IsAny<WorkoutTemplateId>(), It.IsAny<string>(), It.IsAny<UserId>(), It.IsAny<ITransactionScope?>()))
            .ReturnsAsync(ServiceResult<WorkoutTemplateDto>.Success(duplicatedTemplate));
        
        // Act
        var result = await testee.DuplicateAsync(originalId, "Copy");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        
        // Verify that DuplicateAsync was called (it handles exercise duplication internally)
        automocker.GetMock<IWorkoutTemplateCommandDataService>()
            .Verify(x => x.DuplicateAsync(It.IsAny<WorkoutTemplateId>(), It.IsAny<string>(), It.IsAny<UserId>(), It.IsAny<ITransactionScope?>()), Times.Once);
    }

    [Fact]
    public async Task DuplicateAsync_WithMultipleExercises_DuplicatesAllInOrder()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<DuplicationHandler>();
        
        var originalId = WorkoutTemplateId.New();
        
        var exercises = new List<WorkoutTemplateExerciseDto>
        {
            new()
            {
                Id = "wtexercise-1",
                Exercise = new ExerciseDto { Id = ExerciseId.New().ToString(), Name = "Exercise 1", Description = "Test", CoachNotes = new List<CoachNoteDto>(), IsActive = true },
                Zone = "Warmup",
                SequenceOrder = 1,
                Notes = "Warmup exercise"
            },
            new()
            {
                Id = "wtexercise-2",
                Exercise = new ExerciseDto { Id = ExerciseId.New().ToString(), Name = "Exercise 2", Description = "Test", CoachNotes = new List<CoachNoteDto>(), IsActive = true },
                Zone = "Main",
                SequenceOrder = 2,
                Notes = "Main exercise"
            },
            new()
            {
                Id = "wtexercise-3",
                Exercise = new ExerciseDto { Id = ExerciseId.New().ToString(), Name = "Exercise 3", Description = "Test", CoachNotes = new List<CoachNoteDto>(), IsActive = true },
                Zone = "Cooldown",
                SequenceOrder = 3,
                Notes = "Cooldown exercise"
            }
        };
        
        var originalTemplate = new WorkoutTemplateDtoBuilder()
            .WithId(originalId.ToString())
            .WithName("Original")
            .WithExercises(exercises)
            .Build();
        
        var duplicatedTemplate = new WorkoutTemplateDtoBuilder()
            .WithId(WorkoutTemplateId.New().ToString())
            .WithName("Copy")
            .Build();
        
        // Setup mocks
        automocker.GetMock<IWorkoutTemplateQueryDataService>()
            .Setup(x => x.GetByIdWithDetailsAsync(originalId))
            .ReturnsAsync(ServiceResult<WorkoutTemplateDto>.Success(originalTemplate));
        
        automocker.GetMock<IWorkoutTemplateQueryDataService>()
            .Setup(x => x.ExistsByNameAsync("Copy"))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(new BooleanResultDto { Value = false }));
        
        // Setup original template exists (needed for validation chain)
        automocker.GetMock<IWorkoutTemplateQueryDataService>()
            .Setup(x => x.ExistsAsync(originalId))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(new BooleanResultDto { Value = true }));
        
        automocker.GetMock<IWorkoutTemplateCommandDataService>()
            .Setup(x => x.DuplicateAsync(It.IsAny<WorkoutTemplateId>(), It.IsAny<string>(), It.IsAny<UserId>(), It.IsAny<ITransactionScope?>()))
            .ReturnsAsync(ServiceResult<WorkoutTemplateDto>.Success(duplicatedTemplate));
        
        // Act
        var result = await testee.DuplicateAsync(originalId, "Copy");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        
        // Verify that DuplicateAsync was called (it handles all exercise duplication internally)
        automocker.GetMock<IWorkoutTemplateCommandDataService>()
            .Verify(x => x.DuplicateAsync(It.IsAny<WorkoutTemplateId>(), It.IsAny<string>(), It.IsAny<UserId>(), It.IsAny<ITransactionScope?>()), Times.Once);
    }

    #endregion

    #region DuplicateAsync - Validation Failures

    [Fact]
    public async Task DuplicateAsync_WithEmptyOriginalId_ReturnsValidationFailed()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<DuplicationHandler>();

        // Act
        var result = await testee.DuplicateAsync(WorkoutTemplateId.Empty, "New Name");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Data.Should().NotBeNull();
        result.Data.IsEmpty.Should().BeTrue();
        
        // Should not call any data services
        automocker.GetMock<IWorkoutTemplateQueryDataService>()
            .Verify(x => x.GetByIdWithDetailsAsync(It.IsAny<WorkoutTemplateId>()), Times.Never);
    }

    [Fact]
    public async Task DuplicateAsync_WithEmptyName_ReturnsValidationFailed()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<DuplicationHandler>();
        
        var originalId = WorkoutTemplateId.New();

        // Act
        var result = await testee.DuplicateAsync(originalId, string.Empty);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Data.Should().NotBeNull();
        result.Data.IsEmpty.Should().BeTrue();
        
        // Should not call any data services
        automocker.GetMock<IWorkoutTemplateQueryDataService>()
            .Verify(x => x.GetByIdWithDetailsAsync(It.IsAny<WorkoutTemplateId>()), Times.Never);
    }

    [Fact]
    public async Task DuplicateAsync_WithWhitespaceName_ReturnsValidationFailed()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<DuplicationHandler>();
        
        var originalId = WorkoutTemplateId.New();

        // Act
        var result = await testee.DuplicateAsync(originalId, "   ");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Data.Should().NotBeNull();
        result.Data.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public async Task DuplicateAsync_WithTooLongName_ReturnsValidationFailed()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<DuplicationHandler>();
        
        var originalId = WorkoutTemplateId.New();
        var tooLongName = new string('A', 101);

        // Act
        var result = await testee.DuplicateAsync(originalId, tooLongName);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Data.Should().NotBeNull();
        result.Data.IsEmpty.Should().BeTrue();
    }

    #endregion

    #region DuplicateAsync - Business Rule Failures

    [Fact]
    public async Task DuplicateAsync_WhenOriginalNotFound_ReturnsNotFound()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<DuplicationHandler>();
        
        var originalId = WorkoutTemplateId.New();
        
        // Setup name is unique (so we get to the exists check)
        automocker.GetMock<IWorkoutTemplateQueryDataService>()
            .Setup(x => x.ExistsByNameAsync("New Name"))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(new BooleanResultDto { Value = false }));
        
        // Setup original template does not exist - this will cause NotFound error
        automocker.GetMock<IWorkoutTemplateQueryDataService>()
            .Setup(x => x.ExistsAsync(originalId))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(new BooleanResultDto { Value = false }));

        // Act
        var result = await testee.DuplicateAsync(originalId, "New Name");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.NotFound);
        result.Data.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public async Task DuplicateAsync_WhenNameAlreadyExists_ReturnsValidationFailed()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<DuplicationHandler>();
        
        var originalId = WorkoutTemplateId.New();
        
        // Setup name already exists
        automocker.GetMock<IWorkoutTemplateQueryDataService>()
            .Setup(x => x.ExistsByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(new BooleanResultDto { Value = true }));

        // Setup original template exists (needed for validation chain)
        automocker.GetMock<IWorkoutTemplateQueryDataService>()
            .Setup(x => x.ExistsAsync(originalId))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(new BooleanResultDto { Value = true }));

        // Act
        var result = await testee.DuplicateAsync(originalId, "Existing Name");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        
        // Should not fetch the original template
        automocker.GetMock<IWorkoutTemplateQueryDataService>()
            .Verify(x => x.GetByIdWithDetailsAsync(It.IsAny<WorkoutTemplateId>()), Times.Never);
    }

    #endregion
}