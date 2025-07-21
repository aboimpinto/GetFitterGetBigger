using System.Net;
using FluentAssertions;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Tests.Helpers;
using Microsoft.Extensions.Caching.Memory;

namespace GetFitterGetBigger.Admin.Tests.Services
{
    public class ExerciseWeightTypeServiceTests
    {
        private readonly MockHttpMessageHandler _httpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;
        private readonly ExerciseWeightTypeService _exerciseWeightTypeService;

        public ExerciseWeightTypeServiceTests()
        {
            _httpMessageHandler = new MockHttpMessageHandler();
            _httpClient = new HttpClient(_httpMessageHandler)
            {
                BaseAddress = new Uri("http://localhost:5214")
            };
            _memoryCache = new MemoryCache(new MemoryCacheOptions());

            _exerciseWeightTypeService = new ExerciseWeightTypeService(
                _httpClient,
                _memoryCache);
        }

        [Fact]
        public async Task GetWeightTypesAsync_MapsReferenceDataToExerciseWeightTypes()
        {
            // Arrange
            var referenceData = new List<ReferenceDataDto>
            {
                new ReferenceDataDto
                {
                    Id = "exerciseweighttype-a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a",
                    Value = "Bodyweight Only",
                    Description = "Exercises that cannot have external weight added"
                },
                new ReferenceDataDto
                {
                    Id = "exerciseweighttype-b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f",
                    Value = "Weight Required",
                    Description = "Exercises that must have external weight specified"
                }
            };

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, referenceData);

            // Act
            var result = await _exerciseWeightTypeService.GetWeightTypesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            var bodyweightOnly = result.First(wt => wt.Code == "BODYWEIGHT_ONLY");
            bodyweightOnly.Id.Should().Be(Guid.Parse("a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a"));
            bodyweightOnly.Name.Should().Be("Bodyweight Only");
            bodyweightOnly.Description.Should().Be("Exercises that cannot have external weight added");
            bodyweightOnly.IsActive.Should().BeTrue();
            bodyweightOnly.DisplayOrder.Should().Be(1);

            var weightRequired = result.First(wt => wt.Code == "WEIGHT_REQUIRED");
            weightRequired.Id.Should().Be(Guid.Parse("b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f"));
            weightRequired.Name.Should().Be("Weight Required");
            weightRequired.Description.Should().Be("Exercises that must have external weight specified");
            weightRequired.IsActive.Should().BeTrue();
            weightRequired.DisplayOrder.Should().Be(2);
        }

        [Fact]
        public async Task GetWeightTypesAsync_MapsAllFiveWeightTypes()
        {
            // Arrange
            var referenceData = new List<ReferenceDataDto>
            {
                new ReferenceDataDto
                {
                    Id = "exerciseweighttype-a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a",
                    Value = "Bodyweight Only",
                    Description = "Exercises that cannot have external weight added"
                },
                new ReferenceDataDto
                {
                    Id = "exerciseweighttype-b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f",
                    Value = "Bodyweight Optional",
                    Description = "Exercises that can be performed with or without additional weight"
                },
                new ReferenceDataDto
                {
                    Id = "exerciseweighttype-c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a",
                    Value = "Weight Required",
                    Description = "Exercises that must have external weight specified"
                },
                new ReferenceDataDto
                {
                    Id = "exerciseweighttype-d4c6b5a7-8c9d-7e0f-1a2b-3c4d5e6f7a8b",
                    Value = "Machine Weight",
                    Description = "Exercises performed on machines with weight stacks"
                },
                new ReferenceDataDto
                {
                    Id = "exerciseweighttype-e5b7a698-9d0e-8f1a-2b3c-4d5e6f7a8b9c",
                    Value = "No Weight",
                    Description = "Exercises that do not use weight as a metric"
                }
            };

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, referenceData);

            // Act
            var result = await _exerciseWeightTypeService.GetWeightTypesAsync();

            // Assert
            result.Should().HaveCount(5);
            result.Should().Contain(wt => wt.Code == "BODYWEIGHT_ONLY");
            result.Should().Contain(wt => wt.Code == "BODYWEIGHT_OPTIONAL");
            result.Should().Contain(wt => wt.Code == "WEIGHT_REQUIRED");
            result.Should().Contain(wt => wt.Code == "MACHINE_WEIGHT");
            result.Should().Contain(wt => wt.Code == "NO_WEIGHT");
        }

        [Fact]
        public async Task GetWeightTypeByIdAsync_ConvertsGuidToStringFormat()
        {
            // Arrange
            var id = Guid.Parse("a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a");
            var referenceData = new ReferenceDataDto
            {
                Id = "exerciseweighttype-a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a",
                Value = "Bodyweight Only",
                Description = "Exercises that cannot have external weight added"
            };

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, referenceData);

            // Act
            var result = await _exerciseWeightTypeService.GetWeightTypeByIdAsync(id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            result.Code.Should().Be("BODYWEIGHT_ONLY");
            result.Name.Should().Be("Bodyweight Only");
        }

        [Fact]
        public async Task GetWeightTypesAsync_CachesResults()
        {
            // Arrange
            var referenceData = new List<ReferenceDataDto>
            {
                new ReferenceDataDto
                {
                    Id = "exerciseweighttype-a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a",
                    Value = "Bodyweight Only",
                    Description = "Test"
                }
            };

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, referenceData);

            // Act - First call
            var result1 = await _exerciseWeightTypeService.GetWeightTypesAsync();
            // Second call should come from cache
            var result2 = await _exerciseWeightTypeService.GetWeightTypesAsync();

            // Assert
            result1.Should().BeEquivalentTo(result2);
            _httpMessageHandler.Requests.Count.Should().Be(1);
        }
    }
}