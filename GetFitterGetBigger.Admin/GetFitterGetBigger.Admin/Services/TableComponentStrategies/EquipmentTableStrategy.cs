using System;

namespace GetFitterGetBigger.Admin.Services.TableComponentStrategies
{
    /// <summary>
    /// Strategy for the Equipment reference table component.
    /// </summary>
    public class EquipmentTableStrategy : TableComponentStrategyBase
    {
        public override string TableName => "Equipment";
        
        public override string DisplayName => "Equipment";
        
        public override Type ComponentType => typeof(Components.Pages.Equipment.EquipmentTable);
    }
}