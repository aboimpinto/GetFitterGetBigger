using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Services
{
    public interface IExerciseWeightTypeStateService
    {
        event Action? OnChange;

        // Weight types state
        IEnumerable<ExerciseWeightTypeDto> WeightTypes { get; }
        bool IsLoading { get; }
        string? ErrorMessage { get; }

        // Methods
        Task LoadWeightTypesAsync();
        Task<ExerciseWeightTypeDto?> GetWeightTypeByCodeAsync(string code);
        Task<ExerciseWeightTypeDto?> GetWeightTypeByIdAsync(Guid id);
        Task<bool> ValidateWeightAsync(string weightTypeCode, decimal? weight);
        void ClearError();
        string GetValidationMessage(string weightTypeCode);
        bool RequiresWeightInput(string weightTypeCode);
    }
}