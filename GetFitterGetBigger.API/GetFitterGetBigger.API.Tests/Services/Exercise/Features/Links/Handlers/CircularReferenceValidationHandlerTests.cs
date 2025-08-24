using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.DataServices;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.Handlers;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.TestBuilders.DTOs;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;

namespace GetFitterGetBigger.API.Tests.Services.Exercise.Features.Links.Handlers;

public class CircularReferenceValidationHandlerTests
{
    // NO shared state - each test creates its own mocks for complete isolation
    
    [Fact]
    public async Task IsNoCircularReferenceAsync_WhenNoCircularReference_ReturnsTrue()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<IExerciseLinkQueryDataService>();
        var testee = autoMocker.CreateInstance<CircularReferenceValidationHandler>();
        
        var sourceId = ExerciseId.New();
        var targetId = ExerciseId.New();
        var thirdId = ExerciseId.New();
        
        // Setup link chain: target -> third (no link back to source)
        var targetLinks = new List<ExerciseLinkDto>
        {
            ExerciseLinkDtoTestBuilder.Default()
                .WithSourceExercise(targetId)
                .WithTargetExercise(thirdId)
                .Build()
        };
        
        var targetLinksResult = ServiceResult<List<ExerciseLinkDto>>.Success(targetLinks);
        queryDataServiceMock
            .Setup(x => x.GetBySourceExerciseAsync(targetId, null))
            .ReturnsAsync(targetLinksResult);
        
        // Third exercise has no links
        var emptyLinksResult = ServiceResult<List<ExerciseLinkDto>>.Success(new List<ExerciseLinkDto>());
        queryDataServiceMock
            .Setup(x => x.GetBySourceExerciseAsync(thirdId, null))
            .ReturnsAsync(emptyLinksResult);
        
        // Act
        var result = await testee.IsNoCircularReferenceAsync(sourceId, targetId);
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task IsNoCircularReferenceAsync_WhenDirectCircularReference_ReturnsFalse()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<IExerciseLinkQueryDataService>();
        var testee = autoMocker.CreateInstance<CircularReferenceValidationHandler>();
        
        var sourceId = ExerciseId.New();
        var targetId = ExerciseId.New();
        
        // Setup circular reference: target links back to source
        var targetLinks = new List<ExerciseLinkDto>
        {
            ExerciseLinkDtoTestBuilder.Default()
                .WithSourceExercise(targetId)
                .WithTargetExercise(sourceId) // Circular reference!
                .Build()
        };
        
        var targetLinksResult = ServiceResult<List<ExerciseLinkDto>>.Success(targetLinks);
        queryDataServiceMock
            .Setup(x => x.GetBySourceExerciseAsync(targetId, null))
            .ReturnsAsync(targetLinksResult);
        
        // Act
        var result = await testee.IsNoCircularReferenceAsync(sourceId, targetId);
        
        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task IsNoCircularReferenceAsync_WhenIndirectCircularReference_ReturnsFalse()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<IExerciseLinkQueryDataService>();
        var testee = autoMocker.CreateInstance<CircularReferenceValidationHandler>();
        
        var sourceId = ExerciseId.New();
        var targetId = ExerciseId.New();
        var middleId = ExerciseId.New();
        
        // Setup chain: target -> middle -> source (indirect circular reference)
        var targetLinks = new List<ExerciseLinkDto>
        {
            ExerciseLinkDtoTestBuilder.Default()
                .WithSourceExercise(targetId)
                .WithTargetExercise(middleId)
                .Build()
        };
        
        var middleLinks = new List<ExerciseLinkDto>
        {
            ExerciseLinkDtoTestBuilder.Default()
                .WithSourceExercise(middleId)
                .WithTargetExercise(sourceId) // Indirect circular reference!
                .Build()
        };
        
        var targetLinksResult = ServiceResult<List<ExerciseLinkDto>>.Success(targetLinks);
        queryDataServiceMock
            .Setup(x => x.GetBySourceExerciseAsync(targetId, null))
            .ReturnsAsync(targetLinksResult);
        
        var middleLinksResult = ServiceResult<List<ExerciseLinkDto>>.Success(middleLinks);
        queryDataServiceMock
            .Setup(x => x.GetBySourceExerciseAsync(middleId, null))
            .ReturnsAsync(middleLinksResult);
        
        // Act
        var result = await testee.IsNoCircularReferenceAsync(sourceId, targetId);
        
        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task IsNoCircularReferenceAsync_WhenComplexGraphWithoutCircle_ReturnsTrue()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<IExerciseLinkQueryDataService>();
        var testee = autoMocker.CreateInstance<CircularReferenceValidationHandler>();
        
        var sourceId = ExerciseId.New();
        var targetId = ExerciseId.New();
        var nodeA = ExerciseId.New();
        var nodeB = ExerciseId.New();
        var nodeC = ExerciseId.New();
        
        // Setup complex graph without circular reference
        // target -> [nodeA, nodeB]
        // nodeA -> nodeC
        // nodeB -> nodeC
        // nodeC -> (no links)
        
        var targetLinks = new List<ExerciseLinkDto>
        {
            ExerciseLinkDtoTestBuilder.Default()
                .WithSourceExercise(targetId)
                .WithTargetExercise(nodeA)
                .Build(),
            ExerciseLinkDtoTestBuilder.Default()
                .WithSourceExercise(targetId)
                .WithTargetExercise(nodeB)
                .Build()
        };
        
        var nodeALinks = new List<ExerciseLinkDto>
        {
            ExerciseLinkDtoTestBuilder.Default()
                .WithSourceExercise(nodeA)
                .WithTargetExercise(nodeC)
                .Build()
        };
        
        var nodeBLinks = new List<ExerciseLinkDto>
        {
            ExerciseLinkDtoTestBuilder.Default()
                .WithSourceExercise(nodeB)
                .WithTargetExercise(nodeC)
                .Build()
        };
        
        var targetLinksResult = ServiceResult<List<ExerciseLinkDto>>.Success(targetLinks);
        queryDataServiceMock
            .Setup(x => x.GetBySourceExerciseAsync(targetId, null))
            .ReturnsAsync(targetLinksResult);
        
        var nodeALinksResult = ServiceResult<List<ExerciseLinkDto>>.Success(nodeALinks);
        queryDataServiceMock
            .Setup(x => x.GetBySourceExerciseAsync(nodeA, null))
            .ReturnsAsync(nodeALinksResult);
        
        var nodeBLinksResult = ServiceResult<List<ExerciseLinkDto>>.Success(nodeBLinks);
        queryDataServiceMock
            .Setup(x => x.GetBySourceExerciseAsync(nodeB, null))
            .ReturnsAsync(nodeBLinksResult);
        
        var emptyNodeCResult = ServiceResult<List<ExerciseLinkDto>>.Success(new List<ExerciseLinkDto>());
        queryDataServiceMock
            .Setup(x => x.GetBySourceExerciseAsync(nodeC, null))
            .ReturnsAsync(emptyNodeCResult);
        
        // Act
        var result = await testee.IsNoCircularReferenceAsync(sourceId, targetId);
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task IsNoCircularReferenceAsync_WhenQueryFails_ReturnsTrue()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var queryDataServiceMock = autoMocker.GetMock<IExerciseLinkQueryDataService>();
        var testee = autoMocker.CreateInstance<CircularReferenceValidationHandler>();
        
        var sourceId = ExerciseId.New();
        var targetId = ExerciseId.New();
        
        // Setup failed query
        queryDataServiceMock
            .Setup(x => x.GetBySourceExerciseAsync(targetId, null))
            .ReturnsAsync(() => ServiceResult<List<ExerciseLinkDto>>.Failure(
                new List<ExerciseLinkDto>(),
                ServiceError.InternalError("Database error")));
        
        // Act
        var result = await testee.IsNoCircularReferenceAsync(sourceId, targetId);
        
        // Assert
        result.Should().BeTrue(); // Returns true when query fails (safe default)
    }
}