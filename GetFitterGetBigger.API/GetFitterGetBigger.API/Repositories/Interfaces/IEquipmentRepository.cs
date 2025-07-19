using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for Equipment data
/// </summary>
public interface IEquipmentRepository : IRepository
{
    /// <summary>
    /// Gets all equipment
    /// </summary>
    /// <returns>A collection of equipment</returns>
    Task<IEnumerable<Equipment>> GetAllAsync();
    
    /// <summary>
    /// Gets equipment by its ID
    /// </summary>
    /// <param name="id">The ID of the equipment to retrieve</param>
    /// <returns>The equipment if found, Equipment.Empty otherwise</returns>
    Task<Equipment> GetByIdAsync(EquipmentId id);
    
    /// <summary>
    /// Gets equipment by its name
    /// </summary>
    /// <param name="name">The name of the equipment to retrieve</param>
    /// <returns>The equipment if found, Equipment.Empty otherwise</returns>
    Task<Equipment> GetByNameAsync(string name);
    
    /// <summary>
    /// Creates new equipment
    /// </summary>
    /// <param name="entity">The equipment to create</param>
    /// <returns>The created equipment</returns>
    Task<Equipment> CreateAsync(Equipment entity);
    
    /// <summary>
    /// Updates existing equipment
    /// </summary>
    /// <param name="entity">The equipment to update</param>
    /// <returns>The updated equipment</returns>
    Task<Equipment> UpdateAsync(Equipment entity);
    
    /// <summary>
    /// Deactivates equipment by its ID
    /// </summary>
    /// <param name="id">The ID of the equipment to deactivate</param>
    /// <returns>True if the equipment was deactivated, false if not found</returns>
    Task<bool> DeactivateAsync(EquipmentId id);
    
    /// <summary>
    /// Checks if equipment with the given name exists
    /// </summary>
    /// <param name="name">The name to check</param>
    /// <param name="excludeId">Optional ID to exclude from the check (for updates)</param>
    /// <returns>True if equipment with the name exists, false otherwise</returns>
    Task<bool> ExistsAsync(string name, EquipmentId? excludeId = null);
    
    /// <summary>
    /// Checks if equipment is in use by any exercises
    /// </summary>
    /// <param name="id">The ID of the equipment to check</param>
    /// <returns>True if the equipment is in use, false otherwise</returns>
    Task<bool> IsInUseAsync(EquipmentId id);
}
