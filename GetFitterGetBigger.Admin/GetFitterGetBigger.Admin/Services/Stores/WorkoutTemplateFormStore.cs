using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services.Stores.Events;

namespace GetFitterGetBigger.Admin.Services.Stores
{
    public class WorkoutTemplateFormStore : IWorkoutTemplateFormStore
    {
        private readonly IWorkoutTemplateService _workoutTemplateService;
        private readonly IStoreEventAggregator _eventAggregator;
        private readonly ILogger<WorkoutTemplateFormStore> _logger;

        public event Action? OnChange;

        // Form state
        public WorkoutTemplateDto? CurrentTemplate { get; private set; }
        public bool IsLoading { get; private set; }
        public string? ErrorMessage { get; private set; }
        public bool IsDirty { get; private set; }

        public WorkoutTemplateFormStore(
            IWorkoutTemplateService workoutTemplateService,
            IStoreEventAggregator eventAggregator,
            ILogger<WorkoutTemplateFormStore> logger)
        {
            _workoutTemplateService = workoutTemplateService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public async Task LoadTemplateAsync(string id)
        {
            IsLoading = true;
            ErrorMessage = null;
            NotifyStateChanged();

            var result = await _workoutTemplateService.GetWorkoutTemplateByIdAsync(id);

            if (result.IsSuccess)
            {
                CurrentTemplate = result.Data;
                
                if (CurrentTemplate == null || CurrentTemplate.IsEmpty)
                {
                    ErrorMessage = "Workout template not found";
                    _logger.LogWarning("Template not found: {TemplateId}", id);
                }
                else
                {
                    _logger.LogDebug("Loaded template: {TemplateId} - {TemplateName}", id, CurrentTemplate.Name);
                }
            }
            else
            {
                CurrentTemplate = null;
                ErrorMessage = result.Errors.FirstOrDefault()?.Message ?? "Failed to load workout template";
                _logger.LogError("Failed to load template {TemplateId}: {Errors}", id, 
                    string.Join(", ", result.Errors.Select(e => e.Message)));
            }

            IsLoading = false;
            IsDirty = false;
            NotifyStateChanged();
        }

        public async Task<WorkoutTemplateDto> CreateTemplateAsync(CreateWorkoutTemplateDto template)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                NotifyStateChanged();

                var result = await _workoutTemplateService.CreateWorkoutTemplateAsync(template);
                
                if (!result.IsSuccess)
                {
                    ErrorMessage = result.Errors.FirstOrDefault()?.Message ?? "Failed to create workout template";
                    _logger.LogError("Failed to create template: {Errors}", 
                        string.Join(", ", result.Errors.Select(e => e.Message)));
                    throw new InvalidOperationException(ErrorMessage);
                }
                
                var created = result.Data!;
                CurrentTemplate = created;
                
                _logger.LogInformation("Created template: {TemplateId} - {TemplateName}", created.Id, created.Name);
                
                // Publish event for other stores
                _eventAggregator.Publish(new WorkoutTemplateCreatedEvent(created.Id, created.Name));
                
                return created;
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                ErrorMessage = $"Failed to create workout template: {ex.Message}";
                _logger.LogError(ex, "Exception creating template");
                throw;
            }
            finally
            {
                IsLoading = false;
                IsDirty = false;
                NotifyStateChanged();
            }
        }

        public async Task<WorkoutTemplateDto> UpdateTemplateAsync(string id, UpdateWorkoutTemplateDto template)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                NotifyStateChanged();

                var result = await _workoutTemplateService.UpdateWorkoutTemplateAsync(id, template);
                
                if (result.IsSuccess && result.Data != null)
                {
                    CurrentTemplate = result.Data;
                    
                    _logger.LogInformation("Updated template: {TemplateId} - {TemplateName}", result.Data.Id, result.Data.Name);
                    
                    // Publish event for other stores
                    _eventAggregator.Publish(new WorkoutTemplateUpdatedEvent(result.Data.Id, result.Data.Name));
                    
                    return result.Data;
                }
                else
                {
                    ErrorMessage = result.Errors.FirstOrDefault()?.Message ?? "Failed to update workout template";
                    throw new InvalidOperationException(ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to update workout template: {ex.Message}";
                _logger.LogError(ex, "Exception updating template {TemplateId}", id);
                throw;
            }
            finally
            {
                IsLoading = false;
                IsDirty = false;
                NotifyStateChanged();
            }
        }

        public void ClearCurrentTemplate()
        {
            CurrentTemplate = null;
            IsDirty = false;
            ErrorMessage = null;
            NotifyStateChanged();
        }

        public void SetDirty(bool isDirty)
        {
            if (IsDirty != isDirty)
            {
                IsDirty = isDirty;
                NotifyStateChanged();
            }
        }

        public void ClearError()
        {
            ErrorMessage = null;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}