using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for MetricType data
/// </summary>
public interface IMetricTypeRepository : IRepository
{
    /// <summary>
    /// Gets all metric types
    /// </summary>
    /// <returns>A collection of metric types</returns>
    Task<IEnumerable<MetricType>> GetAllAsync();
    
    /// <summary>
    /// Gets a metric type by its ID
    /// </summary>
    /// <param name="id">The ID of the metric type to retrieve</param>
    /// <returns>The metric type if found, null otherwise</returns>
    Task<MetricType?> GetByIdAsync(MetricTypeId id);
    
    /// <summary>
    /// Gets a metric type by its name
    /// </summary>
    /// <param name="name">The name of the metric type to retrieve</param>
    /// <returns>The metric type if found, null otherwise</returns>
    Task<MetricType?> GetByNameAsync(string name);
}
