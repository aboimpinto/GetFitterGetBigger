using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Extensions;
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
            
            // Convert string IDs to specialized IDs using ParseOrEmpty
            DifficultyId = DifficultyLevelId.ParseOrEmpty(request.DifficultyId),
            KineticChainId = KineticChainTypeId.ParseOrEmpty(request.KineticChainId),
            ExerciseWeightTypeId = ExerciseWeightTypeId.ParseOrEmpty(request.ExerciseWeightTypeId),
            ExerciseTypeIds = request.ExerciseTypeIds.ParseExerciseTypeIds(),
            MuscleGroups = request.MuscleGroups.ParseMuscleGroupAssignments(),
            CoachNotes = request.CoachNotes.ParseCoachNoteCommands(),
            EquipmentIds = request.EquipmentIds.ParseEquipmentIds(),
            BodyPartIds = request.BodyPartIds.ParseBodyPartIds(),
            MovementPatternIds = request.MovementPatternIds.ParseMovementPatternIds()
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
            
            // Convert string IDs to specialized IDs using ParseOrEmpty
            DifficultyId = DifficultyLevelId.ParseOrEmpty(request.DifficultyId),
            KineticChainId = KineticChainTypeId.ParseOrEmpty(request.KineticChainId),
            ExerciseWeightTypeId = ExerciseWeightTypeId.ParseOrEmpty(request.ExerciseWeightTypeId),
            ExerciseTypeIds = request.ExerciseTypeIds.ParseExerciseTypeIds(),
            MuscleGroups = request.MuscleGroups.ParseMuscleGroupAssignments(),
            CoachNotes = request.CoachNotes.ParseCoachNoteCommands(),
            EquipmentIds = request.EquipmentIds.ParseEquipmentIds(),
            BodyPartIds = request.BodyPartIds.ParseBodyPartIds(),
            MovementPatternIds = request.MovementPatternIds.ParseMovementPatternIds()
        };
    }

}