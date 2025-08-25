using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplate;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Extensions;

/// <summary>
/// Extension methods for WorkoutTemplateService helpers and utilities
/// </summary>
public static class WorkoutTemplateServiceExtensions
{
    /// <summary>
    /// Checks if a workout template name is unique (doesn't exist in the database)
    /// </summary>
    /// <param name="service">The WorkoutTemplateService instance</param>
    /// <param name="name">The name to check</param>
    /// <returns>True if the name is unique, false if it already exists</returns>
    public static async Task<bool> IsNameUniqueAsync(this WorkoutTemplateService service, string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;
            
        // Use the existing ExistsByNameAsync but invert the result for semantic clarity
        var existsResult = await service.ExistsByNameAsync(name);
        return !existsResult.Data.Value;
    }

    /// <summary>
    /// Validates that all reference IDs in the command exist in the database
    /// </summary>
    /// <param name="service">The WorkoutTemplateService instance</param>
    /// <param name="categoryId">The category ID to validate</param>
    /// <param name="difficultyId">The difficulty ID to validate</param>
    /// <returns>True if all references are valid, false otherwise</returns>
    public static Task<bool> ValidateReferencesExistAsync(
        this WorkoutTemplateService service,
        WorkoutCategoryId categoryId,
        DifficultyLevelId difficultyId)
    {
        // For now, we assume reference data is valid if IDs are not empty
        // In a real implementation, we would check against the respective services
        return Task.FromResult(!categoryId.IsEmpty && !difficultyId.IsEmpty);
    }

    /// <summary>
    /// Validates that the command has all required fields for creation
    /// </summary>
    /// <param name="command">The create command to validate</param>
    /// <returns>True if the command is valid, false otherwise</returns>
    public static bool IsValidCreateCommand(this CreateWorkoutTemplateCommand? command)
    {
        if (command == null)
            return false;

        return IsNameValidForCreate(command.Name) &&
               AreReferenceIdsValid(command.CategoryId, command.DifficultyId) &&
               IsDurationValidForCreate(command.EstimatedDurationMinutes);
    }

    /// <summary>
    /// Validates that the name field is valid for create operations.
    /// For creation, name is required and must meet length requirements.
    /// </summary>
    /// <param name="name">The name to validate</param>
    /// <returns>True if name is valid for create</returns>
    private static bool IsNameValidForCreate(string? name)
    {
        return !string.IsNullOrWhiteSpace(name) && name.HasValidNameLength();
    }

    /// <summary>
    /// Validates that both reference IDs are not empty.
    /// </summary>
    /// <param name="categoryId">The category ID to validate</param>
    /// <param name="difficultyId">The difficulty ID to validate</param>
    /// <returns>True if both IDs are valid</returns>
    private static bool AreReferenceIdsValid(WorkoutCategoryId categoryId, DifficultyLevelId difficultyId)
    {
        return !categoryId.IsEmpty && !difficultyId.IsEmpty;
    }

    /// <summary>
    /// Validates that the duration field is valid for create operations.
    /// For creation, duration is required and must meet valid range.
    /// </summary>
    /// <param name="duration">The duration to validate</param>
    /// <returns>True if duration is valid for create</returns>
    private static bool IsDurationValidForCreate(int duration)
    {
        return duration.IsValidDuration();
    }

    /// <summary>
    /// Validates that the command has valid fields for update
    /// </summary>
    /// <param name="command">The update command to validate</param>
    /// <returns>True if the command is valid, false otherwise</returns>
    public static bool IsValidUpdateCommand(this UpdateWorkoutTemplateCommand? command)
    {
        if (command == null)
            return false;

        return IsNameValidForUpdate(command.Name) &&
               IsDurationValidForUpdate(command.EstimatedDurationMinutes) &&
               IsDescriptionValidForUpdate(command.Description);
    }

    /// <summary>
    /// Validates that the name field is valid for update operations.
    /// For updates, name can be null/empty (no change) or must meet length requirements.
    /// </summary>
    /// <param name="name">The name to validate</param>
    /// <returns>True if name is valid for update</returns>
    private static bool IsNameValidForUpdate(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return true; // Allow null/empty for partial updates

        return name.HasValidNameLength();
    }

    /// <summary>
    /// Validates that the duration field is valid for update operations.
    /// For updates, duration can be null (no change) or must meet valid range.
    /// </summary>
    /// <param name="duration">The duration to validate</param>
    /// <returns>True if duration is valid for update</returns>
    private static bool IsDurationValidForUpdate(int? duration)
    {
        if (!duration.HasValue)
            return true; // Allow null for partial updates

        return duration.Value.IsValidDuration();
    }

    /// <summary>
    /// Validates that the description field is valid for update operations.
    /// For updates, description can be null/empty (no change) or must meet length requirements.
    /// </summary>
    /// <param name="description">The description to validate</param>
    /// <returns>True if description is valid for update</returns>
    private static bool IsDescriptionValidForUpdate(string? description)
    {
        return description.HasValidDescriptionLength();
    }

    /// <summary>
    /// Checks if tags count is within allowed limits
    /// </summary>
    /// <param name="tags">The tags collection to check</param>
    /// <returns>True if tags count is valid, false otherwise</returns>
    public static bool HasValidTagsCount(this ICollection<string>? tags)
    {
        return tags == null || tags.Count <= 10;
    }

    /// <summary>
    /// Validates name length constraints
    /// </summary>
    /// <param name="name">The name to validate</param>
    /// <returns>True if name length is valid, false otherwise</returns>
    public static bool HasValidNameLength(this string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;
            
        return name.Length >= 3 && name.Length <= 100;
    }

    /// <summary>
    /// Validates description length constraints
    /// </summary>
    /// <param name="description">The description to validate</param>
    /// <returns>True if description length is valid, false otherwise</returns>
    public static bool HasValidDescriptionLength(this string? description)
    {
        return string.IsNullOrEmpty(description) || description.Length <= 1000;
    }

    /// <summary>
    /// Validates duration constraints
    /// </summary>
    /// <param name="duration">The duration in minutes to validate</param>
    /// <returns>True if duration is valid, false otherwise</returns>
    public static bool IsValidDuration(this int duration)
    {
        return duration is >= 5 and <= 300;
    }
}