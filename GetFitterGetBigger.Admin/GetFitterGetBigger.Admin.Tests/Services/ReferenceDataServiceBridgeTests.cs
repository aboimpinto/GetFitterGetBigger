using FluentAssertions;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Models.ReferenceData;
using GetFitterGetBigger.Admin.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace GetFitterGetBigger.Admin.Tests.Services
{
    public class ReferenceDataServiceBridgeTests
    {
        private readonly Mock<IGenericReferenceDataService> _genericServiceMock;
        private readonly IMemoryCache _memoryCache;
        private readonly Mock<ILogger<ReferenceDataServiceBridge>> _loggerMock;
        private readonly ReferenceDataServiceBridge _bridge;

        public ReferenceDataServiceBridgeTests()
        {
            _genericServiceMock = new Mock<IGenericReferenceDataService>();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _loggerMock = new Mock<ILogger<ReferenceDataServiceBridge>>();
            
            _bridge = new ReferenceDataServiceBridge(
                _genericServiceMock.Object,
                _memoryCache,
                _loggerMock.Object);
        }

        [Fact]
        public async Task GetBodyPartsAsync_CallsGenericServiceWithCorrectType()
        {
            // Arrange
            var expectedData = new List<ReferenceDataDto>
            {
                new() { Id = "1", Value = "Arms", Description = "Upper body" }
            };
            _genericServiceMock.Setup(x => x.GetReferenceDataAsync<BodyParts>())
                .ReturnsAsync(expectedData);

            // Act
            var result = await _bridge.GetBodyPartsAsync();

            // Assert
            result.Should().BeEquivalentTo(expectedData);
            _genericServiceMock.Verify(x => x.GetReferenceDataAsync<BodyParts>(), Times.Once);
        }

        [Fact]
        public async Task GetDifficultyLevelsAsync_CallsGenericServiceWithCorrectType()
        {
            // Arrange
            var expectedData = new List<ReferenceDataDto>
            {
                new() { Id = "1", Value = "Beginner" }
            };
            _genericServiceMock.Setup(x => x.GetReferenceDataAsync<DifficultyLevels>())
                .ReturnsAsync(expectedData);

            // Act
            var result = await _bridge.GetDifficultyLevelsAsync();

            // Assert
            result.Should().BeEquivalentTo(expectedData);
            _genericServiceMock.Verify(x => x.GetReferenceDataAsync<DifficultyLevels>(), Times.Once);
        }

        [Fact]
        public async Task GetExerciseTypesAsync_TransformsReferenceDataToExerciseTypeDto()
        {
            // Arrange
            var referenceData = new List<ReferenceDataDto>
            {
                new() { Id = "1", Value = "Warmup", Description = "Warmup exercises" },
                new() { Id = "2", Value = "Workout", Description = "Main workout" }
            };
            _genericServiceMock.Setup(x => x.GetReferenceDataAsync<ExerciseTypes>())
                .ReturnsAsync(referenceData);

            // Act
            var result = await _bridge.GetExerciseTypesAsync();

            // Assert
            result.Should().HaveCount(2);
            var exerciseTypes = result.ToList();
            exerciseTypes[0].Id.Should().Be("1");
            exerciseTypes[0].Value.Should().Be("Warmup");
            exerciseTypes[0].Description.Should().Be("Warmup exercises");
        }

        [Fact]
        public async Task GetEquipmentAsync_CallsGenericServiceWithCorrectType()
        {
            // Arrange
            var expectedData = new List<ReferenceDataDto> { new() { Id = "1", Value = "Barbell" } };
            _genericServiceMock.Setup(x => x.GetReferenceDataAsync<Equipment>()).ReturnsAsync(expectedData);

            // Act
            var result = await _bridge.GetEquipmentAsync();

            // Assert
            result.Should().BeEquivalentTo(expectedData);
            _genericServiceMock.Verify(x => x.GetReferenceDataAsync<Equipment>(), Times.Once);
        }

        [Fact]
        public async Task GetKineticChainTypesAsync_CallsGenericServiceWithCorrectType()
        {
            // Arrange
            var expectedData = new List<ReferenceDataDto> { new() { Id = "1", Value = "Open Chain" } };
            _genericServiceMock.Setup(x => x.GetReferenceDataAsync<KineticChainTypes>()).ReturnsAsync(expectedData);

            // Act
            var result = await _bridge.GetKineticChainTypesAsync();

            // Assert
            result.Should().BeEquivalentTo(expectedData);
            _genericServiceMock.Verify(x => x.GetReferenceDataAsync<KineticChainTypes>(), Times.Once);
        }

        [Fact]
        public async Task GetMetricTypesAsync_CallsGenericServiceWithCorrectType()
        {
            // Arrange
            var expectedData = new List<ReferenceDataDto> { new() { Id = "1", Value = "Reps" } };
            _genericServiceMock.Setup(x => x.GetReferenceDataAsync<MetricTypes>()).ReturnsAsync(expectedData);

            // Act
            var result = await _bridge.GetMetricTypesAsync();

            // Assert
            result.Should().BeEquivalentTo(expectedData);
            _genericServiceMock.Verify(x => x.GetReferenceDataAsync<MetricTypes>(), Times.Once);
        }

        [Fact]
        public async Task GetMovementPatternsAsync_CallsGenericServiceWithCorrectType()
        {
            // Arrange
            var expectedData = new List<ReferenceDataDto> { new() { Id = "1", Value = "Push" } };
            _genericServiceMock.Setup(x => x.GetReferenceDataAsync<MovementPatterns>()).ReturnsAsync(expectedData);

            // Act
            var result = await _bridge.GetMovementPatternsAsync();

            // Assert
            result.Should().BeEquivalentTo(expectedData);
            _genericServiceMock.Verify(x => x.GetReferenceDataAsync<MovementPatterns>(), Times.Once);
        }

        [Fact]
        public async Task GetMuscleGroupsAsync_CallsGenericServiceWithCorrectType()
        {
            // Arrange
            var expectedData = new List<ReferenceDataDto> { new() { Id = "1", Value = "Biceps" } };
            _genericServiceMock.Setup(x => x.GetReferenceDataAsync<MuscleGroups>()).ReturnsAsync(expectedData);

            // Act
            var result = await _bridge.GetMuscleGroupsAsync();

            // Assert
            result.Should().BeEquivalentTo(expectedData);
            _genericServiceMock.Verify(x => x.GetReferenceDataAsync<MuscleGroups>(), Times.Once);
        }

        [Fact]
        public async Task GetMuscleRolesAsync_CallsGenericServiceWithCorrectType()
        {
            // Arrange
            var expectedData = new List<ReferenceDataDto> { new() { Id = "1", Value = "Primary" } };
            _genericServiceMock.Setup(x => x.GetReferenceDataAsync<MuscleRoles>()).ReturnsAsync(expectedData);

            // Act
            var result = await _bridge.GetMuscleRolesAsync();

            // Assert
            result.Should().BeEquivalentTo(expectedData);
            _genericServiceMock.Verify(x => x.GetReferenceDataAsync<MuscleRoles>(), Times.Once);
        }

        [Fact]
        public void ClearEquipmentCache_RemovesCorrectCacheKey()
        {
            // Arrange
            _memoryCache.Set("RefData_Equipment", new List<ReferenceDataDto>());

            // Act
            _bridge.ClearEquipmentCache();

            // Assert
            _memoryCache.TryGetValue("RefData_Equipment", out object? _).Should().BeFalse();
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cleared cache: RefData_Equipment")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void ClearMuscleGroupsCache_RemovesAllRelatedCacheKeys()
        {
            // Arrange
            _memoryCache.Set("RefData_MuscleGroups", new List<ReferenceDataDto>());
            _memoryCache.Set("MuscleGroupsDto_Full", new List<MuscleGroupDto>());
            _memoryCache.Set("MuscleGroups", new List<MuscleGroupDto>());

            // Act
            _bridge.ClearMuscleGroupsCache();

            // Assert
            _memoryCache.TryGetValue("RefData_MuscleGroups", out object? _).Should().BeFalse();
            _memoryCache.TryGetValue("MuscleGroupsDto_Full", out object? _).Should().BeFalse();
            _memoryCache.TryGetValue("MuscleGroups", out object? _).Should().BeFalse();
            
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cleared muscle groups related caches")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}