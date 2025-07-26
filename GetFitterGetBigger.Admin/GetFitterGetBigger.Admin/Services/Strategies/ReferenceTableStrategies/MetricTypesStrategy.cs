using GetFitterGetBigger.Admin.Models.ReferenceData;

namespace GetFitterGetBigger.Admin.Services.Strategies.ReferenceTableStrategies
{
    public class MetricTypesStrategy : BaseReferenceTableStrategy<MetricTypes>
    {
        public override string Endpoint => "/api/ReferenceTables/MetricTypes";
        public override string CacheKey => "RefData_MetricTypes";
    }
}