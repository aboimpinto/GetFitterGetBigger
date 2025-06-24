using System.Net.Http.Json;
using System.Text.Json;
using GetFitterGetBigger.API.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GetFitterGetBigger.API.Tests;

public class ReferenceDataDtoTests : IClassFixture<ApiTestFixture>
{
    private readonly HttpClient _client;
    private readonly ApiTestFixture _fixture;

    public ReferenceDataDtoTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Theory]
    [InlineData("/api/ReferenceTables/BodyParts", "bodypart-")]
    [InlineData("/api/ReferenceTables/DifficultyLevels", "difficultylevel-")]
    [InlineData("/api/ReferenceTables/KineticChainTypes", "kineticchaintype-")]
    [InlineData("/api/ReferenceTables/MuscleRoles", "musclerole-")]
    [InlineData("/api/ReferenceTables/Equipment", "equipment-")]
    [InlineData("/api/ReferenceTables/MetricTypes", "metrictype-")]
    [InlineData("/api/ReferenceTables/MovementPatterns", "movementpattern-")]
    [InlineData("/api/ReferenceTables/MuscleGroups", "musclegroup-")]
    public async Task GetAll_ReturnsCorrectlyFormattedIds(string endpoint, string expectedPrefix)
    {
        // Act
        var response = await _client.GetAsync(endpoint);
        
        // Assert
        response.EnsureSuccessStatusCode();
        
        // Get the raw JSON to check the exact structure
        var jsonString = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(jsonString);
        var root = jsonDoc.RootElement;
        
        // Verify it's an array
        Assert.Equal(JsonValueKind.Array, root.ValueKind);
        
        // Check each item in the array
        foreach (var item in root.EnumerateArray())
        {
            // Verify the ID is formatted correctly
            Assert.True(item.TryGetProperty("id", out var idProperty), "JSON should contain an 'id' property");
            Assert.Equal(JsonValueKind.String, idProperty.ValueKind);
            var id = idProperty.GetString();
            Assert.NotNull(id);
            Assert.StartsWith(expectedPrefix, id);
            
            // Verify the value property exists
            Assert.True(item.TryGetProperty("value", out var valueProperty), "JSON should contain a 'value' property");
            
            // Verify description property exists (may be null)
            Assert.True(item.TryGetProperty("description", out _), "JSON should contain a 'description' property");
            
            // Verify displayOrder and isActive properties do NOT exist
            Assert.False(item.TryGetProperty("displayOrder", out _), "JSON should NOT contain a 'displayOrder' property");
            Assert.False(item.TryGetProperty("isActive", out _), "JSON should NOT contain an 'isActive' property");
            
            // Verify no other properties exist
            var propertyCount = 0;
            foreach (var _ in item.EnumerateObject())
            {
                propertyCount++;
            }
            Assert.Equal(3, propertyCount);
            Assert.True(propertyCount == 3, "JSON should contain exactly 3 properties: id, value, and description");
        }
    }

    [Fact]
    public async Task GetById_VerifyJsonStructure()
    {
        // First get all reference data types to find valid IDs
        var endpoints = new[]
        {
            "/api/ReferenceTables/BodyParts",
            "/api/ReferenceTables/DifficultyLevels",
            "/api/ReferenceTables/KineticChainTypes",
            "/api/ReferenceTables/MuscleRoles",
            "/api/ReferenceTables/Equipment",
            "/api/ReferenceTables/MetricTypes",
            "/api/ReferenceTables/MovementPatterns",
            "/api/ReferenceTables/MuscleGroups"
        };
        
        foreach (var endpoint in endpoints)
        {
            // Get all items of this type
            var allResponse = await _client.GetAsync(endpoint);
            allResponse.EnsureSuccessStatusCode();
            var allItems = await allResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
            Assert.NotNull(allItems);
            
            // Skip if there are no items
            if (!allItems.Any())
            {
                continue;
            }
            
            // Get the first item
            var firstItem = allItems.First();
            
            // Act - get by ID
            var response = await _client.GetAsync($"{endpoint}/{firstItem.Id}");
            
            // Assert
            response.EnsureSuccessStatusCode();
            
            // Get the raw JSON to check the exact structure
            var jsonString = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(jsonString);
            var root = jsonDoc.RootElement;
            
            // Verify it's an object
            Assert.Equal(JsonValueKind.Object, root.ValueKind);
            
            // Verify the ID is formatted correctly
            Assert.True(root.TryGetProperty("id", out var idProperty), "JSON should contain an 'id' property");
            Assert.Equal(JsonValueKind.String, idProperty.ValueKind);
            var id = idProperty.GetString();
            Assert.NotNull(id);
            Assert.Equal(firstItem.Id, id);
            
            // Verify the value property exists
            Assert.True(root.TryGetProperty("value", out var valueProperty), "JSON should contain a 'value' property");
            
            // Verify description property exists (may be null)
            Assert.True(root.TryGetProperty("description", out _), "JSON should contain a 'description' property");
            
            // Verify displayOrder and isActive properties do NOT exist
            Assert.False(root.TryGetProperty("displayOrder", out _), "JSON should NOT contain a 'displayOrder' property");
            Assert.False(root.TryGetProperty("isActive", out _), "JSON should NOT contain an 'isActive' property");
            
            // Verify no other properties exist
            var propertyCount = 0;
            foreach (var _ in root.EnumerateObject())
            {
                propertyCount++;
            }
            Assert.Equal(3, propertyCount);
            Assert.True(propertyCount == 3, "JSON should contain exactly 3 properties: id, value, and description");
        }
    }
}
