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
        var syncResult = CheckSyncValidations();
        if (!syncResult.IsValid)
        {
            return syncResult;
        }

        // Run all async validations
        var asyncResult = await ProcessAllAsyncValidationsAsync();
        
        return asyncResult.HasErrors 
            ? CreateFailureResult(asyncResult)
            : ValidationResult.Success();
    }

    /// <summary>
    /// Checks synchronous validations
    /// </summary>
    /// <returns>ValidationResult from sync validations</returns>
    private ValidationResult CheckSyncValidations()
    {
        return _validation.ToResult();
    }

    /// <summary>
    /// Processes all async validations and returns combined results
    /// </summary>
    /// <returns>Combined async validation results</returns>
    private async Task<NonGenericAsyncValidationResult> ProcessAllAsyncValidationsAsync()
    {
        var stringErrors = await ProcessStringValidationsAsync();
        var serviceErrorData = await ProcessServiceErrorValidationsAsync();
        
        return new NonGenericAsyncValidationResult
        {
            StringErrors = stringErrors,
            ServiceErrorMessages = serviceErrorData.Messages,
            FirstServiceError = serviceErrorData.FirstError
        };
    }

    /// <summary>
    /// Processes async string validations
    /// </summary>
    /// <returns>List of string errors</returns>
    private async Task<List<string>> ProcessStringValidationsAsync()
    {
        var errors = new List<string>();
        
        foreach (var asyncValidation in _asyncValidations)
        {
            var (isValid, error) = await asyncValidation();
            if (!isValid && error != null)
            {
                errors.Add(error);
            }
        }
        
        return errors;
    }

    /// <summary>
    /// Processes async ServiceError validations
    /// </summary>
    /// <returns>ServiceError validation data</returns>
    private async Task<ServiceErrorValidationData> ProcessServiceErrorValidationsAsync()
    {
        var messages = new List<string>();
        ServiceError? firstError = null;
        
        foreach (var asyncValidation in _asyncServiceErrorValidations)
        {
            var (isValid, error) = await asyncValidation();
            if (!isValid && error != null)
            {
                messages.Add(error.Message);
                firstError ??= error;
            }
        }
        
        return new ServiceErrorValidationData
        {
            Messages = messages,
            FirstError = firstError
        };
    }

    /// <summary>
    /// Creates a failure ValidationResult from async validation results
    /// </summary>
    /// <param name="asyncResult">The async validation results</param>
    /// <returns>ValidationResult failure</returns>
    private ValidationResult CreateFailureResult(NonGenericAsyncValidationResult asyncResult)
    {
        var allErrors = asyncResult.StringErrors.Concat(asyncResult.ServiceErrorMessages).ToList();
        
        return asyncResult.FirstServiceError != null 
            ? ValidationResult.Failure(asyncResult.FirstServiceError)
            : ValidationResult.Failure(allErrors.ToArray());
    }

    /// <summary>
    /// Helper class to hold async validation results
    /// </summary>
    private class NonGenericAsyncValidationResult
    {
        public List<string> StringErrors { get; set; } = new();
        public List<string> ServiceErrorMessages { get; set; } = new();
        public ServiceError? FirstServiceError { get; set; }
        
        public bool HasErrors => StringErrors.Any() || ServiceErrorMessages.Any();
    }

    /// <summary>
    /// Helper class to hold ServiceError validation data
    /// </summary>
    private class ServiceErrorValidationData
    {
        public List<string> Messages { get; set; } = new();
        public ServiceError? FirstError { get; set; }
    }
}