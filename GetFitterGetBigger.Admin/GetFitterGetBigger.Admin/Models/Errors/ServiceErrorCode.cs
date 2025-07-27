namespace GetFitterGetBigger.Admin.Models.Errors;

public enum ServiceErrorCode
{
    None = 0,
    // Validation errors
    ValidationRequired,
    ValidationInvalid,
    ValidationOutOfRange,
    
    // Business errors
    TemplateNotFound,
    InvalidStateTransition,
    DuplicateName,
    ExceedsLimit,
    InsufficientPermissions,
    
    // Operation errors
    ConcurrencyConflict,
    DependencyFailure
}