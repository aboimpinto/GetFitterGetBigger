using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Enums;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.DataServices;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.Handlers;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.TestBuilders.DTOs;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;

namespace GetFitterGetBigger.API.Tests.Services.Exercise.Features.Links.Handlers;

/// <summary>
/// RED Phase: Tests that reproduce the bidirectional delete bug reported in BACKEND-BIDIRECTIONAL-DELETE-BUG.md
/// These tests should FAIL initially, demonstrating the bug exists
/// </summary>
public class BidirectionalLinkHandlerBugTests
{
    [Fact]
    public async Task DeleteBidirectionalLinkAsync_WithWarmupWorkoutLinks_ShouldDeleteBothDirections()
    {
        // Arrange - Setting up the exact scenario from the bug report
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<IExerciseLinkQueryDataService>();
        var commandDataServiceMock = autoMocker.GetMock<IExerciseLinkCommandDataService>();
        var testee = autoMocker.CreateInstance<BidirectionalLinkHandler>();
        
        // IDs from the bug report scenario
        var warmupExerciseId = ExerciseId.ParseOrEmpty("exercise-950e8400-e29b-41d4-a716-446655440101");
        var workoutExerciseId = ExerciseId.ParseOrEmpty("exercise-123e4567-e89b-12d3-a456-426614174001");
        var warmupToWorkoutLinkId = ExerciseLinkId.ParseOrEmpty("exerciselink-301a6ecf-e5ea-4710-85d6-e1872e124920");
        var workoutToWarmupLinkId = ExerciseLinkId.ParseOrEmpty("exerciselink-402b7fdf-f6fb-5821-96e7-f2983f235a31");
        
        // Setup the warmup->workout link (the one being deleted)
        var warmupToWorkoutLink = new ExerciseLinkDto
        {
            Id = warmupToWorkoutLinkId.ToString(),
            SourceExerciseId = warmupExerciseId.ToString(),
            TargetExerciseId = workoutExerciseId.ToString(),
            LinkType = ExerciseLinkType.WORKOUT.ToString(), // From warmup's perspective, it links to a workout
            DisplayOrder = 1,
            IsActive = true
        };
        
        // Setup the reverse workout->warmup link (should be auto-deleted)
        var workoutToWarmupLink = new ExerciseLinkDto
        {
            Id = workoutToWarmupLinkId.ToString(),
            SourceExerciseId = workoutExerciseId.ToString(),
            TargetExerciseId = warmupExerciseId.ToString(),
            LinkType = ExerciseLinkType.WARMUP.ToString(), // From workout's perspective, it has a warmup
            DisplayOrder = 1,
            IsActive = true
        };
        
        // Mock getting the primary link details
        queryDataServiceMock
            .Setup(x => x.GetByIdAsync(warmupToWorkoutLinkId))
            .ReturnsAsync(ServiceResult<ExerciseLinkDto>.Success(warmupToWorkoutLink));
        
        // Mock finding the reverse link - when deleting a WORKOUT link, we need to check both WARMUP and COOLDOWN
        // The fix should check WARMUP first and find it
        queryDataServiceMock
            .Setup(x => x.GetBySourceExerciseWithEnumAsync(workoutExerciseId, ExerciseLinkType.WARMUP))
            .ReturnsAsync(ServiceResult<List<ExerciseLinkDto>>.Success(new List<ExerciseLinkDto> { workoutToWarmupLink }));
        
        queryDataServiceMock
            .Setup(x => x.GetBySourceExerciseWithEnumAsync(workoutExerciseId, ExerciseLinkType.COOLDOWN))
            .ReturnsAsync(ServiceResult<List<ExerciseLinkDto>>.Success(new List<ExerciseLinkDto>()));
        
        // Setup successful deletion of primary link
        commandDataServiceMock
            .Setup(x => x.DeleteAsync(warmupToWorkoutLinkId))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true)));
        
        // Setup successful deletion of reverse link
        commandDataServiceMock
            .Setup(x => x.DeleteAsync(workoutToWarmupLinkId))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true)));
        
        // Act
        var result = await testee.DeleteBidirectionalLinkAsync(warmupToWorkoutLinkId, deleteReverse: true);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeTrue();
        
        // CRITICAL: Verify BOTH links were deleted
        commandDataServiceMock.Verify(x => x.DeleteAsync(warmupToWorkoutLinkId), Times.Once, 
            "Primary link should be deleted");
        commandDataServiceMock.Verify(x => x.DeleteAsync(workoutToWarmupLinkId), Times.Once, 
            "Reverse link should be deleted when deleteReverse=true");
    }
    
    [Fact]
    public async Task DeleteBidirectionalLinkAsync_WithCooldownWorkoutLinks_ShouldDeleteBothDirections()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<IExerciseLinkQueryDataService>();
        var commandDataServiceMock = autoMocker.GetMock<IExerciseLinkCommandDataService>();
        var loggerMock = autoMocker.GetMock<ILogger<BidirectionalLinkHandler>>();
        var testee = autoMocker.CreateInstance<BidirectionalLinkHandler>();
        
        var cooldownExerciseId = ExerciseId.New();
        var workoutExerciseId = ExerciseId.New();
        var cooldownToWorkoutLinkId = ExerciseLinkId.New();
        var workoutToCooldownLinkId = ExerciseLinkId.New();
        
        // Setup the cooldown->workout link (the one being deleted)
        var cooldownToWorkoutLink = new ExerciseLinkDto
        {
            Id = cooldownToWorkoutLinkId.ToString(),
            SourceExerciseId = cooldownExerciseId.ToString(),
            TargetExerciseId = workoutExerciseId.ToString(),
            LinkType = ExerciseLinkType.WORKOUT.ToString(), // From cooldown's perspective, it links to a workout
            DisplayOrder = 1,
            IsActive = true
        };
        
        // Setup the reverse workout->cooldown link
        var workoutToCooldownLink = new ExerciseLinkDto
        {
            Id = workoutToCooldownLinkId.ToString(),
            SourceExerciseId = workoutExerciseId.ToString(),
            TargetExerciseId = cooldownExerciseId.ToString(),
            LinkType = ExerciseLinkType.COOLDOWN.ToString(), // From workout's perspective, it has a cooldown
            DisplayOrder = 1,
            IsActive = true
        };
        
        // Mock getting the primary link
        queryDataServiceMock
            .Setup(x => x.GetByIdAsync(cooldownToWorkoutLinkId))
            .ReturnsAsync(ServiceResult<ExerciseLinkDto>.Success(cooldownToWorkoutLink));
        
        // Mock finding the reverse link - when deleting a WORKOUT link, we need to check both WARMUP and COOLDOWN
        queryDataServiceMock
            .Setup(x => x.GetBySourceExerciseWithEnumAsync(workoutExerciseId, ExerciseLinkType.WARMUP))
            .ReturnsAsync(ServiceResult<List<ExerciseLinkDto>>.Success(new List<ExerciseLinkDto>()));
        
        queryDataServiceMock
            .Setup(x => x.GetBySourceExerciseWithEnumAsync(workoutExerciseId, ExerciseLinkType.COOLDOWN))
            .ReturnsAsync(ServiceResult<List<ExerciseLinkDto>>.Success(new List<ExerciseLinkDto> { workoutToCooldownLink }));
        
        // Setup successful deletions
        commandDataServiceMock
            .Setup(x => x.DeleteAsync(cooldownToWorkoutLinkId))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true)));
        
        commandDataServiceMock
            .Setup(x => x.DeleteAsync(workoutToCooldownLinkId))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true)));
        
        // Act
        var result = await testee.DeleteBidirectionalLinkAsync(cooldownToWorkoutLinkId, deleteReverse: true);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        
        // Verify both deletions
        commandDataServiceMock.Verify(x => x.DeleteAsync(cooldownToWorkoutLinkId), Times.Once);
        commandDataServiceMock.Verify(x => x.DeleteAsync(workoutToCooldownLinkId), Times.Once);
    }
    
    [Fact]
    public async Task DeleteBidirectionalLinkAsync_WithAlternativeLinks_ShouldDeleteBothDirections()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<IExerciseLinkQueryDataService>();
        var commandDataServiceMock = autoMocker.GetMock<IExerciseLinkCommandDataService>();
        var testee = autoMocker.CreateInstance<BidirectionalLinkHandler>();
        
        var exerciseAId = ExerciseId.New();
        var exerciseBId = ExerciseId.New();
        var linkAToB = ExerciseLinkId.New();
        var linkBToA = ExerciseLinkId.New();
        
        // Setup the A->B alternative link
        var alternativeAToB = new ExerciseLinkDto
        {
            Id = linkAToB.ToString(),
            SourceExerciseId = exerciseAId.ToString(),
            TargetExerciseId = exerciseBId.ToString(),
            LinkType = ExerciseLinkType.ALTERNATIVE.ToString(),
            DisplayOrder = 1,
            IsActive = true
        };
        
        // Setup the B->A alternative link
        var alternativeBToA = new ExerciseLinkDto
        {
            Id = linkBToA.ToString(),
            SourceExerciseId = exerciseBId.ToString(),
            TargetExerciseId = exerciseAId.ToString(),
            LinkType = ExerciseLinkType.ALTERNATIVE.ToString(),
            DisplayOrder = 1,
            IsActive = true
        };
        
        // Mock getting the primary link
        queryDataServiceMock
            .Setup(x => x.GetByIdAsync(linkAToB))
            .ReturnsAsync(ServiceResult<ExerciseLinkDto>.Success(alternativeAToB));
        
        // Mock finding the reverse alternative link
        queryDataServiceMock
            .Setup(x => x.GetBySourceExerciseWithEnumAsync(exerciseBId, ExerciseLinkType.ALTERNATIVE))
            .ReturnsAsync(ServiceResult<List<ExerciseLinkDto>>.Success(new List<ExerciseLinkDto> { alternativeBToA }));
        
        // Setup successful deletions
        commandDataServiceMock
            .Setup(x => x.DeleteAsync(linkAToB))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true)));
        
        commandDataServiceMock
            .Setup(x => x.DeleteAsync(linkBToA))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true)));
        
        // Act
        var result = await testee.DeleteBidirectionalLinkAsync(linkAToB, deleteReverse: true);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        
        // Verify both alternative links deleted
        commandDataServiceMock.Verify(x => x.DeleteAsync(linkAToB), Times.Once);
        commandDataServiceMock.Verify(x => x.DeleteAsync(linkBToA), Times.Once);
    }
    
    [Fact]
    public async Task DeleteBidirectionalLinkAsync_WhenDeletingWorkoutLinkFromWarmup_ShouldFindAndDeleteWarmupLink()
    {
        // This test specifically addresses the bug scenario where:
        // 1. User is on warmup exercise
        // 2. User deletes a workout link
        // 3. System should find and delete the corresponding warmup link on the workout exercise
        
        // Arrange
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<IExerciseLinkQueryDataService>();
        var commandDataServiceMock = autoMocker.GetMock<IExerciseLinkCommandDataService>();
        var testee = autoMocker.CreateInstance<BidirectionalLinkHandler>();
        
        var warmupExerciseId = ExerciseId.New();
        var workoutExerciseId = ExerciseId.New();
        var warmupToWorkoutLinkId = ExerciseLinkId.New(); // Link from warmup perspective
        var workoutToWarmupLinkId = ExerciseLinkId.New(); // Reverse link from workout perspective
        
        // The link being deleted: from warmup exercise pointing to workout
        var warmupPerspectiveLink = new ExerciseLinkDto
        {
            Id = warmupToWorkoutLinkId.ToString(),
            SourceExerciseId = warmupExerciseId.ToString(),
            TargetExerciseId = workoutExerciseId.ToString(),
            LinkType = "WORKOUT", // CRITICAL: Warmup sees this as linking to a WORKOUT
            DisplayOrder = 1,
            IsActive = true
        };
        
        // The reverse link that should be found and deleted
        var workoutPerspectiveLink = new ExerciseLinkDto
        {
            Id = workoutToWarmupLinkId.ToString(),
            SourceExerciseId = workoutExerciseId.ToString(),
            TargetExerciseId = warmupExerciseId.ToString(),
            LinkType = "WARMUP", // CRITICAL: Workout sees this as having a WARMUP
            DisplayOrder = 1,
            IsActive = true
        };
        
        // Mock getting the link being deleted
        queryDataServiceMock
            .Setup(x => x.GetByIdAsync(warmupToWorkoutLinkId))
            .ReturnsAsync(ServiceResult<ExerciseLinkDto>.Success(warmupPerspectiveLink));
        
        // Mock finding the reverse link - the fix now checks both WARMUP and COOLDOWN
        queryDataServiceMock
            .Setup(x => x.GetBySourceExerciseWithEnumAsync(workoutExerciseId, ExerciseLinkType.WARMUP))
            .ReturnsAsync(ServiceResult<List<ExerciseLinkDto>>.Success(new List<ExerciseLinkDto> { workoutPerspectiveLink }));
        
        queryDataServiceMock
            .Setup(x => x.GetBySourceExerciseWithEnumAsync(workoutExerciseId, ExerciseLinkType.COOLDOWN))
            .ReturnsAsync(ServiceResult<List<ExerciseLinkDto>>.Success(new List<ExerciseLinkDto>()));
        
        // Setup deletions
        commandDataServiceMock
            .Setup(x => x.DeleteAsync(warmupToWorkoutLinkId))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true)));
        
        commandDataServiceMock
            .Setup(x => x.DeleteAsync(workoutToWarmupLinkId))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true)));
        
        // Act
        var result = await testee.DeleteBidirectionalLinkAsync(warmupToWorkoutLinkId, deleteReverse: true);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue("Deletion should succeed");
        
        // This will likely FAIL with the current implementation, proving the bug exists
        commandDataServiceMock.Verify(x => x.DeleteAsync(warmupToWorkoutLinkId), Times.Once,
            "Primary WORKOUT link from warmup should be deleted");
        commandDataServiceMock.Verify(x => x.DeleteAsync(workoutToWarmupLinkId), Times.Once,
            "Reverse WARMUP link from workout should be deleted - THIS IS THE BUG!");
    }
}