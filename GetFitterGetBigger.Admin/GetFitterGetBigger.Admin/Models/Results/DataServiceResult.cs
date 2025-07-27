using GetFitterGetBigger.Admin.Models.Errors;

namespace GetFitterGetBigger.Admin.Models.Results;

public class DataServiceResult<T> : ResultBase<T, DataServiceResult<T>, DataError>
{
    // Data-specific helper methods
    public bool IsNotFound => HasError(DataErrorCode.NotFound);
    public bool IsTimeout => HasError(DataErrorCode.Timeout);
    public bool IsNetworkError => HasError(DataErrorCode.NetworkError);
    public bool IsUnauthorized => HasError(DataErrorCode.Unauthorized);
    public bool IsForbidden => HasError(DataErrorCode.Forbidden);
    public bool IsBadRequest => HasError(DataErrorCode.BadRequest);
    public bool IsServerError => HasError(DataErrorCode.ServerError);
    
    // Convenience factory methods for common scenarios
    public static DataServiceResult<T> NotFound(string resource) => 
        Failure(DataError.NotFound(resource));
        
    public static DataServiceResult<T> Timeout() => 
        Failure(DataError.Timeout());
        
    public static DataServiceResult<T> NetworkError(string message) => 
        Failure(DataError.NetworkError(message));
        
    public static DataServiceResult<T> Unauthorized() => 
        Failure(DataError.Unauthorized());
        
    public static DataServiceResult<T> ServerError(string message) => 
        Failure(DataError.ServerError(message));
}