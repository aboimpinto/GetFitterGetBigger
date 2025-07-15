namespace GetFitterGetBigger.API.Constants;

public static class ExecutionProtocolErrorMessages
{
    // Entity validation messages
    public const string ValueCannotBeEmptyEntity = "ExecutionProtocol value cannot be empty";
    public const string DisplayOrderMustBeNonNegative = "Display order must be non-negative";
    public const string CodeCannotBeEmpty = "Code cannot be empty";
    public const string CodeTooLong = "Code cannot exceed 50 characters";
    public const string ValueTooLong = "Value cannot exceed 100 characters";
    public const string CodeInvalidFormat = "Code must be uppercase with underscores only";
    
    // Service error messages
    public const string InvalidIdFormat = "Invalid execution protocol ID format";
    public const string IdCannotBeEmpty = "ExecutionProtocol ID cannot be empty";
    public const string ValueCannotBeEmpty = "Value cannot be empty";
    public const string NotFound = "ExecutionProtocol not found";
    public const string AlreadyExists = "ExecutionProtocol with the same code already exists";
}