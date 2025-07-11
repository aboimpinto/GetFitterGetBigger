namespace GetFitterGetBigger.API.Services.Results;

/// <summary>
/// Represents the result of a service operation including validation errors
/// </summary>
public record ServiceResult<T>
{
    /// <summary>
    /// The data returned by the operation (may be empty if validation failed)
    /// </summary>
    public required T Data { get; init; }
    
    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    public bool IsSuccess { get; init; }
    
    /// <summary>
    /// List of validation errors (empty if operation was successful)
    /// </summary>
    public List<string> Errors { get; init; } = new();
    
    /// <summary>
    /// Creates a successful result with data
    /// </summary>
    public static ServiceResult<T> Success(T data) => new()
    {
        Data = data,
        IsSuccess = true,
        Errors = new List<string>()
    };
    
    /// <summary>
    /// Creates a failed result with validation errors
    /// </summary>
    public static ServiceResult<T> Failure(T emptyData, params string[] errors) => new()
    {
        Data = emptyData,
        IsSuccess = false,
        Errors = errors.ToList()
    };
    
    /// <summary>
    /// Creates a failed result with validation errors
    /// </summary>
    public static ServiceResult<T> Failure(T emptyData, List<string> errors) => new()
    {
        Data = emptyData,
        IsSuccess = false,
        Errors = errors
    };
}

/// <summary>
/// Represents a validation result
/// </summary>
public record ValidationResult
{
    /// <summary>
    /// Indicates if validation passed
    /// </summary>
    public bool IsValid { get; init; }
    
    /// <summary>
    /// List of validation errors (empty if validation passed)
    /// </summary>
    public List<string> Errors { get; init; } = new();
    
    /// <summary>
    /// Creates a successful validation result
    /// </summary>
    public static ValidationResult Success() => new()
    {
        IsValid = true,
        Errors = new List<string>()
    };
    
    /// <summary>
    /// Creates a failed validation result with errors
    /// </summary>
    public static ValidationResult Failure(params string[] errors) => new()
    {
        IsValid = false,
        Errors = errors.ToList()
    };
    
    /// <summary>
    /// Creates a failed validation result with a single error
    /// </summary>
    public static ValidationResult Failure(string error) => new()
    {
        IsValid = false,
        Errors = new List<string> { error }
    };
}