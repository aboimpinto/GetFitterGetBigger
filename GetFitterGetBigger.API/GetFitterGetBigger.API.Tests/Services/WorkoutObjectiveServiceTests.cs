using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.ReferenceTables.WorkoutObjective;
using GetFitterGetBigger.API.Services.ReferenceTables.WorkoutObjective.DataServices;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services;

/// <summary>
/// Unit tests for WorkoutObjectiveService following the new DataService architecture.
/// Uses AutoMocker for isolation and FluentAssertions for clear assertions.
/// Note: WorkoutObjectiveService uses ReferenceDataDto, not WorkoutObjectiveDto.
/// </summary>
public class WorkoutObjectiveServiceTests
{
    [Fact]
    public async Task GetAllActiveAsync_WhenCacheMiss_CallsDataServiceAndCachesResult()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<WorkoutObjectiveService>();
        
        var expectedDtos = new List<ReferenceDataDto>
        {
            new() { Id = "workoutobjective-guid1", Value = "Muscle Gain", Description = "Build muscle mass" },
            new() { Id = "workoutobjective-guid2", Value = "Fat Loss", Description = "Reduce body fat" }
        };
        
        autoMocker.GetMock<IWorkoutObjectiveDataService>()
            .Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(ServiceResult<IEnumerable<ReferenceDataDto>>.Success(expectedDtos));
        
        autoMocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<IEnumerable<ReferenceDataDto>>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<IEnumerable<ReferenceDataDto>>.Miss());
        
        // Act
        var result = await testee.GetAllActiveAsync();
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedDtos);
        
        autoMocker.GetMock<IWorkoutObjectiveDataService>()
            .Verify(x => x.GetAllActiveAsync(), Times.Once);
        
        autoMocker.GetMock<IEternalCacheService>()
            .Verify(x => x.SetAsync(It.IsAny<string>(), It.Is<IEnumerable<ReferenceDataDto>>(
                data => data.Count() == expectedDtos.Count)), Times.Once);
    }
    
    [Fact]
    public async Task GetAllActiveAsync_WhenCacheHit_ReturnsFromCacheWithoutCallingDataService()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<WorkoutObjectiveService>();
        
        var cachedDtos = new List<ReferenceDataDto>
        {
            new() { Id = "workoutobjective-guid1", Value = "Muscle Gain", Description = "Build muscle mass" },
            new() { Id = "workoutobjective-guid2", Value = "Fat Loss", Description = "Reduce body fat" }
        };
        
        autoMocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<IEnumerable<ReferenceDataDto>>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<IEnumerable<ReferenceDataDto>>.Hit(cachedDtos));
        
        // Act
        var result = await testee.GetAllActiveAsync();
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(cachedDtos);
        
        autoMocker.GetMock<IWorkoutObjectiveDataService>()
            .Verify(x => x.GetAllActiveAsync(), Times.Never);
        
        autoMocker.GetMock<IEternalCacheService>()
            .Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ReferenceDataDto>>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsSuccessWithData()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<WorkoutObjectiveService>();
        
        var objectiveId = WorkoutObjectiveId.New();
        var expectedDto = new ReferenceDataDto 
        { 
            Id = objectiveId.ToString(), 
            Value = "Muscle Gain", 
            Description = "Build muscle mass" 
        };
        
        autoMocker.GetMock<IWorkoutObjectiveDataService>()
            .Setup(x => x.GetByIdAsync(objectiveId))
            .ReturnsAsync(ServiceResult<ReferenceDataDto>.Success(expectedDto));
        
        autoMocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());
        
        // Act
        var result = await testee.GetByIdAsync(objectiveId);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedDto);
        result.Errors.Should().BeEmpty();
        
        autoMocker.GetMock<IWorkoutObjectiveDataService>()
            .Verify(x => x.GetByIdAsync(objectiveId), Times.Once);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithEmptyId_ReturnsValidationFailure()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<WorkoutObjectiveService>();
        
        var emptyId = WorkoutObjectiveId.Empty;
        
        // Act
        var result = await testee.GetByIdAsync(emptyId);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(WorkoutObjectiveErrorMessages.InvalidIdFormat);
        
        autoMocker.GetMock<IWorkoutObjectiveDataService>()
            .Verify(x => x.GetByIdAsync(It.IsAny<WorkoutObjectiveId>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByValueAsync_WithValidValue_ReturnsSuccessWithData()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<WorkoutObjectiveService>();
        
        var value = "Muscle Gain";
        var expectedDto = new ReferenceDataDto 
        { 
            Id = "workoutobjective-guid1", 
            Value = value, 
            Description = "Build muscle mass" 
        };
        
        autoMocker.GetMock<IWorkoutObjectiveDataService>()
            .Setup(x => x.GetByValueAsync(value))
            .ReturnsAsync(ServiceResult<ReferenceDataDto>.Success(expectedDto));
        
        autoMocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());
        
        // Act
        var result = await testee.GetByValueAsync(value);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedDto);
        result.Data.Value.Should().Be(value);
        
        autoMocker.GetMock<IWorkoutObjectiveDataService>()
            .Verify(x => x.GetByValueAsync(value), Times.Once);
    }
    
    [Fact]
    public async Task GetByValueAsync_WithEmptyValue_ReturnsValidationFailure()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<WorkoutObjectiveService>();
        
        var emptyValue = "";
        
        // Act
        var result = await testee.GetByValueAsync(emptyValue);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(WorkoutObjectiveErrorMessages.ValueCannotBeEmpty);
        
        autoMocker.GetMock<IWorkoutObjectiveDataService>()
            .Verify(x => x.GetByValueAsync(It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task ExistsAsync_WhenObjectiveExists_ReturnsTrue()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<WorkoutObjectiveService>();
        
        var objectiveId = WorkoutObjectiveId.New();
        var expectedDto = new ReferenceDataDto 
        { 
            Id = objectiveId.ToString(), 
            Value = "Muscle Gain", 
            Description = "Build muscle mass" 
        };
        
        // ExistsAsync calls GetByIdAsync internally, so we need to mock GetByIdAsync
        autoMocker.GetMock<IWorkoutObjectiveDataService>()
            .Setup(x => x.GetByIdAsync(objectiveId))
            .ReturnsAsync(ServiceResult<ReferenceDataDto>.Success(expectedDto));
        
        autoMocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());
        
        // Act
        var result = await testee.ExistsAsync(objectiveId);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeTrue();
        
        autoMocker.GetMock<IWorkoutObjectiveDataService>()
            .Verify(x => x.GetByIdAsync(objectiveId), Times.Once);
    }
    
    [Fact]
    public async Task ExistsAsync_WhenObjectiveDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<WorkoutObjectiveService>();
        
        var objectiveId = WorkoutObjectiveId.New();
        
        // ExistsAsync calls GetByIdAsync internally, so we need to mock GetByIdAsync to return Empty
        autoMocker.GetMock<IWorkoutObjectiveDataService>()
            .Setup(x => x.GetByIdAsync(objectiveId))
            .ReturnsAsync(ServiceResult<ReferenceDataDto>.Success(ReferenceDataDto.Empty));
        
        autoMocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());
        
        // Act
        var result = await testee.ExistsAsync(objectiveId);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeFalse();
        
        autoMocker.GetMock<IWorkoutObjectiveDataService>()
            .Verify(x => x.GetByIdAsync(objectiveId), Times.Once);
    }
    
    [Fact]
    public async Task ExistsAsync_WithEmptyId_ReturnsValidationFailure()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<WorkoutObjectiveService>();
        
        var emptyId = WorkoutObjectiveId.Empty;
        
        // Act
        var result = await testee.ExistsAsync(emptyId);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(WorkoutObjectiveErrorMessages.InvalidIdFormat);
        
        autoMocker.GetMock<IWorkoutObjectiveDataService>()
            .Verify(x => x.ExistsAsync(It.IsAny<WorkoutObjectiveId>()), Times.Never);
    }
}