using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise.Extensions;
using GetFitterGetBigger.API.Tests.TestBuilders.Domain;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services.WorkoutTemplate.Features.Exercise.Extensions;

public class WorkoutTemplateExerciseExtensionsTests
{
    // No shared state - extension methods are pure functions

    #region ToDto Tests

    [Fact]
    public void ToDto_WhenEntityHasAllProperties_ShouldMapCorrectly()
    {
        // Arrange
        const string exerciseName = "Bench Press";
        const string exerciseDescription = "Chest exercise";
        const string difficultyValue = "Intermediate";
        const string mainZone = "Main";
        const int sequenceOrder = 1;
        const string testNotes = "Focus on form";
        const int setNumber1 = 1;
        const int setNumber2 = 2;
        const string targetReps1 = "10";
        const string targetReps2 = "8";
        const decimal targetWeight1 = 50;
        const decimal targetWeight2 = 60;
        const int restSeconds1 = 60;
        const int restSeconds2 = 90;
        
        var exerciseId = ExerciseId.New();
        var difficulty = DifficultyLevelTestBuilder.Intermediate()
            .WithId(DifficultyLevelId.New())
            .Build();
            
        var exercise = new ExerciseBuilder()
            .WithId(exerciseId)
            .WithName(exerciseName)
            .WithDescription(exerciseDescription)
            .WithDifficulty(difficulty)
            .BuildWithNavigationProperties();
            
        var setConfig1 = new SetConfigurationBuilder()
            .WithSetNumber(setNumber1)
            .WithTargetReps(targetReps1)
            .WithTargetWeight(targetWeight1)
            .WithTargetTimeSeconds(null)
            .WithRestSeconds(restSeconds1)
            .Build();
            
        var setConfig2 = new SetConfigurationBuilder()
            .WithSetNumber(setNumber2)
            .WithTargetReps(targetReps2)
            .WithTargetWeight(targetWeight2)
            .WithTargetTimeSeconds(null)
            .WithRestSeconds(restSeconds2)
            .Build();
            
        var entity = new WorkoutTemplateExerciseBuilder()
            .WithId(WorkoutTemplateExerciseId.New())
            .WithExercise(exercise)
            .WithZone(WorkoutZone.Main)
            .WithSequenceOrder(sequenceOrder)
            .WithNotes(testNotes)
            .WithConfigurations([setConfig1, setConfig2])
            .BuildWithNavigationProperties();

        // Act
        var result = entity.ToDto();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(entity.Id.ToString());
        result.Phase.Should().Be(mainZone);
        result.OrderInRound.Should().Be(sequenceOrder);
        result.Notes.Should().Be(testNotes);
        
        result.Exercise.Should().NotBeNull();
        result.Exercise.Id.Should().Be(exerciseId.ToString());
        result.Exercise.Name.Should().Be(exerciseName);
        result.Exercise.Description.Should().Be(exerciseDescription);
        result.Exercise.Difficulty.Value.Should().Be(difficultyValue);
        
        // SetConfigurations functionality was removed in refactoring - now using Metadata
        result.Metadata.Should().Be("{}");
    }

    [Fact]
    public void ToDto_WhenExerciseIsNull_ShouldReturnEmptyExerciseDto()
    {
        // Arrange
        var entity = new WorkoutTemplateExerciseBuilder()
            .WithId(WorkoutTemplateExerciseId.New())
            .WithExercise(null)
            .WithZone(WorkoutZone.Warmup)
            .BuildWithNavigationProperties();

        // Act
        var result = entity.ToDto();

        // Assert
        result.Should().NotBeNull();
        result.Exercise.Should().NotBeNull();
        result.Exercise.IsEmpty.Should().BeTrue();
        result.Exercise.Id.Should().Be(ExerciseDto.Empty.Id);
        result.Exercise.Name.Should().Be(ExerciseDto.Empty.Name);
    }

    [Fact]
    public void ToDto_WhenConfigurationsEmpty_ShouldReturnEmptyList()
    {
        // Arrange
        var entity = new WorkoutTemplateExerciseBuilder()
            .WithId(WorkoutTemplateExerciseId.New())
            .WithConfigurations([])
            .Build();

        // Act
        var result = entity.ToDto();

        // Assert
        result.Should().NotBeNull();
        // SetConfigurations functionality was removed in refactoring - now using Metadata
        result.Metadata.Should().Be("{}");
    }

    // ToDto_ShouldOrderSetConfigurationsBySetNumber test removed - SetConfigurations functionality was removed in refactoring

    #endregion

    #region OrganizeByRound Tests

    [Fact]
    public void OrganizeByRound_WhenExercisesInDifferentZones_ShouldGroupByZone()
    {
        // Arrange
        const string warmupZone = "Warmup";
        const string mainZone = "Main";
        const int sequenceOrder1 = 1;
        const int sequenceOrder2 = 2;
        
        var warmupExercise1 = new WorkoutTemplateExerciseBuilder()
            .WithZone(WorkoutZone.Warmup)
            .WithSequenceOrder(sequenceOrder1)
            .BuildWithNavigationProperties();
            
        var warmupExercise2 = new WorkoutTemplateExerciseBuilder()
            .WithZone(WorkoutZone.Warmup)
            .WithSequenceOrder(sequenceOrder2)
            .BuildWithNavigationProperties();
            
        var mainExercise = new WorkoutTemplateExerciseBuilder()
            .WithZone(WorkoutZone.Main)
            .WithSequenceOrder(sequenceOrder1)
            .BuildWithNavigationProperties();
            
        var exercises = new List<WorkoutTemplateExercise> { warmupExercise1, mainExercise, warmupExercise2 };

        // Act
        var result = exercises.OrganizeByRound();

        // Assert
        result.Should().HaveCount(2);
        
        var warmupRound = result.FirstOrDefault(r => r.Exercises.Any(e => e.Phase == warmupZone));
        warmupRound.Should().NotBeNull();
        warmupRound!.Exercises.Should().HaveCount(2);
        
        var mainRound = result.FirstOrDefault(r => r.Exercises.Any(e => e.Phase == mainZone));
        mainRound.Should().NotBeNull();
        mainRound!.Exercises.Should().HaveCount(1);
    }

    [Fact]
    public void OrganizeByRound_WhenExercisesInSameZone_ShouldOrderBySequence()
    {
        // Arrange
        const string exerciseName1 = "Exercise 1";
        const string exerciseName2 = "Exercise 2";
        const string exerciseName3 = "Exercise 3";
        const int sequenceOrder1 = 1;
        const int sequenceOrder2 = 2;
        const int sequenceOrder3 = 3;
        
        var exercise2 = new WorkoutTemplateExerciseBuilder()
            .WithZone(WorkoutZone.Main)
            .WithSequenceOrder(sequenceOrder2)
            .WithExercise(new ExerciseBuilder().WithName(exerciseName2).BuildWithNavigationProperties())
            .BuildWithNavigationProperties();
            
        var exercise1 = new WorkoutTemplateExerciseBuilder()
            .WithZone(WorkoutZone.Main)
            .WithSequenceOrder(sequenceOrder1)
            .WithExercise(new ExerciseBuilder().WithName(exerciseName1).BuildWithNavigationProperties())
            .BuildWithNavigationProperties();
            
        var exercise3 = new WorkoutTemplateExerciseBuilder()
            .WithZone(WorkoutZone.Main)
            .WithSequenceOrder(sequenceOrder3)
            .WithExercise(new ExerciseBuilder().WithName(exerciseName3).BuildWithNavigationProperties())
            .BuildWithNavigationProperties();
            
        var exercises = new List<WorkoutTemplateExercise> { exercise2, exercise3, exercise1 };

        // Act
        var result = exercises.OrganizeByRound();

        // Assert
        result.Should().HaveCount(1);
        var round = result.First();
        round.Exercises.Should().HaveCount(3);
        round.Exercises[0].Exercise.Name.Should().Be(exerciseName1);
        round.Exercises[1].Exercise.Name.Should().Be(exerciseName2);
        round.Exercises[2].Exercise.Name.Should().Be(exerciseName3);
    }

    [Fact]
    public void OrganizeByRound_WhenEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        var exercises = new List<WorkoutTemplateExercise>();

        // Act
        var result = exercises.OrganizeByRound();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region IsValidPhase Tests

    [Fact]
    public void IsValidPhase_WhenPhaseIsWarmup_ShouldReturnTrue()
    {
        // Arrange
        const string warmupPhase = "Warmup";

        // Act
        var result = warmupPhase.IsValidPhase();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidPhase_WhenPhaseIsWorkout_ShouldReturnTrue()
    {
        // Arrange
        const string workoutPhase = "Workout";

        // Act
        var result = workoutPhase.IsValidPhase();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidPhase_WhenPhaseIsMain_ShouldReturnTrue()
    {
        // Arrange
        const string mainPhase = "Main";

        // Act
        var result = mainPhase.IsValidPhase();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidPhase_WhenPhaseIsCooldown_ShouldReturnTrue()
    {
        // Arrange
        const string cooldownPhase = "Cooldown";

        // Act
        var result = cooldownPhase.IsValidPhase();

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("warmup")]  // lowercase
    [InlineData("WARMUP")]  // uppercase
    [InlineData("WaRmUp")]  // mixed case
    public void IsValidPhase_WhenPhaseHasDifferentCasing_ShouldReturnTrue(string phase)
    {
        // Act
        var result = phase.IsValidPhase();

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Phase1")]
    public void IsValidPhase_WhenPhaseIsInvalid_ShouldReturnFalse(string phase)
    {
        // Act
        var result = phase.IsValidPhase();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidPhase_WhenPhaseIsNull_ShouldReturnFalse()
    {
        // Arrange
        string? phase = null;

        // Act
        var result = phase!.IsValidPhase(); // Intentionally testing null

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}