using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise.Requests;

namespace GetFitterGetBigger.API.Tests.DTOs.WorkoutTemplateExercise.Requests;

public class AddExerciseToTemplateRequestTests
{
    [Fact]
    public void AddExerciseToTemplateRequest_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange
        var exerciseId = "exercise-550e8400-e29b-41d4-a716-446655440000";
        var phase = "Workout";
        var roundNumber = 1;
        var metadata = JsonDocument.Parse("""{"reps": 10, "weight": {"value": 60, "unit": "kg"}}""");

        // Act
        var request = new AddExerciseToTemplateRequest
        {
            ExerciseId = exerciseId,
            Phase = phase,
            RoundNumber = roundNumber,
            Metadata = metadata
        };

        // Assert
        request.ExerciseId.Should().Be(exerciseId);
        request.Phase.Should().Be(phase);
        request.RoundNumber.Should().Be(roundNumber);
        request.Metadata.Should().NotBeNull();
    }

    [Fact]
    public void AddExerciseToTemplateRequest_ShouldHaveRequiredAnnotations()
    {
        // Arrange & Act
        var properties = typeof(AddExerciseToTemplateRequest).GetProperties();

        // Assert
        var exerciseIdProperty = properties.First(p => p.Name == nameof(AddExerciseToTemplateRequest.ExerciseId));
        var phaseProperty = properties.First(p => p.Name == nameof(AddExerciseToTemplateRequest.Phase));
        var roundNumberProperty = properties.First(p => p.Name == nameof(AddExerciseToTemplateRequest.RoundNumber));
        var metadataProperty = properties.First(p => p.Name == nameof(AddExerciseToTemplateRequest.Metadata));

        exerciseIdProperty.GetCustomAttribute<RequiredAttribute>().Should().NotBeNull();
        phaseProperty.GetCustomAttribute<RequiredAttribute>().Should().NotBeNull();
        roundNumberProperty.GetCustomAttribute<RequiredAttribute>().Should().NotBeNull();
        metadataProperty.GetCustomAttribute<RequiredAttribute>().Should().NotBeNull();
    }

    [Fact]
    public void AddExerciseToTemplateRequest_RoundNumber_ShouldHaveRangeValidation()
    {
        // Arrange & Act
        var roundNumberProperty = typeof(AddExerciseToTemplateRequest)
            .GetProperty(nameof(AddExerciseToTemplateRequest.RoundNumber));

        // Assert
        var rangeAttribute = roundNumberProperty?.GetCustomAttribute<RangeAttribute>();
        rangeAttribute.Should().NotBeNull();
        rangeAttribute!.Minimum.Should().Be(1);
    }

    [Fact]
    public void AddExerciseToTemplateRequest_WithJsonMetadata_ShouldSerializeCorrectly()
    {
        // Arrange
        var metadata = JsonDocument.Parse("""
        {
            "reps": 12,
            "sets": 3,
            "weight": {
                "value": 65.5,
                "unit": "kg"
            },
            "restTime": "60s"
        }
        """);

        var request = new AddExerciseToTemplateRequest
        {
            ExerciseId = "exercise-123",
            Phase = "Workout",
            RoundNumber = 2,
            Metadata = metadata
        };

        // Act
        var json = JsonSerializer.Serialize(request);
        var deserialized = JsonSerializer.Deserialize<AddExerciseToTemplateRequest>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.ExerciseId.Should().Be(request.ExerciseId);
        deserialized.Phase.Should().Be(request.Phase);
        deserialized.RoundNumber.Should().Be(request.RoundNumber);
        deserialized.Metadata.Should().NotBeNull();
    }
}