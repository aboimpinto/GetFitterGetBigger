using System;

namespace GetFitterGetBigger.Admin.Services.TableComponentStrategies
{
    /// <summary>
    /// Strategy for the Exercise Weight Types reference table component.
    /// </summary>
    public class ExerciseWeightTypesTableStrategy : TableComponentStrategyBase
    {
        public override string TableName => "ExerciseWeightTypes";
        
        public override string DisplayName => "Exercise Weight Types";
        
        public override Type ComponentType => typeof(Components.Pages.ReferenceTableComponents.ExerciseWeightTypesTable);
    }
}