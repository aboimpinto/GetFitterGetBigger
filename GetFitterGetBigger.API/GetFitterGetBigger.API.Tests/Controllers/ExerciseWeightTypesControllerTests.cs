using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Controllers;

public class ExerciseWeightTypesControllerTests : IClassFixture<ApiTestFixture>
{
    private readonly HttpClient _client;
    private readonly ApiTestFixture _fixture;

    public ExerciseWeightTypesControllerTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsAllExerciseWeightTypes()
    {
        // Act
        var response = await _client.GetAsync("/api/ReferenceTables/ExerciseWeightTypes");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var weightTypes = await response.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        
        Assert.NotNull(weightTypes);
        Assert.NotEmpty(weightTypes); // We should have seeded data
        Assert.Equal(5, weightTypes.Count); // We seed 5 active weight types
        
        // Verify the expected weight types are present
        Assert.Contains(weightTypes, wt => wt.Value == "Bodyweight Only");
        Assert.Contains(weightTypes, wt => wt.Value == "Bodyweight Optional");
        Assert.Contains(weightTypes, wt => wt.Value == "Weight Required");
        Assert.Contains(weightTypes, wt => wt.Value == "Machine Weight");
        Assert.Contains(weightTypes, wt => wt.Value == "No Weight");
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsExerciseWeightType()
    {
        // First get all weight types to find a valid ID
        var allResponse = await _client.GetAsync("/api/ReferenceTables/ExerciseWeightTypes");
        allResponse.EnsureSuccessStatusCode();
        var allWeightTypes = await allResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        Assert.NotNull(allWeightTypes);
        Assert.NotEmpty(allWeightTypes);
        
        var firstWeightType = allWeightTypes.First();
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/ExerciseWeightTypes/{firstWeightType.Id}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var weightType = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(weightType);
        Assert.Equal(firstWeightType.Id, weightType.Id);
        Assert.Equal(firstWeightType.Value, weightType.Value);
        Assert.Equal(firstWeightType.Description, weightType.Description);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/ReferenceTables/ExerciseWeightTypes/exerciseweighttype-99999999-9999-9999-9999-999999999999");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithInvalidIdFormat_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/ReferenceTables/ExerciseWeightTypes/invalid-id");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByValue_WithValidValue_ReturnsExerciseWeightType()
    {
        // Act
        var response = await _client.GetAsync("/api/ReferenceTables/ExerciseWeightTypes/ByValue/Bodyweight Only");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var weightType = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(weightType);
        Assert.Equal("Bodyweight Only", weightType.Value);
        Assert.Contains("cannot have external weight added", weightType.Description);
    }

    [Fact]
    public async Task GetByValue_CaseInsensitive_ReturnsExerciseWeightType()
    {
        // Act
        var response = await _client.GetAsync("/api/ReferenceTables/ExerciseWeightTypes/ByValue/WEIGHT REQUIRED");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var weightType = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(weightType);
        Assert.Equal("Weight Required", weightType.Value); // Original casing preserved
    }

    [Fact]
    public async Task GetByValue_WithInvalidValue_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/ReferenceTables/ExerciseWeightTypes/ByValue/NonExistentWeightType");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByValue_WithUrlEncodedValue_ReturnsExerciseWeightType()
    {
        // Act - URL encoded space becomes %20
        var response = await _client.GetAsync("/api/ReferenceTables/ExerciseWeightTypes/ByValue/Machine%20Weight");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var weightType = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(weightType);
        Assert.Equal("Machine Weight", weightType.Value);
    }

    [Fact]
    public async Task GetByCode_WithValidCode_ReturnsExerciseWeightType()
    {
        // Act
        var response = await _client.GetAsync("/api/ReferenceTables/ExerciseWeightTypes/ByCode/BODYWEIGHT_ONLY");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var weightType = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(weightType);
        Assert.Equal("Bodyweight Only", weightType.Value);
        Assert.Contains("cannot have external weight added", weightType.Description);
    }

    [Fact]
    public async Task GetByCode_WithAllValidCodes_ReturnsCorrectWeightTypes()
    {
        // Test all valid codes
        var testCases = new[]
        {
            ("BODYWEIGHT_ONLY", "Bodyweight Only"),
            ("BODYWEIGHT_OPTIONAL", "Bodyweight Optional"),
            ("WEIGHT_REQUIRED", "Weight Required"),
            ("MACHINE_WEIGHT", "Machine Weight"),
            ("NO_WEIGHT", "No Weight")
        };

        foreach (var (code, expectedValue) in testCases)
        {
            // Act
            var response = await _client.GetAsync($"/api/ReferenceTables/ExerciseWeightTypes/ByCode/{code}");
            
            // Assert
            response.EnsureSuccessStatusCode();
            var weightType = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
            
            Assert.NotNull(weightType);
            Assert.Equal(expectedValue, weightType.Value);
        }
    }

    [Fact]
    public async Task GetByCode_CaseSensitive_ReturnsNotFound()
    {
        // Act - lowercase should not work
        var response = await _client.GetAsync("/api/ReferenceTables/ExerciseWeightTypes/ByCode/bodyweight_only");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByCode_WithInvalidCode_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/ReferenceTables/ExerciseWeightTypes/ByCode/INVALID_CODE");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ResponseHasCorrectStructure()
    {
        // Act
        var response = await _client.GetAsync("/api/ReferenceTables/ExerciseWeightTypes");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var weightTypes = await response.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        
        Assert.NotNull(weightTypes);
        var firstWeightType = weightTypes.First();
        
        // Verify DTO structure
        Assert.NotNull(firstWeightType.Id);
        Assert.NotEmpty(firstWeightType.Id);
        Assert.StartsWith("exerciseweighttype-", firstWeightType.Id);
        
        Assert.NotNull(firstWeightType.Value);
        Assert.NotEmpty(firstWeightType.Value);
        
        // Description can be null, but if present should not be empty
        if (firstWeightType.Description != null)
        {
            Assert.NotEmpty(firstWeightType.Description);
        }
    }
}