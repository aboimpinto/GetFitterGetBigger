using GetFitterGetBigger.API.DTOs.Interfaces;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for body part reference data
/// Uses ReferenceDataDto as the base representation
/// </summary>
public record BodyPartDto : ReferenceDataDto, IEmptyDto<BodyPartDto>
{
    // BodyPartDto inherits all properties from ReferenceDataDto
    // No additional properties needed for body parts
    
    /// <summary>
    /// Gets an empty BodyPartDto instance for the Empty Object Pattern
    /// </summary>
    public new static BodyPartDto Empty => new()
    {
        Id = string.Empty,
        Value = string.Empty,
        Description = null
    };
}