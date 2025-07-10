using GetFitterGetBigger.Admin.Models;
using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Services
{
    public class ExerciseWeightTypeStateService : IExerciseWeightTypeStateService
    {
        private readonly IExerciseWeightTypeService _exerciseWeightTypeService;
        private List<ExerciseWeightTypeDto> _weightTypes = new();
        private bool _isLoading;
        private string? _errorMessage;

        public event Action? OnChange;

        public ExerciseWeightTypeStateService(IExerciseWeightTypeService exerciseWeightTypeService)
        {
            _exerciseWeightTypeService = exerciseWeightTypeService;
        }

        // Weight types state
        public IEnumerable<ExerciseWeightTypeDto> WeightTypes => _weightTypes;
        public bool IsLoading => _isLoading;
        public string? ErrorMessage => _errorMessage;

        public async Task LoadWeightTypesAsync()
        {
            try
            {
                _isLoading = true;
                _errorMessage = null;
                OnChange?.Invoke();

                var weightTypes = await _exerciseWeightTypeService.GetWeightTypesAsync();
                _weightTypes = weightTypes.Where(wt => wt.IsActive)
                                         .OrderBy(wt => wt.DisplayOrder)
                                         .ThenBy(wt => wt.Name)
                                         .ToList();
            }
            catch (Exception ex)
            {
                _errorMessage = $"Failed to load exercise weight types: {ex.Message}";
            }
            finally
            {
                _isLoading = false;
                OnChange?.Invoke();
            }
        }

        public async Task<ExerciseWeightTypeDto?> GetWeightTypeByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            // Check if weight types are already loaded
            if (!_weightTypes.Any())
            {
                await LoadWeightTypesAsync();
            }

            return _weightTypes.FirstOrDefault(wt => 
                string.Equals(wt.Code, code, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<ExerciseWeightTypeDto?> GetWeightTypeByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            // Check if weight types are already loaded
            if (!_weightTypes.Any())
            {
                await LoadWeightTypesAsync();
            }

            // First try to find in cached data
            var cachedResult = _weightTypes.FirstOrDefault(wt => wt.Id == id);
            if (cachedResult != null)
                return cachedResult;

            // If not found in cache, try API call
            try
            {
                return await _exerciseWeightTypeService.GetWeightTypeByIdAsync(id);
            }
            catch (Exception ex)
            {
                _errorMessage = $"Failed to get weight type by ID: {ex.Message}";
                OnChange?.Invoke();
                return null;
            }
        }

        public Task<bool> ValidateWeightAsync(string weightTypeCode, decimal? weight)
        {
            var isValid = WeightValidationRule.ValidateWeight(weightTypeCode, weight);
            return Task.FromResult(isValid);
        }

        public void ClearError()
        {
            if (_errorMessage != null)
            {
                _errorMessage = null;
                OnChange?.Invoke();
            }
        }

        public string GetValidationMessage(string weightTypeCode)
        {
            return WeightValidationRule.GetValidationMessage(weightTypeCode);
        }

        public bool RequiresWeightInput(string weightTypeCode)
        {
            return WeightValidationRule.RequiresWeightInput(weightTypeCode);
        }
    }
}