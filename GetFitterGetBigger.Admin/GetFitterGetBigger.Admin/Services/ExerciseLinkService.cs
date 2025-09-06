using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services.Exceptions;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Web;

namespace GetFitterGetBigger.Admin.Services;

public class ExerciseLinkService : IExerciseLinkService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly JsonSerializerOptions _jsonOptions;

    public ExerciseLinkService(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<ExerciseLinkDto> CreateLinkAsync(string exerciseId, CreateExerciseLinkDto createLinkDto)
    {
        try
        {
            var requestUrl = $"api/exercises/{exerciseId}/links";

            Console.WriteLine($"[ExerciseLinkService] CreateLinkAsync called");
            Console.WriteLine($"[ExerciseLinkService] Request URL: {requestUrl}");
            Console.WriteLine($"[ExerciseLinkService] ExerciseId: {exerciseId}");
            Console.WriteLine($"[ExerciseLinkService] CreateLinkDto: SourceExerciseId={createLinkDto.SourceExerciseId}, TargetExerciseId={createLinkDto.TargetExerciseId}, LinkType={createLinkDto.LinkType}, DisplayOrder={createLinkDto.DisplayOrder}");

            var json = JsonSerializer.Serialize(createLinkDto, _jsonOptions);
            Console.WriteLine($"[ExerciseLinkService] Request JSON: {json}");

            var response = await _httpClient.PostAsJsonAsync(requestUrl, createLinkDto, _jsonOptions);

            Console.WriteLine($"[ExerciseLinkService] Response Status: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ExerciseLinkService] Error Response Body: {errorContent}");
                await HandleErrorResponseAsync(response, exerciseId, createLinkDto.TargetExerciseId);
            }

            var result = await response.Content.ReadFromJsonAsync<ExerciseLinkDto>(_jsonOptions);

            Console.WriteLine($"[ExerciseLinkService] Link created successfully with ID: {result?.Id}");

            // Invalidate cache for the exercise links
            InvalidateExerciseLinksCache(exerciseId);

            return result ?? throw new ExerciseLinkServiceException("Failed to deserialize created exercise link response");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"[ExerciseLinkService] HttpRequestException: {ex.Message}");
            throw new ExerciseLinkApiException($"Network error while creating exercise link: {ex.Message}", HttpStatusCode.ServiceUnavailable, ex);
        }
        catch (TaskCanceledException ex)
        {
            Console.WriteLine($"[ExerciseLinkService] TaskCanceledException: {ex.Message}");
            throw new ExerciseLinkApiException("Request timeout while creating exercise link", HttpStatusCode.RequestTimeout, ex);
        }
        catch (ExerciseLinkServiceException ex)
        {
            Console.WriteLine($"[ExerciseLinkService] ExerciseLinkServiceException: {ex.Message}");
            throw; // Re-throw our custom exceptions
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ExerciseLinkService] Unexpected Exception: {ex.GetType().Name}: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[ExerciseLinkService] Inner Exception: {ex.InnerException.Message}");
            }
            throw new ExerciseLinkServiceException($"Unexpected error while creating exercise link: {ex.Message}", ex);
        }
    }

    public async Task<ExerciseLinkDto> CreateBidirectionalLinkAsync(string exerciseId, CreateExerciseLinkDto createLinkDto)
    {
        try
        {
            // For bidirectional links (Alternative type), use the endpoint that handles both directions
            var requestUrl = $"api/exercises/{exerciseId}/links?createBidirectional=true";

            Console.WriteLine($"[ExerciseLinkService] CreateBidirectionalLinkAsync called");
            Console.WriteLine($"[ExerciseLinkService] Request URL: {requestUrl}");
            Console.WriteLine($"[ExerciseLinkService] ExerciseId: {exerciseId}");
            Console.WriteLine($"[ExerciseLinkService] CreateLinkDto: SourceExerciseId={createLinkDto.SourceExerciseId}, TargetExerciseId={createLinkDto.TargetExerciseId}, LinkType={createLinkDto.LinkType}, DisplayOrder={createLinkDto.DisplayOrder}");

            var json = JsonSerializer.Serialize(createLinkDto, _jsonOptions);
            Console.WriteLine($"[ExerciseLinkService] Request JSON: {json}");

            var result = await ExecuteWithRetryAsync(async () =>
            {
                var response = await _httpClient.PostAsJsonAsync(requestUrl, createLinkDto, _jsonOptions);

                Console.WriteLine($"[ExerciseLinkService] Response Status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[ExerciseLinkService] Error Response Body: {errorContent}");
                    await HandleErrorResponseAsync(response, exerciseId, createLinkDto.TargetExerciseId);
                }

                return await response.Content.ReadFromJsonAsync<ExerciseLinkDto>(_jsonOptions);
            });

            Console.WriteLine($"[ExerciseLinkService] Bidirectional link created successfully with ID: {result?.Id}");

            // Invalidate cache for both source and target exercises
            InvalidateExerciseLinksCache(exerciseId);
            InvalidateExerciseLinksCache(createLinkDto.TargetExerciseId);

            return result ?? throw new ExerciseLinkServiceException("Failed to deserialize created bidirectional exercise link response");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"[ExerciseLinkService] HttpRequestException: {ex.Message}");
            throw new ExerciseLinkApiException($"Network error while creating bidirectional exercise link: {ex.Message}", HttpStatusCode.ServiceUnavailable, ex);
        }
        catch (TaskCanceledException ex)
        {
            Console.WriteLine($"[ExerciseLinkService] TaskCanceledException: {ex.Message}");
            throw new ExerciseLinkApiException("Request timeout while creating bidirectional exercise link", HttpStatusCode.RequestTimeout, ex);
        }
        catch (ExerciseLinkServiceException ex)
        {
            Console.WriteLine($"[ExerciseLinkService] ExerciseLinkServiceException: {ex.Message}");
            throw; // Re-throw our custom exceptions
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ExerciseLinkService] Unexpected Exception: {ex.GetType().Name}: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[ExerciseLinkService] Inner Exception: {ex.InnerException.Message}");
            }
            throw new ExerciseLinkServiceException($"Unexpected error while creating bidirectional exercise link: {ex.Message}", ex);
        }
    }

    public async Task<ExerciseLinksResponseDto> GetLinksAsync(string exerciseId, string? linkType = null, bool includeExerciseDetails = false, bool includeReverse = false)
    {
        try
        {
            var cacheKey = $"exercise_links_{exerciseId}_{linkType}_{includeExerciseDetails}_{includeReverse}";

            if (!_cache.TryGetValue(cacheKey, out ExerciseLinksResponseDto? links))
            {
                var queryParams = BuildLinksQueryString(linkType, includeExerciseDetails, includeReverse);
                var requestUrl = $"api/exercises/{exerciseId}/links{queryParams}";

                Console.WriteLine($"[ExerciseLinkService] GetLinksAsync called");
                Console.WriteLine($"[ExerciseLinkService] Request URL: {requestUrl}");
                Console.WriteLine($"[ExerciseLinkService] ExerciseId: {exerciseId}, LinkType: {linkType}, IncludeDetails: {includeExerciseDetails}, IncludeReverse: {includeReverse}");

                var response = await _httpClient.GetAsync(requestUrl);

                Console.WriteLine($"[ExerciseLinkService] Response Status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[ExerciseLinkService] Error Response Body: {errorContent}");
                    
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new ExerciseNotFoundException(exerciseId);
                    }
                    await HandleErrorResponseAsync(response, exerciseId);
                }

                // First read as string to log the raw response
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ExerciseLinkService] GetLinksAsync - Raw JSON response:");
                Console.WriteLine(json);

                // Then deserialize
                links = JsonSerializer.Deserialize<ExerciseLinksResponseDto>(json, _jsonOptions);

                if (links != null)
                {
                    Console.WriteLine($"[ExerciseLinkService] GetLinksAsync - Parsed response:");
                    Console.WriteLine($"  - Exercise: {links.ExerciseName} (ID: {links.ExerciseId})");
                    Console.WriteLine($"  - Total Links: {links.TotalCount}");
                    Console.WriteLine($"  - Links Count: {links.Links?.Count ?? 0}");
                    
                    if (links.Links?.Any() == true)
                    {
                        foreach (var link in links.Links)
                        {
                            Console.WriteLine($"    - Link: {link.LinkType} to {link.TargetExerciseId}, DisplayOrder: {link.DisplayOrder}");
                            if (link.TargetExercise != null)
                            {
                                Console.WriteLine($"      Exercise Name: {link.TargetExercise.Name}");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"  - No links found in response");
                    }
                    // Use different cache durations based on link type
                    // Alternative links: 15 minutes (more dynamic)
                    // Warmup/Cooldown links: 1 hour (more stable)
                    var cacheDuration = linkType?.Equals("Alternative", StringComparison.OrdinalIgnoreCase) == true 
                        ? TimeSpan.FromMinutes(15) 
                        : TimeSpan.FromHours(1);
                        
                    _cache.Set(cacheKey, links, cacheDuration);
                }
            }

            return links ?? new ExerciseLinksResponseDto();
        }
        catch (HttpRequestException ex)
        {
            throw new ExerciseLinkApiException($"Network error while getting exercise links: {ex.Message}", HttpStatusCode.ServiceUnavailable, ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ExerciseLinkApiException("Request timeout while getting exercise links", HttpStatusCode.RequestTimeout, ex);
        }
        catch (ExerciseLinkServiceException)
        {
            throw; // Re-throw our custom exceptions
        }
        catch (Exception ex)
        {
            throw new ExerciseLinkServiceException($"Unexpected error while getting exercise links: {ex.Message}", ex);
        }
    }

    public async Task<List<ExerciseLinkDto>> GetSuggestedLinksAsync(string exerciseId, int count = 5)
    {
        try
        {
            var cacheKey = $"suggested_links_{exerciseId}_{count}";

            if (!_cache.TryGetValue(cacheKey, out List<ExerciseLinkDto>? suggestions))
            {
                var queryParams = count != 5 ? $"?count={count}" : "";
                var requestUrl = $"api/exercises/{exerciseId}/links/suggested{queryParams}";

                var response = await _httpClient.GetAsync(requestUrl);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new ExerciseNotFoundException(exerciseId);
                    }
                    await HandleErrorResponseAsync(response, exerciseId);
                }

                suggestions = await response.Content.ReadFromJsonAsync<List<ExerciseLinkDto>>(_jsonOptions);

                if (suggestions != null)
                {
                    // Cache suggestions for 30 minutes
                    _cache.Set(cacheKey, suggestions, TimeSpan.FromMinutes(30));
                }
            }

            return suggestions ?? new List<ExerciseLinkDto>();
        }
        catch (HttpRequestException ex)
        {
            throw new ExerciseLinkApiException($"Network error while getting suggested links: {ex.Message}", HttpStatusCode.ServiceUnavailable, ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ExerciseLinkApiException("Request timeout while getting suggested links", HttpStatusCode.RequestTimeout, ex);
        }
        catch (ExerciseLinkServiceException)
        {
            throw; // Re-throw our custom exceptions
        }
        catch (Exception ex)
        {
            throw new ExerciseLinkServiceException($"Unexpected error while getting suggested links: {ex.Message}", ex);
        }
    }

    public async Task<ExerciseLinkDto> UpdateLinkAsync(string exerciseId, string linkId, UpdateExerciseLinkDto updateLinkDto)
    {
        try
        {
            var requestUrl = $"api/exercises/{exerciseId}/links/{linkId}";

            var response = await _httpClient.PutAsJsonAsync(requestUrl, updateLinkDto, _jsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new ExerciseLinkNotFoundException(exerciseId, linkId);
                }
                await HandleErrorResponseAsync(response, exerciseId, linkId);
            }

            var result = await response.Content.ReadFromJsonAsync<ExerciseLinkDto>(_jsonOptions);

            // Invalidate cache for the exercise links
            InvalidateExerciseLinksCache(exerciseId);

            return result ?? throw new ExerciseLinkServiceException("Failed to deserialize updated exercise link response");
        }
        catch (HttpRequestException ex)
        {
            throw new ExerciseLinkApiException($"Network error while updating exercise link: {ex.Message}", HttpStatusCode.ServiceUnavailable, ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ExerciseLinkApiException("Request timeout while updating exercise link", HttpStatusCode.RequestTimeout, ex);
        }
        catch (ExerciseLinkServiceException)
        {
            throw; // Re-throw our custom exceptions
        }
        catch (Exception ex)
        {
            throw new ExerciseLinkServiceException($"Unexpected error while updating exercise link: {ex.Message}", ex);
        }
    }

    public async Task DeleteLinkAsync(string exerciseId, string linkId)
    {
        try
        {
            var requestUrl = $"api/exercises/{exerciseId}/links/{linkId}";

            var response = await _httpClient.DeleteAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new ExerciseLinkNotFoundException(exerciseId, linkId);
                }
                await HandleErrorResponseAsync(response, exerciseId, linkId);
            }

            // Invalidate cache for the exercise links
            InvalidateExerciseLinksCache(exerciseId);
        }
        catch (HttpRequestException ex)
        {
            throw new ExerciseLinkApiException($"Network error while deleting exercise link: {ex.Message}", HttpStatusCode.ServiceUnavailable, ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ExerciseLinkApiException("Request timeout while deleting exercise link", HttpStatusCode.RequestTimeout, ex);
        }
        catch (ExerciseLinkServiceException)
        {
            throw; // Re-throw our custom exceptions
        }
        catch (Exception ex)
        {
            throw new ExerciseLinkServiceException($"Unexpected error while deleting exercise link: {ex.Message}", ex);
        }
    }

    public async Task DeleteBidirectionalLinkAsync(string exerciseId, string linkId, bool deleteReverse = true)
    {
        try
        {
            // For bidirectional links (Alternative type), use the endpoint that handles both directions
            var requestUrl = $"api/exercises/{exerciseId}/links/{linkId}?deleteReverse={deleteReverse}";

            Console.WriteLine($"[ExerciseLinkService] DeleteBidirectionalLinkAsync called");
            Console.WriteLine($"[ExerciseLinkService] Request URL: {requestUrl}");
            Console.WriteLine($"[ExerciseLinkService] ExerciseId: {exerciseId}, LinkId: {linkId}, DeleteReverse: {deleteReverse}");

            var response = await _httpClient.DeleteAsync(requestUrl);

            Console.WriteLine($"[ExerciseLinkService] Response Status: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new ExerciseLinkNotFoundException(exerciseId, linkId);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ExerciseLinkService] Error Response Body: {errorContent}");
                await HandleErrorResponseAsync(response, exerciseId, linkId);
            }

            Console.WriteLine($"[ExerciseLinkService] Bidirectional link deleted successfully");

            // For bidirectional deletion, we need to invalidate cache for both exercises
            // The target exercise ID would need to be retrieved from the link data or passed separately
            // For now, just invalidate the source exercise cache
            InvalidateExerciseLinksCache(exerciseId);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"[ExerciseLinkService] HttpRequestException: {ex.Message}");
            throw new ExerciseLinkApiException($"Network error while deleting bidirectional exercise link: {ex.Message}", HttpStatusCode.ServiceUnavailable, ex);
        }
        catch (TaskCanceledException ex)
        {
            Console.WriteLine($"[ExerciseLinkService] TaskCanceledException: {ex.Message}");
            throw new ExerciseLinkApiException("Request timeout while deleting bidirectional exercise link", HttpStatusCode.RequestTimeout, ex);
        }
        catch (ExerciseLinkServiceException)
        {
            Console.WriteLine($"[ExerciseLinkService] ExerciseLinkServiceException occurred");
            throw; // Re-throw our custom exceptions
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ExerciseLinkService] Unexpected Exception: {ex.GetType().Name}: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[ExerciseLinkService] Inner Exception: {ex.InnerException.Message}");
            }
            throw new ExerciseLinkServiceException($"Unexpected error while deleting bidirectional exercise link: {ex.Message}", ex);
        }
    }

    private string BuildLinksQueryString(string? linkType, bool includeExerciseDetails, bool includeReverse = false)
    {
        var queryParams = new List<string>();

        if (!string.IsNullOrEmpty(linkType))
        {
            queryParams.Add($"linkType={HttpUtility.UrlEncode(linkType)}");
        }

        if (includeExerciseDetails)
        {
            queryParams.Add("includeExerciseDetails=true");
        }

        if (includeReverse)
        {
            queryParams.Add("includeReverse=true");
        }

        return queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
    }

    private async Task HandleErrorResponseAsync(HttpResponseMessage response, string exerciseId, string? targetOrLinkId = null)
    {
        var errorContent = await response.Content.ReadAsStringAsync();

        switch (response.StatusCode)
        {
            case HttpStatusCode.BadRequest:
                // Try to parse specific error types from API response
                if (errorContent.Contains("circular reference", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidExerciseLinkException("Cannot create circular reference between exercises");
                }
                if (errorContent.Contains("maximum", StringComparison.OrdinalIgnoreCase) && errorContent.Contains("exceeded", StringComparison.OrdinalIgnoreCase))
                {
                    var linkType = errorContent.Contains("warmup", StringComparison.OrdinalIgnoreCase) ? "warmup" : "cooldown";
                    throw new MaximumLinksExceededException(linkType, 10);
                }
                if (errorContent.Contains("duplicate", StringComparison.OrdinalIgnoreCase))
                {
                    throw new DuplicateExerciseLinkException(exerciseId, targetOrLinkId ?? "unknown", "unknown");
                }
                if (errorContent.Contains("invalid", StringComparison.OrdinalIgnoreCase) && errorContent.Contains("type", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidExerciseLinkException("Invalid exercise type for linking. Alternative exercises must share at least one exercise type");
                }
                if (errorContent.Contains("incompatible", StringComparison.OrdinalIgnoreCase) && errorContent.Contains("alternative", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidExerciseLinkException("Alternative exercises must share at least one exercise type (Workout, Warmup, or Cooldown)");
                }
                throw new InvalidExerciseLinkException($"Bad request: {errorContent}");

            case HttpStatusCode.NotFound:
                if (targetOrLinkId != null)
                {
                    throw new ExerciseLinkNotFoundException(exerciseId, targetOrLinkId);
                }
                throw new ExerciseNotFoundException(exerciseId);

            case HttpStatusCode.Conflict:
                throw new DuplicateExerciseLinkException(exerciseId, targetOrLinkId ?? "unknown", "unknown");

            case HttpStatusCode.UnprocessableEntity:
                throw new InvalidExerciseLinkException($"Validation error: {errorContent}");

            case HttpStatusCode.TooManyRequests:
                throw new ExerciseLinkApiException("Rate limit exceeded. Please try again later", response.StatusCode);

            case HttpStatusCode.InternalServerError:
                throw new ExerciseLinkApiException("Internal server error occurred", response.StatusCode);

            case HttpStatusCode.ServiceUnavailable:
                throw new ExerciseLinkApiException("Service temporarily unavailable", response.StatusCode);

            default:
                throw new ExerciseLinkApiException($"API error ({response.StatusCode}): {errorContent}", response.StatusCode);
        }
    }

    /// <summary>
    /// Executes an async operation with retry logic for transient failures
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="operation">Operation to execute</param>
    /// <param name="maxRetries">Maximum number of retries (default: 2)</param>
    /// <returns>Operation result</returns>
    private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, int maxRetries = 2)
    {
        var retryDelay = 1000; // Start with 1-second delay
        
        for (var attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (HttpRequestException ex) when (attempt < maxRetries)
            {
                Console.WriteLine($"[ExerciseLinkService] Retry attempt {attempt + 1}/{maxRetries + 1} after HttpRequestException: {ex.Message}");
                await Task.Delay(retryDelay);
                retryDelay *= 2; // Exponential backoff
                continue;
            }
            catch (TaskCanceledException ex) when (ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) && attempt < maxRetries)
            {
                Console.WriteLine($"[ExerciseLinkService] Retry attempt {attempt + 1}/{maxRetries + 1} after timeout: {ex.Message}");
                await Task.Delay(retryDelay);
                retryDelay *= 2;
                continue;
            }
            catch (ExerciseLinkApiException ex) when (ex.StatusCode.HasValue && IsTransientError(ex.StatusCode.Value) && attempt < maxRetries)
            {
                Console.WriteLine($"[ExerciseLinkService] Retry attempt {attempt + 1}/{maxRetries + 1} after API error {ex.StatusCode}: {ex.Message}");
                await Task.Delay(retryDelay);
                retryDelay *= 2;
                continue;
            }
        }

        // This should never be reached due to the exception handling above, but satisfies the compiler
        throw new InvalidOperationException("Retry logic failed unexpectedly");
    }

    /// <summary>
    /// Determines if an HTTP status code represents a transient error that should be retried
    /// </summary>
    /// <param name="statusCode">HTTP status code</param>
    /// <returns>True if the error is transient and should be retried</returns>
    private static bool IsTransientError(HttpStatusCode statusCode)
    {
        return statusCode == HttpStatusCode.ServiceUnavailable ||
               statusCode == HttpStatusCode.RequestTimeout ||
               statusCode == HttpStatusCode.TooManyRequests ||
               statusCode >= HttpStatusCode.InternalServerError;
    }

    private void InvalidateExerciseLinksCache(string exerciseId)
    {
        // Remove all cached entries for this exercise's links, including the new includeReverse combinations
        var cacheKeys = new[]
        {
            // Old format compatibility (without includeReverse)
            $"exercise_links_{exerciseId}__False",
            $"exercise_links_{exerciseId}__True",
            $"exercise_links_{exerciseId}_Warmup_False",
            $"exercise_links_{exerciseId}_Warmup_True",
            $"exercise_links_{exerciseId}_Cooldown_False",
            $"exercise_links_{exerciseId}_Cooldown_True",
            
            // New format with includeReverse parameter
            $"exercise_links_{exerciseId}__False_False",
            $"exercise_links_{exerciseId}__False_True",
            $"exercise_links_{exerciseId}__True_False",
            $"exercise_links_{exerciseId}__True_True",
            $"exercise_links_{exerciseId}_Warmup_False_False",
            $"exercise_links_{exerciseId}_Warmup_False_True",
            $"exercise_links_{exerciseId}_Warmup_True_False",
            $"exercise_links_{exerciseId}_Warmup_True_True",
            $"exercise_links_{exerciseId}_Cooldown_False_False",
            $"exercise_links_{exerciseId}_Cooldown_False_True",
            $"exercise_links_{exerciseId}_Cooldown_True_False",
            $"exercise_links_{exerciseId}_Cooldown_True_True",
            $"exercise_links_{exerciseId}_Alternative_False_False",
            $"exercise_links_{exerciseId}_Alternative_False_True",
            $"exercise_links_{exerciseId}_Alternative_True_False",
            $"exercise_links_{exerciseId}_Alternative_True_True",
            
            // Suggested links cache
            $"suggested_links_{exerciseId}_5",
            $"suggested_links_{exerciseId}_10",
            $"suggested_links_{exerciseId}_20"
        };

        foreach (var key in cacheKeys)
        {
            _cache.Remove(key);
        }
    }
}