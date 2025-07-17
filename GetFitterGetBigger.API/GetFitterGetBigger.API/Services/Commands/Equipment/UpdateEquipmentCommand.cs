namespace GetFitterGetBigger.API.Services.Commands.Equipment;

/// <summary>
/// Command for updating existing equipment
/// </summary>
public record UpdateEquipmentCommand
{
    /// <summary>
    /// The new name for the equipment
    /// </summary>
    public required string Name { get; init; }
}