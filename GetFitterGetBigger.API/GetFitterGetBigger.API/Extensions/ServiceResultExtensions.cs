using GetFitterGetBigger.API.Services.Results;
using Microsoft.AspNetCore.Mvc;

namespace GetFitterGetBigger.API.Extensions;

/// <summary>
/// Extension methods for ServiceResult to simplify controller responses
/// </summary>
public static class ServiceResultExtensions
{
    /// <summary>
    /// Converts a ServiceResult to an appropriate IActionResult for creation operations
    /// </summary>
    public static IActionResult ToCreatedResult<T>(
        this ServiceResult<T> result, 
        string actionName, 
        Func<T, object> routeValuesSelector,
        ControllerBase controller)
    {
        return result switch
        {
            { IsSuccess: true } => controller.CreatedAtAction(
                actionName, 
                routeValuesSelector(result.Data), 
                result.Data),
            { Errors: var errors } when errors.Any(e => e.Contains("already exists")) => 
                controller.Conflict(new { errors }),
            { Errors: var errors } => 
                controller.BadRequest(new { errors })
        };
    }
    
    /// <summary>
    /// Converts a ServiceResult to an appropriate IActionResult for update operations
    /// </summary>
    public static IActionResult ToUpdateResult<T>(this ServiceResult<T> result, ControllerBase controller)
    {
        return result switch
        {
            { IsSuccess: true } => controller.Ok(result.Data),
            { Errors: var errors } when errors.Any(e => e.Contains("not found")) => 
                controller.NotFound(new { errors }),
            { Errors: var errors } when errors.Any(e => e.Contains("already exists")) => 
                controller.Conflict(new { errors }),
            { Errors: var errors } => 
                controller.BadRequest(new { errors })
        };
    }
    
    /// <summary>
    /// Converts a ServiceResult to an appropriate IActionResult with custom error mapping
    /// </summary>
    public static IActionResult ToActionResult<T>(
        this ServiceResult<T> result, 
        ControllerBase controller,
        Func<T, IActionResult> successHandler,
        params (Func<string, bool> predicate, Func<object, IActionResult> handler)[] errorHandlers)
    {
        if (result.IsSuccess)
            return successHandler(result.Data);
        
        var errors = new { errors = result.Errors };
        
        foreach (var (predicate, handler) in errorHandlers)
        {
            if (result.Errors.Any(predicate))
                return handler(errors);
        }
        
        return controller.BadRequest(errors);
    }
}