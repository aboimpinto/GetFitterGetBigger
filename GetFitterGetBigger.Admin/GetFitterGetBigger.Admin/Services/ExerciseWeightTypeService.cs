using GetFitterGetBigger.Admin.Models.Dtos;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Json;
using System.Text.Json;

namespace GetFitterGetBigger.Admin.Services
{
    public class ExerciseWeightTypeService : IExerciseWeightTypeService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly string _apiBaseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public ExerciseWeightTypeService(HttpClient httpClient, IMemoryCache cache, IConfiguration configuration)
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

        public async Task<IEnumerable<ExerciseWeightTypeDto>> GetWeightTypesAsync()
        {
            const string cacheKey = "exercise_weight_types";

            if (_cache.TryGetValue(cacheKey, out IEnumerable<ExerciseWeightTypeDto>? cachedWeightTypes))
            {
                return cachedWeightTypes ?? Enumerable.Empty<ExerciseWeightTypeDto>();
            }

            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/exercise-weight-types");

                if (response.IsSuccessStatusCode)
                {
                    var weightTypes = await response.Content.ReadFromJsonAsync<IEnumerable<ExerciseWeightTypeDto>>(_jsonOptions)
                        ?? Enumerable.Empty<ExerciseWeightTypeDto>();

                    // Cache for 30 minutes since this is reference data
                    _cache.Set(cacheKey, weightTypes, TimeSpan.FromMinutes(30));

                    return weightTypes;
                }

                response.EnsureSuccessStatusCode();
                return Enumerable.Empty<ExerciseWeightTypeDto>();
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException($"Failed to retrieve exercise weight types: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while retrieving exercise weight types: {ex.Message}", ex);
            }
        }

        public async Task<ExerciseWeightTypeDto?> GetWeightTypeByIdAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/exercise-weight-types/{id}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ExerciseWeightTypeDto>(_jsonOptions);
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                response.EnsureSuccessStatusCode();
                return null;
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException($"Failed to retrieve exercise weight type with ID {id}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while retrieving exercise weight type with ID {id}: {ex.Message}", ex);
            }
        }
    }
}