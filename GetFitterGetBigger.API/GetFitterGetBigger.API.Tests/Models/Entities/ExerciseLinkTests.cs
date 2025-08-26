using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.Enums;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Tests.Models.Entities;

/// <summary>
/// Unit tests for ExerciseLink entity including backward compatibility and enum enhancements
/// </summary>
public class ExerciseLinkTests
{
    private readonly ExerciseId _sourceExerciseId = ExerciseId.New();
    private readonly ExerciseId _targetExerciseId = ExerciseId.New();

    #region Empty Pattern Tests

    [Fact]
    public void Empty_Should_Return_Valid_Empty_ExerciseLink()
    {
        // Act
        var empty = ExerciseLink.Empty;

        // Assert
        Assert.True(empty.IsEmpty);
        Assert.Equal(ExerciseLinkId.Empty, empty.Id);
        Assert.Equal(ExerciseId.Empty, empty.SourceExerciseId);
        Assert.Equal(ExerciseId.Empty, empty.TargetExerciseId);
        Assert.Equal(string.Empty, empty.LinkType);
        Assert.Null(empty.LinkTypeEnum);
        Assert.Equal(0, empty.DisplayOrder);
        Assert.False(empty.IsActive);
        Assert.Equal(DateTime.MinValue, empty.CreatedAt);
        Assert.Equal(DateTime.MinValue, empty.UpdatedAt);
    }

    [Fact]
    public void IsEmpty_Should_Return_True_For_Empty_ExerciseLink()
    {
        // Arrange
        var empty = ExerciseLink.Empty;

        // Act & Assert
        Assert.True(empty.IsEmpty);
    }

    [Fact]
    public void IsEmpty_Should_Return_False_For_Valid_ExerciseLink()
    {
        // Arrange
        var exerciseLink = ExerciseLink.Handler.CreateNew(
            _sourceExerciseId,
            _targetExerciseId,
            "WARMUP",
            1);

        // Act & Assert
        Assert.False(exerciseLink.IsEmpty);
    }

    #endregion

    #region ActualLinkType Computed Property Tests

    [Theory]
    [InlineData("WARMUP", null, ExerciseLinkType.WARMUP)]
    [InlineData("COOLDOWN", null, ExerciseLinkType.COOLDOWN)]
    [InlineData("WARMUP", ExerciseLinkType.ALTERNATIVE, ExerciseLinkType.ALTERNATIVE)]
    [InlineData("COOLDOWN", ExerciseLinkType.WORKOUT, ExerciseLinkType.WORKOUT)]
    public void ActualLinkType_Should_Return_Correct_Value(
        string linkType, 
        ExerciseLinkType? linkTypeEnum, 
        ExerciseLinkType expected)
    {
        // Arrange
        var exerciseLink = ExerciseLink.Handler.Create(
            ExerciseLinkId.New(),
            _sourceExerciseId,
            _targetExerciseId,
            linkType,
            linkTypeEnum,
            1,
            true,
            DateTime.UtcNow,
            DateTime.UtcNow);

        // Act
        var actual = exerciseLink.ActualLinkType;

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ActualLinkType_Should_Prefer_Enum_Over_String()
    {
        // Arrange - enum takes precedence when both are set
        var exerciseLink = ExerciseLink.Handler.Create(
            ExerciseLinkId.New(),
            _sourceExerciseId,
            _targetExerciseId,
            "WARMUP", // String says WARMUP
            ExerciseLinkType.ALTERNATIVE, // But enum says ALTERNATIVE
            1,
            true,
            DateTime.UtcNow,
            DateTime.UtcNow);

        // Act
        var actual = exerciseLink.ActualLinkType;

        // Assert - enum should take precedence
        Assert.Equal(ExerciseLinkType.ALTERNATIVE, actual);
    }

    #endregion

    #region Handler.CreateNew String-based Tests (Backward Compatibility)

    [Theory]
    [InlineData("WARMUP")]
    [InlineData("COOLDOWN")]
    public void Handler_CreateNew_String_Should_Create_Valid_ExerciseLink(string linkType)
    {
        // Act
        var exerciseLink = ExerciseLink.Handler.CreateNew(
            _sourceExerciseId,
            _targetExerciseId,
            linkType,
            1);

        // Assert
        Assert.False(exerciseLink.IsEmpty);
        Assert.NotEqual(ExerciseLinkId.Empty, exerciseLink.Id);
        Assert.Equal(_sourceExerciseId, exerciseLink.SourceExerciseId);
        Assert.Equal(_targetExerciseId, exerciseLink.TargetExerciseId);
        Assert.Equal(linkType, exerciseLink.LinkType);
        Assert.NotNull(exerciseLink.LinkTypeEnum); // Should be set from the string value
        Assert.Equal(1, exerciseLink.DisplayOrder);
        Assert.True(exerciseLink.IsActive);
        Assert.True(exerciseLink.CreatedAt > DateTime.MinValue);
        Assert.True(exerciseLink.UpdatedAt > DateTime.MinValue);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Invalid")]
    [InlineData("Warmup")]
    [InlineData("warmup")]
    public void Handler_CreateNew_String_Should_Throw_For_Invalid_LinkType(string linkType)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            ExerciseLink.Handler.CreateNew(_sourceExerciseId, _targetExerciseId, linkType, 1));
    }

    [Fact]
    public void Handler_CreateNew_String_Should_Throw_For_Empty_SourceExerciseId()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            ExerciseLink.Handler.CreateNew(default, _targetExerciseId, "WARMUP", 1));
    }

    [Fact]
    public void Handler_CreateNew_String_Should_Throw_For_Empty_TargetExerciseId()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            ExerciseLink.Handler.CreateNew(_sourceExerciseId, default, "WARMUP", 1));
    }

    [Fact]
    public void Handler_CreateNew_String_Should_Throw_For_Negative_DisplayOrder()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            ExerciseLink.Handler.CreateNew(_sourceExerciseId, _targetExerciseId, "WARMUP", -1));
    }

    #endregion

    #region Handler.CreateNew Enum-based Tests (Enhanced Functionality)

    [Theory]
    [InlineData(ExerciseLinkType.WARMUP, "WARMUP")]
    [InlineData(ExerciseLinkType.COOLDOWN, "COOLDOWN")]
    [InlineData(ExerciseLinkType.WORKOUT, "WORKOUT")]
    [InlineData(ExerciseLinkType.ALTERNATIVE, "ALTERNATIVE")]
    public void Handler_CreateNew_Enum_Should_Create_Valid_ExerciseLink(ExerciseLinkType linkType, string expectedStringValue)
    {
        // Act
        var exerciseLink = ExerciseLink.Handler.CreateNew(
            _sourceExerciseId,
            _targetExerciseId,
            linkType,
            1);

        // Assert
        Assert.False(exerciseLink.IsEmpty);
        Assert.NotEqual(ExerciseLinkId.Empty, exerciseLink.Id);
        Assert.Equal(_sourceExerciseId, exerciseLink.SourceExerciseId);
        Assert.Equal(_targetExerciseId, exerciseLink.TargetExerciseId);
        Assert.Equal(expectedStringValue, exerciseLink.LinkType); // String set for backward compatibility
        Assert.Equal(linkType, exerciseLink.LinkTypeEnum); // Enum set
        Assert.Equal(linkType, exerciseLink.ActualLinkType); // Computed property uses enum
        Assert.Equal(1, exerciseLink.DisplayOrder);
        Assert.True(exerciseLink.IsActive);
        Assert.True(exerciseLink.CreatedAt > DateTime.MinValue);
        Assert.True(exerciseLink.UpdatedAt > DateTime.MinValue);
    }

    [Fact]
    public void Handler_CreateNew_Enum_Should_Throw_For_Invalid_Enum()
    {
        // Arrange - cast invalid int to enum
        var invalidEnum = (ExerciseLinkType)999;

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            ExerciseLink.Handler.CreateNew(_sourceExerciseId, _targetExerciseId, invalidEnum, 1));
    }

    [Fact]
    public void Handler_CreateNew_Enum_Should_Throw_For_Empty_SourceExerciseId()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            ExerciseLink.Handler.CreateNew(default, _targetExerciseId, ExerciseLinkType.WARMUP, 1));
    }

    [Fact]
    public void Handler_CreateNew_Enum_Should_Throw_For_Empty_TargetExerciseId()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            ExerciseLink.Handler.CreateNew(_sourceExerciseId, default, ExerciseLinkType.WARMUP, 1));
    }

    [Fact]
    public void Handler_CreateNew_Enum_Should_Throw_For_Negative_DisplayOrder()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            ExerciseLink.Handler.CreateNew(_sourceExerciseId, _targetExerciseId, ExerciseLinkType.WARMUP, -1));
    }

    #endregion

    #region Handler.Create Tests (From Existing Data)

    [Fact]
    public void Handler_Create_String_Should_Create_ExerciseLink_From_Existing_Data()
    {
        // Arrange
        var id = ExerciseLinkId.New();
        var createdAt = DateTime.UtcNow.AddHours(-1);
        var updatedAt = DateTime.UtcNow;

        // Act
        var exerciseLink = ExerciseLink.Handler.Create(
            id,
            _sourceExerciseId,
            _targetExerciseId,
            "WARMUP",
            2,
            true,
            createdAt,
            updatedAt);

        // Assert
        Assert.Equal(id, exerciseLink.Id);
        Assert.Equal(_sourceExerciseId, exerciseLink.SourceExerciseId);
        Assert.Equal(_targetExerciseId, exerciseLink.TargetExerciseId);
        Assert.Equal("WARMUP", exerciseLink.LinkType);
        Assert.Null(exerciseLink.LinkTypeEnum);
        Assert.Equal(ExerciseLinkType.WARMUP, exerciseLink.ActualLinkType); // Computed from string
        Assert.Equal(2, exerciseLink.DisplayOrder);
        Assert.True(exerciseLink.IsActive);
        Assert.Equal(createdAt, exerciseLink.CreatedAt);
        Assert.Equal(updatedAt, exerciseLink.UpdatedAt);
    }

    [Fact]
    public void Handler_Create_With_Enum_Should_Create_ExerciseLink_From_Existing_Data()
    {
        // Arrange
        var id = ExerciseLinkId.New();
        var createdAt = DateTime.UtcNow.AddHours(-1);
        var updatedAt = DateTime.UtcNow;

        // Act
        var exerciseLink = ExerciseLink.Handler.Create(
            id,
            _sourceExerciseId,
            _targetExerciseId,
            "WARMUP",
            ExerciseLinkType.ALTERNATIVE,
            2,
            false,
            createdAt,
            updatedAt);

        // Assert
        Assert.Equal(id, exerciseLink.Id);
        Assert.Equal(_sourceExerciseId, exerciseLink.SourceExerciseId);
        Assert.Equal(_targetExerciseId, exerciseLink.TargetExerciseId);
        Assert.Equal("WARMUP", exerciseLink.LinkType);
        Assert.Equal(ExerciseLinkType.ALTERNATIVE, exerciseLink.LinkTypeEnum);
        Assert.Equal(ExerciseLinkType.ALTERNATIVE, exerciseLink.ActualLinkType); // Enum takes precedence
        Assert.Equal(2, exerciseLink.DisplayOrder);
        Assert.False(exerciseLink.IsActive);
        Assert.Equal(createdAt, exerciseLink.CreatedAt);
        Assert.Equal(updatedAt, exerciseLink.UpdatedAt);
    }

    #endregion

    #region Backward Compatibility Tests

    [Fact]
    public void String_Based_Creation_Should_Work_With_ActualLinkType()
    {
        // Arrange & Act - Create using string-based method with enum values
        var warmupLink = ExerciseLink.Handler.CreateNew(_sourceExerciseId, _targetExerciseId, "WARMUP", 1);
        var cooldownLink = ExerciseLink.Handler.CreateNew(_sourceExerciseId, _targetExerciseId, "COOLDOWN", 1);

        // Assert - ActualLinkType should work correctly
        Assert.Equal(ExerciseLinkType.WARMUP, warmupLink.ActualLinkType);
        Assert.Equal(ExerciseLinkType.COOLDOWN, cooldownLink.ActualLinkType);
        Assert.Equal(ExerciseLinkType.WARMUP, warmupLink.LinkTypeEnum);
        Assert.Equal(ExerciseLinkType.COOLDOWN, cooldownLink.LinkTypeEnum);
    }

    [Fact]
    public void Enum_Based_Creation_Should_Set_Both_Properties()
    {
        // Arrange & Act - Create using new enum-based method
        var exerciseLink = ExerciseLink.Handler.CreateNew(
            _sourceExerciseId, 
            _targetExerciseId, 
            ExerciseLinkType.ALTERNATIVE, 
            1);

        // Assert - Both properties should be set
        Assert.Equal("ALTERNATIVE", exerciseLink.LinkType);
        Assert.Equal(ExerciseLinkType.ALTERNATIVE, exerciseLink.LinkTypeEnum);
        Assert.Equal(ExerciseLinkType.ALTERNATIVE, exerciseLink.ActualLinkType);
    }

    [Fact]
    public void Migration_Scenario_Should_Prefer_Enum_Over_String()
    {
        // Arrange - Simulate migration where both string and enum are set with different values
        var exerciseLink = ExerciseLink.Handler.Create(
            ExerciseLinkId.New(),
            _sourceExerciseId,
            _targetExerciseId,
            "WARMUP", // String value for enum WARMUP
            ExerciseLinkType.ALTERNATIVE, // Enum value takes precedence
            1,
            true,
            DateTime.UtcNow,
            DateTime.UtcNow);

        // Act & Assert - Enum should take precedence
        Assert.Equal("WARMUP", exerciseLink.LinkType); // Original string preserved
        Assert.Equal(ExerciseLinkType.ALTERNATIVE, exerciseLink.LinkTypeEnum); // Enum set
        Assert.Equal(ExerciseLinkType.ALTERNATIVE, exerciseLink.ActualLinkType); // Computed uses enum
    }

    #endregion
}