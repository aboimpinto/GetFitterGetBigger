using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Services.Commands.MuscleGroup;

/// <summary>
/// Command for updating existing muscle group
/// </summary>
public record UpdateMuscleGroupCommand
{
    /// <summary>
    /// The name of the muscle group
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// The ID of the body part this muscle group belongs to
    /// </summary>
    public required BodyPartId BodyPartId { get; init; }
}