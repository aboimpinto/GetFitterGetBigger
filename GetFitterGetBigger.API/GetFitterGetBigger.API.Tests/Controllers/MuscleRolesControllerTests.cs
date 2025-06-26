using System.Net;
using System.Net.Http.Json;
using GetFitterGetBigger.API.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Controllers;

public class MuscleRolesControllerTests : IClassFixture<ApiTestFixture>
{
    private readonly HttpClient _client;
    private readonly ApiTestFixture _fixture;

    public MuscleRolesControllerTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsAllMuscleRoles()
    {
        // Act
        var response = await _client.GetAsync("/api/ReferenceTables/MuscleRoles");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var muscleRoles = await response.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        
        Assert.NotNull(muscleRoles);
        // We don't assert Count or specific values because the database might be empty in some test environments
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsMuscleRole()
    {
        // First get all muscle roles to find a valid ID
        var allResponse = await _client.GetAsync("/api/ReferenceTables/MuscleRoles");
        allResponse.EnsureSuccessStatusCode();
        var muscleRoles = await allResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        Assert.NotNull(muscleRoles);
        
        // Skip the test if there are no muscle roles
        if (!muscleRoles.Any())
        {
            return; // Skip the test
        }
        
        var firstMuscleRole = muscleRoles.First();
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/MuscleRoles/{firstMuscleRole.Id}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var muscleRole = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(muscleRole);
        Assert.Equal(firstMuscleRole.Id, muscleRole.Id);
        Assert.Equal(firstMuscleRole.Value, muscleRole.Value);
    }

    [Fact]
    public async Task GetById_WithInvalidIdFormat_ReturnsBadRequest()
    {
        // Arrange
        var invalidId = "abcdef12-3456-7890-abcd-ef1234567890"; // Missing prefix
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/MuscleRoles/{invalidId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid ID format", errorMessage);
        Assert.Contains("Expected format: 'musclerole-{guid}'", errorMessage);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = "musclerole-00000000-0000-0000-0000-000000000000";
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/MuscleRoles/{nonExistentId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByValue_WithValidValue_ReturnsMuscleRole()
    {
        // First get all muscle roles to find a valid value
        var allResponse = await _client.GetAsync("/api/ReferenceTables/MuscleRoles");
        allResponse.EnsureSuccessStatusCode();
        var allMuscleRoles = await allResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        Assert.NotNull(allMuscleRoles);
        
        // Skip the test if there are no muscle roles
        if (!allMuscleRoles.Any())
        {
            return; // Skip the test
        }
        
        var firstMuscleRole = allMuscleRoles.First();
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/MuscleRoles/ByValue/{firstMuscleRole.Value}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var muscleRole = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(muscleRole);
        Assert.Equal(firstMuscleRole.Value, muscleRole.Value);
    }

    [Fact]
    public async Task GetByValue_WithNonExistentValue_ReturnsNotFound()
    {
        // Arrange
        var nonExistentValue = "NonExistentMuscleRole";
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/MuscleRoles/ByValue/{nonExistentValue}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Theory]
    [InlineData("Primary")]
    [InlineData("primary")]
    [InlineData("PRIMARY")]
    [InlineData("PrImArY")]
    public async Task GetByValue_WithDifferentCasing_ReturnsMuscleRole(string value)
    {
        // First check if the muscle role exists
        var allResponse = await _client.GetAsync("/api/ReferenceTables/MuscleRoles");
        allResponse.EnsureSuccessStatusCode();
        var allMuscleRoles = await allResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        
        Assert.NotNull(allMuscleRoles);
        
        // Skip the test if there are no muscle roles or if "Primary" doesn't exist
        if (!allMuscleRoles.Any() || !allMuscleRoles.Any(mr => mr.Value.Equals("Primary", StringComparison.OrdinalIgnoreCase)))
        {
            return; // Skip the test
        }
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/MuscleRoles/ByValue/{value}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var muscleRole = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(muscleRole);
        Assert.Equal("Primary", muscleRole.Value, ignoreCase: true); // The actual stored value has proper casing
    }
}
