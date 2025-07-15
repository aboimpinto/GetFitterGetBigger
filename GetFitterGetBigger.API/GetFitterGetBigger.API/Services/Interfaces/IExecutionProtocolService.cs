using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Interfaces;

/// <summary>
/// Service interface for managing execution protocol reference data
/// </summary>
public interface IExecutionProtocolService
{
    /// <summary>
    /// Gets all active execution protocols
    /// </summary>
    /// <returns>A service result containing all active execution protocols</returns>
    Task<ServiceResult<IEnumerable<ExecutionProtocolDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets an execution protocol by its ID
    /// </summary>
    /// <param name="id">The execution protocol ID</param>
    /// <returns>A service result containing the execution protocol if found</returns>
    Task<ServiceResult<ExecutionProtocolDto>> GetByIdAsync(ExecutionProtocolId id);
    
    /// <summary>
    /// Gets an execution protocol by its value
    /// </summary>
    /// <param name="value">The execution protocol value</param>
    /// <returns>A service result containing the execution protocol if found</returns>
    Task<ServiceResult<ExecutionProtocolDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Gets an execution protocol by its code
    /// </summary>
    /// <param name="code">The execution protocol code</param>
    /// <returns>A service result containing the execution protocol if found</returns>
    Task<ServiceResult<ExecutionProtocolDto>> GetByCodeAsync(string code);
    
    /// <summary>
    /// Checks if an execution protocol exists
    /// </summary>
    /// <param name="id">The execution protocol ID to check</param>
    /// <returns>True if the execution protocol exists, false otherwise</returns>
    Task<bool> ExistsAsync(ExecutionProtocolId id);
    
    /// <summary>
    /// Checks if an execution protocol exists with the given string ID
    /// </summary>
    /// <param name="id">The execution protocol ID in string format</param>
    /// <returns>True if the execution protocol exists and is active, false otherwise</returns>
    Task<bool> ExistsAsync(string id);
}