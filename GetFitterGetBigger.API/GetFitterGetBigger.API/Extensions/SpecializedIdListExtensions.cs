using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands;
using System.Linq;

namespace GetFitterGetBigger.API.Extensions;

public static class SpecializedIdListExtensions
{
    public static List<ExerciseTypeId> ParseExerciseTypeIds(this List<string>? stringIds)
    {
        if (stringIds == null || stringIds.Count == 0)
            return new List<ExerciseTypeId>();
        
        return stringIds
            .Select(id => ExerciseTypeId.ParseOrEmpty(id))
            .Where(id => !id.IsEmpty)
            .Distinct() // Deduplicate exercise type IDs
            .ToList();
    }
    
    public static List<MuscleGroupId> ParseMuscleGroupIds(this List<string>? stringIds)
    {
        if (stringIds == null || stringIds.Count == 0)
            return new List<MuscleGroupId>();
        
        return stringIds
            .Select(id => MuscleGroupId.ParseOrEmpty(id))
            .Where(id => !id.IsEmpty)
            .ToList();
    }
    
    public static List<EquipmentId> ParseEquipmentIds(this List<string>? stringIds)
    {
        if (stringIds == null || stringIds.Count == 0)
            return new List<EquipmentId>();
        
        return stringIds
            .Select(id => EquipmentId.ParseOrEmpty(id))
            .Where(id => !id.IsEmpty)
            .ToList();
    }
    
    public static List<MovementPatternId> ParseMovementPatternIds(this List<string>? stringIds)
    {
        if (stringIds == null || stringIds.Count == 0)
            return new List<MovementPatternId>();
        
        return stringIds
            .Select(id => MovementPatternId.ParseOrEmpty(id))
            .Where(id => !id.IsEmpty)
            .ToList();
    }
    
    public static List<BodyPartId> ParseBodyPartIds(this List<string>? stringIds)
    {
        if (stringIds == null || stringIds.Count == 0)
            return new List<BodyPartId>();
        
        return stringIds
            .Select(id => BodyPartId.ParseOrEmpty(id))
            .Where(id => !id.IsEmpty)
            .ToList();
    }
    
    public static List<MuscleGroupAssignment> ParseMuscleGroupAssignments(this List<MuscleGroupWithRoleRequest>? muscleGroups)
    {
        if (muscleGroups == null || muscleGroups.Count == 0)
            return new List<MuscleGroupAssignment>();

        var result = new List<MuscleGroupAssignment>();
        foreach (var mg in muscleGroups)
        {
            var muscleGroupId = MuscleGroupId.ParseOrEmpty(mg.MuscleGroupId);
            var muscleRoleId = MuscleRoleId.ParseOrEmpty(mg.MuscleRoleId);
            
            // Only add if both IDs are valid (not empty)
            if (!muscleGroupId.IsEmpty && !muscleRoleId.IsEmpty)
            {
                result.Add(new MuscleGroupAssignment
                {
                    MuscleGroupId = muscleGroupId,
                    MuscleRoleId = muscleRoleId
                });
            }
        }
        
        return result;
    }
    
    public static List<CoachNoteCommand> ParseCoachNoteCommands(this List<CoachNoteRequest>? coachNotes)
    {
        if (coachNotes == null || coachNotes.Count == 0)
            return new List<CoachNoteCommand>();

        var result = new List<CoachNoteCommand>();
        foreach (var note in coachNotes)
        {
            if (string.IsNullOrWhiteSpace(note.Text))
                continue;

            result.Add(new CoachNoteCommand
            {
                Id = CoachNoteId.ParseOrEmpty(note.Id),
                Text = note.Text,
                Order = note.Order
            });
        }
        
        return result;
    }
}