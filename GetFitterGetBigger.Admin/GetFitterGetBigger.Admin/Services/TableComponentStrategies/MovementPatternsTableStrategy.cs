using System;

namespace GetFitterGetBigger.Admin.Services.TableComponentStrategies
{
    /// <summary>
    /// Strategy for the Movement Patterns reference table component.
    /// </summary>
    public class MovementPatternsTableStrategy : TableComponentStrategyBase
    {
        public override string TableName => "MovementPatterns";
        
        public override string DisplayName => "Movement Patterns";
        
        public override Type ComponentType => typeof(Components.Pages.ReferenceTableComponents.GenericReferenceTable);
    }
}