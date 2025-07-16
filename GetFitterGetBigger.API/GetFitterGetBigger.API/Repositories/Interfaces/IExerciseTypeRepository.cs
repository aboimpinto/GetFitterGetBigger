using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for ExerciseType reference data
/// TEMPORARY: Using IEmptyEnabledReferenceDataRepository until all entities are migrated
/// </summary>
public interface IExerciseTypeRepository : IEmptyEnabledReferenceDataRepository<ExerciseType, ExerciseTypeId>
{
    // Add any ExerciseType-specific repository methods here if needed
}