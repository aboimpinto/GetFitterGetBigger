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
    private readonly IConfiguration _configuration;
    private readonly string _apiBaseUrl;
    private readonly JsonSerializerOptions _jsonOptions;

    public ExerciseLinkService(HttpClient httpClient, IMemoryCache cache, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _cache = cache;
        _configuration = configuration;
        _apiBaseUrl = _configuration["ApiBaseUrl"] ?? string.Empty;
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
            var requestUrl = $"{_apiBaseUrl}/api/exercises/{exerciseId}/links";
            
            var response = await _httpClient.PostAsJsonAsync(requestUrl, createLinkDto, _jsonOptions);
            
            if (!response.IsSuccessStatusCode)
            {
                await HandleErrorResponseAsync(response, exerciseId, createLinkDto.TargetExerciseId);
            }

            var result = await response.Content.ReadFromJsonAsync<ExerciseLinkDto>(_jsonOptions);
            
            // Invalidate cache for the exercise links
            InvalidateExerciseLinksCache(exerciseId);
            
            return result ?? throw new ExerciseLinkServiceException("Failed to deserialize created exercise link response");
        }
        catch (HttpRequestException ex)
        {
            throw new ExerciseLinkApiException($"Network error while creating exercise link: {ex.Message}", HttpStatusCode.ServiceUnavailable, ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ExerciseLinkApiException("Request timeout while creating exercise link", HttpStatusCode.RequestTimeout, ex);
        }
        catch (ExerciseLinkServiceException)
        {
            throw; // Re-throw our custom exceptions
        }
        catch (Exception ex)
        {
            throw new ExerciseLinkServiceException($"Unexpected error while creating exercise link: {ex.Message}", ex);
        }
    }

    public async Task<ExerciseLinksResponseDto> GetLinksAsync(string exerciseId, string? linkType = null, bool includeExerciseDetails = false)
    {
        try
        {
            var cacheKey = $"exercise_links_{exerciseId}_{linkType}_{includeExerciseDetails}";
            
            if (!_cache.TryGetValue(cacheKey, out ExerciseLinksResponseDto? links))
            {
                var queryParams = BuildLinksQueryString(linkType, includeExerciseDetails);
                var requestUrl = $"{_apiBaseUrl}/api/exercises/{exerciseId}/links{queryParams}";

                var response = await _httpClient.GetAsync(requestUrl);
                
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new ExerciseNotFoundException(exerciseId);
                    }
                    await HandleErrorResponseAsync(response, exerciseId);
                }

                links = await response.Content.ReadFromJsonAsync<ExerciseLinksResponseDto>(_jsonOptions);
                
                if (links != null)
                {
                    // Cache for 1 hour as specified in requirements
                    _cache.Set(cacheKey, links, TimeSpan.FromHours(1));
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
                var requestUrl = $"{_apiBaseUrl}/api/exercises/{exerciseId}/links/suggested{queryParams}";

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
            var requestUrl = $"{_apiBaseUrl}/api/exercises/{exerciseId}/links/{linkId}";
            
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
            var requestUrl = $"{_apiBaseUrl}/api/exercises/{exerciseId}/links/{linkId}";
            
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

    private string BuildLinksQueryString(string? linkType, bool includeExerciseDetails)
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
                    throw new InvalidExerciseLinkException("Invalid exercise type for linking. Only Workout exercises can have links");
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

    private void InvalidateExerciseLinksCache(string exerciseId)
    {
        // Remove all cached entries for this exercise's links
        var cacheKeys = new[]
        {
            $"exercise_links_{exerciseId}__False",
            $"exercise_links_{exerciseId}__True",
            $"exercise_links_{exerciseId}_Warmup_False",
            $"exercise_links_{exerciseId}_Warmup_True",
            $"exercise_links_{exerciseId}_Cooldown_False",
            $"exercise_links_{exerciseId}_Cooldown_True",
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