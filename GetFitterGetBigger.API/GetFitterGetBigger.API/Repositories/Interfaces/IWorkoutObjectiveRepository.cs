using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for WorkoutObjective reference data
/// </summary>
public interface IWorkoutObjectiveRepository : IReferenceDataRepository<WorkoutObjective, WorkoutObjectiveId>
{
    // Add any WorkoutObjective-specific repository methods here if needed
}