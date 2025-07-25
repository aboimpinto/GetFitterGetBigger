namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for creating multiple set configurations in bulk
/// </summary>
public class CreateBulkSetConfigurationsDto
{
    /// <summary>
    /// List of set configurations to create
    /// </summary>
    [System.ComponentModel.DataAnnotations.Required]
    public required List<CreateSetConfigurationDto> Sets { get; init; }
}