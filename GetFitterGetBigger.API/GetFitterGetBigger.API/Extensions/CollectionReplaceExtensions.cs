using System.Collections.Generic;

namespace GetFitterGetBigger.API.Extensions;

/// <summary>
/// Extension methods for efficiently replacing collection contents.
/// Provides optimized collection replacement using AddRange where possible.
/// </summary>
public static class CollectionReplaceExtensions
{
    /// <summary>
    /// Replaces all items in a collection with new items efficiently.
    /// Clears the existing collection and adds the new items using the most efficient method available.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection</typeparam>
    /// <param name="collection">The collection to replace contents of</param>
    /// <param name="newItems">The new items to add to the collection</param>
    public static void ReplaceWith<T>(this ICollection<T> collection, IEnumerable<T> newItems)
    {
        // Clear existing items
        collection.Clear();
        
        // Add new items using the most efficient method for the collection type
        switch (collection)
        {
            case List<T> list:
                // List<T> has AddRange which is more efficient than foreach
                list.AddRange(newItems);
                break;
                
            case HashSet<T> hashSet:
                // HashSet<T> has UnionWith which is optimized for set operations
                hashSet.UnionWith(newItems);
                break;
                
            default:
                // Fallback for other collection types (ICollection<T>, Collection<T>, etc.)
                foreach (var item in newItems)
                {
                    collection.Add(item);
                }
                break;
        }
    }
    
    /// <summary>
    /// Replaces all items in a collection with new items from another collection.
    /// Optimized overload when both source and target are collections.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection</typeparam>
    /// <param name="collection">The collection to replace contents of</param>
    /// <param name="newItems">The new collection to copy items from</param>
    public static void ReplaceWith<T>(this ICollection<T> collection, ICollection<T> newItems)
    {
        // If both are the same reference, do nothing
        if (ReferenceEquals(collection, newItems))
            return;
        
        // Clear and replace
        collection.Clear();
        
        // Use optimized path for known collection types
        switch (collection)
        {
            case List<T> list when newItems is IList<T> sourceList:
                // When both are lists, we can pre-allocate capacity for better performance
                list.Capacity = sourceList.Count;
                list.AddRange(sourceList);
                break;
                
            case List<T> list:
                list.AddRange(newItems);
                break;
                
            case HashSet<T> hashSet:
                hashSet.UnionWith(newItems);
                break;
                
            default:
                foreach (var item in newItems)
                {
                    collection.Add(item);
                }
                break;
        }
    }
}