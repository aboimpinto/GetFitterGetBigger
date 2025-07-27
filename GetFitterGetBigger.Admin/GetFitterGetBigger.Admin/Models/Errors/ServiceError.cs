namespace GetFitterGetBigger.Admin.Models.Errors;

public record ServiceError : ErrorDetail<ServiceErrorCode>
{
    public ServiceError(ServiceErrorCode code, string message, Dictionary<string, object>? details = null)
        : base(code, message, details) { }

    // Factory methods
    public static ServiceError TemplateNotFound(string id) => 
        new(ServiceErrorCode.TemplateNotFound, $"Template {id} not found");

    public static ServiceError ValidationRequired(string field) => 
        new(ServiceErrorCode.ValidationRequired, $"{field} is required");

    public static ServiceError ValidationInvalid(string field, string reason) => 
        new(ServiceErrorCode.ValidationInvalid, $"{field} is invalid: {reason}");

    public static ServiceError ValidationOutOfRange(string field, string range) => 
        new(ServiceErrorCode.ValidationOutOfRange, $"{field} is out of range: {range}");

    public static ServiceError InvalidStateTransition(string fromState, string toState) => 
        new(ServiceErrorCode.InvalidStateTransition, $"Cannot transition from {fromState} to {toState}");

    public static ServiceError DuplicateName(string name) => 
        new(ServiceErrorCode.DuplicateName, $"An item with the name '{name}' already exists");

    public static ServiceError ExceedsLimit(string resource, int limit) => 
        new(ServiceErrorCode.ExceedsLimit, $"{resource} exceeds maximum limit of {limit}");

    public static ServiceError InsufficientPermissions(string action) => 
        new(ServiceErrorCode.InsufficientPermissions, $"Insufficient permissions to {action}");

    public static ServiceError ConcurrencyConflict() => 
        new(ServiceErrorCode.ConcurrencyConflict, "The resource was modified by another user");

    public static ServiceError DependencyFailure(string dependency, string message) => 
        new(ServiceErrorCode.DependencyFailure, $"{dependency} failed: {message}");
}