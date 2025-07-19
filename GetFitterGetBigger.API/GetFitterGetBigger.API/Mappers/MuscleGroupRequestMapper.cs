using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.MuscleGroup;

namespace GetFitterGetBigger.API.Mappers;

/// <summary>
/// Maps between web request DTOs (with string IDs) and service commands.
/// This enforces proper separation between web layer and service layer concerns.
/// </summary>
public static class MuscleGroupRequestMapper
{
    /// <summary>
    /// Maps CreateMuscleGroupDto (web DTO) to CreateMuscleGroupCommand (service command)
    /// Always returns a valid command object, never null
    /// </summary>
    public static CreateMuscleGroupCommand ToCommand(this CreateMuscleGroupDto request)
    {
        return new CreateMuscleGroupCommand
        {
            Name = request?.Name ?? string.Empty,
            BodyPartId = BodyPartId.ParseOrEmpty(request?.BodyPartId ?? string.Empty)
        };
    }
    
    /// <summary>
    /// Maps UpdateMuscleGroupDto (web DTO) to UpdateMuscleGroupCommand (service command)
    /// Always returns a valid command object, never null
    /// </summary>
    public static UpdateMuscleGroupCommand ToCommand(this UpdateMuscleGroupDto request)
    {
        return new UpdateMuscleGroupCommand
        {
            Name = request?.Name ?? string.Empty,
            BodyPartId = BodyPartId.ParseOrEmpty(request?.BodyPartId ?? string.Empty)
        };
    }
}