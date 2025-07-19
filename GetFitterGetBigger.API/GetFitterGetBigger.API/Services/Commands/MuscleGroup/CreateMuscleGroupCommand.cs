using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Services.Commands.MuscleGroup;

/// <summary>
/// Command for creating new muscle group
/// </summary>
public record CreateMuscleGroupCommand
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