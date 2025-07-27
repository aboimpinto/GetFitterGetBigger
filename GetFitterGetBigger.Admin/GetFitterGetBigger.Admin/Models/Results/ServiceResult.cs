using GetFitterGetBigger.Admin.Models.Errors;

namespace GetFitterGetBigger.Admin.Models.Results;

/// <summary>
/// Represents the result of a service operation, containing either success data or error information
/// </summary>
/// <typeparam name="T">The type of data returned on success</typeparam>
public class ServiceResult<T> : ResultBase<T, ServiceResult<T>, ServiceError>
{
    // Service-specific helper methods
    
    /// <summary>
    /// Indicates whether the result contains validation errors
    /// </summary>
    public bool IsValidationError => Errors.Any(e => 
        e.Code == ServiceErrorCode.ValidationRequired || 
        e.Code == ServiceErrorCode.ValidationInvalid ||
        e.Code == ServiceErrorCode.ValidationOutOfRange);
    
    /// <summary>
    /// Indicates whether the result contains business rule violations
    /// </summary>    
    public bool IsBusinessError => Errors.Any(e =>
        e.Code == ServiceErrorCode.TemplateNotFound ||
        e.Code == ServiceErrorCode.InvalidStateTransition ||
        e.Code == ServiceErrorCode.DuplicateName ||
        e.Code == ServiceErrorCode.ExceedsLimit ||
        e.Code == ServiceErrorCode.InsufficientPermissions);
    
    /// <summary>
    /// Indicates whether the result contains a concurrency conflict error
    /// </summary>    
    public bool IsConcurrencyError => HasError(ServiceErrorCode.ConcurrencyConflict);
    
    /// <summary>
    /// Indicates whether the result contains a dependency failure error
    /// </summary>
    public bool IsDependencyError => HasError(ServiceErrorCode.DependencyFailure);
    
    // Convenience factory methods for common scenarios
    
    /// <summary>
    /// Creates a failure result with validation errors
    /// </summary>
    /// <param name="errors">The validation errors that occurred</param>
    /// <returns>A failed result containing the validation errors</returns>
    public static ServiceResult<T> ValidationError(params ServiceError[] errors) => 
        Failure(errors);
    
    /// <summary>
    /// Creates a failure result for a resource not found scenario
    /// </summary>
    /// <param name="resource">The type of resource that was not found</param>
    /// <param name="id">The identifier of the resource that was not found</param>
    /// <returns>A failed result with a not found error</returns>    
    public static ServiceResult<T> NotFound(string resource, string id) => 
        Failure(ServiceError.TemplateNotFound(id));
    
    /// <summary>
    /// Creates a failure result for an invalid state transition
    /// </summary>
    /// <param name="fromState">The current state</param>
    /// <param name="toState">The attempted target state</param>
    /// <returns>A failed result with an invalid state transition error</returns>    
    public static ServiceResult<T> InvalidState(string fromState, string toState) => 
        Failure(ServiceError.InvalidStateTransition(fromState, toState));
    
    /// <summary>
    /// Creates a failure result for a duplicate name scenario
    /// </summary>
    /// <param name="name">The name that already exists</param>
    /// <returns>A failed result with a duplicate name error</returns>    
    public static ServiceResult<T> DuplicateName(string name) => 
        Failure(ServiceError.DuplicateName(name));
        
    // Convert from DataServiceResult
    
    /// <summary>
    /// Converts a DataServiceResult to a ServiceResult, mapping errors appropriately
    /// </summary>
    /// <param name="dataResult">The data service result to convert</param>
    /// <param name="errorMapper">Optional custom error mapper function</param>
    /// <returns>A service result with mapped data or errors</returns>
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