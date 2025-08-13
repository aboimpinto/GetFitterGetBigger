using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Models.DTOs;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Equipment;

/// <summary>
/// Service for analyzing and managing equipment requirements for workout templates
/// </summary>
public interface IEquipmentRequirementsService
{
    /// <summary>
    /// Gets all equipment required for a workout template based on its exercises
    /// </summary>
    /// <param name="workoutTemplateId">The workout template ID</param>
    /// <returns>Collection of equipment DTOs required for the workout</returns>
    Task<ServiceResult<IEnumerable<EquipmentDto>>> GetRequiredEquipmentAsync(WorkoutTemplateId workoutTemplateId);
    
    /// <summary>
    /// Analyzes equipment requirements across multiple workout templates
    /// </summary>
    /// <param name="workoutTemplateIds">Collection of workout template IDs</param>
    /// <returns>Aggregated equipment requirements with frequency of use</returns>
    Task<ServiceResult<IEnumerable<EquipmentUsageDto>>> AnalyzeEquipmentUsageAsync(IEnumerable<WorkoutTemplateId> workoutTemplateIds);
    
    /// <summary>
    /// Checks if a workout template can be performed with available equipment
    /// </summary>
    /// <param name="workoutTemplateId">The workout template ID</param>
    /// <param name="availableEquipmentIds">Available equipment IDs</param>
    /// <returns>True if workout can be performed, false otherwise with missing equipment details</returns>
    Task<ServiceResult<EquipmentAvailabilityDto>> CheckEquipmentAvailabilityAsync(
        WorkoutTemplateId workoutTemplateId, 
        IEnumerable<EquipmentId> availableEquipmentIds);
}