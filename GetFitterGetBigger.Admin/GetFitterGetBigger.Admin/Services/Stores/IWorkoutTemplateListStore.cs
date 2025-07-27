using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Services.Stores
{
    public interface IWorkoutTemplateListStore
    {
        // List state
        WorkoutTemplatePagedResultDto? CurrentPage { get; }
        WorkoutTemplateFilterDto CurrentFilter { get; }
        bool IsLoading { get; }
        string? ErrorMessage { get; }

        // Page state management
        bool HasStoredPage { get; }
        void StoreReturnPage();
        void ClearStoredPage();

        // List operations
        Task LoadTemplatesAsync(WorkoutTemplateFilterDto? filter = null);
        Task LoadTemplatesWithStoredPageAsync();
        Task RefreshAsync();
        Task DeleteTemplateAsync(string id);
        Task ChangeTemplateStateAsync(string id, ChangeWorkoutStateDto changeState);
        Task DuplicateTemplateAsync(string id, DuplicateWorkoutTemplateDto duplicate);

        // Error handling
        void ClearError();

        // State change notification
        event Action? OnChange;
    }
}