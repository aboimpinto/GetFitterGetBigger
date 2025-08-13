using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.Equipment;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.ReferenceTables.Equipment;

/// <summary>
/// Service interface for Equipment business operations
/// Provides caching and business logic for equipment reference data
/// </summary>
public interface IEquipmentService
{
    /// <summary>
    /// Gets all active equipment with caching
    /// </summary>
    /// <returns>A service result containing the collection of active equipment</returns>
    Task<ServiceResult<IEnumerable<EquipmentDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets an equipment by its ID with caching
    /// </summary>
    /// <param name="id">The equipment ID</param>
    /// <returns>A service result containing the equipment if found</returns>
    Task<ServiceResult<EquipmentDto>> GetByIdAsync(EquipmentId id);
    
    /// <summary>
    /// Gets an equipment by its ID string with caching
    /// </summary>
    /// <param name="id">The equipment ID as a string</param>
    /// <returns>A service result containing the equipment if found</returns>
    Task<ServiceResult<EquipmentDto>> GetByIdAsync(string id);
    
    /// <summary>
    /// Gets an equipment by its value with caching
    /// </summary>
    /// <param name="value">The equipment value (case-insensitive)</param>
    /// <returns>A service result containing the equipment if found</returns>
    Task<ServiceResult<EquipmentDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Checks if an equipment exists by ID with caching
    /// </summary>
    /// <param name="id">The equipment ID to check</param>
    /// <returns>A service result indicating whether the equipment exists</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(EquipmentId id);
    
    /// <summary>
    /// Gets all equipment (same as GetAllActiveAsync for backward compatibility)
    /// </summary>
    /// <returns>A service result containing the collection of equipment</returns>
    Task<ServiceResult<IEnumerable<EquipmentDto>>> GetAllAsync();
    
    /// <summary>
    /// Gets an equipment by its name (same as GetByValueAsync for backward compatibility)
    /// </summary>
    /// <param name="name">The equipment name</param>
    /// <returns>A service result containing the equipment if found</returns>
    Task<ServiceResult<EquipmentDto>> GetByNameAsync(string name);
    
    /// <summary>
    /// Creates a new equipment
    /// </summary>
    /// <param name="command">The equipment data to create</param>
    /// <returns>A service result containing the created equipment</returns>
    Task<ServiceResult<EquipmentDto>> CreateAsync(CreateEquipmentCommand command);
    
    /// <summary>
    /// Updates an existing equipment
    /// </summary>
    /// <param name="id">The equipment ID to update</param>
    /// <param name="command">The updated equipment data</param>
    /// <returns>A service result containing the updated equipment</returns>
    Task<ServiceResult<EquipmentDto>> UpdateAsync(EquipmentId id, UpdateEquipmentCommand command);
    
    /// <summary>
    /// Soft deletes equipment (sets IsActive to false)
    /// </summary>
    /// <param name="id">The equipment ID to delete</param>
    /// <returns>A service result indicating success or failure</returns>
    Task<ServiceResult<BooleanResultDto>> DeleteAsync(EquipmentId id);
}