using GetFitterGetBigger.Admin.Models.ReferenceData;

namespace GetFitterGetBigger.Admin.Services.Strategies.ReferenceTableStrategies
{
    public class KineticChainTypesStrategy : BaseReferenceTableStrategy<KineticChainTypes>
    {
        public override string Endpoint => "/api/ReferenceTables/KineticChainTypes";
        public override string CacheKey => "RefData_KineticChainTypes";
    }
}