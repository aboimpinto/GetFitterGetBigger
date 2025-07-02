using GetFitterGetBigger.Admin.Models.Dtos;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace GetFitterGetBigger.Admin.Services
{
    public class MuscleGroupsService : IMuscleGroupsService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly string _apiBaseUrl;
        private readonly JsonSerializerOptions _jsonOptions;
        private const string CacheKey = "MuscleGroupsDto_Full";
        private const string CacheKeyPrefix = "MuscleGroups_BodyPart_";

        public MuscleGroupsService(HttpClient httpClient, IMemoryCache cache, IConfiguration configuration)
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

        public async Task<IEnumerable<MuscleGroupDto>> GetMuscleGroupsAsync()
        {
            if (!_cache.TryGetValue(CacheKey, out IEnumerable<MuscleGroupDto>? cachedData))
            {
                // Try the simple endpoint first (without pagination parameters)
                var requestUrl = $"{_apiBaseUrl}/api/ReferenceTables/MuscleGroups";
                var response = await _httpClient.GetAsync(requestUrl);
                
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Failed to get muscle groups: {response.StatusCode}");
                }

                // Log the raw response to debug the issue
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[GET MUSCLE GROUPS] Raw Response: {responseContent}");

                try
                {
                    // First try to deserialize as an array (simple format)
                    cachedData = JsonSerializer.Deserialize<IEnumerable<MuscleGroupDto>>(responseContent, _jsonOptions);
                    
                    if (cachedData != null)
                    {
                        _cache.Set(CacheKey, cachedData, TimeSpan.FromHours(24));
                    }
                }
                catch (JsonException)
                {
                    // If array deserialization fails, try paginated result
                    try
                    {
                        var pagedResult = JsonSerializer.Deserialize<MuscleGroupPagedResultDto>(responseContent, _jsonOptions);
                        
                        if (pagedResult != null)
                        {
                            cachedData = pagedResult.Items;
                            _cache.Set(CacheKey, cachedData, TimeSpan.FromHours(24));
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"[GET MUSCLE GROUPS] Deserialization error: {ex.Message}");
                        // Last attempt: try as ReferenceDataDto array (old format)
                        try
                        {
                            var referenceData = JsonSerializer.Deserialize<IEnumerable<ReferenceDataDto>>(responseContent, _jsonOptions);
                            if (referenceData != null)
                            {
                                cachedData = referenceData.Select(rd => new MuscleGroupDto
                                {
                                    Id = rd.Id,
                                    Name = rd.Value,
                                    BodyPartId = string.Empty,
                                    IsActive = true,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = null
                                }).ToList();
                                
                                _cache.Set(CacheKey, cachedData, TimeSpan.FromHours(24));
                            }
                        }
                        catch
                        {
                            throw new InvalidOperationException($"Failed to deserialize muscle groups response. Raw response: {responseContent}", ex);
                        }
                    }
                }
            }
            
            return cachedData ?? Enumerable.Empty<MuscleGroupDto>();
        }

        public async Task<IEnumerable<MuscleGroupDto>> GetMuscleGroupsByBodyPartAsync(string bodyPartId)
        {
            var cacheKey = $"{CacheKeyPrefix}{bodyPartId}";
            
            if (!_cache.TryGetValue(cacheKey, out IEnumerable<MuscleGroupDto>? cachedData))
            {
                var requestUrl = $"{_apiBaseUrl}/api/ReferenceTables/MuscleGroups/ByBodyPart/{bodyPartId}";
                var response = await _httpClient.GetAsync(requestUrl);
                
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Failed to get muscle groups by body part: {response.StatusCode}");
                }

                // The API now returns an array of MuscleGroupDto
                cachedData = await response.Content.ReadFromJsonAsync<IEnumerable<MuscleGroupDto>>(_jsonOptions);
                
                if (cachedData != null)
                {
                    _cache.Set(cacheKey, cachedData, TimeSpan.FromHours(24));
                }
            }
            
            return cachedData ?? Enumerable.Empty<MuscleGroupDto>();
        }

        public async Task<MuscleGroupDto> CreateMuscleGroupAsync(CreateMuscleGroupDto dto)
        {
            var requestUrl = $"{_apiBaseUrl}/api/ReferenceTables/MuscleGroups";
            
            // Log the request details
            Console.WriteLine($"[CREATE MUSCLE GROUP] URL: {requestUrl}");
            Console.WriteLine($"[CREATE MUSCLE GROUP] DTO: {JsonSerializer.Serialize(dto, _jsonOptions)}");
            
            var response = await _httpClient.PostAsJsonAsync(requestUrl, dto, _jsonOptions);

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Invalid input: {errorContent}");
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException("Body part not found");
            }

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                throw new InvalidOperationException("Muscle group with this name already exists for the selected body part");
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[CREATE MUSCLE GROUP] Error Response: {errorContent}");
                throw new HttpRequestException($"Failed to create muscle group: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[CREATE MUSCLE GROUP] Success Response: {responseContent}");
            
            // API returns the full MuscleGroupDto on create
            var created = JsonSerializer.Deserialize<MuscleGroupDto>(responseContent, _jsonOptions);
            
            // Invalidate all relevant caches
            _cache.Remove(CacheKey);
            _cache.Remove($"{CacheKeyPrefix}{dto.BodyPartId}");
            
            return created ?? throw new InvalidOperationException("Failed to deserialize created muscle group");
        }

        public async Task<MuscleGroupDto> UpdateMuscleGroupAsync(string id, UpdateMuscleGroupDto dto)
        {
            var requestUrl = $"{_apiBaseUrl}/api/ReferenceTables/MuscleGroups/{id}";
            
            // Log the request details
            Console.WriteLine($"[UPDATE MUSCLE GROUP] URL: {requestUrl}");
            Console.WriteLine($"[UPDATE MUSCLE GROUP] ID: {id}");
            Console.WriteLine($"[UPDATE MUSCLE GROUP] DTO: {JsonSerializer.Serialize(dto, _jsonOptions)}");
            
            var response = await _httpClient.PutAsJsonAsync(requestUrl, dto, _jsonOptions);

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Invalid input: {errorContent}");
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException("Muscle group or body part not found");
            }

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                throw new InvalidOperationException("Muscle group with this name already exists for the selected body part");
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[UPDATE MUSCLE GROUP] Error Response: {errorContent}");
                throw new HttpRequestException($"Failed to update muscle group: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[UPDATE MUSCLE GROUP] Success Response: {responseContent}");
            
            // API returns the full MuscleGroupDto on update
            var updated = JsonSerializer.Deserialize<MuscleGroupDto>(responseContent, _jsonOptions);
            
            // Invalidate all caches (including old and new body part caches)
            _cache.Remove(CacheKey);
            // Clear all body part caches since we don't track the old body part
            var cacheEntries = _cache.GetType()
                .GetField("_entries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(_cache);
            
            if (cacheEntries != null)
            {
                var keys = ((dynamic)cacheEntries).Keys;
                foreach (var key in keys)
                {
                    if (key.ToString().StartsWith(CacheKeyPrefix))
                    {
                        _cache.Remove(key);
                    }
                }
            }
            
            return updated ?? throw new InvalidOperationException("Failed to deserialize updated muscle group");
        }

        public async Task DeleteMuscleGroupAsync(string id)
        {
            var requestUrl = $"{_apiBaseUrl}/api/ReferenceTables/MuscleGroups/{id}";
            
            // Log the request details
            Console.WriteLine($"[DELETE MUSCLE GROUP] URL: {requestUrl}");
            Console.WriteLine($"[DELETE MUSCLE GROUP] ID: {id}");
            
            var response = await _httpClient.DeleteAsync(requestUrl);

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new InvalidOperationException("Invalid ID format");
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException("Muscle group not found");
            }

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                throw new InvalidOperationException("Cannot delete muscle group that is in use by exercises");
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DELETE MUSCLE GROUP] Error Response: {errorContent}");
                throw new HttpRequestException($"Failed to delete muscle group: {response.StatusCode}");
            }
            
            Console.WriteLine($"[DELETE MUSCLE GROUP] Success: Muscle group {id} deleted");

            // Invalidate all caches
            _cache.Remove(CacheKey);
            // Clear all body part caches
            var cacheEntries = _cache.GetType()
                .GetField("_entries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(_cache);
            
            if (cacheEntries != null)
            {
                var keys = ((dynamic)cacheEntries).Keys;
                foreach (var key in keys)
                {
                    if (key.ToString().StartsWith(CacheKeyPrefix))
                    {
                        _cache.Remove(key);
                    }
                }
            }
        }
    }
}