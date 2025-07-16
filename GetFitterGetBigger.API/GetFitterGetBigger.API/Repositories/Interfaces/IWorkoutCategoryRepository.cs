using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for WorkoutCategory reference data with Empty pattern support
/// </summary>
public interface IWorkoutCategoryRepository : IEmptyEnabledReferenceDataRepository<WorkoutCategory, WorkoutCategoryId>
{
    // Add any WorkoutCategory-specific repository methods here if needed
}