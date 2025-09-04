using System.Text.Json;
using GetFitterGetBigger.Admin.Models.Dtos;
using FluentAssertions;
using Xunit;

namespace GetFitterGetBigger.Admin.Tests.Models.Dtos;

public class ExerciseContextDtoTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void ExerciseContextDto_ShouldSerializeCorrectly()
    {
        // Arrange
        var dto = new ExerciseContextDto
        {
            ContextType = "Workout",
            ContextLabel = "As Workout Exercise",
            IsActive = true,
            ThemeColor = "emerald",
            AvailableRelationships = new List<string> { "Warmup", "Cooldown", "Alternative" }
        };

        // Act
        var json = JsonSerializer.Serialize(dto, _jsonOptions);

        // Assert
        json.Should().Contain("\"contextType\":");
        json.Should().Contain("\"contextLabel\":");
        json.Should().Contain("\"isActive\":");
        json.Should().Contain("\"themeColor\":");
        json.Should().Contain("\"availableRelationships\":");
    }

    [Fact]
    public void ExerciseContextDto_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
            "contextType": "Warmup",
            "contextLabel": "As Warmup Exercise",
            "isActive": false,
            "themeColor": "orange",
            "availableRelationships": ["Alternative"]
        }
        """;

        // Act
        var dto = JsonSerializer.Deserialize<ExerciseContextDto>(json, _jsonOptions);

        // Assert
        dto.Should().NotBeNull();
        dto!.ContextType.Should().Be("Warmup");
        dto.ContextLabel.Should().Be("As Warmup Exercise");
        dto.IsActive.Should().BeFalse();
        dto.ThemeColor.Should().Be("orange");
        dto.AvailableRelationships.Should().ContainSingle("Alternative");
    }

    [Theory]
    [InlineData("Workout", "emerald")]
    [InlineData("Warmup", "orange")]
    [InlineData("Cooldown", "blue")]
    public void ExerciseContextDto_ShouldHandleContextThemes(string contextType, string themeColor)
    {
        // Arrange
        var dto = new ExerciseContextDto
        {
            ContextType = contextType,
            ThemeColor = themeColor
        };

        // Act & Assert
        dto.ContextType.Should().Be(contextType);
        dto.ThemeColor.Should().Be(themeColor);
    }

    [Fact]
    public void ExerciseContextDto_ShouldHandleEmptyRelationships()
    {
        // Arrange
        var dto = new ExerciseContextDto
        {
            ContextType = "Workout",
            AvailableRelationships = new List<string>()
        };

        // Act & Assert
        dto.AvailableRelationships.Should().BeEmpty();
    }
}