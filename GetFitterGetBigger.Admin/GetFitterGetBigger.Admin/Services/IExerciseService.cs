using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Services
{
    public interface IExerciseService
    {
        Task<ExercisePagedResultDto> GetExercisesAsync(ExerciseFilterDto filter);
        Task<ExerciseDto?> GetExerciseByIdAsync(Guid id);
        Task<ExerciseDto> CreateExerciseAsync(ExerciseCreateDto exercise);
        Task UpdateExerciseAsync(Guid id, ExerciseUpdateDto exercise);
        Task DeleteExerciseAsync(Guid id);
    }
}