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
    /// <returns>The equipment if found, null otherwise</returns>
    Task<Equipment?> GetByIdAsync(EquipmentId id);
    
    /// <summary>
    /// Gets equipment by its name
    /// </summary>
    /// <param name="name">The name of the equipment to retrieve</param>
    /// <returns>The equipment if found, null otherwise</returns>
    Task<Equipment?> GetByNameAsync(string name);
}
