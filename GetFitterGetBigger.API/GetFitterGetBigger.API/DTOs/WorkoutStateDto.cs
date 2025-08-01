using GetFitterGetBigger.API.DTOs.Interfaces;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for workout state reference data
/// Uses ReferenceDataDto as the base representation
/// </summary>
public record WorkoutStateDto : ReferenceDataDto, IEmptyDto<WorkoutStateDto>
{
    // WorkoutStateDto inherits all properties from ReferenceDataDto
    // No additional properties needed for workout states
    
    /// <summary>
    /// Gets an empty WorkoutStateDto instance for the Empty Object Pattern
    /// </summary>
    public new static WorkoutStateDto Empty => new()
    {
        Id = string.Empty,
        Value = string.Empty,
        Description = null
    };
}