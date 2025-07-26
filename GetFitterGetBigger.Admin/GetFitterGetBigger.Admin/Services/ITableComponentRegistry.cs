using System;

namespace GetFitterGetBigger.Admin.Services
{
    /// <summary>
    /// Registry for mapping reference table names to their corresponding Blazor components.
    /// Follows the same strategy pattern as ReferenceDataService.
    /// </summary>
    public interface ITableComponentRegistry
    {
        /// <summary>
        /// Gets the component type for the specified table name.
        /// </summary>
        /// <param name="tableName">The name of the reference table</param>
        /// <returns>The component type if found, null otherwise</returns>
        Type? GetComponentType(string tableName);
        
        /// <summary>
        /// Gets the display name for the specified table name.
        /// </summary>
        /// <param name="tableName">The name of the reference table</param>
        /// <returns>The display name</returns>
        string GetDisplayName(string tableName);
    }
}