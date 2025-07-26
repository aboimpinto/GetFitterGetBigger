using GetFitterGetBigger.Admin.Models.ReferenceData;

namespace GetFitterGetBigger.Admin.Services.Strategies.ReferenceTableStrategies
{
    public class MuscleRolesStrategy : BaseReferenceTableStrategy<MuscleRoles>
    {
        public override string Endpoint => "/api/ReferenceTables/MuscleRoles";
        public override string CacheKey => "RefData_MuscleRoles";
    }
}