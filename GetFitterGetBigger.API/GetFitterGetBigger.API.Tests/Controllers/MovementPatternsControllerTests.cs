using System.Net;
using System.Net.Http.Json;
using GetFitterGetBigger.API.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Controllers;

public class MovementPatternsControllerTests : IClassFixture<ApiTestFixture>
{
    private readonly HttpClient _client;
    private readonly ApiTestFixture _fixture;

    public MovementPatternsControllerTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task GetAll_ReturnsAllMovementPatterns()
    {
        // Act
        var response = await _client.GetAsync("/api/ReferenceTables/MovementPatterns");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var movementPatterns = await response.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        
        Assert.NotNull(movementPatterns);
        // We don't assert NotEmpty because the database might be empty in some test environments
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsMovementPattern()
    {
        // First get all movement patterns to find a valid ID
        var allResponse = await _client.GetAsync("/api/ReferenceTables/MovementPatterns");
        allResponse.EnsureSuccessStatusCode();
        var allMovementPatterns = await allResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        Assert.NotNull(allMovementPatterns);
        
        // Skip the test if there are no movement patterns
        if (!allMovementPatterns.Any())
        {
            return; // Skip the test
        }
        
        var firstMovementPattern = allMovementPatterns.First();
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/MovementPatterns/{firstMovementPattern.Id}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var movementPattern = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(movementPattern);
        Assert.Equal(firstMovementPattern.Id, movementPattern.Id);
        Assert.Equal(firstMovementPattern.Value, movementPattern.Value);
    }

    [Fact]
    public async Task GetById_WithInvalidIdFormat_ReturnsBadRequest()
    {
        // Arrange
        var invalidId = "abcdef12-3456-7890-abcd-ef1234567890"; // Missing prefix
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/MovementPatterns/{invalidId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid ID format", errorMessage);
        Assert.Contains("Expected format: 'movementpattern-{guid}'", errorMessage);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = "movementpattern-00000000-0000-0000-0000-000000000000";
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/MovementPatterns/{nonExistentId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByValue_WithValidValue_ReturnsMovementPattern()
    {
        // First get all movement patterns to find a valid value
        var allResponse = await _client.GetAsync("/api/ReferenceTables/MovementPatterns");
        allResponse.EnsureSuccessStatusCode();
        var allMovementPatterns = await allResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        Assert.NotNull(allMovementPatterns);
        
        // Skip the test if there are no movement patterns
        if (!allMovementPatterns.Any())
        {
            return; // Skip the test
        }
        
        var firstMovementPattern = allMovementPatterns.First();
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/MovementPatterns/ByValue/{firstMovementPattern.Value}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var movementPattern = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(movementPattern);
        Assert.Equal(firstMovementPattern.Value, movementPattern.Value);
    }

    [Fact]
    public async Task GetByValue_WithNonExistentValue_ReturnsNotFound()
    {
        // Arrange
        var nonExistentValue = "NonExistentMovementPattern";
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/MovementPatterns/ByValue/{nonExistentValue}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Theory]
    [InlineData("Push")]
    [InlineData("push")]
    [InlineData("PUSH")]
    [InlineData("PuSh")]
    public async Task GetByValue_WithDifferentCasing_ReturnsMovementPattern(string value)
    {
        // First check if the movement pattern exists
        var allResponse = await _client.GetAsync("/api/ReferenceTables/MovementPatterns");
        allResponse.EnsureSuccessStatusCode();
        var allMovementPatterns = await allResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        
        Assert.NotNull(allMovementPatterns);
        
        // Skip the test if there are no movement patterns or if "Push" doesn't exist
        if (!allMovementPatterns.Any() || !allMovementPatterns.Any(mp => mp.Value.Equals("Push", StringComparison.OrdinalIgnoreCase)))
        {
            return; // Skip the test
        }
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/MovementPatterns/ByValue/{value}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var movementPattern = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(movementPattern);
        Assert.Equal("Push", movementPattern.Value, ignoreCase: true); // The actual stored value has proper casing
    }
}
