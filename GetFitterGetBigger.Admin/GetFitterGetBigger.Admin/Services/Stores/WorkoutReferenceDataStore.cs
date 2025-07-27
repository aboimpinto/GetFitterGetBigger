using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Models.ReferenceData;

namespace GetFitterGetBigger.Admin.Services.Stores
{
    public class WorkoutReferenceDataStore : IWorkoutReferenceDataStore
    {
        private readonly IGenericReferenceDataService _genericReferenceDataService;
        private readonly IWorkoutReferenceDataService _workoutReferenceDataService;
        private readonly ILogger<WorkoutReferenceDataStore> _logger;

        public event Action? OnChange;

        // Reference data
        public IEnumerable<ReferenceDataDto> WorkoutCategories { get; private set; } = Enumerable.Empty<ReferenceDataDto>();
        public IEnumerable<ReferenceDataDto> DifficultyLevels { get; private set; } = Enumerable.Empty<ReferenceDataDto>();
        public IEnumerable<ReferenceDataDto> WorkoutStates { get; private set; } = Enumerable.Empty<ReferenceDataDto>();
        public IEnumerable<ReferenceDataDto> WorkoutObjectives { get; private set; } = Enumerable.Empty<ReferenceDataDto>();
        
        // Loading state
        public bool IsLoading { get; private set; }
        public bool IsLoaded { get; private set; }
        public string? ErrorMessage { get; private set; }

        public WorkoutReferenceDataStore(
            IGenericReferenceDataService genericReferenceDataService,
            IWorkoutReferenceDataService workoutReferenceDataService,
            ILogger<WorkoutReferenceDataStore> logger)
        {
            _genericReferenceDataService = genericReferenceDataService;
            _workoutReferenceDataService = workoutReferenceDataService;
            _logger = logger;
        }

        public async Task LoadReferenceDataAsync()
        {
            // Don't reload if already loaded
            if (IsLoaded)
            {
                _logger.LogDebug("Reference data already loaded, skipping");
                return;
            }

            try
            {
                _logger.LogDebug("Starting to load reference data");
                IsLoading = true;
                ErrorMessage = null;
                NotifyStateChanged();

                // Load all reference data in parallel
                var categoriesTask = GetWorkoutCategoriesAsync();
                var difficultyTask = _genericReferenceDataService.GetReferenceDataAsync<DifficultyLevels>();
                var statesTask = _genericReferenceDataService.GetReferenceDataAsync<WorkoutStates>();
                var objectivesTask = _workoutReferenceDataService.GetWorkoutObjectivesAsync();

                await Task.WhenAll(
                    categoriesTask,
                    difficultyTask,
                    statesTask,
                    objectivesTask
                );

                WorkoutCategories = await categoriesTask;
                DifficultyLevels = (await difficultyTask).ToList();
                WorkoutStates = (await statesTask).ToList();
                WorkoutObjectives = await objectivesTask;
                
                IsLoaded = true;
                
                _logger.LogInformation(
                    "Reference data loaded: Categories={Categories}, Difficulties={Difficulties}, States={States}, Objectives={Objectives}",
                    WorkoutCategories.Count(),
                    DifficultyLevels.Count(),
                    WorkoutStates.Count(),
                    WorkoutObjectives.Count());
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load reference data: {ex.Message}";
                _logger.LogError(ex, "Exception loading reference data");
                IsLoaded = false;
            }
            finally
            {
                IsLoading = false;
                NotifyStateChanged();
            }
        }

        public async Task RefreshAsync()
        {
            IsLoaded = false;
            await LoadReferenceDataAsync();
        }

        private async Task<List<ReferenceDataDto>> GetWorkoutCategoriesAsync()
        {
            var categories = await _workoutReferenceDataService.GetWorkoutCategoriesAsync();
            
            if (categories == null)
            {
                _logger.LogWarning("GetWorkoutCategoriesAsync returned null");
                return new List<ReferenceDataDto>();
            }
            
            // Convert WorkoutCategoryDto to ReferenceDataDto
            return categories.Select(c => new ReferenceDataDto
            {
                Id = c.WorkoutCategoryId,
                Value = c.Value,
                Description = c.Description
            }).ToList();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}