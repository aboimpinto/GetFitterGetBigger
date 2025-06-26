using GetFitterGetBigger.API.Utilities;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Utilities;

public class CacheKeyGeneratorTests
{
    [Fact]
    public void GetAllKey_ReturnsCorrectFormat()
    {
        // Arrange
        var tableName = "DifficultyLevels";

        // Act
        var result = CacheKeyGenerator.GetAllKey(tableName);

        // Assert
        Assert.Equal("ReferenceTable:DifficultyLevels:GetAll", result);
    }

    [Fact]
    public void GetByIdKey_ReturnsCorrectFormat()
    {
        // Arrange
        var tableName = "Equipment";
        var id = "equipment-12345";

        // Act
        var result = CacheKeyGenerator.GetByIdKey(tableName, id);

        // Assert
        Assert.Equal("ReferenceTable:Equipment:GetById:equipment-12345", result);
    }

    [Fact]
    public void GetByValueKey_ReturnsCorrectFormat()
    {
        // Arrange
        var tableName = "MuscleGroups";
        var value = "Biceps";

        // Act
        var result = CacheKeyGenerator.GetByValueKey(tableName, value);

        // Assert
        Assert.Equal("ReferenceTable:MuscleGroups:GetByValue:biceps", result);
    }

    [Fact]
    public void GetByValueKey_NormalizesToLowerCase()
    {
        // Arrange
        var tableName = "BodyParts";
        var value1 = "CHEST";
        var value2 = "Chest";
        var value3 = "chest";

        // Act
        var result1 = CacheKeyGenerator.GetByValueKey(tableName, value1);
        var result2 = CacheKeyGenerator.GetByValueKey(tableName, value2);
        var result3 = CacheKeyGenerator.GetByValueKey(tableName, value3);

        // Assert
        Assert.Equal("ReferenceTable:BodyParts:GetByValue:chest", result1);
        Assert.Equal("ReferenceTable:BodyParts:GetByValue:chest", result2);
        Assert.Equal("ReferenceTable:BodyParts:GetByValue:chest", result3);
    }

    [Fact]
    public void GetByValueKey_HandlesNullValue()
    {
        // Arrange
        var tableName = "MetricTypes";
        string? value = null;

        // Act
        var result = CacheKeyGenerator.GetByValueKey(tableName, value);

        // Assert
        Assert.Equal("ReferenceTable:MetricTypes:GetByValue:", result);
    }

    [Fact]
    public void GetTablePattern_ReturnsCorrectFormat()
    {
        // Arrange
        var tableName = "MovementPatterns";

        // Act
        var result = CacheKeyGenerator.GetTablePattern(tableName);

        // Assert
        Assert.Equal("ReferenceTable:MovementPatterns:", result);
    }

    [Theory]
    [InlineData("DifficultyLevels")]
    [InlineData("KineticChainTypes")]
    [InlineData("BodyParts")]
    [InlineData("MuscleRoles")]
    [InlineData("Equipment")]
    [InlineData("MetricTypes")]
    [InlineData("MovementPatterns")]
    [InlineData("MuscleGroups")]
    public void AllMethods_WorkWithAllTableNames(string tableName)
    {
        // Act & Assert
        var getAllKey = CacheKeyGenerator.GetAllKey(tableName);
        Assert.Contains(tableName, getAllKey);
        Assert.StartsWith("ReferenceTable:", getAllKey);
        Assert.EndsWith(":GetAll", getAllKey);

        var getByIdKey = CacheKeyGenerator.GetByIdKey(tableName, "test-id");
        Assert.Contains(tableName, getByIdKey);
        Assert.Contains(":GetById:", getByIdKey);

        var getByValueKey = CacheKeyGenerator.GetByValueKey(tableName, "test-value");
        Assert.Contains(tableName, getByValueKey);
        Assert.Contains(":GetByValue:", getByValueKey);

        var pattern = CacheKeyGenerator.GetTablePattern(tableName);
        Assert.StartsWith("ReferenceTable:", pattern);
        Assert.EndsWith($"{tableName}:", pattern);
    }
}