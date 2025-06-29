using System;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Models.Entities;

public class MuscleGroupTests
{
    [Fact]
    public void CreateNew_ShouldCreateMuscleGroupWithCorrectProperties()
    {
        // Arrange
        var name = "Biceps";
        var bodyPartId = BodyPartId.New();
        
        // Act
        var muscleGroup = MuscleGroup.Handler.CreateNew(name, bodyPartId);
        
        // Assert
        Assert.NotNull(muscleGroup);
        Assert.NotEqual(default(MuscleGroupId), muscleGroup.Id);
        Assert.Equal(name, muscleGroup.Name);
        Assert.Equal(bodyPartId, muscleGroup.BodyPartId);
        Assert.True(muscleGroup.IsActive);
        Assert.True(Math.Abs((muscleGroup.CreatedAt - DateTime.UtcNow).TotalSeconds) < 1);
        Assert.Null(muscleGroup.UpdatedAt);
    }
    
    [Fact]
    public void CreateNew_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var bodyPartId = BodyPartId.New();
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            MuscleGroup.Handler.CreateNew(string.Empty, bodyPartId));
        Assert.Contains("Name cannot be empty", exception.Message);
        Assert.Equal("name", exception.ParamName);
    }
    
    [Fact]
    public void CreateNew_WithNullName_ShouldThrowArgumentException()
    {
        // Arrange
        var bodyPartId = BodyPartId.New();
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            MuscleGroup.Handler.CreateNew(null!, bodyPartId));
        Assert.Contains("Name cannot be empty", exception.Message);
        Assert.Equal("name", exception.ParamName);
    }
    
    [Fact]
    public void Create_ShouldCreateMuscleGroupWithProvidedValues()
    {
        // Arrange
        var id = MuscleGroupId.New();
        var name = "Triceps";
        var bodyPartId = BodyPartId.New();
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var updatedAt = DateTime.UtcNow;
        
        // Act
        var muscleGroup = MuscleGroup.Handler.Create(id, name, bodyPartId, false, createdAt, updatedAt);
        
        // Assert
        Assert.NotNull(muscleGroup);
        Assert.Equal(id, muscleGroup.Id);
        Assert.Equal(name, muscleGroup.Name);
        Assert.Equal(bodyPartId, muscleGroup.BodyPartId);
        Assert.False(muscleGroup.IsActive);
        Assert.Equal(createdAt, muscleGroup.CreatedAt);
        Assert.Equal(updatedAt, muscleGroup.UpdatedAt);
    }
    
    [Fact]
    public void Create_WithDefaultValues_ShouldUseDefaults()
    {
        // Arrange
        var id = MuscleGroupId.New();
        var name = "Quadriceps";
        var bodyPartId = BodyPartId.New();
        
        // Act
        var muscleGroup = MuscleGroup.Handler.Create(id, name, bodyPartId);
        
        // Assert
        Assert.NotNull(muscleGroup);
        Assert.Equal(id, muscleGroup.Id);
        Assert.Equal(name, muscleGroup.Name);
        Assert.Equal(bodyPartId, muscleGroup.BodyPartId);
        Assert.True(muscleGroup.IsActive);
        Assert.True(Math.Abs((muscleGroup.CreatedAt - DateTime.UtcNow).TotalSeconds) < 1);
        Assert.Null(muscleGroup.UpdatedAt);
    }
    
    [Fact]
    public void Update_ShouldUpdateNameBodyPartAndUpdatedAt()
    {
        // Arrange
        var existing = MuscleGroup.Handler.CreateNew("Biceps", BodyPartId.New());
        var newName = "Biceps Brachii";
        var newBodyPartId = BodyPartId.New();
        
        // Act
        var updated = MuscleGroup.Handler.Update(existing, newName, newBodyPartId);
        
        // Assert
        Assert.NotNull(updated);
        Assert.Equal(existing.Id, updated.Id);
        Assert.Equal(newName, updated.Name);
        Assert.Equal(newBodyPartId, updated.BodyPartId);
        Assert.Equal(existing.IsActive, updated.IsActive);
        Assert.Equal(existing.CreatedAt, updated.CreatedAt);
        Assert.NotNull(updated.UpdatedAt);
        Assert.True(Math.Abs((updated.UpdatedAt!.Value - DateTime.UtcNow).TotalSeconds) < 1);
    }
    
    [Fact]
    public void Update_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var existing = MuscleGroup.Handler.CreateNew("Biceps", BodyPartId.New());
        var newBodyPartId = BodyPartId.New();
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            MuscleGroup.Handler.Update(existing, string.Empty, newBodyPartId));
        Assert.Contains("Name cannot be empty", exception.Message);
        Assert.Equal("name", exception.ParamName);
    }
    
    [Fact]
    public void Update_WithNullName_ShouldThrowArgumentException()
    {
        // Arrange
        var existing = MuscleGroup.Handler.CreateNew("Biceps", BodyPartId.New());
        var newBodyPartId = BodyPartId.New();
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            MuscleGroup.Handler.Update(existing, null!, newBodyPartId));
        Assert.Contains("Name cannot be empty", exception.Message);
        Assert.Equal("name", exception.ParamName);
    }
    
    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalseAndUpdateTimestamp()
    {
        // Arrange
        var existing = MuscleGroup.Handler.CreateNew("Biceps", BodyPartId.New());
        
        // Act
        var deactivated = MuscleGroup.Handler.Deactivate(existing);
        
        // Assert
        Assert.NotNull(deactivated);
        Assert.Equal(existing.Id, deactivated.Id);
        Assert.Equal(existing.Name, deactivated.Name);
        Assert.Equal(existing.BodyPartId, deactivated.BodyPartId);
        Assert.False(deactivated.IsActive);
        Assert.Equal(existing.CreatedAt, deactivated.CreatedAt);
        Assert.NotNull(deactivated.UpdatedAt);
        Assert.True(Math.Abs((deactivated.UpdatedAt!.Value - DateTime.UtcNow).TotalSeconds) < 1);
    }
    
    [Fact]
    public void MuscleGroup_ShouldHaveNavigationProperties()
    {
        // Arrange & Act
        var muscleGroup = MuscleGroup.Handler.CreateNew("Biceps", BodyPartId.New());
        
        // Assert
        Assert.Null(muscleGroup.BodyPart);
        Assert.NotNull(muscleGroup.Exercises);
        Assert.Empty(muscleGroup.Exercises);
    }
}