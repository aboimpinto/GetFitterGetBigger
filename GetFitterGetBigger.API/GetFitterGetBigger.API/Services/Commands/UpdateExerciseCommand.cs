using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Services.Commands;

/// <summary>
/// Service command for updating exercises with proper domain types and specialized IDs.
/// This is the domain object that services work with, separate from web request DTOs.
/// </summary>
public class UpdateExerciseCommand
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public string? VideoUrl { get; init; }
    public string? ImageUrl { get; init; }
    public bool IsUnilateral { get; init; }
    public bool IsActive { get; init; }
    
    // Domain IDs - not strings!
    public required DifficultyLevelId DifficultyId { get; init; }
    public KineticChainTypeId? KineticChainId { get; init; }
    public ExerciseWeightTypeId? ExerciseWeightTypeId { get; init; }
    public required List<ExerciseTypeId> ExerciseTypeIds { get; init; } = new();
    public required List<MuscleGroupAssignment> MuscleGroups { get; init; } = new();
    public List<CoachNoteCommand> CoachNotes { get; init; } = new();
    public List<EquipmentId> EquipmentIds { get; init; } = new();
    public List<BodyPartId> BodyPartIds { get; init; } = new();
    public List<MovementPatternId> MovementPatternIds { get; init; } = new();
}