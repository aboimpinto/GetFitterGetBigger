using System.Text.Json;
using GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise.Enhanced;

namespace GetFitterGetBigger.API.Tests.DTOs.WorkoutTemplateExercise.Enhanced;

public class WorkoutTemplateExerciseEnhancedDtoTests
{
    [Fact]
    public void WorkoutTemplateExerciseEnhancedDto_WithValidData_ShouldCreateCorrectly()
    {
        // Arrange
        var id = "workouttemplateexercise-550e8400-e29b-41d4-a716-446655440000";
        var exerciseId = "exercise-550e8400-e29b-41d4-a716-446655440000";
        var exerciseName = "Barbell Squat";
        var exerciseType = "STRENGTH";
        var phase = "Workout";
        var roundNumber = 1;
        var orderInRound = 2;
        var metadata = JsonDocument.Parse("""{"reps": 10, "weight": {"value": 60, "unit": "kg"}}""");
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow.AddHours(1);

        // Act
        var dto = new WorkoutTemplateExerciseEnhancedDto(
            id, exerciseId, exerciseName, exerciseType, phase,
            roundNumber, orderInRound, metadata, createdAt, updatedAt);

        // Assert
        dto.Id.Should().Be(id);
        dto.ExerciseId.Should().Be(exerciseId);
        dto.ExerciseName.Should().Be(exerciseName);
        dto.ExerciseType.Should().Be(exerciseType);
        dto.Phase.Should().Be(phase);
        dto.RoundNumber.Should().Be(roundNumber);
        dto.OrderInRound.Should().Be(orderInRound);
        dto.Metadata.Should().NotBeNull();
        dto.CreatedAt.Should().Be(createdAt);
        dto.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void Empty_ShouldReturnCorrectEmptyInstance()
    {
        // Act
        var empty = WorkoutTemplateExerciseEnhancedDto.Empty;

        // Assert
        empty.Id.Should().BeEmpty();
        empty.ExerciseId.Should().BeEmpty();
        empty.ExerciseName.Should().BeEmpty();
        empty.ExerciseType.Should().BeEmpty();
        empty.Phase.Should().BeEmpty();
        empty.RoundNumber.Should().Be(0);
        empty.OrderInRound.Should().Be(0);
        empty.Metadata.Should().NotBeNull();
        empty.Metadata.RootElement.ValueKind.Should().Be(JsonValueKind.Object);
        empty.CreatedAt.Should().Be(DateTime.MinValue);
        empty.UpdatedAt.Should().Be(DateTime.MinValue);
    }

    [Fact]
    public void WorkoutTemplateExerciseEnhancedDto_ShouldSerializeAndDeserializeCorrectly()
    {
        // Arrange
        var original = new WorkoutTemplateExerciseEnhancedDto(
            "test-id",
            "exercise-id",
            "Push Up",
            "STRENGTH",
            "Warmup",
            1,
            1,
            JsonDocument.Parse("""{"reps": 15}"""),
            DateTime.UtcNow,
            DateTime.UtcNow);

        // Act
        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<WorkoutTemplateExerciseEnhancedDto>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Id.Should().Be(original.Id);
        deserialized.ExerciseId.Should().Be(original.ExerciseId);
        deserialized.ExerciseName.Should().Be(original.ExerciseName);
        deserialized.ExerciseType.Should().Be(original.ExerciseType);
        deserialized.Phase.Should().Be(original.Phase);
        deserialized.RoundNumber.Should().Be(original.RoundNumber);
        deserialized.OrderInRound.Should().Be(original.OrderInRound);
    }

    [Fact]
    public void WorkoutTemplateExerciseEnhancedDto_WithComplexMetadata_ShouldHandleCorrectly()
    {
        // Arrange
        var complexMetadata = JsonDocument.Parse("""
        {
            "reps": 12,
            "sets": 3,
            "weight": {
                "value": 75.5,
                "unit": "kg"
            },
            "restTime": "120s",
            "notes": "Focus on form",
            "tempo": "3-1-3-1",
            "rpe": 8
        }
        """);

        // Act
        var dto = new WorkoutTemplateExerciseEnhancedDto(
            "test-id", "ex-id", "Deadlift", "STRENGTH", "Workout",
            2, 1, complexMetadata, DateTime.Now, DateTime.Now);

        // Assert
        dto.Metadata.Should().NotBeNull();
        dto.Metadata.RootElement.ValueKind.Should().Be(JsonValueKind.Object);
        dto.Metadata.RootElement.GetProperty("reps").GetInt32().Should().Be(12);
        dto.Metadata.RootElement.GetProperty("weight").GetProperty("value").GetDouble().Should().Be(75.5);
    }
}