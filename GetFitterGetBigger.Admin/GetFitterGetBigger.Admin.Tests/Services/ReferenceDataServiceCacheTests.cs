using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Tests.Helpers;

namespace GetFitterGetBigger.Admin.Tests.Services
{
    public class ReferenceDataServiceCacheTests
    {
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions;

        public ReferenceDataServiceCacheTests()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());

            var inMemorySettings = new Dictionary<string, string?>
            {
                {"ApiBaseUrl", "http://localhost:5214"}
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        [Fact]
        public async Task ReferenceDataService_ShouldNotConflictWithOtherServices_WhenCachingData()
        {
            // Arrange
            // First, populate cache with MuscleGroupDto data (simulating MuscleGroupsService)
            var muscleGroupDtos = new List<MuscleGroupDto>
            {
                new() { Id = "mg1", Name = "Chest", BodyPartId = "bp1", IsActive = true }
            };
            _cache.Set("MuscleGroupsDto_Full", muscleGroupDtos); // Using new cache key

            // Also test old cache key to ensure backward compatibility during transition
            _cache.Set("MuscleGroups", muscleGroupDtos);

            // Setup mock HTTP responses for ReferenceDataService
            // MuscleGroups endpoint now expects MuscleGroupDto
            var mockMuscleGroups = new List<MuscleGroupDto>
            {
                new() { Id = "ref1", Name = "Chest", BodyPartId = "bp1", BodyPartName = "Upper Body", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Id = "ref2", Name = "Back", BodyPartId = "bp1", BodyPartName = "Upper Body", IsActive = true, CreatedAt = DateTime.UtcNow }
            };

            var mockHandler = new MockHttpMessageHandler();
            mockHandler.SetupResponse(HttpStatusCode.OK, mockMuscleGroups);

            var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("http://localhost:5214") };
            var referenceDataService = new ReferenceDataService(httpClient, _cache, _configuration);

            // Act
            var result = await referenceDataService.GetMuscleGroupsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.First().Should().BeOfType<ReferenceDataDto>();
            result.First().Id.Should().Be("ref1");
            result.First().Value.Should().Be("Chest");

            // Verify the cache now contains the correct type under the new key
            _cache.TryGetValue("RefData_MuscleGroups", out object? cachedData).Should().BeTrue();
            cachedData.Should().BeOfType<List<ReferenceDataDto>>();
        }

        [Fact]
        public async Task ReferenceDataService_ShouldHandleInvalidCastException_WhenCacheContainsWrongType()
        {
            // Arrange
            // Populate cache with wrong type data
            var wrongTypeData = new List<MuscleGroupDto>
            {
                new() { Id = "mg1", Name = "Equipment Item", BodyPartId = "bp1", IsActive = true }
            };
            _cache.Set("RefData_Equipment", wrongTypeData); // This will cause InvalidCastException

            var mockReferenceData = new List<ReferenceDataDto>
            {
                new() { Id = "eq1", Value = "Barbell", Description = "Barbell equipment" }
            };

            var mockHandler = new MockHttpMessageHandler();
            mockHandler.SetupResponse(HttpStatusCode.OK, mockReferenceData);

            var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("http://localhost:5214") };
            var referenceDataService = new ReferenceDataService(httpClient, _cache, _configuration);

            // Act
            var result = await referenceDataService.GetEquipmentAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().Value.Should().Be("Barbell");

            // Verify cache was cleared and repopulated with correct type
            _cache.TryGetValue("RefData_Equipment", out object? cachedData).Should().BeTrue();
            cachedData.Should().BeOfType<List<ReferenceDataDto>>();
        }

        [Fact]
        public async Task AllReferenceDataMethods_ShouldUseDifferentCacheKeys()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            var endpoints = new Dictionary<string, string>
            {
                { "/api/ReferenceTables/BodyParts", "RefData_BodyParts" },
                { "/api/ReferenceTables/DifficultyLevels", "RefData_DifficultyLevels" },
                { "/api/ReferenceTables/Equipment", "RefData_Equipment" },
                { "/api/ReferenceTables/MuscleGroups", "RefData_MuscleGroups" },
                { "/api/ReferenceTables/MuscleRoles", "RefData_MuscleRoles" },
                { "/api/ReferenceTables/MovementPatterns", "RefData_MovementPatterns" },
                { "/api/ReferenceTables/ExerciseTypes", "RefData_ExerciseTypes" }
            };

            // Set up responses in the order they will be called
            // BodyParts
            mockHandler.SetupResponse(HttpStatusCode.OK, new List<ReferenceDataDto>
            {
                new() { Id = "test", Value = "BodyParts", Description = "Test" }
            });

            // DifficultyLevels
            mockHandler.SetupResponse(HttpStatusCode.OK, new List<ReferenceDataDto>
            {
                new() { Id = "test", Value = "DifficultyLevels", Description = "Test" }
            });

            // Equipment
            mockHandler.SetupResponse(HttpStatusCode.OK, new List<ReferenceDataDto>
            {
                new() { Id = "test", Value = "Equipment", Description = "Test" }
            });

            // MuscleRoles
            mockHandler.SetupResponse(HttpStatusCode.OK, new List<ReferenceDataDto>
            {
                new() { Id = "test", Value = "MuscleRoles", Description = "Test" }
            });

            // MovementPatterns
            mockHandler.SetupResponse(HttpStatusCode.OK, new List<ReferenceDataDto>
            {
                new() { Id = "test", Value = "MovementPatterns", Description = "Test" }
            });

            // ExerciseTypes - API returns ReferenceDataDto, not ExerciseTypeDto
            mockHandler.SetupResponse(HttpStatusCode.OK, new List<ReferenceDataDto>
            {
                new() { Id = "test", Value = "Test Type", Description = "Test" }
            });

            // MuscleGroups - returns MuscleGroupDto
            mockHandler.SetupResponse(HttpStatusCode.OK, new List<MuscleGroupDto>
            {
                new() { Id = "test", Name = "Test Muscle", BodyPartId = "bp1", IsActive = true, CreatedAt = DateTime.UtcNow }
            });

            var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("http://localhost:5214") };
            var service = new ReferenceDataService(httpClient, _cache, _configuration);

            // Act
            await service.GetBodyPartsAsync();
            await service.GetDifficultyLevelsAsync();
            await service.GetEquipmentAsync();
            await service.GetMuscleRolesAsync();
            await service.GetMovementPatternsAsync();
            await service.GetExerciseTypesAsync();
            await service.GetMuscleGroupsAsync(); // Do this last as it expects different response

            // Assert - verify each method used its unique cache key
            foreach (var expectedCacheKey in endpoints.Values)
            {
                _cache.TryGetValue(expectedCacheKey, out object? _).Should().BeTrue(
                    $"Cache should contain key: {expectedCacheKey}");
            }
        }
    }
}