using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace GetFitterGetBigger.Admin.Tests.Services;

public class ExerciseLinkServiceCacheInvalidationTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;
    private readonly ExerciseLinkService _service;
    private readonly JsonSerializerOptions _jsonOptions;

    public ExerciseLinkServiceCacheInvalidationTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _service = new ExerciseLinkService(_httpClient, _memoryCache);
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    [Fact]
    public async Task DeleteLink_Should_Invalidate_Cache_For_Both_Source_And_Target_Exercises()
    {
        // Arrange
        var workoutExerciseId = "workout-exercise-id";
        var warmupExerciseId = "warmup-exercise-id";
        var linkId = "link-id";

        // Initial data: Workout has a warmup link
        var workoutLinks = new ExerciseLinksResponseDto
        {
            ExerciseId = workoutExerciseId,
            ExerciseName = "Barbell Squat",
            Links = new List<ExerciseLinkDto>
            {
                new ExerciseLinkDto
                {
                    Id = linkId,
                    SourceExerciseId = workoutExerciseId,
                    TargetExerciseId = warmupExerciseId,
                    LinkType = "Warmup",
                    DisplayOrder = 1,
                    TargetExercise = new ExerciseDto
                    {
                        Id = warmupExerciseId,
                        Name = "Bodyweight Squat"
                    }
                }
            },
            TotalCount = 1
        };

        // Warmup shows the workout as a reverse link
        var warmupLinks = new ExerciseLinksResponseDto
        {
            ExerciseId = warmupExerciseId,
            ExerciseName = "Bodyweight Squat",
            Links = new List<ExerciseLinkDto>
            {
                new ExerciseLinkDto
                {
                    Id = "reverse-link-id",
                    SourceExerciseId = workoutExerciseId,
                    TargetExerciseId = warmupExerciseId,
                    LinkType = "Warmup",
                    TargetExercise = new ExerciseDto
                    {
                        Id = workoutExerciseId,
                        Name = "Barbell Squat"
                    }
                }
            },
            TotalCount = 1
        };

        // Setup initial GET requests to populate cache
        // We need multiple returns for workout exercise: 
        // 1. Initial load, 2. Load during delete to get target ID, 3. Load after delete
        _mockHttpMessageHandler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains($"api/exercises/{workoutExerciseId}/links")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage // Initial load
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(workoutLinks, options: _jsonOptions)
            })
            .ReturnsAsync(new HttpResponseMessage // Load during delete to get target ID
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(workoutLinks, options: _jsonOptions)
            })
            .ReturnsAsync(new HttpResponseMessage // After deletion, workout has no links
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new ExerciseLinksResponseDto
                {
                    ExerciseId = workoutExerciseId,
                    ExerciseName = "Barbell Squat",
                    Links = new List<ExerciseLinkDto>(),
                    TotalCount = 0
                }, options: _jsonOptions)
            });

        _mockHttpMessageHandler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains($"api/exercises/{warmupExerciseId}/links")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(warmupLinks, options: _jsonOptions)
            })
            .ReturnsAsync(new HttpResponseMessage // After deletion, warmup should have no reverse links
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new ExerciseLinksResponseDto
                {
                    ExerciseId = warmupExerciseId,
                    ExerciseName = "Bodyweight Squat",
                    Links = new List<ExerciseLinkDto>(),
                    TotalCount = 0
                }, options: _jsonOptions)
            });

        // Setup DELETE request
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Delete &&
                    req.RequestUri!.ToString().Contains($"api/exercises/{workoutExerciseId}/links/{linkId}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent
            });

        // Act
        // Step 1: Load both exercises to populate cache
        var initialWorkoutLinks = await _service.GetLinksAsync(workoutExerciseId, includeReverse: true);
        var initialWarmupLinks = await _service.GetLinksAsync(warmupExerciseId, includeReverse: true);

        // Verify initial state
        Assert.Single(initialWorkoutLinks.Links!);
        Assert.Equal(warmupExerciseId, initialWorkoutLinks.Links!.First().TargetExerciseId);
        Assert.Single(initialWarmupLinks.Links!);
        // The warmup shows the workout as a reverse link (source is workout)
        Assert.Equal(workoutExerciseId, initialWarmupLinks.Links!.First().SourceExerciseId);
        Assert.Equal(workoutExerciseId, initialWarmupLinks.Links!.First().TargetExercise?.Id);

        // Step 2: Delete the link from workout
        await _service.DeleteLinkAsync(workoutExerciseId, linkId);

        // Step 3: Get links again - should fetch fresh data, not cached
        var updatedWorkoutLinks = await _service.GetLinksAsync(workoutExerciseId, includeReverse: true);
        var updatedWarmupLinks = await _service.GetLinksAsync(warmupExerciseId, includeReverse: true);

        // Assert - Both should have no links after deletion
        Assert.Empty(updatedWorkoutLinks.Links!);
        Assert.Empty(updatedWarmupLinks.Links!); // This will FAIL with current implementation
    }

    [Fact]
    public async Task DeleteBidirectionalLink_Should_Invalidate_Cache_For_Both_Exercises()
    {
        // Arrange
        var exerciseAId = "exercise-a-id";
        var exerciseBId = "exercise-b-id";
        var linkId = "link-ab-id";

        // Initial data: Both exercises have alternative links to each other
        var exerciseALinks = new ExerciseLinksResponseDto
        {
            ExerciseId = exerciseAId,
            ExerciseName = "Push-ups",
            Links = new List<ExerciseLinkDto>
            {
                new ExerciseLinkDto
                {
                    Id = linkId,
                    SourceExerciseId = exerciseAId,
                    TargetExerciseId = exerciseBId,
                    LinkType = "Alternative",
                    TargetExercise = new ExerciseDto
                    {
                        Id = exerciseBId,
                        Name = "Bench Press"
                    }
                }
            },
            TotalCount = 1
        };

        var exerciseBLinks = new ExerciseLinksResponseDto
        {
            ExerciseId = exerciseBId,
            ExerciseName = "Bench Press",
            Links = new List<ExerciseLinkDto>
            {
                new ExerciseLinkDto
                {
                    Id = "link-ba-id",
                    SourceExerciseId = exerciseBId,
                    TargetExerciseId = exerciseAId,
                    LinkType = "Alternative",
                    TargetExercise = new ExerciseDto
                    {
                        Id = exerciseAId,
                        Name = "Push-ups"
                    }
                }
            },
            TotalCount = 1
        };

        // Setup initial GET requests
        // We need multiple returns for exercise A: 
        // 1. Initial load, 2. Load during delete to get target ID, 3. Load after delete
        _mockHttpMessageHandler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains($"api/exercises/{exerciseAId}/links")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage // Initial load
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(exerciseALinks, options: _jsonOptions)
            })
            .ReturnsAsync(new HttpResponseMessage // Load during delete to get target ID
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(exerciseALinks, options: _jsonOptions)
            })
            .ReturnsAsync(new HttpResponseMessage // After deletion
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new ExerciseLinksResponseDto
                {
                    ExerciseId = exerciseAId,
                    ExerciseName = "Push-ups",
                    Links = new List<ExerciseLinkDto>(),
                    TotalCount = 0
                }, options: _jsonOptions)
            });

        _mockHttpMessageHandler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains($"api/exercises/{exerciseBId}/links")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(exerciseBLinks, options: _jsonOptions)
            })
            .ReturnsAsync(new HttpResponseMessage // After deletion
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new ExerciseLinksResponseDto
                {
                    ExerciseId = exerciseBId,
                    ExerciseName = "Bench Press",
                    Links = new List<ExerciseLinkDto>(),
                    TotalCount = 0
                }, options: _jsonOptions)
            });

        // Setup DELETE request for bidirectional deletion
        // Use SetupSequence since we have multiple DELETE calls (one to try to get link details, then the actual delete)
        _mockHttpMessageHandler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Delete),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent
            })
            .ReturnsAsync(new HttpResponseMessage // In case there's a second delete call
            {
                StatusCode = HttpStatusCode.NoContent
            });

        // Act
        // Step 1: Load both exercises to populate cache
        var initialExerciseALinks = await _service.GetLinksAsync(exerciseAId);
        var initialExerciseBLinks = await _service.GetLinksAsync(exerciseBId);

        // Verify initial state
        Assert.Single(initialExerciseALinks.Links!);
        Assert.Equal(exerciseBId, initialExerciseALinks.Links!.First().TargetExerciseId);
        Assert.Single(initialExerciseBLinks.Links!);
        Assert.Equal(exerciseAId, initialExerciseBLinks.Links!.First().TargetExerciseId);

        // Step 2: Delete bidirectional link
        await _service.DeleteBidirectionalLinkAsync(exerciseAId, linkId);

        // Step 3: Get links again - should fetch fresh data, not cached
        var updatedExerciseALinks = await _service.GetLinksAsync(exerciseAId);
        var updatedExerciseBLinks = await _service.GetLinksAsync(exerciseBId);

        // Assert - Both should have no links after bidirectional deletion
        Assert.Empty(updatedExerciseALinks.Links!);
        Assert.Empty(updatedExerciseBLinks.Links!);
    }

    [Fact]
    public async Task CreateLink_Should_Invalidate_Cache_For_Both_Source_And_Target_Exercises()
    {
        // Arrange
        var workoutExerciseId = "workout-exercise-id";
        var warmupExerciseId = "warmup-exercise-id";
        var newLinkId = "new-link-id";

        // Initial state: both exercises have no links
        var emptyWorkoutLinks = new ExerciseLinksResponseDto
        {
            ExerciseId = workoutExerciseId,
            ExerciseName = "Barbell Squat",
            Links = new List<ExerciseLinkDto>(),
            TotalCount = 0
        };

        var emptyWarmupLinks = new ExerciseLinksResponseDto
        {
            ExerciseId = warmupExerciseId,
            ExerciseName = "Bodyweight Squat",
            Links = new List<ExerciseLinkDto>(),
            TotalCount = 0
        };

        // After creation: workout has warmup link, warmup shows reverse link
        var workoutWithLink = new ExerciseLinksResponseDto
        {
            ExerciseId = workoutExerciseId,
            ExerciseName = "Barbell Squat",
            Links = new List<ExerciseLinkDto>
            {
                new ExerciseLinkDto
                {
                    Id = newLinkId,
                    SourceExerciseId = workoutExerciseId,
                    TargetExerciseId = warmupExerciseId,
                    LinkType = "Warmup",
                    TargetExercise = new ExerciseDto
                    {
                        Id = warmupExerciseId,
                        Name = "Bodyweight Squat"
                    }
                }
            },
            TotalCount = 1
        };

        var warmupWithReverseLink = new ExerciseLinksResponseDto
        {
            ExerciseId = warmupExerciseId,
            ExerciseName = "Bodyweight Squat",
            Links = new List<ExerciseLinkDto>
            {
                new ExerciseLinkDto
                {
                    Id = "reverse-" + newLinkId,
                    SourceExerciseId = workoutExerciseId,
                    TargetExerciseId = warmupExerciseId,
                    LinkType = "Warmup",
                    TargetExercise = new ExerciseDto
                    {
                        Id = workoutExerciseId,
                        Name = "Barbell Squat"
                    }
                }
            },
            TotalCount = 1
        };

        // Setup GET requests
        _mockHttpMessageHandler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains($"api/exercises/{workoutExerciseId}/links")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage // Initial load - empty
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(emptyWorkoutLinks, options: _jsonOptions)
            })
            .ReturnsAsync(new HttpResponseMessage // After creation - has link
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(workoutWithLink, options: _jsonOptions)
            });

        _mockHttpMessageHandler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains($"api/exercises/{warmupExerciseId}/links")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage // Initial load - empty
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(emptyWarmupLinks, options: _jsonOptions)
            })
            .ReturnsAsync(new HttpResponseMessage // After creation - has reverse link
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(warmupWithReverseLink, options: _jsonOptions)
            });

        // Setup POST request for creating the link
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().Contains($"api/exercises/{workoutExerciseId}/links")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = JsonContent.Create(new ExerciseLinkDto
                {
                    Id = newLinkId,
                    SourceExerciseId = workoutExerciseId,
                    TargetExerciseId = warmupExerciseId,
                    LinkType = "Warmup"
                }, options: _jsonOptions)
            });

        // Act
        // Step 1: Load both exercises to populate cache (both empty)
        var initialWorkoutLinks = await _service.GetLinksAsync(workoutExerciseId, includeReverse: true);
        var initialWarmupLinks = await _service.GetLinksAsync(warmupExerciseId, includeReverse: true);

        // Verify initial state
        Assert.Empty(initialWorkoutLinks.Links!);
        Assert.Empty(initialWarmupLinks.Links!);

        // Step 2: Create the link from workout to warmup
        var createDto = new CreateExerciseLinkDto
        {
            SourceExerciseId = workoutExerciseId,
            TargetExerciseId = warmupExerciseId,
            LinkType = "Warmup",
            DisplayOrder = 1
        };
        await _service.CreateLinkAsync(workoutExerciseId, createDto);

        // Step 3: Get links again - should fetch fresh data, not cached
        var updatedWorkoutLinks = await _service.GetLinksAsync(workoutExerciseId, includeReverse: true);
        var updatedWarmupLinks = await _service.GetLinksAsync(warmupExerciseId, includeReverse: true);

        // Assert - Both should show the new relationship
        Assert.Single(updatedWorkoutLinks.Links!);
        Assert.Equal(warmupExerciseId, updatedWorkoutLinks.Links!.First().TargetExerciseId);
        
        Assert.Single(updatedWarmupLinks.Links!);
        Assert.Equal(workoutExerciseId, updatedWarmupLinks.Links!.First().SourceExerciseId);
    }
}