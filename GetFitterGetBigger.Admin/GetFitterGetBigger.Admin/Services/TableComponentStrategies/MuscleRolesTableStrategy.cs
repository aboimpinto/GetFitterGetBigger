using System;

namespace GetFitterGetBigger.Admin.Services.TableComponentStrategies
{
    /// <summary>
    /// Strategy for the Muscle Roles reference table component.
    /// </summary>
    public class MuscleRolesTableStrategy : TableComponentStrategyBase
    {
        public override string TableName => "MuscleRoles";
        
        public override string DisplayName => "Muscle Roles";
        
        public override Type ComponentType => typeof(Components.Pages.ReferenceTableComponents.GenericReferenceTable);
    }
}