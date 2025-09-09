using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.Enums;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.DataServices;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise.Handlers;
using GetFitterGetBigger.API.Tests.TestBuilders.Domain;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using Olimpo.EntityFramework.Persistency;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services.WorkoutTemplate.Features.Exercise.Handlers;

public class AutoLinkingHandlerTests
{
    // No shared state - each test creates its own AutoMocker instance

    #region AddAutoLinkedExercisesAsync Tests

    [Fact]
    public async Task AddAutoLinkedExercisesAsync_WhenNoLinkedExercises_ShouldReturnEmptyList()
    {
        // Arrange
        const string warmupLinkType = "WARMUP";
        const string cooldownLinkType = "COOLDOWN";
        
        var autoMocker = new AutoMocker();
        var templateId = WorkoutTemplateId.New();
        var workoutExerciseId = ExerciseId.New();
        
        // Setup empty results for warmup and cooldown links
        autoMocker.GetMock<IExerciseLinkQueryDataService>()
            .Setup(x => x.GetBySourceExerciseAsync(workoutExerciseId, warmupLinkType))
            .ReturnsAsync(ServiceResult<List<ExerciseLinkDto>>.Success([]));
            
        autoMocker.GetMock<IExerciseLinkQueryDataService>()
            .Setup(x => x.GetBySourceExerciseAsync(workoutExerciseId, cooldownLinkType))
            .ReturnsAsync(ServiceResult<List<ExerciseLinkDto>>.Success([]));

        var testee = autoMocker.CreateInstance<AutoLinkingHandler>();

        // Act
        var result = await testee.AddAutoLinkedExercisesAsync(
            autoMocker.GetMock<IWorkoutTemplateExerciseRepository>().Object,
            templateId,
            workoutExerciseId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddAutoLinkedExercisesAsync_WhenWarmupLinkedExercisesExist_ShouldAddNewExercises()
    {
        // Arrange
        const string warmupLinkType = "WARMUP";
        const string cooldownLinkType = "COOLDOWN";
        const int displayOrder = 1;
        
        var autoMocker = new AutoMocker();
        var templateId = WorkoutTemplateId.New();
        var workoutExerciseId = ExerciseId.New();
        var linkedExerciseId = ExerciseId.New();
        
        var warmupLinkDto = new ExerciseLinkDto
        {
            Id = ExerciseLinkId.New().ToString(),
            SourceExerciseId = workoutExerciseId.ToString(),
            TargetExerciseId = linkedExerciseId.ToString(),
            LinkType = warmupLinkType,
            DisplayOrder = displayOrder
        };
        
        // Setup existing exercises in template (none matching the linked exercise)
        autoMocker.GetMock<IWorkoutTemplateExerciseRepository>()
            .Setup(x => x.GetByWorkoutTemplateAsync(templateId))
            .ReturnsAsync([]);
            
        autoMocker.GetMock<IExerciseLinkQueryDataService>()
            .Setup(x => x.GetBySourceExerciseAsync(workoutExerciseId, warmupLinkType))
            .ReturnsAsync(ServiceResult<List<ExerciseLinkDto>>.Success([warmupLinkDto]));
            
        autoMocker.GetMock<IExerciseLinkQueryDataService>()
            .Setup(x => x.GetBySourceExerciseAsync(workoutExerciseId, cooldownLinkType))
            .ReturnsAsync(ServiceResult<List<ExerciseLinkDto>>.Success([]));

        var testee = autoMocker.CreateInstance<AutoLinkingHandler>();

        // Act
        var result = await testee.AddAutoLinkedExercisesAsync(
            autoMocker.GetMock<IWorkoutTemplateExerciseRepository>().Object,
            templateId,
            workoutExerciseId);

        // Assert
        result.Should().HaveCount(1);
        result.First().ExerciseId.Should().Be(linkedExerciseId);
        result.First().Zone.Should().Be(WorkoutZone.Warmup);
        autoMocker.GetMock<IWorkoutTemplateExerciseRepository>().Verify(x => x.AddAsync(It.IsAny<WorkoutTemplateExercise>()), Times.Once);
    }

    [Fact]
    public async Task AddAutoLinkedExercisesAsync_WhenLinkedExerciseAlreadyExists_ShouldNotAddDuplicate()
    {
        // Arrange
        const string warmupLinkType = "WARMUP";
        const string cooldownLinkType = "COOLDOWN";
        const int displayOrder = 1;
        
        var autoMocker = new AutoMocker();
        var templateId = WorkoutTemplateId.New();
        var workoutExerciseId = ExerciseId.New();
        var linkedExerciseId = ExerciseId.New();
        
        var existingLinkedExercise = new WorkoutTemplateExerciseBuilder()
            .WithWorkoutTemplateId(templateId)
            .WithExerciseId(linkedExerciseId)
            .WithZone(WorkoutZone.Warmup)
            .Build();
        
        var warmupLinkDto = new ExerciseLinkDto
        {
            Id = ExerciseLinkId.New().ToString(),
            SourceExerciseId = workoutExerciseId.ToString(),
            TargetExerciseId = linkedExerciseId.ToString(),
            LinkType = warmupLinkType,
            DisplayOrder = displayOrder
        };
        
        // Setup existing exercises in template (including the linked exercise)
        autoMocker.GetMock<IWorkoutTemplateExerciseRepository>()
            .Setup(x => x.GetByWorkoutTemplateAsync(templateId))
            .ReturnsAsync([existingLinkedExercise]);
            
        autoMocker.GetMock<IExerciseLinkQueryDataService>()
            .Setup(x => x.GetBySourceExerciseAsync(workoutExerciseId, warmupLinkType))
            .ReturnsAsync(ServiceResult<List<ExerciseLinkDto>>.Success([warmupLinkDto]));
            
        autoMocker.GetMock<IExerciseLinkQueryDataService>()
            .Setup(x => x.GetBySourceExerciseAsync(workoutExerciseId, cooldownLinkType))
            .ReturnsAsync(ServiceResult<List<ExerciseLinkDto>>.Success([]));

        var testee = autoMocker.CreateInstance<AutoLinkingHandler>();

        // Act
        var result = await testee.AddAutoLinkedExercisesAsync(
            autoMocker.GetMock<IWorkoutTemplateExerciseRepository>().Object,
            templateId,
            workoutExerciseId);

        // Assert
        result.Should().BeEmpty();
        autoMocker.GetMock<IWorkoutTemplateExerciseRepository>().Verify(x => x.AddAsync(It.IsAny<WorkoutTemplateExercise>()), Times.Never);
    }

    #endregion

    #region FindOrphanedExercisesAsync Tests

    [Fact]
    public async Task FindOrphanedExercisesAsync_WhenNoOrphanedExercises_ShouldReturnEmptyList()
    {
        // Arrange
        const string warmupLinkType = "WARMUP";
        const string cooldownLinkType = "COOLDOWN";
        
        var autoMocker = new AutoMocker();
        var templateId = WorkoutTemplateId.New();
        var removedExerciseId = ExerciseId.New();
        
        // Setup empty results for warmup and cooldown links from removed exercise
        autoMocker.GetMock<IExerciseLinkQueryDataService>()
            .Setup(x => x.GetBySourceExerciseAsync(removedExerciseId, warmupLinkType))
            .ReturnsAsync(ServiceResult<List<ExerciseLinkDto>>.Success([]));
            
        autoMocker.GetMock<IExerciseLinkQueryDataService>()
            .Setup(x => x.GetBySourceExerciseAsync(removedExerciseId, cooldownLinkType))
            .ReturnsAsync(ServiceResult<List<ExerciseLinkDto>>.Success([]));

        var testee = autoMocker.CreateInstance<AutoLinkingHandler>();

        // Act
        var result = await testee.FindOrphanedExercisesAsync(
            autoMocker.GetMock<IWorkoutTemplateExerciseRepository>().Object,
            templateId,
            removedExerciseId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FindOrphanedExercisesAsync_WhenOrphanedExerciseExists_ShouldReturnOrphanedExercise()
    {
        // Arrange
        const string warmupLinkType = "WARMUP";
        const string cooldownLinkType = "COOLDOWN";
        const int displayOrder = 1;
        
        var autoMocker = new AutoMocker();
        var templateId = WorkoutTemplateId.New();
        var removedExerciseId = ExerciseId.New();
        var orphanedExerciseId = ExerciseId.New();
        var otherMainExerciseId = ExerciseId.New();
        
        var orphanedExercise = new WorkoutTemplateExerciseBuilder()
            .WithWorkoutTemplateId(templateId)
            .WithExerciseId(orphanedExerciseId)
            .WithZone(WorkoutZone.Warmup)
            .Build();
        
        var otherMainExercise = new WorkoutTemplateExerciseBuilder()
            .WithWorkoutTemplateId(templateId)
            .WithExerciseId(otherMainExerciseId)
            .WithZone(WorkoutZone.Main)
            .Build();
        
        // Setup all exercises in template
        autoMocker.GetMock<IWorkoutTemplateExerciseRepository>()
            .Setup(x => x.GetByWorkoutTemplateAsync(templateId))
            .ReturnsAsync([orphanedExercise, otherMainExercise]);
        
        // Setup warmup link from removed exercise to orphaned exercise
        var warmupLinkDto = new ExerciseLinkDto
        {
            Id = ExerciseLinkId.New().ToString(),
            SourceExerciseId = removedExerciseId.ToString(),
            TargetExerciseId = orphanedExerciseId.ToString(),
            LinkType = warmupLinkType,
            DisplayOrder = displayOrder
        };
        
        autoMocker.GetMock<IExerciseLinkQueryDataService>()
            .Setup(x => x.GetBySourceExerciseAsync(removedExerciseId, warmupLinkType))
            .ReturnsAsync(ServiceResult<List<ExerciseLinkDto>>.Success([warmupLinkDto]));
            
        autoMocker.GetMock<IExerciseLinkQueryDataService>()
            .Setup(x => x.GetBySourceExerciseAsync(removedExerciseId, cooldownLinkType))
            .ReturnsAsync(ServiceResult<List<ExerciseLinkDto>>.Success([]));
            
        // Other main exercise doesn't link to the orphaned exercise
        autoMocker.GetMock<IExerciseLinkQueryDataService>()
            .Setup(x => x.GetBySourceExerciseAsync(otherMainExerciseId, warmupLinkType))
            .ReturnsAsync(ServiceResult<List<ExerciseLinkDto>>.Success([]));

        var testee = autoMocker.CreateInstance<AutoLinkingHandler>();

        // Act
        var result = await testee.FindOrphanedExercisesAsync(
            autoMocker.GetMock<IWorkoutTemplateExerciseRepository>().Object,
            templateId,
            removedExerciseId);

        // Assert
        result.Should().HaveCount(1);
        result.First().Should().Be(orphanedExercise);
    }

    #endregion

    // Note: CleanupAfterRemovalAsync method does not exist in the AutoLinkingHandler
    // The cleanup functionality is handled directly in the service layer
    // by calling FindOrphanedExercisesAsync and then deleting the returned exercises
}