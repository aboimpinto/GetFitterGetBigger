using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise.Requests;

namespace GetFitterGetBigger.API.Tests.DTOs.WorkoutTemplateExercise.Requests;

public class UpdateExerciseMetadataRequestTests
{
    [Fact]
    public void UpdateExerciseMetadataRequest_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange
        var metadata = JsonDocument.Parse("""{"reps": 12, "weight": {"value": 65, "unit": "kg"}}""");

        // Act
        var request = new UpdateExerciseMetadataRequest
        {
            Metadata = metadata
        };

        // Assert
        request.Metadata.Should().NotBeNull();
    }

    [Fact]
    public void UpdateExerciseMetadataRequest_ShouldHaveRequiredAnnotation()
    {
        // Arrange & Act
        var metadataProperty = typeof(UpdateExerciseMetadataRequest)
            .GetProperty(nameof(UpdateExerciseMetadataRequest.Metadata));

        // Assert
        var requiredAttribute = metadataProperty?.GetCustomAttribute<RequiredAttribute>();
        requiredAttribute.Should().NotBeNull();
    }

    [Fact]
    public void UpdateExerciseMetadataRequest_WithComplexJsonMetadata_ShouldSerializeCorrectly()
    {
        // Arrange
        var metadata = JsonDocument.Parse("""
        {
            "reps": 15,
            "sets": 4,
            "weight": {
                "value": 70,
                "unit": "kg"
            },
            "restTime": "90s",
            "notes": "Increase weight next session",
            "tempo": "2-1-2-1"
        }
        """);

        var request = new UpdateExerciseMetadataRequest
        {
            Metadata = metadata
        };

        // Act
        var json = JsonSerializer.Serialize(request);
        var deserialized = JsonSerializer.Deserialize<UpdateExerciseMetadataRequest>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Metadata.Should().NotBeNull();
    }

    [Fact]
    public void UpdateExerciseMetadataRequest_WithEmptyMetadata_ShouldHandleCorrectly()
    {
        // Arrange
        var metadata = JsonDocument.Parse("{}");

        // Act
        var request = new UpdateExerciseMetadataRequest
        {
            Metadata = metadata
        };

        // Assert
        request.Metadata.Should().NotBeNull();
        request.Metadata.RootElement.ValueKind.Should().Be(JsonValueKind.Object);
    }
}