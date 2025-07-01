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
            if (!_cache.TryGetValue(cacheKey, out IEnumerable<ReferenceDataDto>? cachedData))
            {
                var requestUrl = $"{_apiBaseUrl}{endpoint}";
                cachedData = await _httpClient.GetFromJsonAsync<IEnumerable<ReferenceDataDto>>(requestUrl, _jsonOptions);
                if (cachedData != null)
                {
                    _cache.Set(cacheKey, cachedData, TimeSpan.FromHours(24));
                }
            }
            return cachedData ?? Enumerable.Empty<ReferenceDataDto>();
        }

        public Task<IEnumerable<ReferenceDataDto>> GetBodyPartsAsync() => GetDataAsync("/api/ReferenceTables/BodyParts", "BodyParts");
        public Task<IEnumerable<ReferenceDataDto>> GetDifficultyLevelsAsync() => GetDataAsync("/api/ReferenceTables/DifficultyLevels", "DifficultyLevels");
        public Task<IEnumerable<ReferenceDataDto>> GetEquipmentAsync() => GetDataAsync("/api/ReferenceTables/Equipment", "Equipment");
        public Task<IEnumerable<ReferenceDataDto>> GetKineticChainTypesAsync() => GetDataAsync("/api/ReferenceTables/KineticChainTypes", "KineticChainTypes");
        public Task<IEnumerable<ReferenceDataDto>> GetMetricTypesAsync() => GetDataAsync("/api/ReferenceTables/MetricTypes", "MetricTypes");
        public Task<IEnumerable<ReferenceDataDto>> GetMovementPatternsAsync() => GetDataAsync("/api/ReferenceTables/MovementPatterns", "MovementPatterns");
        public Task<IEnumerable<ReferenceDataDto>> GetMuscleGroupsAsync() => GetDataAsync("/api/ReferenceTables/MuscleGroups", "MuscleGroups");
        public Task<IEnumerable<ReferenceDataDto>> GetMuscleRolesAsync() => GetDataAsync("/api/ReferenceTables/MuscleRoles", "MuscleRoles");
        
        public async Task<IEnumerable<ExerciseTypeDto>> GetExerciseTypesAsync()
        {
            if (!_cache.TryGetValue("ExerciseTypes", out IEnumerable<ExerciseTypeDto>? cachedData))
            {
                var requestUrl = $"{_apiBaseUrl}/api/ReferenceTables/ExerciseTypes";
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
                    _cache.Set("ExerciseTypes", cachedData, TimeSpan.FromHours(24));
                }
            }
            return cachedData ?? Enumerable.Empty<ExerciseTypeDto>();
        }
    }
}
