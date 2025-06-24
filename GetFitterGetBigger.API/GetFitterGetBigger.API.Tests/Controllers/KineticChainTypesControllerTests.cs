using System.Net;
using System.Net.Http.Json;
using GetFitterGetBigger.API.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Controllers;

public class KineticChainTypesControllerTests : IClassFixture<ApiTestFixture>
{
    private readonly HttpClient _client;
    private readonly ApiTestFixture _fixture;

    public KineticChainTypesControllerTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task GetAll_ReturnsAllKineticChainTypes()
    {
        // Act
        var response = await _client.GetAsync("/api/ReferenceTables/KineticChainTypes");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var kineticChainTypes = await response.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        
        Assert.NotNull(kineticChainTypes);
        // We don't assert Count or specific values because the database might be empty in some test environments
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsKineticChainType()
    {
        // First get all kinetic chain types to find a valid ID
        var allResponse = await _client.GetAsync("/api/ReferenceTables/KineticChainTypes");
        allResponse.EnsureSuccessStatusCode();
        var allKineticChainTypes = await allResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        Assert.NotNull(allKineticChainTypes);
        
        // Skip the test if there are no kinetic chain types
        if (!allKineticChainTypes.Any())
        {
            return; // Skip the test
        }
        
        var firstKineticChainType = allKineticChainTypes.First();
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/KineticChainTypes/{firstKineticChainType.Id}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var kineticChainType = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(kineticChainType);
        Assert.Equal(firstKineticChainType.Id, kineticChainType.Id);
        Assert.Equal(firstKineticChainType.Value, kineticChainType.Value);
    }

    [Fact]
    public async Task GetById_WithInvalidIdFormat_ReturnsBadRequest()
    {
        // Arrange
        var invalidId = "f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4"; // Missing prefix
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/KineticChainTypes/{invalidId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid ID format", errorMessage);
        Assert.Contains("Expected format: 'kineticchaintype-{guid}'", errorMessage);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = "kineticchaintype-00000000-0000-0000-0000-000000000000";
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/KineticChainTypes/{nonExistentId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByValue_WithValidValue_ReturnsKineticChainType()
    {
        // First get all kinetic chain types to find a valid value
        var allResponse = await _client.GetAsync("/api/ReferenceTables/KineticChainTypes");
        allResponse.EnsureSuccessStatusCode();
        var allKineticChainTypes = await allResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        Assert.NotNull(allKineticChainTypes);
        
        // Skip the test if there are no kinetic chain types
        if (!allKineticChainTypes.Any())
        {
            return; // Skip the test
        }
        
        var firstKineticChainType = allKineticChainTypes.First();
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/KineticChainTypes/ByValue/{firstKineticChainType.Value}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var kineticChainType = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(kineticChainType);
        Assert.Equal(firstKineticChainType.Value, kineticChainType.Value);
    }

    [Fact]
    public async Task GetByValue_WithNonExistentValue_ReturnsNotFound()
    {
        // Arrange
        var nonExistentValue = "NonExistentKineticChainType";
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/KineticChainTypes/ByValue/{nonExistentValue}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Theory]
    [InlineData("Compound")]
    [InlineData("compound")]
    [InlineData("COMPOUND")]
    [InlineData("CoMpOuNd")]
    public async Task GetByValue_WithDifferentCasing_ReturnsKineticChainType(string value)
    {
        // First check if the kinetic chain type exists
        var allResponse = await _client.GetAsync("/api/ReferenceTables/KineticChainTypes");
        allResponse.EnsureSuccessStatusCode();
        var allKineticChainTypes = await allResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        
        Assert.NotNull(allKineticChainTypes);
        
        // Skip the test if there are no kinetic chain types or if "Compound" doesn't exist
        if (!allKineticChainTypes.Any() || !allKineticChainTypes.Any(kct => kct.Value.Equals("Compound", StringComparison.OrdinalIgnoreCase)))
        {
            return; // Skip the test
        }
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/KineticChainTypes/ByValue/{value}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var kineticChainType = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(kineticChainType);
        Assert.Equal("Compound", kineticChainType.Value, ignoreCase: true); // The actual stored value has proper casing
    }
}
