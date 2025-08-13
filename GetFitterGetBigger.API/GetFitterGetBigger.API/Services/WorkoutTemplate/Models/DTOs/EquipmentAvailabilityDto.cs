using GetFitterGetBigger.API.DTOs;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Models.DTOs;

/// <summary>
/// Represents equipment availability check result for a workout template
/// </summary>
public class EquipmentAvailabilityDto
{
    /// <summary>
    /// Indicates if all required equipment is available
    /// </summary>
    public bool CanPerformWorkout { get; set; }
    
    /// <summary>
    /// List of required equipment
    /// </summary>
    public IEnumerable<EquipmentDto> RequiredEquipment { get; set; } = [];
    
    /// <summary>
    /// List of available equipment from the required set
    /// </summary>
    public IEnumerable<EquipmentDto> AvailableEquipment { get; set; } = [];
    
    /// <summary>
    /// List of missing equipment preventing workout execution
    /// </summary>
    public IEnumerable<EquipmentDto> MissingEquipment { get; set; } = [];
    
    /// <summary>
    /// Percentage of required equipment that is available
    /// </summary>
    public decimal AvailabilityPercentage { get; set; }
    
    /// <summary>
    /// Suggested alternative exercises that can be done with available equipment
    /// </summary>
    public IEnumerable<ExerciseDto> AlternativeExercises { get; set; } = [];
}