using GetFitterGetBigger.Admin.Models.ReferenceData;

namespace GetFitterGetBigger.Admin.Services.Strategies.ReferenceTableStrategies
{
    public class BodyPartsStrategy : BaseReferenceTableStrategy<BodyParts>
    {
        public override string Endpoint => "/api/ReferenceTables/BodyParts";
        public override string CacheKey => "RefData_BodyParts";
    }
}