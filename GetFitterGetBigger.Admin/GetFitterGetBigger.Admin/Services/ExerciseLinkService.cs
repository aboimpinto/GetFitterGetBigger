using GetFitterGetBigger.Admin.Models.Dtos;
using Microsoft.Extensions.Caching.Memory;
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
        var requestUrl = $"{_apiBaseUrl}/api/exercises/{exerciseId}/links";
        
        var response = await _httpClient.PostAsJsonAsync(requestUrl, createLinkDto, _jsonOptions);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ExerciseLinkDto>(_jsonOptions);
        
        // Invalidate cache for the exercise links
        InvalidateExerciseLinksCache(exerciseId);
        
        return result ?? throw new InvalidOperationException("Failed to create exercise link");
    }

    public async Task<ExerciseLinksResponseDto> GetLinksAsync(string exerciseId, string? linkType = null, bool includeExerciseDetails = false)
    {
        var cacheKey = $"exercise_links_{exerciseId}_{linkType}_{includeExerciseDetails}";
        
        if (!_cache.TryGetValue(cacheKey, out ExerciseLinksResponseDto? links))
        {
            var queryParams = BuildLinksQueryString(linkType, includeExerciseDetails);
            var requestUrl = $"{_apiBaseUrl}/api/exercises/{exerciseId}/links{queryParams}";

            var response = await _httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            links = await response.Content.ReadFromJsonAsync<ExerciseLinksResponseDto>(_jsonOptions);
            
            if (links != null)
            {
                // Cache for 1 hour as specified in requirements
                _cache.Set(cacheKey, links, TimeSpan.FromHours(1));
            }
        }

        return links ?? new ExerciseLinksResponseDto();
    }

    public async Task<List<ExerciseLinkDto>> GetSuggestedLinksAsync(string exerciseId, int count = 5)
    {
        var cacheKey = $"suggested_links_{exerciseId}_{count}";
        
        if (!_cache.TryGetValue(cacheKey, out List<ExerciseLinkDto>? suggestions))
        {
            var queryParams = count != 5 ? $"?count={count}" : "";
            var requestUrl = $"{_apiBaseUrl}/api/exercises/{exerciseId}/links/suggested{queryParams}";

            var response = await _httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            suggestions = await response.Content.ReadFromJsonAsync<List<ExerciseLinkDto>>(_jsonOptions);
            
            if (suggestions != null)
            {
                // Cache suggestions for 30 minutes
                _cache.Set(cacheKey, suggestions, TimeSpan.FromMinutes(30));
            }
        }

        return suggestions ?? new List<ExerciseLinkDto>();
    }

    public async Task<ExerciseLinkDto> UpdateLinkAsync(string exerciseId, string linkId, UpdateExerciseLinkDto updateLinkDto)
    {
        var requestUrl = $"{_apiBaseUrl}/api/exercises/{exerciseId}/links/{linkId}";
        
        var response = await _httpClient.PutAsJsonAsync(requestUrl, updateLinkDto, _jsonOptions);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ExerciseLinkDto>(_jsonOptions);
        
        // Invalidate cache for the exercise links
        InvalidateExerciseLinksCache(exerciseId);
        
        return result ?? throw new InvalidOperationException("Failed to update exercise link");
    }

    public async Task DeleteLinkAsync(string exerciseId, string linkId)
    {
        var requestUrl = $"{_apiBaseUrl}/api/exercises/{exerciseId}/links/{linkId}";
        
        var response = await _httpClient.DeleteAsync(requestUrl);
        response.EnsureSuccessStatusCode();
        
        // Invalidate cache for the exercise links
        InvalidateExerciseLinksCache(exerciseId);
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