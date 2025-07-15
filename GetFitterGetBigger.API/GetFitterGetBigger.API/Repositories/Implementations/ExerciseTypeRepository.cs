using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;

namespace GetFitterGetBigger.API.Repositories.Implementations;

/// <summary>
/// Repository implementation for ExerciseType reference data
/// </summary>
#pragma warning disable CS8613 // Nullability of reference types in return type doesn't match implicitly implemented member
public class ExerciseTypeRepository : 
    EmptyEnabledReferenceDataRepository<ExerciseType, ExerciseTypeId, FitnessDbContext>,
    IExerciseTypeRepository
{
    // Add any ExerciseType-specific repository methods here if needed
    // Note: CS8613 is suppressed because IReferenceDataRepository still uses nullable returns
    // while EmptyEnabledReferenceDataRepository implements the Empty pattern (non-nullable).
    // This will be resolved when IReferenceDataRepository is updated to the Empty pattern.
}
#pragma warning restore CS8613