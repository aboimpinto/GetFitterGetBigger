using System;
using System.Linq;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Models.Entities;

public class WorkoutStateTests
{
    [Fact]
    public void CreateNew_ValidInput_CreatesWorkoutState()
    {
        // Arrange
        var value = "DRAFT";
        var description = "Template under construction";
        var displayOrder = 1;
        
        // Act
        var result = WorkoutState.Handler.CreateNew(value, description, displayOrder);
        
        // Assert
        Assert.True(result.IsSuccess);
        var workoutState = result.Value;
        Assert.NotEqual(default(WorkoutStateId), workoutState.WorkoutStateId);
        Assert.Equal(value, workoutState.Value);
        Assert.Equal(description, workoutState.Description);
        Assert.Equal(displayOrder, workoutState.DisplayOrder);
        Assert.True(workoutState.IsActive);
    }
    
    [Fact]
    public void CreateNew_WithIsActiveFalse_CreatesInactiveWorkoutState()
    {
        // Arrange
        var value = "DEPRECATED";
        var description = "No longer used";
        var displayOrder = 99;
        var isActive = false;
        
        // Act
        var result = WorkoutState.Handler.CreateNew(value, description, displayOrder, isActive);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsActive);
    }
    
    [Fact]
    public void Create_ValidInput_CreatesWorkoutStateWithSpecificId()
    {
        // Arrange
        var id = WorkoutStateId.New();
        var value = "PRODUCTION";
        var description = "Active template for use";
        var displayOrder = 2;
        var isActive = true;
        
        // Act
        var result = WorkoutState.Handler.Create(id, value, description, displayOrder, isActive);
        
        // Assert
        Assert.True(result.IsSuccess);
        var workoutState = result.Value;
        Assert.Equal(id, workoutState.WorkoutStateId);
        Assert.Equal(value, workoutState.Value);
        Assert.Equal(description, workoutState.Description);
        Assert.Equal(displayOrder, workoutState.DisplayOrder);
        Assert.Equal(isActive, workoutState.IsActive);
    }
    
    [Theory]
    [InlineData("")]
    public void CreateNew_EmptyValue_ReturnsFailure(string value)
    {
        // Arrange
        var description = "Description";
        var displayOrder = 1;
        
        // Act
        var result = WorkoutState.Handler.CreateNew(value, description, displayOrder);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Workout state value cannot be empty", result.Errors.First());
    }
    
    [Fact]
    public void CreateNew_NullValue_ReturnsFailure()
    {
        // Arrange
        string? value = null;
        var description = "Description";
        var displayOrder = 1;
        
        // Act
        var result = WorkoutState.Handler.CreateNew(value!, description, displayOrder);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Workout state value cannot be empty", result.Errors.First());
    }
    
    [Fact]
    public void CreateNew_NegativeDisplayOrder_ReturnsFailure()
    {
        // Arrange
        var value = "DRAFT";
        var description = "Description";
        var displayOrder = -1;
        
        // Act
        var result = WorkoutState.Handler.CreateNew(value, description, displayOrder);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Display order must be non-negative", result.Errors.First());
    }
    
    [Fact]
    public void Empty_ReturnsEmptyInstance()
    {
        // Act
        var empty = WorkoutState.Empty;
        
        // Assert
        Assert.True(empty.IsEmpty);
        Assert.Equal(WorkoutStateId.Empty, empty.WorkoutStateId);
        Assert.Equal(string.Empty, empty.Value);
        Assert.Null(empty.Description);
        Assert.Equal(0, empty.DisplayOrder);
        Assert.False(empty.IsActive);
    }
    
    [Fact]
    public void GetCacheStrategy_ReturnsEternal()
    {
        // Arrange
        var result = WorkoutState.Handler.CreateNew("DRAFT", "Description", 1);
        var workoutState = result.Value;
        
        // Act
        var cacheStrategy = workoutState.GetCacheStrategy();
        
        // Assert
        Assert.Equal(CacheStrategy.Eternal, cacheStrategy);
    }
    
    [Fact]
    public void GetCacheDuration_ReturnsNull()
    {
        // Arrange
        var result = WorkoutState.Handler.CreateNew("DRAFT", "Description", 1);
        var workoutState = result.Value;
        
        // Act
        var cacheDuration = workoutState.GetCacheDuration();
        
        // Assert
        Assert.Null(cacheDuration);
    }
    
    [Fact]
    public void Id_ReturnsWorkoutStateIdAsString()
    {
        // Arrange
        var workoutStateId = WorkoutStateId.New();
        var result = WorkoutState.Handler.Create(workoutStateId, "DRAFT", "Description", 1);
        var workoutState = result.Value;
        
        // Act
        var id = workoutState.Id;
        
        // Assert
        Assert.Equal(workoutStateId.ToString(), id);
    }
}