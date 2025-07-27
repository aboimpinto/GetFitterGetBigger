using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Services.Stores
{
    public interface IWorkoutReferenceDataStore
    {
        // Reference data
        IEnumerable<ReferenceDataDto> WorkoutCategories { get; }
        IEnumerable<ReferenceDataDto> DifficultyLevels { get; }
        IEnumerable<ReferenceDataDto> WorkoutStates { get; }
        IEnumerable<ReferenceDataDto> WorkoutObjectives { get; }
        
        // Loading state
        bool IsLoading { get; }
        bool IsLoaded { get; }
        string? ErrorMessage { get; }

        // Operations
        Task LoadReferenceDataAsync();
        Task RefreshAsync();

        // State change notification
        event Action? OnChange;
    }
}