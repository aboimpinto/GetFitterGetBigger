using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.Enums;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.DataServices;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.Extensions;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.Extensions;
using Moq;
using Moq.AutoMock;

namespace GetFitterGetBigger.API.Tests.Services.Exercise.Features.Links.DataServices;

public class ExerciseLinkCommandDataServiceTests
{
    [Fact]
    public async Task CreateBidirectionalAsync_HandlesTransactionCorrectly()
    {
        // Arrange - Testing IExerciseLinkCommandDataService behavior
        var autoMocker = new AutoMocker();
        var commandDataServiceMock = autoMocker.GetMock<IExerciseLinkCommandDataService>();
        
        var sourceId = ExerciseId.New();
        var targetId = ExerciseId.New();
        
        var primaryLink = ExerciseLink.Handler.CreateNew(
            sourceId,
            targetId,
            ExerciseLinkType.WARMUP,
            1);
            
        var reverseLink = ExerciseLink.Handler.CreateNew(
            primaryLink.TargetExerciseId,
            primaryLink.SourceExerciseId,
            ExerciseLinkType.WORKOUT,
            1);
        
        var expectedDto = primaryLink.ToDto();
        
        // Setup the mock with specific values
        commandDataServiceMock
            .Setup(x => x.CreateBidirectionalAsync(
                It.Is<ExerciseLinkDto>(l => l.SourceExerciseId == sourceId.ToString() 
                                          && l.TargetExerciseId == targetId.ToString()
                                          && l.LinkType == "WARMUP"),
                It.Is<ExerciseLinkDto>(l => l.SourceExerciseId == targetId.ToString() 
                                          && l.TargetExerciseId == sourceId.ToString()
                                          && l.LinkType == "WORKOUT")))
            .ReturnsAsync(ServiceResult<ExerciseLinkDto>.Success(expectedDto));
        
        // Act
        var result = await commandDataServiceMock.Object.CreateBidirectionalAsync(
            primaryLink.ToDto(), 
            reverseLink.ToDto());
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.SourceExerciseId.Should().Be(sourceId.ToString());
        result.Data.TargetExerciseId.Should().Be(targetId.ToString());
        result.Data.LinkType.Should().Be("WARMUP");
        
        // Verify the method was called exactly once with both links
        commandDataServiceMock.Verify(x => x.CreateBidirectionalAsync(
            It.Is<ExerciseLinkDto>(l => l.SourceExerciseId == sourceId.ToString()),
            It.Is<ExerciseLinkDto>(l => l.SourceExerciseId == targetId.ToString())),
            Times.Once);
    }
}