using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Tests.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;

namespace GetFitterGetBigger.Admin.Tests.Services
{
    public class EquipmentServiceTests
    {
        private readonly MockHttpMessageHandler _httpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ICacheHelperService> _cacheHelperMock;
        private readonly EquipmentService _equipmentService;
        private readonly JsonSerializerOptions _jsonOptions;

        public EquipmentServiceTests()
        {
            _httpMessageHandler = new MockHttpMessageHandler();
            _httpClient = new HttpClient(_httpMessageHandler)
            {
                BaseAddress = new Uri("http://localhost:5214")
            };
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _configurationMock = new Mock<IConfiguration>();
            _cacheHelperMock = new Mock<ICacheHelperService>();

            _configurationMock
                .Setup(x => x["ApiBaseUrl"])
                .Returns("http://localhost:5214");

            _equipmentService = new EquipmentService(
                _httpClient,
                _memoryCache,
                _configurationMock.Object,
                _cacheHelperMock.Object);

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        [Fact]
        public async Task GetEquipmentAsync_WhenNotCached_FetchesFromApiAndCaches()
        {
            // Arrange
            var referenceData = new List<ReferenceDataDto>
            {
                new() { Id = "equipment-33445566-7788-99aa-bbcc-ddeeff001122", Value = "Barbell", Description = null },
                new() { Id = "equipment-44556677-8899-aabb-ccdd-eeff00112233", Value = "Dumbbell", Description = null }
            };
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, referenceData);

            // Act
            var result = await _equipmentService.GetEquipmentAsync();

            // Assert
            result.Should().HaveCount(2);
            result.First().Name.Should().Be("Barbell");
            _httpMessageHandler.Requests.Should().HaveCount(1);

            // Verify caching
            var cachedResult = await _equipmentService.GetEquipmentAsync();
            cachedResult.Should().HaveCount(2);
            _httpMessageHandler.Requests.Should().HaveCount(1); // Should not make another request
        }

        [Fact]
        public async Task GetEquipmentAsync_WhenApiReturnsError_ThrowsHttpRequestException()
        {
            // Arrange
            _httpMessageHandler.SetupResponse(HttpStatusCode.InternalServerError, "Server error");

            // Act
            var act = async () => await _equipmentService.GetEquipmentAsync();

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>()
                .WithMessage("Failed to get equipment: InternalServerError");
        }

        [Fact]
        public async Task CreateEquipmentAsync_WhenSuccessful_ReturnsCreatedEquipmentAndInvalidatesCache()
        {
            // Arrange
            var createDto = new CreateEquipmentDto { Name = "Kettlebell" };
            var createdEquipment = new EquipmentDto 
            { 
                Id = "equipment-789", 
                Name = "Kettlebell", 
                IsActive = true, 
                CreatedAt = DateTime.UtcNow 
            };
            _httpMessageHandler.SetupResponse(HttpStatusCode.Created, createdEquipment);

            // Pre-populate cache
            _memoryCache.Set("EquipmentDto_Full", new List<EquipmentDto>());
            _memoryCache.Set("RefData_Equipment", new List<ReferenceDataDto>());

            // Act
            var result = await _equipmentService.CreateEquipmentAsync(createDto);

            // Assert
            result.Name.Should().Be("Kettlebell");
            _memoryCache.TryGetValue("EquipmentDto_Full", out _).Should().BeFalse(); // Cache should be invalidated
            _memoryCache.TryGetValue("RefData_Equipment", out _).Should().BeFalse(); // ReferenceData cache should also be invalidated
        }

        [Fact]
        public async Task CreateEquipmentAsync_WhenNameAlreadyExists_ThrowsInvalidOperationException()
        {
            // Arrange
            var createDto = new CreateEquipmentDto { Name = "Barbell" };
            _httpMessageHandler.SetupResponse(HttpStatusCode.Conflict, "Name already exists");

            // Act
            var act = async () => await _equipmentService.CreateEquipmentAsync(createDto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Equipment with this name already exists");
        }

        [Fact]
        public async Task UpdateEquipmentAsync_WhenSuccessful_ReturnsUpdatedEquipmentAndInvalidatesCache()
        {
            // Arrange
            var equipmentId = "equipment-123";
            var updateDto = new UpdateEquipmentDto { Name = "Olympic Barbell" };
            var updatedEquipment = new EquipmentDto 
            { 
                Id = equipmentId, 
                Name = "Olympic Barbell", 
                IsActive = true, 
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow
            };
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, updatedEquipment);

            // Pre-populate cache
            _memoryCache.Set("EquipmentDto_Full", new List<EquipmentDto>());
            _memoryCache.Set("RefData_Equipment", new List<ReferenceDataDto>());

            // Act
            var result = await _equipmentService.UpdateEquipmentAsync(equipmentId, updateDto);

            // Assert
            result.Name.Should().Be("Olympic Barbell");
            result.UpdatedAt.Should().NotBeNull();
            _memoryCache.TryGetValue("EquipmentDto_Full", out _).Should().BeFalse(); // Cache should be invalidated
            _memoryCache.TryGetValue("RefData_Equipment", out _).Should().BeFalse(); // ReferenceData cache should also be invalidated
        }

        [Fact]
        public async Task UpdateEquipmentAsync_WhenNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var equipmentId = "equipment-999";
            var updateDto = new UpdateEquipmentDto { Name = "Not Found" };
            _httpMessageHandler.SetupResponse(HttpStatusCode.NotFound, "Not found");

            // Act
            var act = async () => await _equipmentService.UpdateEquipmentAsync(equipmentId, updateDto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Equipment not found");
        }

        [Fact]
        public async Task UpdateEquipmentAsync_WhenNameConflict_ThrowsInvalidOperationException()
        {
            // Arrange
            var equipmentId = "equipment-123";
            var updateDto = new UpdateEquipmentDto { Name = "Dumbbell" };
            _httpMessageHandler.SetupResponse(HttpStatusCode.Conflict, "Name already exists");

            // Act
            var act = async () => await _equipmentService.UpdateEquipmentAsync(equipmentId, updateDto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Equipment with this name already exists");
        }

        [Fact]
        public async Task DeleteEquipmentAsync_WhenSuccessful_InvalidatesCache()
        {
            // Arrange
            var equipmentId = "equipment-123";
            _httpMessageHandler.SetupResponse(HttpStatusCode.NoContent, null);

            // Pre-populate cache
            _memoryCache.Set("EquipmentDto_Full", new List<EquipmentDto>());
            _memoryCache.Set("RefData_Equipment", new List<ReferenceDataDto>());

            // Act
            await _equipmentService.DeleteEquipmentAsync(equipmentId);

            // Assert
            _memoryCache.TryGetValue("EquipmentDto_Full", out _).Should().BeFalse(); // Cache should be invalidated
            _memoryCache.TryGetValue("RefData_Equipment", out _).Should().BeFalse(); // ReferenceData cache should also be invalidated
        }

        [Fact]
        public async Task DeleteEquipmentAsync_WhenNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var equipmentId = "equipment-999";
            _httpMessageHandler.SetupResponse(HttpStatusCode.NotFound, "Not found");

            // Act
            var act = async () => await _equipmentService.DeleteEquipmentAsync(equipmentId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Equipment not found");
        }

        [Fact]
        public async Task DeleteEquipmentAsync_WhenInUse_ThrowsInvalidOperationException()
        {
            // Arrange
            var equipmentId = "equipment-123";
            _httpMessageHandler.SetupResponse(HttpStatusCode.Conflict, "In use by exercises");

            // Act
            var act = async () => await _equipmentService.DeleteEquipmentAsync(equipmentId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Cannot delete equipment that is in use by exercises");
        }

        [Fact]
        public async Task CreateEquipmentAsync_WhenServerError_ThrowsHttpRequestException()
        {
            // Arrange
            _httpMessageHandler.SetupResponse(HttpStatusCode.InternalServerError, "Server error");

            // Act
            var act = async () => await _equipmentService.CreateEquipmentAsync(new CreateEquipmentDto { Name = "Test" });

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>()
                .WithMessage("Failed to create equipment: InternalServerError");
        }

        [Fact]
        public async Task UpdateEquipmentAsync_WhenServerError_ThrowsHttpRequestException()
        {
            // Arrange
            _httpMessageHandler.SetupResponse(HttpStatusCode.InternalServerError, "Server error");

            // Act
            var act = async () => await _equipmentService.UpdateEquipmentAsync("id", new UpdateEquipmentDto { Name = "Test" });

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>()
                .WithMessage("Failed to update equipment: InternalServerError");
        }

        [Fact]
        public async Task DeleteEquipmentAsync_WhenServerError_ThrowsHttpRequestException()
        {
            // Arrange
            _httpMessageHandler.SetupResponse(HttpStatusCode.InternalServerError, "Server error");

            // Act
            var act = async () => await _equipmentService.DeleteEquipmentAsync("id");

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>()
                .WithMessage("Failed to delete equipment: InternalServerError");
        }
    }
}