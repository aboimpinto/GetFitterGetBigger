using System;

namespace GetFitterGetBigger.Admin.Services.TableComponentStrategies
{
    /// <summary>
    /// Strategy for the Workout Categories reference table component.
    /// </summary>
    public class WorkoutCategoriesTableStrategy : TableComponentStrategyBase
    {
        public override string TableName => "WorkoutCategories";
        
        public override string DisplayName => "Workout Categories";
        
        public override Type ComponentType => typeof(Components.Pages.ReferenceTableComponents.WorkoutCategoriesTable);
    }
}