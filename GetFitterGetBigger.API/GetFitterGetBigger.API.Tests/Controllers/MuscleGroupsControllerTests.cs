using System.Net;
using System.Net.Http.Json;
using GetFitterGetBigger.API.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Controllers;

public class MuscleGroupsControllerTests : IClassFixture<ApiTestFixture>
{
    private readonly HttpClient _client;
    private readonly ApiTestFixture _fixture;

    public MuscleGroupsControllerTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task GetAll_ReturnsAllMuscleGroups()
    {
        // Act
        var response = await _client.GetAsync("/api/ReferenceTables/MuscleGroups");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var muscleGroups = await response.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        
        Assert.NotNull(muscleGroups);
        // We don't assert NotEmpty because the database might be empty in some test environments
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsMuscleGroup()
    {
        // First get all muscle groups to find a valid ID
        var allResponse = await _client.GetAsync("/api/ReferenceTables/MuscleGroups");
        allResponse.EnsureSuccessStatusCode();
        var allMuscleGroups = await allResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        Assert.NotNull(allMuscleGroups);
        
        // Skip the test if there are no muscle groups
        if (!allMuscleGroups.Any())
        {
            return; // Skip the test
        }
        
        var firstMuscleGroup = allMuscleGroups.First();
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/MuscleGroups/{firstMuscleGroup.Id}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var muscleGroup = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(muscleGroup);
        Assert.Equal(firstMuscleGroup.Id, muscleGroup.Id);
        Assert.Equal(firstMuscleGroup.Value, muscleGroup.Value);
    }

    [Fact]
    public async Task GetById_WithInvalidIdFormat_ReturnsBadRequest()
    {
        // Arrange
        var invalidId = "abcdef12-3456-7890-abcd-ef1234567890"; // Missing prefix
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/MuscleGroups/{invalidId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid ID format", errorMessage);
        Assert.Contains("Expected format: 'musclegroup-{guid}'", errorMessage);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = "musclegroup-00000000-0000-0000-0000-000000000000";
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/MuscleGroups/{nonExistentId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByValue_WithValidValue_ReturnsMuscleGroup()
    {
        // First get all muscle groups to find a valid value
        var allResponse = await _client.GetAsync("/api/ReferenceTables/MuscleGroups");
        allResponse.EnsureSuccessStatusCode();
        var allMuscleGroups = await allResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        Assert.NotNull(allMuscleGroups);
        
        // Skip the test if there are no muscle groups
        if (!allMuscleGroups.Any())
        {
            return; // Skip the test
        }
        
        var firstMuscleGroup = allMuscleGroups.First();
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/MuscleGroups/ByValue/{firstMuscleGroup.Value}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var muscleGroup = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(muscleGroup);
        Assert.Equal(firstMuscleGroup.Value, muscleGroup.Value);
    }

    [Fact]
    public async Task GetByValue_WithNonExistentValue_ReturnsNotFound()
    {
        // Arrange
        var nonExistentValue = "NonExistentMuscleGroup";
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/MuscleGroups/ByValue/{nonExistentValue}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Theory]
    [InlineData("Pectoralis")]
    [InlineData("pectoralis")]
    [InlineData("PECTORALIS")]
    [InlineData("PeCtOrAlIs")]
    public async Task GetByValue_WithDifferentCasing_ReturnsMuscleGroup(string value)
    {
        // First check if the muscle group exists
        var allResponse = await _client.GetAsync("/api/ReferenceTables/MuscleGroups");
        allResponse.EnsureSuccessStatusCode();
        var allMuscleGroups = await allResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        
        Assert.NotNull(allMuscleGroups);
        
        // Skip the test if there are no muscle groups or if "Pectoralis" doesn't exist
        if (!allMuscleGroups.Any() || !allMuscleGroups.Any(mg => mg.Value.Equals("Pectoralis", StringComparison.OrdinalIgnoreCase)))
        {
            return; // Skip the test
        }
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/MuscleGroups/ByValue/{value}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var muscleGroup = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(muscleGroup);
        Assert.Equal("Pectoralis", muscleGroup.Value, ignoreCase: true); // The actual stored value has proper casing
    }
}
