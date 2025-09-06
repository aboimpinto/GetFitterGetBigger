using GetFitterGetBigger.API.Models.Enums;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Services.Exercise.Features.Links.Commands;

/// <summary>
/// Command for creating a new exercise link
/// </summary>
public record CreateExerciseLinkCommand
{
    /// <summary>
    /// The ID of the source exercise
    /// </summary>
    public ExerciseId SourceExerciseId { get; init; } = ExerciseId.Empty;
    
    /// <summary>
    /// The ID of the target exercise to link
    /// </summary>
    public ExerciseId TargetExerciseId { get; init; } = ExerciseId.Empty;
    
    /// <summary>
    /// The type of link (supports both string and enum)
    /// </summary>
    public string LinkType { get; init; } = string.Empty;
    
    /// <summary>
    /// The display order for this link
    /// </summary>
    public int DisplayOrder { get; init; }
    
    /// <summary>
    /// The enum-based link type for enhanced functionality
    /// </summary>
    public ExerciseLinkType? LinkTypeEnum { get; init; }
    
    /// <summary>
    /// Default constructor for object initialization syntax (backward compatibility)
    /// </summary>
    public CreateExerciseLinkCommand() { }
    
    /// <summary>
    /// Creates a command using enum-based link type (enhanced functionality)
    /// </summary>
    /// <param name="sourceExerciseId">The source exercise ID</param>
    /// <param name="targetExerciseId">The target exercise ID</param>
    /// <param name="linkType">The enum-based link type</param>
    /// <param name="displayOrder">The display order for this link</param>
    public CreateExerciseLinkCommand(
        ExerciseId sourceExerciseId,
        ExerciseId targetExerciseId,
        ExerciseLinkType linkType,
        int displayOrder)
    {
        SourceExerciseId = sourceExerciseId;
        TargetExerciseId = targetExerciseId;
        LinkType = linkType.ToString();
        DisplayOrder = displayOrder;
        LinkTypeEnum = linkType;
    }
    
    /// <summary>
    /// Gets the actual link type as enum, handling backward compatibility with string values
    /// </summary>
    public ExerciseLinkType ActualLinkType => LinkTypeEnum ?? ConvertStringToEnum(LinkType);
    
    /// <summary>
    /// Converts string link type to enum, handling backward compatibility and edge cases
    /// </summary>
    /// <param name="linkTypeString">The string representation of the link type</param>
    /// <returns>The corresponding ExerciseLinkType enum value</returns>
    private static ExerciseLinkType ConvertStringToEnum(string linkTypeString)
    {
        if (string.IsNullOrWhiteSpace(linkTypeString))
        {
            return ExerciseLinkType.WARMUP; // Default fallback
        }
        
        // Handle legacy string values
        if (IsLegacyWarmupValue(linkTypeString))
        {
            return ExerciseLinkType.WARMUP;
        }
        
        if (IsLegacyCooldownValue(linkTypeString))
        {
            return ExerciseLinkType.COOLDOWN;
        }
        
        // Try standard enum parsing, but only if it's a valid defined enum value
        if (Enum.TryParse<ExerciseLinkType>(linkTypeString, out var enumValue) && Enum.IsDefined(typeof(ExerciseLinkType), enumValue))
        {
            return enumValue;
        }
        
        return ExerciseLinkType.WARMUP; // Default fallback for invalid values
    }
    
    /// <summary>
    /// Checks if the string represents a legacy warmup value
    /// </summary>
    /// <param name="value">The string value to check</param>
    /// <returns>True if it's a legacy warmup value</returns>
    private static bool IsLegacyWarmupValue(string value)
    {
        return string.Equals(value, "Warmup", StringComparison.OrdinalIgnoreCase);
    }
    
    /// <summary>
    /// Checks if the string represents a legacy cooldown value
    /// </summary>
    /// <param name="value">The string value to check</param>
    /// <returns>True if it's a legacy cooldown value</returns>
    private static bool IsLegacyCooldownValue(string value)
    {
        return string.Equals(value, "Cooldown", StringComparison.OrdinalIgnoreCase);
    }
}