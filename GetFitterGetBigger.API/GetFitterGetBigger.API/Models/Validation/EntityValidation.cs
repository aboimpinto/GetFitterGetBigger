using System;
using System.Collections.Generic;
using System.Linq;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.Results;

namespace GetFitterGetBigger.API.Models.Validation;

/// <summary>
/// Provides a fluent validation chain for entity creation that integrates with EntityResult.
/// Accumulates validation errors and returns either a success with the created entity or a failure with all errors.
/// </summary>
/// <typeparam name="T">The type of entity being validated (must implement IEmptyEntity)</typeparam>
public class EntityValidation<T> where T : class, IEmptyEntity<T>
{
    private readonly List<string> _errors = new();

    /// <summary>
    /// Adds a validation rule with a custom predicate.
    /// </summary>
    /// <param name="predicate">The condition that must be true for validation to pass</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public EntityValidation<T> Ensure(Func<bool> predicate, string errorMessage)
    {
        if (!predicate())
            _errors.Add(errorMessage);
        return this;
    }

    /// <summary>
    /// Validates that a string value is not null or empty.
    /// </summary>
    /// <param name="value">The string value to validate</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public EntityValidation<T> EnsureNotEmpty(string value, string errorMessage)
    {
        return Ensure(() => !string.IsNullOrEmpty(value), errorMessage);
    }

    /// <summary>
    /// Validates that a string value is not null, empty, or whitespace.
    /// </summary>
    /// <param name="value">The string value to validate</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public EntityValidation<T> EnsureNotWhiteSpace(string value, string errorMessage)
    {
        return Ensure(() => !string.IsNullOrWhiteSpace(value), errorMessage);
    }

    /// <summary>
    /// Validates that an integer value is greater than or equal to a minimum value.
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="min">The minimum allowed value (inclusive)</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public EntityValidation<T> EnsureMinValue(int value, int min, string errorMessage)
    {
        return Ensure(() => value >= min, errorMessage);
    }

    /// <summary>
    /// Validates that an integer value is less than or equal to a maximum value.
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="max">The maximum allowed value (inclusive)</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public EntityValidation<T> EnsureMaxValue(int value, int max, string errorMessage)
    {
        return Ensure(() => value <= max, errorMessage);
    }

    /// <summary>
    /// Validates that an integer value is within a specified range.
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="min">The minimum allowed value (inclusive)</param>
    /// <param name="max">The maximum allowed value (inclusive)</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public EntityValidation<T> EnsureRange(int value, int min, int max, string errorMessage)
    {
        return Ensure(() => value >= min && value <= max, errorMessage);
    }

    /// <summary>
    /// Validates that a string value does not exceed a maximum length.
    /// </summary>
    /// <param name="value">The string value to validate</param>
    /// <param name="maxLength">The maximum allowed length</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public EntityValidation<T> EnsureMaxLength(string value, int maxLength, string errorMessage)
    {
        return Ensure(() => value?.Length <= maxLength, errorMessage);
    }

    /// <summary>
    /// Validates that a string value meets a minimum length requirement.
    /// </summary>
    /// <param name="value">The string value to validate</param>
    /// <param name="minLength">The minimum required length</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public EntityValidation<T> EnsureMinLength(string value, int minLength, string errorMessage)
    {
        return Ensure(() => value?.Length >= minLength, errorMessage);
    }

    /// <summary>
    /// Completes the validation chain and returns an EntityResult.
    /// If validation passed, creates the entity using the provided factory function.
    /// If validation failed, returns a failure result with all accumulated errors.
    /// </summary>
    /// <param name="factory">The factory function to create the entity if validation passes</param>
    /// <returns>An EntityResult containing either the created entity or validation errors</returns>
    public EntityResult<T> OnSuccess(Func<T> factory)
    {
        return _errors.Any() 
            ? EntityResult<T>.Failure(_errors.ToArray())
            : EntityResult<T>.Success(factory());
    }
}