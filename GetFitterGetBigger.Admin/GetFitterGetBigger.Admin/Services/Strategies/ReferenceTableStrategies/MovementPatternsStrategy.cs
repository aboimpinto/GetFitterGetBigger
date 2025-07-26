using GetFitterGetBigger.Admin.Models.ReferenceData;

namespace GetFitterGetBigger.Admin.Services.Strategies.ReferenceTableStrategies
{
    public class MovementPatternsStrategy : BaseReferenceTableStrategy<MovementPatterns>
    {
        public override string Endpoint => "/api/ReferenceTables/MovementPatterns";
        public override string CacheKey => "RefData_MovementPatterns";
    }
}