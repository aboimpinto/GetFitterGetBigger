using System;
using System.Collections.Generic;

namespace GetFitterGetBigger.Admin.Services.TableComponentStrategies
{
    /// <summary>
    /// Strategy interface for table component configuration.
    /// Each implementation represents the configuration for a specific reference table.
    /// </summary>
    public interface ITableComponentStrategy
    {
        /// <summary>
        /// The table name this strategy handles.
        /// This should match the API endpoint naming convention.
        /// </summary>
        string TableName { get; }
        
        /// <summary>
        /// Display name for UI presentation.
        /// </summary>
        string DisplayName { get; }
        
        /// <summary>
        /// The Blazor component type for rendering this table.
        /// </summary>
        Type ComponentType { get; }
        
        /// <summary>
        /// Optional: Additional parameters to pass to the component.
        /// </summary>
        /// <returns>Dictionary of parameters or null if none needed</returns>
        Dictionary<string, object>? GetComponentParameters();
    }
}