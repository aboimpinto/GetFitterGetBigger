using GetFitterGetBigger.API.Models.Entities;

namespace GetFitterGetBigger.API.Models.Validation;

/// <summary>
/// Provides a static entry point for creating validation chains.
/// </summary>
public static class Validate
{
    /// <summary>
    /// Starts a new validation chain for the specified entity type.
    /// </summary>
    /// <typeparam name="T">The type of entity to validate (must implement IEmptyEntity)</typeparam>
    /// <returns>A new EntityValidation instance for building the validation chain</returns>
    public static EntityValidation<T> For<T>() where T : class, IEmptyEntity<T>
    {
        return new EntityValidation<T>();
    }
}