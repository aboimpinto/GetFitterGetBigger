using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands;

namespace GetFitterGetBigger.API.Services.Implementations.Extensions;

/// <summary>
/// Extension methods for mapping command data to Exercise related entities
/// </summary>
public static class ExerciseCommandExtensions
{
    /// <summary>
    /// Maps exercise type IDs to ExerciseExerciseType entities
    /// </summary>
    public static ICollection<ExerciseExerciseType> ToExerciseTypes(
        this List<ExerciseTypeId> exerciseTypeIds, 
        ExerciseId exerciseId)
    {
        return exerciseTypeIds
            .Where(id => !id.IsEmpty)
            .Select(id => ExerciseExerciseType.Handler.Create(exerciseId, id))
            .ToList();
    }
    
    /// <summary>
    /// Maps muscle group assignments to ExerciseMuscleGroup entities
    /// </summary>
    public static ICollection<ExerciseMuscleGroup> ToExerciseMuscleGroups(
        this List<MuscleGroupAssignment> muscleGroups, 
        ExerciseId exerciseId)
    {
        return muscleGroups
            .Where(mg => !mg.MuscleGroupId.IsEmpty && !mg.MuscleRoleId.IsEmpty)
            .Select(mg => ExerciseMuscleGroup.Handler.Create(exerciseId, mg.MuscleGroupId, mg.MuscleRoleId))
            .ToList();
    }
    
    /// <summary>
    /// Maps coach note commands to CoachNote entities
    /// </summary>
    public static ICollection<CoachNote> ToCoachNotes(
        this List<CoachNoteCommand> coachNotes, 
        ExerciseId exerciseId)
    {
        return coachNotes
            .Where(cn => !string.IsNullOrWhiteSpace(cn.Text))
            .Select(cn => cn.Id.HasValue && !cn.Id.Value.IsEmpty
                ? CoachNote.Handler.Create(cn.Id.Value, exerciseId, cn.Text, cn.Order)
                : CoachNote.Handler.CreateNew(exerciseId, cn.Text, cn.Order))
            .ToList();
    }
    
    /// <summary>
    /// Maps equipment IDs to ExerciseEquipment entities
    /// </summary>
    public static ICollection<ExerciseEquipment> ToExerciseEquipment(
        this List<EquipmentId> equipmentIds, 
        ExerciseId exerciseId)
    {
        return equipmentIds
            .Where(id => !id.IsEmpty)
            .Select(id => ExerciseEquipment.Handler.Create(exerciseId, id))
            .ToList();
    }
    
    /// <summary>
    /// Maps body part IDs to ExerciseBodyPart entities
    /// </summary>
    public static ICollection<ExerciseBodyPart> ToExerciseBodyParts(
        this List<BodyPartId> bodyPartIds, 
        ExerciseId exerciseId)
    {
        return bodyPartIds
            .Where(id => !id.IsEmpty)
            .Select(id => ExerciseBodyPart.Handler.Create(exerciseId, id))
            .ToList();
    }
    
    /// <summary>
    /// Maps movement pattern IDs to ExerciseMovementPattern entities
    /// </summary>
    public static ICollection<ExerciseMovementPattern> ToExerciseMovementPatterns(
        this List<MovementPatternId> movementPatternIds, 
        ExerciseId exerciseId)
    {
        return movementPatternIds
            .Where(id => !id.IsEmpty)
            .Select(id => ExerciseMovementPattern.Handler.Create(exerciseId, id))
            .ToList();
    }
}