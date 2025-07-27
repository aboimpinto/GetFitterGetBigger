namespace GetFitterGetBigger.Admin.Models.Errors;

/// <summary>
/// Base class for strongly-typed error details with error code, message, and additional context
/// </summary>
/// <typeparam name="TErrorCode">The enum type representing error codes</typeparam>
public abstract record ErrorDetail<TErrorCode> : IErrorDetail where TErrorCode : Enum
{
    /// <summary>
    /// The specific error code identifying the type of error
    /// </summary>
    public TErrorCode Code { get; init; }
    
    /// <summary>
    /// A human-readable error message describing the issue
    /// </summary>
    public string Message { get; init; }
    
    /// <summary>
    /// Optional additional details providing context about the error
    /// </summary>
    public Dictionary<string, object>? Details { get; init; }
    
    Enum IErrorDetail.Code => Code;

    /// <summary>
    /// Initializes a new instance of the ErrorDetail record
    /// </summary>
    /// <param name="code">The error code</param>
    /// <param name="message">The error message</param>
    /// <param name="details">Optional additional error details</param>
    protected ErrorDetail(TErrorCode code, string message, Dictionary<string, object>? details = null)
    {
        Code = code;
        Message = message;
        Details = details;
    }
}