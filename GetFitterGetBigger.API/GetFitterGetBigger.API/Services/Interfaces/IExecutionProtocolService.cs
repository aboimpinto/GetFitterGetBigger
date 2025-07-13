using System.Collections.Generic;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Services.Interfaces;

/// <summary>
/// Service for managing execution protocols reference data
/// </summary>
public interface IExecutionProtocolService
{
    /// <summary>
    /// Get all active execution protocols
    /// </summary>
    /// <returns>Collection of active execution protocols</returns>
    Task<IEnumerable<ExecutionProtocol>> GetAllAsync();
    
    /// <summary>
    /// Get all active execution protocols as DTOs
    /// </summary>
    /// <returns>Collection of execution protocol DTOs</returns>
    Task<IEnumerable<ExecutionProtocolDto>> GetAllAsDtosAsync();
    
    /// <summary>
    /// Get execution protocol by ID
    /// </summary>
    /// <param name="id">The execution protocol ID</param>
    /// <returns>The execution protocol or null if not found</returns>
    Task<ExecutionProtocol?> GetByIdAsync(ExecutionProtocolId id);
    
    /// <summary>
    /// Get execution protocol by ID as DTO
    /// </summary>
    /// <param name="id">The execution protocol ID as string</param>
    /// <returns>The execution protocol DTO or null if not found</returns>
    Task<ExecutionProtocolDto?> GetByIdAsDtoAsync(string id);
    
    /// <summary>
    /// Get execution protocol by value
    /// </summary>
    /// <param name="value">The execution protocol value (case insensitive)</param>
    /// <returns>The execution protocol or null if not found</returns>
    Task<ExecutionProtocol?> GetByValueAsync(string value);
    
    /// <summary>
    /// Get execution protocol by code
    /// </summary>
    /// <param name="code">The execution protocol code (case insensitive)</param>
    /// <returns>The execution protocol or null if not found</returns>
    Task<ExecutionProtocol?> GetByCodeAsync(string code);
    
    /// <summary>
    /// Get execution protocol by code as DTO
    /// </summary>
    /// <param name="code">The execution protocol code (case insensitive)</param>
    /// <returns>The execution protocol DTO or null if not found</returns>
    Task<ExecutionProtocolDto?> GetByCodeAsDtoAsync(string code);
    
    /// <summary>
    /// Check if execution protocol exists
    /// </summary>
    /// <param name="id">The execution protocol ID</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> ExistsAsync(ExecutionProtocolId id);
}