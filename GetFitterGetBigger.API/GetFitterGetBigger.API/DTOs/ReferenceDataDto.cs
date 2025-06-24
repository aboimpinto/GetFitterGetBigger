using System;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for reference data entities
/// </summary>
public class ReferenceDataDto
{
    /// <summary>
    /// The ID of the reference data entity in the format "{referencetable}-{guid}"
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// The value of the reference data entity
    /// </summary>
    public string Value { get; set; } = string.Empty;
    
    /// <summary>
    /// The description of the reference data entity
    /// </summary>
    public string? Description { get; set; }
}
