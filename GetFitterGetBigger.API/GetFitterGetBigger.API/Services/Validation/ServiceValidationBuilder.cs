using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Validation;

/// <summary>
/// Provides a builder pattern for creating service validations with async operations.
/// This allows chaining of validation rules including async database checks.
/// </summary>
/// <typeparam name="T">The type of data the service operation will return</typeparam>
public class ServiceValidationBuilder<T>
{
    private readonly ServiceValidation<T> _validation;
    private readonly List<Func<Task<(bool IsValid, string? Error)>>> _asyncValidations = new();
    private readonly List<Func<Task<(bool IsValid, ServiceError? Error)>>> _asyncServiceErrorValidations = new();

    /// <summary>
    /// Initializes a new instance of the ServiceValidationBuilder class.
    /// </summary>
    /// <param name="validation">The underlying validation instance</param>
    internal ServiceValidationBuilder(ServiceValidation<T> validation)
    {
        _validation = validation;
    }

    /// <summary>
    /// Adds a synchronous validation rule.
    /// </summary>
    /// <param name="predicate">The condition that must be true for validation to pass</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The builder instance for chaining</returns>
    public ServiceValidationBuilder<T> Ensure(Func<bool> predicate, string errorMessage)
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
    public ServiceValidationBuilder<T> Ensure(Func<bool> predicate, ServiceError serviceError)
    {
        _validation.Ensure(predicate, serviceError);
        return this;
    }

    /// <summary>
    /// Validates that a string value is not null or whitespace.
    /// </summary>
    /// <param name="value">The string value to validate</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The builder instance for chaining</returns>
    public ServiceValidationBuilder<T> EnsureNotWhiteSpace(string? value, string errorMessage)
    {
        _validation.EnsureNotWhiteSpace(value, errorMessage);
        return this;
    }

    /// <summary>
    /// Adds an async validation that checks if an entity exists.
    /// </summary>
    /// <param name="existenceCheck">Function that returns true if entity exists</param>
    /// <param name="errorMessage">Error message if entity doesn't exist</param>
    /// <returns>The builder instance for chaining</returns>
    public ServiceValidationBuilder<T> EnsureExistsAsync(
        Func<Task<bool>> existenceCheck, 
        string errorMessage)
    {
        _asyncValidations.Add(async () => 
        {
            var exists = await existenceCheck();
            return (exists, exists ? null : errorMessage);
        });
        return this;
    }
    
    /// <summary>
    /// Adds an async validation that checks if an entity exists with a ServiceError.
    /// </summary>
    /// <param name="existenceCheck">Function that returns true if entity exists</param>
    /// <param name="serviceError">Service error if entity doesn't exist</param>
    /// <returns>The builder instance for chaining</returns>
    public ServiceValidationBuilder<T> EnsureExistsAsync(
        Func<Task<bool>> existenceCheck, 
        ServiceError serviceError)
    {
        _asyncServiceErrorValidations.Add(async () => 
        {
            var exists = await existenceCheck();
            return (exists, exists ? null : serviceError);
        });
        return this;
    }

    /// <summary>
    /// Adds an async validation with custom logic.
    /// </summary>
    /// <param name="validationFunc">Function that returns (IsValid, ErrorMessage)</param>
    /// <returns>The builder instance for chaining</returns>
    public ServiceValidationBuilder<T> EnsureAsync(
        Func<Task<(bool IsValid, string? Error)>> validationFunc)
    {
        _asyncValidations.Add(validationFunc);
        return this;
    }

    /// <summary>
    /// Adds an async validation that checks a condition.
    /// </summary>
    /// <param name="predicate">The async condition that must be true for validation to pass</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The builder instance for chaining</returns>
    public ServiceValidationBuilder<T> EnsureAsync(
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
    public ServiceValidationBuilder<T> EnsureAsync(
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
    /// Executes all validations and matches on the result.
    /// </summary>
    /// <param name="whenValid">Function to execute when all validations pass</param>
    /// <param name="whenInvalid">Function to execute when validation fails</param>
    /// <returns>The result from either the valid or invalid function</returns>
    public async Task<ServiceResult<T>> MatchAsync(
        Func<Task<ServiceResult<T>>> whenValid,
        Func<IReadOnlyList<string>, ServiceResult<T>> whenInvalid)
    {
        // First check synchronous validations
        if (_validation.HasErrors)
        {
            return whenInvalid(_validation.ValidationErrors);
        }

        // Then run async validations
        var asyncErrors = new List<string>();
        foreach (var asyncValidation in _asyncValidations)
        {
            var (isValid, error) = await asyncValidation();
            if (!isValid && error != null)
            {
                asyncErrors.Add(error);
            }
        }

        if (asyncErrors.Any())
        {
            return whenInvalid(asyncErrors);
        }

        return await whenValid();
    }
    
    /// <summary>
    /// Executes all validations and matches on the result with ServiceError support.
    /// </summary>
    /// <param name="whenValid">Function to execute when all validations pass</param>
    /// <param name="whenInvalid">Function to execute when validation fails</param>
    /// <returns>The result from either the valid or invalid function</returns>
    public async Task<ServiceResult<T>> MatchAsync(
        Func<Task<ServiceResult<T>>> whenValid,
        Func<IReadOnlyList<ServiceError>, ServiceResult<T>> whenInvalid)
    {
        // First check synchronous validations
        if (_validation.HasErrors)
        {
            // Convert string errors to ServiceError.ValidationFailed
            var syncServiceErrors = _validation.ValidationErrors
                .Select(msg => ServiceError.ValidationFailed(msg))
                .ToList();
            return whenInvalid(syncServiceErrors);
        }

        // Run async string validations
        var asyncStringErrors = new List<string>();
        foreach (var asyncValidation in _asyncValidations)
        {
            var (isValid, error) = await asyncValidation();
            if (!isValid && error != null)
            {
                asyncStringErrors.Add(error);
            }
        }

        // Run async ServiceError validations  
        var asyncServiceErrors = new List<ServiceError>();
        foreach (var asyncValidation in _asyncServiceErrorValidations)
        {
            var (isValid, error) = await asyncValidation();
            if (!isValid && error != null)
            {
                asyncServiceErrors.Add(error);
            }
        }

        // Combine all errors
        var allServiceErrors = asyncStringErrors
            .Select(msg => ServiceError.ValidationFailed(msg))
            .Concat(asyncServiceErrors)
            .ToList();

        if (allServiceErrors.Any())
        {
            return whenInvalid(allServiceErrors);
        }

        return await whenValid();
    }
    
    /// <summary>
    /// Converts the validation result to a ValidationResult after executing all async validations
    /// </summary>
    /// <returns>A ValidationResult containing any validation errors</returns>
    public async Task<ValidationResult> ToValidationResultAsync()
    {
        // First check synchronous validations
        if (_validation.HasErrors)
        {
            return new ValidationResult { Errors = _validation.ValidationErrors.ToList() };
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
        foreach (var asyncValidation in _asyncServiceErrorValidations)
        {
            var (isValid, error) = await asyncValidation();
            if (!isValid && error != null)
            {
                errors.Add(error.Message);
            }
        }

        return new ValidationResult { Errors = errors };
    }
}