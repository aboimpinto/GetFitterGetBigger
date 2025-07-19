using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.Equipment;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Interfaces;

/// <summary>
/// Service interface for equipment operations
/// </summary>
public interface IEquipmentService
{
    /// <summary>
    /// Gets all active equipment
    /// </summary>
    Task<ServiceResult<IEnumerable<EquipmentDto>>> GetAllAsync();
    
    /// <summary>
    /// Gets equipment by ID
    /// </summary>
    Task<ServiceResult<EquipmentDto>> GetByIdAsync(EquipmentId id);
    
    /// <summary>
    /// Gets equipment by name (case-insensitive)
    /// </summary>
    Task<ServiceResult<EquipmentDto>> GetByNameAsync(string name);
    
    /// <summary>
    /// Creates new equipment
    /// </summary>
    Task<ServiceResult<EquipmentDto>> CreateAsync(CreateEquipmentCommand command);
    
    /// <summary>
    /// Updates existing equipment
    /// </summary>
    Task<ServiceResult<EquipmentDto>> UpdateAsync(EquipmentId id, UpdateEquipmentCommand command);
    
    /// <summary>
    /// Deletes equipment (soft delete)
    /// </summary>
    Task<ServiceResult<bool>> DeleteAsync(EquipmentId id);
    
    /// <summary>
    /// Checks if equipment exists with the given ID
    /// </summary>
    Task<ServiceResult<EquipmentDto>> ExistsAsync(EquipmentId id);
}