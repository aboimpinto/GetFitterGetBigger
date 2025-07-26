using System;

namespace GetFitterGetBigger.Admin.Services.TableComponentStrategies
{
    /// <summary>
    /// Strategy for the Muscle Groups reference table component.
    /// </summary>
    public class MuscleGroupsTableStrategy : TableComponentStrategyBase
    {
        public override string TableName => "MuscleGroups";
        
        public override string DisplayName => "Muscle Groups";
        
        public override Type ComponentType => typeof(Components.Pages.MuscleGroups.MuscleGroupsTable);
    }
}