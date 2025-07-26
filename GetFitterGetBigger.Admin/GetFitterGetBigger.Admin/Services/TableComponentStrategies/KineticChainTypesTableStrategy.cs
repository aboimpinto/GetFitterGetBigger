using System;

namespace GetFitterGetBigger.Admin.Services.TableComponentStrategies
{
    /// <summary>
    /// Strategy for the Kinetic Chain Types reference table component.
    /// </summary>
    public class KineticChainTypesTableStrategy : TableComponentStrategyBase
    {
        public override string TableName => "KineticChainTypes";
        
        public override string DisplayName => "Kinetic Chain Types";
        
        public override Type ComponentType => typeof(Components.Pages.ReferenceTableComponents.GenericReferenceTable);
    }
}