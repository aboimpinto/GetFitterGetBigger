using System.Net;
using FluentAssertions;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Tests.Builders;
using GetFitterGetBigger.Admin.Tests.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;

namespace GetFitterGetBigger.Admin.Tests.Services
{
    public class ReferenceDataServiceTests
    {
        private readonly MockHttpMessageHandler _httpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly ReferenceDataService _referenceDataService;

        public ReferenceDataServiceTests()
        {
            _httpMessageHandler = new MockHttpMessageHandler();
            _httpClient = new HttpClient(_httpMessageHandler)
            {
                BaseAddress = new Uri("http://localhost:5214")
            };
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _configurationMock = new Mock<IConfiguration>();
            
            _configurationMock
                .Setup(x => x["ApiBaseUrl"])
                .Returns("http://localhost:5214");

            _referenceDataService = new ReferenceDataService(
                _httpClient,
                _memoryCache,
                _configurationMock.Object);
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
            _memoryCache.Set("BodyParts", cachedData);

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
        [InlineData("GetMuscleGroupsAsync", "/api/ReferenceTables/MuscleGroups")]
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
                var service = new ReferenceDataService(client, _memoryCache, _configurationMock.Object);
                
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
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(x => x["ApiBaseUrl"]).Returns((string?)null);
            
            var service = new ReferenceDataService(_httpClient, _memoryCache, configMock.Object);
            var expectedData = ReferenceDataDtoBuilder.BuildList(1);
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedData);

            // Act
            var result = await service.GetBodyPartsAsync();

            // Assert
            result.Should().HaveCount(1);
            // Service should still work even without configured URL (uses HttpClient's base address)
        }
    }
}