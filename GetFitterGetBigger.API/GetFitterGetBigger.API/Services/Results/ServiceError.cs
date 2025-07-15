namespace GetFitterGetBigger.API.Services.Results;

/// <summary>
/// Represents standardized error codes for service operations
/// </summary>
public enum ServiceErrorCode
{
    /// <summary>
    /// No error (default value)
    /// </summary>
    None = 0,
    
    /// <summary>
    /// The requested resource was not found
    /// </summary>
    NotFound,
    
    /// <summary>
    /// The input format is invalid
    /// </summary>
    InvalidFormat,
    
    /// <summary>
    /// The resource already exists
    /// </summary>
    AlreadyExists,
    
    /// <summary>
    /// Validation of the input failed
    /// </summary>
    ValidationFailed,
    
    /// <summary>
    /// The user is not authorized to perform this operation
    /// </summary>
    Unauthorized,
    
    /// <summary>
    /// A concurrency conflict occurred
    /// </summary>
    ConcurrencyConflict,
    
    /// <summary>
    /// Cannot perform operation due to existing dependencies
    /// </summary>
    DependencyExists,
    
    /// <summary>
    /// An internal error occurred
    /// </summary>
    InternalError
}

/// <summary>
/// Represents a structured error with a code and message
/// </summary>
public record ServiceError(ServiceErrorCode Code, string Message)
{
    /// <summary>
    /// Creates a not found error
    /// </summary>
    /// <param name="entityName">The name of the entity that was not found</param>
    /// <returns>A not found service error</returns>
    public static ServiceError NotFound(string entityName) => 
        new(ServiceErrorCode.NotFound, $"{entityName} not found");
    
    /// <summary>
    /// Creates a not found error with additional context
    /// </summary>
    /// <param name="entityName">The name of the entity</param>
    /// <param name="identifier">The identifier that was searched for</param>
    /// <returns>A not found service error</returns>
    public static ServiceError NotFound(string entityName, string identifier) => 
        new(ServiceErrorCode.NotFound, $"{entityName} with value '{identifier}' not found");
    
    /// <summary>
    /// Creates an invalid format error
    /// </summary>
    /// <param name="fieldName">The name of the field with invalid format</param>
    /// <param name="expectedFormat">The expected format</param>
    /// <returns>An invalid format service error</returns>
    public static ServiceError InvalidFormat(string fieldName, string expectedFormat) => 
        new(ServiceErrorCode.InvalidFormat, $"Invalid {fieldName} format. Expected format: {expectedFormat}");
    
    /// <summary>
    /// Creates an invalid format error with actual value
    /// </summary>
    /// <param name="fieldName">The name of the field with invalid format</param>
    /// <param name="expectedFormat">The expected format</param>
    /// <param name="actualValue">The actual value received</param>
    /// <returns>An invalid format service error</returns>
    public static ServiceError InvalidFormat(string fieldName, string expectedFormat, string actualValue) => 
        new(ServiceErrorCode.InvalidFormat, $"Invalid {fieldName} format. Expected format: {expectedFormat}, got: '{actualValue}'");
    
    /// <summary>
    /// Creates an already exists error
    /// </summary>
    /// <param name="entityName">The name of the entity</param>
    /// <param name="identifier">The identifier that already exists</param>
    /// <returns>An already exists service error</returns>
    public static ServiceError AlreadyExists(string entityName, string identifier) => 
        new(ServiceErrorCode.AlreadyExists, $"{entityName} '{identifier}' already exists");
    
    /// <summary>
    /// Creates a validation failed error
    /// </summary>
    /// <param name="message">The validation error message</param>
    /// <returns>A validation failed service error</returns>
    public static ServiceError ValidationFailed(string message) => 
        new(ServiceErrorCode.ValidationFailed, message);
    
    /// <summary>
    /// Creates an unauthorized error
    /// </summary>
    /// <param name="message">The unauthorized error message</param>
    /// <returns>An unauthorized service error</returns>
    public static ServiceError Unauthorized(string message) => 
        new(ServiceErrorCode.Unauthorized, message);
    
    /// <summary>
    /// Creates a concurrency conflict error
    /// </summary>
    /// <param name="entityName">The name of the entity with the conflict</param>
    /// <returns>A concurrency conflict service error</returns>
    public static ServiceError ConcurrencyConflict(string entityName) => 
        new(ServiceErrorCode.ConcurrencyConflict, $"{entityName} was modified by another user");
    
    /// <summary>
    /// Creates a dependency exists error
    /// </summary>
    /// <param name="entityName">The name of the entity</param>
    /// <param name="dependencyDescription">Description of the dependencies</param>
    /// <returns>A dependency exists service error</returns>
    public static ServiceError DependencyExists(string entityName, string dependencyDescription) => 
        new(ServiceErrorCode.DependencyExists, $"Cannot delete {entityName} because it has {dependencyDescription}");
    
    /// <summary>
    /// Creates an internal error
    /// </summary>
    /// <param name="message">The error message</param>
    /// <returns>An internal service error</returns>
    public static ServiceError InternalError(string message) => 
        new(ServiceErrorCode.InternalError, message);
}