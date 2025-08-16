using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for WorkoutState reference data
/// </summary>
public interface IWorkoutStateRepository : IReferenceDataRepository<WorkoutState, WorkoutStateId>
{
    // Add any WorkoutState-specific repository methods here if needed
}