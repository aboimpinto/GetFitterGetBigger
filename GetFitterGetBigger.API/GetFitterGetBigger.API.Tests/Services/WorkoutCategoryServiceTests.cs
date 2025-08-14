using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.ReferenceTables.WorkoutCategory;
using GetFitterGetBigger.API.Services.ReferenceTables.WorkoutCategory.DataServices;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services;

/// <summary>
/// Unit tests for WorkoutCategoryService following the new DataService architecture.
/// Uses AutoMocker for isolation and FluentAssertions for clear assertions.
/// </summary>
public class WorkoutCategoryServiceTests
{
    [Fact]
    public async Task GetAllActiveAsync_WhenCacheMiss_CallsDataServiceAndCachesResult()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<WorkoutCategoryService>();
        
        var expectedDtos = new List<WorkoutCategoryDto>
        {
            new() { WorkoutCategoryId = "workoutcategory-guid1", Value = "Strength", Description = "Strength training" },
            new() { WorkoutCategoryId = "workoutcategory-guid2", Value = "Cardio", Description = "Cardiovascular training" }
        };
        
        autoMocker.GetMock<IWorkoutCategoryDataService>()
            .Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(ServiceResult<IEnumerable<WorkoutCategoryDto>>.Success(expectedDtos));
        
        autoMocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<IEnumerable<WorkoutCategoryDto>>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<IEnumerable<WorkoutCategoryDto>>.Miss());
        
        // Act
        var result = await testee.GetAllActiveAsync();
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedDtos);
        
        autoMocker.GetMock<IWorkoutCategoryDataService>()
            .Verify(x => x.GetAllActiveAsync(), Times.Once);
        
        autoMocker.GetMock<IEternalCacheService>()
            .Verify(x => x.SetAsync(It.IsAny<string>(), It.Is<IEnumerable<WorkoutCategoryDto>>(
                data => data.Count() == expectedDtos.Count)), Times.Once);
    }
    
    [Fact]
    public async Task GetAllActiveAsync_WhenCacheHit_ReturnsFromCacheWithoutCallingDataService()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<WorkoutCategoryService>();
        
        var cachedDtos = new List<WorkoutCategoryDto>
        {
            new() { WorkoutCategoryId = "workoutcategory-guid1", Value = "Strength", Description = "Strength training" },
            new() { WorkoutCategoryId = "workoutcategory-guid2", Value = "Cardio", Description = "Cardiovascular training" }
        };
        
        autoMocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<IEnumerable<WorkoutCategoryDto>>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<IEnumerable<WorkoutCategoryDto>>.Hit(cachedDtos));
        
        // Act
        var result = await testee.GetAllActiveAsync();
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(cachedDtos);
        
        autoMocker.GetMock<IWorkoutCategoryDataService>()
            .Verify(x => x.GetAllActiveAsync(), Times.Never);
        
        autoMocker.GetMock<IEternalCacheService>()
            .Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<IEnumerable<WorkoutCategoryDto>>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsSuccessWithData()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<WorkoutCategoryService>();
        
        var categoryId = WorkoutCategoryId.New();
        var expectedDto = new WorkoutCategoryDto 
        { 
            WorkoutCategoryId = categoryId.ToString(), 
            Value = "Strength", 
            Description = "Strength training" 
        };
        
        autoMocker.GetMock<IWorkoutCategoryDataService>()
            .Setup(x => x.GetByIdAsync(categoryId))
            .ReturnsAsync(ServiceResult<WorkoutCategoryDto>.Success(expectedDto));
        
        autoMocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<WorkoutCategoryDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<WorkoutCategoryDto>.Miss());
        
        // Act
        var result = await testee.GetByIdAsync(categoryId);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedDto);
        result.Errors.Should().BeEmpty();
        
        autoMocker.GetMock<IWorkoutCategoryDataService>()
            .Verify(x => x.GetByIdAsync(categoryId), Times.Once);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithEmptyId_ReturnsValidationFailure()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<WorkoutCategoryService>();
        
        var emptyId = WorkoutCategoryId.Empty;
        
        // Act
        var result = await testee.GetByIdAsync(emptyId);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(WorkoutCategoryErrorMessages.InvalidIdFormat);
        
        autoMocker.GetMock<IWorkoutCategoryDataService>()
            .Verify(x => x.GetByIdAsync(It.IsAny<WorkoutCategoryId>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByValueAsync_WithValidValue_ReturnsSuccessWithData()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<WorkoutCategoryService>();
        
        var value = "Strength";
        var expectedDto = new WorkoutCategoryDto 
        { 
            WorkoutCategoryId = "workoutcategory-guid1", 
            Value = value, 
            Description = "Strength training" 
        };
        
        autoMocker.GetMock<IWorkoutCategoryDataService>()
            .Setup(x => x.GetByValueAsync(value))
            .ReturnsAsync(ServiceResult<WorkoutCategoryDto>.Success(expectedDto));
        
        autoMocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<WorkoutCategoryDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<WorkoutCategoryDto>.Miss());
        
        // Act
        var result = await testee.GetByValueAsync(value);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedDto);
        result.Data.Value.Should().Be(value);
        
        autoMocker.GetMock<IWorkoutCategoryDataService>()
            .Verify(x => x.GetByValueAsync(value), Times.Once);
    }
    
    [Fact]
    public async Task GetByValueAsync_WithEmptyValue_ReturnsValidationFailure()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<WorkoutCategoryService>();
        
        var emptyValue = "";
        
        // Act
        var result = await testee.GetByValueAsync(emptyValue);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(WorkoutCategoryErrorMessages.ValueCannotBeEmpty);
        
        autoMocker.GetMock<IWorkoutCategoryDataService>()
            .Verify(x => x.GetByValueAsync(It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task ExistsAsync_WhenCategoryExists_ReturnsTrue()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<WorkoutCategoryService>();
        
        var categoryId = WorkoutCategoryId.New();
        var expectedDto = new WorkoutCategoryDto 
        { 
            WorkoutCategoryId = categoryId.ToString(), 
            Value = "Strength", 
            Description = "Strength training" 
        };
        
        // ExistsAsync calls GetByIdAsync internally, so we need to mock GetByIdAsync
        autoMocker.GetMock<IWorkoutCategoryDataService>()
            .Setup(x => x.GetByIdAsync(categoryId))
            .ReturnsAsync(ServiceResult<WorkoutCategoryDto>.Success(expectedDto));
        
        autoMocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<WorkoutCategoryDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<WorkoutCategoryDto>.Miss());
        
        // Act
        var result = await testee.ExistsAsync(categoryId);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeTrue();
        
        autoMocker.GetMock<IWorkoutCategoryDataService>()
            .Verify(x => x.GetByIdAsync(categoryId), Times.Once);
    }
    
    [Fact]
    public async Task ExistsAsync_WhenCategoryDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<WorkoutCategoryService>();
        
        var categoryId = WorkoutCategoryId.New();
        
        // ExistsAsync calls GetByIdAsync internally, so we need to mock GetByIdAsync to return Empty
        autoMocker.GetMock<IWorkoutCategoryDataService>()
            .Setup(x => x.GetByIdAsync(categoryId))
            .ReturnsAsync(ServiceResult<WorkoutCategoryDto>.Success(WorkoutCategoryDto.Empty));
        
        autoMocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<WorkoutCategoryDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<WorkoutCategoryDto>.Miss());
        
        // Act
        var result = await testee.ExistsAsync(categoryId);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeFalse();
        
        autoMocker.GetMock<IWorkoutCategoryDataService>()
            .Verify(x => x.GetByIdAsync(categoryId), Times.Once);
    }
    
    [Fact]
    public async Task ExistsAsync_WithEmptyId_ReturnsValidationFailure()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<WorkoutCategoryService>();
        
        var emptyId = WorkoutCategoryId.Empty;
        
        // Act
        var result = await testee.ExistsAsync(emptyId);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(WorkoutCategoryErrorMessages.InvalidIdFormat);
        
        autoMocker.GetMock<IWorkoutCategoryDataService>()
            .Verify(x => x.ExistsAsync(It.IsAny<WorkoutCategoryId>()), Times.Never);
    }
}