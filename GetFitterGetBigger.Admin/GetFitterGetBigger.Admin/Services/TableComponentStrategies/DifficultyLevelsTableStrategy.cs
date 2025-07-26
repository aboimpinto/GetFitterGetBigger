using System;

namespace GetFitterGetBigger.Admin.Services.TableComponentStrategies
{
    /// <summary>
    /// Strategy for the Difficulty Levels reference table component.
    /// </summary>
    public class DifficultyLevelsTableStrategy : TableComponentStrategyBase
    {
        public override string TableName => "DifficultyLevels";
        
        public override string DisplayName => "Difficulty Levels";
        
        public override Type ComponentType => typeof(Components.Pages.ReferenceTableComponents.GenericReferenceTable);
    }
}