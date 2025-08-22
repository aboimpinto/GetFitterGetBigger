using GetFitterGetBigger.API.Models.Enums;

namespace GetFitterGetBigger.API.Services.Exercise.Features.Links.Commands;

/// <summary>
/// Command for creating a new exercise link
/// </summary>
public record CreateExerciseLinkCommand
{
    /// <summary>
    /// The ID of the source exercise
    /// </summary>
    public string SourceExerciseId { get; init; } = string.Empty;
    
    /// <summary>
    /// The ID of the target exercise to link
    /// </summary>
    public string TargetExerciseId { get; init; } = string.Empty;
    
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
        string sourceExerciseId,
        string targetExerciseId,
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
    public ExerciseLinkType ActualLinkType => LinkTypeEnum ?? 
        (LinkType == "Warmup" ? ExerciseLinkType.WARMUP : 
         LinkType == "Cooldown" ? ExerciseLinkType.COOLDOWN :
         Enum.TryParse<ExerciseLinkType>(LinkType, out var enumValue) ? enumValue : 
         ExerciseLinkType.WARMUP); // Default fallback
}