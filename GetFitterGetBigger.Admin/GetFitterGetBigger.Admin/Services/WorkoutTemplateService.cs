using GetFitterGetBigger.Admin.Models.Dtos;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Web;

namespace GetFitterGetBigger.Admin.Services
{
    public class WorkoutTemplateService : IWorkoutTemplateService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly JsonSerializerOptions _jsonOptions;

        public WorkoutTemplateService(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<WorkoutTemplatePagedResultDto> GetWorkoutTemplatesAsync(WorkoutTemplateFilterDto filter)
        {
            try
            {
                var queryParams = BuildQueryString(filter);
                var requestUrl = $"api/workout-templates{queryParams}";

                Console.WriteLine($"[WorkoutTemplateService] GetWorkoutTemplatesAsync - Request URL: {requestUrl}");

                var response = await _httpClient.GetAsync(requestUrl);

                Console.WriteLine($"[WorkoutTemplateService] GetWorkoutTemplatesAsync - Response Status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[WorkoutTemplateService] GetWorkoutTemplatesAsync - Error Response Body:");
                    Console.WriteLine(errorContent);
                }

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<WorkoutTemplatePagedResultDto>(json, _jsonOptions);

                Console.WriteLine($"[WorkoutTemplateService] GetWorkoutTemplatesAsync - Response Items Count: {result?.Items?.Count ?? 0}");

                return result ?? new WorkoutTemplatePagedResultDto();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WorkoutTemplateService] Exception in GetWorkoutTemplatesAsync: {ex.GetType().Name}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[WorkoutTemplateService] Inner Exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public async Task<WorkoutTemplateDto?> GetWorkoutTemplateByIdAsync(string id)
        {
            var cacheKey = $"workout_template_{id}";

            if (!_cache.TryGetValue(cacheKey, out WorkoutTemplateDto? template))
            {
                var requestUrl = $"api/workout-templates/{id}";
                var response = await _httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    template = await response.Content.ReadFromJsonAsync<WorkoutTemplateDto>(_jsonOptions);
                    if (template != null)
                    {
                        Console.WriteLine($"[WorkoutTemplateService] Loaded template '{template.Name}' with ID: {template.Id}");
                        _cache.Set(cacheKey, template, TimeSpan.FromMinutes(5));
                    }
                }
            }

            return template;
        }

        public async Task<WorkoutTemplateDto> CreateWorkoutTemplateAsync(CreateWorkoutTemplateDto template)
        {
            try
            {
                var requestUrl = "api/workout-templates";
                var json = JsonSerializer.Serialize(template, _jsonOptions);

                Console.WriteLine($"[WorkoutTemplateService] Creating template at URL: {requestUrl}");
                Console.WriteLine($"[WorkoutTemplateService] Request JSON:");
                Console.WriteLine(json);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(requestUrl, content);

                Console.WriteLine($"[WorkoutTemplateService] Response Status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[WorkoutTemplateService] Error Response Body:");
                    Console.WriteLine(errorContent);
                }

                response.EnsureSuccessStatusCode();

                var created = await response.Content.ReadFromJsonAsync<WorkoutTemplateDto>(_jsonOptions);
                return created ?? throw new InvalidOperationException("Failed to create workout template");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WorkoutTemplateService] Exception in CreateWorkoutTemplateAsync: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }

        public async Task<WorkoutTemplateDto> UpdateWorkoutTemplateAsync(string id, UpdateWorkoutTemplateDto template)
        {
            try
            {
                var requestUrl = $"api/workout-templates/{id}";
                var json = JsonSerializer.Serialize(template, _jsonOptions);

                Console.WriteLine($"[WorkoutTemplateService] Updating template at URL: {requestUrl}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync(requestUrl, content);

                Console.WriteLine($"[WorkoutTemplateService] Response Status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[WorkoutTemplateService] Error Response Body:");
                    Console.WriteLine(errorContent);
                }

                response.EnsureSuccessStatusCode();

                // Clear cache for this template
                var cacheKey = $"workout_template_{id}";
                _cache.Remove(cacheKey);

                var updated = await response.Content.ReadFromJsonAsync<WorkoutTemplateDto>(_jsonOptions);
                return updated ?? throw new InvalidOperationException("Failed to update workout template");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WorkoutTemplateService] Exception in UpdateWorkoutTemplateAsync: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteWorkoutTemplateAsync(string id)
        {
            try
            {
                var requestUrl = $"api/workout-templates/{id}";
                var response = await _httpClient.DeleteAsync(requestUrl);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[WorkoutTemplateService] Error deleting template: {errorContent}");
                }

                response.EnsureSuccessStatusCode();

                // Clear cache for this template
                var cacheKey = $"workout_template_{id}";
                _cache.Remove(cacheKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WorkoutTemplateService] Exception in DeleteWorkoutTemplateAsync: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }

        public async Task<WorkoutTemplateDto> ChangeWorkoutTemplateStateAsync(string id, ChangeWorkoutStateDto changeState)
        {
            try
            {
                var requestUrl = $"api/workout-templates/{id}/state";
                var json = JsonSerializer.Serialize(changeState, _jsonOptions);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync(requestUrl, content);

                Console.WriteLine($"[WorkoutTemplateService] Change state response: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[WorkoutTemplateService] Error changing state: {errorContent}");
                }

                response.EnsureSuccessStatusCode();

                // Clear cache for this template
                var cacheKey = $"workout_template_{id}";
                _cache.Remove(cacheKey);

                var updated = await response.Content.ReadFromJsonAsync<WorkoutTemplateDto>(_jsonOptions);
                return updated ?? throw new InvalidOperationException("Failed to change workout template state");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WorkoutTemplateService] Exception in ChangeWorkoutTemplateStateAsync: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }

        public async Task<WorkoutTemplateDto> DuplicateWorkoutTemplateAsync(string id, DuplicateWorkoutTemplateDto duplicate)
        {
            try
            {
                var requestUrl = $"api/workout-templates/{id}/duplicate";
                var json = JsonSerializer.Serialize(duplicate, _jsonOptions);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(requestUrl, content);

                Console.WriteLine($"[WorkoutTemplateService] Duplicate response: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[WorkoutTemplateService] Error duplicating template: {errorContent}");
                }

                response.EnsureSuccessStatusCode();

                var created = await response.Content.ReadFromJsonAsync<WorkoutTemplateDto>(_jsonOptions);
                return created ?? throw new InvalidOperationException("Failed to duplicate workout template");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WorkoutTemplateService] Exception in DuplicateWorkoutTemplateAsync: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }

        public async Task<List<WorkoutTemplateDto>> SearchTemplatesByNameAsync(string namePattern)
        {
            try
            {
                var encodedPattern = HttpUtility.UrlEncode(namePattern);
                var requestUrl = $"api/workout-templates/search?namePattern={encodedPattern}";

                var response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                var templates = await response.Content.ReadFromJsonAsync<List<WorkoutTemplateDto>>(_jsonOptions);
                return templates ?? new List<WorkoutTemplateDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WorkoutTemplateService] Exception in SearchTemplatesByNameAsync: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }

        public async Task<List<WorkoutTemplateDto>> GetTemplatesByCategoryAsync(string categoryId)
        {
            try
            {
                var requestUrl = $"api/workout-templates/filter/category/{categoryId}";
                var response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                var templates = await response.Content.ReadFromJsonAsync<List<WorkoutTemplateDto>>(_jsonOptions);
                return templates ?? new List<WorkoutTemplateDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WorkoutTemplateService] Exception in GetTemplatesByCategoryAsync: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }

        public async Task<List<WorkoutTemplateDto>> GetTemplatesByDifficultyAsync(string difficultyId)
        {
            try
            {
                var requestUrl = $"api/workout-templates/filter/difficulty/{difficultyId}";
                var response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                var templates = await response.Content.ReadFromJsonAsync<List<WorkoutTemplateDto>>(_jsonOptions);
                return templates ?? new List<WorkoutTemplateDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WorkoutTemplateService] Exception in GetTemplatesByDifficultyAsync: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }

        public async Task<List<WorkoutTemplateDto>> GetTemplatesByStateAsync(string stateId)
        {
            try
            {
                var requestUrl = $"api/workout-templates/by-state/{stateId}";
                var response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                var templates = await response.Content.ReadFromJsonAsync<List<WorkoutTemplateDto>>(_jsonOptions);
                return templates ?? new List<WorkoutTemplateDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WorkoutTemplateService] Exception in GetTemplatesByStateAsync: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }

        public async Task<List<WorkoutTemplateExerciseDto>> GetTemplateExercisesAsync(string templateId)
        {
            try
            {
                var requestUrl = $"api/workout-templates/{templateId}/exercises";
                var response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                var exercises = await response.Content.ReadFromJsonAsync<List<WorkoutTemplateExerciseDto>>(_jsonOptions);
                return exercises ?? new List<WorkoutTemplateExerciseDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WorkoutTemplateService] Exception in GetTemplateExercisesAsync: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> CheckTemplateNameExistsAsync(string name)
        {
            try
            {
                var encodedName = HttpUtility.UrlEncode(name);
                var requestUrl = $"api/workout-templates/exists/name?name={encodedName}";

                var response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                var exists = await response.Content.ReadFromJsonAsync<bool>(_jsonOptions);
                return exists;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WorkoutTemplateService] Exception in CheckTemplateNameExistsAsync: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }

        // Reference data methods
        public async Task<List<ReferenceDataDto>> GetWorkoutCategoriesAsync()
        {
            var cacheKey = "workout_categories";
            
            if (!_cache.TryGetValue(cacheKey, out List<ReferenceDataDto>? categories))
            {
                var response = await _httpClient.GetAsync("api/workout-templates/categories");
                response.EnsureSuccessStatusCode();

                categories = await response.Content.ReadFromJsonAsync<List<ReferenceDataDto>>(_jsonOptions);
                if (categories != null)
                {
                    _cache.Set(cacheKey, categories, TimeSpan.FromMinutes(30));
                }
            }

            return categories ?? new List<ReferenceDataDto>();
        }

        public async Task<List<ReferenceDataDto>> GetDifficultyLevelsAsync()
        {
            var cacheKey = "difficulty_levels";
            
            if (!_cache.TryGetValue(cacheKey, out List<ReferenceDataDto>? difficulties))
            {
                var response = await _httpClient.GetAsync("api/workout-templates/difficulties");
                response.EnsureSuccessStatusCode();

                difficulties = await response.Content.ReadFromJsonAsync<List<ReferenceDataDto>>(_jsonOptions);
                if (difficulties != null)
                {
                    _cache.Set(cacheKey, difficulties, TimeSpan.FromMinutes(30));
                }
            }

            return difficulties ?? new List<ReferenceDataDto>();
        }

        public async Task<List<ReferenceDataDto>> GetWorkoutStatesAsync()
        {
            var cacheKey = "workout_states";
            
            if (!_cache.TryGetValue(cacheKey, out List<ReferenceDataDto>? states))
            {
                var response = await _httpClient.GetAsync("api/workout-templates/states");
                response.EnsureSuccessStatusCode();

                states = await response.Content.ReadFromJsonAsync<List<ReferenceDataDto>>(_jsonOptions);
                if (states != null)
                {
                    _cache.Set(cacheKey, states, TimeSpan.FromMinutes(30));
                }
            }

            return states ?? new List<ReferenceDataDto>();
        }

        public async Task<List<ReferenceDataDto>> GetWorkoutObjectivesAsync()
        {
            var cacheKey = "workout_objectives";
            
            if (!_cache.TryGetValue(cacheKey, out List<ReferenceDataDto>? objectives))
            {
                var response = await _httpClient.GetAsync("api/workout-templates/objectives");
                response.EnsureSuccessStatusCode();

                objectives = await response.Content.ReadFromJsonAsync<List<ReferenceDataDto>>(_jsonOptions);
                if (objectives != null)
                {
                    _cache.Set(cacheKey, objectives, TimeSpan.FromMinutes(30));
                }
            }

            return objectives ?? new List<ReferenceDataDto>();
        }

        private string BuildQueryString(WorkoutTemplateFilterDto filter)
        {
            var queryParams = new List<string>();

            queryParams.Add($"page={filter.Page}");
            queryParams.Add($"pageSize={filter.PageSize}");

            if (!string.IsNullOrWhiteSpace(filter.NamePattern))
            {
                queryParams.Add($"namePattern={HttpUtility.UrlEncode(filter.NamePattern)}");
            }

            if (!string.IsNullOrWhiteSpace(filter.CategoryId))
            {
                queryParams.Add($"categoryId={filter.CategoryId}");
            }

            if (!string.IsNullOrWhiteSpace(filter.DifficultyId))
            {
                queryParams.Add($"difficultyId={filter.DifficultyId}");
            }

            if (!string.IsNullOrWhiteSpace(filter.StateId))
            {
                queryParams.Add($"stateId={filter.StateId}");
            }

            if (filter.IsPublic.HasValue)
            {
                queryParams.Add($"isPublic={filter.IsPublic.Value.ToString().ToLower()}");
            }

            return queryParams.Count > 0 ? $"?{string.Join("&", queryParams)}" : string.Empty;
        }
    }
}