using System.ComponentModel.DataAnnotations;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for creating new equipment
/// </summary>
public class CreateEquipmentDto
{
    /// <summary>
    /// The name of the equipment
    /// </summary>
    [Required(ErrorMessage = "Equipment name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Equipment name must be between 1 and 100 characters")]
    public string Name { get; set; } = string.Empty;
}