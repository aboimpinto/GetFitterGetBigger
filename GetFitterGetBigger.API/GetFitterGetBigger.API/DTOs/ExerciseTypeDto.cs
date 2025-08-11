using GetFitterGetBigger.API.DTOs.Interfaces;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for exercise type reference data
/// Uses ReferenceDataDto as the base representation
/// </summary>
public record ExerciseTypeDto : ReferenceDataDto, IEmptyDto<ExerciseTypeDto>
{
    // ExerciseTypeDto inherits all properties from ReferenceDataDto
    // No additional properties needed for exercise types
    
    /// <summary>
    /// Gets an empty ExerciseTypeDto instance for the Empty Object Pattern
    /// </summary>
    public new static ExerciseTypeDto Empty => new()
    {
        Id = string.Empty,
        Value = string.Empty,
        Description = null
    };
}