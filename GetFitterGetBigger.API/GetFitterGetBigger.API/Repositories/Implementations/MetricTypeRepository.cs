using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;

namespace GetFitterGetBigger.API.Repositories.Implementations;

/// <summary>
/// Repository implementation for MetricType data with Empty pattern support
/// </summary>
public class MetricTypeRepository : ReferenceDataRepository<MetricType, MetricTypeId, FitnessDbContext>, IMetricTypeRepository
{
}
