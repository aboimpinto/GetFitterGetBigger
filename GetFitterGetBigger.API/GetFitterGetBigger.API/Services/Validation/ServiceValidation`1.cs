using GetFitterGetBigger.API.DTOs.Interfaces;
using GetFitterGetBigger.API.Models.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Validation.Base;

namespace GetFitterGetBigger.API.Services.Validation;

/// <summary>
/// Provides a fluent validation chain for service operations that integrates with ServiceResult&lt;T&gt;.
/// Accumulates validation errors and returns either a success with the result or a failure with all errors.
/// </summary>
/// <typeparam name="T">The type of data the service operation will return</typeparam>
public class ServiceValidation<T> : ValidationBase<ServiceResult<T>>
{
    private ServiceError? _serviceError = null;

    /// <summary>
    /// Gets whether the validation has a ServiceError set.
    /// </summary>
    internal bool HasServiceError => _serviceError != null;

    /// <summary>
    /// Creates a failure result with the appropriate error (ServiceError if set, otherwise string errors).
    /// </summary>
    /// <param name="emptyValue">The empty value to return</param>
    /// <returns>A failure ServiceResult with the appropriate errors</returns>
    internal ServiceResult<T> CreateFailureWithEmpty(T emptyValue)
    {
        return _serviceError != null
            ? ServiceResult<T>.Failure(emptyValue, _serviceError)
            : ServiceResult<T>.Failure(emptyValue, ValidationErrors.ToArray());
    }

    /// <summary>
    /// Adds a validation rule with a custom predicate.
    /// </summary>
    /// <param name="predicate">The condition that must be true for validation to pass</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public new ServiceValidation<T> Ensure(Func<bool> predicate, string errorMessage)
    {
        base.Ensure(predicate, errorMessage);
        return this;
    }

    /// <summary>
    /// Adds a validation rule with a custom predicate that sets a ServiceError.
    /// </summary>
    /// <param name="predicate">The condition that must be true for validation to pass</param>
    /// <param name="serviceError">The service error if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public ServiceValidation<T> Ensure(Func<bool> predicate, ServiceError serviceError)
    {
        if (!predicate())
        {
            _serviceError = serviceError;
            Errors.Add(serviceError.Message);
        }
        return this;
    }

    /// <summary>
    /// Adds an async validation rule with a custom predicate.
    /// </summary>
    /// <param name="predicate">The async condition that must be true for validation to pass</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public new async Task<ServiceValidation<T>> EnsureAsync(Func<Task<bool>> predicate, string errorMessage)
    {
        await base.EnsureAsync(predicate, errorMessage);
        return this;
    }

    /// <summary>
    /// Adds an async validation rule with a custom predicate that sets a ServiceError.
    /// </summary>
    /// <param name="predicate">The async condition that must be true for validation to pass</param>
    /// <param name="serviceError">The service error if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public async Task<ServiceValidation<T>> EnsureAsync(Func<Task<bool>> predicate, ServiceError serviceError)
    {
        if (!await predicate())
        {
            _serviceError = serviceError;
            Errors.Add(serviceError.Message);
        }
        return this;
    }

    /// <summary>
    /// Validates that an object is not null.
    /// </summary>
    /// <param name="value">The object to validate</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public new ServiceValidation<T> EnsureNotNull(object? value, string errorMessage)
    {
        base.EnsureNotNull(value, errorMessage);
        return this;
    }

    /// <summary>
    /// Validates that an object is not null with a ServiceError.
    /// </summary>
    /// <param name="value">The object to validate</param>
    /// <param name="serviceError">The service error if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public ServiceValidation<T> EnsureNotNull(object? value, ServiceError serviceError)
    {
        return Ensure(() => value != null, serviceError);
    }

    /// <summary>
    /// Validates that a string value is not null or whitespace.
    /// Creates a ServiceError with ValidationFailed code using the provided error message.
    /// </summary>
    /// <param name="value">The string value to validate</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public new ServiceValidation<T> EnsureNotWhiteSpace(string? value, string errorMessage)
    {
        // For ServiceValidate.For<T>(), we use ValidationFailed to maintain backward compatibility
        return Ensure(() => !string.IsNullOrWhiteSpace(value), ServiceError.ValidationFailed(errorMessage));
    }

    /// <summary>
    /// Validates that a string value is not null or whitespace with a ServiceError.
    /// </summary>
    /// <param name="value">The string value to validate</param>
    /// <param name="serviceError">The service error if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public ServiceValidation<T> EnsureNotWhiteSpace(string? value, ServiceError serviceError)
    {
        return Ensure(() => !string.IsNullOrWhiteSpace(value), serviceError);
    }

    /// <summary>
    /// Validates that a specialized ID is not empty.
    /// Creates a ServiceError with ValidationFailed code using the provided error message.
    /// </summary>
    /// <param name="id">The specialized ID to validate</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public ServiceValidation<T> EnsureNotEmpty(ISpecializedIdBase id, string errorMessage)
    {
        // For ServiceValidate.For<T>(), we use ValidationFailed to maintain backward compatibility
        return Ensure(() => !id.IsEmpty, ServiceError.ValidationFailed(errorMessage));
    }

    /// <summary>
    /// Validates that a specialized ID is not empty with a ServiceError.
    /// </summary>
    /// <param name="id">The specialized ID to validate</param>
    /// <param name="serviceError">The service error if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public ServiceValidation<T> EnsureNotEmpty(ISpecializedIdBase id, ServiceError serviceError)
    {
        return Ensure(() => !id.IsEmpty, serviceError);
    }

    /// <summary>
    /// Completes the validation chain and returns a ServiceResult.
    /// If validation passed, creates the result using the provided factory function.
    /// If validation failed, returns a failure result with all accumulated errors.
    /// </summary>
    /// <param name="factory">The factory function to create the result if validation passes</param>
    /// <returns>A ServiceResult containing either the created result or validation errors</returns>
    public ServiceResult<T> OnSuccess(Func<T> factory)
    {
        return HasErrors 
            ? CreateFailureResult(ValidationErrors)
            : ServiceResult<T>.Success(factory());
    }

    /// <summary>
    /// Completes the validation chain and returns a ServiceResult asynchronously.
    /// If validation passed, creates the result using the provided async factory function.
    /// If validation failed, returns a failure result with all accumulated errors.
    /// </summary>
    /// <param name="factory">The async factory function to create the result if validation passes</param>
    /// <returns>A ServiceResult containing either the created result or validation errors</returns>
    public async Task<ServiceResult<T>> OnSuccessAsync(Func<Task<T>> factory)
    {
        return HasErrors 
            ? CreateFailureResult(ValidationErrors)
            : ServiceResult<T>.Success(await factory());
    }

    /// <summary>
    /// Returns a failure result with the provided empty value.
    /// Useful when you want to fail fast after validation with a specific empty value.
    /// </summary>
    /// <param name="emptyValue">The empty value to return in the failure result</param>
    /// <returns>A ServiceResult containing the validation errors and empty value</returns>
    public ServiceResult<T> OnFailure(T emptyValue)
    {
        return CreateFailureResult(ValidationErrors, emptyValue);
    }

    /// <summary>
    /// Matches on the validation result and executes the appropriate function.
    /// This provides explicit handling of both success and failure cases.
    /// </summary>
    /// <param name="whenValid">Function to execute when validation passes</param>
    /// <param name="whenInvalid">Function to execute when validation fails, receives the validation errors</param>
    /// <returns>The result from either the valid or invalid function</returns>
    public async Task<ServiceResult<T>> Match(
        Func<Task<ServiceResult<T>>> whenValid,
        Func<IReadOnlyList<string>, ServiceResult<T>> whenInvalid)
    {
        return HasErrors 
            ? whenInvalid(ValidationErrors)
            : await whenValid();
    }

    /// <summary>
    /// Matches on the validation result with synchronous handlers.
    /// This provides explicit handling of both success and failure cases.
    /// </summary>
    /// <param name="whenValid">Function to execute when validation passes</param>
    /// <param name="whenInvalid">Function to execute when validation fails, receives the validation errors</param>
    /// <returns>The result from either the valid or invalid function</returns>
    public ServiceResult<T> Match(
        Func<ServiceResult<T>> whenValid,
        Func<IReadOnlyList<string>, ServiceResult<T>> whenInvalid)
    {
        return HasErrors 
            ? whenInvalid(ValidationErrors)
            : whenValid();
    }


    /// <summary>
    /// Creates a successful result. This method is not supported for ServiceValidation.
    /// Use OnSuccess(factory) method instead.
    /// </summary>
    protected override ServiceResult<T> CreateSuccessResult()
    {
        throw new NotSupportedException("Use OnSuccess(factory) method instead.");
    }

    /// <summary>
    /// Creates a failure result with the accumulated validation errors.
    /// </summary>
    /// <param name="errors">The validation errors</param>
    protected override ServiceResult<T> CreateFailureResult(IReadOnlyList<string> errors)
    {
        throw new NotSupportedException("Use OnFailure(emptyValue) method instead.");
    }

    /// <summary>
    /// Creates a failure result with the accumulated validation errors and empty value.
    /// </summary>
    /// <param name="errors">The validation errors</param>
    /// <param name="emptyValue">The empty value to return</param>
    private ServiceResult<T> CreateFailureResult(IReadOnlyList<string> errors, T emptyValue)
    {
        if (_serviceError != null)
        {
            return ServiceResult<T>.Failure(emptyValue, _serviceError);
        }
        
        return ServiceResult<T>.Failure(emptyValue, errors.ToArray());
    }
}