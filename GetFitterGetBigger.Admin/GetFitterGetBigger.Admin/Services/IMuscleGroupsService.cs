using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Services
{
    public interface IMuscleGroupsService
    {
        Task<IEnumerable<MuscleGroupDto>> GetMuscleGroupsAsync();
        Task<IEnumerable<MuscleGroupDto>> GetMuscleGroupsByBodyPartAsync(string bodyPartId);
        Task<MuscleGroupDto> CreateMuscleGroupAsync(CreateMuscleGroupDto dto);
        Task<MuscleGroupDto> UpdateMuscleGroupAsync(string id, UpdateMuscleGroupDto dto);
        Task DeleteMuscleGroupAsync(string id);
    }
}