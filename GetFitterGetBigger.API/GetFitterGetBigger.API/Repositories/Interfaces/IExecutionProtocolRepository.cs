using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for ExecutionProtocol reference data
/// </summary>
public interface IExecutionProtocolRepository : IEmptyEnabledReferenceDataRepository<ExecutionProtocol, ExecutionProtocolId>
{
    /// <summary>
    /// Gets an execution protocol by its code
    /// </summary>
    /// <param name="code">The code of the execution protocol (e.g., "STANDARD", "SUPERSET")</param>
    /// <returns>The execution protocol if found, Empty otherwise</returns>
    Task<ExecutionProtocol> GetByCodeAsync(string code);
}