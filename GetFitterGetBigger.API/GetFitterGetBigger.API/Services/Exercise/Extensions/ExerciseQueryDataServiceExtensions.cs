using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Exercise.DataServices;

namespace GetFitterGetBigger.API.Services.Exercise.Extensions;

/// <summary>
/// Extension methods for IExerciseQueryDataService to support positive validation patterns
/// </summary>
public static class ExerciseQueryDataServiceExtensions
{
    /// <summary>
    /// Checks if an exercise name is unique (doesn't exist).
    /// Returns true when the name IS unique, false when it already exists.
    /// </summary>
    /// <param name="dataService">The data service instance</param>
    /// <param name="name">The exercise name to check</param>
    /// <param name="excludeId">Optional ID to exclude from uniqueness check (for updates)</param>
    /// <returns>True if the name is unique, false if it already exists</returns>
    public static async Task<bool> IsExerciseNameUniqueAsync(
        this IExerciseQueryDataService dataService,
        string name,
        ExerciseId? excludeId = null)
    {
        var existsResult = await dataService.ExistsByNameAsync(name, excludeId);
        return existsResult.IsSuccess && !existsResult.Data.Value;
    }
}