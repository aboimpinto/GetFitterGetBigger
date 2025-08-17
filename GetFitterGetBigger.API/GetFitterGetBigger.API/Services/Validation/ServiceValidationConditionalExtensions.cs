using GetFitterGetBigger.API.Models.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Validation.Common;

namespace GetFitterGetBigger.API.Services.Validation;

/// <summary>
/// Extension methods for conditional validation that only executes if previous validations pass.
/// These "Then" methods ensure that validation only proceeds if the chain is still valid.
/// </summary>
public static class ServiceValidationConditionalExtensions
{
    /// <summary>
    /// Validates that a string value is not null or whitespace ONLY if the validation chain is still valid.
    /// This avoids null reference exceptions when the previous validation failed.
    /// </summary>
    /// <param name="validation">The validation instance</param>
    /// <param name="value">The string value to validate</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public static ServiceValidation<T> ThenEnsureNotWhiteSpace<T>(
        this ServiceValidation<T> validation,
        string? value,
        string errorMessage)
    {
        // Only validate if no errors so far
        if (!validation.HasErrors)
        {
            validation.EnsureNotWhiteSpace(value, errorMessage);
        }
        return validation;
    }

    /// <summary>
    /// Validates that a string value is not null or whitespace ONLY if the validation chain is still valid.
    /// This avoids null reference exceptions when the previous validation failed.
    /// </summary>
    /// <param name="validation">The validation instance</param>
    /// <param name="value">The string value to validate</param>
    /// <param name="serviceError">The service error if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public static ServiceValidation<T> ThenEnsureNotWhiteSpace<T>(
        this ServiceValidation<T> validation,
        string? value,
        ServiceError serviceError)
    {
        // Only validate if no errors so far
        if (!validation.HasErrors)
        {
            validation.EnsureNotWhiteSpace(value, serviceError);
        }
        return validation;
    }

    /// <summary>
    /// Validates that an email format is valid ONLY if the validation chain is still valid.
    /// This should be used after ensuring the email is not empty to avoid validating null values.
    /// </summary>
    /// <param name="validation">The validation instance</param>
    /// <param name="email">The email to validate</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public static ServiceValidation<T> ThenEnsureEmailIsValid<T>(
        this ServiceValidation<T> validation,
        string email,
        string errorMessage)
    {
        // Only validate if no errors so far
        if (!validation.HasErrors)
        {
            validation.Ensure(
                () => CommonValidations.IsValidEmail(email),
                ServiceError.ValidationFailed(errorMessage));
        }
        return validation;
    }

    /// <summary>
    /// Validates that an email format is valid ONLY if the validation chain is still valid.
    /// This should be used after ensuring the email is not empty to avoid validating null values.
    /// </summary>
    /// <param name="validation">The validation instance</param>
    /// <param name="email">The email to validate</param>
    /// <param name="serviceError">The service error if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public static ServiceValidation<T> ThenEnsureEmailIsValid<T>(
        this ServiceValidation<T> validation,
        string email,
        ServiceError serviceError)
    {
        // Only validate if no errors so far
        if (!validation.HasErrors)
        {
            validation.Ensure(
                () => CommonValidations.IsValidEmail(email),
                serviceError);
        }
        return validation;
    }

    /// <summary>
    /// Validates a condition ONLY if the validation chain is still valid.
    /// This is the generic version for any custom validation.
    /// </summary>
    /// <param name="validation">The validation instance</param>
    /// <param name="predicate">The condition that must be true for validation to pass</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public static ServiceValidation<T> ThenEnsure<T>(
        this ServiceValidation<T> validation,
        Func<bool> predicate,
        string errorMessage)
    {
        // Only validate if no errors so far
        if (!validation.HasErrors)
        {
            validation.Ensure(predicate, errorMessage);
        }
        return validation;
    }

    /// <summary>
    /// Validates a condition ONLY if the validation chain is still valid.
    /// This is the generic version for any custom validation.
    /// </summary>
    /// <param name="validation">The validation instance</param>
    /// <param name="predicate">The condition that must be true for validation to pass</param>
    /// <param name="serviceError">The service error if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public static ServiceValidation<T> ThenEnsure<T>(
        this ServiceValidation<T> validation,
        Func<bool> predicate,
        ServiceError serviceError)
    {
        // Only validate if no errors so far
        if (!validation.HasErrors)
        {
            validation.Ensure(predicate, serviceError);
        }
        return validation;
    }

    /// <summary>
    /// Validates an async condition ONLY if the validation chain is still valid.
    /// This is the generic async version for any custom validation.
    /// </summary>
    /// <param name="validation">The validation instance</param>
    /// <param name="predicate">The async condition that must be true for validation to pass</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public static async Task<ServiceValidation<T>> ThenEnsureAsync<T>(
        this ServiceValidation<T> validation,
        Func<Task<bool>> predicate,
        string errorMessage)
    {
        // Only validate if no errors so far
        if (!validation.HasErrors)
        {
            await validation.EnsureAsync(predicate, errorMessage);
        }
        return validation;
    }

    /// <summary>
    /// Validates an async condition ONLY if the validation chain is still valid.
    /// This is the generic async version for any custom validation.
    /// </summary>
    /// <param name="validation">The validation instance</param>
    /// <param name="predicate">The async condition that must be true for validation to pass</param>
    /// <param name="serviceError">The service error if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public static async Task<ServiceValidation<T>> ThenEnsureAsync<T>(
        this ServiceValidation<T> validation,
        Func<Task<bool>> predicate,
        ServiceError serviceError)
    {
        // Only validate if no errors so far
        if (!validation.HasErrors)
        {
            await validation.EnsureAsync(predicate, serviceError);
        }
        return validation;
    }

    /// <summary>
    /// Validates that a specialized ID is not empty ONLY if the validation chain is still valid.
    /// </summary>
    /// <param name="validation">The validation instance</param>
    /// <param name="id">The specialized ID to validate</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public static ServiceValidation<T> ThenEnsureNotEmpty<T>(
        this ServiceValidation<T> validation,
        ISpecializedIdBase id,
        string errorMessage)
    {
        // Only validate if no errors so far
        if (!validation.HasErrors)
        {
            validation.EnsureNotEmpty(id, errorMessage);
        }
        return validation;
    }

    /// <summary>
    /// Validates that a specialized ID is not empty ONLY if the validation chain is still valid.
    /// </summary>
    /// <param name="validation">The validation instance</param>
    /// <param name="id">The specialized ID to validate</param>
    /// <param name="serviceError">The service error if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public static ServiceValidation<T> ThenEnsureNotEmpty<T>(
        this ServiceValidation<T> validation,
        ISpecializedIdBase id,
        ServiceError serviceError)
    {
        // Only validate if no errors so far
        if (!validation.HasErrors)
        {
            validation.EnsureNotEmpty(id, serviceError);
        }
        return validation;
    }
}