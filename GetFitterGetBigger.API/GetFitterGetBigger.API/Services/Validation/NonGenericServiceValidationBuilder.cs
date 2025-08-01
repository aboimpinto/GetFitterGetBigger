using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Validation;

/// <summary>
/// Provides a builder pattern for creating service validations with async operations for ValidationResult.
/// This non-generic version is used when returning ValidationResult instead of ServiceResult.
/// </summary>
public class NonGenericServiceValidationBuilder
{
    private readonly ServiceValidation _validation;
    private readonly List<Func<Task<(bool IsValid, string? Error)>>> _asyncValidations = new();
    private readonly List<Func<Task<(bool IsValid, ServiceError? Error)>>> _asyncServiceErrorValidations = new();

    /// <summary>
    /// Initializes a new instance of the NonGenericServiceValidationBuilder class.
    /// </summary>
    /// <param name="validation">The underlying validation instance</param>
    internal NonGenericServiceValidationBuilder(ServiceValidation validation)
    {
        _validation = validation;
    }

    /// <summary>
    /// Adds a synchronous validation rule.
    /// </summary>
    /// <param name="predicate">The condition that must be true for validation to pass</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The builder instance for chaining</returns>
    public NonGenericServiceValidationBuilder Ensure(Func<bool> predicate, string errorMessage)
    {
        _validation.Ensure(predicate, errorMessage);
        return this;
    }

    /// <summary>
    /// Adds a synchronous validation rule with a ServiceError.
    /// </summary>
    /// <param name="predicate">The condition that must be true for validation to pass</param>
    /// <param name="serviceError">The service error if validation fails</param>
    /// <returns>The builder instance for chaining</returns>
    public NonGenericServiceValidationBuilder Ensure(Func<bool> predicate, ServiceError serviceError)
    {
        _validation.Ensure(predicate, serviceError);
        return this;
    }

    /// <summary>
    /// Validates that an object is not null.
    /// </summary>
    /// <param name="value">The object to validate</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The builder instance for chaining</returns>
    public NonGenericServiceValidationBuilder EnsureNotNull(object? value, string errorMessage)
    {
        _validation.EnsureNotNull(value, errorMessage);
        return this;
    }

    /// <summary>
    /// Validates that a string value is not null or whitespace.
    /// </summary>
    /// <param name="value">The string value to validate</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The builder instance for chaining</returns>
    public NonGenericServiceValidationBuilder EnsureNotWhiteSpace(string? value, string errorMessage)
    {
        _validation.EnsureNotWhiteSpace(value, errorMessage);
        return this;
    }

    /// <summary>
    /// Adds an async validation that checks a condition.
    /// </summary>
    /// <param name="predicate">The async condition that must be true for validation to pass</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The builder instance for chaining</returns>
    public NonGenericServiceValidationBuilder EnsureAsync(
        Func<Task<bool>> predicate, 
        string errorMessage)
    {
        _asyncValidations.Add(async () => 
        {
            var isValid = await predicate();
            return (isValid, isValid ? null : errorMessage);
        });
        return this;
    }
    
    /// <summary>
    /// Adds an async validation that checks a condition with a ServiceError.
    /// </summary>
    /// <param name="predicate">The async condition that must be true for validation to pass</param>
    /// <param name="serviceError">The service error if validation fails</param>
    /// <returns>The builder instance for chaining</returns>
    public NonGenericServiceValidationBuilder EnsureAsync(
        Func<Task<bool>> predicate, 
        ServiceError serviceError)
    {
        _asyncServiceErrorValidations.Add(async () => 
        {
            var isValid = await predicate();
            return (isValid, isValid ? null : serviceError);
        });
        return this;
    }

    /// <summary>
    /// Converts the validation result to a ValidationResult after executing all async validations
    /// </summary>
    /// <returns>A ValidationResult containing any validation errors</returns>
    public async Task<ValidationResult> ToValidationResultAsync()
    {
        // First check synchronous validations
        var syncResult = _validation.ToResult();
        if (!syncResult.IsValid)
        {
            return syncResult;
        }

        // Then run async validations
        var errors = new List<string>();
        foreach (var asyncValidation in _asyncValidations)
        {
            var (isValid, error) = await asyncValidation();
            if (!isValid && error != null)
            {
                errors.Add(error);
            }
        }

        // Run async ServiceError validations  
        ServiceError? serviceError = null;
        foreach (var asyncValidation in _asyncServiceErrorValidations)
        {
            var (isValid, error) = await asyncValidation();
            if (!isValid && error != null)
            {
                errors.Add(error.Message);
                serviceError ??= error; // Keep the first ServiceError
            }
        }

        if (errors.Any())
        {
            return serviceError != null 
                ? ValidationResult.Failure(serviceError)
                : ValidationResult.Failure(errors.ToArray());
        }
        
        return ValidationResult.Success();
    }
}