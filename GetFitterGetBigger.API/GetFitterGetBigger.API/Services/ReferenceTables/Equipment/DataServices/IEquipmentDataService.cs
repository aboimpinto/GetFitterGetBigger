using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.Equipment;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.ReferenceTables.Equipment.DataServices;

/// <summary>
/// Data service interface for Equipment database operations
/// Handles all data access concerns for Equipment entities including CRUD operations
/// </summary>
public interface IEquipmentDataService
{
    /// <summary>
    /// Gets all active equipment from the database
    /// </summary>
    /// <returns>Collection of active equipment DTOs</returns>
    Task<ServiceResult<IEnumerable<EquipmentDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets equipment by its ID
    /// </summary>
    /// <param name="id">The equipment ID</param>
    /// <returns>Equipment DTO or Empty if not found</returns>
    Task<ServiceResult<EquipmentDto>> GetByIdAsync(EquipmentId id);
    
    /// <summary>
    /// Gets equipment by its name
    /// </summary>
    /// <param name="name">The equipment name (case-insensitive)</param>
    /// <returns>Equipment DTO or Empty if not found</returns>
    Task<ServiceResult<EquipmentDto>> GetByNameAsync(string name);
    
    /// <summary>
    /// Creates a new equipment
    /// </summary>
    /// <param name="command">The equipment data to create</param>
    /// <returns>Created equipment DTO</returns>
    Task<ServiceResult<EquipmentDto>> CreateAsync(CreateEquipmentCommand command);
    
    /// <summary>
    /// Updates an existing equipment
    /// </summary>
    /// <param name="id">The equipment ID to update</param>
    /// <param name="command">The updated equipment data</param>
    /// <returns>Updated equipment DTO</returns>
    Task<ServiceResult<EquipmentDto>> UpdateAsync(EquipmentId id, UpdateEquipmentCommand command);
    
    /// <summary>
    /// Soft deletes equipment (sets IsActive to false)
    /// </summary>
    /// <param name="id">The equipment ID to delete</param>
    /// <returns>Success or failure result</returns>
    Task<ServiceResult<BooleanResultDto>> DeleteAsync(EquipmentId id);
    
    /// <summary>
    /// Checks if equipment exists by ID
    /// </summary>
    /// <param name="id">The equipment ID</param>
    /// <returns>Boolean result indicating existence</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(EquipmentId id);
    
    /// <summary>
    /// Checks if equipment name is unique (excluding a specific ID if provided)
    /// </summary>
    /// <param name="name">The equipment name to check</param>
    /// <param name="excludeId">Optional ID to exclude from the check (for updates)</param>
    /// <returns>True if unique, false otherwise</returns>
    Task<bool> IsNameUniqueAsync(string name, EquipmentId? excludeId = null);
    
    /// <summary>
    /// Checks if equipment can be deleted (not referenced by other entities)
    /// </summary>
    /// <param name="id">The equipment ID</param>
    /// <returns>True if can be deleted, false otherwise</returns>
    Task<bool> CanDeleteAsync(EquipmentId id);
}