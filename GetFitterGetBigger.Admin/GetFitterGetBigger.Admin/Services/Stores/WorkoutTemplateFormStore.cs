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
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                NotifyStateChanged();

                CurrentTemplate = await _workoutTemplateService.GetWorkoutTemplateByIdAsync(id);

                if (CurrentTemplate == null)
                {
                    ErrorMessage = "Workout template not found";
                    _logger.LogWarning("Template not found: {TemplateId}", id);
                }
                else
                {
                    _logger.LogDebug("Loaded template: {TemplateId} - {TemplateName}", id, CurrentTemplate.Name);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load workout template: {ex.Message}";
                _logger.LogError(ex, "Exception loading template {TemplateId}", id);
            }
            finally
            {
                IsLoading = false;
                IsDirty = false;
                NotifyStateChanged();
            }
        }

        public async Task<WorkoutTemplateDto> CreateTemplateAsync(CreateWorkoutTemplateDto template)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                NotifyStateChanged();

                var created = await _workoutTemplateService.CreateWorkoutTemplateAsync(template);
                CurrentTemplate = created;
                
                _logger.LogInformation("Created template: {TemplateId} - {TemplateName}", created.Id, created.Name);
                
                // Publish event for other stores
                _eventAggregator.Publish(new WorkoutTemplateCreatedEvent(created.Id, created.Name));
                
                return created;
            }
            catch (Exception ex)
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

                var updated = await _workoutTemplateService.UpdateWorkoutTemplateAsync(id, template);
                CurrentTemplate = updated;
                
                _logger.LogInformation("Updated template: {TemplateId} - {TemplateName}", updated.Id, updated.Name);
                
                // Publish event for other stores
                _eventAggregator.Publish(new WorkoutTemplateUpdatedEvent(updated.Id, updated.Name));
                
                return updated;
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