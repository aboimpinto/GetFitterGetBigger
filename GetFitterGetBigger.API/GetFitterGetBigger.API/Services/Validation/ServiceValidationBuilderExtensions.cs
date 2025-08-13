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
        // Execute all validations (including async) using the builder's MatchAsync method
        // IMPORTANT: Use the ServiceError overload to ensure async ServiceError validations are executed
        return await builder.MatchAsync(
            whenValid: whenValid,
            whenInvalid: (IReadOnlyList<ServiceError> errors) => 
            {
                // Use the first ServiceError if available, otherwise create ValidationFailed
                var firstError = errors.FirstOrDefault() ?? ServiceError.ValidationFailed("Validation failed");
                return ServiceResult<T>.Failure(T.Empty, firstError);
            });
    }

    /// <summary>
    /// Loads an entity and converts it to DTO, handling database errors gracefully.
    /// Returns success with the loaded DTO (or Empty DTO if not found).
    /// Database exceptions are converted to validation failures internally.
    /// </summary>
    /// <typeparam name="T">The DTO type that implements IEmptyDto</typeparam>
    /// <param name="builder">The validation builder instance</param>
    /// <param name="loadFunc">Function that loads entity and returns DTO (can return Empty DTO)</param>
    /// <param name="errorMessage">Error message if loading fails</param>
    /// <returns>The builder instance for chaining</returns>
    public static ServiceValidationBuilder<T> LoadUserEntityAsync<T>(
        this ServiceValidationBuilder<T> builder,
        Func<Task<T>> loadFunc,
        string errorMessage = "Failed to load user")
        where T : class, IEmptyDto<T>
    {
        return builder.EnsureAsync(async () =>
        {
            try
            {
                var result = await loadFunc();
                // Always return true - we handle "not found" as Empty DTO, not as failure
                return true;
            }
            catch (Exception)
            {
                // Convert database errors to validation failures
                return false;
            }
        }, errorMessage);
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
    
    /// <summary>
    /// Executes the provided function when validation passes.
    /// Automatically returns validation failure if validation fails.
    /// For async operations that return Task of ServiceResult.
    /// Uses Empty Object Pattern for types that implement IEmptyDto.
    /// </summary>
    /// <typeparam name="T">The result type</typeparam>
    /// <param name="builder">The validation builder instance</param>
    /// <param name="whenValid">Async function to execute when validation passes</param>
    /// <returns>The result from either validation failure or the valid function</returns>
    public static async Task<ServiceResult<T>> WhenValidAsync<T>(
        this ServiceValidationBuilder<T> builder,
        Func<Task<ServiceResult<T>>> whenValid)
    {
        // Execute async validations first
        await ExecuteAsyncValidations(builder);
        
        // Check if validation has failed (including async validations)
        if (builder.Validation.HasErrors)
        {
            // Determine the empty value to use
            T emptyValue;
            var type = typeof(T);
            
            // Check if type implements IEmptyDto<T> interface
            var emptyDtoInterface = type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && 
                    i.GetGenericTypeDefinition() == typeof(IEmptyDto<>) &&
                    i.GetGenericArguments()[0] == type);
            
            if (emptyDtoInterface != null)
            {
                // Use Empty Object Pattern for DTOs that implement IEmptyDto
                var emptyProperty = type.GetProperty("Empty", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                emptyValue = (T)(emptyProperty?.GetValue(null) ?? default(T)!);
            }
            else
            {
                // Use default for other types
                emptyValue = default(T)!;
            }

            // If it has a ServiceError, use it; otherwise create ValidationFailed
            var error = builder.Validation.HasServiceError 
                ? builder.Validation.CreateFailureWithEmpty(emptyValue)
                : ServiceResult<T>.Failure(
                    emptyValue, 
                    ServiceError.ValidationFailed(builder.Validation.ValidationErrors.FirstOrDefault() ?? "Validation failed"));
            
            return error;
        }

        // If no errors, execute the valid function
        return await whenValid();
    }

    /// <summary>
    /// Executes async validations stored in the builder.
    /// This method uses reflection to access private fields since the async validations aren't publicly exposed.
    /// </summary>
    private static async Task ExecuteAsyncValidations<T>(ServiceValidationBuilder<T> builder)
    {
        var builderType = typeof(ServiceValidationBuilder<T>);
        
        // Get the private fields using reflection
        var asyncValidationsField = builderType.GetField("_asyncValidations", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var asyncServiceErrorValidationsField = builderType.GetField("_asyncServiceErrorValidations", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (asyncValidationsField?.GetValue(builder) is List<Func<Task<(bool IsValid, string? Error)>>> asyncValidations)
        {
            foreach (var validation in asyncValidations)
            {
                var (isValid, error) = await validation();
                if (!isValid && error != null)
                {
                    builder.Validation.Ensure(() => false, error);
                }
            }
        }

        if (asyncServiceErrorValidationsField?.GetValue(builder) is List<Func<Task<(bool IsValid, ServiceError? Error)>>> asyncServiceErrorValidations)
        {
            foreach (var validation in asyncServiceErrorValidations)
            {
                var (isValid, error) = await validation();
                if (!isValid && error != null)
                {
                    builder.Validation.Ensure(() => false, error);
                }
            }
        }
    }
}