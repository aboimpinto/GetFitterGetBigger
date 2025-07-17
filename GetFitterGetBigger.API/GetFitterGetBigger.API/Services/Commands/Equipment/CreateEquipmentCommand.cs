namespace GetFitterGetBigger.API.Services.Commands.Equipment;

/// <summary>
/// Command for creating new equipment
/// </summary>
public record CreateEquipmentCommand
{
    /// <summary>
    /// The name of the equipment
    /// </summary>
    public required string Name { get; init; }
}