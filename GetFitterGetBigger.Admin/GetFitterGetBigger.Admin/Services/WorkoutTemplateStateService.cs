using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Builders;

namespace GetFitterGetBigger.Admin.Services
{
    public class WorkoutTemplateStateService : IWorkoutTemplateStateService
    {
        private readonly IWorkoutTemplateService _workoutTemplateService;

        public event Action? OnChange;

        // List state
        public WorkoutTemplatePagedResultDto? CurrentPage { get; private set; }
        public WorkoutTemplateFilterDto CurrentFilter { get; private set; } = new WorkoutTemplateFilterDtoBuilder().Build();
        public bool IsLoading { get; private set; }
        public string? ErrorMessage { get; private set; }

        // Selected template state
        public WorkoutTemplateDto? SelectedTemplate { get; private set; }
        public bool IsLoadingTemplate { get; private set; }

        // Reference data state
        public IEnumerable<ReferenceDataDto> WorkoutCategories { get; private set; } = Enumerable.Empty<ReferenceDataDto>();
        public IEnumerable<ReferenceDataDto> DifficultyLevels { get; private set; } = Enumerable.Empty<ReferenceDataDto>();
        public IEnumerable<ReferenceDataDto> WorkoutStates { get; private set; } = Enumerable.Empty<ReferenceDataDto>();
        public IEnumerable<ReferenceDataDto> WorkoutObjectives { get; private set; } = Enumerable.Empty<ReferenceDataDto>();
        public bool IsLoadingReferenceData { get; private set; }

        // Page state management
        private WorkoutTemplateFilterDto? _storedFilter;
        public bool HasStoredPage => _storedFilter != null;

        public WorkoutTemplateStateService(IWorkoutTemplateService workoutTemplateService)
        {
            _workoutTemplateService = workoutTemplateService;
        }

        public async Task InitializeAsync()
        {
            await LoadReferenceDataAsync();
            await LoadWorkoutTemplatesAsync();
        }

        public async Task LoadWorkoutTemplatesAsync(WorkoutTemplateFilterDto? filter = null)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                NotifyStateChanged();

                if (filter != null)
                {
                    CurrentFilter = filter;
                }

                CurrentPage = await _workoutTemplateService.GetWorkoutTemplatesAsync(CurrentFilter);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load workout templates: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                NotifyStateChanged();
            }
        }

        public async Task LoadWorkoutTemplateByIdAsync(string id)
        {
            try
            {
                IsLoadingTemplate = true;
                ErrorMessage = null;
                NotifyStateChanged();

                SelectedTemplate = await _workoutTemplateService.GetWorkoutTemplateByIdAsync(id);

                if (SelectedTemplate == null)
                {
                    ErrorMessage = "Workout template not found";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load workout template: {ex.Message}";
            }
            finally
            {
                IsLoadingTemplate = false;
                NotifyStateChanged();
            }
        }

        public async Task CreateWorkoutTemplateAsync(CreateWorkoutTemplateDto template)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                NotifyStateChanged();

                var created = await _workoutTemplateService.CreateWorkoutTemplateAsync(template);

                // Refresh the current page to show the new template
                await RefreshCurrentPageAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to create workout template: {ex.Message}";
                IsLoading = false;
                NotifyStateChanged();
                throw; // Re-throw to allow component to handle
            }
        }

        public async Task UpdateWorkoutTemplateAsync(string id, UpdateWorkoutTemplateDto template)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                NotifyStateChanged();

                var updated = await _workoutTemplateService.UpdateWorkoutTemplateAsync(id, template);

                // Update selected template if it's the one being updated
                if (SelectedTemplate?.Id == id)
                {
                    SelectedTemplate = updated;
                }

                // Refresh the current page
                await RefreshCurrentPageAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to update workout template: {ex.Message}";
                IsLoading = false;
                NotifyStateChanged();
                throw; // Re-throw to allow component to handle
            }
        }

        public async Task DeleteWorkoutTemplateAsync(string id)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                NotifyStateChanged();

                await _workoutTemplateService.DeleteWorkoutTemplateAsync(id);

                // Clear selected template if it was deleted
                if (SelectedTemplate?.Id == id)
                {
                    SelectedTemplate = null;
                }

                // Refresh the current page
                await RefreshCurrentPageAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to delete workout template: {ex.Message}";
                IsLoading = false;
                NotifyStateChanged();
                throw; // Re-throw to allow component to handle
            }
        }

        public async Task ChangeWorkoutTemplateStateAsync(string id, ChangeWorkoutStateDto changeState)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                NotifyStateChanged();

                var updated = await _workoutTemplateService.ChangeWorkoutTemplateStateAsync(id, changeState);

                // Update selected template if it's the one being updated
                if (SelectedTemplate?.Id == id)
                {
                    SelectedTemplate = updated;
                }

                // Refresh the current page
                await RefreshCurrentPageAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to change workout template state: {ex.Message}";
                IsLoading = false;
                NotifyStateChanged();
                throw; // Re-throw to allow component to handle
            }
        }

        public async Task DuplicateWorkoutTemplateAsync(string id, DuplicateWorkoutTemplateDto duplicate)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                NotifyStateChanged();

                var duplicated = await _workoutTemplateService.DuplicateWorkoutTemplateAsync(id, duplicate);

                // Refresh the current page to show the new template
                await RefreshCurrentPageAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to duplicate workout template: {ex.Message}";
                IsLoading = false;
                NotifyStateChanged();
                throw; // Re-throw to allow component to handle
            }
        }

        public async Task RefreshCurrentPageAsync()
        {
            await LoadWorkoutTemplatesAsync();
        }

        public void ClearSelectedTemplate()
        {
            SelectedTemplate = null;
            NotifyStateChanged();
        }

        public void ClearError()
        {
            ErrorMessage = null;
            NotifyStateChanged();
        }

        private async Task LoadReferenceDataAsync()
        {
            try
            {
                IsLoadingReferenceData = true;
                NotifyStateChanged();

                // Load all reference data in parallel
                var categoriesTask = _workoutTemplateService.GetWorkoutCategoriesAsync();
                var difficultyTask = _workoutTemplateService.GetDifficultyLevelsAsync();
                var statesTask = _workoutTemplateService.GetWorkoutStatesAsync();
                var objectivesTask = _workoutTemplateService.GetWorkoutObjectivesAsync();

                await Task.WhenAll(
                    categoriesTask,
                    difficultyTask,
                    statesTask,
                    objectivesTask
                );

                WorkoutCategories = await categoriesTask;
                DifficultyLevels = await difficultyTask;
                WorkoutStates = await statesTask;
                WorkoutObjectives = await objectivesTask;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load reference data: {ex.Message}";
            }
            finally
            {
                IsLoadingReferenceData = false;
                NotifyStateChanged();
            }
        }

        private void NotifyStateChanged() => OnChange?.Invoke();

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
        }

        public void ClearStoredPage()
        {
            _storedFilter = null;
        }

        public async Task LoadWorkoutTemplatesWithStoredPageAsync()
        {
            if (_storedFilter != null)
            {
                await LoadWorkoutTemplatesAsync(_storedFilter);
                _storedFilter = null; // Clear after use
            }
            else
            {
                await LoadWorkoutTemplatesAsync();
            }
        }
    }
}