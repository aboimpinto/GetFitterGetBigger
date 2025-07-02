using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Builders;

namespace GetFitterGetBigger.Admin.Services
{
    public class ExerciseStateService : IExerciseStateService
    {
        private readonly IExerciseService _exerciseService;
        private readonly IReferenceDataService _referenceDataService;
        
        public event Action? OnChange;
        
        // List state
        public ExercisePagedResultDto? CurrentPage { get; private set; }
        public ExerciseFilterDto CurrentFilter { get; private set; } = ExerciseFilterBuilder.Default();
        public bool IsLoading { get; private set; }
        public string? ErrorMessage { get; private set; }
        
        // Selected exercise state
        public ExerciseDto? SelectedExercise { get; private set; }
        public bool IsLoadingExercise { get; private set; }
        
        // Reference data state
        public IEnumerable<ReferenceDataDto> DifficultyLevels { get; private set; } = Enumerable.Empty<ReferenceDataDto>();
        public IEnumerable<ReferenceDataDto> MuscleGroups { get; private set; } = Enumerable.Empty<ReferenceDataDto>();
        public IEnumerable<ReferenceDataDto> MuscleRoles { get; private set; } = Enumerable.Empty<ReferenceDataDto>();
        public IEnumerable<ReferenceDataDto> Equipment { get; private set; } = Enumerable.Empty<ReferenceDataDto>();
        public IEnumerable<ReferenceDataDto> BodyParts { get; private set; } = Enumerable.Empty<ReferenceDataDto>();
        public IEnumerable<ReferenceDataDto> MovementPatterns { get; private set; } = Enumerable.Empty<ReferenceDataDto>();
        public IEnumerable<ExerciseTypeDto> ExerciseTypes { get; private set; } = Enumerable.Empty<ExerciseTypeDto>();
        public bool IsLoadingReferenceData { get; private set; }
        
        // Page state management
        private ExerciseFilterDto? _storedFilter;
        public bool HasStoredPage => _storedFilter != null;

        public ExerciseStateService(IExerciseService exerciseService, IReferenceDataService referenceDataService)
        {
            _exerciseService = exerciseService;
            _referenceDataService = referenceDataService;
        }

        public async Task InitializeAsync()
        {
            Console.WriteLine("[ExerciseStateService] InitializeAsync called");
            await LoadReferenceDataAsync();
            Console.WriteLine("[ExerciseStateService] LoadReferenceDataAsync completed");
            await LoadExercisesAsync();
            Console.WriteLine("[ExerciseStateService] LoadExercisesAsync completed");
        }

        public async Task LoadExercisesAsync(ExerciseFilterDto? filter = null)
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
                
                CurrentPage = await _exerciseService.GetExercisesAsync(CurrentFilter);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load exercises: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                NotifyStateChanged();
            }
        }

        public async Task LoadExerciseByIdAsync(string id)
        {
            try
            {
                IsLoadingExercise = true;
                ErrorMessage = null;
                NotifyStateChanged();
                
                SelectedExercise = await _exerciseService.GetExerciseByIdAsync(id);
                
                if (SelectedExercise == null)
                {
                    ErrorMessage = "Exercise not found";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load exercise: {ex.Message}";
            }
            finally
            {
                IsLoadingExercise = false;
                NotifyStateChanged();
            }
        }

        public async Task CreateExerciseAsync(ExerciseCreateDto exercise)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                NotifyStateChanged();
                
                var created = await _exerciseService.CreateExerciseAsync(exercise);
                
                // Refresh the current page to show the new exercise
                await RefreshCurrentPageAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to create exercise: {ex.Message}";
                IsLoading = false;
                NotifyStateChanged();
                throw; // Re-throw to allow component to handle
            }
        }

        public async Task UpdateExerciseAsync(string id, ExerciseUpdateDto exercise)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                NotifyStateChanged();
                
                await _exerciseService.UpdateExerciseAsync(id, exercise);
                
                // Clear selected exercise as it's now outdated
                SelectedExercise = null;
                
                // Refresh the current page
                await RefreshCurrentPageAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to update exercise: {ex.Message}";
                IsLoading = false;
                NotifyStateChanged();
                throw; // Re-throw to allow component to handle
            }
        }

        public async Task DeleteExerciseAsync(string id)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                NotifyStateChanged();
                
                await _exerciseService.DeleteExerciseAsync(id);
                
                // Clear selected exercise if it was deleted
                if (SelectedExercise?.Id == id)
                {
                    SelectedExercise = null;
                }
                
                // Refresh the current page
                await RefreshCurrentPageAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to delete exercise: {ex.Message}";
                IsLoading = false;
                NotifyStateChanged();
                throw; // Re-throw to allow component to handle
            }
        }

        public async Task RefreshCurrentPageAsync()
        {
            await LoadExercisesAsync();
        }

        public void ClearSelectedExercise()
        {
            SelectedExercise = null;
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
                
                Console.WriteLine("[ExerciseStateService] Starting LoadReferenceDataAsync...");
                
                // Load all reference data in parallel
                var difficultyTask = _referenceDataService.GetDifficultyLevelsAsync();
                var muscleGroupsTask = _referenceDataService.GetMuscleGroupsAsync();
                var muscleRolesTask = _referenceDataService.GetMuscleRolesAsync();
                var equipmentTask = _referenceDataService.GetEquipmentAsync();
                var bodyPartsTask = _referenceDataService.GetBodyPartsAsync();
                var movementPatternsTask = _referenceDataService.GetMovementPatternsAsync();
                var exerciseTypesTask = _referenceDataService.GetExerciseTypesAsync();
                
                await Task.WhenAll(
                    difficultyTask,
                    muscleGroupsTask,
                    muscleRolesTask,
                    equipmentTask,
                    bodyPartsTask,
                    movementPatternsTask,
                    exerciseTypesTask
                );
                
                DifficultyLevels = await difficultyTask;
                MuscleGroups = await muscleGroupsTask;
                MuscleRoles = await muscleRolesTask;
                Equipment = await equipmentTask;
                BodyParts = await bodyPartsTask;
                MovementPatterns = await movementPatternsTask;
                ExerciseTypes = await exerciseTypesTask;
                
                Console.WriteLine($"[ExerciseStateService] Reference data loaded:");
                Console.WriteLine($"  - DifficultyLevels: {DifficultyLevels.Count()}");
                Console.WriteLine($"  - MuscleGroups: {MuscleGroups.Count()}");
                Console.WriteLine($"  - MuscleRoles: {MuscleRoles.Count()}");
                Console.WriteLine($"  - Equipment: {Equipment.Count()}");
                Console.WriteLine($"  - BodyParts: {BodyParts.Count()}");
                Console.WriteLine($"  - MovementPatterns: {MovementPatterns.Count()}");
                Console.WriteLine($"  - ExerciseTypes: {ExerciseTypes.Count()}");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load reference data: {ex.Message}";
                Console.WriteLine($"[ExerciseStateService] ERROR: {ex}");
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
            _storedFilter = ExerciseFilterBuilder.CopyFrom(CurrentFilter);
        }
        
        public void ClearStoredPage()
        {
            _storedFilter = null;
        }
        
        public async Task LoadExercisesWithStoredPageAsync()
        {
            if (_storedFilter != null)
            {
                var filter = _storedFilter;
                _storedFilter = null; // Clear after use
                await LoadExercisesAsync(filter);
            }
            else
            {
                await LoadExercisesAsync();
            }
        }
    }
}