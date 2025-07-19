using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.MuscleGroup;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Interfaces;

/// <summary>
/// Service interface for muscle group operations
/// </summary>
public interface IMuscleGroupService
{
    /// <summary>
    /// Gets all active muscle groups
    /// </summary>
    Task<ServiceResult<IEnumerable<MuscleGroupDto>>> GetAllAsync();
    
    /// <summary>
    /// Gets muscle group by ID
    /// </summary>
    Task<ServiceResult<MuscleGroupDto>> GetByIdAsync(MuscleGroupId id);
    
    /// <summary>
    /// Gets muscle group by name (case-insensitive)
    /// </summary>
    Task<ServiceResult<MuscleGroupDto>> GetByNameAsync(string name);
    
    /// <summary>
    /// Gets muscle groups by body part
    /// </summary>
    Task<ServiceResult<IEnumerable<MuscleGroupDto>>> GetByBodyPartAsync(BodyPartId bodyPartId);
    
    /// <summary>
    /// Creates new muscle group
    /// </summary>
    Task<ServiceResult<MuscleGroupDto>> CreateAsync(CreateMuscleGroupCommand command);
    
    /// <summary>
    /// Updates existing muscle group
    /// </summary>
    Task<ServiceResult<MuscleGroupDto>> UpdateAsync(MuscleGroupId id, UpdateMuscleGroupCommand command);
    
    /// <summary>
    /// Deletes muscle group (soft delete)
    /// </summary>
    Task<ServiceResult<bool>> DeleteAsync(MuscleGroupId id);
    
    /// <summary>
    /// Checks if muscle group exists with the given ID
    /// </summary>
    Task<ServiceResult<MuscleGroupDto>> ExistsAsync(MuscleGroupId id);
}