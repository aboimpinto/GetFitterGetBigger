using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Services
{
    public interface IExerciseWeightTypeService
    {
        Task<IEnumerable<ExerciseWeightTypeDto>> GetWeightTypesAsync();
        Task<ExerciseWeightTypeDto?> GetWeightTypeByIdAsync(Guid id);
    }
}