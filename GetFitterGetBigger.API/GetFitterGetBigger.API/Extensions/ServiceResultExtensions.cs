using GetFitterGetBigger.API.Services.Results;
using Microsoft.AspNetCore.Mvc;

namespace GetFitterGetBigger.API.Extensions;

/// <summary>
/// Extension methods for ServiceResult to simplify controller responses
/// </summary>
public static class ServiceResultExtensions
{
    /// <summary>
    /// Converts a ServiceResult to an appropriate IActionResult using error codes
    /// </summary>
    public static IActionResult ToActionResultWithErrorCodes<T>(
        this ServiceResult<T> result,
        ControllerBase controller)
    {
        return result switch
        {
            { IsSuccess: true } => controller.Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => controller.NotFound(new { result.StructuredErrors }),
            { PrimaryErrorCode: ServiceErrorCode.AlreadyExists } => controller.Conflict(new { result.StructuredErrors }),
            { PrimaryErrorCode: ServiceErrorCode.Unauthorized } => controller.Unauthorized(new { result.StructuredErrors }),
            { StructuredErrors: var errors } => controller.BadRequest(new { errors })
        };
    }
    
    /// <summary>
    /// Converts a ServiceResult to an appropriate IActionResult for creation operations using error codes
    /// </summary>
    public static IActionResult ToCreatedResultWithErrorCodes<T>(
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
            { PrimaryErrorCode: ServiceErrorCode.AlreadyExists } => 
                controller.Conflict(new { result.StructuredErrors }),
            { PrimaryErrorCode: ServiceErrorCode.ValidationFailed } => 
                controller.BadRequest(new { result.StructuredErrors }),
            { StructuredErrors: var errors } => 
                controller.BadRequest(new { errors })
        };
    }
    
    /// <summary>
    /// Converts a ServiceResult to an appropriate IActionResult for update operations using error codes
    /// </summary>
    public static IActionResult ToUpdateResultWithErrorCodes<T>(this ServiceResult<T> result, ControllerBase controller)
    {
        return result switch
        {
            { IsSuccess: true } => controller.Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => 
                controller.NotFound(new { result.StructuredErrors }),
            { PrimaryErrorCode: ServiceErrorCode.AlreadyExists } => 
                controller.Conflict(new { result.StructuredErrors }),
            { PrimaryErrorCode: ServiceErrorCode.ConcurrencyConflict } => 
                controller.Conflict(new { result.StructuredErrors }),
            { StructuredErrors: var errors } => 
                controller.BadRequest(new { errors })
        };
    }
    
    /// <summary>
    /// Converts a ServiceResult to an appropriate IActionResult for delete operations using error codes
    /// </summary>
    public static IActionResult ToDeleteResultWithErrorCodes(this ServiceResult<bool> result, ControllerBase controller)
    {
        return result switch
        {
            { IsSuccess: true } => controller.NoContent(),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => 
                controller.NotFound(new { result.StructuredErrors }),
            { PrimaryErrorCode: ServiceErrorCode.DependencyExists } => 
                controller.Conflict(new { result.StructuredErrors }),
            { StructuredErrors: var errors } => 
                controller.BadRequest(new { errors })
        };
    }
    
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