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
    /// Gets muscle groups by body part
    /// </summary>
    Task<IEnumerable<MuscleGroupDto>> GetByBodyPartAsync(string bodyPartId);
    
    /// <summary>
    /// Adds a body part to a muscle group
    /// </summary>
    Task<MuscleGroupDto> AddBodyPartAsync(string id, AddBodyPartRequest request);
    
    /// <summary>
    /// Removes a body part from a muscle group
    /// </summary>
    Task RemoveBodyPartAsync(string id, string bodyPartId);
}