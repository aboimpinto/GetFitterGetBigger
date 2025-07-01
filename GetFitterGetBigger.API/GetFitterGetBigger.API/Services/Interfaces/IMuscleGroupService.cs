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
}