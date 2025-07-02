using GetFitterGetBigger.Admin.Models.Dtos;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Json;
using System.Text.Json;

namespace GetFitterGetBigger.Admin.Services
{
    public class ReferenceDataService : IReferenceDataService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly string _apiBaseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public ReferenceDataService(HttpClient httpClient, IMemoryCache cache, IConfiguration configuration)
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

        private async Task<IEnumerable<ReferenceDataDto>> GetDataAsync(string endpoint, string cacheKey)
        {
            try
            {
                if (!_cache.TryGetValue(cacheKey, out IEnumerable<ReferenceDataDto>? cachedData))
                {
                    Console.WriteLine($"[ReferenceDataService] Cache miss for {cacheKey}, fetching from API");
                    // Use relative URL since HttpClient has BaseAddress configured
                    var requestUrl = endpoint.StartsWith("/") ? endpoint.Substring(1) : endpoint;
                    cachedData = await _httpClient.GetFromJsonAsync<IEnumerable<ReferenceDataDto>>(requestUrl, _jsonOptions);
                    if (cachedData != null)
                    {
                        _cache.Set(cacheKey, cachedData, TimeSpan.FromHours(24));
                        Console.WriteLine($"[ReferenceDataService] Cached {cachedData.Count()} items for {cacheKey}");
                    }
                }
                return cachedData ?? Enumerable.Empty<ReferenceDataDto>();
            }
            catch (InvalidCastException ex)
            {
                Console.WriteLine($"[ReferenceDataService] InvalidCastException for {cacheKey}: {ex.Message}");
                // Cache contains data of wrong type - clear it and retry
                _cache.Remove(cacheKey);
                var requestUrl = endpoint.StartsWith("/") ? endpoint.Substring(1) : endpoint;
                var cachedData = await _httpClient.GetFromJsonAsync<IEnumerable<ReferenceDataDto>>(requestUrl, _jsonOptions);
                if (cachedData != null)
                {
                    _cache.Set(cacheKey, cachedData, TimeSpan.FromHours(24));
                }
                return cachedData ?? Enumerable.Empty<ReferenceDataDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReferenceDataService] ERROR for {cacheKey}: {ex}");
                throw;
            }
        }

        public Task<IEnumerable<ReferenceDataDto>> GetBodyPartsAsync() => GetDataAsync("/api/ReferenceTables/BodyParts", "RefData_BodyParts");
        public Task<IEnumerable<ReferenceDataDto>> GetDifficultyLevelsAsync() => GetDataAsync("/api/ReferenceTables/DifficultyLevels", "RefData_DifficultyLevels");
        public Task<IEnumerable<ReferenceDataDto>> GetEquipmentAsync()
        {
            return GetDataAsync("/api/ReferenceTables/Equipment", "RefData_Equipment");
        }
        public Task<IEnumerable<ReferenceDataDto>> GetKineticChainTypesAsync() => GetDataAsync("/api/ReferenceTables/KineticChainTypes", "RefData_KineticChainTypes");
        public Task<IEnumerable<ReferenceDataDto>> GetMetricTypesAsync() => GetDataAsync("/api/ReferenceTables/MetricTypes", "RefData_MetricTypes");
        public Task<IEnumerable<ReferenceDataDto>> GetMovementPatternsAsync() => GetDataAsync("/api/ReferenceTables/MovementPatterns", "RefData_MovementPatterns");
        public async Task<IEnumerable<ReferenceDataDto>> GetMuscleGroupsAsync()
        {
            const string cacheKey = "RefData_MuscleGroups";
            try
            {
                if (!_cache.TryGetValue(cacheKey, out IEnumerable<ReferenceDataDto>? cachedData))
                {
                    Console.WriteLine($"[ReferenceDataService] Cache miss for {cacheKey}, fetching from API");
                    var requestUrl = "/api/ReferenceTables/MuscleGroups";

                    // MuscleGroups endpoint returns MuscleGroupDto, not ReferenceDataDto
                    var muscleGroups = await _httpClient.GetFromJsonAsync<IEnumerable<MuscleGroupDto>>(requestUrl, _jsonOptions);
                    if (muscleGroups != null)
                    {
                        // Convert MuscleGroupDto to ReferenceDataDto
                        cachedData = muscleGroups.Select(mg => new ReferenceDataDto
                        {
                            Id = mg.Id,
                            Value = mg.Name,
                            Description = $"Body Part: {mg.BodyPartName ?? "Unknown"}"
                        }).ToList();

                        _cache.Set(cacheKey, cachedData, TimeSpan.FromHours(24));
                        Console.WriteLine($"[ReferenceDataService] Cached {cachedData.Count()} items for {cacheKey}");
                    }
                }
                return cachedData ?? Enumerable.Empty<ReferenceDataDto>();
            }
            catch (InvalidCastException ex)
            {
                Console.WriteLine($"[ReferenceDataService] InvalidCastException in GetMuscleGroupsAsync: {ex.Message}");
                // Cache contains data of wrong type - clear it and retry
                _cache.Remove(cacheKey);
                _cache.Remove("MuscleGroupsDto_Full"); // Clear related cache
                
                var requestUrl = "api/ReferenceTables/MuscleGroups";
                var muscleGroups = await _httpClient.GetFromJsonAsync<IEnumerable<MuscleGroupDto>>(requestUrl, _jsonOptions);
                if (muscleGroups != null)
                {
                    var cachedData = muscleGroups.Select(mg => new ReferenceDataDto
                    {
                        Id = mg.Id,
                        Value = mg.Name,
                        Description = $"Body Part: {mg.BodyPartName ?? "Unknown"}"
                    }).ToList();

                    _cache.Set(cacheKey, cachedData, TimeSpan.FromHours(24));
                    return cachedData;
                }
                return Enumerable.Empty<ReferenceDataDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReferenceDataService] ERROR in GetMuscleGroupsAsync: {ex}");
                throw;
            }
        }
        public Task<IEnumerable<ReferenceDataDto>> GetMuscleRolesAsync() => GetDataAsync("/api/ReferenceTables/MuscleRoles", "RefData_MuscleRoles");

        public void ClearEquipmentCache()
        {
            _cache.Remove("RefData_Equipment");
            Console.WriteLine("[ReferenceDataService] Cleared cache: RefData_Equipment");
        }

        public void ClearMuscleGroupsCache()
        {
            _cache.Remove("RefData_MuscleGroups");
            _cache.Remove("MuscleGroupsDto_Full"); // Also clear MuscleGroupsService cache
            _cache.Remove("MuscleGroups"); // Legacy key
            Console.WriteLine("[ReferenceDataService] Cleared caches: RefData_MuscleGroups, MuscleGroupsDto_Full, MuscleGroups");
        }

        public async Task<IEnumerable<ExerciseTypeDto>> GetExerciseTypesAsync()
        {
            if (!_cache.TryGetValue("RefData_ExerciseTypes", out IEnumerable<ExerciseTypeDto>? cachedData))
            {
                var requestUrl = "/api/ReferenceTables/ExerciseTypes";
                var referenceData = await _httpClient.GetFromJsonAsync<IEnumerable<ReferenceDataDto>>(requestUrl, _jsonOptions);
                if (referenceData != null)
                {
                    // Convert ReferenceDataDto to ExerciseTypeDto
                    cachedData = referenceData.Select(rd => new ExerciseTypeDto
                    {
                        Id = rd.Id,
                        Value = rd.Value,
                        Description = rd.Description ?? string.Empty
                    });
                    _cache.Set("RefData_ExerciseTypes", cachedData, TimeSpan.FromHours(24));
                }
            }
            return cachedData ?? Enumerable.Empty<ExerciseTypeDto>();
        }
    }
}
