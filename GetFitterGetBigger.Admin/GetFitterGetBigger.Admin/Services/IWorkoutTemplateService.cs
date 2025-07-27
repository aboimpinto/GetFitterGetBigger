using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Services
{
    public interface IWorkoutTemplateService
    {
        Task<WorkoutTemplatePagedResultDto> GetWorkoutTemplatesAsync(WorkoutTemplateFilterDto filter);
        Task<WorkoutTemplateDto?> GetWorkoutTemplateByIdAsync(string id);
        Task<WorkoutTemplateDto> CreateWorkoutTemplateAsync(CreateWorkoutTemplateDto template);
        Task<WorkoutTemplateDto> UpdateWorkoutTemplateAsync(string id, UpdateWorkoutTemplateDto template);
        Task DeleteWorkoutTemplateAsync(string id);
        Task<WorkoutTemplateDto> ChangeWorkoutTemplateStateAsync(string id, ChangeWorkoutStateDto changeState);
        Task<WorkoutTemplateDto> DuplicateWorkoutTemplateAsync(string id, DuplicateWorkoutTemplateDto duplicate);
        Task<List<WorkoutTemplateExerciseDto>> GetTemplateExercisesAsync(string templateId);
        Task<bool> CheckTemplateNameExistsAsync(string name);
        
        // Reference data methods
        Task<List<ReferenceDataDto>> GetWorkoutCategoriesAsync();
        Task<List<ReferenceDataDto>> GetDifficultyLevelsAsync();
        Task<List<ReferenceDataDto>> GetWorkoutStatesAsync();
        Task<List<ReferenceDataDto>> GetWorkoutObjectivesAsync();
    }
}