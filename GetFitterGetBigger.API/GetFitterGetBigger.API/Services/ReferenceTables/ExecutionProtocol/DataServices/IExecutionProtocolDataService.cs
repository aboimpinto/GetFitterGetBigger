using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.ReferenceTables.ExecutionProtocol.DataServices;

/// <summary>
/// Data service interface for ExecutionProtocol database operations
/// Encapsulates all data access concerns including UnitOfWork and Repository interactions
/// ExecutionProtocols are pure reference data (read-only) that never changes after deployment
/// </summary>
public interface IExecutionProtocolDataService
{
    /// <summary>
    /// Gets all active execution protocols from the database
    /// </summary>
    /// <returns>A service result containing the collection of execution protocols</returns>
    Task<ServiceResult<IEnumerable<ExecutionProtocolDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets an execution protocol by its ID from the database
    /// </summary>
    /// <param name="id">The execution protocol ID</param>
    /// <returns>A service result containing the execution protocol if found, Empty otherwise</returns>
    Task<ServiceResult<ExecutionProtocolDto>> GetByIdAsync(ExecutionProtocolId id);
    
    /// <summary>
    /// Gets an execution protocol by its value from the database
    /// </summary>
    /// <param name="value">The execution protocol value</param>
    /// <returns>A service result containing the execution protocol if found, Empty otherwise</returns>
    Task<ServiceResult<ExecutionProtocolDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Gets an execution protocol by its code from the database
    /// </summary>
    /// <param name="code">The execution protocol code</param>
    /// <returns>A service result containing the execution protocol if found, Empty otherwise</returns>
    Task<ServiceResult<ExecutionProtocolDto>> GetByCodeAsync(string code);
    
    /// <summary>
    /// Checks if an execution protocol exists in the database
    /// </summary>
    /// <param name="id">The execution protocol ID to check</param>
    /// <returns>A service result containing true if exists, false otherwise</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(ExecutionProtocolId id);
}