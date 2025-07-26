using System;

namespace GetFitterGetBigger.Admin.Services.TableComponentStrategies
{
    /// <summary>
    /// Strategy for the Workout Objectives reference table component.
    /// </summary>
    public class WorkoutObjectivesTableStrategy : TableComponentStrategyBase
    {
        public override string TableName => "WorkoutObjectives";
        
        public override string DisplayName => "Workout Objectives";
        
        public override Type ComponentType => typeof(Components.Pages.ReferenceTableComponents.WorkoutObjectivesTable);
    }
}