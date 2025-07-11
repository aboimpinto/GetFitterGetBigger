using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Services.Commands;

/// <summary>
/// Service command for creating exercises with proper domain types and specialized IDs.
/// This is the domain object that services work with, separate from web request DTOs.
/// </summary>
public class CreateExerciseCommand
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public string? VideoUrl { get; init; }
    public string? ImageUrl { get; init; }
    public bool IsUnilateral { get; init; }
    
    // Domain IDs - not strings!
    public required DifficultyLevelId DifficultyId { get; init; }
    public KineticChainTypeId KineticChainId { get; init; }
    public ExerciseWeightTypeId ExerciseWeightTypeId { get; init; }
    public required List<ExerciseTypeId> ExerciseTypeIds { get; init; } = new();
    public required List<MuscleGroupAssignment> MuscleGroups { get; init; } = new();
    public List<CoachNoteCommand> CoachNotes { get; init; } = new();
    public List<EquipmentId> EquipmentIds { get; init; } = new();
    public List<BodyPartId> BodyPartIds { get; init; } = new();
    public List<MovementPatternId> MovementPatternIds { get; init; } = new();
}

/// <summary>
/// Represents a muscle group assignment with its role
/// </summary>
public class MuscleGroupAssignment
{
    public required MuscleGroupId MuscleGroupId { get; init; }
    public required MuscleRoleId MuscleRoleId { get; init; }
}

/// <summary>
/// Represents a coach note in service commands
/// </summary>
public class CoachNoteCommand
{
    /// <summary>
    /// For updates: existing note ID. For creates: null
    /// </summary>
    public CoachNoteId? Id { get; init; }
    public required string Text { get; init; }
    public required int Order { get; init; }
}