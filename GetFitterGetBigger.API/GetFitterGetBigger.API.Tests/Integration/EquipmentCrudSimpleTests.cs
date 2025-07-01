using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Integration
{
    [Collection("SharedDatabase")]
    public class EquipmentCrudSimpleTests : IClassFixture<SharedDatabaseTestFixture>
    {
        private readonly SharedDatabaseTestFixture _fixture;
        private readonly HttpClient _client;
        
        public EquipmentCrudSimpleTests(SharedDatabaseTestFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.CreateClient();
        }
        

        [Fact]
        public async Task EquipmentCrud_FullFlow_Success()
        {
            // Create unique name to avoid conflicts
            var uniqueName = $"Test_Equipment_{System.Guid.NewGuid():N}";
            
            // Step 1: Create equipment
            var createDto = new CreateEquipmentDto { Name = uniqueName };
            var createResponse = await _client.PostAsJsonAsync("/api/ReferenceTables/Equipment", createDto);
            
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            var created = await createResponse.Content.ReadFromJsonAsync<EquipmentDto>();
            Assert.NotNull(created);
            Assert.Equal(uniqueName, created.Name);
            Assert.True(created.IsActive);

            // Step 2: Update equipment
            var updateDto = new UpdateEquipmentDto { Name = uniqueName + "_Updated" };
            var updateResponse = await _client.PutAsJsonAsync($"/api/ReferenceTables/Equipment/{created.Id}", updateDto);
            
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            var updated = await updateResponse.Content.ReadFromJsonAsync<EquipmentDto>();
            Assert.NotNull(updated);
            Assert.Equal(uniqueName + "_Updated", updated.Name);

            // Step 3: Delete equipment
            var deleteResponse = await _client.DeleteAsync($"/api/ReferenceTables/Equipment/{created.Id}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            // Step 4: Verify it's gone from GetAll
            var getAllResponse = await _client.GetAsync("/api/ReferenceTables/Equipment");
            getAllResponse.EnsureSuccessStatusCode();
            var allEquipment = await getAllResponse.Content.ReadFromJsonAsync<ReferenceDataDto[]>();
            Assert.NotNull(allEquipment);
            Assert.DoesNotContain(allEquipment, e => e.Id == created.Id);
        }

        [Fact]
        public async Task Create_WithEmptyName_ReturnsBadRequest()
        {
            var createDto = new CreateEquipmentDto { Name = "" };
            var response = await _client.PostAsJsonAsync("/api/ReferenceTables/Equipment", createDto);
            
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Update_NonExistent_ReturnsNotFound()
        {
            var fakeId = "equipment-00000000-0000-0000-0000-000000000000";
            var updateDto = new UpdateEquipmentDto { Name = "Updated" };
            var response = await _client.PutAsJsonAsync($"/api/ReferenceTables/Equipment/{fakeId}", updateDto);
            
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_NonExistent_ReturnsNotFound()
        {
            var fakeId = "equipment-00000000-0000-0000-0000-000000000000";
            var response = await _client.DeleteAsync($"/api/ReferenceTables/Equipment/{fakeId}");
            
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("invalid-format")]
        [InlineData("equipment-not-a-guid")]
        [InlineData("12345")]
        public async Task InvalidIdFormat_ReturnsBadRequest(string invalidId)
        {
            // Test GET
            var getResponse = await _client.GetAsync($"/api/ReferenceTables/Equipment/{invalidId}");
            Assert.Equal(HttpStatusCode.BadRequest, getResponse.StatusCode);

            // Test PUT
            var updateDto = new UpdateEquipmentDto { Name = "Test" };
            var putResponse = await _client.PutAsJsonAsync($"/api/ReferenceTables/Equipment/{invalidId}", updateDto);
            Assert.Equal(HttpStatusCode.BadRequest, putResponse.StatusCode);

            // Test DELETE
            var deleteResponse = await _client.DeleteAsync($"/api/ReferenceTables/Equipment/{invalidId}");
            Assert.Equal(HttpStatusCode.BadRequest, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task Create_ThenGetById_Success()
        {
            // Create unique name to avoid conflicts
            var uniqueName = $"Test_GetById_{System.Guid.NewGuid():N}";
            
            // Step 1: Create equipment
            var createDto = new CreateEquipmentDto { Name = uniqueName };
            var createResponse = await _client.PostAsJsonAsync("/api/ReferenceTables/Equipment", createDto);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<EquipmentDto>();
            Assert.NotNull(created);
            
            // Log the created ID to help debug
            var createdId = created.Id;
            // Assert ID format is correct
            Assert.StartsWith("equipment-", createdId);
            
            // Step 2: Get it by ID
            var getResponse = await _client.GetAsync($"/api/ReferenceTables/Equipment/{createdId}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            
            var fetched = await getResponse.Content.ReadFromJsonAsync<ReferenceDataDto>();
            Assert.NotNull(fetched);
            Assert.Equal(created.Id, fetched.Id);
            Assert.Equal(created.Name, fetched.Value);
            
            // Cleanup
            await _client.DeleteAsync($"/api/ReferenceTables/Equipment/{created.Id}");
        }

        [Fact]
        public async Task Update_JustCreatedEquipment_Success()
        {
            // Create unique name to avoid conflicts
            var uniqueName = $"Test_Update_{System.Guid.NewGuid():N}";
            
            // Step 1: Create equipment
            var createDto = new CreateEquipmentDto { Name = uniqueName };
            var createResponse = await _client.PostAsJsonAsync("/api/ReferenceTables/Equipment", createDto);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<EquipmentDto>();
            Assert.NotNull(created);
            
            // Step 2: Now update it
            var updateDto = new UpdateEquipmentDto { Name = uniqueName + "_Updated" };
            var updateResponse = await _client.PutAsJsonAsync($"/api/ReferenceTables/Equipment/{created.Id}", updateDto);
            
            // This should work
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            
            // Cleanup
            await _client.DeleteAsync($"/api/ReferenceTables/Equipment/{created.Id}");
        }

        [Fact]
        public async Task Create_TrimsWhitespace()
        {
            var uniqueName = $"Test_Trim_{System.Guid.NewGuid():N}";
            var createDto = new CreateEquipmentDto { Name = $"  {uniqueName}  " };
            var response = await _client.PostAsJsonAsync("/api/ReferenceTables/Equipment", createDto);
            
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<EquipmentDto>();
            Assert.NotNull(created);
            Assert.Equal(uniqueName, created.Name); // Should be trimmed

            // Cleanup
            await _client.DeleteAsync($"/api/ReferenceTables/Equipment/{created.Id}");
        }
    }
}