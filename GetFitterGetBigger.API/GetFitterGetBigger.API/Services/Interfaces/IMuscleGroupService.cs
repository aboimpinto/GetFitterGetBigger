using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Services.ReferenceTable;

namespace GetFitterGetBigger.API.Services.Interfaces;

/// <summary>
/// Service interface for muscle group operations
/// </summary>
public interface IMuscleGroupService : IReferenceTableService<MuscleGroup>
{
    /// <summary>
    /// Gets all muscle groups as DTOs
    /// </summary>
    Task<IEnumerable<MuscleGroupDto>> GetAllAsDtosAsync();
    
    /// <summary>
    /// Gets muscle group by ID as DTO
    /// </summary>
    Task<MuscleGroupDto?> GetByIdAsDtoAsync(string id);
    
    /// <summary>
    /// Gets muscle groups by body part
    /// </summary>
    Task<IEnumerable<MuscleGroupDto>> GetByBodyPartAsync(string bodyPartId);
    
    /// <summary>
    /// Creates a new muscle group
    /// </summary>
    Task<MuscleGroupDto> CreateMuscleGroupAsync(CreateMuscleGroupDto request);
    
    /// <summary>
    /// Updates an existing muscle group
    /// </summary>
    Task<MuscleGroupDto> UpdateMuscleGroupAsync(string id, UpdateMuscleGroupDto request);
    
    /// <summary>
    /// Deactivates a muscle group
    /// </summary>
    Task DeactivateMuscleGroupAsync(string id);
}