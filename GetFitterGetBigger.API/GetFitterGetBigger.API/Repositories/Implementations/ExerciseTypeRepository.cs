using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;

namespace GetFitterGetBigger.API.Repositories.Implementations;

/// <summary>
/// Repository implementation for ExerciseType reference data
/// </summary>
public class ExerciseTypeRepository : 
    EmptyEnabledReferenceDataRepository<ExerciseType, ExerciseTypeId, FitnessDbContext>,
    IExerciseTypeRepository
{
    // Add any ExerciseType-specific repository methods here if needed
}