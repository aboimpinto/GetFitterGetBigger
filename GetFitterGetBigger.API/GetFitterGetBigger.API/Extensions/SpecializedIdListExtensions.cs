using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Extensions;

public static class SpecializedIdListExtensions
{
    public static List<MuscleGroupId> ParseMuscleGroupIds(this List<string>? stringIds)
    {
        if (stringIds == null || stringIds.Count == 0)
            return new List<MuscleGroupId>();
        
        var result = new List<MuscleGroupId>();
        foreach (var stringId in stringIds)
        {
            if (MuscleGroupId.TryParse(stringId, out var id))
                result.Add(id);
        }
        return result;
    }
    
    public static List<EquipmentId> ParseEquipmentIds(this List<string>? stringIds)
    {
        if (stringIds == null || stringIds.Count == 0)
            return new List<EquipmentId>();
        
        var result = new List<EquipmentId>();
        foreach (var stringId in stringIds)
        {
            if (EquipmentId.TryParse(stringId, out var id))
                result.Add(id);
        }
        return result;
    }
    
    public static List<MovementPatternId> ParseMovementPatternIds(this List<string>? stringIds)
    {
        if (stringIds == null || stringIds.Count == 0)
            return new List<MovementPatternId>();
        
        var result = new List<MovementPatternId>();
        foreach (var stringId in stringIds)
        {
            if (MovementPatternId.TryParse(stringId, out var id))
                result.Add(id);
        }
        return result;
    }
    
    public static List<BodyPartId> ParseBodyPartIds(this List<string>? stringIds)
    {
        if (stringIds == null || stringIds.Count == 0)
            return new List<BodyPartId>();
        
        var result = new List<BodyPartId>();
        foreach (var stringId in stringIds)
        {
            if (BodyPartId.TryParse(stringId, out var id))
                result.Add(id);
        }
        return result;
    }
}