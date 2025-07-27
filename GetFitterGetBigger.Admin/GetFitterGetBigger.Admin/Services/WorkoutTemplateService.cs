using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Models.Errors;
using GetFitterGetBigger.Admin.Models.ReferenceData;
using GetFitterGetBigger.Admin.Models.Results;
using GetFitterGetBigger.Admin.Services.DataProviders;
using GetFitterGetBigger.Admin.Services.Validation;
using GetFitterGetBigger.Admin.Extensions;

namespace GetFitterGetBigger.Admin.Services
{
    public class WorkoutTemplateService : IWorkoutTemplateService
    {
        private readonly IWorkoutTemplateDataProvider _dataProvider;
        private readonly IGenericReferenceDataService _referenceDataService;
        private readonly IWorkoutReferenceDataService _workoutReferenceDataService;
        private readonly ILogger<WorkoutTemplateService> _logger;

        public WorkoutTemplateService(
            IWorkoutTemplateDataProvider dataProvider,
            IGenericReferenceDataService referenceDataService,
            IWorkoutReferenceDataService workoutReferenceDataService,
            ILogger<WorkoutTemplateService> logger)
        {
            _dataProvider = dataProvider;
            _referenceDataService = referenceDataService;
            _workoutReferenceDataService = workoutReferenceDataService;
            _logger = logger;
        }

        public async Task<ServiceResult<WorkoutTemplatePagedResultDto>> GetWorkoutTemplatesAsync(WorkoutTemplateFilterDto filter)
        {
            // Validate and process using Result pattern
            return await Validate.For(filter)
                .EnsureCappedRange(f => f.PageSize, 1, 100, _logger)
                .OnSuccessAsync(async validFilter =>
                {
                    var dataResult = await _dataProvider.GetWorkoutTemplatesAsync(validFilter);
                    return dataResult.ToServiceResult();
                });
        }

        public async Task<WorkoutTemplateDto?> GetWorkoutTemplateByIdAsync(string id)
        {
            var result = await _dataProvider.GetWorkoutTemplateByIdAsync(id);
            
            if (!result.IsSuccess)
            {
                var error = result.Errors.FirstOrDefault();
                _logger.LogWarning("Failed to get workout template {Id}: {Error}", id, error?.Message ?? "Unknown error");
                return null;
            }
            
            return result.Data;
        }

        public async Task<WorkoutTemplateDto> CreateWorkoutTemplateAsync(CreateWorkoutTemplateDto template)
        {
            // Business validation
            ValidateTemplateName(template.Name);

            var result = await _dataProvider.CreateWorkoutTemplateAsync(template);
            
            if (!result.IsSuccess)
            {
                HandleDataError(result.Errors.FirstOrDefault(), "create workout template", 
                    DataErrorCode.Conflict, $"A workout template with the name '{template.Name}' already exists");
            }
            
            _logger.LogInformation("Created workout template {Id} - {Name}", result.Data!.Id, result.Data.Name);
            return result.Data!;
        }

        public async Task<WorkoutTemplateDto> UpdateWorkoutTemplateAsync(string id, UpdateWorkoutTemplateDto template)
        {
            // Business validation
            ValidateTemplateName(template.Name);

            var result = await _dataProvider.UpdateWorkoutTemplateAsync(id, template);
            
            if (!result.IsSuccess)
            {
                HandleDataError(result.Errors.FirstOrDefault(), "update workout template",
                    DataErrorCode.NotFound, $"Workout template with ID '{id}' not found");
            }
            
            _logger.LogInformation("Updated workout template {Id} - {Name}", result.Data!.Id, result.Data.Name);
            return result.Data!;
        }

        public async Task DeleteWorkoutTemplateAsync(string id)
        {
            var result = await _dataProvider.DeleteWorkoutTemplateAsync(id);
            
            if (!result.IsSuccess)
            {
                HandleDataError(result.Errors.FirstOrDefault(), "delete workout template",
                    DataErrorCode.NotFound, $"Workout template with ID '{id}' not found");
            }
            
            _logger.LogInformation("Deleted workout template {Id}", id);
        }

        public async Task<WorkoutTemplateDto> ChangeWorkoutTemplateStateAsync(string id, ChangeWorkoutStateDto changeState)
        {
            // Business validation
            if (string.IsNullOrWhiteSpace(changeState.WorkoutStateId))
            {
                throw new ArgumentException("Workout State ID is required");
            }

            var result = await _dataProvider.ChangeWorkoutTemplateStateAsync(id, changeState);
            
            if (!result.IsSuccess)
            {
                HandleDataError(result.Errors.FirstOrDefault(), "change workout template state",
                    DataErrorCode.NotFound, $"Workout template with ID '{id}' not found");
            }
            
            _logger.LogInformation("Changed workout template {Id} state to {WorkoutStateId}", id, changeState.WorkoutStateId);
            return result.Data!;
        }

        public async Task<WorkoutTemplateDto> DuplicateWorkoutTemplateAsync(string id, DuplicateWorkoutTemplateDto duplicate)
        {
            // Business validation
            ValidateTemplateName(duplicate.NewName);

            var result = await _dataProvider.DuplicateWorkoutTemplateAsync(id, duplicate);
            
            if (!result.IsSuccess)
            {
                var error = result.Errors.FirstOrDefault();
                if (error?.Code == DataErrorCode.NotFound)
                {
                    throw new InvalidOperationException($"Original workout template with ID '{id}' not found");
                }
                if (error?.Code == DataErrorCode.Conflict)
                {
                    throw new InvalidOperationException($"A workout template with the name '{duplicate.NewName}' already exists");
                }
                HandleDataError(error, "duplicate workout template");
            }
            
            _logger.LogInformation("Duplicated workout template {OriginalId} to new template {NewId} - {NewName}", 
                id, result.Data!.Id, result.Data.Name);
            return result.Data!;
        }


        public async Task<List<WorkoutTemplateExerciseDto>> GetTemplateExercisesAsync(string templateId)
        {
            var result = await _dataProvider.GetTemplateExercisesAsync(templateId);
            
            if (!result.IsSuccess)
            {
                var error = result.Errors.FirstOrDefault();
                _logger.LogError("Failed to get template exercises: {Error}", error?.Message ?? "Unknown error");
                return new List<WorkoutTemplateExerciseDto>();
            }
            
            _logger.LogDebug("Retrieved {Count} exercises for template {TemplateId}", result.Data?.Count ?? 0, templateId);
            return result.Data ?? new List<WorkoutTemplateExerciseDto>();
        }

        public async Task<bool> CheckTemplateNameExistsAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            var result = await _dataProvider.CheckTemplateNameExistsAsync(name);
            
            if (!result.IsSuccess)
            {
                var error = result.Errors.FirstOrDefault();
                _logger.LogError("Failed to check template name existence: {Error}", error?.Message ?? "Unknown error");
                return false; // Assume doesn't exist on error
            }
            
            _logger.LogDebug("Template name '{Name}' exists: {Exists}", name, result.Data);
            return result.Data;
        }

        // Reference data methods
        public async Task<List<ReferenceDataDto>> GetWorkoutCategoriesAsync()
        {
            var categories = await _workoutReferenceDataService.GetWorkoutCategoriesAsync();
            
            if (categories == null)
            {
                _logger.LogWarning("GetWorkoutCategoriesAsync returned null");
                return new List<ReferenceDataDto>();
            }
            
            // Convert WorkoutCategoryDto to ReferenceDataDto
            var result = categories.Select(c => new ReferenceDataDto
            {
                Id = c.WorkoutCategoryId,
                Value = c.Value,
                Description = c.Description
            }).ToList();
            
            _logger.LogDebug("Categories received: {Count}", result.Count);
            return result;
        }

        public async Task<List<ReferenceDataDto>> GetDifficultyLevelsAsync()
        {
            var difficulties = await _referenceDataService.GetReferenceDataAsync<DifficultyLevels>();
            
            if (difficulties == null)
            {
                _logger.LogWarning("GetReferenceDataAsync<DifficultyLevels> returned null");
                return new List<ReferenceDataDto>();
            }
            
            var result = difficulties.ToList();
            _logger.LogDebug("Difficulties received: {Count}", result.Count);
            return result;
        }

        public async Task<List<ReferenceDataDto>> GetWorkoutStatesAsync()
        {
            var result = await _dataProvider.GetWorkoutStatesAsync();
            
            if (!result.IsSuccess)
            {
                var error = result.Errors.FirstOrDefault();
                _logger.LogError("Failed to get workout states: {Error}", error?.Message ?? "Unknown error");
                return new List<ReferenceDataDto>();
            }
            
            _logger.LogDebug("Retrieved {Count} workout states", result.Data?.Count ?? 0);
            return result.Data ?? new List<ReferenceDataDto>();
        }

        public async Task<List<ReferenceDataDto>> GetWorkoutObjectivesAsync()
        {
            var objectives = await _workoutReferenceDataService.GetWorkoutObjectivesAsync();
            
            if (objectives == null)
            {
                _logger.LogWarning("GetWorkoutObjectivesAsync returned null");
                return new List<ReferenceDataDto>();
            }
            
            _logger.LogDebug("Objectives received: {Count}", objectives.Count);
            return objectives;
        }

        private void ValidateTemplateName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Workout template name is required");
            }

            if (name.Length > 100)
            {
                throw new ArgumentException("Workout template name cannot exceed 100 characters");
            }
        }

        private void HandleDataError(IErrorDetail? error, string operation, DataErrorCode? specificCode = null, string? specificMessage = null)
        {
            if (specificCode != null && error?.Code.Equals(specificCode) == true && specificMessage != null)
            {
                throw new InvalidOperationException(specificMessage);
            }

            var message = $"Failed to {operation}: {error?.Message ?? "Unknown error"}";
            var exception = new InvalidOperationException(message);
            _logger.LogError(exception, "Operation failed with code {Code}", error?.Code.ToString() ?? "UNKNOWN");
            throw exception;
        }
    }
}