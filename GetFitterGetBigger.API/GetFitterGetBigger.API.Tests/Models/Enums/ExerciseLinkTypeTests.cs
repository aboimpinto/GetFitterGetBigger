using GetFitterGetBigger.API.Models.Enums;

namespace GetFitterGetBigger.API.Tests.Models.Enums;

/// <summary>
/// Unit tests for ExerciseLinkType enum
/// </summary>
public class ExerciseLinkTypeTests
{
    /// <summary>
    /// Test that enum values have the expected integer assignments for database stability
    /// </summary>
    [Fact]
    public void ExerciseLinkType_Should_Have_Correct_Integer_Values()
    {
        // Assert - Verify explicit integer values for database compatibility
        Assert.Equal(0, (int)ExerciseLinkType.WARMUP);
        Assert.Equal(1, (int)ExerciseLinkType.COOLDOWN);
        Assert.Equal(2, (int)ExerciseLinkType.WORKOUT);
        Assert.Equal(3, (int)ExerciseLinkType.ALTERNATIVE);
    }

    /// <summary>
    /// Test that enum can be converted to string representation
    /// </summary>
    [Fact]
    public void ExerciseLinkType_Should_Convert_To_String()
    {
        // Assert - Verify string conversion
        Assert.Equal("WARMUP", ExerciseLinkType.WARMUP.ToString());
        Assert.Equal("COOLDOWN", ExerciseLinkType.COOLDOWN.ToString());
        Assert.Equal("WORKOUT", ExerciseLinkType.WORKOUT.ToString());
        Assert.Equal("ALTERNATIVE", ExerciseLinkType.ALTERNATIVE.ToString());
    }

    /// <summary>
    /// Test that enum can be parsed from string values
    /// </summary>
    [Theory]
    [InlineData("WARMUP", ExerciseLinkType.WARMUP)]
    [InlineData("COOLDOWN", ExerciseLinkType.COOLDOWN)]
    [InlineData("WORKOUT", ExerciseLinkType.WORKOUT)]
    [InlineData("ALTERNATIVE", ExerciseLinkType.ALTERNATIVE)]
    public void ExerciseLinkType_Should_Parse_From_String(string input, ExerciseLinkType expected)
    {
        // Act
        var success = Enum.TryParse<ExerciseLinkType>(input, out var result);

        // Assert
        Assert.True(success);
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Test that invalid string values fail to parse
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("Invalid")]
    [InlineData("warmup")] // lowercase should fail
    [InlineData("INVALID_TYPE")]
    public void ExerciseLinkType_Should_Fail_To_Parse_Invalid_Strings(string input)
    {
        // Act
        var success = Enum.TryParse<ExerciseLinkType>(input, out var result);

        // Assert
        Assert.False(success);
        Assert.Equal(default(ExerciseLinkType), result);
    }

    /// <summary>
    /// Test that default enum value is WARMUP (0)
    /// </summary>
    [Fact]
    public void ExerciseLinkType_Should_Have_WARMUP_As_Default()
    {
        // Arrange
        var defaultValue = default(ExerciseLinkType);

        // Assert
        Assert.Equal(ExerciseLinkType.WARMUP, defaultValue);
        Assert.Equal(0, (int)defaultValue);
    }

    /// <summary>
    /// Test that Enum.IsDefined works correctly
    /// </summary>
    [Theory]
    [InlineData(0, true)]  // WARMUP
    [InlineData(1, true)]  // COOLDOWN
    [InlineData(2, true)]  // WORKOUT
    [InlineData(3, true)]  // ALTERNATIVE
    [InlineData(4, false)] // Invalid
    [InlineData(-1, false)] // Invalid
    public void ExerciseLinkType_Should_Validate_Defined_Values(int value, bool expectedValid)
    {
        // Act
        var isDefined = Enum.IsDefined(typeof(ExerciseLinkType), value);

        // Assert
        Assert.Equal(expectedValid, isDefined);
    }

    /// <summary>
    /// Test that all enum values can be cast to and from integers
    /// </summary>
    [Fact]
    public void ExerciseLinkType_Should_Support_Integer_Casting()
    {
        // Test casting to int
        Assert.Equal(0, (int)ExerciseLinkType.WARMUP);
        Assert.Equal(1, (int)ExerciseLinkType.COOLDOWN);
        Assert.Equal(2, (int)ExerciseLinkType.WORKOUT);
        Assert.Equal(3, (int)ExerciseLinkType.ALTERNATIVE);

        // Test casting from int
        Assert.Equal(ExerciseLinkType.WARMUP, (ExerciseLinkType)0);
        Assert.Equal(ExerciseLinkType.COOLDOWN, (ExerciseLinkType)1);
        Assert.Equal(ExerciseLinkType.WORKOUT, (ExerciseLinkType)2);
        Assert.Equal(ExerciseLinkType.ALTERNATIVE, (ExerciseLinkType)3);
    }
}