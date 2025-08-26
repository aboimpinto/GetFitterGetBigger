using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.Enums;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Tests.Models.Entities;

/// <summary>
/// Tests for ExerciseLink migration scenarios from string to enum
/// </summary>
public class ExerciseLinkMigrationTests
{
    private readonly ExerciseId _sourceExerciseId = ExerciseId.New();
    private readonly ExerciseId _targetExerciseId = ExerciseId.New();

    #region Migration Compatibility Tests

    [Fact]
    public void Migration_String_To_Enum_Should_Map_Correctly()
    {
        // Test the migration mapping logic that would occur in the database
        
        // Arrange - simulate string-based links with enum values
        var warmupMappings = new[]
        {
            ("WARMUP", ExerciseLinkType.WARMUP, 0),
            ("COOLDOWN", ExerciseLinkType.COOLDOWN, 1)
        };

        foreach (var (stringValue, expectedEnum, expectedInt) in warmupMappings)
        {
            // Act - test enum parsing from string values
            var actualEnum = Enum.Parse<ExerciseLinkType>(stringValue);

            // Assert - verify mapping is correct
            Assert.Equal(expectedEnum, actualEnum);
            Assert.Equal(expectedInt, (int)actualEnum);
        }
    }

    [Fact]
    public void Migration_Preserves_String_Property_Values()
    {
        // Arrange - create links using string-based method with proper enum values
        var warmupLink = ExerciseLink.Handler.CreateNew(_sourceExerciseId, _targetExerciseId, "WARMUP", 1);
        var cooldownLink = ExerciseLink.Handler.CreateNew(_sourceExerciseId, _targetExerciseId, "COOLDOWN", 1);

        // Act & Assert - string values should be preserved and enum should be set
        Assert.Equal("WARMUP", warmupLink.LinkType);
        Assert.Equal("COOLDOWN", cooldownLink.LinkType);
        
        // Enum should be set from the string values
        Assert.Equal(ExerciseLinkType.WARMUP, warmupLink.LinkTypeEnum);
        Assert.Equal(ExerciseLinkType.COOLDOWN, cooldownLink.LinkTypeEnum);
        Assert.Equal(ExerciseLinkType.WARMUP, warmupLink.ActualLinkType);
        Assert.Equal(ExerciseLinkType.COOLDOWN, cooldownLink.ActualLinkType);
    }

    [Fact]
    public void Post_Migration_Enum_Creation_Should_Set_Both_Properties()
    {
        // Arrange & Act - create links using enum-based method (post-migration scenario)
        var alternativeLink = ExerciseLink.Handler.CreateNew(
            _sourceExerciseId, 
            _targetExerciseId, 
            ExerciseLinkType.ALTERNATIVE, 
            1);

        // Assert - both properties should be set correctly
        Assert.Equal("ALTERNATIVE", alternativeLink.LinkType);
        Assert.Equal(ExerciseLinkType.ALTERNATIVE, alternativeLink.LinkTypeEnum);
        Assert.Equal(ExerciseLinkType.ALTERNATIVE, alternativeLink.ActualLinkType);
    }

    [Fact]
    public void Migration_Should_Support_Mixed_Data_States()
    {
        // Arrange - simulate database state during migration where some records have enum, others don't
        
        // Pre-migration record (string only)
        var preMigrationLink = ExerciseLink.Handler.Create(
            ExerciseLinkId.New(),
            _sourceExerciseId,
            _targetExerciseId,
            "WARMUP",
            null, // No enum set yet
            1,
            true,
            DateTime.UtcNow,
            DateTime.UtcNow);

        // Post-migration record (both string and enum)
        var postMigrationLink = ExerciseLink.Handler.Create(
            ExerciseLinkId.New(),
            _sourceExerciseId,
            _targetExerciseId,
            "WARMUP",
            ExerciseLinkType.WARMUP, // Enum set by migration
            1,
            true,
            DateTime.UtcNow,
            DateTime.UtcNow);

        // Act & Assert - both should work correctly
        Assert.Equal(ExerciseLinkType.WARMUP, preMigrationLink.ActualLinkType); // Uses string fallback
        Assert.Equal(ExerciseLinkType.WARMUP, postMigrationLink.ActualLinkType); // Uses enum
        
        // Verify enum preference
        Assert.Null(preMigrationLink.LinkTypeEnum);
        Assert.Equal(ExerciseLinkType.WARMUP, postMigrationLink.LinkTypeEnum);
    }

    [Fact]
    public void New_Enum_Values_Should_Work_Post_Migration()
    {
        // Arrange & Act - test new enum values that didn't exist pre-migration
        var workoutLink = ExerciseLink.Handler.CreateNew(
            _sourceExerciseId, 
            _targetExerciseId, 
            ExerciseLinkType.WORKOUT, 
            1);
            
        var alternativeLink = ExerciseLink.Handler.CreateNew(
            _sourceExerciseId, 
            _targetExerciseId, 
            ExerciseLinkType.ALTERNATIVE, 
            1);

        // Assert - new enum values should work correctly
        Assert.Equal("WORKOUT", workoutLink.LinkType);
        Assert.Equal(ExerciseLinkType.WORKOUT, workoutLink.LinkTypeEnum);
        Assert.Equal(ExerciseLinkType.WORKOUT, workoutLink.ActualLinkType);
        
        Assert.Equal("ALTERNATIVE", alternativeLink.LinkType);
        Assert.Equal(ExerciseLinkType.ALTERNATIVE, alternativeLink.LinkTypeEnum);
        Assert.Equal(ExerciseLinkType.ALTERNATIVE, alternativeLink.ActualLinkType);
    }

    #endregion

    #region Backward Compatibility Tests

    [Fact]
    public void Legacy_String_API_Should_Continue_Working()
    {
        // Arrange & Act - use string-based API with proper enum values
        var warmupLink = ExerciseLink.Handler.CreateNew(_sourceExerciseId, _targetExerciseId, "WARMUP", 1);
        var cooldownLink = ExerciseLink.Handler.CreateNew(_sourceExerciseId, _targetExerciseId, "COOLDOWN", 1);

        // Assert - should work with enum values properly set
        Assert.Equal("WARMUP", warmupLink.LinkType);
        Assert.Equal("COOLDOWN", cooldownLink.LinkType);
        Assert.Equal(ExerciseLinkType.WARMUP, warmupLink.LinkTypeEnum);
        Assert.Equal(ExerciseLinkType.COOLDOWN, cooldownLink.LinkTypeEnum);
        
        // But computed property should still work
        Assert.Equal(ExerciseLinkType.WARMUP, warmupLink.ActualLinkType);
        Assert.Equal(ExerciseLinkType.COOLDOWN, cooldownLink.ActualLinkType);
    }

    [Fact]
    public void Empty_Pattern_Should_Work_With_Migration()
    {
        // Act
        var empty = ExerciseLink.Empty;

        // Assert - Empty pattern should work with new properties
        Assert.Equal(string.Empty, empty.LinkType);
        Assert.Null(empty.LinkTypeEnum);
        
        // ActualLinkType should handle empty state gracefully
        Assert.Equal(ExerciseLinkType.COOLDOWN, empty.ActualLinkType); // Falls back to COOLDOWN for empty string
    }

    #endregion

    #region Data Integrity Tests

    [Fact]
    public void Migration_Should_Preserve_All_Original_Data()
    {
        // Arrange - create test data that simulates pre-migration state
        var originalCreatedAt = DateTime.UtcNow.AddDays(-30);
        var originalUpdatedAt = DateTime.UtcNow.AddDays(-1);
        
        var originalLink = ExerciseLink.Handler.Create(
            ExerciseLinkId.New(),
            _sourceExerciseId,
            _targetExerciseId,
            "WARMUP",
            5,
            true,
            originalCreatedAt,
            originalUpdatedAt);

        // Act - simulate migration by adding enum
        var migratedLink = ExerciseLink.Handler.Create(
            originalLink.Id,
            originalLink.SourceExerciseId,
            originalLink.TargetExerciseId,
            originalLink.LinkType,
            ExerciseLinkType.WARMUP, // Added by migration
            originalLink.DisplayOrder,
            originalLink.IsActive,
            originalLink.CreatedAt,
            originalLink.UpdatedAt);

        // Assert - all original data should be preserved
        Assert.Equal(originalLink.Id, migratedLink.Id);
        Assert.Equal(originalLink.SourceExerciseId, migratedLink.SourceExerciseId);
        Assert.Equal(originalLink.TargetExerciseId, migratedLink.TargetExerciseId);
        Assert.Equal(originalLink.LinkType, migratedLink.LinkType);
        Assert.Equal(originalLink.DisplayOrder, migratedLink.DisplayOrder);
        Assert.Equal(originalLink.IsActive, migratedLink.IsActive);
        Assert.Equal(originalLink.CreatedAt, migratedLink.CreatedAt);
        Assert.Equal(originalLink.UpdatedAt, migratedLink.UpdatedAt);
        
        // New enum property should be set
        Assert.Equal(ExerciseLinkType.WARMUP, migratedLink.LinkTypeEnum);
        Assert.Equal(ExerciseLinkType.WARMUP, migratedLink.ActualLinkType);
    }

    [Theory]
    [InlineData("WARMUP", ExerciseLinkType.WARMUP)]
    [InlineData("COOLDOWN", ExerciseLinkType.COOLDOWN)]
    public void Migration_Mapping_Should_Be_Bidirectional(string stringValue, ExerciseLinkType enumValue)
    {
        // Test that the migration mapping works in both directions
        
        // Act & Assert - string to enum
        var enumFromString = Enum.Parse<ExerciseLinkType>(stringValue);
        Assert.Equal(enumValue, enumFromString);
        
        // Act & Assert - enum to string
        var stringFromEnum = enumValue.ToString();
        Assert.True(stringFromEnum == "WARMUP" || stringFromEnum == "COOLDOWN");
    }

    #endregion
}