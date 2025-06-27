using System.Net;
using System.Text.Json;
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
    public class ExerciseServiceTests
    {
        private readonly MockHttpMessageHandler _httpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly ExerciseService _exerciseService;

        public ExerciseServiceTests()
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

            _exerciseService = new ExerciseService(
                _httpClient,
                _memoryCache,
                _configurationMock.Object);
        }

        [Fact]
        public async Task GetExercisesAsync_WithFilter_ReturnsPagedResult()
        {
            // Arrange
            var filter = new ExerciseFilterDto
            {
                Name = "squat",
                Page = 1,
                PageSize = 10
            };

            var expectedResult = new ExercisePagedResultDto
            {
                Items = new List<ExerciseListDto>
                {
                    new() { Id = Guid.NewGuid(), Name = "Back Squat", DifficultyName = "Intermediate" },
                    new() { Id = Guid.NewGuid(), Name = "Front Squat", DifficultyName = "Advanced" }
                },
                TotalCount = 2,
                PageNumber = 1,
                PageSize = 10
            };

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedResult);

            // Act
            var result = await _exerciseService.GetExercisesAsync(filter);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            _httpMessageHandler.VerifyRequest(request =>
                request.RequestUri!.PathAndQuery.Contains("name=squat"));
        }

        [Fact]
        public async Task GetExerciseByIdAsync_WhenNotCached_FetchesFromApiAndCaches()
        {
            // Arrange
            var exerciseId = Guid.NewGuid();
            var expectedExercise = new ExerciseDto
            {
                Id = exerciseId,
                Name = "Bench Press",
                Description = "A compound pushing exercise",
                Instructions = "Lie on bench...",
                IsUnilateral = false
            };

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedExercise);

            // Act
            var result = await _exerciseService.GetExerciseByIdAsync(exerciseId);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Bench Press");
            
            // Verify it's cached
            _memoryCache.TryGetValue($"exercise_{exerciseId}", out ExerciseDto? cached).Should().BeTrue();
            cached.Should().NotBeNull();
        }

        [Fact]
        public async Task GetExerciseByIdAsync_WhenCached_ReturnsCachedValue()
        {
            // Arrange
            var exerciseId = Guid.NewGuid();
            var cachedExercise = new ExerciseDto
            {
                Id = exerciseId,
                Name = "Cached Exercise"
            };
            
            _memoryCache.Set($"exercise_{exerciseId}", cachedExercise);

            // Act
            var result = await _exerciseService.GetExerciseByIdAsync(exerciseId);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Cached Exercise");
            _httpMessageHandler.VerifyNoRequests();
        }

        [Fact]
        public async Task CreateExerciseAsync_WithValidData_ReturnsCreatedExercise()
        {
            // Arrange
            var createDto = new ExerciseCreateDto
            {
                Name = "New Exercise",
                Description = "Test description",
                Instructions = "Test instructions",
                DifficultyId = Guid.NewGuid(),
                IsUnilateral = false
            };

            var createdExercise = new ExerciseDto
            {
                Id = Guid.NewGuid(),
                Name = createDto.Name,
                Description = createDto.Description,
                Instructions = createDto.Instructions
            };

            _httpMessageHandler.SetupResponse(HttpStatusCode.Created, createdExercise);

            // Act
            var result = await _exerciseService.CreateExerciseAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("New Exercise");
            _httpMessageHandler.VerifyRequest(request =>
            {
                request.Method.Should().Be(HttpMethod.Post);
                request.RequestUri!.PathAndQuery.Should().Be("/api/exercises");
                return true;
            });
        }

        [Fact]
        public async Task UpdateExerciseAsync_WithValidData_UpdatesAndClearsCache()
        {
            // Arrange
            var exerciseId = Guid.NewGuid();
            var updateDto = new ExerciseUpdateDto
            {
                Name = "Updated Exercise",
                Description = "Updated description",
                Instructions = "Updated instructions",
                DifficultyId = Guid.NewGuid()
            };

            // Cache an old version
            _memoryCache.Set($"exercise_{exerciseId}", new ExerciseDto { Id = exerciseId, Name = "Old Name" });

            _httpMessageHandler.SetupResponse(HttpStatusCode.NoContent, null);

            // Act
            await _exerciseService.UpdateExerciseAsync(exerciseId, updateDto);

            // Assert
            _httpMessageHandler.VerifyRequest(request =>
            {
                request.Method.Should().Be(HttpMethod.Put);
                request.RequestUri!.PathAndQuery.Should().Be($"/api/exercises/{exerciseId}");
                return true;
            });

            // Verify cache is cleared
            _memoryCache.TryGetValue($"exercise_{exerciseId}", out ExerciseDto? _).Should().BeFalse();
        }

        [Fact]
        public async Task DeleteExerciseAsync_WithValidId_DeletesAndClearsCache()
        {
            // Arrange
            var exerciseId = Guid.NewGuid();
            
            // Cache the exercise
            _memoryCache.Set($"exercise_{exerciseId}", new ExerciseDto { Id = exerciseId });

            _httpMessageHandler.SetupResponse(HttpStatusCode.NoContent, null);

            // Act
            await _exerciseService.DeleteExerciseAsync(exerciseId);

            // Assert
            _httpMessageHandler.VerifyRequest(request =>
            {
                request.Method.Should().Be(HttpMethod.Delete);
                request.RequestUri!.PathAndQuery.Should().Be($"/api/exercises/{exerciseId}");
                return true;
            });

            // Verify cache is cleared
            _memoryCache.TryGetValue($"exercise_{exerciseId}", out ExerciseDto? _).Should().BeFalse();
        }

        [Fact]
        public async Task GetExercisesAsync_WithComplexFilter_BuildsCorrectQueryString()
        {
            // Arrange
            var muscleGroupId1 = Guid.NewGuid();
            var muscleGroupId2 = Guid.NewGuid();
            var equipmentId = Guid.NewGuid();
            
            var filter = new ExerciseFilterDto
            {
                Name = "press",
                DifficultyId = Guid.NewGuid(),
                MuscleGroupIds = new List<Guid> { muscleGroupId1, muscleGroupId2 },
                EquipmentIds = new List<Guid> { equipmentId },
                IsActive = true,
                Page = 2,
                PageSize = 20
            };

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, new ExercisePagedResultDto());

            // Act
            await _exerciseService.GetExercisesAsync(filter);

            // Assert
            _httpMessageHandler.VerifyRequest(request =>
            {
                var query = request.RequestUri!.Query;
                query.Should().Contain("name=press");
                query.Should().Contain($"difficultyId={filter.DifficultyId}");
                query.Should().Contain($"muscleGroupIds={muscleGroupId1}");
                query.Should().Contain($"muscleGroupIds={muscleGroupId2}");
                query.Should().Contain($"equipmentIds={equipmentId}");
                query.Should().Contain("isActive=True");
                query.Should().Contain("page=2");
                query.Should().Contain("pageSize=20");
                return true;
            });
        }

        [Fact]
        public async Task CreateExerciseAsync_WhenApiFails_ThrowsHttpRequestException()
        {
            // Arrange
            var createDto = new ExerciseCreateDto { Name = "Test" };
            _httpMessageHandler.SetupResponse(HttpStatusCode.BadRequest, new { error = "Validation failed" });

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(
                () => _exerciseService.CreateExerciseAsync(createDto));
        }

        [Fact]
        public async Task GetExerciseByIdAsync_WhenNotFound_ReturnsNull()
        {
            // Arrange
            var exerciseId = Guid.NewGuid();
            _httpMessageHandler.SetupResponse(HttpStatusCode.NotFound, null);

            // Act
            var result = await _exerciseService.GetExerciseByIdAsync(exerciseId);

            // Assert
            result.Should().BeNull();
        }
    }
}