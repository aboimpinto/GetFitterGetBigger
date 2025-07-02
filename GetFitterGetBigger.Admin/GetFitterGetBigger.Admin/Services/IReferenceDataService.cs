using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Services
{
    public interface IReferenceDataService
    {
        Task<IEnumerable<ReferenceDataDto>> GetBodyPartsAsync();
        Task<IEnumerable<ReferenceDataDto>> GetDifficultyLevelsAsync();
        Task<IEnumerable<ReferenceDataDto>> GetEquipmentAsync();
        Task<IEnumerable<ExerciseTypeDto>> GetExerciseTypesAsync();
        Task<IEnumerable<ReferenceDataDto>> GetKineticChainTypesAsync();
        Task<IEnumerable<ReferenceDataDto>> GetMetricTypesAsync();
        Task<IEnumerable<ReferenceDataDto>> GetMovementPatternsAsync();
        Task<IEnumerable<ReferenceDataDto>> GetMuscleGroupsAsync();
        Task<IEnumerable<ReferenceDataDto>> GetMuscleRolesAsync();
        void ClearEquipmentCache();
        void ClearMuscleGroupsCache();
    }
}
