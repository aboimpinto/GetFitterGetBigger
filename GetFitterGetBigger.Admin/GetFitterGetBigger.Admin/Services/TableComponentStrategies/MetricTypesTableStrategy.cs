using System;

namespace GetFitterGetBigger.Admin.Services.TableComponentStrategies
{
    /// <summary>
    /// Strategy for the Metric Types reference table component.
    /// </summary>
    public class MetricTypesTableStrategy : TableComponentStrategyBase
    {
        public override string TableName => "MetricTypes";
        
        public override string DisplayName => "Metric Types";
        
        public override Type ComponentType => typeof(Components.Pages.ReferenceTableComponents.GenericReferenceTable);
    }
}