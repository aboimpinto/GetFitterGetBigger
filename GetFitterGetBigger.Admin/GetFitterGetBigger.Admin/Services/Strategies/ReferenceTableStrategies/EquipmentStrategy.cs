using GetFitterGetBigger.Admin.Models.ReferenceData;

namespace GetFitterGetBigger.Admin.Services.Strategies.ReferenceTableStrategies
{
    public class EquipmentStrategy : BaseReferenceTableStrategy<Equipment>
    {
        public override string Endpoint => "/api/ReferenceTables/Equipment";
        public override string CacheKey => "RefData_Equipment";
    }
}