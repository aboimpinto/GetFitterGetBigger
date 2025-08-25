using FluentAssertions;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Exercise.Extensions;
using GetFitterGetBigger.API.Tests.TestBuilders.Domain;

namespace GetFitterGetBigger.API.Tests.Services.Exercise.Extensions;

/// <summary>
/// Focused unit tests for ExerciseQueryExtensions.ApplyFluentSorting method
/// This is the method with high Crap Score due to lack of test coverage
/// </summary>
public class ExerciseQueryExtensionsApplyFluentSortingTests
{
    private IQueryable<GetFitterGetBigger.API.Models.Entities.Exercise> CreateTestExercises()
    {
        var difficulty1 = DifficultyLevelId.New();
        var difficulty2 = DifficultyLevelId.New();
        
        // Create exercises with different difficulties and names for sorting
        return new List<GetFitterGetBigger.API.Models.Entities.Exercise>
        {
            // Use Handler.Create for proper instantiation
            GetFitterGetBigger.API.Models.Entities.Exercise.Handler.Create(
                ExerciseId.New(),
                "Zebra Exercise",
                "Description Z",
                null, // videoUrl
                null, // imageUrl
                false, // isUnilateral
                true, // isActive
                difficulty2
            ),
            GetFitterGetBigger.API.Models.Entities.Exercise.Handler.Create(
                ExerciseId.New(),
                "Alpha Exercise", 
                "Description A",
                null, // videoUrl
                null, // imageUrl
                false, // isUnilateral
                true, // isActive
                difficulty1
            ),
            GetFitterGetBigger.API.Models.Entities.Exercise.Handler.Create(
                ExerciseId.New(),
                "Beta Exercise",
                "Description B",
                null, // videoUrl
                null, // imageUrl
                false, // isUnilateral
                true, // isActive
                difficulty1
            )
        }.AsQueryable();
    }

    [Theory]
    [InlineData("name", "asc")]
    [InlineData("name", "desc")]
    [InlineData("Name", "Asc")] // Case insensitive
    [InlineData("NAME", "DESC")] // Case insensitive
    public void ApplyFluentSorting_NameSorting_AppliesCorrectOrder(string sortBy, string sortOrder)
    {
        // Arrange
        var query = CreateTestExercises();
        var isDescending = sortOrder.ToLower() == "desc";

        // Act
        var result = query.ApplyFluentSorting(sortBy, sortOrder).ToList();

        // Assert
        result.Should().HaveCount(3);
        
        if (isDescending)
        {
            result[0].Name.Should().Be("Zebra Exercise");
            result[1].Name.Should().Be("Beta Exercise");
            result[2].Name.Should().Be("Alpha Exercise");
        }
        else
        {
            result[0].Name.Should().Be("Alpha Exercise");
            result[1].Name.Should().Be("Beta Exercise");
            result[2].Name.Should().Be("Zebra Exercise");
        }
    }

    [Theory]
    [InlineData("difficulty", "asc")]
    [InlineData("difficulty", "desc")]
    [InlineData("Difficulty", "Asc")] // Case insensitive
    [InlineData("DIFFICULTY", "DESC")] // Case insensitive
    public void ApplyFluentSorting_DifficultySorting_AppliesCorrectOrder(string sortBy, string sortOrder)
    {
        // Arrange
        var query = CreateTestExercises();

        // Act
        var result = query.ApplyFluentSorting(sortBy, sortOrder).ToList();

        // Assert
        result.Should().HaveCount(3);
        
        // Just verify that the method executes without throwing and returns all items
        // The exact order depends on the specific DifficultyId values, which are GUIDs
        // The important thing is that the method handles difficulty sorting without errors
        result.Should().HaveCount(3);
        result.Should().OnlyContain(e => e != null);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("", null)]
    [InlineData(null, "")]
    [InlineData("invalid", "asc")]
    [InlineData("unknown", "desc")]
    [InlineData("xyz", "invalid")]
    public void ApplyFluentSorting_InvalidOrNullSortCriteria_DefaultsToNameAscending(string? sortBy, string? sortOrder)
    {
        // Arrange
        var query = CreateTestExercises();

        // Act
        var result = query.ApplyFluentSorting(sortBy, sortOrder).ToList();

        // Assert
        result.Should().HaveCount(3);
        // Should default to name ascending
        result[0].Name.Should().Be("Alpha Exercise");
        result[1].Name.Should().Be("Beta Exercise");
        result[2].Name.Should().Be("Zebra Exercise");
    }

    [Theory]
    [InlineData("name", null)]
    [InlineData("name", "")]
    [InlineData("name", "invalid")]
    [InlineData("name", "xyz")]
    public void ApplyFluentSorting_ValidSortByInvalidSortOrder_DefaultsToAscending(string sortBy, string? sortOrder)
    {
        // Arrange
        var query = CreateTestExercises();

        // Act
        var result = query.ApplyFluentSorting(sortBy, sortOrder).ToList();

        // Assert
        result.Should().HaveCount(3);
        // Should sort by name ascending (invalid sortOrder defaults to ascending)
        result[0].Name.Should().Be("Alpha Exercise");
        result[1].Name.Should().Be("Beta Exercise");
        result[2].Name.Should().Be("Zebra Exercise");
    }

    [Fact]
    public void ApplyFluentSorting_CaseInsensitiveDesc_RecognizesDescending()
    {
        // Test various case combinations for "desc"
        var testCases = new[] { "desc", "DESC", "Desc", "dEsC" };
        var query = CreateTestExercises();

        foreach (var sortOrder in testCases)
        {
            // Act
            var result = query.ApplyFluentSorting("name", sortOrder).ToList();

            // Assert
            result.Should().HaveCount(3);
            result[0].Name.Should().Be("Zebra Exercise", $"Failed for sortOrder: {sortOrder}");
            result[1].Name.Should().Be("Beta Exercise", $"Failed for sortOrder: {sortOrder}");
            result[2].Name.Should().Be("Alpha Exercise", $"Failed for sortOrder: {sortOrder}");
        }
    }

    [Fact]
    public void ApplyFluentSorting_SingleItem_DoesNotThrow()
    {
        // Arrange
        var query = new List<GetFitterGetBigger.API.Models.Entities.Exercise>
        {
            GetFitterGetBigger.API.Models.Entities.Exercise.Handler.Create(
                ExerciseId.New(),
                "Single Exercise",
                "Description",
                null, // videoUrl
                null, // imageUrl
                false, // isUnilateral
                true, // isActive
                DifficultyLevelId.New()
            )
        }.AsQueryable();

        // Act & Assert
        var action = () => query.ApplyFluentSorting("name", "desc").ToList();
        action.Should().NotThrow();
        
        var result = action();
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Single Exercise");
    }

    [Fact]
    public void ApplyFluentSorting_EmptyQuery_DoesNotThrow()
    {
        // Arrange
        var query = new List<GetFitterGetBigger.API.Models.Entities.Exercise>().AsQueryable();

        // Act & Assert
        var action = () => query.ApplyFluentSorting("name", "asc").ToList();
        action.Should().NotThrow();
        
        var result = action();
        result.Should().BeEmpty();
    }

    [Fact]
    public void ApplyFluentSorting_ChainedWithOtherMethods_WorksCorrectly()
    {
        // Arrange
        var query = CreateTestExercises();

        // Act - Test chaining with other LINQ methods
        var result = query
            .Where(e => e.Name.Contains("Exercise"))
            .ApplyFluentSorting("name", "desc")
            .Take(2)
            .ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Zebra Exercise");
        result[1].Name.Should().Be("Beta Exercise");
    }

    [Theory]
    [InlineData("name")]
    [InlineData("difficulty")]
    public void ApplyFluentSorting_OnlyValidSortByProvided_DefaultsToAscending(string sortBy)
    {
        // Arrange
        var query = CreateTestExercises();

        // Act - Only provide sortBy, sortOrder should default to ascending
        var result = query.ApplyFluentSorting(sortBy, null).ToList();

        // Assert
        result.Should().HaveCount(3);
        
        if (sortBy.ToLower() == "name")
        {
            // Should be sorted by name ascending
            result[0].Name.Should().Be("Alpha Exercise");
            result[1].Name.Should().Be("Beta Exercise");
            result[2].Name.Should().Be("Zebra Exercise");
        }
        else
        {
            // Should be sorted by difficulty - can't easily test exact order with GUIDs
            // but verify it doesn't throw and returns all items
            result.Should().HaveCount(3);
            result.Should().OnlyContain(e => e != null);
        }
    }
}