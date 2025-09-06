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

/// <summary>
/// Integration tests to verify that Warmup and Cooldown exercises can create Alternative links
/// Following Red-Green-Refactor TDD methodology
/// </summary>
public class ExerciseLinkServiceAlternativeLinksTests
{
    [Fact]
    public async Task CreateLinkAsync_WhenWarmupExerciseCreatesAlternativeLink_ShouldSucceed()
    {
        // Arrange - Complete test isolation
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<API.Services.Exercise.Features.Links.DataServices.IExerciseLinkQueryDataService>();
        var bidirectionalLinkHandlerMock = autoMocker.GetMock<IBidirectionalLinkHandler>();
        var linkValidationHandlerMock = autoMocker.GetMock<ILinkValidationHandler>();
        
        var testee = autoMocker.CreateInstance<ExerciseLinkService>();
        
        var sourceId = ExerciseId.New();
        var targetId = ExerciseId.New();
        var linkType = ExerciseLinkType.ALTERNATIVE;
        
        // Build test data - Warmup exercise creating Alternative link
        var sourceExerciseDto = ExerciseDtoTestBuilder.WarmupExercise()
            .WithId(sourceId)
            .WithName("Warmup Exercise")
            .Build();
            
        var targetExerciseDto = ExerciseDtoTestBuilder.AlternativeExercise()
            .WithId(targetId)
            .WithName("Alternative Exercise")
            .Build();
        
        // Mock the query data service for exercise validation
        queryDataServiceMock
            .Setup(x => x.GetAndValidateExerciseAsync(sourceId))
            .ReturnsAsync(ServiceResult<ExerciseDto>.Success(sourceExerciseDto));
        queryDataServiceMock
            .Setup(x => x.GetAndValidateExerciseAsync(targetId))
            .ReturnsAsync(ServiceResult<ExerciseDto>.Success(targetExerciseDto));
        
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
        
        // Assert - This test should PASS after the fix
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue("Warmup exercises should be able to create Alternative links");
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
    public async Task CreateLinkAsync_WhenCooldownExerciseCreatesAlternativeLink_ShouldSucceed()
    {
        // Arrange - Complete test isolation
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<API.Services.Exercise.Features.Links.DataServices.IExerciseLinkQueryDataService>();
        var bidirectionalLinkHandlerMock = autoMocker.GetMock<IBidirectionalLinkHandler>();
        var linkValidationHandlerMock = autoMocker.GetMock<ILinkValidationHandler>();
        
        var testee = autoMocker.CreateInstance<ExerciseLinkService>();
        
        var sourceId = ExerciseId.New();
        var targetId = ExerciseId.New();
        var linkType = ExerciseLinkType.ALTERNATIVE;
        
        // Build test data - Cooldown exercise creating Alternative link
        var sourceExerciseDto = ExerciseDtoTestBuilder.CooldownExercise()
            .WithId(sourceId)
            .WithName("Cooldown Exercise")
            .Build();
            
        var targetExerciseDto = ExerciseDtoTestBuilder.AlternativeExercise()
            .WithId(targetId)
            .WithName("Alternative Exercise")
            .Build();
        
        // Mock the query data service for exercise validation
        queryDataServiceMock
            .Setup(x => x.GetAndValidateExerciseAsync(sourceId))
            .ReturnsAsync(ServiceResult<ExerciseDto>.Success(sourceExerciseDto));
        queryDataServiceMock
            .Setup(x => x.GetAndValidateExerciseAsync(targetId))
            .ReturnsAsync(ServiceResult<ExerciseDto>.Success(targetExerciseDto));
        
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
        
        // Assert - This test should PASS after the fix
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue("Cooldown exercises should be able to create Alternative links");
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
    public async Task CreateLinkAsync_WhenWarmupExerciseCreatesWorkoutLink_ShouldSucceed()
    {
        // Arrange - Complete test isolation
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<API.Services.Exercise.Features.Links.DataServices.IExerciseLinkQueryDataService>();
        var bidirectionalLinkHandlerMock = autoMocker.GetMock<IBidirectionalLinkHandler>();
        var linkValidationHandlerMock = autoMocker.GetMock<ILinkValidationHandler>();
        
        var testee = autoMocker.CreateInstance<ExerciseLinkService>();
        
        var sourceId = ExerciseId.New();
        var targetId = ExerciseId.New();
        // Note: WORKOUT link type indicates linking TO a Workout exercise
        // This creates the reverse relationship - from Warmup TO Workout
        var linkType = ExerciseLinkType.WORKOUT;
        
        // Build test data - Warmup linking to Workout (creates reverse WARMUP link)
        var sourceExerciseDto = ExerciseDtoTestBuilder.WarmupExercise()
            .WithId(sourceId)
            .WithName("Warmup Exercise")
            .Build();
            
        var targetExerciseDto = ExerciseDtoTestBuilder.WorkoutExercise()
            .WithId(targetId)
            .WithName("Workout Exercise")
            .Build();
        
        // Mock the query data service for exercise validation
        queryDataServiceMock
            .Setup(x => x.GetAndValidateExerciseAsync(sourceId))
            .ReturnsAsync(ServiceResult<ExerciseDto>.Success(sourceExerciseDto));
        queryDataServiceMock
            .Setup(x => x.GetAndValidateExerciseAsync(targetId))
            .ReturnsAsync(ServiceResult<ExerciseDto>.Success(targetExerciseDto));
        
        // Act
        var result = await testee.CreateLinkAsync(
            sourceId, 
            targetId, 
            linkType);
        
        // Assert - WORKOUT link type should be rejected by early validation
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse("WORKOUT link type is not allowed for manual creation");
        result.Errors.Should().Contain(ExerciseLinkErrorMessages.WorkoutLinksAutoCreated);
    }
    
    [Fact]
    public async Task CreateLinkAsync_WhenCooldownExerciseCreatesWorkoutLink_ShouldSucceed()
    {
        // Arrange - Complete test isolation
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<API.Services.Exercise.Features.Links.DataServices.IExerciseLinkQueryDataService>();
        var bidirectionalLinkHandlerMock = autoMocker.GetMock<IBidirectionalLinkHandler>();
        var linkValidationHandlerMock = autoMocker.GetMock<ILinkValidationHandler>();
        
        var testee = autoMocker.CreateInstance<ExerciseLinkService>();
        
        var sourceId = ExerciseId.New();
        var targetId = ExerciseId.New();
        // Note: WORKOUT link type indicates linking TO a Workout exercise
        // This creates the reverse relationship - from Cooldown TO Workout
        var linkType = ExerciseLinkType.WORKOUT;
        
        // Build test data - Cooldown linking to Workout (creates reverse COOLDOWN link)
        var sourceExerciseDto = ExerciseDtoTestBuilder.CooldownExercise()
            .WithId(sourceId)
            .WithName("Cooldown Exercise")
            .Build();
            
        var targetExerciseDto = ExerciseDtoTestBuilder.WorkoutExercise()
            .WithId(targetId)
            .WithName("Workout Exercise")
            .Build();
        
        // Mock the query data service for exercise validation
        queryDataServiceMock
            .Setup(x => x.GetAndValidateExerciseAsync(sourceId))
            .ReturnsAsync(ServiceResult<ExerciseDto>.Success(sourceExerciseDto));
        queryDataServiceMock
            .Setup(x => x.GetAndValidateExerciseAsync(targetId))
            .ReturnsAsync(ServiceResult<ExerciseDto>.Success(targetExerciseDto));
        
        // Act
        var result = await testee.CreateLinkAsync(
            sourceId, 
            targetId, 
            linkType);
        
        // Assert - WORKOUT link type should be rejected by early validation
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse("WORKOUT link type is not allowed for manual creation");
        result.Errors.Should().Contain(ExerciseLinkErrorMessages.WorkoutLinksAutoCreated);
    }
}