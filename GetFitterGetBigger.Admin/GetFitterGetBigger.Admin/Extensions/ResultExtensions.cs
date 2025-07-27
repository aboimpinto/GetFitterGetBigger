using GetFitterGetBigger.Admin.Models.Errors;
using GetFitterGetBigger.Admin.Models.Results;
using GetFitterGetBigger.Admin.Services;
using Microsoft.AspNetCore.Components;

namespace GetFitterGetBigger.Admin.Extensions;

public static class ResultExtensions
{
    /// <summary>
    /// Converts a DataServiceResult to ServiceResult by mapping errors.
    /// </summary>
    public static ServiceResult<T> ToServiceResult<T>(
        this DataServiceResult<T> source,
        Func<DataError, ServiceError>? errorMapper = null)
    {
        if (source.IsSuccess)
        {
            return ServiceResult<T>.Success(source.Data!);
        }
        
        errorMapper ??= DefaultDataErrorToServiceError;
        
        var serviceErrors = source.Errors.Select(errorMapper).ToArray();
        return ServiceResult<T>.Failure(serviceErrors);
    }
    
    private static ServiceError DefaultDataErrorToServiceError(DataError dataError)
    {
        return dataError.Code switch
        {
            DataErrorCode.NotFound => new ServiceError(ServiceErrorCode.TemplateNotFound, dataError.Message),
            DataErrorCode.Conflict => new ServiceError(ServiceErrorCode.DuplicateName, dataError.Message),
            DataErrorCode.Unauthorized => new ServiceError(ServiceErrorCode.InsufficientPermissions, dataError.Message),
            DataErrorCode.Forbidden => new ServiceError(ServiceErrorCode.InsufficientPermissions, dataError.Message),
            _ => new ServiceError(ServiceErrorCode.DependencyFailure, dataError.Message)
        };
    }

    /// <summary>
    /// Handles a result in a Blazor component with automatic error display via toast.
    /// </summary>
    public static async Task HandleResultAsync<T, TResult, TError>(
        this TResult result,
        Action<T> onSuccess,
        IToastService toastService,
        ILogger? logger = null,
        Action? stateHasChanged = null)
        where TResult : ResultBase<T, TResult, TError>, new()
        where TError : IErrorDetail
    {
        await Task.Run(() =>
        {
            result.Match(
                onSuccess: data =>
                {
                    onSuccess(data);
                    stateHasChanged?.Invoke();
                    return true;
                },
                onFailure: errors =>
                {
                    var error = errors.FirstOrDefault();
                    var message = error?.Message ?? "Operation failed";
                    
                    toastService.ShowError(message);
                    logger?.LogWarning("Operation failed: {@Errors}", errors);
                    
                    return false;
                }
            );
        });
    }

    /// <summary>
    /// Handles a result in a Blazor component with custom error handling.
    /// </summary>
    public static void HandleResult<T, TResult, TError>(
        this TResult result,
        Action<T> onSuccess,
        Action<IReadOnlyList<TError>> onFailure,
        Action? stateHasChanged = null)
        where TResult : ResultBase<T, TResult, TError>, new()
        where TError : IErrorDetail
    {
        result.Match(
            onSuccess: data =>
            {
                onSuccess(data);
                stateHasChanged?.Invoke();
                return true;
            },
            onFailure: errors =>
            {
                onFailure(errors);
                return false;
            }
        );
    }

    /// <summary>
    /// Maps a result to a different type while preserving error information.
    /// </summary>
    public static async Task<TResult> MapResultAsync<TSource, TTarget, TResult, TError>(
        this Task<TResult> resultTask,
        Func<TSource, TTarget> mapper)
        where TResult : ResultBase<TSource, TResult, TError>, new()
        where TError : IErrorDetail
    {
        var result = await resultTask;
        
        if (result.IsSuccess)
        {
            var mappedData = mapper(result.Data!);
            // This is a limitation - we can't create a result of a different type
            // We'd need a more complex generic system for this
            throw new NotImplementedException("Cross-type mapping not yet implemented");
        }
        
        return result;
    }

    /// <summary>
    /// Provides a default value if the result is a failure.
    /// </summary>
    public static T GetOrDefault<T, TResult, TError>(
        this TResult result,
        T defaultValue)
        where TResult : ResultBase<T, TResult, TError>, new()
        where TError : IErrorDetail
    {
        return result.IsSuccess ? result.Data! : defaultValue;
    }

    /// <summary>
    /// Throws an exception if the result is a failure.
    /// </summary>
    public static T GetOrThrow<T, TResult, TError>(
        this TResult result,
        Func<IReadOnlyList<TError>, Exception>? exceptionFactory = null)
        where TResult : ResultBase<T, TResult, TError>, new()
        where TError : IErrorDetail
    {
        if (result.IsSuccess)
        {
            return result.Data!;
        }

        if (exceptionFactory != null)
        {
            throw exceptionFactory(result.Errors);
        }

        var error = result.Errors.FirstOrDefault();
        throw new InvalidOperationException(error?.Message ?? "Operation failed");
    }

    /// <summary>
    /// Chains multiple async operations, short-circuiting on first failure.
    /// </summary>
    public static async Task<TResult> ThenAsync<T, TResult, TError>(
        this Task<TResult> resultTask,
        Func<T, Task<TResult>> nextOperation)
        where TResult : ResultBase<T, TResult, TError>, new()
        where TError : IErrorDetail
    {
        var result = await resultTask;
        
        if (result.IsSuccess)
        {
            return await nextOperation(result.Data!);
        }
        
        return result;
    }

    /// <summary>
    /// Executes a side effect if the result is successful.
    /// </summary>
    public static TResult Tap<T, TResult, TError>(
        this TResult result,
        Action<T> action)
        where TResult : ResultBase<T, TResult, TError>, new()
        where TError : IErrorDetail
    {
        if (result.IsSuccess)
        {
            action(result.Data!);
        }
        
        return result;
    }

    /// <summary>
    /// Executes a side effect if the result is a failure.
    /// </summary>
    public static TResult TapError<T, TResult, TError>(
        this TResult result,
        Action<IReadOnlyList<TError>> action)
        where TResult : ResultBase<T, TResult, TError>, new()
        where TError : IErrorDetail
    {
        if (!result.IsSuccess)
        {
            action(result.Errors);
        }
        
        return result;
    }

    /// <summary>
    /// Logs the result (success or failure) and returns it unchanged.
    /// </summary>
    public static TResult Log<T, TResult, TError>(
        this TResult result,
        ILogger logger,
        string successMessage = "Operation completed successfully",
        LogLevel successLevel = LogLevel.Information,
        LogLevel errorLevel = LogLevel.Error)
        where TResult : ResultBase<T, TResult, TError>, new()
        where TError : IErrorDetail
    {
        if (result.IsSuccess)
        {
            logger.Log(successLevel, successMessage);
        }
        else
        {
            logger.Log(errorLevel, "Operation failed: {@Errors}", result.Errors);
        }
        
        return result;
    }
}