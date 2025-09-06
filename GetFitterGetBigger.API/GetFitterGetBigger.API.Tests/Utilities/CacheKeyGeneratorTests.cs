using GetFitterGetBigger.API.DTOs.Interfaces;
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
        var result = CacheKeyGenerator.GetByValueKey(tableName, value!);

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

    #region Generate Method Tests (High Crap Score method - Complexity: 12)

    [Fact]
    public void Generate_WithNoParameters_ReturnsBaseKey()
    {
        // Act
        var result = CacheKeyGenerator.Generate<TestDto>("getByName");

        // Assert
        Assert.Equal("ReferenceTable:Tests:getByName", result);
    }

    [Fact]
    public void Generate_WithSingleParameter_CombinesCorrectly()
    {
        // Act
        var result = CacheKeyGenerator.Generate<TestDto>("getByValue", "TestValue");

        // Assert
        Assert.Equal("ReferenceTable:Tests:getByValue:testvalue", result);
    }

    [Fact]
    public void Generate_WithMultipleParameters_JoinsWithColons()
    {
        // Act
        var result = CacheKeyGenerator.Generate<TestDto>("getByComposite", "Param1", "Param2", "Param3");

        // Assert
        Assert.Equal("ReferenceTable:Tests:getByComposite:param1:param2:param3", result);
    }

    [Fact]
    public void Generate_WithMixedParameterTypes_ConvertsAllToString()
    {
        // Act
        var result = CacheKeyGenerator.Generate<TestDto>("getByMixed", 123, true, "Text");

        // Assert
        Assert.Equal("ReferenceTable:Tests:getByMixed:123:true:text", result);
    }

    [Fact]
    public void Generate_WithNullParameters_HandlesNullsAsEmpty()
    {
        // Act
        var result = CacheKeyGenerator.Generate<TestDto>("getByNulls", "Valid", null!, "Another");

        // Assert
        Assert.Equal("ReferenceTable:Tests:getByNulls:valid::another", result);
    }

    [Fact]
    public void Generate_WithEmptyParametersArray_ReturnsBaseKey()
    {
        // Act
        var result = CacheKeyGenerator.Generate<TestDto>("operation", new object[0]);

        // Assert
        Assert.Equal("ReferenceTable:Tests:operation", result);
    }

    [Fact]
    public void Generate_NormalizesToLowercase_EnsuresConsistentKeys()
    {
        // Act
        var result1 = CacheKeyGenerator.Generate<TestDto>("getByName", "VALUE");
        var result2 = CacheKeyGenerator.Generate<TestDto>("getByName", "Value");
        var result3 = CacheKeyGenerator.Generate<TestDto>("getByName", "value");

        // Assert - All should be identical
        Assert.Equal("ReferenceTable:Tests:getByName:value", result1);
        Assert.Equal("ReferenceTable:Tests:getByName:value", result2);
        Assert.Equal("ReferenceTable:Tests:getByName:value", result3);
        Assert.Equal(result1, result2);
        Assert.Equal(result2, result3);
    }

    [Fact]
    public void Generate_WithComplexObjects_UsesToStringMethod()
    {
        // Arrange
        var complexObject = new { Id = 123, Name = "Test" };

        // Act
        var result = CacheKeyGenerator.Generate<TestDto>("getByObject", complexObject);

        // Assert - Should use the ToString() representation (lowercased)
        Assert.Equal("ReferenceTable:Tests:getByObject:{ id = 123, name = test }", result);
    }

    [Theory]
    [InlineData("operation1", "ReferenceTable:Tests:operation1")]
    [InlineData("getAll", "ReferenceTable:Tests:getAll")]
    [InlineData("byId", "ReferenceTable:Tests:byId")]
    public void Generate_WithDifferentOperations_BuildsCorrectBaseKeys(string operation, string expected)
    {
        // Act
        var result = CacheKeyGenerator.Generate<TestDto>(operation);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Generate_ConsistentWithExplicitMethods_ProducesSameResults()
    {
        // This test ensures the refactored Generate method produces the same results
        // as the equivalent explicit methods for consistency

        // Test GetAll equivalence
        var generateAll = CacheKeyGenerator.GenerateForAll<TestDto>();
        var explicitAll = CacheKeyGenerator.GetAllKey("Tests");
        Assert.Equal(explicitAll, generateAll);

        // Test GetById equivalence
        var generateById = CacheKeyGenerator.GenerateForId<TestDto>("test-id");
        var explicitById = CacheKeyGenerator.GetByIdKey("Tests", "test-id");
        Assert.Equal(explicitById, generateById);

        // Test GetByValue equivalence
        var generateByValue = CacheKeyGenerator.GenerateForValue<TestDto>("test-value");
        var explicitByValue = CacheKeyGenerator.GetByValueKey("Tests", "test-value");
        Assert.Equal(explicitByValue, generateByValue);
    }

    #endregion

    #region Test DTOs

    // Test DTO that should pluralize to "Tests"
    private class TestDto : IEmptyDto<TestDto>
    {
        public static TestDto Empty => new();
        public bool IsEmpty => true;
    }

    #endregion
}