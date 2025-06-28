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
                    new() { 
                        Id = Guid.NewGuid().ToString(), 
                        Name = "Back Squat", 
                        Difficulty = new ReferenceDataDto { Id = "1", Value = "Intermediate", Description = "Intermediate level" },
                        MuscleGroups = new List<MuscleGroupListItemDto>(),
                        Equipment = new List<ReferenceDataDto>(),
                        MovementPatterns = new List<ReferenceDataDto>(),
                        BodyParts = new List<ReferenceDataDto>()
                    },
                    new() { 
                        Id = Guid.NewGuid().ToString(), 
                        Name = "Front Squat", 
                        Difficulty = new ReferenceDataDto { Id = "2", Value = "Advanced", Description = "Advanced level" },
                        MuscleGroups = new List<MuscleGroupListItemDto>(),
                        Equipment = new List<ReferenceDataDto>(),
                        MovementPatterns = new List<ReferenceDataDto>(),
                        BodyParts = new List<ReferenceDataDto>()
                    }
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
            var exerciseId = Guid.NewGuid().ToString();
            var expectedExercise = new ExerciseDto
            {
                Id = exerciseId,
                Name = "Bench Press",
                Description = "A compound pushing exercise",
                Instructions = "Lie on bench...",
                IsUnilateral = false,
                Difficulty = new ReferenceDataDto { Id = "1", Value = "Intermediate", Description = "Intermediate level" },
                MuscleGroups = new List<MuscleGroupWithRoleDto>(),
                Equipment = new List<ReferenceDataDto>(),
                BodyParts = new List<ReferenceDataDto>(),
                MovementPatterns = new List<ReferenceDataDto>()
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
            var exerciseId = Guid.NewGuid().ToString();
            var cachedExercise = new ExerciseDto
            {
                Id = exerciseId,
                Name = "Cached Exercise",
                Description = string.Empty,
                Instructions = string.Empty,
                Difficulty = new ReferenceDataDto { Id = "1", Value = "Intermediate", Description = "Intermediate level" },
                MuscleGroups = new List<MuscleGroupWithRoleDto>(),
                Equipment = new List<ReferenceDataDto>(),
                BodyParts = new List<ReferenceDataDto>(),
                MovementPatterns = new List<ReferenceDataDto>()
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
                DifficultyId = Guid.NewGuid().ToString(),
                IsUnilateral = false
            };

            var createdExercise = new ExerciseDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = createDto.Name,
                Description = createDto.Description,
                Instructions = createDto.Instructions,
                Difficulty = new ReferenceDataDto { Id = "1", Value = "Intermediate", Description = "Intermediate level" },
                MuscleGroups = new List<MuscleGroupWithRoleDto>(),
                Equipment = new List<ReferenceDataDto>(),
                BodyParts = new List<ReferenceDataDto>(),
                MovementPatterns = new List<ReferenceDataDto>()
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
            var exerciseId = Guid.NewGuid().ToString();
            var updateDto = new ExerciseUpdateDto
            {
                Name = "Updated Exercise",
                Description = "Updated description",
                Instructions = "Updated instructions",
                DifficultyId = Guid.NewGuid().ToString()
            };

            // Cache an old version
            _memoryCache.Set($"exercise_{exerciseId}", new ExerciseDto { 
                Id = exerciseId, 
                Name = "Old Name",
                Description = string.Empty,
                Instructions = string.Empty,
                Difficulty = new ReferenceDataDto { Id = "1", Value = "Intermediate", Description = "Intermediate level" },
                MuscleGroups = new List<MuscleGroupWithRoleDto>(),
                Equipment = new List<ReferenceDataDto>(),
                BodyParts = new List<ReferenceDataDto>(),
                MovementPatterns = new List<ReferenceDataDto>()
            });

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
            _memoryCache.Set($"exercise_{exerciseId}", new ExerciseDto { 
                Id = exerciseId,
                Name = string.Empty,
                Description = string.Empty,
                Instructions = string.Empty,
                Difficulty = new ReferenceDataDto { Id = "1", Value = "Intermediate", Description = "Intermediate level" },
                MuscleGroups = new List<MuscleGroupWithRoleDto>(),
                Equipment = new List<ReferenceDataDto>(),
                BodyParts = new List<ReferenceDataDto>(),
                MovementPatterns = new List<ReferenceDataDto>()
            });

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
            
            var filter = new ExerciseFilterDto
            {
                Name = "press",
                DifficultyId = Guid.NewGuid().ToString(),
                MuscleGroupIds = new List<string> { muscleGroupId1, muscleGroupId2 },
                EquipmentIds = new List<string> { equipmentId },
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
            var filter = new ExerciseFilterDto { Page = 1, PageSize = 10 };
            var expectedResult = new ExercisePagedResultDto
            {
                Items = new List<ExerciseListDto>
                {
                    new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Squat",
                        Description = "Full depth squat",
                        CoachNotes = new List<CoachNoteDto>
                        {
                            new() { Id = "note-1", Text = "Keep your back straight", Order = 0 },
                            new() { Id = "note-2", Text = "Drive through your heels", Order = 1 },
                            new() { Id = "note-3", Text = "Maintain knee alignment", Order = 2 }
                        },
                        ExerciseTypes = new List<ExerciseTypeDto>
                        {
                            new() { Id = "type-1", Value = "Workout", Description = "Main workout exercise" },
                            new() { Id = "type-2", Value = "Warmup", Description = "Can be used for warmup" }
                        },
                        Difficulty = new ReferenceDataDto { Id = "1", Value = "Intermediate", Description = "Intermediate level" },
                        MuscleGroups = new List<MuscleGroupListItemDto>(),
                        Equipment = new List<ReferenceDataDto>(),
                        MovementPatterns = new List<ReferenceDataDto>(),
                        BodyParts = new List<ReferenceDataDto>()
                    }
                },
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 10
            };

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
            var createDto = new ExerciseCreateDto
            {
                Name = "Deadlift",
                Description = "Conventional deadlift",
                DifficultyId = "2",
                CoachNotes = new List<CoachNoteCreateDto>
                {
                    new() { Text = "Set your feet hip-width apart", Order = 0 },
                    new() { Text = "Engage your lats", Order = 1 },
                    new() { Text = "Drive hips forward at the top", Order = 2 }
                },
                ExerciseTypeIds = new List<string> { "type-workout", "type-cooldown" },
                MuscleGroups = new List<MuscleGroupApiDto>(),
                EquipmentIds = new List<string>(),
                BodyPartIds = new List<string>(),
                MovementPatternIds = new List<string>()
            };

            var expectedResponse = new ExerciseDtoBuilder()
                .WithId("exercise-123")
                .WithName("Deadlift")
                .WithDescription("Conventional deadlift")
                .WithCoachNote("Set your feet hip-width apart", 0)
                .WithCoachNote("Engage your lats", 1)
                .WithCoachNote("Drive hips forward at the top", 2)
                .WithExerciseType("Workout", "Main workout exercise")
                .WithExerciseType("Cooldown", "Cooldown exercise")
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
                // The JSON serializer uses PascalCase by default
                content.Should().Contain("CoachNotes");
                content.Should().Contain("ExerciseTypeIds");
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
                .WithCoachNote("Grip the bar slightly wider than shoulder-width", 0)
                .WithCoachNote("Lower the bar to chest level", 1)
                .WithExerciseType("Workout")
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