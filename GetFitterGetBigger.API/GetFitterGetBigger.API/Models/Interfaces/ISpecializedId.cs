namespace GetFitterGetBigger.API.Models.Interfaces;

/// <summary>
/// Common interface for all specialized ID types in the system.
/// Provides type-safe ID handling and eliminates string conversions.
/// </summary>
/// <typeparam name="TSelf">The implementing ID type (for static polymorphism)</typeparam>
public interface ISpecializedId<TSelf> : ISpecializedIdBase where TSelf : struct, ISpecializedId<TSelf>
{
    /// <summary>
    /// Gets a value indicating whether this ID represents an empty/null value
    /// </summary>
    new bool IsEmpty { get; }
    
    /// <summary>
    /// Converts the ID to its underlying GUID value
    /// </summary>
    /// <returns>The GUID representation of this ID</returns>
    new Guid ToGuid();
    
    /// <summary>
    /// Gets an empty instance of this ID type
    /// </summary>
    static abstract TSelf Empty { get; }
    
    /// <summary>
    /// Creates an ID from a GUID value
    /// </summary>
    /// <param name="guid">The GUID to convert</param>
    /// <returns>A new ID instance</returns>
    static abstract TSelf From(Guid guid);
    
    /// <summary>
    /// Parses a string into an ID, returning Empty if parsing fails
    /// </summary>
    /// <param name="input">The string to parse</param>
    /// <returns>The parsed ID or Empty if parsing fails</returns>
    static abstract TSelf ParseOrEmpty(string? input);
}