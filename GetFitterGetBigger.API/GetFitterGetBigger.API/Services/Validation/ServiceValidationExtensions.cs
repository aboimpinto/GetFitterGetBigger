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
    
    /// <summary>
    /// Validates using a method that returns ValidationResult.
    /// This allows delegating validation logic to specialized methods that already return
    /// ValidationResult with appropriate error messages.
    /// </summary>
    /// <typeparam name="T">The DTO type that implements IEmptyDto</typeparam>
    /// <param name="validation">The validation instance</param>
    /// <param name="validationFunc">Function that performs validation and returns ValidationResult</param>
    /// <returns>The validation instance for chaining</returns>
    public static ServiceValidation<T> ValidateWith<T>(
        this ServiceValidation<T> validation,
        Func<ValidationResult> validationFunc)
        where T : class, IEmptyDto<T>
    {
        var result = validationFunc();
        
        if (!result.IsValid)
        {
            // If there's a ServiceError, use it
            if (result.ServiceError != null)
            {
                validation.Ensure(() => false, result.ServiceError);
            }
            // Otherwise use the error messages
            else if (result.Errors.Any())
            {
                foreach (var error in result.Errors)
                {
                    validation.Ensure(() => false, error);
                }
            }
        }
        
        return validation;
    }

    /// <summary>
    /// Validates a ServiceResult and adds any errors to the validation chain.
    /// This allows incorporating ServiceResult validation into the fluent validation chain.
    /// </summary>
    /// <typeparam name="T">The DTO type that implements IEmptyDto</typeparam>
    /// <param name="validation">The validation instance</param>
    /// <param name="serviceResultFunc">Function that returns a ServiceResult to validate</param>
    /// <returns>The validation instance for chaining</returns>
    public static async Task<ServiceValidation<T>> EnsureServiceResultAsync<T>(
        this ServiceValidation<T> validation,
        Func<Task<ServiceResult<bool>>> serviceResultFunc)
        where T : class, IEmptyDto<T>
    {
        // If validation already has errors, don't execute the service call
        if (validation.HasErrors)
        {
            return validation;
        }

        var result = await serviceResultFunc();
        
        if (!result.IsSuccess)
        {
            // Add all the errors from the ServiceResult to our validation chain
            // Prefer structured errors if available
            if (result.StructuredErrors != null && result.StructuredErrors.Any())
            {
                foreach (var error in result.StructuredErrors)
                {
                    validation.Ensure(() => false, error);
                }
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    validation.Ensure(() => false, error);
                }
            }
        }
        
        return validation;
    }

    /// <summary>
    /// Validates a ServiceResult and stores the data for later use if successful.
    /// This allows checking ServiceResult success and using its data in the match.
    /// </summary>
    /// <typeparam name="T">The DTO type that implements IEmptyDto</typeparam>
    /// <typeparam name="TData">The type of data in the ServiceResult</typeparam>
    /// <param name="validation">The validation instance</param>
    /// <param name="serviceResultFunc">Function that returns a ServiceResult with data</param>
    /// <param name="dataHandler">Action to handle the data if successful</param>
    /// <returns>The validation instance for chaining</returns>
    public static async Task<ServiceValidation<T>> EnsureServiceResultAsync<T, TData>(
        this ServiceValidation<T> validation,
        Func<Task<ServiceResult<TData>>> serviceResultFunc,
        Action<TData> dataHandler)
        where T : class, IEmptyDto<T>
    {
        // If validation already has errors, don't execute the service call
        if (validation.HasErrors)
        {
            return validation;
        }

        var result = await serviceResultFunc();
        
        if (!result.IsSuccess)
        {
            // Add all the errors from the ServiceResult to our validation chain
            // Prefer structured errors if available
            if (result.StructuredErrors != null && result.StructuredErrors.Any())
            {
                foreach (var error in result.StructuredErrors)
                {
                    validation.Ensure(() => false, error);
                }
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    validation.Ensure(() => false, error);
                }
            }
        }
        else if (dataHandler != null)
        {
            dataHandler(result.Data);
        }
        
        return validation;
    }

    /// <summary>
    /// Wraps a ServiceValidation instance to allow continuing the chain after async operations.
    /// This enables mixing sync and async validations in a single fluent chain.
    /// </summary>
    /// <typeparam name="T">The DTO type that implements IEmptyDto</typeparam>
    /// <param name="validation">The validation instance</param>
    /// <returns>A task wrapping the validation for async chaining</returns>
    public static Task<ServiceValidation<T>> AsAsync<T>(
        this ServiceValidation<T> validation)
        where T : class, IEmptyDto<T>
    {
        return Task.FromResult(validation);
    }

    /// <summary>
    /// Validates a ServiceResult from a Task continuation and adds any errors to the validation chain.
    /// This allows chaining async service result validation after sync validations.
    /// </summary>
    /// <typeparam name="T">The DTO type that implements IEmptyDto</typeparam>
    /// <param name="validationTask">The async validation task</param>
    /// <param name="serviceResultFunc">Function that returns a ServiceResult to validate</param>
    /// <returns>The validation instance for continued chaining</returns>
    public static async Task<ServiceValidation<T>> EnsureServiceResultAsync<T>(
        this Task<ServiceValidation<T>> validationTask,
        Func<Task<ServiceResult<bool>>> serviceResultFunc)
        where T : class, IEmptyDto<T>
    {
        var validation = await validationTask;
        
        // If validation already has errors, don't execute the service call
        if (validation.HasErrors)
        {
            return validation;
        }

        var result = await serviceResultFunc();
        
        if (!result.IsSuccess)
        {
            // Add all the errors from the ServiceResult to our validation chain
            // Prefer structured errors if available
            if (result.StructuredErrors != null && result.StructuredErrors.Any())
            {
                foreach (var error in result.StructuredErrors)
                {
                    validation.Ensure(() => false, error);
                }
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    validation.Ensure(() => false, error);
                }
            }
        }
        
        return validation;
    }

    /// <summary>
    /// Continues a validation chain from an async operation and performs matching.
    /// This allows keeping the fluent chain intact when using async validation operations.
    /// </summary>
    /// <typeparam name="T">The DTO type that implements IEmptyDto</typeparam>
    /// <param name="validationTask">The async validation task</param>
    /// <param name="whenValid">Function to execute when validation passes</param>
    /// <returns>The result from either validation failure with T.Empty or the valid function</returns>
    public static async Task<ServiceResult<T>> ThenMatchAsync<T>(
        this Task<ServiceValidation<T>> validationTask,
        Func<Task<ServiceResult<T>>> whenValid)
        where T : class, IEmptyDto<T>
    {
        var validation = await validationTask;
        return await validation.MatchAsync(whenValid);
    }

    /// <summary>
    /// Continues a validation chain from an async operation and performs matching with explicit handlers.
    /// This allows keeping the fluent chain intact when using async validation operations.
    /// </summary>
    /// <typeparam name="T">The DTO type that implements IEmptyDto</typeparam>
    /// <param name="validationTask">The async validation task</param>
    /// <param name="whenValid">Function to execute when validation passes</param>
    /// <param name="whenInvalid">Function to execute when validation fails</param>
    /// <returns>The result from either the valid or invalid function</returns>
    public static async Task<ServiceResult<T>> ThenMatchAsync<T>(
        this Task<ServiceValidation<T>> validationTask,
        Func<Task<ServiceResult<T>>> whenValid,
        Func<IReadOnlyList<string>, ServiceResult<T>> whenInvalid)
        where T : class, IEmptyDto<T>
    {
        var validation = await validationTask;
        return await validation.Match(whenValid, whenInvalid);
    }

    /// <summary>
    /// Validates a ServiceResult and captures its data for use in subsequent operations.
    /// This eliminates the need for mutable state when capturing data from service calls.
    /// </summary>
    /// <typeparam name="T">The DTO type that implements IEmptyDto</typeparam>
    /// <typeparam name="TData">The type of data in the ServiceResult</typeparam>
    /// <param name="validation">The validation instance</param>
    /// <param name="serviceResultFunc">Function that returns a ServiceResult with data</param>
    /// <returns>A ServiceValidationWithData instance that carries the data</returns>
    public static async Task<ServiceValidationWithData<T, TData>> WithServiceResultAsync<T, TData>(
        this ServiceValidation<T> validation,
        Func<Task<ServiceResult<TData>>> serviceResultFunc)
        where T : class, IEmptyDto<T>
        where TData : class
    {
        // If validation already has errors, return with default data
        if (validation.HasErrors)
        {
            return new ServiceValidationWithData<T, TData>(validation, default);
        }

        var result = await serviceResultFunc();
        
        if (!result.IsSuccess)
        {
            // Add all the errors from the ServiceResult to our validation chain
            // Prefer structured errors if available
            if (result.StructuredErrors != null && result.StructuredErrors.Any())
            {
                foreach (var error in result.StructuredErrors)
                {
                    validation.Ensure(() => false, error);
                }
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    validation.Ensure(() => false, error);
                }
            }
            return new ServiceValidationWithData<T, TData>(validation, default);
        }
        
        return new ServiceValidationWithData<T, TData>(validation, result.Data);
    }

    /// <summary>
    /// Performs matching on a validation chain that carries data from a previous service call.
    /// This allows pattern matching on the data (e.g., empty vs non-empty) without mutable state.
    /// </summary>
    /// <typeparam name="T">The DTO type that implements IEmptyDto</typeparam>
    /// <typeparam name="TData">The type of data being carried</typeparam>
    /// <param name="validationWithDataTask">The async validation with data task</param>
    /// <param name="whenEmpty">Function to execute when data is empty/null</param>
    /// <param name="whenNotEmpty">Function to execute when data has value</param>
    /// <returns>The result from either the empty or not-empty function</returns>
    public static async Task<ServiceResult<T>> ThenMatchDataAsync<T, TData>(
        this Task<ServiceValidationWithData<T, TData>> validationWithDataTask,
        Func<Task<ServiceResult<T>>> whenEmpty,
        Func<TData, Task<ServiceResult<T>>> whenNotEmpty)
        where T : class, IEmptyDto<T>
        where TData : class, IEmptyDto<TData>
    {
        var validationWithData = await validationWithDataTask;
        
        // Check if validation has errors
        if (validationWithData.Validation.HasErrors)
        {
            return validationWithData.Validation.CreateFailureWithEmpty(T.Empty);
        }
        
        // Check if data is empty
        if (validationWithData.Data == null || validationWithData.Data.IsEmpty)
        {
            return await whenEmpty();
        }
        
        return await whenNotEmpty(validationWithData.Data);
    }
}

/// <summary>
/// Represents a validation state that carries data from a previous service call.
/// This allows passing data through the validation chain without mutable state.
/// </summary>
/// <typeparam name="T">The DTO type that implements IEmptyDto</typeparam>
/// <typeparam name="TData">The type of data being carried</typeparam>
public class ServiceValidationWithData<T, TData>
    where T : class, IEmptyDto<T>
    where TData : class
{
    public ServiceValidation<T> Validation { get; }
    public TData? Data { get; }

    public ServiceValidationWithData(ServiceValidation<T> validation, TData? data)
    {
        Validation = validation;
        Data = data;
    }
}