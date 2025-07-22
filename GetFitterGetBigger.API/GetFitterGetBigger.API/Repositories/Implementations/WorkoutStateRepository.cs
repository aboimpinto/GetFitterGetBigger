using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;

namespace GetFitterGetBigger.API.Repositories.Implementations;

/// <summary>
/// Repository implementation for WorkoutState reference data
/// </summary>
public class WorkoutStateRepository : 
    EmptyEnabledReferenceDataRepository<WorkoutState, WorkoutStateId, FitnessDbContext>,
    IWorkoutStateRepository
{
    // Add any WorkoutState-specific repository methods here if needed
}