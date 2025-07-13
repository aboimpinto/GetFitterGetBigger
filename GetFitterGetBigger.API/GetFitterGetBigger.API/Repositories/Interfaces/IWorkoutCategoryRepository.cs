using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for WorkoutCategory reference data
/// </summary>
public interface IWorkoutCategoryRepository : IReferenceDataRepository<WorkoutCategory, WorkoutCategoryId>
{
    // Add any WorkoutCategory-specific repository methods here if needed
}