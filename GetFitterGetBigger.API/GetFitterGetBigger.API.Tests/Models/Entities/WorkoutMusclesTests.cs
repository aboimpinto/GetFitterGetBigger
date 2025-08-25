using FluentAssertions;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Tests.Models.Entities;

/// <summary>
/// Unit tests for WorkoutMuscles entity
/// Focus on validation logic, creation patterns, and parameter handling
/// </summary>
public class WorkoutMusclesTests
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(5, 5)]
    [InlineData(10, 10)]
    [InlineData(3, 7)]
    [InlineData(8, 2)]
    public void Create_ValidParameters_CreatesWorkoutMuscles(int engagementLevel, int loadEstimation)
    {
        // Arrange
        var workoutTemplateId = Guid.NewGuid();
        var muscleGroupId = MuscleGroupId.New();

        // Act
        var result = WorkoutMuscles.Handler.Create(
            workoutTemplateId,
            muscleGroupId,
            engagementLevel,
            loadEstimation);

        // Assert
        result.Should().NotBeNull();
        result.WorkoutTemplateId.Should().Be(workoutTemplateId);
        result.MuscleGroupId.Should().Be(muscleGroupId);
        result.EngagementLevel.Should().Be(engagementLevel);
        result.LoadEstimation.Should().Be(loadEstimation);
        result.IsActive.Should().BeTrue();
        result.Id.Should().NotBe(WorkoutMusclesId.Empty);
    }

    [Fact]
    public void Create_DefaultIsActive_SetsActiveTrue()
    {
        // Arrange
        var workoutTemplateId = Guid.NewGuid();
        var muscleGroupId = MuscleGroupId.New();

        // Act
        var result = WorkoutMuscles.Handler.Create(
            workoutTemplateId,
            muscleGroupId,
            5,
            5);

        // Assert
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_ExplicitIsActiveFalse_SetsActiveFalse()
    {
        // Arrange
        var workoutTemplateId = Guid.NewGuid();
        var muscleGroupId = MuscleGroupId.New();

        // Act
        var result = WorkoutMuscles.Handler.Create(
            workoutTemplateId,
            muscleGroupId,
            5,
            5,
            false);

        // Assert
        result.IsActive.Should().BeFalse();
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(11, 5)]
    [InlineData(-1, 5)]
    [InlineData(100, 5)]
    public void Create_InvalidEngagementLevel_ThrowsArgumentException(int invalidEngagementLevel, int validLoadEstimation)
    {
        // Arrange
        var workoutTemplateId = Guid.NewGuid();
        var muscleGroupId = MuscleGroupId.New();

        // Act & Assert
        var action = () => WorkoutMuscles.Handler.Create(
            workoutTemplateId,
            muscleGroupId,
            invalidEngagementLevel,
            validLoadEstimation);

        action.Should().Throw<ArgumentException>()
            .WithParameterName("engagementLevel")
            .WithMessage("Engagement level must be between 1 and 10*");
    }

    [Theory]
    [InlineData(5, 0)]
    [InlineData(5, 11)]
    [InlineData(5, -1)]
    [InlineData(5, 100)]
    public void Create_InvalidLoadEstimation_ThrowsArgumentException(int validEngagementLevel, int invalidLoadEstimation)
    {
        // Arrange
        var workoutTemplateId = Guid.NewGuid();
        var muscleGroupId = MuscleGroupId.New();

        // Act & Assert
        var action = () => WorkoutMuscles.Handler.Create(
            workoutTemplateId,
            muscleGroupId,
            validEngagementLevel,
            invalidLoadEstimation);

        action.Should().Throw<ArgumentException>()
            .WithParameterName("loadEstimation")
            .WithMessage("Load estimation must be between 1 and 10*");
    }

    [Fact]
    public void CreateWithId_ValidParameters_CreatesWorkoutMusclesWithSpecificId()
    {
        // Arrange
        var id = WorkoutMusclesId.New();
        var workoutTemplateId = Guid.NewGuid();
        var muscleGroupId = MuscleGroupId.New();
        var engagementLevel = 7;
        var loadEstimation = 3;

        // Act
        var result = WorkoutMuscles.Handler.CreateWithId(
            id,
            workoutTemplateId,
            muscleGroupId,
            engagementLevel,
            loadEstimation);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(id);
        result.WorkoutTemplateId.Should().Be(workoutTemplateId);
        result.MuscleGroupId.Should().Be(muscleGroupId);
        result.EngagementLevel.Should().Be(engagementLevel);
        result.LoadEstimation.Should().Be(loadEstimation);
        result.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(11, 5)]
    public void CreateWithId_InvalidEngagementLevel_ThrowsArgumentException(int invalidEngagementLevel, int validLoadEstimation)
    {
        // Arrange
        var id = WorkoutMusclesId.New();
        var workoutTemplateId = Guid.NewGuid();
        var muscleGroupId = MuscleGroupId.New();

        // Act & Assert
        var action = () => WorkoutMuscles.Handler.CreateWithId(
            id,
            workoutTemplateId,
            muscleGroupId,
            invalidEngagementLevel,
            validLoadEstimation);

        action.Should().Throw<ArgumentException>()
            .WithParameterName("engagementLevel");
    }

    [Theory]
    [InlineData(5, 0)]
    [InlineData(5, 11)]
    public void CreateWithId_InvalidLoadEstimation_ThrowsArgumentException(int validEngagementLevel, int invalidLoadEstimation)
    {
        // Arrange
        var id = WorkoutMusclesId.New();
        var workoutTemplateId = Guid.NewGuid();
        var muscleGroupId = MuscleGroupId.New();

        // Act & Assert
        var action = () => WorkoutMuscles.Handler.CreateWithId(
            id,
            workoutTemplateId,
            muscleGroupId,
            validEngagementLevel,
            invalidLoadEstimation);

        action.Should().Throw<ArgumentException>()
            .WithParameterName("loadEstimation");
    }

    [Fact]
    public void Update_ValidParameters_UpdatesWorkoutMuscles()
    {
        // Arrange
        var original = WorkoutMuscles.Handler.Create(
            Guid.NewGuid(),
            MuscleGroupId.New(),
            3,
            4);

        var newEngagementLevel = 8;
        var newLoadEstimation = 6;
        var newIsActive = false;

        // Act
        var result = WorkoutMuscles.Handler.Update(
            original,
            newEngagementLevel,
            newLoadEstimation,
            newIsActive);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(original.Id);
        result.WorkoutTemplateId.Should().Be(original.WorkoutTemplateId);
        result.MuscleGroupId.Should().Be(original.MuscleGroupId);
        result.EngagementLevel.Should().Be(newEngagementLevel);
        result.LoadEstimation.Should().Be(newLoadEstimation);
        result.IsActive.Should().Be(newIsActive);
    }

    [Fact]
    public void Update_NullParameters_KeepsOriginalValues()
    {
        // Arrange
        var original = WorkoutMuscles.Handler.Create(
            Guid.NewGuid(),
            MuscleGroupId.New(),
            3,
            4);

        // Act
        var result = WorkoutMuscles.Handler.Update(original);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(original.Id);
        result.EngagementLevel.Should().Be(original.EngagementLevel);
        result.LoadEstimation.Should().Be(original.LoadEstimation);
        result.IsActive.Should().Be(original.IsActive);
    }

    [Fact]
    public void Update_PartialUpdate_UpdatesSpecifiedFieldsOnly()
    {
        // Arrange
        var original = WorkoutMuscles.Handler.Create(
            Guid.NewGuid(),
            MuscleGroupId.New(),
            3,
            4,
            true);

        var newEngagementLevel = 9;

        // Act
        var result = WorkoutMuscles.Handler.Update(
            original,
            engagementLevel: newEngagementLevel);

        // Assert
        result.Should().NotBeNull();
        result.EngagementLevel.Should().Be(newEngagementLevel);
        result.LoadEstimation.Should().Be(original.LoadEstimation); // Unchanged
        result.IsActive.Should().Be(original.IsActive); // Unchanged
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    [InlineData(-5)]
    public void Update_InvalidEngagementLevel_ThrowsArgumentException(int invalidEngagementLevel)
    {
        // Arrange
        var original = WorkoutMuscles.Handler.Create(
            Guid.NewGuid(),
            MuscleGroupId.New(),
            5,
            5);

        // Act & Assert
        var action = () => WorkoutMuscles.Handler.Update(
            original,
            engagementLevel: invalidEngagementLevel);

        action.Should().Throw<ArgumentException>()
            .WithParameterName("engagementLevel");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    [InlineData(-3)]
    public void Update_InvalidLoadEstimation_ThrowsArgumentException(int invalidLoadEstimation)
    {
        // Arrange
        var original = WorkoutMuscles.Handler.Create(
            Guid.NewGuid(),
            MuscleGroupId.New(),
            5,
            5);

        // Act & Assert
        var action = () => WorkoutMuscles.Handler.Update(
            original,
            loadEstimation: invalidLoadEstimation);

        action.Should().Throw<ArgumentException>()
            .WithParameterName("loadEstimation");
    }

    [Fact]
    public void Update_InvalidBothParameters_ThrowsArgumentException()
    {
        // Arrange
        var original = WorkoutMuscles.Handler.Create(
            Guid.NewGuid(),
            MuscleGroupId.New(),
            5,
            5);

        // Act & Assert
        var action = () => WorkoutMuscles.Handler.Update(
            original,
            engagementLevel: 0,
            loadEstimation: 11);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_ActiveWorkoutMuscles_SetsIsActiveFalse()
    {
        // Arrange
        var original = WorkoutMuscles.Handler.Create(
            Guid.NewGuid(),
            MuscleGroupId.New(),
            5,
            5,
            true);

        // Act
        var result = WorkoutMuscles.Handler.Deactivate(original);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(original.Id);
        result.EngagementLevel.Should().Be(original.EngagementLevel);
        result.LoadEstimation.Should().Be(original.LoadEstimation);
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_InactiveWorkoutMuscles_StaysInactive()
    {
        // Arrange
        var original = WorkoutMuscles.Handler.Create(
            Guid.NewGuid(),
            MuscleGroupId.New(),
            5,
            5,
            false);

        // Act
        var result = WorkoutMuscles.Handler.Deactivate(original);

        // Assert
        result.Should().NotBeNull();
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public void ValidateParameters_BoundaryValues_AcceptedCorrectly()
    {
        // This test verifies the private ValidateParameters method indirectly
        // by testing boundary values through public methods
        
        // Arrange & Act & Assert - Min values
        var minResult = WorkoutMuscles.Handler.Create(
            Guid.NewGuid(),
            MuscleGroupId.New(),
            1, // Min engagement
            1); // Min load
        minResult.Should().NotBeNull();

        // Arrange & Act & Assert - Max values
        var maxResult = WorkoutMuscles.Handler.Create(
            Guid.NewGuid(),
            MuscleGroupId.New(),
            10, // Max engagement
            10); // Max load
        maxResult.Should().NotBeNull();
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 10)]
    [InlineData(10, 1)]
    [InlineData(10, 10)]
    [InlineData(5, 5)]
    public void ValidateParameters_AllValidCombinations_DoesNotThrow(int engagementLevel, int loadEstimation)
    {
        // This test validates that all valid combinations pass validation
        // by testing through the public Create method
        
        // Arrange & Act & Assert
        var action = () => WorkoutMuscles.Handler.Create(
            Guid.NewGuid(),
            MuscleGroupId.New(),
            engagementLevel,
            loadEstimation);

        action.Should().NotThrow();
    }

    [Theory]
    [InlineData(0, 1, "engagementLevel")]
    [InlineData(11, 1, "engagementLevel")]
    [InlineData(-1, 1, "engagementLevel")]
    [InlineData(1, 0, "loadEstimation")]
    [InlineData(1, 11, "loadEstimation")]
    [InlineData(1, -1, "loadEstimation")]
    [InlineData(0, 0, "engagementLevel")] // First parameter to fail wins
    [InlineData(11, 11, "engagementLevel")] // First parameter to fail wins
    public void ValidateParameters_InvalidValues_ThrowsCorrectException(
        int engagementLevel, 
        int loadEstimation, 
        string expectedParameterName)
    {
        // This test validates that invalid combinations throw the correct exception
        // by testing through the public Create method
        
        // Arrange & Act & Assert
        var action = () => WorkoutMuscles.Handler.Create(
            Guid.NewGuid(),
            MuscleGroupId.New(),
            engagementLevel,
            loadEstimation);

        action.Should().Throw<ArgumentException>()
            .WithParameterName(expectedParameterName);
    }
}