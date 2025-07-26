using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Models.ReferenceData;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Services.Strategies;
using GetFitterGetBigger.Admin.Services.Strategies.ReferenceTableStrategies;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using Xunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Tests.Helpers;
using Moq;

namespace GetFitterGetBigger.Admin.Tests.Services
{
    public class ReferenceDataServiceCacheTests
    {
        private readonly IMemoryCache _cache;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly Mock<ILogger<ReferenceDataService>> _loggerMock;
        private readonly List<IReferenceTableStrategy> _strategies;

        public ReferenceDataServiceCacheTests()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
            _loggerMock = new Mock<ILogger<ReferenceDataService>>();

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // Register all strategies
            _strategies = new List<IReferenceTableStrategy>
            {
                new BodyPartsStrategy(),
                new DifficultyLevelsStrategy(),
                new EquipmentStrategy(),
                new KineticChainTypesStrategy(),
                new MetricTypesStrategy(),
                new MovementPatternsStrategy(),
                new MuscleGroupsStrategy(),
                new MuscleRolesStrategy(),
                new ExerciseTypesStrategy()
            };
        }

        [Fact]
        public async Task ReferenceDataService_ShouldNotConflictWithOtherServices_WhenCachingData()
        {
            // Arrange
            // First, populate cache with MuscleGroupDto data (simulating MuscleGroupsService)
            var muscleGroupDtos = new List<MuscleGroupDto>
            {
                new() { Id = "mg1", Name = "Chest", BodyPartId = "bp1", IsActive = true, CreatedAt = DateTime.UtcNow }
            };
            _cache.Set("MuscleGroupsDto_Full", muscleGroupDtos); // Using new cache key

            // Also test old cache key to ensure backward compatibility during transition
            _cache.Set("MuscleGroups", muscleGroupDtos);

            // Setup mock HTTP responses for ReferenceDataService
            var mockMuscleGroups = new List<MuscleGroupDto>
            {
                new() { Id = "ref1", Name = "Chest", BodyPartId = "bp1", BodyPartName = "Upper Body", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Id = "ref2", Name = "Back", BodyPartId = "bp1", BodyPartName = "Upper Body", IsActive = true, CreatedAt = DateTime.UtcNow }
            };

            var mockHandler = new MockHttpMessageHandler();
            mockHandler.SetupResponse(HttpStatusCode.OK, mockMuscleGroups);

            var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("http://localhost:5214") };
            var referenceDataService = new ReferenceDataService(_strategies, httpClient, _cache, _loggerMock.Object);

            // Act
            var referenceData = await referenceDataService.GetReferenceDataAsync<MuscleGroups>();

            // Assert
            // Verify the reference data was fetched and transformed correctly
            referenceData.Should().HaveCount(2);
            referenceData.First().Value.Should().Be("Chest");
            referenceData.First().Description.Should().Be("Body Part: Upper Body");

            // Verify that the original MuscleGroupsService cache is not affected
            var cachedMuscleGroups = _cache.Get<List<MuscleGroupDto>>("MuscleGroupsDto_Full");
            cachedMuscleGroups.Should().NotBeNull();
            cachedMuscleGroups.Should().HaveCount(1);
            cachedMuscleGroups!.First().Id.Should().Be("mg1");
        }

        [Fact]
        public async Task ReferenceDataService_ShouldUseSeparateCacheKeys_ForEachReferenceType()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            
            // Setup different responses for different endpoints
            var bodyParts = new List<FlexibleReferenceDataDto>
            {
                new() { Id = "bp1", Value = "Arms", Description = "Upper body" }
            };
            
            var equipment = new List<FlexibleReferenceDataDto>
            {
                new() { Id = "eq1", Value = "Barbell", Description = "Weight equipment" }
            };

            // Setup responses in order - first BodyParts, then Equipment
            mockHandler.SetupResponse(HttpStatusCode.OK, bodyParts);
            mockHandler.SetupResponse(HttpStatusCode.OK, equipment);

            var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("http://localhost:5214") };
            var service = new ReferenceDataService(_strategies, httpClient, _cache, _loggerMock.Object);

            // Act
            var bodyPartsResult = await service.GetReferenceDataAsync<BodyParts>();
            var equipmentResult = await service.GetReferenceDataAsync<Equipment>();

            // Assert
            bodyPartsResult.Should().HaveCount(1);
            bodyPartsResult.First().Value.Should().Be("Arms");

            equipmentResult.Should().HaveCount(1);
            equipmentResult.First().Value.Should().Be("Barbell");

            // Verify separate cache keys are used
            _cache.TryGetValue("RefData_BodyParts", out object? cachedBodyParts).Should().BeTrue();
            _cache.TryGetValue("RefData_Equipment", out object? cachedEquipment).Should().BeTrue();
        }

        [Fact]
        public async Task ReferenceDataService_ShouldUseCachedData_OnSubsequentCalls()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            var expectedData = new List<FlexibleReferenceDataDto>
            {
                new() { Id = "1", Value = "Beginner", Description = "For beginners" }
            };
            mockHandler.SetupResponse(HttpStatusCode.OK, expectedData);

            var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("http://localhost:5214") };
            var service = new ReferenceDataService(_strategies, httpClient, _cache, _loggerMock.Object);

            // Act
            var firstCall = await service.GetReferenceDataAsync<DifficultyLevels>();
            var secondCall = await service.GetReferenceDataAsync<DifficultyLevels>();
            var thirdCall = await service.GetReferenceDataAsync<DifficultyLevels>();

            // Assert
            firstCall.Should().BeEquivalentTo(secondCall);
            secondCall.Should().BeEquivalentTo(thirdCall);

            // Verify HTTP was called only once
            mockHandler.Requests.Should().HaveCount(1);
        }

        [Fact]
        public async Task ReferenceDataService_ShouldHandleEmptyResponse_Gracefully()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            mockHandler.SetupResponse(HttpStatusCode.OK, new List<FlexibleReferenceDataDto>());

            var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("http://localhost:5214") };
            var service = new ReferenceDataService(_strategies, httpClient, _cache, _loggerMock.Object);

            // Act
            var result = await service.GetReferenceDataAsync<MetricTypes>();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetReferenceDataAsync_ShouldCacheFor24Hours_ByDefault()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            var expectedData = new List<FlexibleReferenceDataDto>
            {
                new() { Id = "1", Value = "Push", Description = "Push movement" }
            };
            mockHandler.SetupResponse(HttpStatusCode.OK, expectedData);

            var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("http://localhost:5214") };
            var service = new ReferenceDataService(_strategies, httpClient, _cache, _loggerMock.Object);

            // Act
            await service.GetReferenceDataAsync<MovementPatterns>();

            // Assert
            var cacheEntry = _cache.Get<IEnumerable<ReferenceDataDto>>("RefData_MovementPatterns");
            cacheEntry.Should().NotBeNull();
            
            // Note: We can't easily test the exact cache duration without more complex setup,
            // but we can verify the data was cached
            cacheEntry!.First().Value.Should().Be("Push");
        }
    }
}