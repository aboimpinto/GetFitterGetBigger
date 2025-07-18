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
    /// List of structured errors with error codes (empty if operation was successful)
    /// </summary>
    public List<ServiceError> StructuredErrors { get; init; } = new();
    
    /// <summary>
    /// Gets the primary error code from the first structured error, or None if no structured errors
    /// </summary>
    public ServiceErrorCode PrimaryErrorCode => StructuredErrors.FirstOrDefault()?.Code ?? ServiceErrorCode.None;
    
    /// <summary>
    /// Creates a successful result with data
    /// </summary>
    public static ServiceResult<T> Success(T data) => new()
    {
        Data = data,
        IsSuccess = true,
        Errors = new List<string>(),
        StructuredErrors = new List<ServiceError>()
    };
    
    /// <summary>
    /// Creates a failed result with validation errors
    /// </summary>
    public static ServiceResult<T> Failure(T emptyData, params string[] errors) => new()
    {
        Data = emptyData,
        IsSuccess = false,
        Errors = errors.ToList(),
        StructuredErrors = new List<ServiceError>()
    };
    
    /// <summary>
    /// Creates a failed result with validation errors
    /// </summary>
    public static ServiceResult<T> Failure(T emptyData, List<string> errors) => new()
    {
        Data = emptyData,
        IsSuccess = false,
        Errors = errors,
        StructuredErrors = new List<ServiceError>()
    };
    
    /// <summary>
    /// Creates a failed result with a structured error
    /// </summary>
    public static ServiceResult<T> Failure(T emptyData, ServiceError error) => new()
    {
        Data = emptyData,
        IsSuccess = false,
        Errors = new List<string> { error.Message },
        StructuredErrors = new List<ServiceError> { error }
    };
    
    /// <summary>
    /// Creates a failed result with multiple structured errors
    /// </summary>
    public static ServiceResult<T> Failure(T emptyData, params ServiceError[] errors) => new()
    {
        Data = emptyData,
        IsSuccess = false,
        Errors = errors.Select(e => e.Message).ToList(),
        StructuredErrors = errors.ToList()
    };
    
    /// <summary>
    /// Creates a failed result with multiple structured errors
    /// </summary>
    public static ServiceResult<T> Failure(T emptyData, List<ServiceError> errors) => new()
    {
        Data = emptyData,
        IsSuccess = false,
        Errors = errors.Select(e => e.Message).ToList(),
        StructuredErrors = errors
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
    /// The service error if validation failed with a specific error code
    /// </summary>
    public ServiceError? ServiceError { get; init; }
    
    /// <summary>
    /// Creates a successful validation result
    /// </summary>
    public static ValidationResult Success() => new()
    {
        IsValid = true,
        Errors = new List<string>(),
        ServiceError = null
    };
    
    /// <summary>
    /// Creates a failed validation result with errors
    /// </summary>
    public static ValidationResult Failure(params string[] errors) => new()
    {
        IsValid = false,
        Errors = errors.ToList(),
        ServiceError = null
    };
    
    /// <summary>
    /// Creates a failed validation result with a single error
    /// </summary>
    public static ValidationResult Failure(string error) => new()
    {
        IsValid = false,
        Errors = new List<string> { error },
        ServiceError = null
    };
    
    /// <summary>
    /// Creates a failed validation result with a service error
    /// </summary>
    public static ValidationResult Failure(ServiceError serviceError) => new()
    {
        IsValid = false,
        Errors = new List<string> { serviceError.Message },
        ServiceError = serviceError
    };
}