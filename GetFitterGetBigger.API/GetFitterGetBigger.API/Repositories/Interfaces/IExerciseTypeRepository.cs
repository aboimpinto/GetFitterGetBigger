using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for ExerciseType reference data
/// </summary>
public interface IExerciseTypeRepository : IReferenceDataRepository<ExerciseType, ExerciseTypeId>
{
    // Add any ExerciseType-specific repository methods here if needed
}