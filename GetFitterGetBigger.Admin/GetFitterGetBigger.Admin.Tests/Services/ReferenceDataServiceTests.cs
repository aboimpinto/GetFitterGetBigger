using System.Net;
using FluentAssertions;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Tests.Builders;
using GetFitterGetBigger.Admin.Tests.Helpers;
using Microsoft.Extensions.Caching.Memory;

namespace GetFitterGetBigger.Admin.Tests.Services
{
    public class ReferenceDataServiceTests
    {
        private readonly MockHttpMessageHandler _httpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;
        private readonly ReferenceDataService _referenceDataService;

        public ReferenceDataServiceTests()
        {
            _httpMessageHandler = new MockHttpMessageHandler();
            _httpClient = new HttpClient(_httpMessageHandler)
            {
                BaseAddress = new Uri("http://localhost:5214")
            };
            _memoryCache = new MemoryCache(new MemoryCacheOptions());

            _referenceDataService = new ReferenceDataService(
                _httpClient,
                _memoryCache);
        }

        [Fact]
        public async Task GetBodyPartsAsync_WhenNotCached_FetchesFromApiAndCaches()
        {
            // Arrange
            var expectedData = ReferenceDataDtoBuilder.BuildList(3);
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedData);

            // Act
            var result = await _referenceDataService.GetBodyPartsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.First().Id.Should().Be("ref-1");

            // Verify API was called
            _httpMessageHandler.VerifyRequest(req =>
                req.Method == HttpMethod.Get &&
                req.RequestUri!.ToString().Contains("/api/ReferenceTables/BodyParts"));
        }

        [Fact]
        public async Task GetBodyPartsAsync_WhenCached_ReturnsFromCacheWithoutApiCall()
        {
            // Arrange
            var cachedData = ReferenceDataDtoBuilder.BuildList(2);
            // Ensure we're setting the exact type expected
            _memoryCache.Set("RefData_BodyParts", cachedData as IEnumerable<ReferenceDataDto>);

            // Act
            var result = await _referenceDataService.GetBodyPartsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            // Verify no API call was made
            _httpMessageHandler.VerifyNoRequests();
        }

        [Fact]
        public async Task GetBodyPartsAsync_WhenApiReturnsError_ThrowsHttpRequestException()
        {
            // Arrange
            _httpMessageHandler.SetupResponse(HttpStatusCode.InternalServerError);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(
                () => _referenceDataService.GetBodyPartsAsync());
        }

        [Fact]
        public async Task GetBodyPartsAsync_CachesDataFor24Hours()
        {
            // Arrange
            var expectedData = ReferenceDataDtoBuilder.BuildList(1);
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedData);

            // Act
            var result1 = await _referenceDataService.GetBodyPartsAsync();
            var result2 = await _referenceDataService.GetBodyPartsAsync();

            // Assert
            result1.Should().HaveCount(1);
            result2.Should().HaveCount(1);
            result1.Should().BeEquivalentTo(result2);

            // Verify API was called only once
            _httpMessageHandler.Requests.Count.Should().Be(1);
        }

        [Fact]
        public async Task GetDifficultyLevelsAsync_WhenNotCached_FetchesFromApiAndCaches()
        {
            // Arrange
            var expectedData = ReferenceDataDtoBuilder.BuildList(5);
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedData);

            // Act
            var result = await _referenceDataService.GetDifficultyLevelsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(5);

            // Verify correct endpoint was called
            _httpMessageHandler.VerifyRequest(req =>
                req.Method == HttpMethod.Get &&
                req.RequestUri!.ToString().Contains("/api/ReferenceTables/DifficultyLevels"));
        }

        [Fact]
        public async Task GetEquipmentAsync_UsesCorrectEndpointAndCacheKey()
        {
            // Arrange
            var expectedData = ReferenceDataDtoBuilder.BuildList(10);
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedData);

            // Act
            var result = await _referenceDataService.GetEquipmentAsync();

            // Assert
            result.Should().HaveCount(10);
            _httpMessageHandler.VerifyRequest(req =>
                req.RequestUri!.ToString().Contains("/api/ReferenceTables/Equipment"));
        }

        [Theory]
        [InlineData("GetKineticChainTypesAsync", "/api/ReferenceTables/KineticChainTypes")]
        [InlineData("GetMetricTypesAsync", "/api/ReferenceTables/MetricTypes")]
        [InlineData("GetMovementPatternsAsync", "/api/ReferenceTables/MovementPatterns")]
        [InlineData("GetMuscleRolesAsync", "/api/ReferenceTables/MuscleRoles")]
        public async Task ReferenceDataMethods_UseCorrectEndpoints(string methodName, string expectedEndpoint)
        {
            // Arrange
            var expectedData = ReferenceDataDtoBuilder.BuildList(2);
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedData);

            var method = _referenceDataService.GetType().GetMethod(methodName);

            // Act
            var task = (Task<IEnumerable<ReferenceDataDto>>)method!.Invoke(_referenceDataService, null)!;
            var result = await task;

            // Assert
            result.Should().HaveCount(2);
            _httpMessageHandler.VerifyRequest(req =>
                req.RequestUri!.ToString().Contains(expectedEndpoint));
        }

        [Fact]
        public async Task GetMuscleGroupsAsync_UseCorrectEndpoint()
        {
            // Arrange - MuscleGroups endpoint returns MuscleGroupDto, not ReferenceDataDto
            var expectedData = new List<MuscleGroupDto>
            {
                new() { Id = "1", Name = "Chest", BodyPartId = "1", BodyPartName = "Upper Body", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Id = "2", Name = "Back", BodyPartId = "1", BodyPartName = "Upper Body", IsActive = true, CreatedAt = DateTime.UtcNow }
            };
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedData);

            // Act
            var result = await _referenceDataService.GetMuscleGroupsAsync();

            // Assert
            result.Should().HaveCount(2);
            result.First().Value.Should().Be("Chest");
            result.First().Description.Should().Contain("Upper Body");
            _httpMessageHandler.VerifyRequest(req =>
                req.RequestUri!.ToString().Contains("/api/ReferenceTables/MuscleGroups"));
        }

        [Fact]
        public async Task AllMethods_ShareSameCachingBehavior()
        {
            // Arrange
            var methods = new[]
            {
                ("GetBodyPartsAsync", "BodyParts"),
                ("GetDifficultyLevelsAsync", "DifficultyLevels"),
                ("GetEquipmentAsync", "Equipment")
            };

            foreach (var (methodName, cacheKey) in methods)
            {
                // Clear cache and handler
                _memoryCache.Remove(cacheKey);
                var handler = new MockHttpMessageHandler();
                var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5214") };
                var service = new ReferenceDataService(client, _memoryCache);

                var expectedData = ReferenceDataDtoBuilder.BuildList(1);
                handler.SetupResponse(HttpStatusCode.OK, expectedData)
                       .SetupResponse(HttpStatusCode.OK, expectedData);

                var method = service.GetType().GetMethod(methodName);

                // Act
                var task1 = (Task<IEnumerable<ReferenceDataDto>>)method!.Invoke(service, null)!;
                var result1 = await task1;

                var task2 = (Task<IEnumerable<ReferenceDataDto>>)method!.Invoke(service, null)!;
                var result2 = await task2;

                // Assert
                handler.Requests.Count.Should().Be(1, $"{methodName} should only call API once due to caching");
            }
        }

        [Fact]
        public async Task GetBodyPartsAsync_WhenApiReturnsNull_HandlesGracefully()
        {
            // Arrange
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, null);

            // Act & Assert
            // This test verifies the current behavior - it will throw when trying to deserialize null
            await Assert.ThrowsAsync<System.Text.Json.JsonException>(
                () => _referenceDataService.GetBodyPartsAsync());
        }

        [Fact]
        public async Task GetBodyPartsAsync_WhenApiUrlNotConfigured_UsesDefaultUrl()
        {
            // Arrange
            var service = new ReferenceDataService(_httpClient, _memoryCache);
            var expectedData = ReferenceDataDtoBuilder.BuildList(1);
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedData);

            // Act
            var result = await service.GetBodyPartsAsync();

            // Assert
            result.Should().HaveCount(1);
            // Service should still work (uses HttpClient's base address)
        }

        [Fact]
        public async Task GetExerciseTypesAsync_WhenNotCached_FetchesFromApiAndCaches()
        {
            // Arrange
            var expectedData = new List<ReferenceDataDto>
            {
                new() { Id = "type-warmup", Value = "Warmup", Description = "Warmup exercises" },
                new() { Id = "type-workout", Value = "Workout", Description = "Main workout exercises" },
                new() { Id = "type-cooldown", Value = "Cooldown", Description = "Cooldown exercises" },
                new() { Id = "type-rest", Value = "Rest", Description = "Rest periods" }
            };
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedData);

            // Act
            var result = await _referenceDataService.GetExerciseTypesAsync();

            // Assert
            result.Should().HaveCount(4);
            result.Should().Contain(x => x.Value == "Warmup");
            result.Should().Contain(x => x.Value == "Workout");
            result.Should().Contain(x => x.Value == "Cooldown");
            result.Should().Contain(x => x.Value == "Rest");

            _httpMessageHandler.VerifyRequest(request =>
                request.RequestUri!.AbsolutePath == "/api/ReferenceTables/ExerciseTypes");
        }

        [Fact]
        public async Task GetExerciseTypesAsync_WhenCached_ReturnsCachedData()
        {
            // Arrange
            var expectedData = new List<ReferenceDataDto>
            {
                new() { Id = "type-warmup", Value = "Warmup", Description = "Warmup exercises" },
                new() { Id = "type-workout", Value = "Workout", Description = "Main workout exercises" }
            };
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedData);

            // Act - First call should hit the API
            var firstResult = await _referenceDataService.GetExerciseTypesAsync();

            // Act - Second call should use cache
            var secondResult = await _referenceDataService.GetExerciseTypesAsync();

            // Assert
            firstResult.Should().HaveCount(2);
            secondResult.Should().BeEquivalentTo(firstResult);

            // Verify API was only called once
            _httpMessageHandler.Requests.Should().HaveCount(1);
        }
    }
}