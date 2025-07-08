using System.Collections.Generic;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Services.Interfaces;

/// <summary>
/// Service interface for exercise type operations
/// </summary>
public interface IExerciseTypeService
{
    /// <summary>
    /// Checks if an exercise type exists
    /// </summary>
    /// <param name="id">The exercise type ID to check</param>
    /// <returns>True if the exercise type exists, false otherwise</returns>
    Task<bool> ExistsAsync(ExerciseTypeId id);

    /// <summary>
    /// Checks if all exercise types in a collection exist
    /// </summary>
    /// <param name="ids">The exercise type IDs to check</param>
    /// <returns>True if all exercise types exist, false otherwise</returns>
    Task<bool> AllExistAsync(IEnumerable<ExerciseTypeId> ids);
}