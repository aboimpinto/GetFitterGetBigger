using System.Linq;

namespace GetFitterGetBigger.API.Utilities;

/// <summary>
/// Utility class for generating consistent cache keys across the application
/// Provides both explicit (table name based) and generic (DTO type based) approaches
/// </summary>
public static class CacheKeyGenerator
{
    private const string ReferenceTablePrefix = "ReferenceTable";

    #region Explicit Table Name Methods (Original approach - widely used)

    /// <summary>
    /// Generates a cache key for getting all items from a reference table
    /// </summary>
    /// <param name="tableName">The name of the reference table</param>
    /// <returns>The generated cache key</returns>
    public static string GetAllKey(string tableName)
    {
        return $"{ReferenceTablePrefix}:{tableName}:GetAll";
    }

    /// <summary>
    /// Generates a cache key for getting an item by ID
    /// </summary>
    /// <param name="tableName">The name of the reference table</param>
    /// <param name="id">The ID of the item</param>
    /// <returns>The generated cache key</returns>
    public static string GetByIdKey(string tableName, string id)
    {
        return $"{ReferenceTablePrefix}:{tableName}:GetById:{id}";
    }

    /// <summary>
    /// Generates a cache key for getting an item by value
    /// </summary>
    /// <param name="tableName">The name of the reference table</param>
    /// <param name="value">The value to search for</param>
    /// <returns>The generated cache key</returns>
    public static string GetByValueKey(string tableName, string value)
    {
        // Normalize the value to handle case-insensitive searches
        var normalizedValue = value?.ToLowerInvariant() ?? string.Empty;
        return $"{ReferenceTablePrefix}:{tableName}:GetByValue:{normalizedValue}";
    }

    /// <summary>
    /// Generates a cache key for getting an item by code
    /// </summary>
    /// <param name="tableName">The name of the reference table</param>
    /// <param name="code">The code to search for</param>
    /// <returns>The generated cache key</returns>
    public static string GetByCodeKey(string tableName, string code)
    {
        // Normalize the code to handle case-insensitive searches
        var normalizedCode = code?.ToLowerInvariant() ?? string.Empty;
        return $"{ReferenceTablePrefix}:{tableName}:GetByCode:{normalizedCode}";
    }

    /// <summary>
    /// Generates a pattern for removing all cache entries for a specific table
    /// </summary>
    /// <param name="tableName">The name of the reference table</param>
    /// <returns>The pattern to match cache keys</returns>
    public static string GetTablePattern(string tableName)
    {
        return $"{ReferenceTablePrefix}:{tableName}:";
    }

    #endregion

    #region Generic DTO-based Methods (From Services.Cache - used by MuscleGroupService)

    /// <summary>
    /// Generates a standardized cache key based on DTO type and operation parameters
    /// </summary>
    /// <typeparam name="TDto">The DTO type to extract entity name from</typeparam>
    /// <param name="operation">The operation identifier (e.g., "byName", "byBodyPart")</param>
    /// <param name="parameters">The parameters to include in the cache key</param>
    /// <returns>A standardized cache key</returns>
    public static string Generate<TDto>(string operation, params object[] parameters)
    {
        var entityName = ExtractEntityNameFromType<TDto>();
        var paramString = parameters.Length > 0 
            ? string.Join(":", parameters.Select(p => p?.ToString()?.ToLowerInvariant() ?? ""))
            : "";
        
        // Use consistent prefix for all reference data
        var baseKey = $"{ReferenceTablePrefix}:{entityName}:{operation}";
        
        return string.IsNullOrEmpty(paramString) 
            ? baseKey
            : $"{baseKey}:{paramString}";
    }
    
    /// <summary>
    /// Generates a cache key for "all" operations using generic type
    /// </summary>
    /// <typeparam name="TDto">The DTO type to extract entity name from</typeparam>
    /// <returns>A cache key for retrieving all entities</returns>
    public static string GenerateForAll<TDto>()
    {
        var entityName = ExtractEntityNameFromType<TDto>();
        return GetAllKey(entityName);
    }
    
    /// <summary>
    /// Generates a cache key for "byId" operations using generic type
    /// </summary>
    /// <typeparam name="TDto">The DTO type to extract entity name from</typeparam>
    /// <param name="id">The entity ID</param>
    /// <returns>A cache key for retrieving entity by ID</returns>
    public static string GenerateForId<TDto>(object id)
    {
        var entityName = ExtractEntityNameFromType<TDto>();
        return GetByIdKey(entityName, id?.ToString() ?? "");
    }

    /// <summary>
    /// Generates a cache key for "byValue" operations using generic type
    /// </summary>
    /// <typeparam name="TDto">The DTO type to extract entity name from</typeparam>
    /// <param name="value">The value to search for</param>
    /// <returns>A cache key for retrieving entity by value</returns>
    public static string GenerateForValue<TDto>(string value)
    {
        var entityName = ExtractEntityNameFromType<TDto>();
        return GetByValueKey(entityName, value);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Extracts the entity name from a DTO type
    /// </summary>
    private static string ExtractEntityNameFromType<TDto>()
    {
        var typeName = typeof(TDto).Name;
        
        // Remove "Dto" suffix if present
        if (typeName.EndsWith("Dto"))
        {
            typeName = typeName[..^3];
        }
        
        // Pluralize common entity names for consistency with table names
        // This ensures MuscleGroupDto -> MuscleGroups (matching the explicit table name approach)
        return PluralizeEntityName(typeName);
    }

    /// <summary>
    /// Simple pluralization for common patterns
    /// </summary>
    private static string PluralizeEntityName(string entityName)
    {
        // Handle common patterns
        return entityName switch
        {
            "BodyPart" => "BodyParts",
            "MuscleGroup" => "MuscleGroups",
            "Equipment" => "Equipment", // Already plural/uncountable
            "Exercise" => "Exercises",
            "ExerciseType" => "ExerciseTypes",
            "DifficultyLevel" => "DifficultyLevels",
            "ExecutionProtocol" => "ExecutionProtocols",
            "ExerciseWeightType" => "ExerciseWeightTypes",
            "KineticChainType" => "KineticChainTypes",
            "MetricType" => "MetricTypes",
            "MovementPattern" => "MovementPatterns",
            "MuscleRole" => "MuscleRoles",
            "WorkoutCategory" => "WorkoutCategories",
            "WorkoutObjective" => "WorkoutObjectives",
            "WorkoutState" => "WorkoutStates",
            _ => entityName + "s" // Default simple pluralization
        };
    }

    #endregion
}