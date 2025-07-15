using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GetFitterGetBigger.API.Repositories.Implementations;

/// <summary>
/// Repository implementation for ExecutionProtocol reference data
/// </summary>
public class ExecutionProtocolRepository : 
    EmptyEnabledReferenceDataRepository<ExecutionProtocol, ExecutionProtocolId, FitnessDbContext>, 
    IExecutionProtocolRepository
{
    /// <summary>
    /// Gets an execution protocol by its code
    /// </summary>
    /// <param name="code">The code of the execution protocol (e.g., "STANDARD", "SUPERSET")</param>
    /// <returns>The execution protocol if found, Empty otherwise</returns>
    public async Task<ExecutionProtocol> GetByCodeAsync(string code) =>
        await Context.Set<ExecutionProtocol>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code.ToLower() == code.ToLower() && x.IsActive) ?? ExecutionProtocol.Empty;
}