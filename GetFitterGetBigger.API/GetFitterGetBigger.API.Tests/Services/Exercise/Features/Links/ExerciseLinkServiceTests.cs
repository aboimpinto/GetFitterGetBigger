using FluentAssertions;
using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Enums;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Exercise.Features.Links;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.Handlers;
using GetFitterGetBigger.API.Services.Results;
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
        var exerciseServiceMock = autoMocker.GetMock<API.Services.Exercise.IExerciseService>();
        var bidirectionalLinkHandlerMock = autoMocker.GetMock<IBidirectionalLinkHandler>();
        var linkValidationHandlerMock = autoMocker.GetMock<ILinkValidationHandler>();
        
        var testee = autoMocker.CreateInstance<ExerciseLinkService>();
        
        var sourceId = ExerciseId.New();
        var targetId = ExerciseId.New();
        var linkType = ExerciseLinkType.WARMUP;
        
        // Build focused test data - only specify what's relevant for this test
        // WARMUP links go FROM Workout TO Warmup
        var sourceExerciseDto = ExerciseDtoTestBuilder.WorkoutExercise()
            .WithId(sourceId)
            .Build();
            
        var targetExerciseDto = ExerciseDtoTestBuilder.WarmupExercise()
            .WithId(targetId)
            .Build();
        
        // Fluent mock setup
        exerciseServiceMock
            .SetupExerciseById(sourceId, sourceExerciseDto)
            .SetupExerciseById(targetId, targetExerciseDto);
        
        // Mock validation handler for successful validation
        linkValidationHandlerMock
            .Setup(x => x.IsBidirectionalLinkUniqueAsync(sourceId, targetId, linkType))
            .ReturnsAsync(true);
        linkValidationHandlerMock
            .Setup(x => x.IsUnderMaximumLinksAsync(sourceId, linkType.ToString()))
            .ReturnsAsync(true);
        
        var expectedLinkDto = ExerciseLinkDtoTestBuilder.WarmupLink()
            .WithSourceExercise(sourceId)
            .WithTargetExercise(targetId)
            .Build();
        
        // Mock the bidirectional handler with specific values
        bidirectionalLinkHandlerMock.Setup(x => x.CreateBidirectionalLinkAsync(
            sourceId,
            targetId,
            ExerciseLinkType.WARMUP))
            .ReturnsAsync(ServiceResult<ExerciseLinkDto>.Success(expectedLinkDto));
        
        // Act
        var result = await testee.CreateLinkAsync(
            sourceId, 
            targetId, 
            linkType);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.LinkType.Should().Be(linkType.ToString());
        
        // Verify bidirectional handler was called
        bidirectionalLinkHandlerMock.Verify(x => x.CreateBidirectionalLinkAsync(
            sourceId,
            targetId,
            linkType),
            Times.Once);
    }

    [Fact]
    public async Task CreateLinkAsync_WithEnum_WhenValidCooldownLink_CreatesSuccessfully()
    {
        // Arrange - Complete test isolation
        var autoMocker = new AutoMocker();
        var exerciseServiceMock = autoMocker.GetMock<GetFitterGetBigger.API.Services.Exercise.IExerciseService>();
        var bidirectionalLinkHandlerMock = autoMocker.GetMock<IBidirectionalLinkHandler>();
        var linkValidationHandlerMock = autoMocker.GetMock<ILinkValidationHandler>();
        
        var testee = autoMocker.CreateInstance<ExerciseLinkService>();
        
        var sourceId = ExerciseId.New();
        var targetId = ExerciseId.New();
        var linkType = ExerciseLinkType.COOLDOWN;
        
        // Build focused test data
        // COOLDOWN links go FROM Workout TO Cooldown
        var sourceExerciseDto = ExerciseDtoTestBuilder.WorkoutExercise()
            .WithId(sourceId)
            .Build();
            
        var targetExerciseDto = ExerciseDtoTestBuilder.CooldownExercise()
            .WithId(targetId)
            .Build();
        
        // Fluent mock setup
        exerciseServiceMock
            .SetupExerciseById(sourceId, sourceExerciseDto)
            .SetupExerciseById(targetId, targetExerciseDto);
        
        // Mock validation handler for successful validation
        linkValidationHandlerMock
            .Setup(x => x.IsBidirectionalLinkUniqueAsync(sourceId, targetId, linkType))
            .ReturnsAsync(true);
        linkValidationHandlerMock
            .Setup(x => x.IsUnderMaximumLinksAsync(sourceId, linkType.ToString()))
            .ReturnsAsync(true);
        
        var expectedLinkDto = ExerciseLinkDtoTestBuilder.CooldownLink()
            .WithSourceExercise(sourceId)
            .WithTargetExercise(targetId)
            .Build();
        
        // Mock the bidirectional handler with specific values
        bidirectionalLinkHandlerMock.Setup(x => x.CreateBidirectionalLinkAsync(
            sourceId,
            targetId,
            ExerciseLinkType.COOLDOWN))
            .ReturnsAsync(ServiceResult<ExerciseLinkDto>.Success(expectedLinkDto));
        
        // Act
        var result = await testee.CreateLinkAsync(
            sourceId, 
            targetId, 
            linkType);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.LinkType.Should().Be(linkType.ToString());
        
        // Verify bidirectional handler was called
        bidirectionalLinkHandlerMock.Verify(x => x.CreateBidirectionalLinkAsync(
            sourceId,
            targetId,
            linkType),
            Times.Once);
    }

    [Fact]
    public async Task CreateLinkAsync_WithEnum_WhenValidAlternativeLink_CreatesBidirectional()
    {
        // Arrange - Complete test isolation
        var autoMocker = new AutoMocker();
        var exerciseServiceMock = autoMocker.GetMock<GetFitterGetBigger.API.Services.Exercise.IExerciseService>();
        var bidirectionalLinkHandlerMock = autoMocker.GetMock<IBidirectionalLinkHandler>();
        var linkValidationHandlerMock = autoMocker.GetMock<ILinkValidationHandler>();
        
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
        
        // Mock validation handler for successful validation
        linkValidationHandlerMock
            .Setup(x => x.IsBidirectionalLinkUniqueAsync(sourceId, targetId, linkType))
            .ReturnsAsync(true);
        linkValidationHandlerMock
            .Setup(x => x.IsUnderMaximumLinksAsync(sourceId, linkType.ToString()))
            .ReturnsAsync(true);
        
        var expectedLinkDto = ExerciseLinkDtoTestBuilder.AlternativeLink()
            .WithSourceExercise(sourceId)
            .WithTargetExercise(targetId)
            .Build();
        
        // Mock the bidirectional handler with specific values
        bidirectionalLinkHandlerMock.Setup(x => x.CreateBidirectionalLinkAsync(
            sourceId,
            targetId,
            ExerciseLinkType.ALTERNATIVE))
            .ReturnsAsync(ServiceResult<ExerciseLinkDto>.Success(expectedLinkDto));
        
        // Act
        var result = await testee.CreateLinkAsync(
            sourceId, 
            targetId, 
            linkType);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        
        // Verify bidirectional handler was called for ALTERNATIVE link
        bidirectionalLinkHandlerMock.Verify(x => x.CreateBidirectionalLinkAsync(
            sourceId,
            targetId,
            linkType),
            Times.Once);
    }

    [Fact]
    public async Task CreateLinkAsync_WithEnum_WhenWorkoutLinkType_FailsValidation()
    {
        // Arrange - No mocks needed, fails early validation
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExerciseLinkService>();
        
        var sourceId = ExerciseId.New();
        var targetId = ExerciseId.New();
        var linkType = ExerciseLinkType.WORKOUT;
        
        // Act
        var result = await testee.CreateLinkAsync(
            sourceId, 
            targetId, 
            linkType);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(ExerciseLinkErrorMessages.WorkoutLinksAutoCreated);
    }

    [Fact]
    public async Task CreateLinkAsync_WithEnum_WhenRestExercise_FailsValidation()
    {
        // Arrange - Only need exercise service for validation
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
            sourceId, 
            targetId, 
            linkType);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(ExerciseLinkErrorMessages.RestExercisesCannotHaveLinks);
    }

    [Fact]
    public async Task CreateLinkAsync_WithEnum_WhenWarmupToNonWorkout_FailsValidation()
    {
        // Arrange - Only need exercise service for validation
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
            sourceId, 
            targetId, 
            linkType);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(ExerciseLinkErrorMessages.OnlyWorkoutExercisesCanCreateLinks);
    }

    // ===== BIDIRECTIONAL DELETION TESTS =====

    [Fact]
    public async Task DeleteLinkAsync_WithDeleteReverseTrue_CallsDeleteTwice()
    {
        // Arrange - Only need validation and bidirectional handlers
        var autoMocker = new AutoMocker();
        var bidirectionalLinkHandlerMock = autoMocker.GetMock<IBidirectionalLinkHandler>();
        var linkValidationHandlerMock = autoMocker.GetMock<ILinkValidationHandler>();
        
        var testee = autoMocker.CreateInstance<ExerciseLinkService>();
        
        var exerciseId = ExerciseId.New();
        var linkId = ExerciseLinkId.New();
        var targetId = ExerciseId.New();
        
        // Setup mocks for validation
        linkValidationHandlerMock
            .Setup(x => x.DoesLinkExistAsync(linkId))
            .ReturnsAsync(true);
            
        linkValidationHandlerMock
            .Setup(x => x.DoesLinkBelongToExerciseAsync(exerciseId, linkId))
            .ReturnsAsync(true);
        
        // Mock the bidirectional handler
        bidirectionalLinkHandlerMock
            .Setup(x => x.DeleteBidirectionalLinkAsync(linkId, true))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true)));
        
        // Act
        var result = await testee.DeleteLinkAsync(exerciseId, linkId, deleteReverse: true);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeTrue();
        
        // Verify bidirectional handler was called with deleteReverse=true
        bidirectionalLinkHandlerMock.Verify(x => x.DeleteBidirectionalLinkAsync(linkId, true), Times.Once);
    }

    [Fact]
    public async Task DeleteLinkAsync_WithDeleteReverseFalse_DeletesOnlyPrimaryLink()
    {
        // Arrange - Only need validation and bidirectional handlers
        var autoMocker = new AutoMocker();
        var bidirectionalLinkHandlerMock = autoMocker.GetMock<IBidirectionalLinkHandler>();
        var linkValidationHandlerMock = autoMocker.GetMock<ILinkValidationHandler>();
        
        var testee = autoMocker.CreateInstance<ExerciseLinkService>();
        
        var exerciseId = ExerciseId.New();
        var linkId = ExerciseLinkId.New();
        
        // Setup mocks for validation
        linkValidationHandlerMock
            .Setup(x => x.DoesLinkExistAsync(linkId))
            .ReturnsAsync(true);
            
        linkValidationHandlerMock
            .Setup(x => x.DoesLinkBelongToExerciseAsync(exerciseId, linkId))
            .ReturnsAsync(true);
        
        // Mock the bidirectional handler
        bidirectionalLinkHandlerMock
            .Setup(x => x.DeleteBidirectionalLinkAsync(linkId, false))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true)));
        
        // Act
        var result = await testee.DeleteLinkAsync(exerciseId, linkId, deleteReverse: false);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeTrue();
        
        // Verify bidirectional handler was called with deleteReverse=false
        bidirectionalLinkHandlerMock.Verify(x => x.DeleteBidirectionalLinkAsync(linkId, false), Times.Once);
    }

    [Fact]
    public async Task DeleteLinkAsync_WhenReverseLinkNotFound_DeletesOnlyPrimaryLink()
    {
        // Arrange - Only need validation and bidirectional handlers
        var autoMocker = new AutoMocker();
        var bidirectionalLinkHandlerMock = autoMocker.GetMock<IBidirectionalLinkHandler>();
        var linkValidationHandlerMock = autoMocker.GetMock<ILinkValidationHandler>();
        
        var testee = autoMocker.CreateInstance<ExerciseLinkService>();
        
        var exerciseId = ExerciseId.New();
        var linkId = ExerciseLinkId.New();
        var targetId = ExerciseId.New();
        
        // Setup mocks for validation
        linkValidationHandlerMock
            .Setup(x => x.DoesLinkExistAsync(linkId))
            .ReturnsAsync(true);
            
        linkValidationHandlerMock
            .Setup(x => x.DoesLinkBelongToExerciseAsync(exerciseId, linkId))
            .ReturnsAsync(true);
        
        // Mock the bidirectional handler - it handles finding reverse link internally
        bidirectionalLinkHandlerMock
            .Setup(x => x.DeleteBidirectionalLinkAsync(linkId, true))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true)));
        
        // Act
        var result = await testee.DeleteLinkAsync(exerciseId, linkId, deleteReverse: true);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeTrue();
        
        // Verify bidirectional handler was called
        bidirectionalLinkHandlerMock.Verify(x => x.DeleteBidirectionalLinkAsync(linkId, true), Times.Once);
    }

    [Fact]
    public async Task DeleteLinkAsync_WithInvalidLinkId_ReturnsFalse()
    {
        // Arrange - No mocks needed, fails early validation
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExerciseLinkService>();
        
        var exerciseId = ExerciseId.New();
        var invalidLinkId = ExerciseLinkId.ParseOrEmpty("invalid-link-id");
        
        // Act
        var result = await testee.DeleteLinkAsync(exerciseId, invalidLinkId);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(ExerciseLinkErrorMessages.InvalidLinkId);
    }

    [Fact]
    public async Task DeleteLinkAsync_WhenLinkDoesNotExist_ReturnsNotFound()
    {
        // Arrange - Only need validation handler
        var autoMocker = new AutoMocker();
        var linkValidationHandlerMock = autoMocker.GetMock<ILinkValidationHandler>();
        var testee = autoMocker.CreateInstance<ExerciseLinkService>();
        
        var exerciseId = ExerciseId.New();
        var linkId = ExerciseLinkId.New();
        
        // Setup link validation to return false (link doesn't exist)
        linkValidationHandlerMock
            .Setup(x => x.DoesLinkExistAsync(linkId))
            .ReturnsAsync(false);
        
        // Act
        var result = await testee.DeleteLinkAsync(exerciseId, linkId);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task DeleteLinkAsync_WhenPrimaryDeletionFails_DoesNotDeleteReverse()
    {
        // Arrange - Only need validation and bidirectional handlers
        var autoMocker = new AutoMocker();
        var bidirectionalLinkHandlerMock = autoMocker.GetMock<IBidirectionalLinkHandler>();
        var linkValidationHandlerMock = autoMocker.GetMock<ILinkValidationHandler>();
        
        var testee = autoMocker.CreateInstance<ExerciseLinkService>();
        
        var exerciseId = ExerciseId.New();
        var linkId = ExerciseLinkId.New();
        
        // Setup mocks for validation
        linkValidationHandlerMock
            .Setup(x => x.DoesLinkExistAsync(linkId))
            .ReturnsAsync(true);
            
        linkValidationHandlerMock
            .Setup(x => x.DoesLinkBelongToExerciseAsync(exerciseId, linkId))
            .ReturnsAsync(true);
        
        // Setup bidirectional handler to fail - it handles the logic internally
        bidirectionalLinkHandlerMock
            .Setup(x => x.DeleteBidirectionalLinkAsync(linkId, true))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Failure(
                BooleanResultDto.Create(false),
                ServiceError.InternalError("Failed to delete link")));
        
        // Act
        var result = await testee.DeleteLinkAsync(exerciseId, linkId, deleteReverse: true);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        
        // Verify bidirectional handler was called
        bidirectionalLinkHandlerMock.Verify(x => x.DeleteBidirectionalLinkAsync(linkId, true), Times.Once);
    }
}