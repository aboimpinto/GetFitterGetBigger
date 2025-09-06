using FluentAssertions;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplate;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Extensions;

namespace GetFitterGetBigger.API.Tests.Services.WorkoutTemplate.Extensions;

/// <summary>
/// Tests for WorkoutTemplateServiceExtensions methods focusing on high Crap Score methods:
/// - IsValidCreateCommand() - Crap: 110, Complexity: 10
/// - IsValidUpdateCommand() - Crap: 156, Complexity: 12
/// </summary>
public class WorkoutTemplateServiceExtensionsTests
{
    #region IsValidCreateCommand Tests

    [Fact]
    public void IsValidCreateCommand_NullCommand_ReturnsFalse()
    {
        // Act
        var result = WorkoutTemplateServiceExtensions.IsValidCreateCommand(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidCreateCommand_ValidCommand_ReturnsTrue()
    {
        // Arrange
        var command = new CreateWorkoutTemplateCommand
        {
            Name = "Test Workout",
            CategoryId = WorkoutCategoryId.New(),
            DifficultyId = DifficultyLevelId.New(),
            EstimatedDurationMinutes = 60
        };

        // Act
        var result = command.IsValidCreateCommand();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidCreateCommand_EmptyName_ReturnsFalse()
    {
        // Arrange
        var command = new CreateWorkoutTemplateCommand
        {
            Name = "",
            CategoryId = WorkoutCategoryId.New(),
            DifficultyId = DifficultyLevelId.New(),
            EstimatedDurationMinutes = 60
        };

        // Act
        var result = command.IsValidCreateCommand();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidCreateCommand_WhitespaceOnlyName_ReturnsFalse()
    {
        // Arrange
        var command = new CreateWorkoutTemplateCommand
        {
            Name = "   ",
            CategoryId = WorkoutCategoryId.New(),
            DifficultyId = DifficultyLevelId.New(),
            EstimatedDurationMinutes = 60
        };

        // Act
        var result = command.IsValidCreateCommand();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidCreateCommand_NameTooShort_ReturnsFalse()
    {
        // Arrange
        var command = new CreateWorkoutTemplateCommand
        {
            Name = "AB", // Only 2 characters, minimum is 3
            CategoryId = WorkoutCategoryId.New(),
            DifficultyId = DifficultyLevelId.New(),
            EstimatedDurationMinutes = 60
        };

        // Act
        var result = command.IsValidCreateCommand();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidCreateCommand_NameTooLong_ReturnsFalse()
    {
        // Arrange
        var command = new CreateWorkoutTemplateCommand
        {
            Name = new string('A', 101), // 101 characters, maximum is 100
            CategoryId = WorkoutCategoryId.New(),
            DifficultyId = DifficultyLevelId.New(),
            EstimatedDurationMinutes = 60
        };

        // Act
        var result = command.IsValidCreateCommand();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidCreateCommand_EmptyCategoryId_ReturnsFalse()
    {
        // Arrange
        var command = new CreateWorkoutTemplateCommand
        {
            Name = "Test Workout",
            CategoryId = WorkoutCategoryId.Empty,
            DifficultyId = DifficultyLevelId.New(),
            EstimatedDurationMinutes = 60
        };

        // Act
        var result = command.IsValidCreateCommand();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidCreateCommand_EmptyDifficultyId_ReturnsFalse()
    {
        // Arrange
        var command = new CreateWorkoutTemplateCommand
        {
            Name = "Test Workout",
            CategoryId = WorkoutCategoryId.New(),
            DifficultyId = DifficultyLevelId.Empty,
            EstimatedDurationMinutes = 60
        };

        // Act
        var result = command.IsValidCreateCommand();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidCreateCommand_DurationTooShort_ReturnsFalse()
    {
        // Arrange
        var command = new CreateWorkoutTemplateCommand
        {
            Name = "Test Workout",
            CategoryId = WorkoutCategoryId.New(),
            DifficultyId = DifficultyLevelId.New(),
            EstimatedDurationMinutes = 4 // Minimum is 5
        };

        // Act
        var result = command.IsValidCreateCommand();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidCreateCommand_DurationTooLong_ReturnsFalse()
    {
        // Arrange
        var command = new CreateWorkoutTemplateCommand
        {
            Name = "Test Workout",
            CategoryId = WorkoutCategoryId.New(),
            DifficultyId = DifficultyLevelId.New(),
            EstimatedDurationMinutes = 301 // Maximum is 300
        };

        // Act
        var result = command.IsValidCreateCommand();

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(5)]    // Minimum valid
    [InlineData(60)]   // Typical value
    [InlineData(300)]  // Maximum valid
    public void IsValidCreateCommand_ValidDurationRange_ReturnsTrue(int duration)
    {
        // Arrange
        var command = new CreateWorkoutTemplateCommand
        {
            Name = "Test Workout",
            CategoryId = WorkoutCategoryId.New(),
            DifficultyId = DifficultyLevelId.New(),
            EstimatedDurationMinutes = duration
        };

        // Act
        var result = command.IsValidCreateCommand();

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region IsValidUpdateCommand Tests

    [Fact]
    public void IsValidUpdateCommand_NullCommand_ReturnsFalse()
    {
        // Act
        var result = WorkoutTemplateServiceExtensions.IsValidUpdateCommand(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidUpdateCommand_ValidPartialUpdate_ReturnsTrue()
    {
        // Arrange
        var command = new UpdateWorkoutTemplateCommand
        {
            Name = "Updated Workout",
            EstimatedDurationMinutes = 45,
            Description = "Updated description"
        };

        // Act
        var result = command.IsValidUpdateCommand();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidUpdateCommand_AllFieldsNull_ReturnsTrue()
    {
        // Arrange - For partial updates, null values are allowed (no change)
        var command = new UpdateWorkoutTemplateCommand
        {
            Name = null,
            EstimatedDurationMinutes = null,
            Description = null
        };

        // Act
        var result = command.IsValidUpdateCommand();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidUpdateCommand_EmptyName_ReturnsTrue()
    {
        // Arrange - For updates, empty name is allowed (means no change)
        var command = new UpdateWorkoutTemplateCommand
        {
            Name = "",
            EstimatedDurationMinutes = 60,
            Description = "Some description"
        };

        // Act
        var result = command.IsValidUpdateCommand();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidUpdateCommand_WhitespaceOnlyName_ReturnsTrue()
    {
        // Arrange - For updates, whitespace-only name is allowed (means no change)
        var command = new UpdateWorkoutTemplateCommand
        {
            Name = "   ",
            EstimatedDurationMinutes = 60,
            Description = "Some description"
        };

        // Act
        var result = command.IsValidUpdateCommand();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidUpdateCommand_ValidNameLength_ReturnsTrue()
    {
        // Arrange
        var command = new UpdateWorkoutTemplateCommand
        {
            Name = "Valid Name",
            EstimatedDurationMinutes = 60,
            Description = "Some description"
        };

        // Act
        var result = command.IsValidUpdateCommand();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidUpdateCommand_NameTooShort_ReturnsFalse()
    {
        // Arrange - If name is provided, it must meet length requirements
        var command = new UpdateWorkoutTemplateCommand
        {
            Name = "AB", // Only 2 characters, minimum is 3
            EstimatedDurationMinutes = 60,
            Description = "Some description"
        };

        // Act
        var result = command.IsValidUpdateCommand();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidUpdateCommand_NameTooLong_ReturnsFalse()
    {
        // Arrange - If name is provided, it must meet length requirements
        var command = new UpdateWorkoutTemplateCommand
        {
            Name = new string('A', 101), // 101 characters, maximum is 100
            EstimatedDurationMinutes = 60,
            Description = "Some description"
        };

        // Act
        var result = command.IsValidUpdateCommand();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidUpdateCommand_DurationTooShort_ReturnsFalse()
    {
        // Arrange - If duration is provided, it must meet range requirements
        var command = new UpdateWorkoutTemplateCommand
        {
            Name = "Valid Name",
            EstimatedDurationMinutes = 4, // Minimum is 5
            Description = "Some description"
        };

        // Act
        var result = command.IsValidUpdateCommand();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidUpdateCommand_DurationTooLong_ReturnsFalse()
    {
        // Arrange - If duration is provided, it must meet range requirements
        var command = new UpdateWorkoutTemplateCommand
        {
            Name = "Valid Name",
            EstimatedDurationMinutes = 301, // Maximum is 300
            Description = "Some description"
        };

        // Act
        var result = command.IsValidUpdateCommand();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidUpdateCommand_DescriptionTooLong_ReturnsFalse()
    {
        // Arrange
        var command = new UpdateWorkoutTemplateCommand
        {
            Name = "Valid Name",
            EstimatedDurationMinutes = 60,
            Description = new string('A', 1001) // Maximum is 1000
        };

        // Act
        var result = command.IsValidUpdateCommand();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidUpdateCommand_DescriptionValidLength_ReturnsTrue()
    {
        // Arrange
        var command = new UpdateWorkoutTemplateCommand
        {
            Name = "Valid Name",
            EstimatedDurationMinutes = 60,
            Description = new string('A', 1000) // Exactly maximum length
        };

        // Act
        var result = command.IsValidUpdateCommand();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidUpdateCommand_EmptyDescription_ReturnsTrue()
    {
        // Arrange - Empty description is allowed
        var command = new UpdateWorkoutTemplateCommand
        {
            Name = "Valid Name",
            EstimatedDurationMinutes = 60,
            Description = ""
        };

        // Act
        var result = command.IsValidUpdateCommand();

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(5)]    // Minimum valid
    [InlineData(60)]   // Typical value
    [InlineData(300)]  // Maximum valid
    public void IsValidUpdateCommand_ValidDurationRange_ReturnsTrue(int duration)
    {
        // Arrange
        var command = new UpdateWorkoutTemplateCommand
        {
            Name = "Valid Name",
            EstimatedDurationMinutes = duration,
            Description = "Some description"
        };

        // Act
        var result = command.IsValidUpdateCommand();

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Helper Method Tests

    [Theory]
    [InlineData("null", false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("AB", false)]  // Too short
    [InlineData(101, false)]   // Too long (via string constructor)
    [InlineData("ABC", true)]  // Minimum valid
    [InlineData("Valid Name", true)]
    public void HasValidNameLength_VariousInputs_ReturnsExpected(object input, bool expected)
    {
        // Arrange
        string? name = input switch
        {
            "null" => null,
            int length => new string('A', length),
            string str => str,
            _ => throw new ArgumentException("Invalid test input")
        };

        // Act
        var result = name.HasValidNameLength();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("null", true)]
    [InlineData("", true)]
    [InlineData("Valid description", true)]
    [InlineData(1000, true)]  // Exactly at limit
    [InlineData(1001, false)] // Over limit
    public void HasValidDescriptionLength_VariousInputs_ReturnsExpected(object input, bool expected)
    {
        // Arrange
        string? description = input switch
        {
            "null" => null,
            string str => str,
            int length => new string('A', length),
            _ => throw new ArgumentException("Invalid test input")
        };

        // Act
        var result = description.HasValidDescriptionLength();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(4, false)]   // Too short
    [InlineData(5, true)]    // Minimum valid
    [InlineData(60, true)]   // Typical value
    [InlineData(300, true)]  // Maximum valid
    [InlineData(301, false)] // Too long
    public void IsValidDuration_VariousInputs_ReturnsExpected(int duration, bool expected)
    {
        // Act
        var result = duration.IsValidDuration();

        // Assert
        result.Should().Be(expected);
    }

    #endregion
}