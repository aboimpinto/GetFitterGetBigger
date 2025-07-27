using GetFitterGetBigger.Admin.Models.Errors;

namespace GetFitterGetBigger.Admin.Models.Results;

public class ServiceResult<T> : ResultBase<T, ServiceResult<T>, ServiceError>
{
    // Service-specific helper methods
    public bool IsValidationError => Errors.Any(e => 
        e.Code == ServiceErrorCode.ValidationRequired || 
        e.Code == ServiceErrorCode.ValidationInvalid ||
        e.Code == ServiceErrorCode.ValidationOutOfRange);
        
    public bool IsBusinessError => Errors.Any(e =>
        e.Code == ServiceErrorCode.TemplateNotFound ||
        e.Code == ServiceErrorCode.InvalidStateTransition ||
        e.Code == ServiceErrorCode.DuplicateName ||
        e.Code == ServiceErrorCode.ExceedsLimit ||
        e.Code == ServiceErrorCode.InsufficientPermissions);
        
    public bool IsConcurrencyError => HasError(ServiceErrorCode.ConcurrencyConflict);
    public bool IsDependencyError => HasError(ServiceErrorCode.DependencyFailure);
    
    // Convenience factory methods for common scenarios
    public static ServiceResult<T> ValidationError(params ServiceError[] errors) => 
        Failure(errors);
        
    public static ServiceResult<T> NotFound(string resource, string id) => 
        Failure(ServiceError.TemplateNotFound(id));
        
    public static ServiceResult<T> InvalidState(string fromState, string toState) => 
        Failure(ServiceError.InvalidStateTransition(fromState, toState));
        
    public static ServiceResult<T> DuplicateName(string name) => 
        Failure(ServiceError.DuplicateName(name));
        
    // Convert from DataServiceResult
    public static ServiceResult<T> FromDataResult(DataServiceResult<T> dataResult, 
        Func<DataError, ServiceError>? errorMapper = null)
    {
        if (dataResult.IsSuccess)
        {
            return Success(dataResult.Data!);
        }
        
        var serviceErrors = dataResult.Errors
            .Select(dataError => errorMapper?.Invoke(dataError) ?? MapDataErrorToServiceError(dataError))
            .ToArray();
            
        return Failure(serviceErrors);
    }
    
    private static ServiceError MapDataErrorToServiceError(DataError dataError)
    {
        return dataError.Code switch
        {
            DataErrorCode.NotFound => new ServiceError(ServiceErrorCode.TemplateNotFound, dataError.Message),
            DataErrorCode.Conflict => new ServiceError(ServiceErrorCode.DuplicateName, dataError.Message),
            DataErrorCode.Unauthorized => new ServiceError(ServiceErrorCode.InsufficientPermissions, dataError.Message),
            DataErrorCode.Forbidden => new ServiceError(ServiceErrorCode.InsufficientPermissions, dataError.Message),
            _ => new ServiceError(ServiceErrorCode.DependencyFailure, dataError.Message)
        };
    }
}