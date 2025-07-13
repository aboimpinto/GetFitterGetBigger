using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;

namespace GetFitterGetBigger.API.Repositories.Implementations;

/// <summary>
/// Repository implementation for WorkoutCategory reference data
/// </summary>
public class WorkoutCategoryRepository : 
    ReferenceDataRepository<WorkoutCategory, WorkoutCategoryId, FitnessDbContext>, 
    IWorkoutCategoryRepository
{
    // All required methods are implemented by the base ReferenceDataRepository class
}