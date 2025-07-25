namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for set order information
/// </summary>
public class SetOrderDto
{
    /// <summary>
    /// The set configuration ID
    /// <example>setconfiguration-550e8400-e29b-41d4-a716-446655440000</example>
    /// </summary>
    [System.ComponentModel.DataAnnotations.Required]
    public required string SetId { get; init; }

    /// <summary>
    /// The new set number
    /// <example>1</example>
    /// </summary>
    [System.ComponentModel.DataAnnotations.Required]
    public required int SetNumber { get; init; }
}