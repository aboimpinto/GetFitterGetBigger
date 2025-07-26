using System;

namespace GetFitterGetBigger.Admin.Services.TableComponentStrategies
{
    /// <summary>
    /// Strategy for the Body Parts reference table component.
    /// </summary>
    public class BodyPartsTableStrategy : TableComponentStrategyBase
    {
        public override string TableName => "BodyParts";
        
        public override string DisplayName => "Body Parts";
        
        public override Type ComponentType => typeof(Components.Pages.ReferenceTableComponents.GenericReferenceTable);
    }
}