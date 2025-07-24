using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Services
{
    public interface IWorkoutTemplateStateService
    {
        event Action? OnChange;

        // List state
        WorkoutTemplatePagedResultDto? CurrentPage { get; }
        WorkoutTemplateFilterDto CurrentFilter { get; }
        bool IsLoading { get; }
        string? ErrorMessage { get; }

        // Selected template state
        WorkoutTemplateDto? SelectedTemplate { get; }
        bool IsLoadingTemplate { get; }

        // Reference data state
        IEnumerable<ReferenceDataDto> WorkoutCategories { get; }
        IEnumerable<ReferenceDataDto> DifficultyLevels { get; }
        IEnumerable<ReferenceDataDto> WorkoutStates { get; }
        IEnumerable<ReferenceDataDto> WorkoutObjectives { get; }
        bool IsLoadingReferenceData { get; }

        // Methods
        Task InitializeAsync();
        Task LoadWorkoutTemplatesAsync(WorkoutTemplateFilterDto? filter = null);
        Task LoadWorkoutTemplateByIdAsync(string id);
        Task CreateWorkoutTemplateAsync(CreateWorkoutTemplateDto template);
        Task UpdateWorkoutTemplateAsync(string id, UpdateWorkoutTemplateDto template);
        Task DeleteWorkoutTemplateAsync(string id);
        Task ChangeWorkoutTemplateStateAsync(string id, ChangeWorkoutStateDto changeState);
        Task DuplicateWorkoutTemplateAsync(string id, DuplicateWorkoutTemplateDto duplicate);
        Task RefreshCurrentPageAsync();
        void ClearSelectedTemplate();
        void ClearError();
        void StoreReturnPage();
        void ClearStoredPage();
        bool HasStoredPage { get; }
        Task LoadWorkoutTemplatesWithStoredPageAsync();
    }
}