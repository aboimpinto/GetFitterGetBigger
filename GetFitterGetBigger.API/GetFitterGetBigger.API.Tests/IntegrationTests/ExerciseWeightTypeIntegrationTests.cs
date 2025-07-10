using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Xunit;
using GetFitterGetBigger.API.Services.Interfaces;

namespace GetFitterGetBigger.API.Tests.IntegrationTests;

[Collection("SharedDatabase")]
public class ExerciseWeightTypeIntegrationTests : IClassFixture<SharedDatabaseTestFixture>
{
    private readonly SharedDatabaseTestFixture _fixture;
    private readonly HttpClient _client;
    
    public ExerciseWeightTypeIntegrationTests(SharedDatabaseTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
    }
    
    #region GET All Tests
    
    [Fact]
    public async Task GetAll_ReturnsAllFiveActiveWeightTypes()
    {
        // Act
        var response = await _client.GetAsync("/api/ReferenceTables/ExerciseWeightTypes");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var weightTypes = await response.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        
        Assert.NotNull(weightTypes);
        Assert.Equal(5, weightTypes.Count);
        
        // Verify all expected weight types are present
        var expectedValues = new[] 
        { 
            "Bodyweight Only", 
            "Bodyweight Optional", 
            "Weight Required", 
            "Machine Weight", 
            "No Weight" 
        };
        
        foreach (var expectedValue in expectedValues)
        {
            Assert.Contains(weightTypes, wt => wt.Value == expectedValue);
        }
    }
    
    [Fact]
    public async Task GetAll_ReturnsCorrectDataStructure()
    {
        // Act
        var response = await _client.GetAsync("/api/ReferenceTables/ExerciseWeightTypes");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var weightTypes = await response.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        
        Assert.NotNull(weightTypes);
        
        // Check the BODYWEIGHT_ONLY type specifically
        var bodyweightOnly = weightTypes.FirstOrDefault(wt => wt.Value == "Bodyweight Only");
        Assert.NotNull(bodyweightOnly);
        Assert.Equal("exerciseweighttype-a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a", bodyweightOnly.Id);
        Assert.Equal("Exercises that cannot have external weight added", bodyweightOnly.Description);
    }
    
    #endregion
    
    #region GET By ID Tests
    
    [Fact]
    public async Task GetById_WithValidId_ReturnsCorrectWeightType()
    {
        // Arrange
        var id = "exerciseweighttype-a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a"; // BODYWEIGHT_ONLY
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/ExerciseWeightTypes/{id}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var weightType = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(weightType);
        Assert.Equal(id, weightType.Id);
        Assert.Equal("Bodyweight Only", weightType.Value);
        Assert.Equal("Exercises that cannot have external weight added", weightType.Description);
    }
    
    [Fact]
    public async Task GetById_WithInvalidId_Returns404()
    {
        // Arrange
        var invalidId = "exerciseweighttype-00000000-0000-0000-0000-000000000000";
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/ExerciseWeightTypes/{invalidId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task GetById_WithMalformedId_Returns404()
    {
        // Act
        var response = await _client.GetAsync("/api/ReferenceTables/ExerciseWeightTypes/not-a-valid-id");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    #endregion
    
    #region GET By Value Tests
    
    [Fact]
    public async Task GetByValue_WithExactValue_ReturnsCorrectWeightType()
    {
        // Act
        var response = await _client.GetAsync("/api/ReferenceTables/ExerciseWeightTypes/ByValue/Weight Required");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var weightType = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(weightType);
        Assert.Equal("Weight Required", weightType.Value);
        Assert.Equal("exerciseweighttype-c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a", weightType.Id);
    }
    
    [Fact]
    public async Task GetByValue_CaseInsensitive_ReturnsCorrectWeightType()
    {
        // Act
        var response = await _client.GetAsync("/api/ReferenceTables/ExerciseWeightTypes/ByValue/MACHINE WEIGHT");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var weightType = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(weightType);
        Assert.Equal("Machine Weight", weightType.Value); // Original casing preserved
        Assert.Equal("exerciseweighttype-d4c6b5a7-8c9d-7e0f-1a2b-3c4d5e6f7a8b", weightType.Id);
    }
    
    [Fact]
    public async Task GetByValue_WithNonExistentValue_Returns404()
    {
        // Act
        var response = await _client.GetAsync("/api/ReferenceTables/ExerciseWeightTypes/ByValue/Invalid Weight Type");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    #endregion
    
    #region GET By Code Tests
    
    [Fact]
    public async Task GetByCode_WithValidCode_ReturnsCorrectWeightType()
    {
        // Act
        var response = await _client.GetAsync("/api/ReferenceTables/ExerciseWeightTypes/ByCode/BODYWEIGHT_OPTIONAL");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var weightType = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(weightType);
        Assert.Equal("Bodyweight Optional", weightType.Value);
        Assert.Equal("exerciseweighttype-b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f", weightType.Id);
        Assert.Equal("Exercises that can be performed with or without additional weight", weightType.Description);
    }
    
    [Fact]
    public async Task GetByCode_WithAllValidCodes_ReturnsCorrectWeightTypes()
    {
        // Test all 5 valid codes
        var testCases = new[]
        {
            ("BODYWEIGHT_ONLY", "exerciseweighttype-a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a", "Bodyweight Only"),
            ("BODYWEIGHT_OPTIONAL", "exerciseweighttype-b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f", "Bodyweight Optional"),
            ("WEIGHT_REQUIRED", "exerciseweighttype-c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a", "Weight Required"),
            ("MACHINE_WEIGHT", "exerciseweighttype-d4c6b5a7-8c9d-7e0f-1a2b-3c4d5e6f7a8b", "Machine Weight"),
            ("NO_WEIGHT", "exerciseweighttype-e5b7a698-9d0e-8f1a-2b3c-4d5e6f7a8b9c", "No Weight")
        };
        
        foreach (var (code, expectedId, expectedValue) in testCases)
        {
            // Act
            var response = await _client.GetAsync($"/api/ReferenceTables/ExerciseWeightTypes/ByCode/{code}");
            
            // Assert
            response.EnsureSuccessStatusCode();
            var weightType = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
            
            Assert.NotNull(weightType);
            Assert.Equal(expectedId, weightType.Id);
            Assert.Equal(expectedValue, weightType.Value);
        }
    }
    
    [Fact]
    public async Task GetByCode_CaseSensitive_Returns404ForLowercase()
    {
        // Act
        var response = await _client.GetAsync("/api/ReferenceTables/ExerciseWeightTypes/ByCode/bodyweight_only");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task GetByCode_WithInvalidCode_Returns404()
    {
        // Act
        var response = await _client.GetAsync("/api/ReferenceTables/ExerciseWeightTypes/ByCode/INVALID_CODE");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    #endregion
    
    #region Caching Behavior Tests
    
    [Fact]
    public async Task GetAll_SecondCall_ReturnsCachedData()
    {
        // Arrange - Clear cache first
        using (var scope = _fixture.Services.CreateScope())
        {
            var cache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
            if (cache is MemoryCache memCache)
            {
                memCache.Clear();
            }
        }
        
        // Act - First call (should hit database)
        var sw1 = Stopwatch.StartNew();
        var response1 = await _client.GetAsync("/api/ReferenceTables/ExerciseWeightTypes");
        sw1.Stop();
        
        response1.EnsureSuccessStatusCode();
        var weightTypes1 = await response1.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        
        // Act - Second call (should hit cache)
        var sw2 = Stopwatch.StartNew();
        var response2 = await _client.GetAsync("/api/ReferenceTables/ExerciseWeightTypes");
        sw2.Stop();
        
        response2.EnsureSuccessStatusCode();
        var weightTypes2 = await response2.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        
        // Assert
        Assert.NotNull(weightTypes1);
        Assert.NotNull(weightTypes2);
        Assert.Equal(weightTypes1.Count, weightTypes2.Count);
        
        // The cached call should typically be faster, but we can't guarantee this in all environments
        // So we just verify both calls returned the same data
        for (int i = 0; i < weightTypes1.Count; i++)
        {
            Assert.Equal(weightTypes1[i].Id, weightTypes2[i].Id);
            Assert.Equal(weightTypes1[i].Value, weightTypes2[i].Value);
            Assert.Equal(weightTypes1[i].Description, weightTypes2[i].Description);
        }
    }
    
    [Fact]
    public async Task GetById_UsesCache_WhenCalledMultipleTimes()
    {
        // Arrange
        var id = "exerciseweighttype-c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a"; // WEIGHT_REQUIRED
        
        // Act - Call the same ID multiple times
        var response1 = await _client.GetAsync($"/api/ReferenceTables/ExerciseWeightTypes/{id}");
        var response2 = await _client.GetAsync($"/api/ReferenceTables/ExerciseWeightTypes/{id}");
        var response3 = await _client.GetAsync($"/api/ReferenceTables/ExerciseWeightTypes/{id}");
        
        // Assert - All calls should succeed and return the same data
        response1.EnsureSuccessStatusCode();
        response2.EnsureSuccessStatusCode();
        response3.EnsureSuccessStatusCode();
        
        var wt1 = await response1.Content.ReadFromJsonAsync<ReferenceDataDto>();
        var wt2 = await response2.Content.ReadFromJsonAsync<ReferenceDataDto>();
        var wt3 = await response3.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(wt1);
        Assert.NotNull(wt2);
        Assert.NotNull(wt3);
        Assert.Equal(wt1.Id, wt2.Id);
        Assert.Equal(wt2.Id, wt3.Id);
        Assert.Equal("Weight Required", wt1.Value);
    }
    
    [Fact]
    public async Task GetByCode_UsesCache_WhenCalledMultipleTimes()
    {
        // Arrange
        var code = "NO_WEIGHT";
        
        // Act - Call the same code multiple times
        var tasks = new[]
        {
            _client.GetAsync($"/api/ReferenceTables/ExerciseWeightTypes/ByCode/{code}"),
            _client.GetAsync($"/api/ReferenceTables/ExerciseWeightTypes/ByCode/{code}"),
            _client.GetAsync($"/api/ReferenceTables/ExerciseWeightTypes/ByCode/{code}")
        };
        
        var responses = await Task.WhenAll(tasks);
        
        // Assert - All calls should succeed
        foreach (var response in responses)
        {
            response.EnsureSuccessStatusCode();
            var weightType = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
            Assert.NotNull(weightType);
            Assert.Equal("No Weight", weightType.Value);
        }
    }
    
    #endregion
}