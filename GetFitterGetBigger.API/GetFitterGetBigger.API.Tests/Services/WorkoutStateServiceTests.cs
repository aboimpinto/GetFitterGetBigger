using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.ReferenceTables.WorkoutState;
using GetFitterGetBigger.API.Services.ReferenceTables.WorkoutState.DataServices;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services;

/// <summary>
/// Unit tests for WorkoutStateService following the new DataService architecture.
/// Uses AutoMocker for isolation and FluentAssertions for clear assertions.
/// </summary>
public class WorkoutStateServiceTests
{
    [Fact]
    public async Task GetAllActiveAsync_WhenCacheMiss_CallsDataServiceAndCachesResult()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<WorkoutStateService>();
        
        var expectedDtos = new List<WorkoutStateDto>
        {
            new() { Id = "workoutstate-guid1", Value = "Draft", Description = "Draft state" },
            new() { Id = "workoutstate-guid2", Value = "Production", Description = "Production state" }
        };
        
        autoMocker.GetMock<IWorkoutStateDataService>()
            .Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(ServiceResult<IEnumerable<WorkoutStateDto>>.Success(expectedDtos));
        
        autoMocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<IEnumerable<WorkoutStateDto>>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<IEnumerable<WorkoutStateDto>>.Miss());
        
        // Act
        var result = await testee.GetAllActiveAsync();
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedDtos);
        
        autoMocker.GetMock<IWorkoutStateDataService>()
            .Verify(x => x.GetAllActiveAsync(), Times.Once);
        
        autoMocker.GetMock<IEternalCacheService>()
            .Verify(x => x.SetAsync(It.IsAny<string>(), It.Is<IEnumerable<WorkoutStateDto>>(
                data => data.Count() == expectedDtos.Count)), Times.Once);
    }
    
    [Fact]
    public async Task GetAllActiveAsync_WhenCacheHit_ReturnsFromCacheWithoutCallingDataService()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<WorkoutStateService>();
        
        var cachedDtos = new List<WorkoutStateDto>
        {
            new() { Id = "workoutstate-guid1", Value = "Draft", Description = "Draft state" },
            new() { Id = "workoutstate-guid2", Value = "Production", Description = "Production state" }
        };
        
        autoMocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<IEnumerable<WorkoutStateDto>>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<IEnumerable<WorkoutStateDto>>.Hit(cachedDtos));
        
        // Act
        var result = await testee.GetAllActiveAsync();
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(cachedDtos);
        
        autoMocker.GetMock<IWorkoutStateDataService>()
            .Verify(x => x.GetAllActiveAsync(), Times.Never);
        
        autoMocker.GetMock<IEternalCacheService>()
            .Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<IEnumerable<WorkoutStateDto>>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsSuccessWithData()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<WorkoutStateService>();
        
        var stateId = WorkoutStateId.New();
        var expectedDto = new WorkoutStateDto 
        { 
            Id = stateId.ToString(), 
            Value = "Draft", 
            Description = "Draft state" 
        };
        
        autoMocker.GetMock<IWorkoutStateDataService>()
            .Setup(x => x.GetByIdAsync(stateId))
            .ReturnsAsync(ServiceResult<WorkoutStateDto>.Success(expectedDto));
        
        autoMocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<WorkoutStateDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<WorkoutStateDto>.Miss());
        
        // Act
        var result = await testee.GetByIdAsync(stateId);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedDto);
        result.Errors.Should().BeEmpty();
        
        autoMocker.GetMock<IWorkoutStateDataService>()
            .Verify(x => x.GetByIdAsync(stateId), Times.Once);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithEmptyId_ReturnsValidationFailure()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<WorkoutStateService>();
        
        var emptyId = WorkoutStateId.Empty;
        
        // Act
        var result = await testee.GetByIdAsync(emptyId);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(WorkoutStateErrorMessages.InvalidIdFormat);
        
        autoMocker.GetMock<IWorkoutStateDataService>()
            .Verify(x => x.GetByIdAsync(It.IsAny<WorkoutStateId>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByValueAsync_WithValidValue_ReturnsSuccessWithData()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<WorkoutStateService>();
        
        var value = "Draft";
        var expectedDto = new WorkoutStateDto 
        { 
            Id = "workoutstate-guid1", 
            Value = value, 
            Description = "Draft state" 
        };
        
        autoMocker.GetMock<IWorkoutStateDataService>()
            .Setup(x => x.GetByValueAsync(value))
            .ReturnsAsync(ServiceResult<WorkoutStateDto>.Success(expectedDto));
        
        autoMocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<WorkoutStateDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<WorkoutStateDto>.Miss());
        
        // Act
        var result = await testee.GetByValueAsync(value);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedDto);
        result.Data.Value.Should().Be(value);
        
        autoMocker.GetMock<IWorkoutStateDataService>()
            .Verify(x => x.GetByValueAsync(value), Times.Once);
    }
    
    [Fact]
    public async Task GetByValueAsync_WithEmptyValue_ReturnsValidationFailure()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<WorkoutStateService>();
        
        var emptyValue = "";
        
        // Act
        var result = await testee.GetByValueAsync(emptyValue);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(WorkoutStateErrorMessages.ValueCannotBeEmpty);
        
        autoMocker.GetMock<IWorkoutStateDataService>()
            .Verify(x => x.GetByValueAsync(It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task ExistsAsync_WhenStateExists_ReturnsTrue()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<WorkoutStateService>();
        
        var stateId = WorkoutStateId.New();
        var expectedDto = new WorkoutStateDto 
        { 
            Id = stateId.ToString(), 
            Value = "Draft", 
            Description = "Draft state" 
        };
        
        // ExistsAsync calls GetByIdAsync internally, so we need to mock GetByIdAsync
        autoMocker.GetMock<IWorkoutStateDataService>()
            .Setup(x => x.GetByIdAsync(stateId))
            .ReturnsAsync(ServiceResult<WorkoutStateDto>.Success(expectedDto));
        
        autoMocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<WorkoutStateDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<WorkoutStateDto>.Miss());
        
        // Act
        var result = await testee.ExistsAsync(stateId);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeTrue();
        
        autoMocker.GetMock<IWorkoutStateDataService>()
            .Verify(x => x.GetByIdAsync(stateId), Times.Once);
    }
    
    [Fact]
    public async Task ExistsAsync_WhenStateDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<WorkoutStateService>();
        
        var stateId = WorkoutStateId.New();
        
        // ExistsAsync calls GetByIdAsync internally, so we need to mock GetByIdAsync to return Empty
        autoMocker.GetMock<IWorkoutStateDataService>()
            .Setup(x => x.GetByIdAsync(stateId))
            .ReturnsAsync(ServiceResult<WorkoutStateDto>.Success(WorkoutStateDto.Empty));
        
        autoMocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<WorkoutStateDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<WorkoutStateDto>.Miss());
        
        // Act
        var result = await testee.ExistsAsync(stateId);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeFalse();
        
        autoMocker.GetMock<IWorkoutStateDataService>()
            .Verify(x => x.GetByIdAsync(stateId), Times.Once);
    }
    
    [Fact]
    public async Task ExistsAsync_WithEmptyId_ReturnsValidationFailure()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<WorkoutStateService>();
        
        var emptyId = WorkoutStateId.Empty;
        
        // Act
        var result = await testee.ExistsAsync(emptyId);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(WorkoutStateErrorMessages.InvalidIdFormat);
        
        autoMocker.GetMock<IWorkoutStateDataService>()
            .Verify(x => x.ExistsAsync(It.IsAny<WorkoutStateId>()), Times.Never);
    }
}