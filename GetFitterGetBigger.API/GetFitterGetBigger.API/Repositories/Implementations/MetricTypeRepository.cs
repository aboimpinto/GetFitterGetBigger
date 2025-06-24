using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Repositories.Implementations;

/// <summary>
/// Repository implementation for MetricType data
/// </summary>
public class MetricTypeRepository : RepositoryBase<FitnessDbContext>, IMetricTypeRepository
{
    /// <summary>
    /// Gets all metric types
    /// </summary>
    /// <returns>A collection of metric types</returns>
    public async Task<IEnumerable<MetricType>> GetAllAsync() =>
        await Context.MetricTypes
            .AsNoTracking()
            .OrderBy(mt => mt.Name)
            .ToListAsync();
    
    /// <summary>
    /// Gets a metric type by its ID
    /// </summary>
    /// <param name="id">The ID of the metric type to retrieve</param>
    /// <returns>The metric type if found, null otherwise</returns>
    public async Task<MetricType?> GetByIdAsync(MetricTypeId id)
    {
        var metricType = await Context.MetricTypes.FindAsync(id);
        
        if (metricType != null)
        {
            // Detach the entity from the context to achieve the same effect as AsNoTracking
            Context.Entry(metricType).State = EntityState.Detached;
        }
        
        return metricType;
    }
    
    /// <summary>
    /// Gets a metric type by its name (case-insensitive)
    /// </summary>
    /// <param name="name">The name of the metric type to retrieve</param>
    /// <returns>The metric type if found, null otherwise</returns>
    public async Task<MetricType?> GetByNameAsync(string name) =>
        await Context.MetricTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(mt => mt.Name.ToLower() == name.ToLower());
}
