using System;
using System.Collections.Generic;
using System.Linq;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.Results;
using GetFitterGetBigger.API.Validation.Base;

namespace GetFitterGetBigger.API.Models.Validation;

/// <summary>
/// Provides a fluent validation chain for entity creation that integrates with EntityResult.
/// Accumulates validation errors and returns either a success with the created entity or a failure with all errors.
/// </summary>
/// <typeparam name="T">The type of entity being validated (must implement IEmptyEntity)</typeparam>
public class EntityValidation<T> : ValidationBase<EntityResult<T>> where T : class, IEmptyEntity<T>
{
    /// <summary>
    /// Adds a validation rule with a custom predicate.
    /// </summary>
    /// <param name="predicate">The condition that must be true for validation to pass</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public new EntityValidation<T> Ensure(Func<bool> predicate, string errorMessage)
    {
        base.Ensure(predicate, errorMessage);
        return this;
    }

    /// <summary>
    /// Validates that a string value is not null or empty.
    /// </summary>
    /// <param name="value">The string value to validate</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public new EntityValidation<T> EnsureNotEmpty(string? value, string errorMessage)
    {
        base.EnsureNotEmpty(value, errorMessage);
        return this;
    }

    /// <summary>
    /// Validates that a string value is not null, empty, or whitespace.
    /// </summary>
    /// <param name="value">The string value to validate</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public new EntityValidation<T> EnsureNotWhiteSpace(string? value, string errorMessage)
    {
        base.EnsureNotWhiteSpace(value, errorMessage);
        return this;
    }

    /// <summary>
    /// Validates that an integer value is greater than or equal to a minimum value.
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="min">The minimum allowed value (inclusive)</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public new EntityValidation<T> EnsureMinValue(int value, int min, string errorMessage)
    {
        base.EnsureMinValue(value, min, errorMessage);
        return this;
    }

    /// <summary>
    /// Validates that an integer value is less than or equal to a maximum value.
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="max">The maximum allowed value (inclusive)</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public new EntityValidation<T> EnsureMaxValue(int value, int max, string errorMessage)
    {
        base.EnsureMaxValue(value, max, errorMessage);
        return this;
    }

    /// <summary>
    /// Validates that an integer value is within a specified range.
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="min">The minimum allowed value (inclusive)</param>
    /// <param name="max">The maximum allowed value (inclusive)</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public new EntityValidation<T> EnsureRange(int value, int min, int max, string errorMessage)
    {
        base.EnsureRange(value, min, max, errorMessage);
        return this;
    }

    /// <summary>
    /// Validates that a string value does not exceed a maximum length.
    /// </summary>
    /// <param name="value">The string value to validate</param>
    /// <param name="maxLength">The maximum allowed length</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public new EntityValidation<T> EnsureMaxLength(string? value, int maxLength, string errorMessage)
    {
        base.EnsureMaxLength(value, maxLength, errorMessage);
        return this;
    }

    /// <summary>
    /// Validates that a string value meets a minimum length requirement.
    /// </summary>
    /// <param name="value">The string value to validate</param>
    /// <param name="minLength">The minimum required length</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public new EntityValidation<T> EnsureMinLength(string? value, int minLength, string errorMessage)
    {
        base.EnsureMinLength(value, minLength, errorMessage);
        return this;
    }

    /// <summary>
    /// Validates that a string value has a specific length.
    /// </summary>
    /// <param name="value">The string value to validate</param>
    /// <param name="minLength">The minimum allowed length (inclusive)</param>
    /// <param name="maxLength">The maximum allowed length (inclusive)</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public new EntityValidation<T> EnsureLength(string? value, int minLength, int maxLength, string errorMessage)
    {
        base.EnsureLength(value, minLength, maxLength, errorMessage);
        return this;
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
        return HasErrors 
            ? CreateFailureResult(ValidationErrors)
            : CreateSuccessResult(factory);
    }

    /// <summary>
    /// Returns a failure result without executing any factory function.
    /// Useful when you want to fail fast after validation.
    /// </summary>
    /// <returns>An EntityResult containing the validation errors</returns>
    public EntityResult<T> OnFailure()
    {
        return CreateFailureResult(ValidationErrors);
    }

    /// <summary>
    /// Creates a successful result with the entity created by the factory.
    /// </summary>
    protected override EntityResult<T> CreateSuccessResult()
    {
        throw new NotSupportedException("Use OnSuccess(factory) method instead.");
    }

    /// <summary>
    /// Creates a successful result with the entity created by the factory.
    /// </summary>
    private EntityResult<T> CreateSuccessResult(Func<T> factory)
    {
        return EntityResult<T>.Success(factory());
    }

    /// <summary>
    /// Creates a failure result with the accumulated validation errors.
    /// </summary>
    /// <param name="errors">The validation errors</param>
    protected override EntityResult<T> CreateFailureResult(IReadOnlyList<string> errors)
    {
        return EntityResult<T>.Failure(errors.ToArray());
    }
}