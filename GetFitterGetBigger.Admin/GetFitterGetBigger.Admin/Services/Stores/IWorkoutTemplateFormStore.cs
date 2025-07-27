using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Services.Stores
{
    public interface IWorkoutTemplateFormStore
    {
        // Form state
        WorkoutTemplateDto? CurrentTemplate { get; }
        bool IsLoading { get; }
        string? ErrorMessage { get; }
        bool IsDirty { get; }

        // Form operations
        Task LoadTemplateAsync(string id);
        Task<WorkoutTemplateDto> CreateTemplateAsync(CreateWorkoutTemplateDto template);
        Task<WorkoutTemplateDto> UpdateTemplateAsync(string id, UpdateWorkoutTemplateDto template);
        void ClearCurrentTemplate();
        void SetDirty(bool isDirty);

        // Error handling
        void ClearError();

        // State change notification
        event Action? OnChange;
    }
}