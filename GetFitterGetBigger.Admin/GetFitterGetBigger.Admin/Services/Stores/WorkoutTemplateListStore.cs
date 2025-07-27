using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Models.Results;
using GetFitterGetBigger.Admin.Builders;
using GetFitterGetBigger.Admin.Services.Stores.Events;
using GetFitterGetBigger.Admin.Extensions;

namespace GetFitterGetBigger.Admin.Services.Stores
{
    public class WorkoutTemplateListStore : IWorkoutTemplateListStore
    {
        private readonly IWorkoutTemplateService _workoutTemplateService;
        private readonly IStoreEventAggregator _eventAggregator;
        private readonly ILogger<WorkoutTemplateListStore> _logger;

        public event Action? OnChange;

        // List state
        public WorkoutTemplatePagedResultDto? CurrentPage { get; private set; }
        public WorkoutTemplateFilterDto CurrentFilter { get; private set; } = new WorkoutTemplateFilterDtoBuilder().Build();
        public bool IsLoading { get; private set; }
        public string? ErrorMessage { get; private set; }

        // Page state management
        private WorkoutTemplateFilterDto? _storedFilter;
        public bool HasStoredPage => _storedFilter != null;

        public WorkoutTemplateListStore(
            IWorkoutTemplateService workoutTemplateService,
            IStoreEventAggregator eventAggregator,
            ILogger<WorkoutTemplateListStore> logger)
        {
            _workoutTemplateService = workoutTemplateService;
            _eventAggregator = eventAggregator;
            _logger = logger;

            // Subscribe to events from other stores
            _eventAggregator.Subscribe<WorkoutTemplateCreatedEvent>(OnTemplateCreated);
            _eventAggregator.Subscribe<WorkoutTemplateUpdatedEvent>(OnTemplateUpdated);
            _eventAggregator.Subscribe<WorkoutTemplateDeletedEvent>(OnTemplateDeleted);
            _eventAggregator.Subscribe<WorkoutTemplateStateChangedEvent>(OnTemplateStateChanged);
            _eventAggregator.Subscribe<WorkoutTemplateDuplicatedEvent>(OnTemplateDuplicated);
        }

        public async Task LoadTemplatesAsync(WorkoutTemplateFilterDto? filter = null)
        {
            // Initialize state
            IsLoading = true;
            ErrorMessage = null;
            CurrentPage = WorkoutTemplatePagedResultDto.Empty();
            NotifyStateChanged();

            // Update filter if provided
            if (filter != null)
            {
                CurrentFilter = filter;
            }

            // Call service and get result
            var result = await _workoutTemplateService.GetWorkoutTemplatesAsync(CurrentFilter);

            // Process result with pattern matching
            switch (result.IsSuccess)
            {
                case true:
                    CurrentPage = result.ExtractData();
                    break;
                case false:
                    ErrorMessage = result.ExtractErrorMessage();
                    break;
            }

            // Log and finalize
            result.LogOperation("load workout templates", _logger);
            IsLoading = false;
            NotifyStateChanged();
        }

        public async Task RefreshAsync()
        {
            await LoadTemplatesAsync();
        }

        public async Task DeleteTemplateAsync(string id)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                NotifyStateChanged();

                await _workoutTemplateService.DeleteWorkoutTemplateAsync(id);
                
                // Publish event for other stores
                _eventAggregator.Publish(new WorkoutTemplateDeletedEvent(id));
                
                // Refresh the current page
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to delete workout template: {ex.Message}";
                IsLoading = false;
                NotifyStateChanged();
                throw;
            }
        }

        public async Task ChangeTemplateStateAsync(string id, ChangeWorkoutStateDto changeState)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                NotifyStateChanged();

                var updated = await _workoutTemplateService.ChangeWorkoutTemplateStateAsync(id, changeState);
                
                // Publish event for other stores
                _eventAggregator.Publish(new WorkoutTemplateStateChangedEvent(id, changeState.WorkoutStateId));
                
                // Refresh the current page
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to change workout template state: {ex.Message}";
                IsLoading = false;
                NotifyStateChanged();
                throw;
            }
        }

        public async Task DuplicateTemplateAsync(string id, DuplicateWorkoutTemplateDto duplicate)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                NotifyStateChanged();

                var result = await _workoutTemplateService.DuplicateWorkoutTemplateAsync(id, duplicate);
                
                if (result.IsSuccess)
                {
                    // Publish event for other stores
                    _eventAggregator.Publish(new WorkoutTemplateDuplicatedEvent(id, result.Data!.Id, result.Data.Name));
                }
                else
                {
                    ErrorMessage = result.Errors.FirstOrDefault()?.Message ?? "Failed to duplicate workout template";
                    IsLoading = false;
                    NotifyStateChanged();
                    return;
                }
                
                // Refresh the current page
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to duplicate workout template: {ex.Message}";
                IsLoading = false;
                NotifyStateChanged();
                throw;
            }
        }

        public void StoreReturnPage()
        {
            // Create a deep copy of the current filter to preserve state
            _storedFilter = new WorkoutTemplateFilterDto
            {
                Page = CurrentFilter.Page,
                PageSize = CurrentFilter.PageSize,
                NamePattern = CurrentFilter.NamePattern,
                CategoryId = CurrentFilter.CategoryId,
                DifficultyId = CurrentFilter.DifficultyId,
                StateId = CurrentFilter.StateId,
                IsPublic = CurrentFilter.IsPublic
            };
            _logger.LogDebug("Stored return page: {Page}", _storedFilter.Page);
        }

        public void ClearStoredPage()
        {
            _storedFilter = null;
        }

        public async Task LoadTemplatesWithStoredPageAsync()
        {
            if (_storedFilter != null)
            {
                await LoadTemplatesAsync(_storedFilter);
                _storedFilter = null; // Clear after use
            }
            else
            {
                await LoadTemplatesAsync();
            }
        }

        public void ClearError()
        {
            ErrorMessage = null;
            NotifyStateChanged();
        }

        // Event handlers for cross-store communication
        private void OnTemplateCreated(WorkoutTemplateCreatedEvent e)
        {
            _logger.LogDebug("Template created event received: {TemplateId}", e.TemplateId);
            _ = RefreshAsync(); // Fire and forget
        }

        private void OnTemplateUpdated(WorkoutTemplateUpdatedEvent e)
        {
            _logger.LogDebug("Template updated event received: {TemplateId}", e.TemplateId);
            _ = RefreshAsync(); // Fire and forget
        }

        private void OnTemplateDeleted(WorkoutTemplateDeletedEvent e)
        {
            _logger.LogDebug("Template deleted event received: {TemplateId}", e.TemplateId);
            // Already handled in DeleteTemplateAsync, but here for external deletions
        }

        private void OnTemplateStateChanged(WorkoutTemplateStateChangedEvent e)
        {
            _logger.LogDebug("Template state changed event received: {TemplateId}", e.TemplateId);
            // Already handled in ChangeTemplateStateAsync, but here for external changes
        }

        private void OnTemplateDuplicated(WorkoutTemplateDuplicatedEvent e)
        {
            _logger.LogDebug("Template duplicated event received: {OriginalId} -> {NewId}", e.OriginalId, e.NewId);
            // Already handled in DuplicateTemplateAsync, but here for external duplications
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}