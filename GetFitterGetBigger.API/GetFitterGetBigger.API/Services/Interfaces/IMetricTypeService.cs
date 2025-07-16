using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Interfaces;

/// <summary>
/// Service interface for metric type operations
/// </summary>
public interface IMetricTypeService
{
    /// <summary>
    /// Gets all active metric types
    /// </summary>
    /// <returns>A service result containing the collection of active metric types</returns>
    Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets a metric type by its ID
    /// </summary>
    /// <param name="id">The ID of the metric type to retrieve</param>
    /// <returns>A service result containing the metric type if found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(MetricTypeId id);
    
    /// <summary>
    /// Gets a metric type by its value
    /// </summary>
    /// <param name="value">The value of the metric type to retrieve</param>
    /// <returns>A service result containing the metric type if found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Checks if a metric type exists by ID
    /// </summary>
    /// <param name="id">The ID to check</param>
    /// <returns>True if the metric type exists, false otherwise</returns>
    Task<bool> ExistsAsync(MetricTypeId id);
}