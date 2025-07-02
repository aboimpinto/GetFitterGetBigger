using GetFitterGetBigger.Admin.Models.Dtos;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Web;

namespace GetFitterGetBigger.Admin.Services
{
    public class ExerciseService : IExerciseService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly string _apiBaseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public ExerciseService(HttpClient httpClient, IMemoryCache cache, IConfiguration configuration)
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

        public async Task<ExercisePagedResultDto> GetExercisesAsync(ExerciseFilterDto filter)
        {
            var queryParams = BuildQueryString(filter);
            var requestUrl = $"{_apiBaseUrl}/api/exercises{queryParams}";

            var response = await _httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ExercisePagedResultDto>(_jsonOptions);
            return result ?? new ExercisePagedResultDto();
        }

        public async Task<ExerciseDto?> GetExerciseByIdAsync(string id)
        {
            var cacheKey = $"exercise_{id}";

            if (!_cache.TryGetValue(cacheKey, out ExerciseDto? exercise))
            {
                var requestUrl = $"{_apiBaseUrl}/api/exercises/{id}";
                var response = await _httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    exercise = await response.Content.ReadFromJsonAsync<ExerciseDto>(_jsonOptions);
                    if (exercise != null)
                    {
                        _cache.Set(cacheKey, exercise, TimeSpan.FromMinutes(5));
                    }
                }
            }

            return exercise;
        }

        public async Task<ExerciseDto> CreateExerciseAsync(ExerciseCreateDto exercise)
        {
            try
            {
                var requestUrl = $"{_apiBaseUrl}/api/exercises";
                var json = JsonSerializer.Serialize(exercise, _jsonOptions);

                Console.WriteLine($"[ExerciseService] Creating exercise at URL: {requestUrl}");
                Console.WriteLine($"[ExerciseService] Request JSON:");
                Console.WriteLine(json);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(requestUrl, content);

                Console.WriteLine($"[ExerciseService] Response Status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[ExerciseService] Error Response Body:");
                    Console.WriteLine(errorContent);
                }

                response.EnsureSuccessStatusCode();

                var created = await response.Content.ReadFromJsonAsync<ExerciseDto>(_jsonOptions);
                return created ?? throw new InvalidOperationException("Failed to create exercise");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ExerciseService] Exception in CreateExerciseAsync: {ex.GetType().Name}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[ExerciseService] Inner Exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public async Task UpdateExerciseAsync(string id, ExerciseUpdateDto exercise)
        {
            var requestUrl = $"{_apiBaseUrl}/api/exercises/{id}";
            var content = new StringContent(
                JsonSerializer.Serialize(exercise, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PutAsync(requestUrl, content);
            response.EnsureSuccessStatusCode();

            // Clear cache for this exercise
            _cache.Remove($"exercise_{id}");
        }

        public async Task DeleteExerciseAsync(string id)
        {
            var requestUrl = $"{_apiBaseUrl}/api/exercises/{id}";
            var response = await _httpClient.DeleteAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            // Clear cache for this exercise
            _cache.Remove($"exercise_{id}");
        }

        private string BuildQueryString(ExerciseFilterDto filter)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);

            query["page"] = filter.Page.ToString();
            query["pageSize"] = filter.PageSize.ToString();

            if (!string.IsNullOrWhiteSpace(filter.Name))
                query["name"] = filter.Name;

            if (!string.IsNullOrWhiteSpace(filter.DifficultyId))
                query["difficultyId"] = filter.DifficultyId;

            if (filter.IsActive.HasValue)
                query["isActive"] = filter.IsActive.Value.ToString();

            if (filter.MuscleGroupIds?.Any() == true)
            {
                foreach (var id in filter.MuscleGroupIds)
                {
                    query.Add("muscleGroupIds", id);
                }
            }

            if (filter.EquipmentIds?.Any() == true)
            {
                foreach (var id in filter.EquipmentIds)
                {
                    query.Add("equipmentIds", id);
                }
            }

            return query.ToString() == "" ? "" : "?" + query.ToString();
        }
    }
}