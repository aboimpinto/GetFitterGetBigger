namespace GetFitterGetBigger.Admin.Models.Errors;

public record DataError : ErrorDetail<DataErrorCode>
{
    public DataError(DataErrorCode code, string message, Dictionary<string, object>? details = null)
        : base(code, message, details) { }

    // Factory methods
    public static DataError NotFound(string resource) => 
        new(DataErrorCode.NotFound, $"{resource} not found");

    public static DataError Timeout() => 
        new(DataErrorCode.Timeout, "Request timed out");

    public static DataError NetworkError(string message) => 
        new(DataErrorCode.NetworkError, $"Network error: {message}");

    public static DataError Unauthorized() => 
        new(DataErrorCode.Unauthorized, "Unauthorized access");

    public static DataError Forbidden() => 
        new(DataErrorCode.Forbidden, "Access forbidden");

    public static DataError BadRequest(string message) => 
        new(DataErrorCode.BadRequest, message);

    public static DataError ServerError(string message) => 
        new(DataErrorCode.ServerError, $"Server error: {message}");

    public static DataError DeserializationError(string message) => 
        new(DataErrorCode.DeserializationError, $"Failed to deserialize: {message}");

    public static DataError Conflict(string message) => 
        new(DataErrorCode.Conflict, message);
}