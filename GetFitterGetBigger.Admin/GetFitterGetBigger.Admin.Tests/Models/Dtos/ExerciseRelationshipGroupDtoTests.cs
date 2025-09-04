using System.Text.Json;
using GetFitterGetBigger.Admin.Models.Dtos;
using FluentAssertions;
using Xunit;

namespace GetFitterGetBigger.Admin.Tests.Models.Dtos;

public class ExerciseRelationshipGroupDtoTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void ExerciseRelationshipGroupDto_ShouldSerializeCorrectly()
    {
        // Arrange
        var link = new ExerciseLinkDto
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
        };

        var dto = new ExerciseRelationshipGroupDto
        {
            GroupName = "Warmups",
            RelationshipType = "Warmup",
            IsReadOnly = false,
            HasDisplayOrder = true,
            MaximumCount = 10,
            CurrentCount = 1,
            ThemeColor = "orange",
            Relationships = new List<ExerciseLinkDto> { link }
        };

        // Act
        var json = JsonSerializer.Serialize(dto, _jsonOptions);

        // Assert
        json.Should().Contain("\"groupName\":");
        json.Should().Contain("\"relationshipType\":");
        json.Should().Contain("\"isReadOnly\":");
        json.Should().Contain("\"hasDisplayOrder\":");
        json.Should().Contain("\"maximumCount\":");
        json.Should().Contain("\"currentCount\":");
        json.Should().Contain("\"themeColor\":");
        json.Should().Contain("\"relationships\":");
    }

    [Fact]
    public void ExerciseRelationshipGroupDto_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
            "groupName": "Alternatives",
            "relationshipType": "Alternative",
            "isReadOnly": false,
            "hasDisplayOrder": false,
            "maximumCount": null,
            "currentCount": 3,
            "themeColor": "purple",
            "relationships": []
        }
        """;

        // Act
        var dto = JsonSerializer.Deserialize<ExerciseRelationshipGroupDto>(json, _jsonOptions);

        // Assert
        dto.Should().NotBeNull();
        dto!.GroupName.Should().Be("Alternatives");
        dto.RelationshipType.Should().Be("Alternative");
        dto.IsReadOnly.Should().BeFalse();
        dto.HasDisplayOrder.Should().BeFalse();
        dto.MaximumCount.Should().BeNull();
        dto.CurrentCount.Should().Be(3);
        dto.ThemeColor.Should().Be("purple");
        dto.Relationships.Should().BeEmpty();
    }

    [Fact]
    public void ExerciseRelationshipGroupDto_IsAtMaximum_ShouldReturnTrueWhenAtLimit()
    {
        // Arrange
        var dto = new ExerciseRelationshipGroupDto
        {
            MaximumCount = 10,
            CurrentCount = 10
        };

        // Act & Assert
        dto.IsAtMaximum.Should().BeTrue();
    }

    [Fact]
    public void ExerciseRelationshipGroupDto_IsAtMaximum_ShouldReturnFalseWhenBelowLimit()
    {
        // Arrange
        var dto = new ExerciseRelationshipGroupDto
        {
            MaximumCount = 10,
            CurrentCount = 5
        };

        // Act & Assert
        dto.IsAtMaximum.Should().BeFalse();
    }

    [Fact]
    public void ExerciseRelationshipGroupDto_IsAtMaximum_ShouldReturnFalseWhenNoLimit()
    {
        // Arrange
        var dto = new ExerciseRelationshipGroupDto
        {
            MaximumCount = null,
            CurrentCount = 100
        };

        // Act & Assert
        dto.IsAtMaximum.Should().BeFalse();
    }

    [Theory]
    [InlineData("Warmup", true, 10)]
    [InlineData("Cooldown", true, 10)]
    [InlineData("Alternative", false, null)]
    public void ExerciseRelationshipGroupDto_ShouldHandleGroupTypes(string relationshipType, bool hasDisplayOrder, int? maximumCount)
    {
        // Arrange
        var dto = new ExerciseRelationshipGroupDto
        {
            RelationshipType = relationshipType,
            HasDisplayOrder = hasDisplayOrder,
            MaximumCount = maximumCount
        };

        // Act & Assert
        dto.RelationshipType.Should().Be(relationshipType);
        dto.HasDisplayOrder.Should().Be(hasDisplayOrder);
        dto.MaximumCount.Should().Be(maximumCount);
    }

    [Fact]
    public void ExerciseRelationshipGroupDto_ReadOnlyGroup_ShouldSerializeCorrectly()
    {
        // Arrange
        var dto = new ExerciseRelationshipGroupDto
        {
            GroupName = "Workouts using this warmup",
            RelationshipType = "Workout",
            IsReadOnly = true,
            HasDisplayOrder = false,
            MaximumCount = null,
            CurrentCount = 2,
            ThemeColor = "gray",
            Relationships = new List<ExerciseLinkDto>()
        };

        // Act
        var json = JsonSerializer.Serialize(dto, _jsonOptions);

        // Assert
        json.Should().Contain("\"isReadOnly\":true");
        json.Should().Contain("\"groupName\":\"Workouts using this warmup\"");
    }
}