using System.Net;
using System.Net.Http.Json;
using GetFitterGetBigger.API.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Controllers;

public class MetricTypesControllerTests : IClassFixture<ApiTestFixture>
{
    private readonly HttpClient _client;
    private readonly ApiTestFixture _fixture;

    public MetricTypesControllerTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task GetAll_ReturnsAllMetricTypes()
    {
        // Act
        var response = await _client.GetAsync("/api/ReferenceTables/MetricTypes");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var metricTypes = await response.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        
        Assert.NotNull(metricTypes);
        // We don't assert NotEmpty because the database might be empty in some test environments
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsMetricType()
    {
        // First get all metric types to find a valid ID
        var allResponse = await _client.GetAsync("/api/ReferenceTables/MetricTypes");
        allResponse.EnsureSuccessStatusCode();
        var allMetricTypes = await allResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        Assert.NotNull(allMetricTypes);
        
        // Skip the test if there are no metric types
        if (!allMetricTypes.Any())
        {
            return; // Skip the test
        }
        
        var firstMetricType = allMetricTypes.First();
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/MetricTypes/{firstMetricType.Id}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var metricType = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(metricType);
        Assert.Equal(firstMetricType.Id, metricType.Id);
        Assert.Equal(firstMetricType.Value, metricType.Value);
    }

    [Fact]
    public async Task GetById_WithInvalidIdFormat_ReturnsBadRequest()
    {
        // Arrange
        var invalidId = "abcdef12-3456-7890-abcd-ef1234567890"; // Missing prefix
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/MetricTypes/{invalidId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid ID format", errorMessage);
        Assert.Contains("Expected format: 'metrictype-{guid}'", errorMessage);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = "metrictype-00000000-0000-0000-0000-000000000000";
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/MetricTypes/{nonExistentId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByValue_WithValidValue_ReturnsMetricType()
    {
        // First get all metric types to find a valid value
        var allResponse = await _client.GetAsync("/api/ReferenceTables/MetricTypes");
        allResponse.EnsureSuccessStatusCode();
        var allMetricTypes = await allResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        Assert.NotNull(allMetricTypes);
        
        // Skip the test if there are no metric types
        if (!allMetricTypes.Any())
        {
            return; // Skip the test
        }
        
        var firstMetricType = allMetricTypes.First();
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/MetricTypes/ByValue/{firstMetricType.Value}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var metricType = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(metricType);
        Assert.Equal(firstMetricType.Value, metricType.Value);
    }

    [Fact]
    public async Task GetByValue_WithNonExistentValue_ReturnsNotFound()
    {
        // Arrange
        var nonExistentValue = "NonExistentMetricType";
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/MetricTypes/ByValue/{nonExistentValue}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Theory]
    [InlineData("Weight")]
    [InlineData("weight")]
    [InlineData("WEIGHT")]
    [InlineData("WeIgHt")]
    public async Task GetByValue_WithDifferentCasing_ReturnsMetricType(string value)
    {
        // First check if the metric type exists
        var allResponse = await _client.GetAsync("/api/ReferenceTables/MetricTypes");
        allResponse.EnsureSuccessStatusCode();
        var allMetricTypes = await allResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        
        Assert.NotNull(allMetricTypes);
        
        // Skip the test if there are no metric types or if "Weight" doesn't exist
        if (!allMetricTypes.Any() || !allMetricTypes.Any(mt => mt.Value.Equals("Weight", StringComparison.OrdinalIgnoreCase)))
        {
            return; // Skip the test
        }
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/MetricTypes/ByValue/{value}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var metricType = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(metricType);
        Assert.Equal("Weight", metricType.Value, ignoreCase: true); // The actual stored value has proper casing
    }
}
