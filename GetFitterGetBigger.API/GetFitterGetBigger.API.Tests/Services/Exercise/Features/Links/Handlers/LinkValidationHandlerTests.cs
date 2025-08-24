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

public class LinkValidationHandlerTests
{
    // NO shared state - each test creates its own mocks for complete isolation
    
    [Theory]
    [InlineData("Warmup", true)]
    [InlineData("Cooldown", true)]
    [InlineData("WARMUP", true)]
    [InlineData("COOLDOWN", true)]
    [InlineData("ALTERNATIVE", true)]
    [InlineData("WORKOUT", true)]
    [InlineData("InvalidType", false)]
    [InlineData("", false)]
    public void IsValidLinkType_String_ReturnsExpectedResult(string linkType, bool expected)
    {
        // Act
        var result = LinkValidationHandler.IsValidLinkType(linkType);
        
        // Assert
        result.Should().Be(expected);
    }
    
    [Theory]
    [InlineData(ExerciseLinkType.WARMUP, true)]
    [InlineData(ExerciseLinkType.COOLDOWN, true)]
    [InlineData(ExerciseLinkType.ALTERNATIVE, true)]
    [InlineData(ExerciseLinkType.WORKOUT, true)]
    [InlineData((ExerciseLinkType)999, false)]
    public void IsValidLinkType_Enum_ReturnsExpectedResult(ExerciseLinkType linkType, bool expected)
    {
        // Act
        var result = LinkValidationHandler.IsValidLinkType(linkType);
        
        // Assert
        result.Should().Be(expected);
    }
    
    [Fact]
    public async Task IsLinkUniqueAsync_WhenLinkDoesNotExist_ReturnsTrue()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<IExerciseLinkQueryDataService>();
        var testee = autoMocker.CreateInstance<LinkValidationHandler>();
        
        var sourceId = ExerciseId.New();
        var targetId = ExerciseId.New();
        var linkType = "Warmup";
        
        queryDataServiceMock
            .Setup(x => x.ExistsAsync(sourceId, targetId, linkType))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(false)));
        
        // Act
        var result = await testee.IsLinkUniqueAsync(sourceId, targetId, linkType);
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task IsLinkUniqueAsync_WhenLinkExists_ReturnsFalse()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<IExerciseLinkQueryDataService>();
        var testee = autoMocker.CreateInstance<LinkValidationHandler>();
        
        var sourceId = ExerciseId.New();
        var targetId = ExerciseId.New();
        var linkType = "Warmup";
        
        queryDataServiceMock
            .Setup(x => x.ExistsAsync(sourceId, targetId, linkType))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true)));
        
        // Act
        var result = await testee.IsLinkUniqueAsync(sourceId, targetId, linkType);
        
        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task IsUnderMaximumLinksAsync_WhenUnderLimit_ReturnsTrue()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<IExerciseLinkQueryDataService>();
        var testee = autoMocker.CreateInstance<LinkValidationHandler>();
        
        var sourceId = ExerciseId.New();
        var linkType = "Warmup";
        
        queryDataServiceMock
            .Setup(x => x.GetLinkCountAsync(sourceId, linkType))
            .ReturnsAsync(ServiceResult<int>.Success(5)); // Under 10 limit
        
        // Act
        var result = await testee.IsUnderMaximumLinksAsync(sourceId, linkType);
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task IsUnderMaximumLinksAsync_WhenAtLimit_ReturnsFalse()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<IExerciseLinkQueryDataService>();
        var testee = autoMocker.CreateInstance<LinkValidationHandler>();
        
        var sourceId = ExerciseId.New();
        var linkType = "Warmup";
        
        queryDataServiceMock
            .Setup(x => x.GetLinkCountAsync(sourceId, linkType))
            .ReturnsAsync(ServiceResult<int>.Success(10)); // At limit
        
        // Act
        var result = await testee.IsUnderMaximumLinksAsync(sourceId, linkType);
        
        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task IsUnderMaximumLinksAsync_WhenOverLimit_ReturnsFalse()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<IExerciseLinkQueryDataService>();
        var testee = autoMocker.CreateInstance<LinkValidationHandler>();
        
        var sourceId = ExerciseId.New();
        var linkType = "Warmup";
        
        queryDataServiceMock
            .Setup(x => x.GetLinkCountAsync(sourceId, linkType))
            .ReturnsAsync(ServiceResult<int>.Success(15)); // Over limit
        
        // Act
        var result = await testee.IsUnderMaximumLinksAsync(sourceId, linkType);
        
        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task DoesLinkExistAsync_WhenLinkExists_ReturnsTrue()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<IExerciseLinkQueryDataService>();
        var testee = autoMocker.CreateInstance<LinkValidationHandler>();
        
        var linkId = ExerciseLinkId.New();
        var linkDto = ExerciseLinkDtoTestBuilder.Default()
            .WithId(linkId)
            .Build();
        
        queryDataServiceMock
            .Setup(x => x.GetByIdAsync(linkId))
            .ReturnsAsync(ServiceResult<ExerciseLinkDto>.Success(linkDto));
        
        // Act
        var result = await testee.DoesLinkExistAsync(linkId);
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task DoesLinkExistAsync_WhenLinkIsEmpty_ReturnsFalse()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<IExerciseLinkQueryDataService>();
        var testee = autoMocker.CreateInstance<LinkValidationHandler>();
        
        var linkId = ExerciseLinkId.New();
        
        queryDataServiceMock
            .Setup(x => x.GetByIdAsync(linkId))
            .ReturnsAsync(ServiceResult<ExerciseLinkDto>.Success(ExerciseLinkDto.Empty));
        
        // Act
        var result = await testee.DoesLinkExistAsync(linkId);
        
        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task DoesLinkBelongToExerciseAsync_WhenLinkBelongsToExercise_ReturnsTrue()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<IExerciseLinkQueryDataService>();
        var testee = autoMocker.CreateInstance<LinkValidationHandler>();
        
        var exerciseId = ExerciseId.New();
        var linkId = ExerciseLinkId.New();
        
        var linkDto = ExerciseLinkDtoTestBuilder.Default()
            .WithId(linkId)
            .WithSourceExercise(exerciseId)
            .Build();
        
        queryDataServiceMock
            .Setup(x => x.GetByIdAsync(linkId))
            .ReturnsAsync(ServiceResult<ExerciseLinkDto>.Success(linkDto));
        
        // Act
        var result = await testee.DoesLinkBelongToExerciseAsync(exerciseId, linkId);
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task DoesLinkBelongToExerciseAsync_WhenLinkDoesNotBelongToExercise_ReturnsFalse()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<IExerciseLinkQueryDataService>();
        var testee = autoMocker.CreateInstance<LinkValidationHandler>();
        
        var exerciseId = ExerciseId.New();
        var differentExerciseId = ExerciseId.New();
        var linkId = ExerciseLinkId.New();
        
        var linkDto = ExerciseLinkDtoTestBuilder.Default()
            .WithId(linkId)
            .WithSourceExercise(differentExerciseId) // Different exercise
            .Build();
        
        queryDataServiceMock
            .Setup(x => x.GetByIdAsync(linkId))
            .ReturnsAsync(ServiceResult<ExerciseLinkDto>.Success(linkDto));
        
        // Act
        var result = await testee.DoesLinkBelongToExerciseAsync(exerciseId, linkId);
        
        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task DoesLinkBelongToExerciseAsync_WhenQueryFails_ReturnsFalse()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<IExerciseLinkQueryDataService>();
        var testee = autoMocker.CreateInstance<LinkValidationHandler>();
        
        var exerciseId = ExerciseId.New();
        var linkId = ExerciseLinkId.New();
        
        queryDataServiceMock
            .Setup(x => x.GetByIdAsync(linkId))
            .ReturnsAsync(ServiceResult<ExerciseLinkDto>.Failure(
                ExerciseLinkDto.Empty,
                ServiceError.NotFound("ExerciseLink", linkId.ToString())));
        
        // Act
        var result = await testee.DoesLinkBelongToExerciseAsync(exerciseId, linkId);
        
        // Assert
        result.Should().BeFalse();
    }
}