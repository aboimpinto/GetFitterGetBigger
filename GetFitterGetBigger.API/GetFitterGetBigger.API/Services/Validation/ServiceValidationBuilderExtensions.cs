using GetFitterGetBigger.API.DTOs.Interfaces;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Validation;

/// <summary>
/// Extension methods for ServiceValidationBuilder to provide common validation patterns.
/// </summary>
public static class ServiceValidationBuilderExtensions
{
    /// <summary>
    /// Convenience method for matching when T implements IEmptyDto.
    /// Automatically uses T.Empty for validation failures.
    /// </summary>
    /// <typeparam name="T">The DTO type that implements IEmptyDto</typeparam>
    /// <param name="builder">The validation builder instance</param>
    /// <param name="whenValid">Function to execute when validation passes</param>
    /// <returns>The result from either validation failure with T.Empty or the valid function</returns>
    public static async Task<ServiceResult<T>> MatchAsync<T>(
        this ServiceValidationBuilder<T> builder,
        Func<Task<ServiceResult<T>>> whenValid)
        where T : class, IEmptyDto<T>
    {
        // First check if validation has already failed
        if (builder.Validation.HasErrors)
        {
            return builder.Validation.CreateFailureWithEmpty(T.Empty);
        }

        // If no errors, execute the valid function
        return await whenValid();
    }

    /// <summary>
    /// Adds validation to ensure an entity exists using a service result check.
    /// Common pattern for validating entity existence before operations.
    /// </summary>
    /// <typeparam name="T">The result type</typeparam>
    /// <typeparam name="TDto">The DTO type returned by the existence check</typeparam>
    /// <param name="builder">The validation builder instance</param>
    /// <param name="existenceCheck">Function that returns a ServiceResult indicating existence</param>
    /// <param name="errorMessage">Error message if entity doesn't exist</param>
    /// <returns>The builder instance for chaining</returns>
    public static ServiceValidationBuilder<T> EnsureEntityExists<T, TDto>(
        this ServiceValidationBuilder<T> builder,
        Func<Task<ServiceResult<TDto>>> existenceCheck,
        string errorMessage)
    {
        return builder.EnsureAsync(async () =>
        {
            var result = await existenceCheck();
            return (result.IsSuccess, result.IsSuccess ? null : errorMessage);
        });
    }

    /// <summary>
    /// Adds validation to ensure an operation is allowed (returns true/false).
    /// Common pattern for checking business rules like "is entity in use".
    /// </summary>
    /// <typeparam name="T">The result type</typeparam>
    /// <param name="builder">The validation builder instance</param>
    /// <param name="operationCheck">Function that returns true if operation is allowed</param>
    /// <param name="errorMessage">Error message if operation is not allowed</param>
    /// <returns>The builder instance for chaining</returns>
    public static ServiceValidationBuilder<T> EnsureOperationAllowed<T>(
        this ServiceValidationBuilder<T> builder,
        Func<Task<bool>> operationCheck,
        string errorMessage)
    {
        return builder.EnsureAsync(operationCheck, errorMessage);
    }
}