using System.Net.Http.Json;
using System.Text.Json;
using GetFitterGetBigger.Admin.Models.Dtos;
using Microsoft.Extensions.Caching.Memory;

namespace GetFitterGetBigger.Admin.Services;

public class WorkoutReferenceDataService : IWorkoutReferenceDataService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<WorkoutReferenceDataService> _logger;
    
    private const string WorkoutObjectivesCacheKey = "WorkoutObjectives";
    private const string WorkoutCategoriesCacheKey = "WorkoutCategories";
    private const string ExecutionProtocolsCacheKey = "ExecutionProtocols";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);
    private const int MaxRetries = 3;

    public WorkoutReferenceDataService(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<WorkoutReferenceDataService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<ReferenceDataDto>> GetWorkoutObjectivesAsync()
    {
        _logger.LogInformation("Getting workout objectives...");
        _logger.LogInformation("HttpClient BaseAddress: {BaseAddress}", _httpClient.BaseAddress);
        
        return await GetCachedDataAsync(
            WorkoutObjectivesCacheKey,
            async () =>
            {
                var endpoint = "/api/ReferenceTables/WorkoutObjectives";
                _logger.LogInformation("Fetching workout objectives from endpoint: {Endpoint}", endpoint);
                
                var response = await ExecuteWithRetryAsync(
                    async () => await _httpClient.GetAsync(endpoint));
                
                response.EnsureSuccessStatusCode();
                var data = await response.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
                _logger.LogInformation("Successfully fetched {Count} workout objectives", data?.Count ?? 0);
                return data ?? new List<ReferenceDataDto>();
            });
    }

    public async Task<ReferenceDataDto?> GetWorkoutObjectiveByIdAsync(string id)
    {
        var objectives = await GetWorkoutObjectivesAsync();
        return objectives.FirstOrDefault(o => o.Id == id);
    }

    public async Task<List<WorkoutCategoryDto>> GetWorkoutCategoriesAsync()
    {
        return await GetCachedDataAsync(
            WorkoutCategoriesCacheKey,
            async () =>
            {
                var response = await ExecuteWithRetryAsync(
                    async () => await _httpClient.GetAsync("/api/ReferenceTables/WorkoutCategories"));
                
                response.EnsureSuccessStatusCode();
                var wrapper = await response.Content.ReadFromJsonAsync<WorkoutCategoriesResponseDto>();
                return wrapper?.WorkoutCategories ?? new List<WorkoutCategoryDto>();
            });
    }

    public async Task<WorkoutCategoryDto?> GetWorkoutCategoryByIdAsync(string id)
    {
        var categories = await GetWorkoutCategoriesAsync();
        return categories.FirstOrDefault(c => c.WorkoutCategoryId == id);
    }

    public async Task<List<ExecutionProtocolDto>> GetExecutionProtocolsAsync()
    {
        return await GetCachedDataAsync(
            ExecutionProtocolsCacheKey,
            async () =>
            {
                var response = await ExecuteWithRetryAsync(
                    async () => await _httpClient.GetAsync("/api/ReferenceTables/ExecutionProtocols"));
                
                response.EnsureSuccessStatusCode();
                
                // Read content as string first to avoid stream already read issues
                var content = await response.Content.ReadAsStringAsync();
                
                // First try to deserialize as ExecutionProtocolDto list
                try
                {
                    var data = JsonSerializer.Deserialize<List<ExecutionProtocolDto>>(content, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return data ?? new List<ExecutionProtocolDto>();
                }
                catch
                {
                    // If that fails, try as ReferenceDataDto and map
                    _logger.LogWarning("ExecutionProtocols returned ReferenceDataDto format, mapping to ExecutionProtocolDto");
                    var referenceData = JsonSerializer.Deserialize<List<ReferenceDataDto>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return referenceData?.Select(rd => new ExecutionProtocolDto
                    {
                        ExecutionProtocolId = rd.Id,
                        Code = rd.Value, // Using Value as Code for now
                        Value = rd.Value,
                        Description = rd.Description,
                        DisplayOrder = 0,
                        IsActive = true,
                        TimeBase = false,
                        RepBase = true,
                        RestPattern = null,
                        IntensityLevel = null
                    }).ToList() ?? new List<ExecutionProtocolDto>();
                }
            });
    }

    public async Task<ExecutionProtocolDto?> GetExecutionProtocolByIdAsync(string id)
    {
        var protocols = await GetExecutionProtocolsAsync();
        return protocols.FirstOrDefault(p => p.ExecutionProtocolId == id);
    }

    public async Task<ExecutionProtocolDto?> GetExecutionProtocolByValueAsync(string value)
    {
        try
        {
            var response = await ExecuteWithRetryAsync(
                async () => await _httpClient.GetAsync($"/api/ReferenceTables/ExecutionProtocols/ByValue/{value}"));
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;
                
                response.EnsureSuccessStatusCode();
            }
            
            // Try to deserialize as ExecutionProtocolDto first
            try
            {
                return await response.Content.ReadFromJsonAsync<ExecutionProtocolDto>();
            }
            catch
            {
                // If that fails, try as ReferenceDataDto and map
                var referenceData = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
                if (referenceData == null) return null;
                
                return new ExecutionProtocolDto
                {
                    ExecutionProtocolId = referenceData.Id,
                    Code = value,
                    Value = referenceData.Value,
                    Description = referenceData.Description,
                    DisplayOrder = 0,
                    IsActive = true,
                    TimeBase = false,
                    RepBase = true,
                    RestPattern = null,
                    IntensityLevel = null
                };
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching execution protocol by value: {Value}", value);
            return null;
        }
    }

    private async Task<T> GetCachedDataAsync<T>(string cacheKey, Func<Task<T>> fetchData)
    {
        if (_cache.TryGetValue(cacheKey, out T? cachedData) && cachedData != null)
        {
            _logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
            return cachedData;
        }

        _logger.LogDebug("Cache miss for {CacheKey}, fetching from API", cacheKey);
        var data = await fetchData();
        
        _cache.Set(cacheKey, data, CacheDuration);
        return data;
    }

    private async Task<HttpResponseMessage> ExecuteWithRetryAsync(Func<Task<HttpResponseMessage>> operation)
    {
        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            try
            {
                _logger.LogDebug("Executing HTTP request, attempt {Attempt} of {MaxRetries}", attempt + 1, MaxRetries);
                var response = await operation();
                
                _logger.LogDebug("HTTP response: {StatusCode} {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                
                // If successful or a client error (4xx), return immediately
                if (response.IsSuccessStatusCode || (int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                {
                    return response;
                }
                
                // For server errors (5xx), retry
                if (attempt < MaxRetries - 1)
                {
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                    _logger.LogWarning("Request failed with {StatusCode}. Retrying in {Delay}ms (attempt {Attempt}/{MaxRetries})",
                        response.StatusCode, delay.TotalMilliseconds, attempt + 1, MaxRetries);
                    await Task.Delay(delay);
                }
                else
                {
                    return response; // Return the failed response on last attempt
                }
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("invalid request URI"))
            {
                _logger.LogError(ex, "Invalid request URI. BaseAddress: {BaseAddress}. This usually means BaseAddress is not set on HttpClient", 
                    _httpClient.BaseAddress?.ToString() ?? "NULL");
                throw;
            }
            catch (HttpRequestException ex) when (attempt < MaxRetries - 1)
            {
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                _logger.LogWarning(ex, "Request failed with exception. Retrying in {Delay}ms (attempt {Attempt}/{MaxRetries})",
                    delay.TotalMilliseconds, attempt + 1, MaxRetries);
                await Task.Delay(delay);
            }
        }
        
        // This should never be reached due to the logic above, but just in case
        throw new HttpRequestException("Max retries exceeded");
    }
}