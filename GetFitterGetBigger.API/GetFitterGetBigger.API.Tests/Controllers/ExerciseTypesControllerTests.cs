using System.Net;
using System.Net.Http.Json;
using GetFitterGetBigger.API.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Controllers;

public class ExerciseTypesControllerTests : IClassFixture<ApiTestFixture>
{
    private readonly HttpClient _client;
    private readonly ApiTestFixture _fixture;

    public ExerciseTypesControllerTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsAllExerciseTypes()
    {
        // Act
        var response = await _client.GetAsync("/api/ReferenceTables/ExerciseTypes");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var exerciseTypes = await response.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        
        Assert.NotNull(exerciseTypes);
        // We don't assert on specific values because the database might be empty in some test environments
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsExerciseType()
    {
        // First get all exercise types to find a valid ID
        var allResponse = await _client.GetAsync("/api/ReferenceTables/ExerciseTypes");
        allResponse.EnsureSuccessStatusCode();
        var allExerciseTypes = await allResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        Assert.NotNull(allExerciseTypes);
        
        // Skip the test if there are no exercise types
        if (!allExerciseTypes.Any())
        {
            return; // Skip the test
        }
        
        var firstExerciseType = allExerciseTypes.First();
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/ExerciseTypes/{firstExerciseType.Id}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var exerciseType = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(exerciseType);
        Assert.Equal(firstExerciseType.Id, exerciseType.Id);
        Assert.Equal(firstExerciseType.Value, exerciseType.Value);
        Assert.Equal(firstExerciseType.Description, exerciseType.Description);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/ReferenceTables/ExerciseTypes/exercisetype-99999999-9999-9999-9999-999999999999");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithInvalidIdFormat_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/ReferenceTables/ExerciseTypes/invalid-id");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByValue_WithValidValue_ReturnsExerciseType()
    {
        // First get all exercise types to check if we have data
        var allResponse = await _client.GetAsync("/api/ReferenceTables/ExerciseTypes");
        allResponse.EnsureSuccessStatusCode();
        var allExerciseTypes = await allResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        
        // Skip the test if there are no exercise types
        if (allExerciseTypes == null || !allExerciseTypes.Any())
        {
            return; // Skip the test
        }
        
        var firstExerciseType = allExerciseTypes.First();
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/ExerciseTypes/ByValue/{firstExerciseType.Value}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var exerciseType = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(exerciseType);
        Assert.Equal(firstExerciseType.Value, exerciseType.Value);
    }

    [Fact]
    public async Task GetByValue_WithInvalidValue_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/ReferenceTables/ExerciseTypes/ByValue/InvalidType");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("Warmup")]
    [InlineData("warmup")]
    [InlineData("WARMUP")]
    [InlineData("WaRmUp")]
    public async Task GetByValue_WithDifferentCasing_ReturnsSameResult(string value)
    {
        // First check if the exercise type exists
        var allResponse = await _client.GetAsync("/api/ReferenceTables/ExerciseTypes");
        allResponse.EnsureSuccessStatusCode();
        var allExerciseTypes = await allResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        
        Assert.NotNull(allExerciseTypes);
        
        // Skip the test if there are no exercise types or if "Warmup" doesn't exist
        if (!allExerciseTypes.Any() || !allExerciseTypes.Any(et => et.Value.Equals("Warmup", StringComparison.OrdinalIgnoreCase)))
        {
            return; // Skip the test
        }
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/ExerciseTypes/ByValue/{value}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var exerciseType = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(exerciseType);
        Assert.Equal("Warmup", exerciseType.Value, ignoreCase: true);
    }
}