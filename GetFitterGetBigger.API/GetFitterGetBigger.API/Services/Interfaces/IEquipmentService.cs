using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Services.ReferenceTable;

namespace GetFitterGetBigger.API.Services.Interfaces;

/// <summary>
/// Service interface for equipment operations
/// </summary>
public interface IEquipmentService : IReferenceTableService<Equipment>
{
    /// <summary>
    /// Gets equipment as DTOs
    /// </summary>
    Task<IEnumerable<ReferenceDataDto>> GetAllAsDtosAsync();
    
    /// <summary>
    /// Gets equipment by ID as DTO
    /// </summary>
    Task<ReferenceDataDto?> GetByIdAsDtoAsync(string id);
    
    /// <summary>
    /// Creates equipment and returns as DTO
    /// </summary>
    Task<EquipmentDto> CreateEquipmentAsync(CreateEquipmentDto request);
    
    /// <summary>
    /// Updates equipment and returns as DTO
    /// </summary>
    Task<EquipmentDto> UpdateEquipmentAsync(string id, UpdateEquipmentDto request);
    
    /// <summary>
    /// Deactivates equipment
    /// </summary>
    Task DeactivateAsync(string id);
}