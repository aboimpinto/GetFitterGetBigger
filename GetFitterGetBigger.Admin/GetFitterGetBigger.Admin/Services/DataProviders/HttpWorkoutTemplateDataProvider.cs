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
    public class HttpWorkoutTemplateDataProvider : IWorkoutTemplateDataProvider
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<HttpWorkoutTemplateDataProvider> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public HttpWorkoutTemplateDataProvider(
            HttpClient httpClient,
            IMemoryCache cache,
            ILogger<HttpWorkoutTemplateDataProvider> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<DataServiceResult<WorkoutTemplatePagedResultDto>> GetWorkoutTemplatesAsync(WorkoutTemplateFilterDto filter)
        {
            var queryParams = BuildQueryString(filter);
            var requestUrl = $"api/workout-templates{queryParams}";
            
            _logger.LogDebug("HTTP GET {BaseAddress}{RequestUrl}", _httpClient.BaseAddress, requestUrl);
            
            return await ExecuteHttpRequestAsync(
                async () => await _httpClient.GetAsync(requestUrl),
                async response => await response.Content.ReadFromJsonAsync<WorkoutTemplatePagedResultDto>(_jsonOptions),
                "GetWorkoutTemplatesAsync");
        }

        public async Task<DataServiceResult<WorkoutTemplateDto>> GetWorkoutTemplateByIdAsync(string id)
        {
            return await ExecuteHttpRequestAsync(
                async () => await _httpClient.GetAsync($"api/workout-templates/{id}"),
                async response => await response.Content.ReadFromJsonAsync<WorkoutTemplateDto>(_jsonOptions),
                "GetWorkoutTemplateByIdAsync");
        }

        public async Task<DataServiceResult<WorkoutTemplateDto>> CreateWorkoutTemplateAsync(CreateWorkoutTemplateDto template)
        {
            var json = JsonSerializer.Serialize(template, _jsonOptions);
            _logger.LogTrace("Request body: {Json}", json);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            return await ExecuteHttpRequestAsync(
                async () => await _httpClient.PostAsync("api/workout-templates", content),
                async response => await response.Content.ReadFromJsonAsync<WorkoutTemplateDto>(_jsonOptions),
                "CreateWorkoutTemplateAsync");
        }

        public async Task<DataServiceResult<WorkoutTemplateDto>> UpdateWorkoutTemplateAsync(string id, UpdateWorkoutTemplateDto template)
        {
            var json = JsonSerializer.Serialize(template, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            return await ExecuteHttpRequestAsync(
                async () => await _httpClient.PutAsync($"api/workout-templates/{id}", content),
                async response => await response.Content.ReadFromJsonAsync<WorkoutTemplateDto>(_jsonOptions),
                "UpdateWorkoutTemplateAsync");
        }

        public async Task<DataServiceResult<bool>> DeleteWorkoutTemplateAsync(string id)
        {
            return await ExecuteHttpRequestAsync(
                async () => await _httpClient.DeleteAsync($"api/workout-templates/{id}"),
                async response => await Task.FromResult(true),
                "DeleteWorkoutTemplateAsync");
        }

        public async Task<DataServiceResult<WorkoutTemplateDto>> ChangeWorkoutTemplateStateAsync(string id, ChangeWorkoutStateDto changeState)
        {
            var json = JsonSerializer.Serialize(changeState, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            return await ExecuteHttpRequestAsync(
                async () => await _httpClient.PutAsync($"api/workout-templates/{id}/state", content),
                async response => await response.Content.ReadFromJsonAsync<WorkoutTemplateDto>(_jsonOptions),
                "ChangeWorkoutTemplateStateAsync");
        }

        public async Task<DataServiceResult<WorkoutTemplateDto>> DuplicateWorkoutTemplateAsync(string id, DuplicateWorkoutTemplateDto duplicate)
        {
            var json = JsonSerializer.Serialize(duplicate, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            return await ExecuteHttpRequestAsync(
                async () => await _httpClient.PostAsync($"api/workout-templates/{id}/duplicate", content),
                async response => await response.Content.ReadFromJsonAsync<WorkoutTemplateDto>(_jsonOptions),
                "DuplicateWorkoutTemplateAsync");
        }


        public async Task<DataServiceResult<List<WorkoutTemplateExerciseDto>>> GetTemplateExercisesAsync(string templateId)
        {
            return await ExecuteHttpRequestAsync(
                async () => await _httpClient.GetAsync($"api/workout-templates/{templateId}/exercises"),
                async response => await response.Content.ReadFromJsonAsync<List<WorkoutTemplateExerciseDto>>(_jsonOptions) ?? new List<WorkoutTemplateExerciseDto>(),
                "GetTemplateExercisesAsync");
        }

        public async Task<DataServiceResult<bool>> CheckTemplateNameExistsAsync(string name)
        {
            var encodedName = HttpUtility.UrlEncode(name);
            
            return await ExecuteHttpRequestAsync(
                async () => await _httpClient.GetAsync($"api/workout-templates/exists/name?name={encodedName}"),
                async response => await response.Content.ReadFromJsonAsync<bool>(_jsonOptions),
                "CheckTemplateNameExistsAsync");
        }

        public async Task<DataServiceResult<List<ReferenceDataDto>>> GetWorkoutStatesAsync()
        {
            var cacheKey = "workout_states";

            // Check cache first
            if (_cache.TryGetValue(cacheKey, out List<ReferenceDataDto>? cachedStates) && cachedStates != null)
            {
                _logger.LogDebug("Cache HIT for workout states - returning {Count} items", cachedStates.Count);
                return DataServiceResult<List<ReferenceDataDto>>.Success(cachedStates);
            }

            _logger.LogDebug("Cache MISS for workout states, fetching from API");
            
            var result = await ExecuteHttpRequestAsync(
                async () => await _httpClient.GetAsync("api/workout-states"),
                async response => 
                {
                    var states = await response.Content.ReadFromJsonAsync<List<ReferenceDataDto>>(_jsonOptions);
                    if (states != null)
                    {
                        _cache.Set(cacheKey, states, TimeSpan.FromMinutes(30));
                        _logger.LogDebug("Cached {Count} workout states for 30 minutes", states.Count);
                    }
                    return states ?? new List<ReferenceDataDto>();
                },
                "GetWorkoutStatesAsync");
                
            return result;
        }



        private async Task<DataServiceResult<T>> CreateFailureResult<T>(HttpResponseMessage response)
        {
            var errorContent = await response.Content.ReadAsStringAsync();

            _logger.LogError("HTTP {StatusCode} error. Response: {ErrorContent}",
                response.StatusCode, errorContent);

            var details = new Dictionary<string, object>
            {
                ["statusCode"] = (int)response.StatusCode,
                ["response"] = errorContent
            };

            var error = response.StatusCode switch
            {
                HttpStatusCode.NotFound => DataError.NotFound("Resource"),
                HttpStatusCode.BadRequest => DataError.BadRequest("Invalid request"),
                HttpStatusCode.Unauthorized => DataError.Unauthorized(),
                HttpStatusCode.Forbidden => DataError.Forbidden(),
                HttpStatusCode.Conflict => DataError.Conflict("Resource conflict"),
                HttpStatusCode.UnprocessableEntity => DataError.BadRequest("Validation failed"),
                _ => DataError.ServerError($"HTTP error {response.StatusCode}")
            };

            // Add details to the error
            var errorWithDetails = new DataError(error.Code, error.Message, details);

            return DataServiceResult<T>.Failure(errorWithDetails);
        }

        private async Task<DataServiceResult<T>> ExecuteHttpRequestAsync<T>(
            Func<Task<HttpResponseMessage>> httpRequest,
            Func<HttpResponseMessage, Task<T?>> deserializeResponse,
            string methodName)
        {
            try
            {
                var response = await httpRequest();

                if (response.IsSuccessStatusCode)
                {
                    var data = await deserializeResponse(response);
                    return data != null
                        ? DataServiceResult<T>.Success(data)
                        : DataServiceResult<T>.Failure(
                            DataError.DeserializationError("Failed to deserialize response"));
                }
                else
                {
                    return await CreateFailureResult<T>(response);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed for {MethodName}", methodName);
                return DataServiceResult<T>.Failure(DataError.NetworkError(ex.Message));
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request timeout for {MethodName}", methodName);
                return DataServiceResult<T>.Failure(DataError.Timeout());
            }
        }

        private string BuildQueryString(WorkoutTemplateFilterDto filter)
        {
            var queryParams = new List<string>();

            AddPaginationParams(queryParams, filter);
            AddFilterParams(queryParams, filter);

            var queryString = queryParams.Count > 0 ? $"?{string.Join("&", queryParams)}" : string.Empty;

            _logger.LogTrace("Built query string: {QueryString}", queryString);

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