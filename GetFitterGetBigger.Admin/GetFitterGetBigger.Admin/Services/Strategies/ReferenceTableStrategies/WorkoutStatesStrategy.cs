using GetFitterGetBigger.Admin.Models.ReferenceData;

namespace GetFitterGetBigger.Admin.Services.Strategies.ReferenceTableStrategies
{
    public class WorkoutStatesStrategy : BaseReferenceTableStrategy<WorkoutStates>
    {
        public override string Endpoint => "/api/workout-states";
        public override string CacheKey => "RefData_WorkoutStates";
    }
}