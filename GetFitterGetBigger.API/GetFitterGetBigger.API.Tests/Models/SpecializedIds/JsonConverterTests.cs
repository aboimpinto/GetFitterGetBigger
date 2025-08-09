using System.Text.Json;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Models.SpecializedIds.JsonConverters;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Models.SpecializedIds;

/// <summary>
/// Tests for JSON converters to improve branch coverage.
/// These converters handle serialization/deserialization of specialized ID types.
/// </summary>
public class JsonConverterTests
{
    #region ClaimIdJsonConverter Tests

    [Fact]
    public void ClaimIdJsonConverter_SerializeValidId_ProducesCorrectJson()
    {
        // Arrange
        var claimId = ClaimId.New();
        var options = new JsonSerializerOptions();
        options.Converters.Add(new ClaimIdJsonConverter());

        // Act
        var json = JsonSerializer.Serialize(claimId, options);
        
        // Assert
        Assert.Equal($"\"{claimId}\"", json);
    }

    [Fact]
    public void ClaimIdJsonConverter_DeserializeValidJson_ProducesCorrectId()
    {
        // Arrange
        var originalId = ClaimId.New();
        var json = $"\"{originalId}\"";
        var options = new JsonSerializerOptions();
        options.Converters.Add(new ClaimIdJsonConverter());

        // Act
        var deserializedId = JsonSerializer.Deserialize<ClaimId>(json, options);
        
        // Assert
        Assert.Equal(originalId, deserializedId);
    }

    [Fact]
    public void ClaimIdJsonConverter_DeserializeNull_ThrowsJsonException()
    {
        // Arrange
        var json = "null";
        var options = new JsonSerializerOptions();
        options.Converters.Add(new ClaimIdJsonConverter());

        // Act & Assert - The converter doesn't handle null tokens, it expects a string
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<ClaimId>(json, options));
    }

    [Fact]
    public void ClaimIdJsonConverter_DeserializeInvalidFormat_ThrowsJsonException()
    {
        // Arrange
        var json = "\"invalid-format\"";
        var options = new JsonSerializerOptions();
        options.Converters.Add(new ClaimIdJsonConverter());

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<ClaimId>(json, options));
    }

    [Fact]
    public void ClaimIdJsonConverter_DeserializeNonStringToken_ThrowsJsonException()
    {
        // Arrange
        var json = "123"; // Number instead of string
        var options = new JsonSerializerOptions();
        options.Converters.Add(new ClaimIdJsonConverter());

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<ClaimId>(json, options));
    }

    #endregion

    #region UserIdJsonConverter Tests

    [Fact]
    public void UserIdJsonConverter_SerializeValidId_ProducesCorrectJson()
    {
        // Arrange
        var userId = UserId.New();
        var options = new JsonSerializerOptions();
        options.Converters.Add(new UserIdJsonConverter());

        // Act
        var json = JsonSerializer.Serialize(userId, options);
        
        // Assert
        Assert.Equal($"\"{userId}\"", json);
    }

    [Fact]
    public void UserIdJsonConverter_DeserializeValidJson_ProducesCorrectId()
    {
        // Arrange
        var originalId = UserId.New();
        var json = $"\"{originalId}\"";
        var options = new JsonSerializerOptions();
        options.Converters.Add(new UserIdJsonConverter());

        // Act
        var deserializedId = JsonSerializer.Deserialize<UserId>(json, options);
        
        // Assert
        Assert.Equal(originalId, deserializedId);
    }

    [Fact]
    public void UserIdJsonConverter_DeserializeNull_ThrowsJsonException()
    {
        // Arrange
        var json = "null";
        var options = new JsonSerializerOptions();
        options.Converters.Add(new UserIdJsonConverter());

        // Act & Assert - The converter doesn't handle null tokens, it expects a string
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<UserId>(json, options));
    }

    [Fact]
    public void UserIdJsonConverter_DeserializeInvalidFormat_ThrowsJsonException()
    {
        // Arrange
        var json = "\"not-a-user-id\"";
        var options = new JsonSerializerOptions();
        options.Converters.Add(new UserIdJsonConverter());

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<UserId>(json, options));
    }

    [Fact]
    public void UserIdJsonConverter_DeserializeEmptyString_ThrowsJsonException()
    {
        // Arrange
        var json = "\"\"";
        var options = new JsonSerializerOptions();
        options.Converters.Add(new UserIdJsonConverter());

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<UserId>(json, options));
    }

    #endregion

    #region Complex Object Serialization Tests

    [Fact]
    public void ComplexObject_WithClaimIdProperty_SerializesCorrectly()
    {
        // Arrange
        var testObject = new TestObjectWithIds
            {
            ClaimId = ClaimId.New(),
            UserId = UserId.New(),
            Name = "Test Object"
        };

        var options = new JsonSerializerOptions();
        options.Converters.Add(new ClaimIdJsonConverter());
        options.Converters.Add(new UserIdJsonConverter());

        // Act
        var json = JsonSerializer.Serialize(testObject, options);
        var deserialized = JsonSerializer.Deserialize<TestObjectWithIds>(json, options);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(testObject.ClaimId, deserialized.ClaimId);
        Assert.Equal(testObject.UserId, deserialized.UserId);
        Assert.Equal(testObject.Name, deserialized.Name);
    }

    [Fact]
    public void NullableId_SerializesAsNull()
    {
        // Arrange
        var testObject = new TestObjectWithNullableIds
            {
            ClaimId = null,
            UserId = UserId.New(),
            Name = "Test"
        };

        var options = new JsonSerializerOptions();
        options.Converters.Add(new ClaimIdJsonConverter());
        options.Converters.Add(new UserIdJsonConverter());

        // Act
        var json = JsonSerializer.Serialize(testObject, options);
        var deserialized = JsonSerializer.Deserialize<TestObjectWithNullableIds>(json, options);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Null(deserialized.ClaimId);
        Assert.Equal(testObject.UserId, deserialized.UserId);
    }

    #endregion

    #region Helper Classes

    private class TestObjectWithIds
    {
        public ClaimId ClaimId { get; set; }
        public UserId UserId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class TestObjectWithNullableIds
    {
        public ClaimId? ClaimId { get; set; }
        public UserId UserId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    #endregion
}