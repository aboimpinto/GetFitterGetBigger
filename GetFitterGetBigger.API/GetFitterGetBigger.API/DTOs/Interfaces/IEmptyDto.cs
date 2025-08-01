namespace GetFitterGetBigger.API.DTOs.Interfaces;

/// <summary>
/// Defines a contract for DTOs that can provide an empty instance.
/// This interface ensures that DTOs can be used with validation patterns
/// that need to return empty values on validation failure.
/// </summary>
/// <typeparam name="T">The type of the DTO implementing this interface</typeparam>
public interface IEmptyDto<T> where T : class
{
    /// <summary>
    /// Gets a static empty instance of the DTO.
    /// This instance should have all properties set to their default/empty values.
    /// </summary>
    static abstract T Empty { get; }
    
    /// <summary>
    /// Gets a value indicating whether this instance represents an empty DTO.
    /// </summary>
    bool IsEmpty { get; }
}