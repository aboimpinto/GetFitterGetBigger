using GetFitterGetBigger.API.DTOs.Interfaces;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Validation;

/// <summary>
/// Extension methods for ServiceValidation to provide convenience methods
/// when working with DTOs that implement IEmptyDto.
/// </summary>
public static class ServiceValidationExtensions
{
    /// <summary>
    /// Convenience method for matching when T implements IEmptyDto.
    /// Automatically uses T.Empty for validation failures.
    /// This provides a cleaner API when you don't need custom error handling.
    /// </summary>
    /// <typeparam name="T">The DTO type that implements IEmptyDto</typeparam>
    /// <param name="validation">The validation instance</param>
    /// <param name="whenValid">Function to execute when validation passes</param>
    /// <returns>The result from either validation failure with T.Empty or the valid function</returns>
    public static async Task<ServiceResult<T>> MatchAsync<T>(
        this ServiceValidation<T> validation,
        Func<Task<ServiceResult<T>>> whenValid)
        where T : class, IEmptyDto<T>
    {
        // Check if validation has errors
        if (validation.HasErrors)
        {
            // Use the internal method that properly handles ServiceError
            return validation.CreateFailureWithEmpty(T.Empty);
        }
        
        // If no errors, execute the valid function
        return await whenValid();
    }

    /// <summary>
    /// Convenience method for synchronous matching when T implements IEmptyDto.
    /// Automatically uses T.Empty for validation failures.
    /// </summary>
    /// <typeparam name="T">The DTO type that implements IEmptyDto</typeparam>
    /// <param name="validation">The validation instance</param>
    /// <param name="whenValid">Function to execute when validation passes</param>
    /// <returns>The result from either validation failure with T.Empty or the valid function</returns>
    public static ServiceResult<T> Match<T>(
        this ServiceValidation<T> validation,
        Func<ServiceResult<T>> whenValid)
        where T : class, IEmptyDto<T>
    {
        // Check if validation has errors
        if (validation.HasErrors)
        {
            // Use the internal method that properly handles ServiceError
            return validation.CreateFailureWithEmpty(T.Empty);
        }
        
        // If no errors, execute the valid function
        return whenValid();
    }
}