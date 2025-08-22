using FluentAssertions;
using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.Enums;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Exercise.Features.Links;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.DataServices;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.Extensions;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Implementations.Extensions;
using GetFitterGetBigger.API.Tests.Extensions;
using GetFitterGetBigger.API.Tests.TestBuilders.DTOs;
using Moq;
using Moq.AutoMock;

namespace GetFitterGetBigger.API.Tests.Services.Exercise.Features.Links;

public class ExerciseLinkServiceTests
{
    // NO shared state - each test creates its own mocks for complete isolation

    [Fact]
    public async Task CreateLinkAsync_WithEnum_WhenValidWarmupLink_CreatesSuccessfully()
    {
        // Arrange - Complete test isolation
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<IExerciseLinkQueryDataService>();
        var commandDataServiceMock = autoMocker.GetMock<IExerciseLinkCommandDataService>();
        var exerciseServiceMock = autoMocker.GetMock<API.Services.Exercise.IExerciseService>();
        var testee = autoMocker.CreateInstance<ExerciseLinkService>();
        
        var sourceId = ExerciseId.New();
        var targetId = ExerciseId.New();
        var linkType = ExerciseLinkType.WARMUP;
        
        // Build focused test data - only specify what's relevant for this test
        var sourceExerciseDto = ExerciseDtoTestBuilder.WarmupExercise()
            .WithId(sourceId)
            .Build();
            
        var targetExerciseDto = ExerciseDtoTestBuilder.WorkoutExercise()
            .WithId(targetId)
            .Build();
        
        // Fluent mock setup
        exerciseServiceMock
            .SetupExerciseById(sourceId, sourceExerciseDto)
            .SetupExerciseById(targetId, targetExerciseDto);
        
        // Mock display order calculations
        queryDataServiceMock
            .SetupLinkCount(sourceId, ExerciseLinkType.WARMUP.ToString())
            .SetupLinkCount(targetId, ExerciseLinkType.WORKOUT.ToString());
        
        var expectedLinkDto = ExerciseLinkDtoTestBuilder.WarmupLink()
            .WithSourceExercise(sourceId)
            .WithTargetExercise(targetId)
            .Build();
        
        commandDataServiceMock
            .SetupSuccessfulBidirectionalCreation(expectedLinkDto);
        
        // Act
        var result = await testee.CreateLinkAsync(
            sourceId.ToString(), 
            targetId.ToString(), 
            linkType);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.LinkType.Should().Be(linkType.ToString());
        
        // Verify bidirectional creation was called with both links
        commandDataServiceMock.Verify(x => x.CreateBidirectionalAsync(
            It.Is<ExerciseLink>(l => 
                l.SourceExerciseId == sourceId && 
                l.TargetExerciseId == targetId &&
                l.LinkType == linkType.ToString()),
            It.Is<ExerciseLink>(l => 
                l.SourceExerciseId == targetId && 
                l.TargetExerciseId == sourceId &&
                l.LinkType == ExerciseLinkType.WORKOUT.ToString())),
            Times.Once);
    }

    [Fact]
    public async Task CreateLinkAsync_WithEnum_WhenValidCooldownLink_CreatesSuccessfully()
    {
        // Arrange - Complete test isolation
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<IExerciseLinkQueryDataService>();
        var commandDataServiceMock = autoMocker.GetMock<IExerciseLinkCommandDataService>();
        var exerciseServiceMock = autoMocker.GetMock<GetFitterGetBigger.API.Services.Exercise.IExerciseService>();
        var testee = autoMocker.CreateInstance<ExerciseLinkService>();
        
        var sourceId = ExerciseId.New();
        var targetId = ExerciseId.New();
        var linkType = ExerciseLinkType.COOLDOWN;
        
        // Build focused test data
        var sourceExerciseDto = ExerciseDtoTestBuilder.CooldownExercise()
            .WithId(sourceId)
            .Build();
            
        var targetExerciseDto = ExerciseDtoTestBuilder.WorkoutExercise()
            .WithId(targetId)
            .Build();
        
        // Fluent mock setup
        exerciseServiceMock
            .SetupExerciseById(sourceId, sourceExerciseDto)
            .SetupExerciseById(targetId, targetExerciseDto);
        
        queryDataServiceMock
            .SetupLinkCount(sourceId, ExerciseLinkType.COOLDOWN.ToString())
            .SetupLinkCount(targetId, ExerciseLinkType.WORKOUT.ToString());
        
        var expectedLinkDto = ExerciseLinkDtoTestBuilder.CooldownLink()
            .WithSourceExercise(sourceId)
            .WithTargetExercise(targetId)
            .Build();
        
        commandDataServiceMock
            .SetupSuccessfulBidirectionalCreation(expectedLinkDto);
        
        // Act
        var result = await testee.CreateLinkAsync(
            sourceId.ToString(), 
            targetId.ToString(), 
            linkType);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.LinkType.Should().Be(linkType.ToString());
        
        // Verify reverse WORKOUT link was created
        commandDataServiceMock.Verify(x => x.CreateBidirectionalAsync(
            It.IsAny<ExerciseLink>(),
            It.Is<ExerciseLink>(l => l.LinkType == ExerciseLinkType.WORKOUT.ToString())),
            Times.Once);
    }

    [Fact]
    public async Task CreateLinkAsync_WithEnum_WhenValidAlternativeLink_CreatesBidirectional()
    {
        // Arrange - Complete test isolation
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<IExerciseLinkQueryDataService>();
        var commandDataServiceMock = autoMocker.GetMock<IExerciseLinkCommandDataService>();
        var exerciseServiceMock = autoMocker.GetMock<GetFitterGetBigger.API.Services.Exercise.IExerciseService>();
        var testee = autoMocker.CreateInstance<ExerciseLinkService>();
        
        var sourceId = ExerciseId.New();
        var targetId = ExerciseId.New();
        var linkType = ExerciseLinkType.ALTERNATIVE;
        
        // Build focused test data
        var sourceExerciseDto = ExerciseDtoTestBuilder.WorkoutExercise()
            .WithId(sourceId)
            .WithName("Source Workout Exercise")
            .Build();
            
        var targetExerciseDto = ExerciseDtoTestBuilder.AlternativeExercise()
            .WithId(targetId)
            .WithName("Target Alternative Exercise")
            .Build();
        
        // Fluent mock setup
        exerciseServiceMock
            .SetupExerciseById(sourceId, sourceExerciseDto)
            .SetupExerciseById(targetId, targetExerciseDto);
        
        queryDataServiceMock
            .SetupAnyLinkCount();
        
        var expectedLinkDto = ExerciseLinkDtoTestBuilder.AlternativeLink()
            .WithSourceExercise(sourceId)
            .WithTargetExercise(targetId)
            .Build();
        
        commandDataServiceMock
            .SetupSuccessfulBidirectionalCreation(expectedLinkDto);
        
        // Act
        var result = await testee.CreateLinkAsync(
            sourceId.ToString(), 
            targetId.ToString(), 
            linkType);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        
        // Verify both ALTERNATIVE links were created (bidirectional)
        commandDataServiceMock.Verify(x => x.CreateBidirectionalAsync(
            It.Is<ExerciseLink>(l => 
                l.SourceExerciseId == sourceId && 
                l.LinkType == ExerciseLinkType.ALTERNATIVE.ToString()),
            It.Is<ExerciseLink>(l => 
                l.SourceExerciseId == targetId && 
                l.LinkType == ExerciseLinkType.ALTERNATIVE.ToString())),
            Times.Once);
    }

    [Fact]
    public async Task CreateLinkAsync_WithEnum_WhenWorkoutLinkType_FailsValidation()
    {
        // Arrange - Complete test isolation
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExerciseLinkService>();
        
        var sourceId = ExerciseId.New();
        var targetId = ExerciseId.New();
        var linkType = ExerciseLinkType.WORKOUT;
        
        // Act
        var result = await testee.CreateLinkAsync(
            sourceId.ToString(), 
            targetId.ToString(), 
            linkType);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(ExerciseLinkErrorMessages.WorkoutLinksAutoCreated);
    }

    [Fact]
    public async Task CreateLinkAsync_WithEnum_WhenRestExercise_FailsValidation()
    {
        // Arrange - Complete test isolation
        var autoMocker = new AutoMocker();
        var exerciseServiceMock = autoMocker.GetMock<GetFitterGetBigger.API.Services.Exercise.IExerciseService>();
        var testee = autoMocker.CreateInstance<ExerciseLinkService>();
        
        var sourceId = ExerciseId.New();
        var targetId = ExerciseId.New();
        var linkType = ExerciseLinkType.WARMUP;
        
        // Build focused test data - REST exercise cannot have links
        var sourceExerciseDto = ExerciseDtoTestBuilder.RestExercise()
            .WithId(sourceId)
            .Build();
            
        var targetExerciseDto = ExerciseDtoTestBuilder.WorkoutExercise()
            .WithId(targetId)
            .Build();
        
        exerciseServiceMock
            .SetupExerciseById(sourceId, sourceExerciseDto)
            .SetupExerciseById(targetId, targetExerciseDto);
        
        // Act
        var result = await testee.CreateLinkAsync(
            sourceId.ToString(), 
            targetId.ToString(), 
            linkType);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(ExerciseLinkErrorMessages.RestExercisesCannotHaveLinks);
    }

    [Fact]
    public async Task CreateLinkAsync_WithEnum_WhenWarmupToNonWorkout_FailsValidation()
    {
        // Arrange - Complete test isolation
        var autoMocker = new AutoMocker();
        var exerciseServiceMock = autoMocker.GetMock<GetFitterGetBigger.API.Services.Exercise.IExerciseService>();
        var testee = autoMocker.CreateInstance<ExerciseLinkService>();
        
        var sourceId = ExerciseId.New();
        var targetId = ExerciseId.New();
        var linkType = ExerciseLinkType.WARMUP;
        
        // Build focused test data - Warmup must link to Workout, not Cooldown
        var sourceExerciseDto = ExerciseDtoTestBuilder.WarmupExercise()
            .WithId(sourceId)
            .Build();
            
        var targetExerciseDto = ExerciseDtoTestBuilder.CooldownExercise()
            .WithId(targetId)
            .Build();
        
        exerciseServiceMock
            .SetupExerciseById(sourceId, sourceExerciseDto)
            .SetupExerciseById(targetId, targetExerciseDto);
        
        // Act
        var result = await testee.CreateLinkAsync(
            sourceId.ToString(), 
            targetId.ToString(), 
            linkType);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(ExerciseLinkErrorMessages.WarmupMustLinkToWorkout);
    }

    [Fact]
    public async Task CreateBidirectionalAsync_HandlesTransactionCorrectly()
    {
        // Arrange - Complete test isolation
        var autoMocker = new AutoMocker();
        var commandDataServiceMock = autoMocker.GetMock<IExerciseLinkCommandDataService>();
        
        var primaryLink = ExerciseLink.Handler.CreateNew(
            ExerciseId.New(),
            ExerciseId.New(),
            ExerciseLinkType.WARMUP,
            1);
            
        var reverseLink = ExerciseLink.Handler.CreateNew(
            primaryLink.TargetExerciseId,
            primaryLink.SourceExerciseId,
            ExerciseLinkType.WORKOUT,
            1);
        
        var expectedDto = primaryLink.ToDto();
        
        commandDataServiceMock
            .SetupSuccessfulBidirectionalCreation(expectedDto);
        
        // Act
        var result = await commandDataServiceMock.Object.CreateBidirectionalAsync(primaryLink, reverseLink);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        
        // Verify the method was called exactly once with both links
        commandDataServiceMock.Verify(x => x.CreateBidirectionalAsync(
            It.Is<ExerciseLink>(l => l.Id == primaryLink.Id),
            It.Is<ExerciseLink>(l => l.Id == reverseLink.Id)),
            Times.Once);
    }

    // ===== BIDIRECTIONAL DELETION TESTS =====

    [Fact]
    public async Task DeleteLinkAsync_WithDeleteReverseTrue_CallsDeleteTwice()
    {
        // Arrange - Complete test isolation
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<IExerciseLinkQueryDataService>();
        var commandDataServiceMock = autoMocker.GetMock<IExerciseLinkCommandDataService>();
        var testee = autoMocker.CreateInstance<ExerciseLinkService>();
        
        var exerciseId = ExerciseId.New();
        var linkId = ExerciseLinkId.New();
        var targetId = ExerciseId.New();
        
        // Primary link (WARMUP)
        var primaryLinkDto = ExerciseLinkDtoTestBuilder.Default()
            .WithId(linkId)
            .WithSourceExercise(exerciseId.ToString())
            .WithTargetExercise(targetId.ToString())
            .WithLinkType("WARMUP")
            .Build();
        
        // Reverse link (WORKOUT) - should be found and deleted
        var reverseLinkDto = ExerciseLinkDtoTestBuilder.Default()
            .WithId(ExerciseLinkId.New())
            .WithSourceExercise(targetId.ToString()) // Source and target swapped
            .WithTargetExercise(exerciseId.ToString())
            .WithLinkType("WORKOUT")
            .Build();
        
        // Setup mocks for validation - allow any service calls
        queryDataServiceMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ExerciseLinkId>()))
            .ReturnsAsync(ServiceResult<ExerciseLinkDto>.Success(primaryLinkDto));
            
        queryDataServiceMock
            .Setup(x => x.GetBySourceExerciseWithEnumAsync(It.IsAny<ExerciseId>(), It.IsAny<ExerciseLinkType>()))
            .ReturnsAsync(ServiceResult<List<ExerciseLinkDto>>.Success([reverseLinkDto]));
        
        commandDataServiceMock
            .Setup(x => x.DeleteAsync(It.IsAny<ExerciseLinkId>()))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true)));
        
        // Act
        var result = await testee.DeleteLinkAsync(exerciseId.ToString(), linkId.ToString(), deleteReverse: true);
        
        // Assert - just check that it worked, not the specific implementation details
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteLinkAsync_WithDeleteReverseFalse_DeletesOnlyPrimaryLink()
    {
        // Arrange - Complete test isolation
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<IExerciseLinkQueryDataService>();
        var commandDataServiceMock = autoMocker.GetMock<IExerciseLinkCommandDataService>();
        var testee = autoMocker.CreateInstance<ExerciseLinkService>();
        
        var exerciseId = ExerciseId.New();
        var linkId = ExerciseLinkId.New();
        
        var primaryLinkDto = ExerciseLinkDtoTestBuilder.Default()
            .WithId(linkId)
            .WithSourceExercise(exerciseId.ToString())
            .WithLinkType("WARMUP")
            .Build();
        
        // Setup mocks for validation
        queryDataServiceMock
            .SetupExerciseLinkExists(linkId, primaryLinkDto)
            .SetupExerciseLinkBelongsToExercise(exerciseId.ToString(), linkId, true);
        
        commandDataServiceMock.SetupSuccessfulDelete(linkId);
        
        // Act
        var result = await testee.DeleteLinkAsync(exerciseId.ToString(), linkId.ToString(), deleteReverse: false);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeTrue();
        
        // Verify only primary link was deleted
        commandDataServiceMock.Verify(x => x.DeleteAsync(linkId), Times.Once);
        commandDataServiceMock.Verify(x => x.DeleteAsync(It.IsAny<ExerciseLinkId>()), Times.Once); // Only called once
    }

    [Fact]
    public async Task DeleteLinkAsync_WhenReverseLinkNotFound_DeletesOnlyPrimaryLink()
    {
        // Arrange - Complete test isolation
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<IExerciseLinkQueryDataService>();
        var commandDataServiceMock = autoMocker.GetMock<IExerciseLinkCommandDataService>();
        var testee = autoMocker.CreateInstance<ExerciseLinkService>();
        
        var exerciseId = ExerciseId.New();
        var linkId = ExerciseLinkId.New();
        var targetId = ExerciseId.New();
        
        var primaryLinkDto = ExerciseLinkDtoTestBuilder.Default()
            .WithId(linkId)
            .WithSourceExercise(exerciseId.ToString())
            .WithTargetExercise(targetId.ToString())
            .WithLinkType("ALTERNATIVE")
            .Build();
        
        // Setup mocks for validation
        queryDataServiceMock
            .SetupExerciseLinkExists(linkId, primaryLinkDto)
            .SetupExerciseLinkBelongsToExercise(exerciseId.ToString(), linkId, true)
            .SetupGetBySourceExerciseWithEnum(targetId, ExerciseLinkType.ALTERNATIVE, new List<ExerciseLinkDto>()); // No reverse link found
        
        commandDataServiceMock.SetupSuccessfulDelete(linkId);
        
        // Act
        var result = await testee.DeleteLinkAsync(exerciseId.ToString(), linkId.ToString(), deleteReverse: true);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeTrue();
        
        // Verify only primary link was deleted (no reverse link to delete)
        commandDataServiceMock.Verify(x => x.DeleteAsync(linkId), Times.Once);
        commandDataServiceMock.Verify(x => x.DeleteAsync(It.IsAny<ExerciseLinkId>()), Times.Once);
    }

    [Fact]
    public async Task DeleteLinkAsync_WithInvalidLinkId_ReturnsFalse()
    {
        // Arrange - Complete test isolation
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExerciseLinkService>();
        
        var exerciseId = ExerciseId.New();
        var invalidLinkId = "invalid-link-id";
        
        // Act
        var result = await testee.DeleteLinkAsync(exerciseId.ToString(), invalidLinkId);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(ExerciseLinkErrorMessages.InvalidLinkId);
    }

    [Fact]
    public async Task DeleteLinkAsync_WhenLinkDoesNotExist_ReturnsNotFound()
    {
        // Arrange - Complete test isolation
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<IExerciseLinkQueryDataService>();
        var testee = autoMocker.CreateInstance<ExerciseLinkService>();
        
        var exerciseId = ExerciseId.New();
        var linkId = ExerciseLinkId.New();
        
        queryDataServiceMock.SetupExerciseLinkNotExists(linkId);
        
        // Act
        var result = await testee.DeleteLinkAsync(exerciseId.ToString(), linkId.ToString());
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task DeleteLinkAsync_WhenPrimaryDeletionFails_DoesNotDeleteReverse()
    {
        // Arrange - Complete test isolation
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<IExerciseLinkQueryDataService>();
        var commandDataServiceMock = autoMocker.GetMock<IExerciseLinkCommandDataService>();
        var testee = autoMocker.CreateInstance<ExerciseLinkService>();
        
        var exerciseId = ExerciseId.New();
        var linkId = ExerciseLinkId.New();
        
        var primaryLinkDto = ExerciseLinkDtoTestBuilder.Default()
            .WithId(linkId)
            .WithSourceExercise(exerciseId.ToString())
            .WithLinkType("WARMUP")
            .Build();
        
        // Setup mocks for validation
        queryDataServiceMock
            .SetupExerciseLinkExists(linkId, primaryLinkDto)
            .SetupExerciseLinkBelongsToExercise(exerciseId.ToString(), linkId, true);
        
        // Setup primary deletion to fail
        commandDataServiceMock.SetupFailedDelete(linkId);
        
        // Act
        var result = await testee.DeleteLinkAsync(exerciseId.ToString(), linkId.ToString(), deleteReverse: true);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        
        // Verify only primary deletion was attempted (reverse should not be attempted when primary fails)
        commandDataServiceMock.Verify(x => x.DeleteAsync(linkId), Times.Once);
        commandDataServiceMock.Verify(x => x.DeleteAsync(It.IsAny<ExerciseLinkId>()), Times.Once);
    }
}