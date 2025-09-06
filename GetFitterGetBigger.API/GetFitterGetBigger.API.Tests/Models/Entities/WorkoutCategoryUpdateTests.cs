using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Constants;
using FluentAssertions;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Models.Entities;

/// <summary>
/// Tests for the refactored WorkoutCategory.Update method to verify complexity reduction
/// and comprehensive coverage of all code paths.
/// </summary>
public class WorkoutCategoryUpdateTests
{
    private WorkoutCategory CreateBaseCategory()
    {
        var result = WorkoutCategory.Handler.Create(
            WorkoutCategoryId.New(),
            "Original",
            "Original Description",
            "original-icon",
            "#FF0000",
            "Original Groups",
            1,
            true);
        
        return result.Value;
    }

    [Fact]
    public void Update_WithValidValues_ReturnsSuccessWithUpdatedCategory()
    {
        // Arrange
        var baseCategory = CreateBaseCategory();
        
        // Act
        var result = WorkoutCategory.Handler.Update(
            baseCategory,
            value: "Updated Value",
            description: "Updated Description", 
            icon: "updated-icon",
            color: "#00FF00",
            primaryMuscleGroups: "Updated Groups",
            displayOrder: 2,
            isActive: false);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("Updated Value");
        result.Value.Description.Should().Be("Updated Description");
        result.Value.Icon.Should().Be("updated-icon");
        result.Value.Color.Should().Be("#00FF00");
        result.Value.PrimaryMuscleGroups.Should().Be("Updated Groups");
        result.Value.DisplayOrder.Should().Be(2);
        result.Value.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Update_WithNullValues_KeepsOriginalValues()
    {
        // Arrange
        var baseCategory = CreateBaseCategory();
        
        // Act
        var result = WorkoutCategory.Handler.Update(
            baseCategory,
            value: null,
            description: null,
            icon: null,
            color: null,
            primaryMuscleGroups: null,
            displayOrder: null,
            isActive: null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(baseCategory.Value);
        result.Value.Description.Should().Be(baseCategory.Description);
        result.Value.Icon.Should().Be(baseCategory.Icon);
        result.Value.Color.Should().Be(baseCategory.Color);
        result.Value.PrimaryMuscleGroups.Should().Be(baseCategory.PrimaryMuscleGroups);
        result.Value.DisplayOrder.Should().Be(baseCategory.DisplayOrder);
        result.Value.IsActive.Should().Be(baseCategory.IsActive);
    }

    [Fact]
    public void Update_WithMixedNullAndValues_UpdatesOnlyProvidedValues()
    {
        // Arrange
        var baseCategory = CreateBaseCategory();
        
        // Act
        var result = WorkoutCategory.Handler.Update(
            baseCategory,
            value: "New Value",
            description: null, // Keep original
            icon: "new-icon",
            color: null, // Keep original
            primaryMuscleGroups: "New Groups",
            displayOrder: null, // Keep original
            isActive: false);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("New Value");
        result.Value.Description.Should().Be(baseCategory.Description, "null description should keep original");
        result.Value.Icon.Should().Be("new-icon");
        result.Value.Color.Should().Be(baseCategory.Color, "null color should keep original");
        result.Value.PrimaryMuscleGroups.Should().Be("New Groups");
        result.Value.DisplayOrder.Should().Be(baseCategory.DisplayOrder, "null displayOrder should keep original");
        result.Value.IsActive.Should().BeFalse();
    }

    [Theory]
    [InlineData("", WorkoutCategoryErrorMessages.ValueCannotBeEmpty)]
    [InlineData("   ", WorkoutCategoryErrorMessages.ValueCannotBeEmpty)]
    public void Update_WithInvalidValue_ReturnsFailure(string? invalidValue, string expectedError)
    {
        // Arrange
        var baseCategory = CreateBaseCategory();
        
        // Act
        var result = WorkoutCategory.Handler.Update(baseCategory, value: invalidValue);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(expectedError);
    }

    [Fact]
    public void Update_WithValueTooLong_ReturnsFailure()
    {
        // Arrange
        var baseCategory = CreateBaseCategory();
        var longValue = new string('a', 101); // Exceeds max length of 100

        // Act
        var result = WorkoutCategory.Handler.Update(baseCategory, value: longValue);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(WorkoutCategoryErrorMessages.ValueExceedsMaxLength);
    }

    [Theory]
    [InlineData("", WorkoutCategoryErrorMessages.IconIsRequired)]
    [InlineData("   ", WorkoutCategoryErrorMessages.IconIsRequired)]
    public void Update_WithInvalidIcon_ReturnsFailure(string? invalidIcon, string expectedError)
    {
        // Arrange
        var baseCategory = CreateBaseCategory();
        
        // Act
        var result = WorkoutCategory.Handler.Update(baseCategory, icon: invalidIcon);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(expectedError);
    }

    [Fact]
    public void Update_WithIconTooLong_ReturnsFailure()
    {
        // Arrange
        var baseCategory = CreateBaseCategory();
        var longIcon = new string('a', 51); // Exceeds max length of 50

        // Act
        var result = WorkoutCategory.Handler.Update(baseCategory, icon: longIcon);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(WorkoutCategoryErrorMessages.IconExceedsMaxLength);
    }

    [Theory]
    [InlineData("", WorkoutCategoryErrorMessages.ColorIsRequired)]
    [InlineData("   ", WorkoutCategoryErrorMessages.ColorIsRequired)]
    public void Update_WithInvalidColor_ReturnsFailure(string? invalidColor, string expectedError)
    {
        // Arrange
        var baseCategory = CreateBaseCategory();
        
        // Act
        var result = WorkoutCategory.Handler.Update(baseCategory, color: invalidColor);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(expectedError);
    }

    [Theory]
    [InlineData("red", WorkoutCategoryErrorMessages.InvalidHexColorCode)]
    [InlineData("#GG0000", WorkoutCategoryErrorMessages.InvalidHexColorCode)]
    [InlineData("#12345", WorkoutCategoryErrorMessages.InvalidHexColorCode)]
    [InlineData("#1234567", WorkoutCategoryErrorMessages.InvalidHexColorCode)]
    public void Update_WithInvalidHexColor_ReturnsFailure(string invalidColor, string expectedError)
    {
        // Arrange
        var baseCategory = CreateBaseCategory();
        
        // Act
        var result = WorkoutCategory.Handler.Update(baseCategory, color: invalidColor);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(expectedError);
    }

    [Theory]
    [InlineData("#FF0000")] // 6-digit hex
    [InlineData("#00FF00")] // 6-digit hex
    [InlineData("#ABC")]    // 3-digit hex
    [InlineData("#123")]    // 3-digit hex
    [InlineData("#abc")]    // 3-digit hex lowercase
    [InlineData("#ff0000")] // 6-digit hex lowercase
    public void Update_WithValidHexColors_ReturnsSuccess(string validColor)
    {
        // Arrange
        var baseCategory = CreateBaseCategory();
        
        // Act
        var result = WorkoutCategory.Handler.Update(baseCategory, color: validColor);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Color.Should().Be(validColor);
    }

    [Fact]
    public void Update_WithNegativeDisplayOrder_ReturnsFailure()
    {
        // Arrange
        var baseCategory = CreateBaseCategory();
        
        // Act
        var result = WorkoutCategory.Handler.Update(baseCategory, displayOrder: -1);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(WorkoutCategoryErrorMessages.DisplayOrderMustBeNonNegative);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public void Update_WithValidDisplayOrders_ReturnsSuccess(int validDisplayOrder)
    {
        // Arrange
        var baseCategory = CreateBaseCategory();
        
        // Act
        var result = WorkoutCategory.Handler.Update(baseCategory, displayOrder: validDisplayOrder);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.DisplayOrder.Should().Be(validDisplayOrder);
    }

    [Fact]
    public void Update_WithMultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var baseCategory = CreateBaseCategory();
        
        // Act
        var result = WorkoutCategory.Handler.Update(
            baseCategory,
            value: "", // Invalid
            icon: "", // Invalid
            color: "invalid", // Invalid
            displayOrder: -1); // Invalid

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(4);
        result.Errors.Should().Contain(WorkoutCategoryErrorMessages.ValueCannotBeEmpty);
        result.Errors.Should().Contain(WorkoutCategoryErrorMessages.IconIsRequired);
        result.Errors.Should().Contain(WorkoutCategoryErrorMessages.InvalidHexColorCode);
        result.Errors.Should().Contain(WorkoutCategoryErrorMessages.DisplayOrderMustBeNonNegative);
    }

    [Fact]
    public void Update_PreservesOriginalId_DoesNotChangeId()
    {
        // Arrange
        var baseCategory = CreateBaseCategory();
        
        // Act
        var result = WorkoutCategory.Handler.Update(
            baseCategory,
            value: "Updated Value");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.WorkoutCategoryId.Should().Be(baseCategory.WorkoutCategoryId);
    }
}