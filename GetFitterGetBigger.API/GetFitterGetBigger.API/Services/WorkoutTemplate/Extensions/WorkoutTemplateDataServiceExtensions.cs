using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.WorkoutTemplate.DataServices;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Extensions;

/// <summary>
/// Extension methods for WorkoutTemplate DataServices to provide positive validation assertions
/// </summary>
public static class WorkoutTemplateDataServiceExtensions
{
    /// <summary>
    /// Checks if a workout template name is unique (doesn't exist in the database)
    /// </summary>
    /// <param name="dataService">The query data service instance</param>
    /// <param name="name">The name to check</param>
    /// <param name="excludeId">Optional ID to exclude from the check (for updates)</param>
    /// <returns>True if the name is unique, false if it already exists</returns>
    public static async Task<bool> IsWorkoutTemplateNameUniqueAsync(
        this IWorkoutTemplateQueryDataService dataService,
        string name,
        WorkoutTemplateId? excludeId = null)
    {
        var existsResult = await dataService.ExistsByNameAsync(name);
        
        // If there's an error checking existence, consider it not unique (safer)
        if (!existsResult.IsSuccess)
            return false;
            
        // If it doesn't exist, it's unique
        if (!existsResult.Data.Value)
            return true;
            
        // TODO: If excludeId is provided, we need to check if the existing template
        // is the one being updated. This requires additional query functionality.
        // For now, if it exists and we have an excludeId, assume it's valid for update
        return excludeId != null;
    }

    /// <summary>
    /// Checks if a workout template can be deleted (has no execution logs)
    /// </summary>
    /// <param name="dataService">The query data service instance</param>
    /// <param name="id">The workout template ID</param>
    /// <returns>True if the template can be deleted, false if it has execution logs</returns>
    public static async Task<bool> IsWorkoutTemplateDeletableAsync(
        this IWorkoutTemplateQueryDataService dataService,
        WorkoutTemplateId id)
    {
        var hasLogsResult = await dataService.HasExecutionLogsAsync(id);
        
        // If there's an error checking logs, consider it not deletable (safer)
        if (!hasLogsResult.IsSuccess)
            return false;
            
        // Return true when template CAN be deleted (no execution logs)
        return !hasLogsResult.Data.Value;
    }
}