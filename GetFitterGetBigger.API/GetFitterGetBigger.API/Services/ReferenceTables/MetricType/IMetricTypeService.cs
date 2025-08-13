using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.ReferenceTables.MetricType;

/// <summary>
/// Service interface for MetricType business operations
/// Provides caching and business logic for metric type reference data
/// </summary>
public interface IMetricTypeService
{
    /// <summary>
    /// Gets all active metric types with caching
    /// </summary>
    /// <returns>A service result containing the collection of active metric types</returns>
    Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets a metric type by its ID with caching
    /// </summary>
    /// <param name="id">The metric type ID</param>
    /// <returns>A service result containing the metric type if found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(MetricTypeId id);
    
    /// <summary>
    /// Gets a metric type by its ID string with caching
    /// </summary>
    /// <param name="id">The metric type ID as a string</param>
    /// <returns>A service result containing the metric type if found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(string id);
    
    /// <summary>
    /// Gets a metric type by its value with caching
    /// </summary>
    /// <param name="value">The metric type value (case-insensitive)</param>
    /// <returns>A service result containing the metric type if found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Checks if a metric type exists by ID with caching
    /// </summary>
    /// <param name="id">The metric type ID to check</param>
    /// <returns>A service result indicating whether the metric type exists</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(MetricTypeId id);
}