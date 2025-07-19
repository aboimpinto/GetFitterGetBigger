using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Validation;

/// <summary>
/// Provides a fluent validation chain for service operations that integrates with ValidationResult.
/// Accumulates validation errors and returns either a success or a failure with all errors.
/// </summary>
public class ServiceValidation
{
    private readonly List<string> _errors = new();
    private ServiceError? _serviceError = null;

    /// <summary>
    /// Adds a validation rule with a custom predicate.
    /// </summary>
    /// <param name="predicate">The condition that must be true for validation to pass</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public ServiceValidation Ensure(Func<bool> predicate, string errorMessage)
    {
        if (!predicate())
            _errors.Add(errorMessage);
        return this;
    }

    /// <summary>
    /// Adds a validation rule with a custom predicate that sets a ServiceError.
    /// </summary>
    /// <param name="predicate">The condition that must be true for validation to pass</param>
    /// <param name="serviceError">The service error if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public ServiceValidation Ensure(Func<bool> predicate, ServiceError serviceError)
    {
        if (!predicate())
        {
            _serviceError = serviceError;
            _errors.Add(serviceError.Message);
        }
        return this;
    }

    /// <summary>
    /// Adds an async validation rule with a custom predicate.
    /// </summary>
    /// <param name="predicate">The async condition that must be true for validation to pass</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public async Task<ServiceValidation> EnsureAsync(Func<Task<bool>> predicate, string errorMessage)
    {
        if (!await predicate())
            _errors.Add(errorMessage);
        return this;
    }

    /// <summary>
    /// Adds an async validation rule with a custom predicate that sets a ServiceError.
    /// </summary>
    /// <param name="predicate">The async condition that must be true for validation to pass</param>
    /// <param name="serviceError">The service error if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public async Task<ServiceValidation> EnsureAsync(Func<Task<bool>> predicate, ServiceError serviceError)
    {
        if (!await predicate())
        {
            _serviceError = serviceError;
            _errors.Add(serviceError.Message);
        }
        return this;
    }

    /// <summary>
    /// Validates that an object is not null.
    /// </summary>
    /// <param name="value">The object to validate</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public ServiceValidation EnsureNotNull(object? value, string errorMessage)
    {
        return Ensure(() => value != null, errorMessage);
    }

    /// <summary>
    /// Validates that a string value is not null or whitespace.
    /// </summary>
    /// <param name="value">The string value to validate</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public ServiceValidation EnsureNotWhiteSpace(string? value, string errorMessage)
    {
        return Ensure(() => !string.IsNullOrWhiteSpace(value), errorMessage);
    }

    /// <summary>
    /// Completes the validation chain and returns a ValidationResult.
    /// If validation passed, returns success.
    /// If validation failed, returns a failure result with all accumulated errors.
    /// </summary>
    /// <returns>A ValidationResult containing either success or validation errors</returns>
    public ValidationResult ToResult()
    {
        if (!_errors.Any())
            return ValidationResult.Success();
            
        return _serviceError != null
            ? ValidationResult.Failure(_serviceError)
            : ValidationResult.Failure(_errors.ToArray());
    }
}