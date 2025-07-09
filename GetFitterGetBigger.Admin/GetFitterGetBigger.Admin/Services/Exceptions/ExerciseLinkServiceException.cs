using System.Net;

namespace GetFitterGetBigger.Admin.Services.Exceptions;

/// <summary>
/// Base exception class for all exercise link service related errors
/// </summary>
public class ExerciseLinkServiceException : Exception
{
    public HttpStatusCode? StatusCode { get; }
    public string? ErrorCode { get; }

    public ExerciseLinkServiceException(string message) : base(message)
    {
    }

    public ExerciseLinkServiceException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ExerciseLinkServiceException(string message, HttpStatusCode statusCode, string? errorCode = null) : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }

    public ExerciseLinkServiceException(string message, HttpStatusCode statusCode, Exception innerException, string? errorCode = null) : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Exception thrown when an exercise link is not found
/// </summary>
public class ExerciseLinkNotFoundException : ExerciseLinkServiceException
{
    public ExerciseLinkNotFoundException(string exerciseId, string linkId) 
        : base($"Exercise link '{linkId}' not found for exercise '{exerciseId}'", HttpStatusCode.NotFound, "EXERCISE_LINK_NOT_FOUND")
    {
    }
}

/// <summary>
/// Exception thrown when an exercise cannot be found
/// </summary>
public class ExerciseNotFoundException : ExerciseLinkServiceException
{
    public ExerciseNotFoundException(string exerciseId) 
        : base($"Exercise '{exerciseId}' not found", HttpStatusCode.NotFound, "EXERCISE_NOT_FOUND")
    {
    }
}

/// <summary>
/// Exception thrown when trying to create an invalid link (e.g., circular reference, wrong type)
/// </summary>
public class InvalidExerciseLinkException : ExerciseLinkServiceException
{
    public InvalidExerciseLinkException(string message) 
        : base(message, HttpStatusCode.BadRequest, "INVALID_EXERCISE_LINK")
    {
    }
}

/// <summary>
/// Exception thrown when maximum link limit is exceeded
/// </summary>
public class MaximumLinksExceededException : ExerciseLinkServiceException
{
    public MaximumLinksExceededException(string linkType, int maxAllowed) 
        : base($"Maximum {linkType} links exceeded. Only {maxAllowed} links are allowed per exercise", HttpStatusCode.BadRequest, "MAX_LINKS_EXCEEDED")
    {
    }
}

/// <summary>
/// Exception thrown when a duplicate link is attempted
/// </summary>
public class DuplicateExerciseLinkException : ExerciseLinkServiceException
{
    public DuplicateExerciseLinkException(string sourceExerciseId, string targetExerciseId, string linkType) 
        : base($"Link already exists between exercises '{sourceExerciseId}' and '{targetExerciseId}' of type '{linkType}'", HttpStatusCode.Conflict, "DUPLICATE_EXERCISE_LINK")
    {
    }
}

/// <summary>
/// Exception thrown when API communication fails
/// </summary>
public class ExerciseLinkApiException : ExerciseLinkServiceException
{
    public ExerciseLinkApiException(string message, HttpStatusCode statusCode) 
        : base(message, statusCode, "API_ERROR")
    {
    }

    public ExerciseLinkApiException(string message, HttpStatusCode statusCode, Exception innerException) 
        : base(message, statusCode, innerException, "API_ERROR")
    {
    }
}