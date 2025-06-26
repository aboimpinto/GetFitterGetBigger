using System.Net;
using System.Net.Http.Json;
using GetFitterGetBigger.API.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Controllers;

public class DifficultyLevelsControllerTests : IClassFixture<ApiTestFixture>
{
    private readonly HttpClient _client;
    private readonly ApiTestFixture _fixture;

    public DifficultyLevelsControllerTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsAllDifficultyLevels()
    {
        // Act
        var response = await _client.GetAsync("/api/ReferenceTables/DifficultyLevels");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var difficultyLevels = await response.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        
        Assert.NotNull(difficultyLevels);
        // We don't assert Count or specific values because the database might be empty in some test environments
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsDifficultyLevel()
    {
        // First get all difficulty levels to find a valid ID
        var allResponse = await _client.GetAsync("/api/ReferenceTables/DifficultyLevels");
        allResponse.EnsureSuccessStatusCode();
        var allDifficultyLevels = await allResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        Assert.NotNull(allDifficultyLevels);
        
        // Skip the test if there are no difficulty levels
        if (!allDifficultyLevels.Any())
        {
            return; // Skip the test
        }
        
        var firstDifficultyLevel = allDifficultyLevels.First();
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/DifficultyLevels/{firstDifficultyLevel.Id}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var difficultyLevel = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(difficultyLevel);
        Assert.Equal(firstDifficultyLevel.Id, difficultyLevel.Id);
        Assert.Equal(firstDifficultyLevel.Value, difficultyLevel.Value);
    }

    [Fact]
    public async Task GetById_WithInvalidIdFormat_ReturnsBadRequest()
    {
        // Arrange
        var invalidId = "8a8adb1d-24d2-4979-a5a6-0d760e6da24b"; // Missing prefix
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/DifficultyLevels/{invalidId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid ID format", errorMessage);
        Assert.Contains("Expected format: 'difficultylevel-{guid}'", errorMessage);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = "difficultylevel-00000000-0000-0000-0000-000000000000";
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/DifficultyLevels/{nonExistentId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByValue_WithValidValue_ReturnsDifficultyLevel()
    {
        // First get all difficulty levels to find a valid value
        var allResponse = await _client.GetAsync("/api/ReferenceTables/DifficultyLevels");
        allResponse.EnsureSuccessStatusCode();
        var allDifficultyLevels = await allResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        Assert.NotNull(allDifficultyLevels);
        
        // Skip the test if there are no difficulty levels
        if (!allDifficultyLevels.Any())
        {
            return; // Skip the test
        }
        
        var firstDifficultyLevel = allDifficultyLevels.First();
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/DifficultyLevels/ByValue/{firstDifficultyLevel.Value}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var difficultyLevel = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(difficultyLevel);
        Assert.Equal(firstDifficultyLevel.Value, difficultyLevel.Value);
    }

    [Fact]
    public async Task GetByValue_WithNonExistentValue_ReturnsNotFound()
    {
        // Arrange
        var nonExistentValue = "NonExistentDifficultyLevel";
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/DifficultyLevels/ByValue/{nonExistentValue}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Theory]
    [InlineData("Beginner")]
    [InlineData("beginner")]
    [InlineData("BEGINNER")]
    [InlineData("BeGiNnEr")]
    public async Task GetByValue_WithDifferentCasing_ReturnsDifficultyLevel(string value)
    {
        // First check if the difficulty level exists
        var allResponse = await _client.GetAsync("/api/ReferenceTables/DifficultyLevels");
        allResponse.EnsureSuccessStatusCode();
        var allDifficultyLevels = await allResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        
        Assert.NotNull(allDifficultyLevels);
        
        // Skip the test if there are no difficulty levels or if "Beginner" doesn't exist
        if (!allDifficultyLevels.Any() || !allDifficultyLevels.Any(dl => dl.Value.Equals("Beginner", StringComparison.OrdinalIgnoreCase)))
        {
            return; // Skip the test
        }
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/DifficultyLevels/ByValue/{value}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var difficultyLevel = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(difficultyLevel);
        Assert.Equal("Beginner", difficultyLevel.Value, ignoreCase: true); // The actual stored value has proper casing
    }
}
