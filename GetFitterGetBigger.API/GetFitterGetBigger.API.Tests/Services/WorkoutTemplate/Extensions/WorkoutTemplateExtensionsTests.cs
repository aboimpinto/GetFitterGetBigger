using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Extensions;
using GetFitterGetBigger.API.Tests.TestBuilders.Domain;
using Moq.AutoMock;
using WorkoutTemplateEntity = GetFitterGetBigger.API.Models.Entities.WorkoutTemplate;

namespace GetFitterGetBigger.API.Tests.Services.WorkoutTemplate.Extensions;

/// <summary>
/// Unit tests for WorkoutTemplateExtensions methods
/// These tests are critical to reduce the Crap Score by providing comprehensive coverage
/// </summary>
public class WorkoutTemplateExtensionsTests
{
    #region ToDto Tests (WorkoutTemplate)

    [Fact]
    public void ToDto_WhenWorkoutTemplateIsEmpty_ShouldReturnEmptyDto()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var workoutTemplate = WorkoutTemplateEntity.Empty;

        // Act
        var result = workoutTemplate.ToDto();

        // Assert
        result.Should().BeEquivalentTo(WorkoutTemplateDto.Empty);
    }

    [Fact]
    public void ToDto_WhenValidWorkoutTemplate_ShouldMapAllProperties()
    {
        // Arrange
        var category = WorkoutCategoryTestBuilder.UpperBodyPush().Build();
        var difficulty = DifficultyLevelTestBuilder.Beginner().Build();
        var workoutState = WorkoutStateTestBuilder.Draft().Build();
        
        var workoutTemplate = new WorkoutTemplateBuilder()
            .WithName("Test Workout")
            .WithDescription("Test Description")
            .WithCategory(category)
            .WithDifficulty(difficulty)
            .WithEstimatedDuration(45)
            .WithTags("strength", "cardio")
            .WithWorkoutState(workoutState)
            .Build();

        // Act
        var result = workoutTemplate.ToDto();

        // Assert
        result.Should().NotBe(WorkoutTemplateDto.Empty);
        result.Id.Should().Be(workoutTemplate.Id.ToString());
        result.Name.Should().Be("Test Workout");
        result.Description.Should().Be("Test Description");
        result.Category.Should().NotBeNull();
        result.Difficulty.Should().NotBeNull();
        result.EstimatedDurationMinutes.Should().Be(45);
        result.Tags.Should().BeEquivalentTo(new[] { "strength", "cardio" });
        result.IsPublic.Should().BeTrue();
        result.WorkoutState.Should().NotBeNull();
        result.Objectives.Should().NotBeNull();
        result.Exercises.Should().NotBeNull();
        result.CreatedAt.Should().Be(workoutTemplate.CreatedAt);
        result.UpdatedAt.Should().Be(workoutTemplate.UpdatedAt);
    }

    [Fact]
    public void ToDto_WhenWorkoutTemplateHasObjectives_ShouldMapObjectives()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var workoutTemplateId = WorkoutTemplateId.New();
        
        var objective1 = WorkoutObjectiveTestBuilder.MuscularStrength().WithValue("Strength").Build();
        var objective2 = WorkoutObjectiveTestBuilder.MuscularEndurance().WithValue("Cardio").Build();
        
        var workoutTemplateObjective1 = WorkoutTemplateObjective.Handler.Create(workoutTemplateId, objective1.WorkoutObjectiveId).Value 
            with { WorkoutObjective = objective1 };
        var workoutTemplateObjective2 = WorkoutTemplateObjective.Handler.Create(workoutTemplateId, objective2.WorkoutObjectiveId).Value 
            with { WorkoutObjective = objective2 };

        var workoutTemplate = new WorkoutTemplateBuilder()
            .WithId(workoutTemplateId)
            .WithObjectives(workoutTemplateObjective1, workoutTemplateObjective2)
            .Build();

        // Act
        var result = workoutTemplate.ToDto();

        // Assert
        result.Objectives.Should().HaveCount(2);
        result.Objectives.Should().Contain(o => o.Value == "Strength");
        result.Objectives.Should().Contain(o => o.Value == "Cardio");
    }

    [Fact]
    public void ToDto_WhenWorkoutTemplateHasExercises_ShouldMapExercises()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var exercise = new ExerciseBuilder().WithName("Push Up").Build();
        
        var workoutExercise = new WorkoutTemplateExerciseBuilder()
            .WithExercise(exercise)
            .WithSequenceOrder(1)
            .BuildWithNavigationProperties();

        var workoutTemplate = new WorkoutTemplateBuilder()
            .WithExercises(workoutExercise)
            .Build();

        // Act
        var result = workoutTemplate.ToDto();

        // Assert
        result.Exercises.Should().HaveCount(1);
        result.Exercises.First().Exercise.Name.Should().Be("Push Up");
        result.Exercises.First().SequenceOrder.Should().Be(1);
    }

    #endregion

    #region ToDto Tests (WorkoutTemplateExercise)

    [Fact]
    public void ToDto_WhenWorkoutTemplateExerciseIsEmpty_ShouldReturnEmptyDto()
    {
        // Arrange
        var exercise = WorkoutTemplateExercise.Empty;

        // Act
        var result = exercise.ToDto();

        // Assert
        result.Should().BeEquivalentTo(new WorkoutTemplateExerciseDto());
    }

    [Fact]
    public void ToDto_WhenValidWorkoutTemplateExercise_ShouldMapAllProperties()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var exercise = new ExerciseBuilder().WithName("Bench Press").Build();
        var setConfig = new SetConfigurationBuilder().Build();
        
        var workoutExercise = new WorkoutTemplateExerciseBuilder()
            .WithExercise(exercise)
            .WithZone(WorkoutZone.Main)
            .WithSequenceOrder(2)
            .WithNotes("Test notes")
            .WithSetConfigurations(setConfig)
            .BuildWithNavigationProperties();

        // Act
        var result = workoutExercise.ToDto();

        // Assert
        result.Id.Should().Be(workoutExercise.Id.ToString());
        result.Exercise.Should().NotBe(ExerciseDto.Empty);
        result.Exercise.Name.Should().Be("Bench Press");
        result.Zone.Should().Be(WorkoutZone.Main.ToString());
        result.SequenceOrder.Should().Be(2);
        result.Notes.Should().Be("Test notes");
        result.SetConfigurations.Should().HaveCount(1);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ToDto_WhenWorkoutTemplateExerciseHasNoExercise_ShouldReturnEmptyExerciseDto()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var workoutExercise = new WorkoutTemplateExerciseBuilder()
            .WithExercise(null)
            .BuildWithNavigationProperties();

        // Act
        var result = workoutExercise.ToDto();

        // Assert
        result.Exercise.IsEmpty.Should().BeTrue();
        result.Exercise.Id.Should().BeEmpty();
        result.Exercise.Name.Should().BeEmpty();
    }

    #endregion

    #region ToDto Tests (SetConfiguration)

    [Fact]
    public void ToDto_WhenSetConfigurationIsEmpty_ShouldReturnEmptyDto()
    {
        // Arrange
        var config = SetConfiguration.Empty;

        // Act
        var result = config.ToDto();

        // Assert
        result.Should().BeEquivalentTo(new SetConfigurationDto());
    }

    [Fact]
    public void ToDto_WhenValidSetConfiguration_ShouldMapAllProperties()
    {
        // Arrange
        var config = new SetConfigurationBuilder()
            .WithSetNumber(1)
            .WithTargetReps("10")
            .WithTargetWeight(100.5m)
            .WithTargetTimeSeconds(30)
            .WithRestSeconds(60)
            .Build();

        // Act
        var result = config.ToDto();

        // Assert
        result.Id.Should().Be(config.Id.ToString());
        result.SetNumber.Should().Be(1);
        result.TargetReps.Should().Be("10");
        result.TargetWeight.Should().Be(100.5m);
        result.TargetTime.Should().Be(30);
        result.RestSeconds.Should().Be(60);
        result.Notes.Should().BeNull();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region ToReferenceDataDto Tests - Main Method

    [Fact]
    public void ToReferenceDataDto_WhenEntityIsNull_ShouldReturnEmptyDto()
    {
        // Arrange
        WorkoutCategory? entity = null;

        // Act
        var result = entity.ToReferenceDataDto();

        // Assert
        result.Should().Be(ReferenceDataDto.Empty);
    }

    [Fact]
    public void ToReferenceDataDto_WhenEntityIsEmpty_ShouldReturnEmptyDto()
    {
        // Arrange
        var entity = WorkoutCategory.Empty;

        // Act
        var result = entity.ToReferenceDataDto();

        // Assert
        result.Should().Be(ReferenceDataDto.Empty);
    }

    [Fact]
    public void ToReferenceDataDto_WhenValidEntityWithValueProperty_ShouldMapCorrectly()
    {
        // Arrange
        var entity = WorkoutCategoryTestBuilder.UpperBodyPush()
            .WithValue("Strength Training")
            .WithDescription("Strength focused workout")
            .Build();

        // Act
        var result = entity.ToReferenceDataDto();

        // Assert
        result.Should().NotBe(ReferenceDataDto.Empty);
        result.Id.Should().Be(entity.Id.ToString());
        result.Value.Should().Be("Strength Training");
        result.Description.Should().Be("Strength focused workout");
    }

    [Fact]
    public void ToReferenceDataDto_WhenDifficultyLevelEntity_ShouldMapCorrectly()
    {
        // Arrange - Using DifficultyLevel entity
        var entity = DifficultyLevelTestBuilder.Beginner()
            .WithValue("Beginner")
            .WithDescription("Suitable for beginners")
            .Build();

        // Act
        var result = entity.ToReferenceDataDto();

        // Assert
        result.Should().NotBe(ReferenceDataDto.Empty);
        result.Id.Should().Be(entity.Id.ToString());
        result.Value.Should().Be("Beginner");
        result.Description.Should().Be("Suitable for beginners");
    }

    [Fact]
    public void ToReferenceDataDto_WhenWorkoutStateEntity_ShouldMapCorrectly()
    {
        // Arrange
        var entity = WorkoutStateTestBuilder.Draft()
            .WithValue("Draft")
            .WithDescription("Draft state")
            .Build();

        // Act
        var result = entity.ToReferenceDataDto();

        // Assert
        result.Should().NotBe(ReferenceDataDto.Empty);
        result.Id.Should().Be(entity.Id.ToString());
        result.Value.Should().Be("Draft");
        result.Description.Should().Be("Draft state");
    }

    #endregion

    #region ToReferenceDataDto Edge Cases

    [Fact]
    public void ToReferenceDataDto_WhenExecutionProtocolEntity_ShouldMapCorrectly()
    {
        // Arrange
        var entity = ExecutionProtocolTestBuilder.Standard()
            .WithValue("STANDARD_PROTOCOL")
            .WithDescription("Standard execution protocol")
            .Build();

        // Act
        var result = entity.ToReferenceDataDto();

        // Assert
        result.Should().NotBe(ReferenceDataDto.Empty);
        result.Id.Should().Be(entity.Id.ToString());
        result.Value.Should().Be("STANDARD_PROTOCOL");
        result.Description.Should().Be("Standard execution protocol");
    }

    [Fact]
    public void ToReferenceDataDto_WhenWorkoutObjectiveEntity_ShouldMapCorrectly()
    {
        // Arrange
        var entity = WorkoutObjectiveTestBuilder.MuscularStrength()
            .WithValue("MUSCULAR_STRENGTH")
            .WithDescription("Muscular strength objective")
            .Build();

        // Act
        var result = entity.ToReferenceDataDto();

        // Assert
        result.Should().NotBe(ReferenceDataDto.Empty);
        result.Id.Should().Be(entity.Id.ToString());
        result.Value.Should().Be("MUSCULAR_STRENGTH");
        result.Description.Should().Be("Muscular strength objective");
    }

    [Fact]
    public void ToReferenceDataDto_WhenWorkoutCategoryWithNullDescription_ShouldHandleGracefully()
    {
        // Arrange
        var entity = WorkoutCategoryTestBuilder.UpperBodyPush()
            .WithValue("Upper Body Push")
            .WithDescription(null)
            .Build();

        // Act
        var result = entity.ToReferenceDataDto();

        // Assert
        result.Should().NotBe(ReferenceDataDto.Empty);
        result.Id.Should().Be(entity.Id.ToString());
        result.Value.Should().Be("Upper Body Push");
        result.Description.Should().BeNull();
    }

    [Fact]
    public void ToReferenceDataDto_WhenDifficultyLevelWithAllProperties_ShouldMapAllFields()
    {
        // Arrange
        var entity = DifficultyLevelTestBuilder.Advanced()
            .WithValue("Advanced")
            .WithDescription("Advanced difficulty level")
            .Build();

        // Act
        var result = entity.ToReferenceDataDto();

        // Assert
        result.Should().NotBe(ReferenceDataDto.Empty);
        result.Id.Should().Be(entity.Id.ToString());
        result.Value.Should().Be("Advanced");
        result.Description.Should().Be("Advanced difficulty level");
    }

    [Fact]
    public void ToReferenceDataDto_WhenWorkoutStateEmpty_ShouldReturnEmptyDto()
    {
        // Arrange
        var entity = WorkoutState.Empty;

        // Act
        var result = entity.ToReferenceDataDto();

        // Assert
        result.Should().Be(ReferenceDataDto.Empty);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void ToDto_CompleteWorkoutTemplateMapping_ShouldHandleAllNestedObjects()
    {
        // Arrange - Create a complete workout template with all nested objects
        var autoMocker = new AutoMocker();
        var workoutTemplateId = WorkoutTemplateId.New();
        
        var category = WorkoutCategoryTestBuilder.UpperBodyPush().WithValue("Strength").Build();
        var difficulty = DifficultyLevelTestBuilder.Intermediate().WithValue("Intermediate").Build();
        var workoutState = WorkoutStateTestBuilder.Production().WithValue("Active").Build();
        var objective = WorkoutObjectiveTestBuilder.MuscularHypertrophy().WithValue("Build Muscle").Build();
        var exercise = new ExerciseBuilder().WithName("Squat").Build();
        var setConfig = new SetConfigurationBuilder().WithSetNumber(3).WithTargetReps("12").Build();
        
        var workoutTemplateObjective = WorkoutTemplateObjective.Handler.Create(workoutTemplateId, objective.WorkoutObjectiveId).Value 
            with { WorkoutObjective = objective };
        
        var workoutExercise = new WorkoutTemplateExerciseBuilder()
            .WithExercise(exercise)
            .WithSetConfigurations(setConfig)
            .BuildWithNavigationProperties();

        var workoutTemplate = new WorkoutTemplateBuilder()
            .WithId(workoutTemplateId)
            .WithName("Full Body Strength")
            .WithCategory(category)
            .WithDifficulty(difficulty)
            .WithWorkoutState(workoutState)
            .WithObjectives(workoutTemplateObjective)
            .WithExercises(workoutExercise)
            .Build();

        // Act
        var result = workoutTemplate.ToDto();

        // Assert
        result.Name.Should().Be("Full Body Strength");
        result.Category.Value.Should().Be("Strength");
        result.Difficulty.Value.Should().Be("Intermediate");
        result.WorkoutState.Value.Should().Be("Active");
        result.Objectives.Should().HaveCount(1);
        result.Objectives.First().Value.Should().Be("Build Muscle");
        result.Exercises.Should().HaveCount(1);
        result.Exercises.First().Exercise.Name.Should().Be("Squat");
        result.Exercises.First().SetConfigurations.Should().HaveCount(1);
        result.Exercises.First().SetConfigurations.First().SetNumber.Should().Be(3);
        result.Exercises.First().SetConfigurations.First().TargetReps.Should().Be("12");
    }

    #endregion

    #region Performance and Memory Tests

    [Fact]
    public void ToReferenceDataDto_WhenCalledMultipleTimesOnSameEntity_ShouldPerformConsistently()
    {
        // Arrange
        var entity = WorkoutCategoryTestBuilder.UpperBodyPush().WithValue("Test").Build();

        // Act
        var result1 = entity.ToReferenceDataDto();
        var result2 = entity.ToReferenceDataDto();
        var result3 = entity.ToReferenceDataDto();

        // Assert
        result1.Should().BeEquivalentTo(result2);
        result2.Should().BeEquivalentTo(result3);
        result1.Value.Should().Be("Test");
    }

    #endregion
}