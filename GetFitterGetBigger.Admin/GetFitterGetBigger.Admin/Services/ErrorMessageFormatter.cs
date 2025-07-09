using GetFitterGetBigger.Admin.Services.Exceptions;
using System.Net;

namespace GetFitterGetBigger.Admin.Services;

/// <summary>
/// Provides user-friendly error messages for exercise link operations
/// </summary>
public static class ErrorMessageFormatter
{
    public static string FormatExerciseLinkError(Exception exception)
    {
        return exception switch
        {
            // Specific exercise link exceptions
            DuplicateExerciseLinkException => 
                "This exercise is already linked. Each exercise can only be linked once per type.",
            
            MaximumLinksExceededException => 
                "You've reached the maximum number of linked exercises. Please remove an existing link before adding a new one.",
            
            InvalidExerciseLinkException invEx when invEx.Message.Contains("circular reference", StringComparison.OrdinalIgnoreCase) => 
                "This would create a circular reference. An exercise cannot be linked to itself or to exercises that link back to it.",
            
            InvalidExerciseLinkException invEx when invEx.Message.Contains("exercise type", StringComparison.OrdinalIgnoreCase) => 
                "Only exercises of type 'Workout' can have warmup or cooldown links. Please select a different exercise or change the exercise type.",
            
            InvalidExerciseLinkException invEx => 
                $"This link is not valid: {invEx.Message}",
            
            ExerciseLinkNotFoundException => 
                "The exercise link could not be found. It may have been deleted by another user. The list will be refreshed.",
            
            // API exceptions with status codes
            ExerciseLinkApiException apiEx when apiEx.StatusCode == HttpStatusCode.Unauthorized => 
                "You don't have permission to manage exercise links. Please contact your administrator.",
            
            ExerciseLinkApiException apiEx when apiEx.StatusCode == HttpStatusCode.Forbidden => 
                "Access denied. You may not have the required role to perform this action.",
            
            ExerciseLinkApiException apiEx when apiEx.StatusCode == HttpStatusCode.ServiceUnavailable => 
                "The service is temporarily unavailable. Please try again in a few moments.",
            
            ExerciseLinkApiException apiEx when apiEx.StatusCode == HttpStatusCode.RequestTimeout => 
                "The request took too long to complete. Please check your connection and try again.",
            
            ExerciseLinkApiException apiEx when apiEx.StatusCode == HttpStatusCode.TooManyRequests => 
                "Too many requests. Please wait a moment before trying again.",
            
            ExerciseLinkApiException apiEx when apiEx.StatusCode >= HttpStatusCode.InternalServerError => 
                "A server error occurred. If this problem persists, please contact support.",
            
            // Network exceptions
            HttpRequestException => 
                "Unable to connect to the server. Please check your internet connection and try again.",
            
            TaskCanceledException => 
                "The operation was cancelled or timed out. Please try again.",
            
            // Generic fallback
            _ => "An unexpected error occurred. Please try again or contact support if the problem persists."
        };
    }
    
    /// <summary>
    /// Gets a retry suggestion based on the exception type
    /// </summary>
    public static string? GetRetrySuggestion(Exception exception)
    {
        return exception switch
        {
            ExerciseLinkApiException apiEx when apiEx.StatusCode == HttpStatusCode.ServiceUnavailable => 
                "The service should be back soon. Try again in 30 seconds.",
            
            HttpRequestException => 
                "Check your internet connection and try again.",
            
            TaskCanceledException => 
                "The operation timed out. Try again with a stable connection.",
            
            ExerciseLinkNotFoundException => 
                "The page will refresh to show the current state.",
            
            _ => null
        };
    }
    
    /// <summary>
    /// Determines if the error is retryable
    /// </summary>
    public static bool IsRetryable(Exception exception)
    {
        return exception switch
        {
            ExerciseLinkApiException apiEx => apiEx.StatusCode == HttpStatusCode.ServiceUnavailable ||
                                             apiEx.StatusCode == HttpStatusCode.RequestTimeout ||
                                             apiEx.StatusCode == HttpStatusCode.TooManyRequests ||
                                             apiEx.StatusCode >= HttpStatusCode.InternalServerError,
            HttpRequestException => true,
            TaskCanceledException => true,
            _ => false
        };
    }
}