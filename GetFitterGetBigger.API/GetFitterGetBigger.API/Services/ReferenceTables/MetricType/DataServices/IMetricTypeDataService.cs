using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.ReferenceTables.MetricType.DataServices;

/// <summary>
/// Data service interface for MetricType database operations
/// Handles all data access concerns for MetricType entities
/// </summary>
public interface IMetricTypeDataService
{
    /// <summary>
    /// Gets all active metric types from the database
    /// </summary>
    /// <returns>Collection of active metric type DTOs</returns>
    Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets a metric type by its ID
    /// </summary>
    /// <param name="id">The metric type ID</param>
    /// <returns>Reference data DTO or Empty if not found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(MetricTypeId id);
    
    /// <summary>
    /// Gets a metric type by its value
    /// </summary>
    /// <param name="value">The metric type value (case-insensitive)</param>
    /// <returns>Reference data DTO or Empty if not found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Checks if a metric type exists by ID
    /// </summary>
    /// <param name="id">The metric type ID</param>
    /// <returns>Boolean result indicating existence</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(MetricTypeId id);
}