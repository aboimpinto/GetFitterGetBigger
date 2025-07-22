using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Tests.TestBuilders;
using GetFitterGetBigger.API.Tests.TestHelpers;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Models.Entities;

public class WorkoutTemplateTests
{
    [Fact]
    public void CreateNew_ValidData_CreatesWorkoutTemplate()
    {
        // Arrange
        var name = "Full Body Workout";
        var description = "A comprehensive full body workout";
        var categoryId = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.UpperBodyPush);
        var difficultyId = DifficultyLevelId.ParseOrEmpty(TestIds.DifficultyLevelIds.Intermediate);
        var duration = 60;
        var tags = new List<string> { "strength", "full-body", "intermediate" };
        var createdBy = UserId.ParseOrEmpty(TestIds.UserIds.PersonalTrainer);
        var workoutStateId = WorkoutStateId.ParseOrEmpty(TestIds.WorkoutStateIds.Draft);

        // Act
        var result = WorkoutTemplate.Handler.CreateNew(
            name, description, categoryId, difficultyId, duration, tags, true, createdBy, workoutStateId);

        // Assert
        Assert.True(result.IsSuccess);
        var workoutTemplate = result.Unwrap();
        Assert.False(workoutTemplate.IsEmpty);
        Assert.Equal(name, workoutTemplate.Name);
        Assert.Equal(description, workoutTemplate.Description);
        Assert.Equal(categoryId, workoutTemplate.CategoryId);
        Assert.Equal(difficultyId, workoutTemplate.DifficultyId);
        Assert.Equal(duration, workoutTemplate.EstimatedDurationMinutes);
        Assert.Equal(3, workoutTemplate.Tags.Count);
        Assert.True(workoutTemplate.IsPublic);
        Assert.Equal(createdBy, workoutTemplate.CreatedBy);
        Assert.Equal(workoutStateId, workoutTemplate.WorkoutStateId);
    }

    [Fact]
    public void CreateNew_EmptyName_ReturnsFailure()
    {
        // Arrange
        var categoryId = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.UpperBodyPush);
        var difficultyId = DifficultyLevelId.ParseOrEmpty(TestIds.DifficultyLevelIds.Intermediate);
        var createdBy = UserId.ParseOrEmpty(TestIds.UserIds.PersonalTrainer);
        var workoutStateId = WorkoutStateId.ParseOrEmpty(TestIds.WorkoutStateIds.Draft);

        // Act
        var result = WorkoutTemplate.Handler.CreateNew("", null, categoryId, difficultyId, 60, null, true, createdBy, workoutStateId);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Name cannot be empty", result.FirstError);
    }

    [Fact]
    public void CreateNew_NameTooShort_ReturnsFailure()
    {
        // Arrange
        var categoryId = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.UpperBodyPush);
        var difficultyId = DifficultyLevelId.ParseOrEmpty(TestIds.DifficultyLevelIds.Intermediate);
        var createdBy = UserId.ParseOrEmpty(TestIds.UserIds.PersonalTrainer);
        var workoutStateId = WorkoutStateId.ParseOrEmpty(TestIds.WorkoutStateIds.Draft);

        // Act
        var result = WorkoutTemplate.Handler.CreateNew("AB", null, categoryId, difficultyId, 60, null, true, createdBy, workoutStateId);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Name must be at least 3 characters long", result.FirstError);
    }

    [Fact]
    public void CreateNew_NameTooLong_ReturnsFailure()
    {
        // Arrange
        var longName = new string('A', 101);
        var categoryId = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.UpperBodyPush);
        var difficultyId = DifficultyLevelId.ParseOrEmpty(TestIds.DifficultyLevelIds.Intermediate);
        var createdBy = UserId.ParseOrEmpty(TestIds.UserIds.PersonalTrainer);
        var workoutStateId = WorkoutStateId.ParseOrEmpty(TestIds.WorkoutStateIds.Draft);

        // Act
        var result = WorkoutTemplate.Handler.CreateNew(longName, null, categoryId, difficultyId, 60, null, true, createdBy, workoutStateId);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Name cannot exceed 100 characters", result.FirstError);
    }

    [Fact]
    public void CreateNew_DurationTooShort_ReturnsFailure()
    {
        // Arrange
        var categoryId = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.UpperBodyPush);
        var difficultyId = DifficultyLevelId.ParseOrEmpty(TestIds.DifficultyLevelIds.Intermediate);
        var createdBy = UserId.ParseOrEmpty(TestIds.UserIds.PersonalTrainer);
        var workoutStateId = WorkoutStateId.ParseOrEmpty(TestIds.WorkoutStateIds.Draft);

        // Act
        var result = WorkoutTemplate.Handler.CreateNew("Valid Name", null, categoryId, difficultyId, 4, null, true, createdBy, workoutStateId);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Estimated duration must be between 5 and 300 minutes", result.FirstError);
    }

    [Fact]
    public void CreateNew_DurationTooLong_ReturnsFailure()
    {
        // Arrange
        var categoryId = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.UpperBodyPush);
        var difficultyId = DifficultyLevelId.ParseOrEmpty(TestIds.DifficultyLevelIds.Intermediate);
        var createdBy = UserId.ParseOrEmpty(TestIds.UserIds.PersonalTrainer);
        var workoutStateId = WorkoutStateId.ParseOrEmpty(TestIds.WorkoutStateIds.Draft);

        // Act
        var result = WorkoutTemplate.Handler.CreateNew("Valid Name", null, categoryId, difficultyId, 301, null, true, createdBy, workoutStateId);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Estimated duration must be between 5 and 300 minutes", result.FirstError);
    }

    [Fact]
    public void Update_ValidData_UpdatesWorkoutTemplate()
    {
        // Arrange
        var categoryId = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.UpperBodyPush);
        var difficultyId = DifficultyLevelId.ParseOrEmpty(TestIds.DifficultyLevelIds.Intermediate);
        var createdBy = UserId.ParseOrEmpty(TestIds.UserIds.PersonalTrainer);
        var workoutStateId = WorkoutStateId.ParseOrEmpty(TestIds.WorkoutStateIds.Draft);
        
        var original = WorkoutTemplate.Handler.CreateNew(
            "Original Name", "Original Description", categoryId, difficultyId, 60, null, true, createdBy, workoutStateId).Unwrap();
        
        var newName = "Updated Name";
        var newDescription = "Updated Description";
        var newDuration = 90;

        // Act
        var result = WorkoutTemplate.Handler.Update(original, newName, newDescription, null, null, newDuration, null, false);

        // Assert
        Assert.True(result.IsSuccess);
        var updated = result.Unwrap();
        Assert.Equal(newName, updated.Name);
        Assert.Equal(newDescription, updated.Description);
        Assert.Equal(newDuration, updated.EstimatedDurationMinutes);
        Assert.False(updated.IsPublic);
        Assert.Equal(original.CategoryId, updated.CategoryId);
        Assert.Equal(original.DifficultyId, updated.DifficultyId);
        Assert.True(updated.UpdatedAt > original.UpdatedAt);
    }

    [Fact]
    public void ChangeState_ValidState_ChangesState()
    {
        // Arrange
        var categoryId = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.UpperBodyPush);
        var difficultyId = DifficultyLevelId.ParseOrEmpty(TestIds.DifficultyLevelIds.Intermediate);
        var createdBy = UserId.ParseOrEmpty(TestIds.UserIds.PersonalTrainer);
        var draftStateId = WorkoutStateId.ParseOrEmpty(TestIds.WorkoutStateIds.Draft);
        var productionStateId = WorkoutStateId.ParseOrEmpty(TestIds.WorkoutStateIds.Production);
        
        var template = WorkoutTemplate.Handler.CreateNew(
            "Test Template", null, categoryId, difficultyId, 60, null, true, createdBy, draftStateId).Unwrap();

        // Act
        var result = WorkoutTemplate.Handler.ChangeState(template, productionStateId);

        // Assert
        Assert.True(result.IsSuccess);
        var updated = result.Unwrap();
        Assert.Equal(productionStateId, updated.WorkoutStateId);
        Assert.True(updated.UpdatedAt > template.UpdatedAt);
    }

    [Fact]
    public void Empty_ReturnsEmptyInstance()
    {
        // Act
        var empty = WorkoutTemplate.Empty;

        // Assert
        Assert.True(empty.IsEmpty);
        Assert.Equal(WorkoutTemplateId.Empty, empty.Id);
        Assert.Empty(empty.Name);
        Assert.Equal(WorkoutCategoryId.Empty, empty.CategoryId);
        Assert.Equal(DifficultyLevelId.Empty, empty.DifficultyId);
        Assert.Equal(0, empty.EstimatedDurationMinutes);
        Assert.Empty(empty.Tags);
        Assert.False(empty.IsPublic);
        Assert.Equal(UserId.Empty, empty.CreatedBy);
        Assert.Equal(WorkoutStateId.Empty, empty.WorkoutStateId);
    }

    [Fact]
    public void CreateNew_TagsLimitedTo10_OnlyFirst10TagsKept()
    {
        // Arrange
        var categoryId = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.UpperBodyPush);
        var difficultyId = DifficultyLevelId.ParseOrEmpty(TestIds.DifficultyLevelIds.Intermediate);
        var createdBy = UserId.ParseOrEmpty(TestIds.UserIds.PersonalTrainer);
        var workoutStateId = WorkoutStateId.ParseOrEmpty(TestIds.WorkoutStateIds.Draft);
        var tags = new List<string>();
        for (int i = 0; i < 15; i++)
        {
            tags.Add($"tag{i}");
        }

        // Act
        var result = WorkoutTemplate.Handler.CreateNew(
            "Test Template", null, categoryId, difficultyId, 60, tags, true, createdBy, workoutStateId);

        // Assert
        Assert.True(result.IsSuccess);
        var template = result.Unwrap();
        Assert.Equal(10, template.Tags.Count);
        Assert.Equal("tag0", template.Tags[0]);
        Assert.Equal("tag9", template.Tags[9]);
    }
}