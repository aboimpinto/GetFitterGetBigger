using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Services
{
    public interface IExerciseService
    {
        Task<ExercisePagedResultDto> GetExercisesAsync(ExerciseFilterDto filter);
        Task<ExerciseDto?> GetExerciseByIdAsync(string id);
        Task<ExerciseDto> CreateExerciseAsync(ExerciseCreateDto exercise);
        Task UpdateExerciseAsync(string id, ExerciseUpdateDto exercise);
        Task DeleteExerciseAsync(string id);
    }
}