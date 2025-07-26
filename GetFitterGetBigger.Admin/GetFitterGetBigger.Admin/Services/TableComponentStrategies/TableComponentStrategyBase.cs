using System;
using System.Collections.Generic;

namespace GetFitterGetBigger.Admin.Services.TableComponentStrategies
{
    /// <summary>
    /// Base implementation for table component strategies.
    /// Provides common functionality and enforces the strategy pattern.
    /// </summary>
    public abstract class TableComponentStrategyBase : ITableComponentStrategy
    {
        /// <inheritdoc />
        public abstract string TableName { get; }
        
        /// <inheritdoc />
        public abstract string DisplayName { get; }
        
        /// <inheritdoc />
        public abstract Type ComponentType { get; }
        
        /// <inheritdoc />
        /// <remarks>
        /// Default implementation returns null. Override if your component needs parameters.
        /// </remarks>
        public virtual Dictionary<string, object>? GetComponentParameters()
        {
            return null;
        }
    }
}