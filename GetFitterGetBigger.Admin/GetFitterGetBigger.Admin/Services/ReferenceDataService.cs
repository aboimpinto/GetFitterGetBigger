using GetFitterGetBigger.Admin.Models.Dtos;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Json;

namespace GetFitterGetBigger.Admin.Services
{
    public class ReferenceDataService : IReferenceDataService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly string _apiBaseUrl;

        public ReferenceDataService(HttpClient httpClient, IMemoryCache cache, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _cache = cache;
            _configuration = configuration;
            _apiBaseUrl = _configuration["ApiBaseUrl"] ?? string.Empty;
        }

        private async Task<IEnumerable<ReferenceDataDto>> GetDataAsync(string endpoint, string cacheKey)
        {
            if (!_cache.TryGetValue(cacheKey, out IEnumerable<ReferenceDataDto>? cachedData))
            {
                var requestUrl = $"{_apiBaseUrl}{endpoint}";
                cachedData = await _httpClient.GetFromJsonAsync<IEnumerable<ReferenceDataDto>>(requestUrl);
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
        public Task<IEnumerable<ReferenceDataDto>> GetExerciseTypesAsync() => GetDataAsync("/api/ReferenceTables/ExerciseTypes", "ExerciseTypes");
        public Task<IEnumerable<ReferenceDataDto>> GetKineticChainTypesAsync() => GetDataAsync("/api/ReferenceTables/KineticChainTypes", "KineticChainTypes");
        public Task<IEnumerable<ReferenceDataDto>> GetMetricTypesAsync() => GetDataAsync("/api/ReferenceTables/MetricTypes", "MetricTypes");
        public Task<IEnumerable<ReferenceDataDto>> GetMovementPatternsAsync() => GetDataAsync("/api/ReferenceTables/MovementPatterns", "MovementPatterns");
        public Task<IEnumerable<ReferenceDataDto>> GetMuscleGroupsAsync() => GetDataAsync("/api/ReferenceTables/MuscleGroups", "MuscleGroups");
        public Task<IEnumerable<ReferenceDataDto>> GetMuscleRolesAsync() => GetDataAsync("/api/ReferenceTables/MuscleRoles", "MuscleRoles");
    }
}
