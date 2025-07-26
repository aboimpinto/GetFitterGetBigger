using GetFitterGetBigger.Admin.Models.ReferenceData;

namespace GetFitterGetBigger.Admin.Services.Strategies.ReferenceTableStrategies
{
    public class DifficultyLevelsStrategy : BaseReferenceTableStrategy<DifficultyLevels>
    {
        public override string Endpoint => "/api/ReferenceTables/DifficultyLevels";
        public override string CacheKey => "RefData_DifficultyLevels";
    }
}