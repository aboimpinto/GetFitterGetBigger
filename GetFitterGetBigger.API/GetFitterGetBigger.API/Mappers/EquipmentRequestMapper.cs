using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Services.Commands.Equipment;

namespace GetFitterGetBigger.API.Mappers;

/// <summary>
/// Maps between web request DTOs (with string IDs) and service commands.
/// This enforces proper separation between web layer and service layer concerns.
/// </summary>
public static class EquipmentRequestMapper
{
    /// <summary>
    /// Maps CreateEquipmentDto (web DTO) to CreateEquipmentCommand (service command)
    /// Always returns a valid command object, never null
    /// </summary>
    public static CreateEquipmentCommand ToCommand(this CreateEquipmentDto request)
    {
        return new CreateEquipmentCommand
        {
            Name = request?.Name ?? string.Empty
        };
    }
    
    /// <summary>
    /// Maps UpdateEquipmentDto (web DTO) to UpdateEquipmentCommand (service command)
    /// Always returns a valid command object, never null
    /// </summary>
    public static UpdateEquipmentCommand ToCommand(this UpdateEquipmentDto request)
    {
        return new UpdateEquipmentCommand
        {
            Name = request?.Name ?? string.Empty
        };
    }
}