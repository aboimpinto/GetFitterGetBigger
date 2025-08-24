using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Enums;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.DataServices;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.Handlers;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.Extensions;
using GetFitterGetBigger.API.Tests.TestBuilders.DTOs;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;

namespace GetFitterGetBigger.API.Tests.Services.Exercise.Features.Links.Handlers;

public class BidirectionalLinkHandlerTests
{
    // NO shared state - each test creates its own mocks for complete isolation
    
    [Fact]
    public async Task CreateBidirectionalLinkAsync_WhenWarmupLink_CreatesBothDirections()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<IExerciseLinkQueryDataService>();
        var commandDataServiceMock = autoMocker.GetMock<IExerciseLinkCommandDataService>();
        var testee = autoMocker.CreateInstance<BidirectionalLinkHandler>();
        
        var sourceId = ExerciseId.New();
        var targetId = ExerciseId.New();
        var linkType = ExerciseLinkType.WARMUP;
        
        // Mock display order calculations
        queryDataServiceMock
            .SetupLinkCount(sourceId, ExerciseLinkType.WARMUP.ToString(), 2)
            .SetupLinkCount(targetId, ExerciseLinkType.WORKOUT.ToString(), 0);
        
        var expectedLinkDto = ExerciseLinkDtoTestBuilder.WarmupLink()
            .WithSourceExercise(sourceId)
            .WithTargetExercise(targetId)
            .Build();
        
        commandDataServiceMock
            .SetupSuccessfulBidirectionalCreation(expectedLinkDto);
        
        // Act
        var result = await testee.CreateBidirectionalLinkAsync(sourceId, targetId, linkType);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.LinkType.Should().Be(linkType.ToString());
        
        // Verify both links created with correct display orders
        commandDataServiceMock.Verify(x => x.CreateBidirectionalAsync(
            It.Is<ExerciseLinkDto>(l => 
                l.SourceExerciseId == sourceId.ToString() && 
                l.TargetExerciseId == targetId.ToString() &&
                l.LinkType == ExerciseLinkType.WARMUP.ToString() &&
                l.DisplayOrder == 3), // count + 1
            It.Is<ExerciseLinkDto>(l => 
                l.SourceExerciseId == targetId.ToString() && 
                l.TargetExerciseId == sourceId.ToString() &&
                l.LinkType == ExerciseLinkType.WORKOUT.ToString() &&
                l.DisplayOrder == 1)), // count + 1
            Times.Once);
    }
    
    [Fact]
    public async Task CreateBidirectionalLinkAsync_WhenAlternativeLink_CreatesBidirectionalAlternatives()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<IExerciseLinkQueryDataService>();
        var commandDataServiceMock = autoMocker.GetMock<IExerciseLinkCommandDataService>();
        var testee = autoMocker.CreateInstance<BidirectionalLinkHandler>();
        
        var sourceId = ExerciseId.New();
        var targetId = ExerciseId.New();
        var linkType = ExerciseLinkType.ALTERNATIVE;
        
        // Mock display order calculations
        queryDataServiceMock
            .SetupLinkCount(sourceId, ExerciseLinkType.ALTERNATIVE.ToString(), 1)
            .SetupLinkCount(targetId, ExerciseLinkType.ALTERNATIVE.ToString(), 3);
        
        var expectedLinkDto = ExerciseLinkDtoTestBuilder.AlternativeLink()
            .WithSourceExercise(sourceId)
            .WithTargetExercise(targetId)
            .Build();
        
        commandDataServiceMock
            .SetupSuccessfulBidirectionalCreation(expectedLinkDto);
        
        // Act
        var result = await testee.CreateBidirectionalLinkAsync(sourceId, targetId, linkType);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        
        // Verify ALTERNATIVE creates reverse ALTERNATIVE link
        commandDataServiceMock.Verify(x => x.CreateBidirectionalAsync(
            It.Is<ExerciseLinkDto>(l => 
                l.LinkType == ExerciseLinkType.ALTERNATIVE.ToString() &&
                l.DisplayOrder == 2),
            It.Is<ExerciseLinkDto>(l => 
                l.LinkType == ExerciseLinkType.ALTERNATIVE.ToString() &&
                l.DisplayOrder == 4)),
            Times.Once);
    }
    
    [Fact]
    public async Task DeleteBidirectionalLinkAsync_WhenDeleteReverseTrue_DeletesBothLinks()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<IExerciseLinkQueryDataService>();
        var commandDataServiceMock = autoMocker.GetMock<IExerciseLinkCommandDataService>();
        var testee = autoMocker.CreateInstance<BidirectionalLinkHandler>();
        
        var linkId = ExerciseLinkId.New();
        var sourceId = ExerciseId.New();
        var targetId = ExerciseId.New();
        var reverseLinkId = ExerciseLinkId.New();
        
        // Setup primary link
        var primaryLink = ExerciseLinkDtoTestBuilder.WarmupLink()
            .WithId(linkId)
            .WithSourceExercise(sourceId)
            .WithTargetExercise(targetId)
            .Build();
        
        queryDataServiceMock
            .Setup(x => x.GetByIdAsync(linkId))
            .ReturnsAsync(ServiceResult<ExerciseLinkDto>.Success(primaryLink));
        
        // Setup reverse link search
        var reverseLink = ExerciseLinkDtoTestBuilder.WorkoutLink()
            .WithId(reverseLinkId)
            .WithSourceExercise(targetId)
            .WithTargetExercise(sourceId)
            .Build();
        
        queryDataServiceMock
            .Setup(x => x.GetBySourceExerciseWithEnumAsync(targetId, ExerciseLinkType.WORKOUT))
            .ReturnsAsync(ServiceResult<List<ExerciseLinkDto>>.Success(new List<ExerciseLinkDto> { reverseLink }));
        
        // Setup successful deletions
        commandDataServiceMock
            .Setup(x => x.DeleteAsync(linkId))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true)));
        
        commandDataServiceMock
            .Setup(x => x.DeleteAsync(reverseLinkId))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true)));
        
        // Act
        var result = await testee.DeleteBidirectionalLinkAsync(linkId, deleteReverse: true);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeTrue();
        
        // Verify both links deleted
        commandDataServiceMock.Verify(x => x.DeleteAsync(linkId), Times.Once);
        commandDataServiceMock.Verify(x => x.DeleteAsync(reverseLinkId), Times.Once);
    }
    
    [Fact]
    public async Task DeleteBidirectionalLinkAsync_WhenDeleteReverseFalse_DeletesOnlyPrimaryLink()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<IExerciseLinkQueryDataService>();
        var commandDataServiceMock = autoMocker.GetMock<IExerciseLinkCommandDataService>();
        var testee = autoMocker.CreateInstance<BidirectionalLinkHandler>();
        
        var linkId = ExerciseLinkId.New();
        
        // Setup primary link
        var primaryLink = ExerciseLinkDtoTestBuilder.WarmupLink()
            .WithId(linkId)
            .Build();
        
        queryDataServiceMock
            .Setup(x => x.GetByIdAsync(linkId))
            .ReturnsAsync(ServiceResult<ExerciseLinkDto>.Success(primaryLink));
        
        // Setup successful deletion
        commandDataServiceMock
            .Setup(x => x.DeleteAsync(linkId))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true)));
        
        // Act
        var result = await testee.DeleteBidirectionalLinkAsync(linkId, deleteReverse: false);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        
        // Verify only primary link deleted
        commandDataServiceMock.Verify(x => x.DeleteAsync(linkId), Times.Once);
        commandDataServiceMock.Verify(x => x.DeleteAsync(It.IsAny<ExerciseLinkId>()), Times.Once);
    }
    
    [Theory]
    [InlineData("Warmup", ExerciseLinkType.WARMUP)]
    [InlineData("Cooldown", ExerciseLinkType.COOLDOWN)]
    [InlineData("ALTERNATIVE", ExerciseLinkType.ALTERNATIVE)]
    [InlineData("WORKOUT", ExerciseLinkType.WORKOUT)]
    public void DetermineActualLinkType_MapsCorrectly(string linkTypeString, ExerciseLinkType expected)
    {
        // Arrange
        var link = ExerciseLinkDtoTestBuilder.Default()
            .WithLinkType(linkTypeString)
            .Build();
        
        // Act
        var result = BidirectionalLinkHandler.DetermineActualLinkType(link);
        
        // Assert
        result.Should().Be(expected);
    }
    
    [Theory]
    [InlineData(ExerciseLinkType.WARMUP, ExerciseLinkType.WORKOUT)]
    [InlineData(ExerciseLinkType.COOLDOWN, ExerciseLinkType.WORKOUT)]
    [InlineData(ExerciseLinkType.ALTERNATIVE, ExerciseLinkType.ALTERNATIVE)]
    [InlineData(ExerciseLinkType.WORKOUT, null)]
    public void GetReverseExerciseLinkType_ReturnsCorrectReverseType(
        ExerciseLinkType linkType, 
        ExerciseLinkType? expectedReverse)
    {
        // Act
        var result = BidirectionalLinkHandler.GetReverseExerciseLinkType(linkType);
        
        // Assert
        result.Should().Be(expectedReverse);
    }
}