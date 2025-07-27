namespace GetFitterGetBigger.Admin.Models.Errors;

public interface IErrorDetail
{
    Enum Code { get; }
    string Message { get; }
    Dictionary<string, object>? Details { get; }
}