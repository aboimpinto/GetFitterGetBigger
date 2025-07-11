using System.Net;
using FluentAssertions;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Tests.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;

namespace GetFitterGetBigger.Admin.Tests.Services
{
    public class ExerciseWeightTypeServiceTests
    {
        private readonly MockHttpMessageHandler _httpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly ExerciseWeightTypeService _exerciseWeightTypeService;

        public ExerciseWeightTypeServiceTests()
        {
            _httpMessageHandler = new MockHttpMessageHandler();
            _httpClient = new HttpClient(_httpMessageHandler);
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _configurationMock = new Mock<IConfiguration>();

            _configurationMock
                .Setup(x => x["ApiBaseUrl"])
                .Returns("http://localhost:5214");

            _exerciseWeightTypeService = new ExerciseWeightTypeService(
                _httpClient,
                _memoryCache,
                _configurationMock.Object);
        }

        [Fact]
        public async Task GetWeightTypesAsync_FirstCall_ReturnsWeightTypesFromApi()
        {
            // Arrange
            var expectedWeightTypes = new List<ExerciseWeightTypeDto>
            {
                new ExerciseWeightTypeDto
                {
                    Id = Guid.NewGuid(),
                    Code = "BODYWEIGHT_ONLY",
                    Name = "Bodyweight Only",
                    Description = "Exercise uses bodyweight only",
                    IsActive = true,
                    DisplayOrder = 1
                },
                new ExerciseWeightTypeDto
                {
                    Id = Guid.NewGuid(),
                    Code = "WEIGHT_REQUIRED",
                    Name = "Weight Required",
                    Description = "Exercise requires external weight",
                    IsActive = true,
                    DisplayOrder = 2
                }
            };

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedWeightTypes);

            // Act
            var result = await _exerciseWeightTypeService.GetWeightTypesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(wt => wt.Code == "BODYWEIGHT_ONLY");
            result.Should().Contain(wt => wt.Code == "WEIGHT_REQUIRED");
        }

        [Fact]
        public async Task GetWeightTypesAsync_SecondCall_ReturnsCachedWeightTypes()
        {
            // Arrange
            var expectedWeightTypes = new List<ExerciseWeightTypeDto>
            {
                new ExerciseWeightTypeDto
                {
                    Id = Guid.NewGuid(),
                    Code = "BODYWEIGHT_ONLY",
                    Name = "Bodyweight Only",
                    Description = "Exercise uses bodyweight only",
                    IsActive = true,
                    DisplayOrder = 1
                }
            };

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedWeightTypes);

            // Act - First call
            var firstResult = await _exerciseWeightTypeService.GetWeightTypesAsync();

            // Act - Second call (should use cache, no additional HTTP request)
            var secondResult = await _exerciseWeightTypeService.GetWeightTypesAsync();

            // Assert
            firstResult.Should().BeEquivalentTo(secondResult);
            _httpMessageHandler.Requests.Should().HaveCount(1, "Second call should use cache");
        }

        [Fact]
        public async Task GetWeightTypesAsync_ApiReturnsNotFound_ThrowsHttpRequestException()
        {
            // Arrange
            _httpMessageHandler.SetupResponse(HttpStatusCode.NotFound);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(
                () => _exerciseWeightTypeService.GetWeightTypesAsync());
        }

        [Fact]
        public async Task GetWeightTypesAsync_ApiReturnsServerError_ThrowsHttpRequestException()
        {
            // Arrange
            _httpMessageHandler.SetupResponse(HttpStatusCode.InternalServerError);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(
                () => _exerciseWeightTypeService.GetWeightTypesAsync());
        }

        [Fact]
        public async Task GetWeightTypeByIdAsync_ValidId_ReturnsWeightType()
        {
            // Arrange
            var weightTypeId = Guid.NewGuid();
            var expectedWeightType = new ExerciseWeightTypeDto
            {
                Id = weightTypeId,
                Code = "MACHINE_WEIGHT",
                Name = "Machine Weight",
                Description = "Exercise uses machine weight",
                IsActive = true,
                DisplayOrder = 5
            };

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedWeightType);

            // Act
            var result = await _exerciseWeightTypeService.GetWeightTypeByIdAsync(weightTypeId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(weightTypeId);
            result.Code.Should().Be("MACHINE_WEIGHT");
            result.Name.Should().Be("Machine Weight");
        }

        [Fact]
        public async Task GetWeightTypeByIdAsync_NotFound_ReturnsNull()
        {
            // Arrange
            var weightTypeId = Guid.NewGuid();
            _httpMessageHandler.SetupResponse(HttpStatusCode.NotFound);

            // Act
            var result = await _exerciseWeightTypeService.GetWeightTypeByIdAsync(weightTypeId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetWeightTypeByIdAsync_ServerError_ThrowsHttpRequestException()
        {
            // Arrange
            var weightTypeId = Guid.NewGuid();
            _httpMessageHandler.SetupResponse(HttpStatusCode.InternalServerError);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(
                () => _exerciseWeightTypeService.GetWeightTypeByIdAsync(weightTypeId));
        }

        [Fact]
        public async Task GetWeightTypesAsync_WithCorrectUrl_CallsExpectedEndpoint()
        {
            // Arrange
            var expectedWeightTypes = new List<ExerciseWeightTypeDto>();
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedWeightTypes);

            // Act
            await _exerciseWeightTypeService.GetWeightTypesAsync();

            // Assert
            _httpMessageHandler.VerifyRequest(req => 
                req.Method == HttpMethod.Get && 
                req.RequestUri!.ToString() == "http://localhost:5214/api/ReferenceTables/ExerciseWeightTypes");
        }

        [Fact]
        public async Task GetWeightTypeByIdAsync_WithCorrectUrl_CallsExpectedEndpoint()
        {
            // Arrange
            var weightTypeId = Guid.NewGuid();
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, new ExerciseWeightTypeDto
            {
                Id = weightTypeId,
                Code = "TEST",
                Name = "Test",
                IsActive = true,
                DisplayOrder = 1
            });

            // Act
            await _exerciseWeightTypeService.GetWeightTypeByIdAsync(weightTypeId);

            // Assert
            _httpMessageHandler.VerifyRequest(req => 
                req.Method == HttpMethod.Get && 
                req.RequestUri!.ToString() == $"http://localhost:5214/api/ReferenceTables/ExerciseWeightTypes/{weightTypeId}");
        }

        [Fact]
        public async Task GetWeightTypesAsync_EmptyApiResponse_ReturnsEmptyCollection()
        {
            // Arrange
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, new List<ExerciseWeightTypeDto>());

            // Act
            var result = await _exerciseWeightTypeService.GetWeightTypesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }
}