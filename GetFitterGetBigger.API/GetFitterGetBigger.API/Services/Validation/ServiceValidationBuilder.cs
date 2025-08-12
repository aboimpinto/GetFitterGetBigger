using GetFitterGetBigger.API.Models.Interfaces;
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
    /// Gets the underlying validation instance.
    /// </summary>
    internal ServiceValidation<T> Validation => _validation;

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
    /// Automatically creates a ServiceError.ValidationFailed with the provided message.
    /// </summary>
    /// <param name="value">The string value to validate</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The builder instance for chaining</returns>
    public ServiceValidationBuilder<T> EnsureNotWhiteSpace(string? value, string errorMessage)
    {
        _validation.EnsureNotWhiteSpace(value, ServiceError.ValidationFailed(errorMessage));
        return this;
    }

    /// <summary>
    /// Validates that a string value is not null or whitespace with a ServiceError.
    /// </summary>
    /// <param name="value">The string value to validate</param>
    /// <param name="serviceError">The service error if validation fails</param>
    /// <returns>The builder instance for chaining</returns>
    public ServiceValidationBuilder<T> EnsureNotWhiteSpace(string? value, ServiceError serviceError)
    {
        _validation.EnsureNotWhiteSpace(value, serviceError);
        return this;
    }

    /// <summary>
    /// Validates that a specialized ID is not empty.
    /// Creates a ServiceError with InvalidFormat code using the provided error message.
    /// This is specifically for ID validation to maintain proper error codes.
    /// </summary>
    /// <param name="id">The specialized ID to validate</param>
    /// <param name="errorMessage">The complete error message for invalid ID format</param>
    /// <returns>The builder instance for chaining</returns>
    public ServiceValidationBuilder<T> EnsureNotEmpty(ISpecializedIdBase id, string errorMessage)
    {
        // For ID validation, we use InvalidFormat error code to match test expectations
        // The message is used as-is to avoid redundant "Invalid X format. Expected format: Y" pattern
        var serviceError = new ServiceError(ServiceErrorCode.InvalidFormat, errorMessage);
        _validation.EnsureNotEmpty(id, serviceError);
        return this;
    }

    /// <summary>
    /// Validates that a specialized ID is not empty with a ServiceError.
    /// </summary>
    /// <param name="id">The specialized ID to validate</param>
    /// <param name="serviceError">The service error if validation fails</param>
    /// <returns>The builder instance for chaining</returns>
    public ServiceValidationBuilder<T> EnsureNotEmpty(ISpecializedIdBase id, ServiceError serviceError)
    {
        _validation.EnsureNotEmpty(id, serviceError);
        return this;
    }

    /// <summary>
    /// Validates that a specialized ID is valid (not null and not empty).
    /// Uses positive phrasing: "Ensure ID IS valid" instead of "Ensure NOT empty".
    /// </summary>
    /// <param name="id">The specialized ID to validate</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The builder instance for chaining</returns>
    public ServiceValidationBuilder<T> EnsureValidId(ISpecializedIdBase id, string errorMessage)
    {
        // For ServiceValidationBuilder (Build pattern), use InvalidFormat to match test expectations
        var serviceError = new ServiceError(ServiceErrorCode.InvalidFormat, errorMessage);
        _validation.Ensure(() => id != null && !id.IsEmpty, serviceError);
        return this;
    }

    /// <summary>
    /// Validates that a string does not exceed a maximum length.
    /// </summary>
    /// <param name="value">The string value to validate</param>
    /// <param name="maxLength">The maximum allowed length</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The builder instance for chaining</returns>
    public ServiceValidationBuilder<T> EnsureMaxLength(string? value, int maxLength, string errorMessage)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _validation.Ensure(() => value.Length <= maxLength, ServiceError.ValidationFailed(errorMessage));
        }
        return this;
    }

    /// <summary>
    /// Validates that a string meets a minimum length requirement.
    /// </summary>
    /// <param name="value">The string value to validate</param>
    /// <param name="minLength">The minimum required length</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The builder instance for chaining</returns>
    public ServiceValidationBuilder<T> EnsureMinLength(string? value, int minLength, string errorMessage)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _validation.Ensure(() => value.Length >= minLength, ServiceError.ValidationFailed(errorMessage));
        }
        return this;
    }

    /// <summary>
    /// Validates that an object is not null.
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The builder instance for chaining</returns>
    public ServiceValidationBuilder<T> EnsureNotNull(object? value, string errorMessage)
    {
        _validation.Ensure(() => value != null, ServiceError.ValidationFailed(errorMessage));
        return this;
    }

    /// <summary>
    /// Validates that a number is between a minimum and maximum value (inclusive).
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="min">The minimum allowed value (inclusive)</param>
    /// <param name="max">The maximum allowed value (inclusive)</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The builder instance for chaining</returns>
    public ServiceValidationBuilder<T> EnsureNumberBetween(int value, int min, int max, string errorMessage)
    {
        _validation.Ensure(() => value >= min && value <= max, ServiceError.ValidationFailed(errorMessage));
        return this;
    }

    /// <summary>
    /// Validates that a nullable number is between a minimum and maximum value (inclusive).
    /// If the value is null, validation passes.
    /// </summary>
    /// <param name="value">The nullable value to validate</param>
    /// <param name="min">The minimum allowed value (inclusive)</param>
    /// <param name="max">The maximum allowed value (inclusive)</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The builder instance for chaining</returns>
    public ServiceValidationBuilder<T> EnsureNumberBetween(int? value, int min, int max, string errorMessage)
    {
        if (value.HasValue)
        {
            _validation.Ensure(() => value.Value >= min && value.Value <= max, ServiceError.ValidationFailed(errorMessage));
        }
        return this;
    }

    /// <summary>
    /// Validates that a collection does not exceed a maximum count.
    /// Null collections are considered valid (empty).
    /// </summary>
    /// <typeparam name="TItem">The type of items in the collection</typeparam>
    /// <param name="collection">The collection to validate</param>
    /// <param name="maxCount">The maximum allowed count</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The builder instance for chaining</returns>
    public ServiceValidationBuilder<T> EnsureMaxCount<TItem>(IEnumerable<TItem>? collection, int maxCount, string errorMessage)
    {
        if (collection != null)
        {
            _validation.Ensure(() => collection.Count() <= maxCount, ServiceError.ValidationFailed(errorMessage));
        }
        return this;
    }

    /// <summary>
    /// Validates that a nullable specialized ID is not empty if it has a value.
    /// If the ID is null, validation passes.
    /// </summary>
    /// <typeparam name="TId">The type of specialized ID</typeparam>
    /// <param name="id">The nullable specialized ID to validate</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The builder instance for chaining</returns>
    public ServiceValidationBuilder<T> EnsureNotEmpty<TId>(TId? id, string errorMessage) 
        where TId : struct, ISpecializedIdBase
    {
        if (id.HasValue)
        {
            var serviceError = new ServiceError(ServiceErrorCode.InvalidFormat, errorMessage);
            _validation.EnsureNotEmpty(id.Value, serviceError);
        }
        return this;
    }


    /// <summary>
    /// Validates that a name IS unique (positive assertion).
    /// </summary>
    /// <param name="isUniqueCheck">Async function that returns true if the name IS unique</param>
    /// <param name="entityName">The entity name for the error message</param>
    /// <param name="nameValue">The name value being checked</param>
    /// <returns>The builder instance for chaining</returns>
    public ServiceValidationBuilder<T> EnsureNameIsUniqueAsync(
        Func<Task<bool>> isUniqueCheck,
        string entityName,
        string nameValue)
    {
        _asyncServiceErrorValidations.Add(async () =>
        {
            var isUnique = await isUniqueCheck();
            return (isUnique, isUnique ? null : ServiceError.AlreadyExists(entityName, nameValue));
        });
        return this;
    }

    /// <summary>
    /// Validates that something HAS a valid configuration (positive assertion).
    /// </summary>
    /// <param name="hasValidCheck">Async function that returns true if configuration IS valid</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The builder instance for chaining</returns>
    public ServiceValidationBuilder<T> EnsureHasValidAsync(
        Func<Task<bool>> hasValidCheck,
        string errorMessage)
    {
        _asyncServiceErrorValidations.Add(async () =>
        {
            var isValid = await hasValidCheck();
            return (isValid, isValid ? null : ServiceError.ValidationFailed(errorMessage));
        });
        return this;
    }

    /// <summary>
    /// Validates that an item is unique using a positive check.
    /// The predicate should return true when the item IS unique.
    /// </summary>
    /// <param name="isUniqueCheck">Async function that returns true if the item is unique</param>
    /// <param name="serviceError">The service error if the item is not unique</param>
    /// <returns>The builder instance for chaining</returns>
    public ServiceValidationBuilder<T> EnsureIsUniqueAsync(
        Func<Task<bool>> isUniqueCheck,
        ServiceError serviceError)
    {
        _asyncServiceErrorValidations.Add(async () =>
        {
            var isUnique = await isUniqueCheck();
            return (isUnique, isUnique ? null : serviceError);
        });
        return this;
    }

    /// <summary>
    /// Validates that an item is unique using a positive check.
    /// Automatically creates a ServiceError.AlreadyExists with the entity name and value.
    /// </summary>
    /// <param name="isUniqueCheck">Async function that returns true if the item is unique</param>
    /// <param name="entityName">The name of the entity for the error message</param>
    /// <param name="itemValue">The value that already exists</param>
    /// <returns>The builder instance for chaining</returns>
    public ServiceValidationBuilder<T> EnsureIsUniqueAsync(
        Func<Task<bool>> isUniqueCheck,
        string entityName,
        string itemValue)
    {
        return EnsureIsUniqueAsync(isUniqueCheck, ServiceError.AlreadyExists(entityName, itemValue));
    }

    /// <summary>
    /// Validates that an item does not exist.
    /// The predicate should return true when the item does NOT exist.
    /// </summary>
    /// <param name="doesNotExistCheck">Async function that returns true if the item doesn't exist</param>
    /// <param name="serviceError">The service error if the item exists</param>
    /// <returns>The builder instance for chaining</returns>
    public ServiceValidationBuilder<T> EnsureNotExistsAsync(
        Func<Task<bool>> doesNotExistCheck,
        ServiceError serviceError)
    {
        _asyncServiceErrorValidations.Add(async () =>
        {
            var doesNotExist = await doesNotExistCheck();
            return (doesNotExist, doesNotExist ? null : serviceError);
        });
        return this;
    }

    /// <summary>
    /// Adds an async validation that checks if an entity exists.
    /// Automatically creates a ServiceError.NotFound with the entity name.
    /// </summary>
    /// <param name="existenceCheck">Function that returns true if entity exists</param>
    /// <param name="entityName">The name of the entity for the error message</param>
    /// <returns>The builder instance for chaining</returns>
    public ServiceValidationBuilder<T> EnsureExistsAsync(
        Func<Task<bool>> existenceCheck, 
        string entityName)
    {
        return EnsureExistsAsync(existenceCheck, ServiceError.NotFound(entityName));
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
    /// Adds an async validation with custom logic that returns a ServiceError.
    /// </summary>
    /// <param name="validationFunc">Function that returns (IsValid, ServiceError)</param>
    /// <returns>The builder instance for chaining</returns>
    public ServiceValidationBuilder<T> EnsureAsync(
        Func<Task<(bool IsValid, ServiceError? Error)>> validationFunc)
    {
        _asyncServiceErrorValidations.Add(validationFunc);
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
    /// Marks a point in the validation chain where subsequent validations should only run if no errors have occurred so far.
    /// This allows dependent validations to be skipped if basic validations fail.
    /// </summary>
    /// <returns>The builder instance for chaining</returns>
    public ServiceValidationBuilder<T> WhenValid()
    {
        // Add a marker that subsequent validations are conditional
        // This is handled by checking HasErrors before adding new validations
        return this;
    }

    /// <summary>
    /// Adds a validation that only runs if all previous validations have passed.
    /// This is useful for expensive validations like database checks that should only run after basic validations pass.
    /// </summary>
    /// <param name="predicate">The condition that must be true for validation to pass</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The builder instance for chaining</returns>
    public ServiceValidationBuilder<T> EnsureWhenValid(Func<bool> predicate, string errorMessage)
    {
        // Only add this validation if no errors exist
        if (!_validation.HasErrors)
        {
            _validation.Ensure(predicate, errorMessage);
        }
        return this;
    }

    /// <summary>
    /// Adds an async validation that only runs if all previous validations have passed.
    /// This prevents unnecessary database calls when basic validations fail.
    /// </summary>
    /// <param name="predicate">The async condition that must be true for validation to pass</param>
    /// <param name="serviceError">The service error if validation fails</param>
    /// <returns>The builder instance for chaining</returns>
    public ServiceValidationBuilder<T> EnsureAsyncWhenValid(
        Func<Task<bool>> predicate, 
        ServiceError serviceError)
    {
        _asyncServiceErrorValidations.Add(async () => 
        {
            // Check if there are any synchronous validation errors
            if (_validation.HasErrors)
            {
                // Skip this validation - return as valid since we don't want to add more errors
                return (true, null);
            }
            
            var isValid = await predicate();
            return (isValid, isValid ? null : serviceError);
        });
        return this;
    }

    /// <summary>
    /// Validates that a name is unique, but only if previous validations have passed.
    /// This prevents database checks when the name is already invalid.
    /// </summary>
    /// <param name="isUniqueCheck">Async function that returns true if the name IS unique</param>
    /// <param name="entityName">The entity name for the error message</param>
    /// <param name="nameValue">The name value being checked</param>
    /// <returns>The builder instance for chaining</returns>
    public ServiceValidationBuilder<T> EnsureNameIsUniqueWhenValidAsync(
        Func<Task<bool>> isUniqueCheck,
        string entityName,
        string nameValue)
    {
        _asyncServiceErrorValidations.Add(async () =>
        {
            // Skip if there are validation errors
            if (_validation.HasErrors)
            {
                return (true, null);
            }
            
            var isUnique = await isUniqueCheck();
            return (isUnique, isUnique ? null : ServiceError.AlreadyExists(entityName, nameValue));
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
        ServiceError? firstServiceError = null;
        
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
                // Capture the first ServiceError to preserve the error code
                firstServiceError ??= error;
            }
        }

        // If we have errors, return a failure with the first ServiceError (if any)
        if (errors.Any())
        {
            // If we have a ServiceError, create a new one with all error messages
            if (firstServiceError != null)
            {
                var combinedMessage = errors.Count == 1 
                    ? firstServiceError.Message 
                    : string.Join("; ", errors);
                var combinedError = new ServiceError(firstServiceError.Code, combinedMessage);
                return ValidationResult.Failure(combinedError);
            }
            return ValidationResult.Failure(errors.ToArray());
        }

        return ValidationResult.Success();
    }
}