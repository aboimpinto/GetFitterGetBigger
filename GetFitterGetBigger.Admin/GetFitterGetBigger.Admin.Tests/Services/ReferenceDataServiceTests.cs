using System.Net;
using FluentAssertions;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Models.ReferenceData;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Services.Strategies;
using GetFitterGetBigger.Admin.Services.Strategies.ReferenceTableStrategies;
using GetFitterGetBigger.Admin.Tests.Builders;
using GetFitterGetBigger.Admin.Tests.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace GetFitterGetBigger.Admin.Tests.Services
{
    public class ReferenceDataServiceTests
    {
        private readonly MockHttpMessageHandler _httpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;
        private readonly Mock<ILogger<ReferenceDataService>> _loggerMock;
        private readonly List<IReferenceTableStrategy> _strategies;
        private readonly ReferenceDataService _referenceDataService;

        public ReferenceDataServiceTests()
        {
            _httpMessageHandler = new MockHttpMessageHandler();
            _httpClient = new HttpClient(_httpMessageHandler)
            {
                BaseAddress = new Uri("http://localhost:5214")
            };
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _loggerMock = new Mock<ILogger<ReferenceDataService>>();
            
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

            _referenceDataService = new ReferenceDataService(
                _strategies,
                _httpClient,
                _memoryCache,
                _loggerMock.Object);
        }

        [Fact]
        public async Task GetReferenceDataAsync_BodyParts_WhenNotCached_FetchesFromApiAndCaches()
        {
            // Arrange
            var expectedData = ReferenceDataDtoBuilder.BuildList(3);
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedData);

            // Act
            var result = await _referenceDataService.GetReferenceDataAsync<BodyParts>();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.First().Id.Should().Be("ref-1");

            // Verify API was called
            _httpMessageHandler.VerifyRequest(req =>
                req.Method == HttpMethod.Get &&
                req.RequestUri!.ToString().Contains("api/ReferenceTables/BodyParts"));
        }

        [Fact]
        public async Task GetReferenceDataAsync_DifficultyLevels_ReturnsCorrectData()
        {
            // Arrange
            var expectedData = ReferenceDataDtoBuilder.BuildList(5);
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedData);

            // Act
            var result = await _referenceDataService.GetReferenceDataAsync<DifficultyLevels>();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(5);

            // Verify correct endpoint was called
            _httpMessageHandler.VerifyRequest(req =>
                req.Method == HttpMethod.Get &&
                req.RequestUri!.ToString().Contains("api/ReferenceTables/DifficultyLevels"));
        }

        [Fact]
        public async Task GetReferenceDataAsync_Equipment_UsesCachedDataOnSecondCall()
        {
            // Arrange
            var expectedData = ReferenceDataDtoBuilder.BuildList(2);
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedData);

            // Act - First call
            var firstResult = await _referenceDataService.GetReferenceDataAsync<Equipment>();
            
            // Act - Second call
            var secondResult = await _referenceDataService.GetReferenceDataAsync<Equipment>();

            // Assert
            firstResult.Should().BeEquivalentTo(secondResult);
            
            // Verify API was called only once
            _httpMessageHandler.Requests.Should().HaveCount(1);
        }

        [Theory]
        [InlineData(typeof(KineticChainTypes), "api/ReferenceTables/KineticChainTypes")]
        [InlineData(typeof(MetricTypes), "api/ReferenceTables/MetricTypes")]
        [InlineData(typeof(MovementPatterns), "api/ReferenceTables/MovementPatterns")]
        [InlineData(typeof(MuscleRoles), "api/ReferenceTables/MuscleRoles")]
        public async Task GetReferenceDataAsync_VariousTypes_UseCorrectEndpoints(Type entityType, string expectedEndpoint)
        {
            // Arrange
            var expectedData = ReferenceDataDtoBuilder.BuildList(1);
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedData);
            
            // Create generic method info
            var method = typeof(ReferenceDataService)
                .GetMethod(nameof(ReferenceDataService.GetReferenceDataAsync))!
                .MakeGenericMethod(entityType);

            // Act
            var task = (Task<IEnumerable<ReferenceDataDto>>)method.Invoke(_referenceDataService, null)!;
            var result = await task;

            // Assert
            result.Should().NotBeNull();
            _httpMessageHandler.VerifyRequest(req =>
                req.RequestUri!.ToString().Contains(expectedEndpoint));
        }

        [Fact]
        public async Task GetReferenceDataAsync_WhenApiReturnsError_ReturnsEmptyCollection()
        {
            // Arrange
            _httpMessageHandler.SetupResponse(HttpStatusCode.InternalServerError, "Server error");

            // Act
            var result = await _referenceDataService.GetReferenceDataAsync<BodyParts>();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetReferenceDataAsync_WhenNoStrategyRegistered_ReturnsEmptyCollection()
        {
            // Arrange - Create service without strategies
            var service = new ReferenceDataService(
                new List<IReferenceTableStrategy>(), // No strategies
                _httpClient,
                _memoryCache,
                _loggerMock.Object);

            // Act
            var result = await service.GetReferenceDataAsync<BodyParts>();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            
            // Verify error was logged
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No strategy registered")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetReferenceDataAsync_MuscleGroups_TransformsDataCorrectly()
        {
            // Arrange
            var muscleGroupsData = new List<MuscleGroupDto>
            {
                new MuscleGroupDto
                {
                    Id = "mg-1",
                    Name = "Biceps",
                    BodyPartId = "bp-1",
                    BodyPartName = "Arms",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, muscleGroupsData);

            // Act
            var result = await _referenceDataService.GetReferenceDataAsync<MuscleGroups>();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            var item = result.First();
            item.Id.Should().Be("mg-1");
            item.Value.Should().Be("Biceps");
            item.Description.Should().Be("Body Part: Arms");
        }

        [Fact]
        public void Constructor_LogsStrategyCount()
        {
            // Arrange
            var freshLoggerMock = new Mock<ILogger<ReferenceDataService>>();
            
            // Act
            var service = new ReferenceDataService(
                _strategies,
                _httpClient,
                _memoryCache,
                freshLoggerMock.Object);

            // Assert
            freshLoggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"initialized with {_strategies.Count} strategies")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}