using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;

namespace GetFitterGetBigger.API.Repositories.Implementations;

/// <summary>
/// Repository implementation for WorkoutObjective reference data
/// </summary>
public class WorkoutObjectiveRepository : 
    EmptyEnabledReferenceDataRepository<WorkoutObjective, WorkoutObjectiveId, FitnessDbContext>, 
    IWorkoutObjectiveRepository
{
    // All required methods are implemented by the base EmptyEnabledReferenceDataRepository class
}