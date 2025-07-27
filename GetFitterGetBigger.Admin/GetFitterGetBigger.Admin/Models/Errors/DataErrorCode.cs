namespace GetFitterGetBigger.Admin.Models.Errors;

public enum DataErrorCode
{
    None = 0,
    NotFound,
    Unauthorized,
    Forbidden,
    Conflict,
    BadRequest,
    NetworkError,
    Timeout,
    ServerError,
    DeserializationError
}