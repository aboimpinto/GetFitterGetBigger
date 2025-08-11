using GetFitterGetBigger.API.Services.Results;
using Microsoft.AspNetCore.Mvc;

namespace GetFitterGetBigger.API.Extensions;

/// <summary>
/// Extension methods for ServiceResult to simplify controller responses
/// </summary>
public static class ServiceResultExtensions
{
    /// <summary>
    /// Gets a combined error message from all errors in the result
    /// </summary>
    /// <param name="result">The service result</param>
    /// <param name="separator">The separator between error messages (default: " | ")</param>
    /// <returns>A single string with all error messages combined</returns>
    public static string GetCombinedErrorMessage<T>(this ServiceResult<T> result, string separator = " | ")
    {
        if (result.IsSuccess || !result.Errors.Any())
            return string.Empty;

        // Using Aggregate to combine all error messages (Errors is List<string>)
        return result.Errors.Aggregate((current, next) => $"{current}{separator}{next}");
    }

    /// <summary>
    /// Gets the primary error message (first error) or empty string
    /// </summary>
    /// <param name="result">The service result</param>
    /// <returns>The first error message or empty string</returns>
    public static string GetPrimaryErrorMessage<T>(this ServiceResult<T> result)
    {
        return result.Errors.FirstOrDefault() ?? string.Empty;
    }
}