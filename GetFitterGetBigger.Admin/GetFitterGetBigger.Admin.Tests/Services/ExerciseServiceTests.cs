using System.Net;
using System.Text.Json;
using FluentAssertions;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Builders;
using GetFitterGetBigger.Admin.Tests.Helpers;
using Microsoft.Extensions.Caching.Memory;

namespace GetFitterGetBigger.Admin.Tests.Services
{
    public class ExerciseServiceTests
    {
        private readonly MockHttpMessageHandler _httpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;
        private readonly ExerciseService _exerciseService;

        public ExerciseServiceTests()
        {
            _httpMessageHandler = new MockHttpMessageHandler();
            _httpClient = new HttpClient(_httpMessageHandler)
            {
                BaseAddress = new Uri("http://localhost:5214")
            };
            _memoryCache = new MemoryCache(new MemoryCacheOptions());

            _exerciseService = new ExerciseService(
                _httpClient,
                _memoryCache);
        }

        [Fact]
        public async Task GetExercisesAsync_WithFilter_ReturnsPagedResult()
        {
            // Arrange
            var filter = new ExerciseFilterDtoBuilder()
                .WithName("squat")
                .WithPageSize(10)
                .Build();

            var expectedResult = new ExercisePagedResultDtoBuilder()
                .WithItems(
                    new ExerciseListDtoBuilder()
                        .WithName("Back Squat")
                        .WithDifficulty("Intermediate", "1")
                        .Build(),
                    new ExerciseListDtoBuilder()
                        .WithName("Front Squat")
                        .WithDifficulty("Advanced", "2")
                        .Build()
                )
                .WithPageNumber(1)
                .WithPageSize(10)
                .Build();

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
            var exerciseId = Guid.NewGuid().ToString();
            var expectedExercise = new ExerciseDtoBuilder()
                .WithId(exerciseId)
                .WithName("Bench Press")
                .WithDescription("A compound pushing exercise")
                .WithInstructions("Lie on bench...")
                .Build();

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
            var exerciseId = Guid.NewGuid().ToString();
            var cachedExercise = new ExerciseDtoBuilder()
                .WithId(exerciseId)
                .WithName("Cached Exercise")
                .WithDescription(string.Empty)
                .WithInstructions(string.Empty)
                .Build();

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
            var createDto = new ExerciseCreateDtoBuilder()
                .WithName("New Exercise")
                .WithDescription("Test description")
                .WithInstructions("Test instructions")
                .WithDifficultyId(Guid.NewGuid().ToString())
                .Build();

            var createdExercise = new ExerciseDtoBuilder()
                .WithName(createDto.Name)
                .WithDescription(createDto.Description)
                .WithInstructions(createDto.Instructions)
                .Build();

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
            var exerciseId = Guid.NewGuid().ToString();
            var updateDto = new ExerciseUpdateDtoBuilder()
                .WithName("Updated Exercise")
                .WithDescription("Updated description")
                .WithDifficultyId(Guid.NewGuid().ToString())
                .Build();

            // Cache an old version
            _memoryCache.Set($"exercise_{exerciseId}", new ExerciseDtoBuilder()
                .WithId(exerciseId)
                .WithName("Old Name")
                .WithDescription(string.Empty)
                .WithInstructions(string.Empty)
                .Build());

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
            var exerciseId = Guid.NewGuid().ToString();

            // Cache the exercise
            _memoryCache.Set($"exercise_{exerciseId}", new ExerciseDtoBuilder()
                .WithId(exerciseId)
                .WithName(string.Empty)
                .WithDescription(string.Empty)
                .WithInstructions(string.Empty)
                .Build());

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
            var muscleGroupId1 = Guid.NewGuid().ToString();
            var muscleGroupId2 = Guid.NewGuid().ToString();
            var equipmentId = Guid.NewGuid().ToString();

            var filter = new ExerciseFilterDtoBuilder()
                .WithName("press")
                .WithDifficultyId(Guid.NewGuid().ToString())
                .WithMuscleGroupIds(muscleGroupId1, muscleGroupId2)
                .WithEquipmentIds(equipmentId)
                .WithIsActive(true)
                .WithPage(2)
                .WithPageSize(20)
                .Build();

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
            var createDto = new ExerciseCreateDtoBuilder()
                .WithName("Test")
                .Build();
            _httpMessageHandler.SetupResponse(HttpStatusCode.BadRequest, new { error = "Validation failed" });

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(
                () => _exerciseService.CreateExerciseAsync(createDto));
        }

        [Fact]
        public async Task GetExerciseByIdAsync_WhenNotFound_ReturnsNull()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            _httpMessageHandler.SetupResponse(HttpStatusCode.NotFound, null);

            // Act
            var result = await _exerciseService.GetExerciseByIdAsync(exerciseId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetExercisesAsync_WithCoachNotesAndExerciseTypes_ReturnsCorrectData()
        {
            // Arrange
            var filter = new ExerciseFilterDtoBuilder().Build();

            var squatExercise = new ExerciseListDtoBuilder()
                .WithName("Squat")
                .WithDescription("Full depth squat")
                .WithCoachNotes(
                    ("Keep your back straight", 0),
                    ("Drive through your heels", 1),
                    ("Maintain knee alignment", 2)
                )
                .WithExerciseTypes(
                    ("Workout", "Main workout exercise"),
                    ("Warmup", "Can be used for warmup")
                )
                .WithDifficulty("Intermediate", "1")
                .Build();

            var expectedResult = new ExercisePagedResultDtoBuilder()
                .WithItems(squatExercise)
                .WithPagination(1, 10, 1)
                .Build();

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedResult);

            // Act
            var result = await _exerciseService.GetExercisesAsync(filter);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);

            var exercise = result.Items.First();
            exercise.CoachNotes.Should().HaveCount(3);
            exercise.CoachNotes[0].Text.Should().Be("Keep your back straight");
            exercise.CoachNotes[0].Order.Should().Be(0);
            exercise.CoachNotes[1].Text.Should().Be("Drive through your heels");
            exercise.CoachNotes[1].Order.Should().Be(1);
            exercise.CoachNotes[2].Text.Should().Be("Maintain knee alignment");
            exercise.CoachNotes[2].Order.Should().Be(2);

            exercise.ExerciseTypes.Should().HaveCount(2);
            exercise.ExerciseTypes.Should().Contain(et => et.Value == "Workout");
            exercise.ExerciseTypes.Should().Contain(et => et.Value == "Warmup");
        }

        [Fact]
        public async Task CreateExerciseAsync_WithCoachNotesAndExerciseTypes_SendsCorrectData()
        {
            // Arrange
            var createDto = new ExerciseCreateDtoBuilder()
                .WithName("Deadlift")
                .WithDescription("Conventional deadlift")
                .WithDifficultyId("2")
                .WithCoachNotes(
                    ("Set your feet hip-width apart", 0),
                    ("Engage your lats", 1),
                    ("Drive hips forward at the top", 2)
                )
                .WithExerciseTypes("type-workout", "type-cooldown")
                .Build();

            var expectedResponse = new ExerciseDtoBuilder()
                .WithId("exercise-123")
                .WithName("Deadlift")
                .WithDescription("Conventional deadlift")
                .WithCoachNotes(
                    ("Set your feet hip-width apart", 0),
                    ("Engage your lats", 1),
                    ("Drive hips forward at the top", 2)
                )
                .WithExerciseTypes(
                    ("Workout", "Main workout exercise"),
                    ("Cooldown", "Cooldown exercise")
                )
                .Build();

            _httpMessageHandler.SetupResponse(HttpStatusCode.Created, expectedResponse);

            // Act
            var result = await _exerciseService.CreateExerciseAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.CoachNotes.Should().HaveCount(3);
            result.ExerciseTypes.Should().HaveCount(2);

            _httpMessageHandler.VerifyRequest(request =>
            {
                var content = request.Content?.ReadAsStringAsync().Result;
                // The JSON serializer now uses camelCase as configured
                content.Should().Contain("coachNotes");
                content.Should().Contain("exerciseTypeIds");
                return true;
            });
        }

        [Fact]
        public async Task GetExerciseByIdAsync_WithCoachNotesAndExerciseTypes_ReturnsCachedData()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var expectedExercise = new ExerciseDtoBuilder()
                .WithId(exerciseId)
                .WithName("Bench Press")
                .WithCoachNotes(
                    ("Grip the bar slightly wider than shoulder-width", 0),
                    ("Lower the bar to chest level", 1)
                )
                .WithExerciseTypes(("Workout", "Main workout exercise"))
                .Build();

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedExercise);

            // Act - First call should hit the API
            var firstResult = await _exerciseService.GetExerciseByIdAsync(exerciseId);

            // Act - Second call should use cache
            var secondResult = await _exerciseService.GetExerciseByIdAsync(exerciseId);

            // Assert
            firstResult.Should().NotBeNull();
            firstResult!.CoachNotes.Should().HaveCount(2);
            firstResult.ExerciseTypes.Should().HaveCount(1);

            secondResult.Should().BeEquivalentTo(firstResult);

            // Verify API was only called once
            _httpMessageHandler.Requests.Should().HaveCount(1);
        }
    }
}