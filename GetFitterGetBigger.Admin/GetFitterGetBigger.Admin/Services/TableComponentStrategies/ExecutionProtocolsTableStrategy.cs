using System;

namespace GetFitterGetBigger.Admin.Services.TableComponentStrategies
{
    /// <summary>
    /// Strategy for the Execution Protocols reference table component.
    /// </summary>
    public class ExecutionProtocolsTableStrategy : TableComponentStrategyBase
    {
        public override string TableName => "ExecutionProtocols";
        
        public override string DisplayName => "Execution Protocols";
        
        public override Type ComponentType => typeof(Components.Pages.ReferenceTableComponents.ExecutionProtocolsTable);
    }
}