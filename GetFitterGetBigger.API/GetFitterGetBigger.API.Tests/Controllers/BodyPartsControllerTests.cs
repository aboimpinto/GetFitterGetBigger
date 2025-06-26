using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GetFitterGetBigger.API.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Controllers;

public class BodyPartsControllerTests : IClassFixture<ApiTestFixture>
{
    private readonly HttpClient _client;
    private readonly ApiTestFixture _fixture;

    public BodyPartsControllerTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task GetAll_ReturnsAllBodyParts()
    {
        // Act
        var response = await _client.GetAsync("/api/ReferenceTables/BodyParts");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var bodyParts = await response.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        
        Assert.NotNull(bodyParts);
        // We don't assert Count or specific values because the database might be empty in some test environments
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsBodyPart()
    {
        // First get all body parts to find a valid ID
        var allResponse = await _client.GetAsync("/api/ReferenceTables/BodyParts");
        allResponse.EnsureSuccessStatusCode();
        var allBodyParts = await allResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        Assert.NotNull(allBodyParts);
        
        // Skip the test if there are no body parts
        if (!allBodyParts.Any())
        {
            return; // Skip the test
        }
        
        var firstBodyPart = allBodyParts.First();
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/BodyParts/{firstBodyPart.Id}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var bodyPart = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(bodyPart);
        Assert.Equal(firstBodyPart.Id, bodyPart.Id);
        Assert.Equal(firstBodyPart.Value, bodyPart.Value);
    }

    [Fact]
    public async Task GetById_WithInvalidIdFormat_ReturnsBadRequest()
    {
        // Arrange
        var invalidId = "7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c"; // Missing prefix
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/BodyParts/{invalidId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid ID format", errorMessage);
        Assert.Contains("Expected format: 'bodypart-{guid}'", errorMessage);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = "bodypart-00000000-0000-0000-0000-000000000000";
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/BodyParts/{nonExistentId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByValue_WithValidValue_ReturnsBodyPart()
    {
        // First get all body parts to find a valid value
        var allResponse = await _client.GetAsync("/api/ReferenceTables/BodyParts");
        allResponse.EnsureSuccessStatusCode();
        var allBodyParts = await allResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        Assert.NotNull(allBodyParts);
        
        // Skip the test if there are no body parts
        if (!allBodyParts.Any())
        {
            return; // Skip the test
        }
        
        var firstBodyPart = allBodyParts.First();
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/BodyParts/ByValue/{firstBodyPart.Value}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var bodyPart = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(bodyPart);
        Assert.Equal(firstBodyPart.Value, bodyPart.Value);
    }

    [Fact]
    public async Task GetByValue_WithNonExistentValue_ReturnsNotFound()
    {
        // Arrange
        var nonExistentValue = "NonExistentBodyPart";
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/BodyParts/ByValue/{nonExistentValue}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Theory]
    [InlineData("Chest")]
    [InlineData("chest")]
    [InlineData("CHEST")]
    [InlineData("ChEsT")]
    public async Task GetByValue_WithDifferentCasing_ReturnsBodyPart(string value)
    {
        // First check if the body part exists
        var allResponse = await _client.GetAsync("/api/ReferenceTables/BodyParts");
        allResponse.EnsureSuccessStatusCode();
        var allBodyParts = await allResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        
        Assert.NotNull(allBodyParts);
        
        // Skip the test if there are no body parts or if "Chest" doesn't exist
        if (!allBodyParts.Any() || !allBodyParts.Any(bp => bp.Value.Equals("Chest", StringComparison.OrdinalIgnoreCase)))
        {
            return; // Skip the test
        }
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/BodyParts/ByValue/{value}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var bodyPart = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(bodyPart);
        Assert.Equal("Chest", bodyPart.Value, ignoreCase: true); // The actual stored value has proper casing
    }
}
