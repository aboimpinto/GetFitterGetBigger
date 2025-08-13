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

        return !string.IsNullOrWhiteSpace(command.Name) &&
               !command.CategoryId.IsEmpty &&
               !command.DifficultyId.IsEmpty &&
               command.EstimatedDurationMinutes is >= 5 and <= 300;
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

        // For update, we allow partial updates, so we only validate if fields are provided
        var isNameValid = string.IsNullOrWhiteSpace(command.Name) || 
                         (command.Name.Length >= 3 && command.Name.Length <= 100);
                         
        var isDurationValid = !command.EstimatedDurationMinutes.HasValue || 
                             (command.EstimatedDurationMinutes >= 5 && command.EstimatedDurationMinutes <= 300);
                             
        var isDescriptionValid = string.IsNullOrEmpty(command.Description) || 
                                command.Description.Length <= 1000;

        return isNameValid && isDurationValid && isDescriptionValid;
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