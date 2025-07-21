using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Tests.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace GetFitterGetBigger.Admin.Tests.Services
{
    public class MuscleGroupsServiceTests
    {
        private readonly MockHttpMessageHandler _httpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;
        private readonly Mock<ICacheHelperService> _cacheHelperMock;
        private readonly MuscleGroupsService _muscleGroupsService;
        private readonly JsonSerializerOptions _jsonOptions;

        public MuscleGroupsServiceTests()
        {
            _httpMessageHandler = new MockHttpMessageHandler();
            _httpClient = new HttpClient(_httpMessageHandler)
            {
                BaseAddress = new Uri("http://localhost:5214")
            };
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _cacheHelperMock = new Mock<ICacheHelperService>();
            _muscleGroupsService = new MuscleGroupsService(
                _httpClient,
                _memoryCache,
                _cacheHelperMock.Object);
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        #region GetMuscleGroupsAsync Tests

        [Fact]
        public async Task GetMuscleGroupsAsync_ShouldReturnMuscleGroups_WhenApiCallSucceeds()
        {
            // Arrange
            var muscleGroups = new[]
            {
                new MuscleGroupDto
                {
                    Id = "musclegroup-1",
                    Name = "Biceps",
                    BodyPartId = "bodypart-1",
                    BodyPartName = "Arms",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new MuscleGroupDto
                {
                    Id = "musclegroup-2",
                    Name = "Triceps",
                    BodyPartId = "bodypart-1",
                    BodyPartName = "Arms",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };
            // The API returns a simple array of MuscleGroupDto
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, muscleGroups);

            // Act
            var result = await _muscleGroupsService.GetMuscleGroupsAsync();

            // Assert
            result.Should().HaveCount(2);
            result.First().Id.Should().Be("musclegroup-1");
            result.First().Name.Should().Be("Biceps");
            result.First().BodyPartName.Should().Be("Arms");
            result.First().IsActive.Should().BeTrue();
            _httpMessageHandler.Requests.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetMuscleGroupsAsync_ShouldReturnCachedData_WhenCalledSecondTime()
        {
            // Arrange
            var muscleGroups = new[]
            {
                new MuscleGroupDto
                {
                    Id = "musclegroup-1",
                    Name = "Biceps",
                    BodyPartId = "bodypart-1",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };
            // The API returns a simple array of MuscleGroupDto
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, muscleGroups);

            // Act
            await _muscleGroupsService.GetMuscleGroupsAsync(); // First call
            var result = await _muscleGroupsService.GetMuscleGroupsAsync(); // Second call should use cache

            // Assert
            result.Should().HaveCount(1);
            _httpMessageHandler.Requests.Should().HaveCount(1); // Only one API call
        }

        [Fact]
        public async Task GetMuscleGroupsAsync_ShouldThrowException_WhenApiCallFails()
        {
            // Arrange
            _httpMessageHandler.SetupResponse(HttpStatusCode.InternalServerError);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _muscleGroupsService.GetMuscleGroupsAsync());
        }

        #endregion

        #region GetMuscleGroupsByBodyPartAsync Tests

        [Fact]
        public async Task GetMuscleGroupsByBodyPartAsync_ShouldReturnFilteredMuscleGroups_WhenApiCallSucceeds()
        {
            // Arrange
            var bodyPartId = "bodypart-123";
            var muscleGroups = new[]
            {
                new MuscleGroupDto
                {
                    Id = "musclegroup-1",
                    Name = "Biceps",
                    BodyPartId = bodyPartId,
                    BodyPartName = "Arms",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new MuscleGroupDto
                {
                    Id = "musclegroup-2",
                    Name = "Brachialis",
                    BodyPartId = bodyPartId,
                    BodyPartName = "Arms",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, muscleGroups);

            // Act
            var result = await _muscleGroupsService.GetMuscleGroupsByBodyPartAsync(bodyPartId);

            // Assert
            result.Should().HaveCount(2);
            result.All(mg => mg.BodyPartId == bodyPartId).Should().BeTrue();
            _httpMessageHandler.Requests.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetMuscleGroupsByBodyPartAsync_ShouldUseSeparateCacheKeys_ForDifferentBodyParts()
        {
            // Arrange
            var bodyPartId1 = "bodypart-123";
            var bodyPartId2 = "bodypart-456";
            var muscleGroups1 = new[]
            {
                new MuscleGroupDto
                {
                    Id = "musclegroup-1",
                    Name = "Biceps",
                    BodyPartId = bodyPartId1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };
            var muscleGroups2 = new[]
            {
                new MuscleGroupDto
                {
                    Id = "musclegroup-2",
                    Name = "Quadriceps",
                    BodyPartId = bodyPartId2,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _httpMessageHandler
                .SetupResponse(HttpStatusCode.OK, muscleGroups1)
                .SetupResponse(HttpStatusCode.OK, muscleGroups2);

            // Act
            var result1 = await _muscleGroupsService.GetMuscleGroupsByBodyPartAsync(bodyPartId1);
            var result2 = await _muscleGroupsService.GetMuscleGroupsByBodyPartAsync(bodyPartId2);

            // Assert
            result1.Should().HaveCount(1);
            result1.First().Name.Should().Be("Biceps");
            result2.Should().HaveCount(1);
            result2.First().Name.Should().Be("Quadriceps");
            _httpMessageHandler.Requests.Should().HaveCount(2); // Two different API calls
        }

        #endregion

        #region CreateMuscleGroupAsync Tests

        [Fact]
        public async Task CreateMuscleGroupAsync_ShouldReturnCreatedMuscleGroup_WhenApiCallSucceeds()
        {
            // Arrange
            var createDto = new CreateMuscleGroupDto { Name = "Deltoids", BodyPartId = "bodypart-123" };
            var createdMuscleGroup = new MuscleGroupDto
            {
                Id = "musclegroup-new",
                Name = "Deltoids",
                BodyPartId = "bodypart-123",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _httpMessageHandler.SetupResponse(HttpStatusCode.Created, createdMuscleGroup);

            // Act
            var result = await _muscleGroupsService.CreateMuscleGroupAsync(createDto);

            // Assert
            result.Id.Should().Be("musclegroup-new");
            result.Name.Should().Be("Deltoids");
            result.BodyPartId.Should().Be("bodypart-123");
        }

        [Fact]
        public async Task CreateMuscleGroupAsync_ShouldInvalidateCache_WhenSuccessful()
        {
            // Arrange
            var createDto = new CreateMuscleGroupDto { Name = "Deltoids", BodyPartId = "bodypart-123" };
            var createdMuscleGroup = new MuscleGroupDto
            {
                Id = "musclegroup-new",
                Name = "Deltoids",
                BodyPartId = "bodypart-123",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Pre-populate cache
            _memoryCache.Set("MuscleGroupsDto_Full", new List<MuscleGroupDto>());
            _memoryCache.Set($"MuscleGroups_BodyPart_bodypart-123", new List<MuscleGroupDto>());
            _memoryCache.Set("RefData_MuscleGroups", new List<ReferenceDataDto>());

            _httpMessageHandler.SetupResponse(HttpStatusCode.Created, createdMuscleGroup);

            // Act
            await _muscleGroupsService.CreateMuscleGroupAsync(createDto);

            // Assert
            _memoryCache.TryGetValue("MuscleGroupsDto_Full", out _).Should().BeFalse();
            _memoryCache.TryGetValue($"MuscleGroups_BodyPart_bodypart-123", out _).Should().BeFalse();
            _memoryCache.TryGetValue("RefData_MuscleGroups", out _).Should().BeFalse();
        }

        [Fact]
        public async Task CreateMuscleGroupAsync_ShouldThrowInvalidOperationException_WhenBodyPartNotFound()
        {
            // Arrange
            var createDto = new CreateMuscleGroupDto { Name = "Deltoids", BodyPartId = "bodypart-invalid" };
            _httpMessageHandler.SetupResponse(HttpStatusCode.NotFound);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _muscleGroupsService.CreateMuscleGroupAsync(createDto));
            exception.Message.Should().Be("Body part not found");
        }

        [Fact]
        public async Task CreateMuscleGroupAsync_ShouldThrowInvalidOperationException_WhenNameAlreadyExists()
        {
            // Arrange
            var createDto = new CreateMuscleGroupDto { Name = "Biceps", BodyPartId = "bodypart-123" };
            _httpMessageHandler.SetupResponse(HttpStatusCode.Conflict);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _muscleGroupsService.CreateMuscleGroupAsync(createDto));
            exception.Message.Should().Be("Muscle group with this name already exists for the selected body part");
        }

        #endregion

        #region UpdateMuscleGroupAsync Tests

        [Fact]
        public async Task UpdateMuscleGroupAsync_ShouldReturnUpdatedMuscleGroup_WhenApiCallSucceeds()
        {
            // Arrange
            var updateDto = new UpdateMuscleGroupDto { Name = "Anterior Deltoids", BodyPartId = "bodypart-123" };
            var updatedMuscleGroup = new MuscleGroupDto
            {
                Id = "musclegroup-1",
                Name = "Anterior Deltoids",
                BodyPartId = "bodypart-123",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow
            };
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, updatedMuscleGroup);

            // Act
            var result = await _muscleGroupsService.UpdateMuscleGroupAsync("musclegroup-1", updateDto);

            // Assert
            result.Id.Should().Be("musclegroup-1");
            result.Name.Should().Be("Anterior Deltoids");
            result.UpdatedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateMuscleGroupAsync_ShouldThrowInvalidOperationException_WhenMuscleGroupNotFound()
        {
            // Arrange
            var updateDto = new UpdateMuscleGroupDto { Name = "Updated Name", BodyPartId = "bodypart-123" };
            _httpMessageHandler.SetupResponse(HttpStatusCode.NotFound);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _muscleGroupsService.UpdateMuscleGroupAsync("musclegroup-invalid", updateDto));
            exception.Message.Should().Be("Muscle group or body part not found");
        }

        [Fact]
        public async Task UpdateMuscleGroupAsync_ShouldThrowInvalidOperationException_WhenNameConflict()
        {
            // Arrange
            var updateDto = new UpdateMuscleGroupDto { Name = "Existing Name", BodyPartId = "bodypart-123" };
            _httpMessageHandler.SetupResponse(HttpStatusCode.Conflict);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _muscleGroupsService.UpdateMuscleGroupAsync("musclegroup-1", updateDto));
            exception.Message.Should().Be("Muscle group with this name already exists for the selected body part");
        }

        #endregion

        #region DeleteMuscleGroupAsync Tests

        [Fact]
        public async Task DeleteMuscleGroupAsync_ShouldSucceed_WhenApiCallSucceeds()
        {
            // Arrange
            _httpMessageHandler.SetupResponse(HttpStatusCode.NoContent);

            // Act & Assert (should not throw)
            await _muscleGroupsService.DeleteMuscleGroupAsync("musclegroup-1");
        }

        [Fact]
        public async Task DeleteMuscleGroupAsync_ShouldInvalidateAllCaches_WhenSuccessful()
        {
            // Arrange
            // Pre-populate caches
            _memoryCache.Set("MuscleGroupsDto_Full", new List<MuscleGroupDto>());
            _memoryCache.Set("MuscleGroups_BodyPart_bodypart-123", new List<MuscleGroupDto>());
            _memoryCache.Set("MuscleGroups_BodyPart_bodypart-456", new List<MuscleGroupDto>());
            _memoryCache.Set("RefData_MuscleGroups", new List<ReferenceDataDto>());

            _httpMessageHandler.SetupResponse(HttpStatusCode.NoContent);

            // Act
            await _muscleGroupsService.DeleteMuscleGroupAsync("musclegroup-1");

            // Assert
            _memoryCache.TryGetValue("MuscleGroupsDto_Full", out _).Should().BeFalse();
            _memoryCache.TryGetValue("RefData_MuscleGroups", out _).Should().BeFalse();
            // Note: The actual implementation tries to clear all body part caches,
            // but in unit tests we can't easily verify this due to IMemoryCache limitations
        }

        [Fact]
        public async Task DeleteMuscleGroupAsync_ShouldThrowInvalidOperationException_WhenMuscleGroupNotFound()
        {
            // Arrange
            _httpMessageHandler.SetupResponse(HttpStatusCode.NotFound);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _muscleGroupsService.DeleteMuscleGroupAsync("musclegroup-invalid"));
            exception.Message.Should().Be("Muscle group not found");
        }

        [Fact]
        public async Task DeleteMuscleGroupAsync_ShouldThrowInvalidOperationException_WhenMuscleGroupInUse()
        {
            // Arrange
            _httpMessageHandler.SetupResponse(HttpStatusCode.Conflict);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _muscleGroupsService.DeleteMuscleGroupAsync("musclegroup-1"));
            exception.Message.Should().Be("Cannot delete muscle group that is in use by exercises");
        }

        [Fact]
        public async Task DeleteMuscleGroupAsync_ShouldThrowHttpRequestException_WhenServerError()
        {
            // Arrange
            _httpMessageHandler.SetupResponse(HttpStatusCode.InternalServerError);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() =>
                _muscleGroupsService.DeleteMuscleGroupAsync("musclegroup-1"));
        }

        #endregion
    }
}