using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Validation;

/// <summary>
/// Extension methods for the generic ServiceValidation to handle final operations.
/// </summary>
public static class ServiceValidationResultExtensions
{
    /// <summary>
    /// Executes an operation that returns a ServiceResult when validation passes.
    /// This is used for the final operation in a validation chain.
    /// </summary>
    /// <typeparam name="T">The type expected as validation context</typeparam>
    /// <typeparam name="TResult">The type returned by the operation</typeparam>
    /// <param name="builder">The validation builder</param>
    /// <param name="whenValid">The operation to execute when all validations pass</param>
    /// <returns>The ServiceResult from the operation or validation failure</returns>
    public static async Task<ServiceResult<TResult>> ExecuteAsync<T, TResult>(
        this ServiceValidationBuilder<T> builder,
        Func<Task<ServiceResult<TResult>>> whenValid)
    {
        // Execute all pending async validations
        var validationResult = await builder.ToValidationResultAsync();
        
        if (!validationResult.IsValid)
        {
            // Convert validation errors to ServiceResult failure
            var errorMessage = validationResult.Errors.Any() 
                ? string.Join("; ", validationResult.Errors) 
                : "Validation failed";
            var serviceError = validationResult.ServiceError ?? 
                              ServiceError.ValidationFailed(errorMessage);
            
            // Use default(TResult) which works for both reference and value types
            return ServiceResult<TResult>.Failure(default(TResult)!, serviceError);
        }
        
        // All validations passed, execute the operation
        return await whenValid();
    }
}