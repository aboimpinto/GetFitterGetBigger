using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GetFitterGetBigger.API.Services.Validation;

/// <summary>
/// Provides intelligent naming conventions for storing repositories and data in DynamicChainContext.
/// </summary>
public static class NamingEngine
{
    /// <summary>
    /// Generates a storage key for a repository based on type and access mode.
    /// Example: IUserRepository with isReadOnly=true becomes "ReadOnlyUserRepository"
    /// </summary>
    /// <param name="repositoryType">The repository interface type</param>
    /// <param name="isReadOnly">Whether this is a read-only repository</param>
    /// <returns>The storage key for the repository</returns>
    public static string GetRepositoryKey(Type repositoryType, bool isReadOnly)
    {
        if (repositoryType == null)
            throw new ArgumentNullException(nameof(repositoryType));

        // Remove 'I' prefix from interface name
        var typeName = repositoryType.Name;
        if (typeName.StartsWith("I") && typeName.Length > 1 && char.IsUpper(typeName[1]))
        {
            typeName = typeName.Substring(1);
        }

        // Add access mode prefix
        var prefix = isReadOnly ? "ReadOnly" : "Writable";
        return $"{prefix}{typeName}";
    }

    /// <summary>
    /// Generates a storage key for data based on its type.
    /// Handles collections, ServiceResult wrappers, and nested generics.
    /// </summary>
    /// <param name="dataType">The type of data to generate a key for</param>
    /// <returns>The storage key for the data</returns>
    public static string GetDataKey(Type dataType)
    {
        if (dataType == null)
            throw new ArgumentNullException(nameof(dataType));

        // Handle ServiceResult<T> - unwrap to inner type
        if (IsServiceResult(dataType, out var innerType))
        {
            return GetDataKey(innerType);
        }

        // Handle collections - pluralize the element type
        if (IsCollection(dataType, out var elementType))
        {
            return Pluralize(elementType.Name);
        }

        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(dataType);
        if (underlyingType != null)
        {
            return GetDataKey(underlyingType);
        }

        // For regular types, use the type name as-is
        return dataType.Name;
    }

    /// <summary>
    /// Determines if a type is a ServiceResult and extracts its inner type.
    /// </summary>
    private static bool IsServiceResult(Type type, out Type innerType)
    {
        innerType = null!;

        if (!type.IsGenericType)
            return false;

        var genericDef = type.GetGenericTypeDefinition();
        
        // Check if it's ServiceResult<T>
        if (genericDef.Name.StartsWith("ServiceResult`"))
        {
            innerType = type.GetGenericArguments()[0];
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determines if a type is a collection and extracts its element type.
    /// </summary>
    private static bool IsCollection(Type type, out Type elementType)
    {
        elementType = null!;

        // Check for array
        if (type.IsArray)
        {
            elementType = type.GetElementType()!;
            return true;
        }

        // Check for generic collections
        if (type.IsGenericType)
        {
            var genericDef = type.GetGenericTypeDefinition();
            
            // Common collection interfaces and implementations
            var collectionTypes = new[]
            {
                typeof(IEnumerable<>),
                typeof(ICollection<>),
                typeof(IList<>),
                typeof(List<>),
                typeof(HashSet<>),
                typeof(ISet<>),
                typeof(IReadOnlyList<>),
                typeof(IReadOnlyCollection<>)
            };

            foreach (var collectionType in collectionTypes)
            {
                if (genericDef == collectionType || 
                    type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == collectionType))
                {
                    elementType = type.GetGenericArguments()[0];
                    return true;
                }
            }
        }

        // Check if it implements non-generic IEnumerable (but not string)
        if (type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type))
        {
            // Try to find the element type from generic interfaces
            var enumerableInterface = type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            
            if (enumerableInterface != null)
            {
                elementType = enumerableInterface.GetGenericArguments()[0];
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Pluralizes a singular word using basic English rules.
    /// </summary>
    private static string Pluralize(string singular)
    {
        if (string.IsNullOrEmpty(singular))
            return singular;

        // Handle some common irregular plurals
        var irregulars = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Person", "People" },
            { "Child", "Children" },
            { "Man", "Men" },
            { "Woman", "Women" },
            { "Tooth", "Teeth" },
            { "Foot", "Feet" },
            { "Mouse", "Mice" },
            { "Goose", "Geese" }
        };

        if (irregulars.TryGetValue(singular, out var irregular))
            return irregular;

        // Handle words ending in 'y' preceded by consonant
        if (singular.Length > 1 && singular.EndsWith("y", StringComparison.OrdinalIgnoreCase))
        {
            var beforeY = singular[singular.Length - 2];
            if (!IsVowel(beforeY))
            {
                return singular.Substring(0, singular.Length - 1) + "ies";
            }
        }

        // Handle words ending in 's', 'ss', 'sh', 'ch', 'x', 'z'
        if (singular.EndsWith("s", StringComparison.OrdinalIgnoreCase) ||
            singular.EndsWith("ss", StringComparison.OrdinalIgnoreCase) ||
            singular.EndsWith("sh", StringComparison.OrdinalIgnoreCase) ||
            singular.EndsWith("ch", StringComparison.OrdinalIgnoreCase) ||
            singular.EndsWith("x", StringComparison.OrdinalIgnoreCase) ||
            singular.EndsWith("z", StringComparison.OrdinalIgnoreCase))
        {
            return singular + "es";
        }

        // Handle words ending in 'o' preceded by consonant
        if (singular.Length > 1 && singular.EndsWith("o", StringComparison.OrdinalIgnoreCase))
        {
            var beforeO = singular[singular.Length - 2];
            if (!IsVowel(beforeO))
            {
                // Common exceptions that just add 's'
                var exceptions = new[] { "photo", "piano", "halo" };
                if (!exceptions.Any(e => singular.EndsWith(e, StringComparison.OrdinalIgnoreCase)))
                {
                    return singular + "es";
                }
            }
        }

        // Handle words ending in 'f' or 'fe'
        if (singular.EndsWith("f", StringComparison.OrdinalIgnoreCase))
        {
            return singular.Substring(0, singular.Length - 1) + "ves";
        }
        if (singular.EndsWith("fe", StringComparison.OrdinalIgnoreCase))
        {
            return singular.Substring(0, singular.Length - 2) + "ves";
        }

        // Default: just add 's'
        return singular + "s";
    }

    /// <summary>
    /// Determines if a character is a vowel.
    /// </summary>
    private static bool IsVowel(char c)
    {
        return "aeiouAEIOU".Contains(c);
    }

    /// <summary>
    /// Generates a suggested key for complex types that don't have obvious naming.
    /// This is used when automatic naming would produce unclear results.
    /// </summary>
    public static string GetSuggestedKeyForComplexType(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            var keyType = type.GetGenericArguments()[0];
            var valueType = type.GetGenericArguments()[1];
            
            // For Dictionary<string, List<Exercise>> suggest "ExercisesByKey" or similar
            if (IsCollection(valueType, out var elementType))
            {
                return $"{Pluralize(elementType.Name)}By{keyType.Name}";
            }
            
            return $"{valueType.Name}By{keyType.Name}";
        }

        // For other complex types, return a descriptive but generic name
        return $"Complex{type.Name.Replace("`", "").Replace("[", "").Replace("]", "")}";
    }
}