using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Services
{
    public interface IExerciseStateService
    {
        event Action? OnChange;

        // List state
        ExercisePagedResultDto? CurrentPage { get; }
        ExerciseFilterDto CurrentFilter { get; }
        bool IsLoading { get; }
        string? ErrorMessage { get; }

        // Selected exercise state
        ExerciseDto? SelectedExercise { get; }
        bool IsLoadingExercise { get; }

        // Reference data state
        IEnumerable<ReferenceDataDto> DifficultyLevels { get; }
        IEnumerable<ReferenceDataDto> MuscleGroups { get; }
        IEnumerable<ReferenceDataDto> MuscleRoles { get; }
        IEnumerable<ReferenceDataDto> Equipment { get; }
        IEnumerable<ReferenceDataDto> BodyParts { get; }
        IEnumerable<ReferenceDataDto> MovementPatterns { get; }
        IEnumerable<ExerciseTypeDto> ExerciseTypes { get; }
        bool IsLoadingReferenceData { get; }

        // Methods
        Task InitializeAsync();
        Task LoadExercisesAsync(ExerciseFilterDto? filter = null);
        Task LoadExerciseByIdAsync(string id);
        Task CreateExerciseAsync(ExerciseCreateDto exercise);
        Task UpdateExerciseAsync(string id, ExerciseUpdateDto exercise);
        Task DeleteExerciseAsync(string id);
        Task RefreshCurrentPageAsync();
        void ClearSelectedExercise();
        void ClearError();
        void StoreReturnPage();
        void ClearStoredPage();
        bool HasStoredPage { get; }
        Task LoadExercisesWithStoredPageAsync();
    }
}