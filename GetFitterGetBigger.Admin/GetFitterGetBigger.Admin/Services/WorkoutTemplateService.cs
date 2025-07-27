using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Models.Errors;
using GetFitterGetBigger.Admin.Models.ReferenceData;
using GetFitterGetBigger.Admin.Models.Results;
using GetFitterGetBigger.Admin.Services.DataProviders;
using GetFitterGetBigger.Admin.Services.Validation;
using Microsoft.Extensions.Caching.Memory;

namespace GetFitterGetBigger.Admin.Services
{
    public class WorkoutTemplateService : IWorkoutTemplateService
    {
        private readonly IWorkoutTemplateDataProvider _dataProvider;
        private readonly IMemoryCache _cache;
        private readonly IGenericReferenceDataService _referenceDataService;
        private readonly IWorkoutReferenceDataService _workoutReferenceDataService;
        private readonly ILogger<WorkoutTemplateService> _logger;

        public WorkoutTemplateService(
            IWorkoutTemplateDataProvider dataProvider,
            IMemoryCache cache,
            IGenericReferenceDataService referenceDataService,
            IWorkoutReferenceDataService workoutReferenceDataService,
            ILogger<WorkoutTemplateService> logger)
        {
            _dataProvider = dataProvider;
            _cache = cache;
            _referenceDataService = referenceDataService;
            _workoutReferenceDataService = workoutReferenceDataService;
            _logger = logger;
        }

        public async Task<WorkoutTemplatePagedResultDto> GetWorkoutTemplatesAsync(WorkoutTemplateFilterDto filter)
        {
            WorkoutTemplatePagedResultDto result;

            // Business logic - validate filter
            if (filter.PageSize > 100)
            {
                _logger.LogWarning("Page size {PageSize} exceeds maximum, capping at 100", filter.PageSize);
                filter.PageSize = 100;
            }

            // Delegate data retrieval to provider
            var dataResult = await _dataProvider.GetWorkoutTemplatesAsync(filter);

            // Handle result with business logic
            result = dataResult.Match(
                onSuccess: data =>
                {
                    _logger.LogDebug("Retrieved {Count} workout templates", data.Items?.Count ?? 0);
                    return data;
                },
                onFailure: errors =>
                {
                    var error = errors.FirstOrDefault();
                    _logger.LogError("Failed to retrieve workout templates: {Code} - {Message}", 
                        error?.Code.ToString() ?? "UNKNOWN", error?.Message ?? "Unknown error");
                    return new WorkoutTemplatePagedResultDto(); // Return empty result per business rules
                }
            );

            return result;
        }

        public async Task<WorkoutTemplateDto?> GetWorkoutTemplateByIdAsync(string id)
        {
            WorkoutTemplateDto? template = null;
            var cacheKey = $"workout_template_{id}";

            // Check cache first (business logic)
            if (!_cache.TryGetValue(cacheKey, out template))
            {
                var dataResult = await _dataProvider.GetWorkoutTemplateByIdAsync(id);

                template = dataResult.Match(
                    onSuccess: data =>
                    {
                        // Business logic: cache successful results
                        _cache.Set(cacheKey, data, TimeSpan.FromMinutes(5));
                        _logger.LogDebug("Cached workout template {Id} - {Name}", data.Id, data.Name);
                        return data;
                    },
                    onFailure: errors =>
                    {
                        var error = errors.FirstOrDefault();
                        _logger.LogWarning("Template {Id} not found: {Code} - {Message}", id, 
                            error?.Code.ToString() ?? "UNKNOWN", error?.Message ?? "Unknown error");
                        return null!;
                    }
                );
            }
            else
            {
                _logger.LogDebug("Cache HIT for workout template {Id}", id);
            }

            return template;
        }

        public async Task<WorkoutTemplateDto> CreateWorkoutTemplateAsync(CreateWorkoutTemplateDto template)
        {
            WorkoutTemplateDto result;

            // Business validation
            if (string.IsNullOrWhiteSpace(template.Name))
            {
                throw new ArgumentException("Workout template name is required", nameof(template));
            }

            if (template.Name.Length > 100)
            {
                throw new ArgumentException("Workout template name cannot exceed 100 characters", nameof(template));
            }

            var dataResult = await _dataProvider.CreateWorkoutTemplateAsync(template);

            result = dataResult.Match(
                onSuccess: data =>
                {
                    _logger.LogInformation("Created workout template {Id} - {Name}", data.Id, data.Name);
                    return data;
                },
                onFailure: errors =>
                {
                    var error = errors.FirstOrDefault();
                    if (error?.Code == DataErrorCode.Conflict)
                    {
                        throw new InvalidOperationException($"A workout template with the name '{template.Name}' already exists");
                    }

                    var exception = new InvalidOperationException($"Failed to create workout template: {error?.Message ?? "Unknown error"}");
                    _logger.LogError(exception, "Creation failed with code {Code}", error?.Code.ToString() ?? "UNKNOWN");
                    throw exception;
                }
            );

            return result;
        }

        public async Task<WorkoutTemplateDto> UpdateWorkoutTemplateAsync(string id, UpdateWorkoutTemplateDto template)
        {
            WorkoutTemplateDto result;

            // Business validation
            if (string.IsNullOrWhiteSpace(template.Name))
            {
                throw new ArgumentException("Workout template name is required", nameof(template));
            }

            var dataResult = await _dataProvider.UpdateWorkoutTemplateAsync(id, template);

            result = dataResult.Match(
                onSuccess: data =>
                {
                    // Clear cache for this template
                    var cacheKey = $"workout_template_{id}";
                    _cache.Remove(cacheKey);
                    
                    _logger.LogInformation("Updated workout template {Id} - {Name}", data.Id, data.Name);
                    return data;
                },
                onFailure: errors =>
                {
                    var error = errors.FirstOrDefault();
                    if (error?.Code == DataErrorCode.NotFound)
                    {
                        throw new InvalidOperationException($"Workout template with ID '{id}' not found");
                    }

                    var exception = new InvalidOperationException($"Failed to update workout template: {error?.Message ?? "Unknown error"}");
                    _logger.LogError(exception, "Update failed with code {Code}", error?.Code.ToString() ?? "UNKNOWN");
                    throw exception;
                }
            );

            return result;
        }

        public async Task DeleteWorkoutTemplateAsync(string id)
        {
            var dataResult = await _dataProvider.DeleteWorkoutTemplateAsync(id);

            dataResult.Match(
                onSuccess: _ =>
                {
                    // Clear cache for this template
                    var cacheKey = $"workout_template_{id}";
                    _cache.Remove(cacheKey);
                    
                    _logger.LogInformation("Deleted workout template {Id}", id);
                    return true;
                },
                onFailure: errors =>
                {
                    var error = errors.FirstOrDefault();
                    if (error?.Code == DataErrorCode.NotFound)
                    {
                        throw new InvalidOperationException($"Workout template with ID '{id}' not found");
                    }

                    var exception = new InvalidOperationException($"Failed to delete workout template: {error?.Message ?? "Unknown error"}");
                    _logger.LogError(exception, "Delete failed with code {Code}", error?.Code.ToString() ?? "UNKNOWN");
                    throw exception;
                }
            );
        }

        public async Task<WorkoutTemplateDto> ChangeWorkoutTemplateStateAsync(string id, ChangeWorkoutStateDto changeState)
        {
            WorkoutTemplateDto result;

            // Business validation
            if (string.IsNullOrWhiteSpace(changeState.WorkoutStateId))
            {
                throw new ArgumentException("Workout State ID is required", nameof(changeState));
            }

            var dataResult = await _dataProvider.ChangeWorkoutTemplateStateAsync(id, changeState);

            result = dataResult.Match(
                onSuccess: data =>
                {
                    // Clear cache for this template
                    var cacheKey = $"workout_template_{id}";
                    _cache.Remove(cacheKey);
                    
                    _logger.LogInformation("Changed workout template {Id} state to {WorkoutStateId}", id, changeState.WorkoutStateId);
                    return data;
                },
                onFailure: errors =>
                {
                    var error = errors.FirstOrDefault();
                    if (error?.Code == DataErrorCode.NotFound)
                    {
                        throw new InvalidOperationException($"Workout template with ID '{id}' not found");
                    }

                    var exception = new InvalidOperationException($"Failed to change workout template state: {error?.Message ?? "Unknown error"}");
                    _logger.LogError(exception, "State change failed with code {Code}", error?.Code.ToString() ?? "UNKNOWN");
                    throw exception;
                }
            );

            return result;
        }

        public async Task<WorkoutTemplateDto> DuplicateWorkoutTemplateAsync(string id, DuplicateWorkoutTemplateDto duplicate)
        {
            WorkoutTemplateDto result;

            // Business validation
            if (string.IsNullOrWhiteSpace(duplicate.NewName))
            {
                throw new ArgumentException("New template name is required", nameof(duplicate));
            }

            var dataResult = await _dataProvider.DuplicateWorkoutTemplateAsync(id, duplicate);

            result = dataResult.Match(
                onSuccess: data =>
                {
                    _logger.LogInformation("Duplicated workout template {OriginalId} to new template {NewId} - {NewName}", 
                        id, data.Id, data.Name);
                    return data;
                },
                onFailure: errors =>
                {
                    var error = errors.FirstOrDefault();
                    if (error?.Code == DataErrorCode.NotFound)
                    {
                        throw new InvalidOperationException($"Original workout template with ID '{id}' not found");
                    }
                    
                    if (error?.Code == DataErrorCode.Conflict)
                    {
                        throw new InvalidOperationException($"A workout template with the name '{duplicate.NewName}' already exists");
                    }

                    var exception = new InvalidOperationException($"Failed to duplicate workout template: {error?.Message ?? "Unknown error"}");
                    _logger.LogError(exception, "Duplication failed with code {Code}", error?.Code.ToString() ?? "UNKNOWN");
                    throw exception;
                }
            );

            return result;
        }


        public async Task<List<WorkoutTemplateExerciseDto>> GetTemplateExercisesAsync(string templateId)
        {
            List<WorkoutTemplateExerciseDto> result;

            var dataResult = await _dataProvider.GetTemplateExercisesAsync(templateId);

            result = dataResult.Match(
                onSuccess: data =>
                {
                    _logger.LogDebug("Retrieved {Count} exercises for template {TemplateId}", data.Count, templateId);
                    return data;
                },
                onFailure: errors =>
                {
                    var error = errors.FirstOrDefault();
                    _logger.LogError("Failed to get template exercises: {Code} - {Message}", error?.Code.ToString() ?? "UNKNOWN", error?.Message ?? "Unknown error");
                    return new List<WorkoutTemplateExerciseDto>();
                }
            );

            return result;
        }

        public async Task<bool> CheckTemplateNameExistsAsync(string name)
        {
            bool result;

            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            var dataResult = await _dataProvider.CheckTemplateNameExistsAsync(name);

            result = dataResult.Match(
                onSuccess: exists =>
                {
                    _logger.LogDebug("Template name '{Name}' exists: {Exists}", name, exists);
                    return exists;
                },
                onFailure: errors =>
                {
                    var error = errors.FirstOrDefault();
                    _logger.LogError("Failed to check template name existence: {Code} - {Message}", error?.Code.ToString() ?? "UNKNOWN", error?.Message ?? "Unknown error");
                    return false; // Assume doesn't exist on error
                }
            );

            return result;
        }

        // Reference data methods
        public async Task<List<ReferenceDataDto>> GetWorkoutCategoriesAsync()
        {
            List<ReferenceDataDto> result;
            
            _logger.LogDebug("Getting workout categories from WorkoutReferenceDataService");
            var categories = await _workoutReferenceDataService.GetWorkoutCategoriesAsync();
            
            if (categories == null)
            {
                _logger.LogWarning("GetWorkoutCategoriesAsync returned null");
                result = new List<ReferenceDataDto>();
            }
            else
            {
                // Convert WorkoutCategoryDto to ReferenceDataDto
                result = categories.Select(c => new ReferenceDataDto
                {
                    Id = c.WorkoutCategoryId,
                    Value = c.Value,
                    Description = c.Description
                }).ToList();
                
                _logger.LogDebug("Categories received: {Count}", result.Count);
            }
            
            return result;
        }

        public async Task<List<ReferenceDataDto>> GetDifficultyLevelsAsync()
        {
            List<ReferenceDataDto> result;
            
            _logger.LogDebug("Getting difficulty levels from ReferenceDataService");
            var difficulties = await _referenceDataService.GetReferenceDataAsync<DifficultyLevels>();
            
            if (difficulties == null)
            {
                _logger.LogWarning("GetReferenceDataAsync<DifficultyLevels> returned null");
                result = new List<ReferenceDataDto>();
            }
            else
            {
                result = difficulties.ToList();
                _logger.LogDebug("Difficulties received: {Count}", result.Count);
            }
            
            return result;
        }

        public async Task<List<ReferenceDataDto>> GetWorkoutStatesAsync()
        {
            List<ReferenceDataDto> result;
            
            // Delegate to data provider which handles caching
            var dataResult = await _dataProvider.GetWorkoutStatesAsync();
            
            result = dataResult.Match(
                onSuccess: data =>
                {
                    _logger.LogDebug("Retrieved {Count} workout states", data.Count);
                    return data;
                },
                onFailure: errors =>
                {
                    var error = errors.FirstOrDefault();
                    _logger.LogError("Failed to get workout states: {Code} - {Message}", error?.Code.ToString() ?? "UNKNOWN", error?.Message ?? "Unknown error");
                    return new List<ReferenceDataDto>();
                }
            );
            
            return result;
        }

        public async Task<List<ReferenceDataDto>> GetWorkoutObjectivesAsync()
        {
            List<ReferenceDataDto> result;
            
            _logger.LogDebug("Getting workout objectives from WorkoutReferenceDataService");
            var objectives = await _workoutReferenceDataService.GetWorkoutObjectivesAsync();
            
            if (objectives == null)
            {
                _logger.LogWarning("GetWorkoutObjectivesAsync returned null");
                result = new List<ReferenceDataDto>();
            }
            else
            {
                result = objectives;
                _logger.LogDebug("Objectives received: {Count}", result.Count);
            }
            
            return result;
        }

    }
}