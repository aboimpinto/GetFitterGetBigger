using System.Text.Json;
using GetFitterGetBigger.Admin.Models.Dtos;
using FluentAssertions;
using Xunit;

namespace GetFitterGetBigger.Admin.Tests.Models.Dtos;

public class ExerciseLinkDtoTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void ExerciseLinkDto_ShouldSerializeCorrectly()
    {
        // Arrange
        var dto = new ExerciseLinkDto
        {
            Id = "exerciselink-550e8400-e29b-41d4-a716-446655440000",
            SourceExerciseId = "exercise-123e4567-e89b-12d3-a456-426614174001",
            TargetExerciseId = "exercise-123e4567-e89b-12d3-a456-426614174000",
            TargetExerciseName = "Air Squat",
            LinkType = "Warmup",
            DisplayOrder = 1,
            IsActive = true,
            CreatedAt = new DateTime(2025, 7, 9, 10, 30, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2025, 7, 9, 10, 30, 0, DateTimeKind.Utc)
        };

        // Act
        var json = JsonSerializer.Serialize(dto, _jsonOptions);

        // Assert
        json.Should().Contain("\"id\":");
        json.Should().Contain("\"sourceExerciseId\":");
        json.Should().Contain("\"targetExerciseId\":");
        json.Should().Contain("\"targetExerciseName\":");
        json.Should().Contain("\"linkType\":");
        json.Should().Contain("\"displayOrder\":");
        json.Should().Contain("\"isActive\":");
        json.Should().Contain("\"createdAt\":");
        json.Should().Contain("\"updatedAt\":");
    }

    [Fact]
    public void ExerciseLinkDto_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
            "id": "exerciselink-550e8400-e29b-41d4-a716-446655440000",
            "sourceExerciseId": "exercise-123e4567-e89b-12d3-a456-426614174001",
            "targetExerciseId": "exercise-123e4567-e89b-12d3-a456-426614174000",
            "targetExerciseName": "Air Squat",
            "linkType": "Warmup",
            "displayOrder": 1,
            "isActive": true,
            "createdAt": "2025-07-09T10:30:00Z",
            "updatedAt": "2025-07-09T10:30:00Z"
        }
        """;

        // Act
        var dto = JsonSerializer.Deserialize<ExerciseLinkDto>(json, _jsonOptions);

        // Assert
        dto.Should().NotBeNull();
        dto!.Id.Should().Be("exerciselink-550e8400-e29b-41d4-a716-446655440000");
        dto.SourceExerciseId.Should().Be("exercise-123e4567-e89b-12d3-a456-426614174001");
        dto.TargetExerciseId.Should().Be("exercise-123e4567-e89b-12d3-a456-426614174000");
        dto.TargetExerciseName.Should().Be("Air Squat");
        dto.LinkType.Should().Be("Warmup");
        dto.DisplayOrder.Should().Be(1);
        dto.IsActive.Should().BeTrue();
        dto.CreatedAt.Should().Be(new DateTime(2025, 7, 9, 10, 30, 0, DateTimeKind.Utc));
        dto.UpdatedAt.Should().Be(new DateTime(2025, 7, 9, 10, 30, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void ExerciseLinksResponseDto_ShouldSerializeCorrectly()
    {
        // Arrange
        var dto = new ExerciseLinksResponseDto
        {
            ExerciseId = "exercise-123e4567-e89b-12d3-a456-426614174001",
            ExerciseName = "Barbell Squat",
            Links = new List<ExerciseLinkDto>
            {
                new()
                {
                    Id = "exerciselink-550e8400-e29b-41d4-a716-446655440000",
                    SourceExerciseId = "exercise-123e4567-e89b-12d3-a456-426614174001",
                    TargetExerciseId = "exercise-123e4567-e89b-12d3-a456-426614174000",
                    TargetExerciseName = "Air Squat",
                    LinkType = "Warmup",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            },
            TotalCount = 1
        };

        // Act
        var json = JsonSerializer.Serialize(dto, _jsonOptions);

        // Assert
        json.Should().Contain("\"exerciseId\":");
        json.Should().Contain("\"exerciseName\":");
        json.Should().Contain("\"links\":");
        json.Should().Contain("\"totalCount\":");
    }

    [Fact]
    public void CreateExerciseLinkDto_ShouldSerializeCorrectly()
    {
        // Arrange
        var dto = new CreateExerciseLinkDto
        {
            TargetExerciseId = "exercise-123e4567-e89b-12d3-a456-426614174000",
            LinkType = "Warmup",
            DisplayOrder = 1
        };

        // Act
        var json = JsonSerializer.Serialize(dto, _jsonOptions);

        // Assert
        json.Should().Contain("\"targetExerciseId\":");
        json.Should().Contain("\"linkType\":");
        json.Should().Contain("\"displayOrder\":");
    }

    [Fact]
    public void UpdateExerciseLinkDto_ShouldSerializeCorrectly()
    {
        // Arrange
        var dto = new UpdateExerciseLinkDto
        {
            DisplayOrder = 3,
            IsActive = true
        };

        // Act
        var json = JsonSerializer.Serialize(dto, _jsonOptions);

        // Assert
        json.Should().Contain("\"displayOrder\":");
        json.Should().Contain("\"isActive\":");
    }

    [Theory]
    [InlineData("Warmup")]
    [InlineData("Cooldown")]
    public void ExerciseLinkDto_ShouldHandleValidLinkTypes(string linkType)
    {
        // Arrange
        var dto = new ExerciseLinkDto
        {
            LinkType = linkType
        };

        // Act & Assert
        dto.LinkType.Should().Be(linkType);
    }

    [Fact]
    public void ExerciseLinkDto_WithTargetExercise_ShouldSerializeCorrectly()
    {
        // Arrange
        var dto = new ExerciseLinkDto
        {
            Id = "exerciselink-550e8400-e29b-41d4-a716-446655440000",
            TargetExercise = new ExerciseDto
            {
                Id = "exercise-123e4567-e89b-12d3-a456-426614174000",
                Name = "Air Squat",
                Description = "Bodyweight squat exercise"
            }
        };

        // Act
        var json = JsonSerializer.Serialize(dto, _jsonOptions);

        // Assert
        json.Should().Contain("\"targetExercise\":");
        json.Should().Contain("\"Air Squat\"");
    }
}