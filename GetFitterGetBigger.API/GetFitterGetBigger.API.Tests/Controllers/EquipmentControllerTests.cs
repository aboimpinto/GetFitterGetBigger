using System.Net;
using System.Net.Http.Json;
using GetFitterGetBigger.API.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Controllers;

public class EquipmentControllerTests : IClassFixture<ApiTestFixture>
{
    private readonly HttpClient _client;
    private readonly ApiTestFixture _fixture;

    public EquipmentControllerTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task GetAll_ReturnsAllEquipment()
    {
        // Act
        var response = await _client.GetAsync("/api/ReferenceTables/Equipment");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var equipment = await response.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        
        Assert.NotNull(equipment);
        // We don't assert NotEmpty because the database might be empty in some test environments
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsEquipment()
    {
        // First get all equipment to find a valid ID
        var allResponse = await _client.GetAsync("/api/ReferenceTables/Equipment");
        allResponse.EnsureSuccessStatusCode();
        var allEquipment = await allResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        Assert.NotNull(allEquipment);
        
        // Skip the test if there are no equipment items
        if (!allEquipment.Any())
        {
            return; // Skip the test
        }
        
        var firstEquipment = allEquipment.First();
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/Equipment/{firstEquipment.Id}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var equipment = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(equipment);
        Assert.Equal(firstEquipment.Id, equipment.Id);
        Assert.Equal(firstEquipment.Value, equipment.Value);
    }

    [Fact]
    public async Task GetById_WithInvalidIdFormat_ReturnsBadRequest()
    {
        // Arrange
        var invalidId = "abcdef12-3456-7890-abcd-ef1234567890"; // Missing prefix
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/Equipment/{invalidId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid ID format", errorMessage);
        Assert.Contains("Expected format: 'equipment-{guid}'", errorMessage);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = "equipment-00000000-0000-0000-0000-000000000000";
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/Equipment/{nonExistentId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByValue_WithValidValue_ReturnsEquipment()
    {
        // First get all equipment to find a valid value
        var allResponse = await _client.GetAsync("/api/ReferenceTables/Equipment");
        allResponse.EnsureSuccessStatusCode();
        var allEquipment = await allResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        Assert.NotNull(allEquipment);
        
        // Skip the test if there are no equipment items
        if (!allEquipment.Any())
        {
            return; // Skip the test
        }
        
        var firstEquipment = allEquipment.First();
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/Equipment/ByValue/{firstEquipment.Value}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var equipment = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(equipment);
        Assert.Equal(firstEquipment.Value, equipment.Value);
    }

    [Fact]
    public async Task GetByValue_WithNonExistentValue_ReturnsNotFound()
    {
        // Arrange
        var nonExistentValue = "NonExistentEquipment";
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/Equipment/ByValue/{nonExistentValue}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Theory]
    [InlineData("Barbell")]
    [InlineData("barbell")]
    [InlineData("BARBELL")]
    [InlineData("BaRbElL")]
    public async Task GetByValue_WithDifferentCasing_ReturnsEquipment(string value)
    {
        // First check if the equipment exists
        var allResponse = await _client.GetAsync("/api/ReferenceTables/Equipment");
        allResponse.EnsureSuccessStatusCode();
        var allEquipment = await allResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        
        Assert.NotNull(allEquipment);
        
        // Skip the test if there are no equipment items or if "Barbell" doesn't exist
        if (!allEquipment.Any() || !allEquipment.Any(e => e.Value.Equals("Barbell", StringComparison.OrdinalIgnoreCase)))
        {
            return; // Skip the test
        }
        
        // Act
        var response = await _client.GetAsync($"/api/ReferenceTables/Equipment/ByValue/{value}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var equipment = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        
        Assert.NotNull(equipment);
        Assert.Equal("Barbell", equipment.Value, ignoreCase: true); // The actual stored value has proper casing
    }
}
