namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for reordering set configurations
/// </summary>
public class ReorderSetConfigurationsDto
{
    /// <summary>
    /// List of set orders
    /// </summary>
    [System.ComponentModel.DataAnnotations.Required]
    public required List<SetOrderDto> SetOrders { get; init; }
}