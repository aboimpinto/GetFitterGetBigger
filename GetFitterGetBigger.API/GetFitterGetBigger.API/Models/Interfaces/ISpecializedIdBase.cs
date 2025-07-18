namespace GetFitterGetBigger.API.Models.Interfaces;

/// <summary>
/// Non-generic base interface for specialized IDs that provides common functionality
/// needed by service layer without requiring knowledge of the concrete ID type.
/// </summary>
public interface ISpecializedIdBase
{
    /// <summary>
    /// Indicates whether this ID represents an empty/null value.
    /// </summary>
    bool IsEmpty { get; }
    
    /// <summary>
    /// Converts the specialized ID to its underlying Guid value.
    /// </summary>
    Guid ToGuid();
    
    /// <summary>
    /// Gets the string representation of this ID (e.g., "equipment-12345678-1234-5678-1234-567812345678").
    /// </summary>
    string ToString();
    
    /// <summary>
    /// Gets the type-specific prefix for this ID (e.g., "equipment", "muscle-group").
    /// </summary>
    string GetPrefix();
}