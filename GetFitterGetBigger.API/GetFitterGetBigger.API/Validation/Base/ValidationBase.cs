using System.Text.RegularExpressions;

namespace GetFitterGetBigger.API.Validation.Base;

/// <summary>
/// Base class for all fluent validation implementations providing common validation methods.
/// This abstraction eliminates code duplication across different validation layers.
/// </summary>
/// <typeparam name="TResult">The type of result this validation will produce</typeparam>
public abstract class ValidationBase<TResult>
{
    /// <summary>
    /// The list of accumulated validation errors
    /// </summary>
    protected readonly List<string> Errors = new();

    /// <summary>
    /// Gets whether the validation has any errors
    /// </summary>
    public bool HasErrors => Errors.Any();

    /// <summary>
    /// Gets the collection of validation errors
    /// </summary>
    public IReadOnlyList<string> ValidationErrors => Errors.AsReadOnly();

    /// <summary>
    /// Adds a validation rule with a custom predicate.
    /// </summary>
    /// <param name="predicate">The condition that must be true for validation to pass</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public ValidationBase<TResult> Ensure(Func<bool> predicate, string errorMessage)
    {
        if (!predicate())
            Errors.Add(errorMessage);
        return this;
    }

    /// <summary>
    /// Adds an async validation rule with a custom predicate.
    /// </summary>
    /// <param name="predicate">The async condition that must be true for validation to pass</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public async Task<ValidationBase<TResult>> EnsureAsync(Func<Task<bool>> predicate, string errorMessage)
    {
        if (!await predicate())
            Errors.Add(errorMessage);
        return this;
    }

    /// <summary>
    /// Validates that an object is not null.
    /// </summary>
    /// <param name="value">The object to validate</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public ValidationBase<TResult> EnsureNotNull(object? value, string errorMessage)
    {
        return Ensure(() => value != null, errorMessage);
    }

    /// <summary>
    /// Validates that a string value is not null or empty.
    /// </summary>
    /// <param name="value">The string value to validate</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public ValidationBase<TResult> EnsureNotEmpty(string? value, string errorMessage)
    {
        return Ensure(() => !string.IsNullOrEmpty(value), errorMessage);
    }

    /// <summary>
    /// Validates that a string value is not null, empty, or whitespace.
    /// </summary>
    /// <param name="value">The string value to validate</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public ValidationBase<TResult> EnsureNotWhiteSpace(string? value, string errorMessage)
    {
        return Ensure(() => !string.IsNullOrWhiteSpace(value), errorMessage);
    }

    /// <summary>
    /// Validates that a string value has a specific length.
    /// </summary>
    /// <param name="value">The string value to validate</param>
    /// <param name="minLength">The minimum allowed length (inclusive)</param>
    /// <param name="maxLength">The maximum allowed length (inclusive)</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public ValidationBase<TResult> EnsureLength(string? value, int minLength, int maxLength, string errorMessage)
    {
        return Ensure(() => value != null && value.Length >= minLength && value.Length <= maxLength, errorMessage);
    }

    /// <summary>
    /// Validates that a string value does not exceed a maximum length.
    /// </summary>
    /// <param name="value">The string value to validate</param>
    /// <param name="maxLength">The maximum allowed length</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public ValidationBase<TResult> EnsureMaxLength(string? value, int maxLength, string errorMessage)
    {
        return Ensure(() => value == null || value.Length <= maxLength, errorMessage);
    }

    /// <summary>
    /// Validates that a string value meets a minimum length requirement.
    /// </summary>
    /// <param name="value">The string value to validate</param>
    /// <param name="minLength">The minimum required length</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public ValidationBase<TResult> EnsureMinLength(string? value, int minLength, string errorMessage)
    {
        return Ensure(() => value != null && value.Length >= minLength, errorMessage);
    }

    /// <summary>
    /// Validates that a numeric value is within a specified range.
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="min">The minimum allowed value (inclusive)</param>
    /// <param name="max">The maximum allowed value (inclusive)</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public ValidationBase<TResult> EnsureRange(int value, int min, int max, string errorMessage)
    {
        return Ensure(() => value >= min && value <= max, errorMessage);
    }

    /// <summary>
    /// Validates that a numeric value is within a specified range.
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="min">The minimum allowed value (inclusive)</param>
    /// <param name="max">The maximum allowed value (inclusive)</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public ValidationBase<TResult> EnsureRange(decimal value, decimal min, decimal max, string errorMessage)
    {
        return Ensure(() => value >= min && value <= max, errorMessage);
    }

    /// <summary>
    /// Validates that a numeric value is greater than or equal to a minimum value.
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="min">The minimum allowed value (inclusive)</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public ValidationBase<TResult> EnsureMinValue(int value, int min, string errorMessage)
    {
        return Ensure(() => value >= min, errorMessage);
    }

    /// <summary>
    /// Validates that a numeric value is greater than or equal to a minimum value.
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="min">The minimum allowed value (inclusive)</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public ValidationBase<TResult> EnsureMinValue(decimal value, decimal min, string errorMessage)
    {
        return Ensure(() => value >= min, errorMessage);
    }

    /// <summary>
    /// Validates that a numeric value is less than or equal to a maximum value.
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="max">The maximum allowed value (inclusive)</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public ValidationBase<TResult> EnsureMaxValue(int value, int max, string errorMessage)
    {
        return Ensure(() => value <= max, errorMessage);
    }

    /// <summary>
    /// Validates that a numeric value is less than or equal to a maximum value.
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="max">The maximum allowed value (inclusive)</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public ValidationBase<TResult> EnsureMaxValue(decimal value, decimal max, string errorMessage)
    {
        return Ensure(() => value <= max, errorMessage);
    }

    /// <summary>
    /// Validates that a string matches a regular expression pattern.
    /// </summary>
    /// <param name="value">The string value to validate</param>
    /// <param name="pattern">The regular expression pattern</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public ValidationBase<TResult> EnsurePattern(string? value, string pattern, string errorMessage)
    {
        return Ensure(() => value != null && Regex.IsMatch(value, pattern), errorMessage);
    }

    /// <summary>
    /// Validates that a collection is not null or empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection</typeparam>
    /// <param name="collection">The collection to validate</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public ValidationBase<TResult> EnsureNotEmpty<T>(IEnumerable<T>? collection, string errorMessage)
    {
        return Ensure(() => collection != null && collection.Any(), errorMessage);
    }

    /// <summary>
    /// Validates that a value is contained within a collection of valid values.
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="value">The value to validate</param>
    /// <param name="validValues">The collection of valid values</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The current validation instance for chaining</returns>
    public ValidationBase<TResult> EnsureIn<T>(T value, IEnumerable<T> validValues, string errorMessage)
    {
        return Ensure(() => validValues?.Contains(value) ?? false, errorMessage);
    }

    /// <summary>
    /// Creates a successful result when validation passes.
    /// Must be implemented by derived classes.
    /// </summary>
    protected abstract TResult CreateSuccessResult();

    /// <summary>
    /// Creates a failure result with the accumulated validation errors.
    /// Must be implemented by derived classes.
    /// </summary>
    /// <param name="errors">The validation errors</param>
    protected abstract TResult CreateFailureResult(IReadOnlyList<string> errors);
}