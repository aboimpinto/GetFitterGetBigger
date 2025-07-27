using GetFitterGetBigger.Admin.Models.Errors;

namespace GetFitterGetBigger.Admin.Models.Results;

public abstract class ResultBase<T, TResult, TError> 
    where TResult : ResultBase<T, TResult, TError>, new()
    where TError : IErrorDetail
{
    public bool IsSuccess { get; protected init; }
    public T? Data { get; protected init; }
    public IReadOnlyList<TError> Errors { get; protected init; } = Array.Empty<TError>();

    public static TResult Success(T data) => new() 
    { 
        IsSuccess = true, 
        Data = data 
    };

    public static TResult Failure(params TError[] errors) => new() 
    { 
        IsSuccess = false, 
        Errors = errors 
    };

    public TReturn Match<TReturn>(
        Func<T, TReturn> onSuccess,
        Func<IReadOnlyList<TError>, TReturn> onFailure)
    {
        return IsSuccess ? onSuccess(Data!) : onFailure(Errors);
    }

    public bool HasError(Enum code)
    {
        return Errors.Any(e => e.Code.Equals(code));
    }

    public async Task<TResultNext> ThenAsync<TNext, TResultNext, TErrorNext>(
        Func<T, Task<TResultNext>> next) 
        where TResultNext : ResultBase<TNext, TResultNext, TErrorNext>, new()
        where TErrorNext : IErrorDetail
    {
        if (IsSuccess)
        {
            return await next(Data!);
        }

        // If we can't transform errors, we need to handle this differently
        // This is a simplified version - in practice, you'd want error transformation
        throw new InvalidOperationException("Error transformation not implemented");
    }

    public TResult Map<TNew>(Func<T, TNew> mapper) where TNew : T
    {
        if (IsSuccess)
        {
            var newResult = new TResult();
            var type = newResult.GetType();
            type.GetProperty(nameof(IsSuccess))!.SetValue(newResult, true);
            type.GetProperty(nameof(Data))!.SetValue(newResult, mapper(Data!));
            return newResult;
        }
        return (TResult)this;
    }

    public async Task<TResult> MapAsync<TNew>(Func<T, Task<TNew>> mapper) where TNew : T
    {
        if (IsSuccess)
        {
            var newData = await mapper(Data!);
            var newResult = new TResult();
            var type = newResult.GetType();
            type.GetProperty(nameof(IsSuccess))!.SetValue(newResult, true);
            type.GetProperty(nameof(Data))!.SetValue(newResult, newData);
            return newResult;
        }
        return (TResult)this;
    }

    public TResult Then(Action<T> action)
    {
        if (IsSuccess)
        {
            action(Data!);
        }
        return (TResult)this;
    }

    public async Task<TResult> ThenAsync(Func<T, Task> action)
    {
        if (IsSuccess)
        {
            await action(Data!);
        }
        return (TResult)this;
    }
}