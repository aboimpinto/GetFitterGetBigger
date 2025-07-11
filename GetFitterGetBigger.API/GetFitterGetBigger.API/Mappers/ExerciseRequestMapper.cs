using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands;

namespace GetFitterGetBigger.API.Mappers;

/// <summary>
/// Maps between web request DTOs (with string IDs) and service commands (with specialized IDs).
/// This enforces proper separation between web layer and service layer concerns.
/// </summary>
public static class ExerciseRequestMapper
{
    /// <summary>
    /// Maps CreateExerciseRequest (web DTO) to CreateExerciseCommand (service command)
    /// </summary>
    public static CreateExerciseCommand ToCommand(this CreateExerciseRequest request)
    {
        return new CreateExerciseCommand
        {
            Name = request.Name,
            Description = request.Description,
            VideoUrl = request.VideoUrl,
            ImageUrl = request.ImageUrl,
            IsUnilateral = request.IsUnilateral,
            
            // Convert string IDs to specialized IDs
            DifficultyId = ParseDifficultyLevelId(request.DifficultyId),
            KineticChainId = ParseKineticChainTypeId(request.KineticChainId),
            ExerciseWeightTypeId = ParseExerciseWeightTypeId(request.ExerciseWeightTypeId),
            ExerciseTypeIds = ParseExerciseTypeIds(request.ExerciseTypeIds),
            MuscleGroups = MapMuscleGroups(request.MuscleGroups),
            CoachNotes = MapCoachNotes(request.CoachNotes),
            EquipmentIds = ParseEquipmentIds(request.EquipmentIds),
            BodyPartIds = ParseBodyPartIds(request.BodyPartIds),
            MovementPatternIds = ParseMovementPatternIds(request.MovementPatternIds)
        };
    }

    /// <summary>
    /// Maps UpdateExerciseRequest (web DTO) to UpdateExerciseCommand (service command)
    /// </summary>
    public static UpdateExerciseCommand ToCommand(this UpdateExerciseRequest request)
    {
        return new UpdateExerciseCommand
        {
            Name = request.Name,
            Description = request.Description,
            VideoUrl = request.VideoUrl,
            ImageUrl = request.ImageUrl,
            IsUnilateral = request.IsUnilateral ?? false,
            IsActive = request.IsActive ?? true,
            
            // Convert string IDs to specialized IDs
            DifficultyId = ParseDifficultyLevelId(request.DifficultyId),
            KineticChainId = ParseKineticChainTypeId(request.KineticChainId),
            ExerciseWeightTypeId = ParseExerciseWeightTypeId(request.ExerciseWeightTypeId),
            ExerciseTypeIds = ParseExerciseTypeIds(request.ExerciseTypeIds),
            MuscleGroups = MapMuscleGroups(request.MuscleGroups),
            CoachNotes = MapCoachNotes(request.CoachNotes),
            EquipmentIds = ParseEquipmentIds(request.EquipmentIds),
            BodyPartIds = ParseBodyPartIds(request.BodyPartIds),
            MovementPatternIds = ParseMovementPatternIds(request.MovementPatternIds)
        };
    }

    #region Private Parsing Methods

    private static DifficultyLevelId ParseDifficultyLevelId(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("DifficultyId is required");
            
        if (!id.StartsWith("difficultylevel-"))
            throw new ArgumentException($"Invalid DifficultyLevelId format: '{id}'. Expected format: 'difficultylevel-{{guid}}'");
        
        var guidPart = id["difficultylevel-".Length..];
        if (!Guid.TryParse(guidPart, out var guid))
            throw new ArgumentException($"Invalid GUID in DifficultyLevelId: '{guidPart}'");
        
        return DifficultyLevelId.From(guid);
    }

    private static KineticChainTypeId? ParseKineticChainTypeId(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;
            
        if (!id.StartsWith("kineticchaintype-"))
            throw new ArgumentException($"Invalid KineticChainTypeId format: '{id}'. Expected format: 'kineticchaintype-{{guid}}'");
        
        var guidPart = id["kineticchaintype-".Length..];
        if (!Guid.TryParse(guidPart, out var guid))
            throw new ArgumentException($"Invalid GUID in KineticChainTypeId: '{guidPart}'");
        
        return KineticChainTypeId.From(guid);
    }

    private static ExerciseWeightTypeId? ParseExerciseWeightTypeId(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;
            
        if (!id.StartsWith("exerciseweighttype-"))
            throw new ArgumentException($"Invalid ExerciseWeightTypeId format: '{id}'. Expected format: 'exerciseweighttype-{{guid}}'");
        
        var guidPart = id["exerciseweighttype-".Length..];
        if (!Guid.TryParse(guidPart, out var guid))
            throw new ArgumentException($"Invalid GUID in ExerciseWeightTypeId: '{guidPart}'");
        
        return ExerciseWeightTypeId.From(guid);
    }

    private static List<ExerciseTypeId> ParseExerciseTypeIds(List<string>? ids)
    {
        if (ids == null || !ids.Any())
            return new List<ExerciseTypeId>();

        var result = new List<ExerciseTypeId>();
        foreach (var id in ids)
        {
            if (string.IsNullOrWhiteSpace(id))
                continue;
                
            if (!id.StartsWith("exercisetype-"))
                continue; // Skip invalid IDs rather than throwing - service will validate
            
            var guidPart = id["exercisetype-".Length..];
            if (!Guid.TryParse(guidPart, out var guid))
                continue; // Skip invalid IDs rather than throwing - service will validate
            
            result.Add(ExerciseTypeId.From(guid));
        }
        
        return result;
    }

    private static List<MuscleGroupAssignment> MapMuscleGroups(List<MuscleGroupWithRoleRequest>? muscleGroups)
    {
        if (muscleGroups == null || !muscleGroups.Any())
            return new List<MuscleGroupAssignment>();

        var result = new List<MuscleGroupAssignment>();
        foreach (var mg in muscleGroups)
        {
            if (string.IsNullOrWhiteSpace(mg.MuscleGroupId) || string.IsNullOrWhiteSpace(mg.MuscleRoleId))
                continue;

            try
            {
                var muscleGroupId = ParseMuscleGroupId(mg.MuscleGroupId);
                var muscleRoleId = ParseMuscleRoleId(mg.MuscleRoleId);
                
                result.Add(new MuscleGroupAssignment
                {
                    MuscleGroupId = muscleGroupId,
                    MuscleRoleId = muscleRoleId
                });
            }
            catch
            {
                // Skip invalid muscle group assignments
                continue;
            }
        }
        
        return result;
    }

    private static List<CoachNoteCommand> MapCoachNotes(List<CoachNoteRequest>? coachNotes)
    {
        if (coachNotes == null || !coachNotes.Any())
            return new List<CoachNoteCommand>();

        var result = new List<CoachNoteCommand>();
        foreach (var note in coachNotes)
        {
            if (string.IsNullOrWhiteSpace(note.Text))
                continue;

            CoachNoteId? noteId = null;
            if (!string.IsNullOrWhiteSpace(note.Id))
            {
                try
                {
                    noteId = ParseCoachNoteId(note.Id);
                }
                catch
                {
                    // Invalid ID - will be handled by service validation
                }
            }
            
            result.Add(new CoachNoteCommand
            {
                Id = noteId,
                Text = note.Text,
                Order = note.Order
            });
        }
        
        return result;
    }

    private static MuscleGroupId ParseMuscleGroupId(string id)
    {
        if (!id.StartsWith("musclegroup-"))
            throw new ArgumentException($"Invalid MuscleGroupId format: '{id}'. Expected format: 'musclegroup-{{guid}}'");
        
        var guidPart = id["musclegroup-".Length..];
        if (!Guid.TryParse(guidPart, out var guid))
            throw new ArgumentException($"Invalid GUID in MuscleGroupId: '{guidPart}'");
        
        return MuscleGroupId.From(guid);
    }

    private static MuscleRoleId ParseMuscleRoleId(string id)
    {
        if (!id.StartsWith("musclerole-"))
            throw new ArgumentException($"Invalid MuscleRoleId format: '{id}'. Expected format: 'musclerole-{{guid}}'");
        
        var guidPart = id["musclerole-".Length..];
        if (!Guid.TryParse(guidPart, out var guid))
            throw new ArgumentException($"Invalid GUID in MuscleRoleId: '{guidPart}'");
        
        return MuscleRoleId.From(guid);
    }

    private static CoachNoteId ParseCoachNoteId(string id)
    {
        if (!id.StartsWith("coachnote-"))
            throw new ArgumentException($"Invalid CoachNoteId format: '{id}'. Expected format: 'coachnote-{{guid}}'");
        
        var guidPart = id["coachnote-".Length..];
        if (!Guid.TryParse(guidPart, out var guid))
            throw new ArgumentException($"Invalid GUID in CoachNoteId: '{guidPart}'");
        
        return CoachNoteId.From(guid);
    }

    private static List<EquipmentId> ParseEquipmentIds(List<string>? ids) =>
        ParseSpecializedIds(ids, "equipment-", EquipmentId.From);

    private static List<BodyPartId> ParseBodyPartIds(List<string>? ids) =>
        ParseSpecializedIds(ids, "bodypart-", BodyPartId.From);

    private static List<MovementPatternId> ParseMovementPatternIds(List<string>? ids) =>
        ParseSpecializedIds(ids, "movementpattern-", MovementPatternId.From);

    private static List<T> ParseSpecializedIds<T>(List<string>? ids, string prefix, Func<Guid, T> factory)
    {
        if (ids == null || !ids.Any())
            return new List<T>();

        var result = new List<T>();
        foreach (var id in ids)
        {
            if (string.IsNullOrWhiteSpace(id) || !id.StartsWith(prefix))
                continue;
                
            var guidPart = id[prefix.Length..];
            if (!Guid.TryParse(guidPart, out var guid))
                continue;
            
            result.Add(factory(guid));
        }
        
        return result;
    }

    #endregion
}