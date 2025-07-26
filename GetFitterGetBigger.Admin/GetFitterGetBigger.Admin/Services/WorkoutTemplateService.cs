using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Models.ReferenceData;
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
        private readonly IGenericReferenceDataService _referenceDataService;
        private readonly IWorkoutReferenceDataService _workoutReferenceDataService;
        private readonly JsonSerializerOptions _jsonOptions;

        public WorkoutTemplateService(HttpClient httpClient, IMemoryCache cache, IGenericReferenceDataService referenceDataService, IWorkoutReferenceDataService workoutReferenceDataService)
        {
            _httpClient = httpClient;
            _cache = cache;
            _referenceDataService = referenceDataService;
            _workoutReferenceDataService = workoutReferenceDataService;
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
                var fullUrl = $"{_httpClient.BaseAddress}{requestUrl}";

                Console.WriteLine($"[WorkoutTemplateService] GetWorkoutTemplatesAsync - Request URL: {requestUrl}");
                Console.WriteLine($"[WorkoutTemplateService] GetWorkoutTemplatesAsync - Full URL: {fullUrl}");

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
                
                Console.WriteLine($"[WorkoutTemplateService] ChangeWorkoutTemplateStateAsync - Endpoint: PUT {requestUrl}");
                Console.WriteLine($"[WorkoutTemplateService] ChangeWorkoutTemplateStateAsync - Request Body: {json}");
                Console.WriteLine($"[WorkoutTemplateService] ChangeWorkoutTemplateStateAsync - Full URL: {_httpClient.BaseAddress}{requestUrl}");

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
            Console.WriteLine("[WorkoutTemplateService] Getting workout categories from WorkoutReferenceDataService");
            var categories = await _workoutReferenceDataService.GetWorkoutCategoriesAsync();
            // Convert WorkoutCategoryDto to ReferenceDataDto
            var referenceDtos = categories?.Select(c => new ReferenceDataDto
            {
                Id = c.WorkoutCategoryId,
                Value = c.Value,
                Description = c.Description
            }).ToList();
            Console.WriteLine($"[WorkoutTemplateService] Categories received: {referenceDtos?.Count ?? 0}");
            return referenceDtos ?? new List<ReferenceDataDto>();
        }

        public async Task<List<ReferenceDataDto>> GetDifficultyLevelsAsync()
        {
            Console.WriteLine("[WorkoutTemplateService] Getting difficulty levels from ReferenceDataService");
            var difficulties = await _referenceDataService.GetReferenceDataAsync<DifficultyLevels>();
            Console.WriteLine($"[WorkoutTemplateService] Difficulties received: {difficulties?.Count() ?? 0}");
            return difficulties?.ToList() ?? new List<ReferenceDataDto>();
        }

        public async Task<List<ReferenceDataDto>> GetWorkoutStatesAsync()
        {
            // Workout states need to be fetched directly from API as they're not in reference services
            var cacheKey = "workout_states";
            
            if (!_cache.TryGetValue(cacheKey, out List<ReferenceDataDto>? states))
            {
                Console.WriteLine("[WorkoutTemplateService] Cache MISS for workout states, fetching from API");
                var response = await _httpClient.GetAsync("api/workout-states");
                Console.WriteLine($"[WorkoutTemplateService] States response status: {response.StatusCode}");
                response.EnsureSuccessStatusCode();

                states = await response.Content.ReadFromJsonAsync<List<ReferenceDataDto>>(_jsonOptions);
                Console.WriteLine($"[WorkoutTemplateService] States received: {states?.Count ?? 0}");
                if (states != null)
                {
                    _cache.Set(cacheKey, states, TimeSpan.FromMinutes(30));
                    Console.WriteLine($"[WorkoutTemplateService] Cached workout states for 30 minutes");
                }
            }
            else
            {
                Console.WriteLine($"[WorkoutTemplateService] Cache HIT for workout states - returning {states?.Count ?? 0} items");
            }

            return states ?? new List<ReferenceDataDto>();
        }

        public async Task<List<ReferenceDataDto>> GetWorkoutObjectivesAsync()
        {
            Console.WriteLine("[WorkoutTemplateService] Getting workout objectives from WorkoutReferenceDataService");
            var objectives = await _workoutReferenceDataService.GetWorkoutObjectivesAsync();
            Console.WriteLine($"[WorkoutTemplateService] Objectives received: {objectives?.Count ?? 0}");
            return objectives ?? new List<ReferenceDataDto>();
        }

        private string BuildQueryString(WorkoutTemplateFilterDto filter)
        {
            var queryParams = new List<string>();

            // Log all filter values
            Console.WriteLine($"[WorkoutTemplateService] BuildQueryString - Filter values:");
            Console.WriteLine($"  - Page: {filter.Page}");
            Console.WriteLine($"  - PageSize: {filter.PageSize}");
            Console.WriteLine($"  - NamePattern: '{filter.NamePattern ?? "null"}'");
            Console.WriteLine($"  - CategoryId: '{filter.CategoryId ?? "null"}'");
            Console.WriteLine($"  - DifficultyId: '{filter.DifficultyId ?? "null"}'");
            Console.WriteLine($"  - StateId: '{filter.StateId ?? "null"}'");
            Console.WriteLine($"  - IsPublic: {(filter.IsPublic.HasValue ? filter.IsPublic.Value.ToString() : "null")}");

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

            var queryString = queryParams.Count > 0 ? $"?{string.Join("&", queryParams)}" : string.Empty;
            Console.WriteLine($"[WorkoutTemplateService] BuildQueryString - Final query string: {queryString}");
            
            return queryString;
        }
    }
}