using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for MetricType data with Empty pattern support
/// </summary>
public interface IMetricTypeRepository : IEmptyEnabledReferenceDataRepository<MetricType, MetricTypeId>
{
}
