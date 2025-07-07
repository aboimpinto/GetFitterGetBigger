using System;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Models.Entities;

public class ExerciseTests
{
    [Fact]
    public void CreateNew_WithKineticChainId_ShouldSetKineticChainId()
    {
        // Arrange
        var kineticChainId = KineticChainTypeId.New();
        var difficultyId = DifficultyLevelId.New();
        
        // Act
        var exercise = Exercise.Handler.CreateNew(
            "Test Exercise",
            "Test Description",
            "https://example.com/video",
            "https://example.com/image",
            true,
            difficultyId,
            kineticChainId);
        
        // Assert
        Assert.Equal(kineticChainId, exercise.KineticChainId);
    }
    
    [Fact]
    public void CreateNew_WithoutKineticChainId_ShouldHaveNullKineticChainId()
    {
        // Arrange
        var difficultyId = DifficultyLevelId.New();
        
        // Act
        var exercise = Exercise.Handler.CreateNew(
            "Test Exercise",
            "Test Description",
            "https://example.com/video",
            "https://example.com/image",
            true,
            difficultyId);
        
        // Assert
        Assert.Null(exercise.KineticChainId);
    }
    
    [Fact]
    public void Create_WithKineticChainId_ShouldSetKineticChainId()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var kineticChainId = KineticChainTypeId.New();
        var difficultyId = DifficultyLevelId.New();
        
        // Act
        var exercise = Exercise.Handler.Create(
            exerciseId,
            "Test Exercise",
            "Test Description",
            "https://example.com/video",
            "https://example.com/image",
            true,
            true,
            difficultyId,
            kineticChainId);
        
        // Assert
        Assert.Equal(kineticChainId, exercise.KineticChainId);
    }
    
    [Fact]
    public void Create_WithoutKineticChainId_ShouldHaveNullKineticChainId()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var difficultyId = DifficultyLevelId.New();
        
        // Act
        var exercise = Exercise.Handler.Create(
            exerciseId,
            "Test Exercise",
            "Test Description",
            "https://example.com/video",
            "https://example.com/image",
            true,
            true,
            difficultyId);
        
        // Assert
        Assert.Null(exercise.KineticChainId);
    }
    
    [Fact]
    public void Exercise_ShouldHaveKineticChainNavigationProperty()
    {
        // Arrange
        var exercise = Exercise.Handler.CreateNew(
            "Test Exercise",
            "Test Description",
            null,
            null,
            false,
            DifficultyLevelId.New());
        
        // Assert
        Assert.Null(exercise.KineticChain);
        Assert.NotNull(exercise.Id);
    }
}