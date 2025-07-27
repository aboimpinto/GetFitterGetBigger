using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Web;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Models.Errors;
using GetFitterGetBigger.Admin.Models.Results;
using Microsoft.Extensions.Caching.Memory;

namespace GetFitterGetBigger.Admin.Services.DataProviders
{
    /// <summary>
    /// HTTP implementation of the workout template data provider.
    /// Handles all HTTP-specific concerns for workout template data access.
    /// </summary>
    public class HttpWorkoutTemplateDataProvider : HttpDataProviderBase, IWorkoutTemplateDataProvider
    {
        private readonly IMemoryCache _cache;

        public HttpWorkoutTemplateDataProvider(
            HttpClient httpClient,
            IMemoryCache cache,
            ILogger<HttpWorkoutTemplateDataProvider> logger)
            : base(httpClient, logger)
        {
            _cache = cache;
        }

        public async Task<DataServiceResult<WorkoutTemplatePagedResultDto>> GetWorkoutTemplatesAsync(WorkoutTemplateFilterDto filter)
        {
            var queryParams = BuildQueryString(filter);
            var requestUrl = $"api/workout-templates{queryParams}";
            
            return await ExecuteHttpGetRequestAsync<WorkoutTemplatePagedResultDto>(requestUrl);
        }

        public async Task<DataServiceResult<WorkoutTemplateDto>> GetWorkoutTemplateByIdAsync(string id)
        {
            var cacheKey = $"workout_template_{id}";
            
            // Check cache first
            if (_cache.TryGetValue(cacheKey, out WorkoutTemplateDto? cached) && cached != null)
            {
                Logger.LogDebug("Cache HIT for workout template {Id}", id);
                return DataServiceResult<WorkoutTemplateDto>.Success(cached);
            }
            
            Logger.LogDebug("Cache MISS for workout template {Id}, fetching from API", id);
            var result = await ExecuteHttpGetRequestAsync<WorkoutTemplateDto>($"api/workout-templates/{id}");
            
            // Cache successful results
            if (result.IsSuccess && result.Data != null)
            {
                _cache.Set(cacheKey, result.Data, TimeSpan.FromMinutes(5));
                Logger.LogDebug("Cached workout template {Id} for 5 minutes", id);
            }
            
            return result;
        }

        public async Task<DataServiceResult<WorkoutTemplateDto>> CreateWorkoutTemplateAsync(CreateWorkoutTemplateDto template)
        {
            return await ExecuteHttpPostRequestAsync<WorkoutTemplateDto>("api/workout-templates", template);
        }

        public async Task<DataServiceResult<WorkoutTemplateDto>> UpdateWorkoutTemplateAsync(string id, UpdateWorkoutTemplateDto template)
        {
            var result = await ExecuteHttpPutRequestAsync<WorkoutTemplateDto>($"api/workout-templates/{id}", template);
            
            // Clear cache on successful update
            if (result.IsSuccess)
            {
                var cacheKey = $"workout_template_{id}";
                _cache.Remove(cacheKey);
                Logger.LogDebug("Cleared cache for updated workout template {Id}", id);
            }
            
            return result;
        }

        public async Task<DataServiceResult<bool>> DeleteWorkoutTemplateAsync(string id)
        {
            var result = await ExecuteHttpDeleteRequestAsync($"api/workout-templates/{id}");
            
            // Clear cache on successful delete
            if (result.IsSuccess)
            {
                var cacheKey = $"workout_template_{id}";
                _cache.Remove(cacheKey);
                Logger.LogDebug("Cleared cache for deleted workout template {Id}", id);
            }
            
            return result;
        }

        public async Task<DataServiceResult<WorkoutTemplateDto>> ChangeWorkoutTemplateStateAsync(string id, ChangeWorkoutStateDto changeState)
        {
            var result = await ExecuteHttpPutRequestAsync<WorkoutTemplateDto>($"api/workout-templates/{id}/state", changeState);
            
            // Clear cache on successful state change
            if (result.IsSuccess)
            {
                var cacheKey = $"workout_template_{id}";
                _cache.Remove(cacheKey);
                Logger.LogDebug("Cleared cache for workout template {Id} after state change", id);
            }
            
            return result;
        }

        public async Task<DataServiceResult<WorkoutTemplateDto>> DuplicateWorkoutTemplateAsync(string id, DuplicateWorkoutTemplateDto duplicate)
        {
            return await ExecuteHttpPostRequestAsync<WorkoutTemplateDto>($"api/workout-templates/{id}/duplicate", duplicate);
        }


        public async Task<DataServiceResult<List<WorkoutTemplateExerciseDto>>> GetTemplateExercisesAsync(string templateId)
        {
            return await ExecuteHttpGetRequestAsync<List<WorkoutTemplateExerciseDto>>($"api/workout-templates/{templateId}/exercises");
        }

        public async Task<DataServiceResult<bool>> CheckTemplateNameExistsAsync(string name)
        {
            var encodedName = HttpUtility.UrlEncode(name);
            
            return await ExecuteHttpGetRequestAsync<bool>($"api/workout-templates/exists/name?name={encodedName}");
        }

        public async Task<DataServiceResult<List<ReferenceDataDto>>> GetWorkoutStatesAsync()
        {
            var cacheKey = "workout_states";

            // Check cache first
            if (_cache.TryGetValue(cacheKey, out List<ReferenceDataDto>? cachedStates) && cachedStates != null)
            {
                Logger.LogDebug("Cache HIT for workout states - returning {Count} items", cachedStates.Count);
                return DataServiceResult<List<ReferenceDataDto>>.Success(cachedStates);
            }

            Logger.LogDebug("Cache MISS for workout states, fetching from API");
            
            var result = await ExecuteHttpGetRequestAsync<List<ReferenceDataDto>>("api/workout-states");
            
            if (result.IsSuccess && result.Data != null)
            {
                _cache.Set(cacheKey, result.Data, TimeSpan.FromMinutes(30));
                Logger.LogDebug("Cached {Count} workout states for 30 minutes", result.Data.Count);
            }
                
            return result;
        }

        private string BuildQueryString(WorkoutTemplateFilterDto filter)
        {
            var queryParams = new List<string>();

            AddPaginationParams(queryParams, filter);
            AddFilterParams(queryParams, filter);

            var queryString = queryParams.Count > 0 ? $"?{string.Join("&", queryParams)}" : string.Empty;

            Logger.LogTrace("Built query string: {QueryString}", queryString);

            return queryString;
        }

        private void AddPaginationParams(List<string> queryParams, WorkoutTemplateFilterDto filter)
        {
            queryParams.Add($"page={filter.Page}");
            queryParams.Add($"pageSize={filter.PageSize}");
        }

        private void AddFilterParams(List<string> queryParams, WorkoutTemplateFilterDto filter)
        {
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
        }
    }
}