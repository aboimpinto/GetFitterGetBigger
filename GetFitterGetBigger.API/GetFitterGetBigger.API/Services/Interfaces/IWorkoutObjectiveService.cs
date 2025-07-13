using System.Collections.Generic;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Services.Interfaces;

/// <summary>
/// Service for managing workout objectives reference data
/// </summary>
public interface IWorkoutObjectiveService
{
    /// <summary>
    /// Get all active workout objectives
    /// </summary>
    /// <returns>Collection of active workout objectives</returns>
    Task<IEnumerable<WorkoutObjective>> GetAllAsync();
    
    /// <summary>
    /// Get all active workout objectives as DTOs
    /// </summary>
    /// <returns>Collection of workout objective DTOs</returns>
    Task<IEnumerable<ReferenceDataDto>> GetAllAsDtosAsync();
    
    /// <summary>
    /// Get all workout objectives as WorkoutObjectiveDto (with option to include inactive)
    /// </summary>
    /// <param name="includeInactive">Whether to include inactive objectives</param>
    /// <returns>Collection of workout objective DTOs</returns>
    Task<IEnumerable<WorkoutObjectiveDto>> GetAllAsWorkoutObjectiveDtosAsync(bool includeInactive = false);
    
    /// <summary>
    /// Get workout objective by ID
    /// </summary>
    /// <param name="id">The workout objective ID</param>
    /// <returns>The workout objective or null if not found</returns>
    Task<WorkoutObjective?> GetByIdAsync(WorkoutObjectiveId id);
    
    /// <summary>
    /// Get workout objective by ID as DTO
    /// </summary>
    /// <param name="id">The workout objective ID as string</param>
    /// <returns>The workout objective DTO or null if not found</returns>
    Task<ReferenceDataDto?> GetByIdAsDtoAsync(string id);
    
    /// <summary>
    /// Get workout objective by ID as WorkoutObjectiveDto (with option to include inactive)
    /// </summary>
    /// <param name="id">The workout objective ID as string</param>
    /// <param name="includeInactive">Whether to include inactive objectives</param>
    /// <returns>The workout objective DTO or null if not found</returns>
    Task<WorkoutObjectiveDto?> GetByIdAsWorkoutObjectiveDtoAsync(string id, bool includeInactive = false);
    
    /// <summary>
    /// Get workout objective by value
    /// </summary>
    /// <param name="value">The workout objective value (case insensitive)</param>
    /// <returns>The workout objective or null if not found</returns>
    Task<WorkoutObjective?> GetByValueAsync(string value);
    
    /// <summary>
    /// Check if workout objective exists
    /// </summary>
    /// <param name="id">The workout objective ID</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> ExistsAsync(WorkoutObjectiveId id);
}