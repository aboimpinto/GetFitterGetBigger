using FluentAssertions;
using GetFitterGetBigger.API.Models.Enums;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.Commands;

namespace GetFitterGetBigger.API.Tests.Services.Exercise.Features.Links.Commands;

/// <summary>
/// Unit tests for CreateExerciseLinkCommand focusing on ActualLinkType property logic
/// These tests are critical to reduce the Crap Score by providing comprehensive coverage
/// </summary>
public class CreateExerciseLinkCommandTests
{
    #region ActualLinkType Tests - Enum Priority

    [Fact]
    public void ActualLinkType_WhenLinkTypeEnumIsSet_ShouldReturnLinkTypeEnum()
    {
        // Arrange
        var command = new CreateExerciseLinkCommand
        {
            LinkType = "SomeString",
            LinkTypeEnum = ExerciseLinkType.COOLDOWN
        };

        // Act
        var result = command.ActualLinkType;

        // Assert
        result.Should().Be(ExerciseLinkType.COOLDOWN);
    }

    [Fact]
    public void ActualLinkType_WhenLinkTypeEnumIsNullAndStringIsWarmup_ShouldReturnWarmup()
    {
        // Arrange
        var command = new CreateExerciseLinkCommand
        {
            LinkType = "WARMUP",
            LinkTypeEnum = null
        };

        // Act
        var result = command.ActualLinkType;

        // Assert
        result.Should().Be(ExerciseLinkType.WARMUP);
    }

    [Fact]
    public void ActualLinkType_WhenLinkTypeEnumIsNullAndStringIsCooldown_ShouldReturnCooldown()
    {
        // Arrange
        var command = new CreateExerciseLinkCommand
        {
            LinkType = "COOLDOWN",
            LinkTypeEnum = null
        };

        // Act
        var result = command.ActualLinkType;

        // Assert
        result.Should().Be(ExerciseLinkType.COOLDOWN);
    }

    #endregion

    #region ActualLinkType Tests - Legacy String Support

    [Fact]
    public void ActualLinkType_WhenLegacyWarmupString_ShouldReturnWarmup()
    {
        // Arrange
        var command = new CreateExerciseLinkCommand
        {
            LinkType = "Warmup",
            LinkTypeEnum = null
        };

        // Act
        var result = command.ActualLinkType;

        // Assert
        result.Should().Be(ExerciseLinkType.WARMUP);
    }

    [Fact]
    public void ActualLinkType_WhenLegacyCooldownString_ShouldReturnCooldown()
    {
        // Arrange
        var command = new CreateExerciseLinkCommand
        {
            LinkType = "Cooldown",
            LinkTypeEnum = null
        };

        // Act
        var result = command.ActualLinkType;

        // Assert
        result.Should().Be(ExerciseLinkType.COOLDOWN);
    }

    [Theory]
    [InlineData("warmup")]
    [InlineData("WARMUP")]
    [InlineData("Warmup")]
    [InlineData("WaRmUp")]
    public void ActualLinkType_WhenLegacyWarmupStringWithDifferentCasing_ShouldReturnWarmup(string linkType)
    {
        // Arrange
        var command = new CreateExerciseLinkCommand
        {
            LinkType = linkType,
            LinkTypeEnum = null
        };

        // Act
        var result = command.ActualLinkType;

        // Assert
        result.Should().Be(ExerciseLinkType.WARMUP);
    }

    [Theory]
    [InlineData("cooldown")]
    [InlineData("COOLDOWN")]
    [InlineData("Cooldown")]
    [InlineData("CoOlDoWn")]
    public void ActualLinkType_WhenLegacyCooldownStringWithDifferentCasing_ShouldReturnCooldown(string linkType)
    {
        // Arrange
        var command = new CreateExerciseLinkCommand
        {
            LinkType = linkType,
            LinkTypeEnum = null
        };

        // Act
        var result = command.ActualLinkType;

        // Assert
        result.Should().Be(ExerciseLinkType.COOLDOWN);
    }

    #endregion

    #region ActualLinkType Tests - Edge Cases

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void ActualLinkType_WhenLinkTypeIsNullOrWhitespace_ShouldReturnWarmupAsDefault(string? linkType)
    {
        // Arrange
        var command = new CreateExerciseLinkCommand
        {
            LinkType = linkType,
            LinkTypeEnum = null
        };

        // Act
        var result = command.ActualLinkType;

        // Assert
        result.Should().Be(ExerciseLinkType.WARMUP);
    }

    [Theory]
    [InlineData("InvalidValue")]
    [InlineData("Unknown")]
    [InlineData("123")]
    [InlineData("!@#")]
    public void ActualLinkType_WhenLinkTypeIsInvalid_ShouldReturnWarmupAsDefault(string linkType)
    {
        // Arrange
        var command = new CreateExerciseLinkCommand
        {
            LinkType = linkType,
            LinkTypeEnum = null
        };

        // Act
        var result = command.ActualLinkType;

        // Assert
        result.Should().Be(ExerciseLinkType.WARMUP);
    }

    [Fact]
    public void ActualLinkType_WhenValidEnumStringValue_ShouldParseCorrectly()
    {
        // Arrange - Test that standard enum strings work
        var command = new CreateExerciseLinkCommand
        {
            LinkType = ExerciseLinkType.COOLDOWN.ToString(),
            LinkTypeEnum = null
        };

        // Act
        var result = command.ActualLinkType;

        // Assert
        result.Should().Be(ExerciseLinkType.COOLDOWN);
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void DefaultConstructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var command = new CreateExerciseLinkCommand();

        // Assert
        command.SourceExerciseId.Should().Be(ExerciseId.Empty);
        command.TargetExerciseId.Should().Be(ExerciseId.Empty);
        command.LinkType.Should().Be(string.Empty);
        command.DisplayOrder.Should().Be(0);
        command.LinkTypeEnum.Should().BeNull();
        command.ActualLinkType.Should().Be(ExerciseLinkType.WARMUP); // Default fallback
    }

    [Fact]
    public void EnumBasedConstructor_ShouldSetAllPropertiesCorrectly()
    {
        // Arrange
        var sourceId = ExerciseId.New();
        var targetId = ExerciseId.New();
        const int displayOrder = 5;
        const ExerciseLinkType linkType = ExerciseLinkType.COOLDOWN;

        // Act
        var command = new CreateExerciseLinkCommand(sourceId, targetId, linkType, displayOrder);

        // Assert
        command.SourceExerciseId.Should().Be(sourceId);
        command.TargetExerciseId.Should().Be(targetId);
        command.LinkType.Should().Be(linkType.ToString());
        command.DisplayOrder.Should().Be(displayOrder);
        command.LinkTypeEnum.Should().Be(linkType);
        command.ActualLinkType.Should().Be(linkType);
    }

    [Fact]
    public void EnumBasedConstructor_WhenCreatedWithWarmup_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var sourceId = ExerciseId.New();
        var targetId = ExerciseId.New();
        const int displayOrder = 1;
        const ExerciseLinkType linkType = ExerciseLinkType.WARMUP;

        // Act
        var command = new CreateExerciseLinkCommand(sourceId, targetId, linkType, displayOrder);

        // Assert
        command.SourceExerciseId.Should().Be(sourceId);
        command.TargetExerciseId.Should().Be(targetId);
        command.LinkType.Should().Be("WARMUP");
        command.DisplayOrder.Should().Be(displayOrder);
        command.LinkTypeEnum.Should().Be(ExerciseLinkType.WARMUP);
        command.ActualLinkType.Should().Be(ExerciseLinkType.WARMUP);
    }

    #endregion

    #region Record Equality Tests

    [Fact]
    public void RecordEquality_WhenSameValues_ShouldBeEqual()
    {
        // Arrange
        var sourceId = ExerciseId.New();
        var targetId = ExerciseId.New();
        
        var command1 = new CreateExerciseLinkCommand
        {
            SourceExerciseId = sourceId,
            TargetExerciseId = targetId,
            LinkType = "WARMUP",
            DisplayOrder = 1,
            LinkTypeEnum = ExerciseLinkType.WARMUP
        };
        
        var command2 = new CreateExerciseLinkCommand
        {
            SourceExerciseId = sourceId,
            TargetExerciseId = targetId,
            LinkType = "WARMUP",
            DisplayOrder = 1,
            LinkTypeEnum = ExerciseLinkType.WARMUP
        };

        // Act & Assert
        command1.Should().Be(command2);
        command1.GetHashCode().Should().Be(command2.GetHashCode());
    }

    [Fact]
    public void RecordEquality_WhenDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var sourceId1 = ExerciseId.New();
        var sourceId2 = ExerciseId.New();
        var targetId = ExerciseId.New();
        
        var command1 = new CreateExerciseLinkCommand
        {
            SourceExerciseId = sourceId1,
            TargetExerciseId = targetId,
            LinkType = "WARMUP",
            DisplayOrder = 1
        };
        
        var command2 = new CreateExerciseLinkCommand
        {
            SourceExerciseId = sourceId2,
            TargetExerciseId = targetId,
            LinkType = "WARMUP",
            DisplayOrder = 1
        };

        // Act & Assert
        command1.Should().NotBe(command2);
    }

    #endregion

    #region Backward Compatibility Integration Tests

    [Fact]
    public void ActualLinkType_BackwardCompatibility_LegacyStringOverride()
    {
        // Test scenario: Old client sends "Warmup" string but new logic sets COOLDOWN enum
        // The enum should take priority
        
        // Arrange
        var command = new CreateExerciseLinkCommand
        {
            LinkType = "Warmup", // Legacy string value
            LinkTypeEnum = ExerciseLinkType.COOLDOWN // New enum value
        };

        // Act
        var result = command.ActualLinkType;

        // Assert
        result.Should().Be(ExerciseLinkType.COOLDOWN, 
            "Enum value should take priority over legacy string value");
    }

    [Fact]
    public void ActualLinkType_BackwardCompatibility_OnlyLegacyString()
    {
        // Test scenario: Old client sends only legacy string
        
        // Arrange
        var command = new CreateExerciseLinkCommand
        {
            LinkType = "Cooldown", // Legacy string value
            LinkTypeEnum = null // No new enum value
        };

        // Act
        var result = command.ActualLinkType;

        // Assert
        result.Should().Be(ExerciseLinkType.COOLDOWN, 
            "Should correctly parse legacy string when no enum is provided");
    }

    #endregion
}