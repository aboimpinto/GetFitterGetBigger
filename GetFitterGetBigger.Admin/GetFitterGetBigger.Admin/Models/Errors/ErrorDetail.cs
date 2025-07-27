namespace GetFitterGetBigger.Admin.Models.Errors;

public abstract record ErrorDetail<TErrorCode> : IErrorDetail where TErrorCode : Enum
{
    public TErrorCode Code { get; init; }
    public string Message { get; init; }
    public Dictionary<string, object>? Details { get; init; }
    
    Enum IErrorDetail.Code => Code;

    protected ErrorDetail(TErrorCode code, string message, Dictionary<string, object>? details = null)
    {
        Code = code;
        Message = message;
        Details = details;
    }
}