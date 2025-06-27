using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Models.Entities;

public class ExerciseExerciseTypeTests
{
    [Fact]
    public void Create_ValidInput_CreatesExerciseExerciseType()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var exerciseTypeId = ExerciseTypeId.New();
        
        // Act
        var association = ExerciseExerciseType.Handler.Create(exerciseId, exerciseTypeId);
        
        // Assert
        Assert.Equal(exerciseId, association.ExerciseId);
        Assert.Equal(exerciseTypeId, association.ExerciseTypeId);
        Assert.Null(association.Exercise);
        Assert.Null(association.ExerciseType);
    }
    
    [Fact]
    public void Create_MultipleAssociations_CreatesDistinctInstances()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var exerciseTypeId1 = ExerciseTypeId.New();
        var exerciseTypeId2 = ExerciseTypeId.New();
        
        // Act
        var association1 = ExerciseExerciseType.Handler.Create(exerciseId, exerciseTypeId1);
        var association2 = ExerciseExerciseType.Handler.Create(exerciseId, exerciseTypeId2);
        
        // Assert
        Assert.NotEqual(association1, association2);
        Assert.Equal(exerciseId, association1.ExerciseId);
        Assert.Equal(exerciseId, association2.ExerciseId);
        Assert.NotEqual(association1.ExerciseTypeId, association2.ExerciseTypeId);
    }
    
    [Fact]
    public void ExerciseExerciseType_IsRecord_SupportsValueEquality()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var exerciseTypeId = ExerciseTypeId.New();
        
        // Act
        var association1 = ExerciseExerciseType.Handler.Create(exerciseId, exerciseTypeId);
        var association2 = ExerciseExerciseType.Handler.Create(exerciseId, exerciseTypeId);
        
        // Assert
        Assert.Equal(association1, association2);
        Assert.True(association1 == association2);
    }
    
    [Fact]
    public void ExerciseExerciseType_WithDifferentIds_AreNotEqual()
    {
        // Arrange
        var exerciseId1 = ExerciseId.New();
        var exerciseId2 = ExerciseId.New();
        var exerciseTypeId = ExerciseTypeId.New();
        
        // Act
        var association1 = ExerciseExerciseType.Handler.Create(exerciseId1, exerciseTypeId);
        var association2 = ExerciseExerciseType.Handler.Create(exerciseId2, exerciseTypeId);
        
        // Assert
        Assert.NotEqual(association1, association2);
        Assert.False(association1 == association2);
    }
}