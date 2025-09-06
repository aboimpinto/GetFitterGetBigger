using System.Net;
using System.Text.Json;
using FluentAssertions;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Services.Exceptions;
using GetFitterGetBigger.Admin.Builders;
using GetFitterGetBigger.Admin.Tests.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace GetFitterGetBigger.Admin.Tests.Services
{
    public class ExerciseLinkServiceTests
    {
        private readonly MockHttpMessageHandler _httpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;
        private readonly ExerciseLinkService _exerciseLinkService;

        public ExerciseLinkServiceTests()
        {
            _httpMessageHandler = new MockHttpMessageHandler();
            _httpClient = new HttpClient(_httpMessageHandler)
            {
                BaseAddress = new Uri("http://localhost:5214")
            };
            _memoryCache = new MemoryCache(new MemoryCacheOptions());

            _exerciseLinkService = new ExerciseLinkService(
                _httpClient,
                _memoryCache);
        }

        [Fact]
        public async Task CreateLinkAsync_WithValidData_ReturnsCreatedLink()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var createDto = new CreateExerciseLinkDtoBuilder()
                .WithTargetExerciseId(Guid.NewGuid().ToString())
                .AsWarmup()
                .WithDisplayOrder(1)
                .Build();

            var expectedLink = new ExerciseLinkDtoBuilder()
                .WithSourceExerciseId(exerciseId)
                .WithTargetExerciseId(createDto.TargetExerciseId)
                .WithTargetExerciseName("Target Exercise")
                .WithLinkType(createDto.LinkType)
                .WithDisplayOrder(createDto.DisplayOrder)
                .Build();

            _httpMessageHandler.SetupResponse(HttpStatusCode.Created, expectedLink);

            // Act
            var result = await _exerciseLinkService.CreateLinkAsync(exerciseId, createDto);

            // Assert
            result.Should().NotBeNull();
            result.SourceExerciseId.Should().Be(exerciseId);
            result.TargetExerciseId.Should().Be(createDto.TargetExerciseId);
            result.LinkType.Should().Be(createDto.LinkType);
            result.DisplayOrder.Should().Be(createDto.DisplayOrder);

            _httpMessageHandler.VerifyRequest(request =>
            {
                request.Method.Should().Be(HttpMethod.Post);
                request.RequestUri!.PathAndQuery.Should().Be($"/api/exercises/{exerciseId}/links");
                return true;
            });
        }

        [Fact]
        public async Task CreateLinkAsync_WhenDuplicateLink_ThrowsDuplicateExerciseLinkException()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var createDto = new CreateExerciseLinkDtoBuilder().Build();

            _httpMessageHandler.SetupResponse(HttpStatusCode.Conflict, new { error = "Duplicate link" });

            // Act & Assert
            await Assert.ThrowsAsync<DuplicateExerciseLinkException>(
                () => _exerciseLinkService.CreateLinkAsync(exerciseId, createDto));
        }

        [Fact]
        public async Task CreateLinkAsync_WhenMaximumLinksExceeded_ThrowsMaximumLinksExceededException()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var createDto = new CreateExerciseLinkDtoBuilder().AsWarmup().Build();

            _httpMessageHandler.SetupResponse(HttpStatusCode.BadRequest, new { error = "Maximum warmup links exceeded" });

            // Act & Assert
            await Assert.ThrowsAsync<MaximumLinksExceededException>(
                () => _exerciseLinkService.CreateLinkAsync(exerciseId, createDto));
        }

        [Fact]
        public async Task CreateLinkAsync_WhenInvalidExerciseType_ThrowsInvalidExerciseLinkException()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var createDto = new CreateExerciseLinkDtoBuilder().Build();

            _httpMessageHandler.SetupResponse(HttpStatusCode.BadRequest, new { error = "Invalid exercise type for linking" });

            // Act & Assert
            await Assert.ThrowsAsync<InvalidExerciseLinkException>(
                () => _exerciseLinkService.CreateLinkAsync(exerciseId, createDto));
        }

        [Fact]
        public async Task CreateLinkAsync_WhenCircularReference_ThrowsInvalidExerciseLinkException()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var createDto = new CreateExerciseLinkDtoBuilder().Build();

            _httpMessageHandler.SetupResponse(HttpStatusCode.BadRequest, new { error = "Circular reference detected" });

            // Act & Assert
            await Assert.ThrowsAsync<InvalidExerciseLinkException>(
                () => _exerciseLinkService.CreateLinkAsync(exerciseId, createDto));
        }

        [Fact]
        public async Task CreateLinkAsync_WhenNetworkError_ThrowsExerciseLinkApiException()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var createDto = new CreateExerciseLinkDtoBuilder().Build();

            _httpClient.Dispose();
            var faultyClient = new HttpClient(new FaultyHttpMessageHandler())
            {
                BaseAddress = new Uri("http://localhost:5214")
            };
            var service = new ExerciseLinkService(faultyClient, _memoryCache);

            // Act & Assert
            await Assert.ThrowsAsync<ExerciseLinkApiException>(
                () => service.CreateLinkAsync(exerciseId, createDto));
        }

        [Fact]
        public async Task GetLinksAsync_WithoutFilters_ReturnsAllLinks()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var warmupLink = new ExerciseLinkDtoBuilder()
                .WithSourceExerciseId(exerciseId)
                .AsWarmup()
                .WithDisplayOrder(1)
                .Build();
            var cooldownLink = new ExerciseLinkDtoBuilder()
                .WithSourceExerciseId(exerciseId)
                .AsCooldown()
                .WithDisplayOrder(1)
                .Build();

            var expectedResponse = new ExerciseLinksResponseDtoBuilder()
                .WithExerciseId(exerciseId)
                .WithLinks(warmupLink, cooldownLink)
                .WithTotalCount(2)
                .Build();

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedResponse);

            // Act
            var result = await _exerciseLinkService.GetLinksAsync(exerciseId);

            // Assert
            result.Should().NotBeNull();
            result.Links.Should().HaveCount(2);
            result.Links.Should().Contain(l => l.LinkType == "Warmup");
            result.Links.Should().Contain(l => l.LinkType == "Cooldown");
            result.TotalCount.Should().Be(2);

            _httpMessageHandler.VerifyRequest(request =>
            {
                request.Method.Should().Be(HttpMethod.Get);
                request.RequestUri!.PathAndQuery.Should().Be($"/api/exercises/{exerciseId}/links");
                return true;
            });
        }

        [Fact]
        public async Task GetLinksAsync_WithLinkTypeFilter_ReturnsFilteredLinks()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var warmupLink = new ExerciseLinkDtoBuilder()
                .WithSourceExerciseId(exerciseId)
                .AsWarmup()
                .Build();

            var expectedResponse = new ExerciseLinksResponseDtoBuilder()
                .WithExerciseId(exerciseId)
                .WithLinks(warmupLink)
                .WithTotalCount(1)
                .Build();

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedResponse);

            // Act
            var result = await _exerciseLinkService.GetLinksAsync(exerciseId, "Warmup");

            // Assert
            result.Should().NotBeNull();
            result.Links.Should().HaveCount(1);
            result.Links[0].LinkType.Should().Be("Warmup");

            _httpMessageHandler.VerifyRequest(request =>
            {
                request.RequestUri!.PathAndQuery.Should().Contain("linkType=Warmup");
                return true;
            });
        }

        [Fact]
        public async Task GetLinksAsync_WithIncludeExerciseDetails_ReturnsDetailsInLinks()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var targetExercise = new ExerciseDtoBuilder()
                .WithName("Target Exercise")
                .WithDescription("Target exercise description")
                .Build();

            var warmupLink = new ExerciseLinkDtoBuilder()
                .WithSourceExerciseId(exerciseId)
                .AsWarmup()
                .WithTargetExercise(targetExercise)
                .Build();

            var expectedResponse = new ExerciseLinksResponseDtoBuilder()
                .WithExerciseId(exerciseId)
                .WithLinks(warmupLink)
                .WithTotalCount(1)
                .Build();

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedResponse);

            // Act
            var result = await _exerciseLinkService.GetLinksAsync(exerciseId, null, true);

            // Assert
            result.Should().NotBeNull();
            result.Links.Should().HaveCount(1);
            result.Links[0].TargetExercise.Should().NotBeNull();
            result.Links[0].TargetExercise!.Name.Should().Be("Target Exercise");

            _httpMessageHandler.VerifyRequest(request =>
            {
                request.RequestUri!.PathAndQuery.Should().Contain("includeExerciseDetails=true");
                return true;
            });
        }


        [Fact]
        public async Task GetLinksAsync_WhenExerciseNotFound_ThrowsExerciseNotFoundException()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            _httpMessageHandler.SetupResponse(HttpStatusCode.NotFound, new { error = "Exercise not found" });

            // Act & Assert
            await Assert.ThrowsAsync<ExerciseNotFoundException>(
                () => _exerciseLinkService.GetLinksAsync(exerciseId));
        }

        [Fact]
        public async Task GetSuggestedLinksAsync_WithDefaultCount_ReturnsSuggestions()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var suggestions = new List<ExerciseLinkDto>
            {
                new ExerciseLinkDtoBuilder().AsWarmup().WithTargetExerciseName("Suggested 1").Build(),
                new ExerciseLinkDtoBuilder().AsWarmup().WithTargetExerciseName("Suggested 2").Build(),
                new ExerciseLinkDtoBuilder().AsCooldown().WithTargetExerciseName("Suggested 3").Build()
            };

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, suggestions);

            // Act
            var result = await _exerciseLinkService.GetSuggestedLinksAsync(exerciseId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].TargetExerciseName.Should().Be("Suggested 1");
            result[1].TargetExerciseName.Should().Be("Suggested 2");
            result[2].TargetExerciseName.Should().Be("Suggested 3");

            _httpMessageHandler.VerifyRequest(request =>
            {
                request.Method.Should().Be(HttpMethod.Get);
                request.RequestUri!.PathAndQuery.Should().Be($"/api/exercises/{exerciseId}/links/suggested");
                return true;
            });
        }

        [Fact]
        public async Task GetSuggestedLinksAsync_WithCustomCount_ReturnsCorrectNumber()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var suggestions = new List<ExerciseLinkDto>
            {
                new ExerciseLinkDtoBuilder().AsWarmup().Build(),
                new ExerciseLinkDtoBuilder().AsWarmup().Build()
            };

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, suggestions);

            // Act
            var result = await _exerciseLinkService.GetSuggestedLinksAsync(exerciseId, 2);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            _httpMessageHandler.VerifyRequest(request =>
            {
                request.RequestUri!.PathAndQuery.Should().Be($"/api/exercises/{exerciseId}/links/suggested?count=2");
                return true;
            });
        }


        [Fact]
        public async Task UpdateLinkAsync_WithValidData_ReturnsUpdatedLink()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var linkId = Guid.NewGuid().ToString();
            var targetExerciseId = Guid.NewGuid().ToString();
            var updateDto = new UpdateExerciseLinkDtoBuilder()
                .WithDisplayOrder(2)
                .AsActive()
                .Build();

            // Setup response for GET request to fetch link details (for cache invalidation)
            var linksResponse = new ExerciseLinksResponseDto
            {
                ExerciseId = exerciseId,
                ExerciseName = "Test Exercise",
                Links = new List<ExerciseLinkDto>
                {
                    new ExerciseLinkDto
                    {
                        Id = linkId,
                        SourceExerciseId = exerciseId,
                        TargetExerciseId = targetExerciseId,
                        LinkType = "Warmup"
                    }
                },
                TotalCount = 1
            };
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, linksResponse);

            var updatedLink = new ExerciseLinkDtoBuilder()
                .WithId(linkId)
                .WithSourceExerciseId(exerciseId)
                .WithDisplayOrder(updateDto.DisplayOrder)
                .Build();

            // Setup response for PUT request
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, updatedLink);

            // Act
            var result = await _exerciseLinkService.UpdateLinkAsync(exerciseId, linkId, updateDto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(linkId);
            result.DisplayOrder.Should().Be(updateDto.DisplayOrder);

            // Verify both GET (for cache invalidation) and PUT requests were made
            _httpMessageHandler.VerifyRequest(request =>
                request.Method == HttpMethod.Get && 
                request.RequestUri!.PathAndQuery.Contains($"/api/exercises/{exerciseId}/links"));
            
            _httpMessageHandler.VerifyRequest(request =>
                request.Method == HttpMethod.Put && 
                request.RequestUri!.PathAndQuery == $"/api/exercises/{exerciseId}/links/{linkId}");
        }

        [Fact]
        public async Task UpdateLinkAsync_WhenLinkNotFound_ThrowsExerciseLinkNotFoundException()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var linkId = Guid.NewGuid().ToString();
            var updateDto = new UpdateExerciseLinkDtoBuilder().Build();

            // Setup response for GET request to fetch link details (for cache invalidation)
            // This returns OK but with no matching link ID (simulating link not found in the list)
            var linksResponse = new ExerciseLinksResponseDto
            {
                ExerciseId = exerciseId,
                ExerciseName = "Test Exercise",
                Links = new List<ExerciseLinkDto>(), // Empty list - no links found
                TotalCount = 0
            };
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, linksResponse);
            
            // Setup response for PUT request
            _httpMessageHandler.SetupResponse(HttpStatusCode.NotFound, new { error = "Link not found" });

            // Act & Assert
            await Assert.ThrowsAsync<ExerciseLinkNotFoundException>(
                () => _exerciseLinkService.UpdateLinkAsync(exerciseId, linkId, updateDto));
        }


        [Fact]
        public async Task DeleteLinkAsync_WithValidId_DeletesSuccessfully()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var linkId = Guid.NewGuid().ToString();

            // Setup response for GET request to fetch link details (for cache invalidation)
            var linksResponse = new ExerciseLinksResponseDto
            {
                ExerciseId = exerciseId,
                ExerciseName = "Test Exercise",
                Links = new List<ExerciseLinkDto>
                {
                    new ExerciseLinkDto
                    {
                        Id = linkId,
                        SourceExerciseId = exerciseId,
                        TargetExerciseId = "target-exercise-id",
                        LinkType = "Warmup"
                    }
                },
                TotalCount = 1
            };
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, linksResponse);
            
            // Setup response for DELETE request
            _httpMessageHandler.SetupResponse(HttpStatusCode.NoContent);

            // Act
            await _exerciseLinkService.DeleteLinkAsync(exerciseId, linkId);

            // Assert
            // Verify GET request was made first (to fetch link details for cache invalidation)
            _httpMessageHandler.VerifyRequest(request =>
                request.Method == HttpMethod.Get && 
                request.RequestUri!.PathAndQuery.Contains($"/api/exercises/{exerciseId}/links"));
            
            // Verify DELETE request was made
            _httpMessageHandler.VerifyRequest(request =>
                request.Method == HttpMethod.Delete && 
                request.RequestUri!.PathAndQuery == $"/api/exercises/{exerciseId}/links/{linkId}");
        }

        [Fact]
        public async Task DeleteLinkAsync_WhenLinkNotFound_ThrowsExerciseLinkNotFoundException()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var linkId = Guid.NewGuid().ToString();

            // Setup response for GET request to fetch link details (link won't be found but we continue)
            var emptyLinksResponse = new ExerciseLinksResponseDto
            {
                ExerciseId = exerciseId,
                ExerciseName = "Test Exercise",
                Links = new List<ExerciseLinkDto>(), // No links found
                TotalCount = 0
            };
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, emptyLinksResponse);
            
            // Setup response for DELETE request - returns NotFound
            _httpMessageHandler.SetupResponse(HttpStatusCode.NotFound, new { error = "Link not found" });

            // Act & Assert
            await Assert.ThrowsAsync<ExerciseLinkNotFoundException>(
                () => _exerciseLinkService.DeleteLinkAsync(exerciseId, linkId));
        }

        [Fact]
        public async Task CreateBidirectionalLinkAsync_WithAlternativeLink_CreatesBidirectionalLink()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var createDto = new CreateExerciseLinkDtoBuilder()
                .WithTargetExerciseId(Guid.NewGuid().ToString())
                .AsAlternative()
                .Build();

            var expectedLink = new ExerciseLinkDtoBuilder()
                .WithSourceExerciseId(exerciseId)
                .WithTargetExerciseId(createDto.TargetExerciseId)
                .WithTargetExerciseName("Alternative Exercise")
                .WithLinkType("Alternative")
                .Build();

            _httpMessageHandler.SetupResponse(HttpStatusCode.Created, expectedLink);

            // Act
            var result = await _exerciseLinkService.CreateBidirectionalLinkAsync(exerciseId, createDto);

            // Assert
            result.Should().NotBeNull();
            result.SourceExerciseId.Should().Be(exerciseId);
            result.TargetExerciseId.Should().Be(createDto.TargetExerciseId);
            result.LinkType.Should().Be("Alternative");

            _httpMessageHandler.VerifyRequest(request =>
            {
                request.Method.Should().Be(HttpMethod.Post);
                request.RequestUri!.PathAndQuery.Should().Be($"/api/exercises/{exerciseId}/links?createBidirectional=true");
                return true;
            });
        }

        [Fact]
        public async Task CreateBidirectionalLinkAsync_WhenAlternativeIncompatible_ThrowsInvalidExerciseLinkException()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var createDto = new CreateExerciseLinkDtoBuilder()
                .WithTargetExerciseId(Guid.NewGuid().ToString())
                .AsAlternative()
                .Build();

            _httpMessageHandler.SetupResponse(HttpStatusCode.BadRequest, 
                new { error = "Alternative exercises must share at least one exercise type" });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidExerciseLinkException>(
                () => _exerciseLinkService.CreateBidirectionalLinkAsync(exerciseId, createDto));

            exception.Message.Should().Contain("Alternative exercises must share at least one exercise type");
        }

        [Fact]
        public async Task CreateBidirectionalLinkAsync_WhenServiceUnavailable_ThrowsExerciseLinkApiException()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var createDto = new CreateExerciseLinkDtoBuilder()
                .WithTargetExerciseId(Guid.NewGuid().ToString())
                .AsAlternative()
                .Build();

            // Setup multiple ServiceUnavailable responses for retry attempts
            _httpMessageHandler
                .SetupResponse(HttpStatusCode.ServiceUnavailable)
                .SetupResponse(HttpStatusCode.ServiceUnavailable)
                .SetupResponse(HttpStatusCode.ServiceUnavailable);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ExerciseLinkApiException>(
                () => _exerciseLinkService.CreateBidirectionalLinkAsync(exerciseId, createDto));

            exception.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        public async Task CreateBidirectionalLinkAsync_WhenTimeout_ThrowsExerciseLinkApiException()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var createDto = new CreateExerciseLinkDtoBuilder()
                .WithTargetExerciseId(Guid.NewGuid().ToString())
                .AsAlternative()
                .Build();

            // Setup multiple RequestTimeout responses for retry attempts  
            _httpMessageHandler
                .SetupResponse(HttpStatusCode.RequestTimeout)
                .SetupResponse(HttpStatusCode.RequestTimeout)
                .SetupResponse(HttpStatusCode.RequestTimeout);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ExerciseLinkApiException>(
                () => _exerciseLinkService.CreateBidirectionalLinkAsync(exerciseId, createDto));

            exception.StatusCode.Should().Be(HttpStatusCode.RequestTimeout);
        }

        [Fact]
        public async Task DeleteBidirectionalLinkAsync_WithValidId_DeletesBidirectionalLink()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var linkId = Guid.NewGuid().ToString();
            var targetExerciseId = Guid.NewGuid().ToString();

            // Setup response for GET request to fetch link details (for cache invalidation)
            var linksResponse = new ExerciseLinksResponseDto
            {
                ExerciseId = exerciseId,
                ExerciseName = "Test Exercise",
                Links = new List<ExerciseLinkDto>
                {
                    new ExerciseLinkDto
                    {
                        Id = linkId,
                        SourceExerciseId = exerciseId,
                        TargetExerciseId = targetExerciseId,
                        LinkType = "Alternative"
                    }
                },
                TotalCount = 1
            };
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, linksResponse);
            
            // Setup response for DELETE request
            _httpMessageHandler.SetupResponse(HttpStatusCode.NoContent);

            // Act
            await _exerciseLinkService.DeleteBidirectionalLinkAsync(exerciseId, linkId, true);

            // Assert
            // Verify GET request was made first (to fetch link details for cache invalidation)
            _httpMessageHandler.VerifyRequest(request =>
                request.Method == HttpMethod.Get && 
                request.RequestUri!.PathAndQuery.Contains($"/api/exercises/{exerciseId}/links"));
            
            // Verify DELETE request was made with deleteReverse parameter
            _httpMessageHandler.VerifyRequest(request =>
                request.Method == HttpMethod.Delete && 
                request.RequestUri!.PathAndQuery == $"/api/exercises/{exerciseId}/links/{linkId}?deleteReverse=True");
        }

        [Fact]
        public async Task GetLinksAsync_WithIncludeReverse_IncludesReverseParameter()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var responseDto = new ExerciseLinksResponseDto 
            { 
                Links = new List<ExerciseLinkDto>(), 
                TotalCount = 0 
            };

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, responseDto);

            // Act
            await _exerciseLinkService.GetLinksAsync(exerciseId, "Alternative", true, true);

            // Assert
            _httpMessageHandler.VerifyRequest(request =>
            {
                request.Method.Should().Be(HttpMethod.Get);
                request.RequestUri!.PathAndQuery.Should().Be($"/api/exercises/{exerciseId}/links?linkType=Alternative&includeExerciseDetails=true&includeReverse=true");
                return true;
            });
        }

        [Fact]
        public async Task GetLinksAsync_AlternativeLinksAreCachedFor15Minutes()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var responseDto = new ExerciseLinksResponseDto 
            { 
                Links = new List<ExerciseLinkDto>(), 
                TotalCount = 0 
            };

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, responseDto);

            // Act - First call
            await _exerciseLinkService.GetLinksAsync(exerciseId, "Alternative", true, false);
            
            // Act - Second call (should use cache)
            await _exerciseLinkService.GetLinksAsync(exerciseId, "Alternative", true, false);

            // Assert
            _httpMessageHandler.VerifyCallCount(1); // Should only call API once due to caching
        }

        // Helper class for testing network failures
        private class FaultyHttpMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                throw new HttpRequestException("Network failure");
            }
        }
    }
}