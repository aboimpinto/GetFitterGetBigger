namespace GetFitterGetBigger.API.Models.Interfaces;

/// <summary>
/// Base interface for objects that support the Empty/Null Object pattern
/// </summary>
public interface IEmpty
{
    /// <summary>
    /// Gets a value indicating whether this is an empty/null object instance
    /// </summary>
    bool IsEmpty { get; }
}