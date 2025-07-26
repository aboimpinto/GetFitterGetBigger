using System;
using System.Collections.Generic;
using System.Linq;
using GetFitterGetBigger.Admin.Services.TableComponentStrategies;
using Microsoft.Extensions.Logging;

namespace GetFitterGetBigger.Admin.Services
{
    /// <summary>
    /// Implementation of table component registry using a strategy pattern
    /// to map table names to their corresponding Blazor components.
    /// This design allows adding new tables without modifying this class.
    /// </summary>
    public class TableComponentRegistry : ITableComponentRegistry
    {
        private readonly Dictionary<string, ITableComponentStrategy> _strategies;
        private readonly ILogger<TableComponentRegistry> _logger;

        public TableComponentRegistry(
            IEnumerable<ITableComponentStrategy> strategies,
            ILogger<TableComponentRegistry> logger)
        {
            _logger = logger;
            _strategies = strategies.ToDictionary(
                s => s.TableName, 
                StringComparer.OrdinalIgnoreCase);
            
            _logger.LogInformation("TableComponentRegistry initialized with {StrategyCount} strategies", 
                _strategies.Count);
        }

        public Type? GetComponentType(string tableName)
        {
            Type? componentType = null;
            
            if (!string.IsNullOrWhiteSpace(tableName) && 
                _strategies.TryGetValue(tableName, out var strategy))
            {
                componentType = strategy.ComponentType;
                _logger.LogDebug("Found component {ComponentType} for table {TableName}", 
                    componentType.Name, tableName);
            }
            else
            {
                _logger.LogWarning(!string.IsNullOrWhiteSpace(tableName) 
                    ? "No component mapping found for table {TableName}" 
                    : "GetComponentType called with null or empty tableName", 
                    tableName);
            }
            
            return componentType;
        }

        public string GetDisplayName(string tableName)
        {
            string displayName = "Unknown";
            
            if (!string.IsNullOrWhiteSpace(tableName) && 
                _strategies.TryGetValue(tableName, out var strategy))
            {
                displayName = strategy.DisplayName;
            }
            
            return displayName;
        }
    }
}