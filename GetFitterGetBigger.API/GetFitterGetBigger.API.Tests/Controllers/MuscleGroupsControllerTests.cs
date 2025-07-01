using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
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
        _client = fixture.CreateClient();
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
    
    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedMuscleGroup()
    {
        // Arrange - first get a valid body part ID
        var bodyPartsResponse = await _client.GetAsync("/api/ReferenceTables/BodyParts");
        bodyPartsResponse.EnsureSuccessStatusCode();
        var bodyParts = await bodyPartsResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        
        if (bodyParts == null || !bodyParts.Any())
        {
            return; // Skip test if no body parts available
        }
        
        var bodyPartId = bodyParts.First().Id;
        var uniqueName = $"TestMuscleGroup_{Guid.NewGuid()}";
        var createDto = new CreateMuscleGroupDto
        {
            Name = uniqueName,
            BodyPartId = bodyPartId
        };
        
        var content = new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json");
        
        // Act
        var response = await _client.PostAsync("/api/ReferenceTables/MuscleGroups", content);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdDto = await response.Content.ReadFromJsonAsync<MuscleGroupDto>();
        
        Assert.NotNull(createdDto);
        Assert.Equal(uniqueName, createdDto.Name);
        Assert.Equal(bodyPartId, createdDto.BodyPartId);
        Assert.True(createdDto.IsActive);
        Assert.True(createdDto.CreatedAt > DateTime.MinValue);
        Assert.Null(createdDto.UpdatedAt);
        
        // Clean up - deactivate the created muscle group
        await _client.DeleteAsync($"/api/ReferenceTables/MuscleGroups/{createdDto.Id}");
    }
    
    [Fact]
    public async Task Create_WithDuplicateName_ReturnsConflict()
    {
        // Arrange - first create a muscle group
        var bodyPartsResponse = await _client.GetAsync("/api/ReferenceTables/BodyParts");
        bodyPartsResponse.EnsureSuccessStatusCode();
        var bodyParts = await bodyPartsResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        
        if (bodyParts == null || !bodyParts.Any())
        {
            return; // Skip test if no body parts available
        }
        
        var bodyPartId = bodyParts.First().Id;
        var uniqueName = $"TestMuscleGroup_{Guid.NewGuid()}";
        var createDto = new CreateMuscleGroupDto
        {
            Name = uniqueName,
            BodyPartId = bodyPartId
        };
        
        var content = new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json");
        var firstResponse = await _client.PostAsync("/api/ReferenceTables/MuscleGroups", content);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        
        var createdDto = await firstResponse.Content.ReadFromJsonAsync<MuscleGroupDto>();
        
        // Act - try to create with same name
        var duplicateResponse = await _client.PostAsync("/api/ReferenceTables/MuscleGroups", content);
        
        // Assert
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
        
        // Clean up
        if (createdDto != null)
        {
            await _client.DeleteAsync($"/api/ReferenceTables/MuscleGroups/{createdDto.Id}");
        }
    }
    
    [Fact]
    public async Task Create_WithInvalidBodyPartId_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateMuscleGroupDto
        {
            Name = "TestMuscleGroup",
            BodyPartId = "invalid-format"
        };
        
        var content = new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json");
        
        // Act
        var response = await _client.PostAsync("/api/ReferenceTables/MuscleGroups", content);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task Update_WithValidData_ReturnsUpdatedMuscleGroup()
    {
        // Arrange - first create a muscle group to update
        var bodyPartsResponse = await _client.GetAsync("/api/ReferenceTables/BodyParts");
        bodyPartsResponse.EnsureSuccessStatusCode();
        var bodyParts = await bodyPartsResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        
        if (bodyParts == null || !bodyParts.Any())
        {
            return; // Skip test if no body parts available
        }
        
        var bodyPartId = bodyParts.First().Id;
        var originalName = $"TestMuscleGroup_{Guid.NewGuid()}";
        var createDto = new CreateMuscleGroupDto
        {
            Name = originalName,
            BodyPartId = bodyPartId
        };
        
        var createContent = new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/ReferenceTables/MuscleGroups", createContent);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        
        var createdDto = await createResponse.Content.ReadFromJsonAsync<MuscleGroupDto>();
        Assert.NotNull(createdDto);
        
        // Update with new name
        var updatedName = $"UpdatedMuscleGroup_{Guid.NewGuid()}";
        var updateDto = new UpdateMuscleGroupDto
        {
            Name = updatedName,
            BodyPartId = bodyPartId
        };
        
        var updateContent = new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json");
        
        // Act
        var updateResponse = await _client.PutAsync($"/api/ReferenceTables/MuscleGroups/{createdDto.Id}", updateContent);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updatedDto = await updateResponse.Content.ReadFromJsonAsync<MuscleGroupDto>();
        
        Assert.NotNull(updatedDto);
        Assert.Equal(updatedName, updatedDto.Name);
        Assert.NotNull(updatedDto.UpdatedAt);
        
        // Clean up
        await _client.DeleteAsync($"/api/ReferenceTables/MuscleGroups/{createdDto.Id}");
    }
    
    [Fact]
    public async Task Update_NonExistentMuscleGroup_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = $"musclegroup-{Guid.NewGuid()}";
        var updateDto = new UpdateMuscleGroupDto
        {
            Name = "UpdatedName",
            BodyPartId = "bodypart-12345678-1234-1234-1234-123456789012"
        };
        
        var content = new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json");
        
        // Act
        var response = await _client.PutAsync($"/api/ReferenceTables/MuscleGroups/{nonExistentId}", content);
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task Delete_ExistingMuscleGroup_ReturnsNoContent()
    {
        // Arrange - first create a muscle group to delete
        var bodyPartsResponse = await _client.GetAsync("/api/ReferenceTables/BodyParts");
        bodyPartsResponse.EnsureSuccessStatusCode();
        var bodyParts = await bodyPartsResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        
        if (bodyParts == null || !bodyParts.Any())
        {
            return; // Skip test if no body parts available
        }
        
        var bodyPartId = bodyParts.First().Id;
        var createDto = new CreateMuscleGroupDto
        {
            Name = $"TestMuscleGroup_{Guid.NewGuid()}",
            BodyPartId = bodyPartId
        };
        
        var createContent = new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/ReferenceTables/MuscleGroups", createContent);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        
        var createdDto = await createResponse.Content.ReadFromJsonAsync<MuscleGroupDto>();
        Assert.NotNull(createdDto);
        
        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/ReferenceTables/MuscleGroups/{createdDto.Id}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        
        // Verify it's no longer retrievable
        var getResponse = await _client.GetAsync($"/api/ReferenceTables/MuscleGroups/{createdDto.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
    
    [Fact]
    public async Task Delete_NonExistentMuscleGroup_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = $"musclegroup-{Guid.NewGuid()}";
        
        // Act
        var response = await _client.DeleteAsync($"/api/ReferenceTables/MuscleGroups/{nonExistentId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task CacheInvalidation_AfterCreate_ReturnsUpdatedList()
    {
        // Arrange - get initial list
        var initialResponse = await _client.GetAsync("/api/ReferenceTables/MuscleGroups");
        initialResponse.EnsureSuccessStatusCode();
        var initialList = await initialResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        Assert.NotNull(initialList);
        var initialCount = initialList.Count;
        
        // Get a body part for creation
        var bodyPartsResponse = await _client.GetAsync("/api/ReferenceTables/BodyParts");
        bodyPartsResponse.EnsureSuccessStatusCode();
        var bodyParts = await bodyPartsResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        
        if (bodyParts == null || !bodyParts.Any())
        {
            return; // Skip test if no body parts available
        }
        
        var bodyPartId = bodyParts.First().Id;
        var createDto = new CreateMuscleGroupDto
        {
            Name = $"TestMuscleGroup_{Guid.NewGuid()}",
            BodyPartId = bodyPartId
        };
        
        var content = new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json");
        
        // Act - create new muscle group
        var createResponse = await _client.PostAsync("/api/ReferenceTables/MuscleGroups", content);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createdDto = await createResponse.Content.ReadFromJsonAsync<MuscleGroupDto>();
        
        // Get list again
        var updatedResponse = await _client.GetAsync("/api/ReferenceTables/MuscleGroups");
        updatedResponse.EnsureSuccessStatusCode();
        var updatedList = await updatedResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        
        // Assert
        Assert.NotNull(updatedList);
        Assert.NotNull(createdDto);
        
        // Check if the newly created item is in the list
        var newItemInList = updatedList.FirstOrDefault(mg => mg.Id == createdDto.Id);
        Assert.NotNull(newItemInList); // Ensure the new item is actually in the list
        
        Assert.Equal(initialCount + 1, updatedList.Count);
        Assert.Contains(updatedList, mg => mg.Value == createDto.Name);
        
        // Clean up
        if (createdDto != null)
        {
            await _client.DeleteAsync($"/api/ReferenceTables/MuscleGroups/{createdDto.Id}");
        }
    }
}
