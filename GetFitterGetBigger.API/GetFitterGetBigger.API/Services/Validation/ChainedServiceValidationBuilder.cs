using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Interfaces;
using GetFitterGetBigger.API.Models.Results;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Validation;

/// <summary>
/// A chained validation builder that allows data transformations through a pipeline.
/// All operations are deferred until MatchAsync is called to maintain fluent interface.
/// This allows mixing sync and async operations in a single fluent chain.
/// </summary>
public class ChainedServiceValidationBuilder<TData, TResult>
{
    private readonly ServiceValidationBuilder<TResult> _innerBuilder;
    private readonly Func<Task<(object data, bool isValid, ServiceError? error)>> _pipeline;
    
    internal ChainedServiceValidationBuilder(
        ServiceValidationBuilder<TResult> innerBuilder,
        TData data,
        bool isValid = true,
        ServiceError? error = null)
    {
        _innerBuilder = innerBuilder;
        _pipeline = () => Task.FromResult((data as object, isValid, error));
    }
    
    private ChainedServiceValidationBuilder(
        ServiceValidationBuilder<TResult> innerBuilder,
        Func<Task<(object data, bool isValid, ServiceError? error)>> pipeline)
    {
        _innerBuilder = innerBuilder;
        _pipeline = pipeline;
    }
    
    /// <summary>
    /// Transforms the current data using an EntityResult-returning function.
    /// The operation is deferred until MatchAsync is called.
    /// </summary>
    public ChainedServiceValidationBuilder<TNewData, TResult> ThenCreate<TNewData>(
        Func<TData, EntityResult<TNewData>> transformFunc,
        string operationDescription)
        where TNewData : class, IEmptyEntity<TNewData>
    {
        async Task<(object data, bool isValid, ServiceError? error)> NewPipeline()
        {
            var (prevData, prevValid, prevError) = await _pipeline();
            
            if (!prevValid)
            {
                return (default(TNewData)!, false, prevError);
            }
            
            var result = transformFunc((TData)prevData);
            if (!result.IsSuccess)
            {
                var error = ServiceError.ValidationFailed($"{operationDescription}: {result.FirstError}");
                return (result.Value, false, error);
            }
            
            return (result.Value, true, null);
        }
        
        return new ChainedServiceValidationBuilder<TNewData, TResult>(_innerBuilder, NewPipeline);
    }
    
    /// <summary>
    /// Performs an async operation on the current data.
    /// The operation is deferred until MatchAsync is called.
    /// </summary>
    public ChainedServiceValidationBuilder<TData, TResult> ThenPerformAsync(
        Func<TData, Task> operation,
        string operationDescription)
    {
        async Task<(object data, bool isValid, ServiceError? error)> NewPipeline()
        {
            var (prevData, prevValid, prevError) = await _pipeline();
            
            if (!prevValid)
            {
                return (prevData, false, prevError);
            }
            
            try
            {
                await operation((TData)prevData);
                return (prevData, true, null);
            }
            catch (Exception ex)
            {
                var error = ServiceError.ValidationFailed($"{operationDescription} failed: {ex.Message}");
                return (prevData, false, error);
            }
        }
        
        return new ChainedServiceValidationBuilder<TData, TResult>(_innerBuilder, NewPipeline);
    }
    
    /// <summary>
    /// Transforms the current data asynchronously.
    /// The operation is deferred until MatchAsync is called.
    /// </summary>
    public ChainedServiceValidationBuilder<TNewData, TResult> ThenTransformAsync<TNewData>(
        Func<TData, Task<TNewData>> transformFunc,
        string operationDescription)
    {
        async Task<(object data, bool isValid, ServiceError? error)> NewPipeline()
        {
            var (prevData, prevValid, prevError) = await _pipeline();
            
            if (!prevValid)
            {
                return (default(TNewData)!, false, prevError);
            }
            
            try
            {
                var newData = await transformFunc((TData)prevData);
                return (newData!, true, null);
            }
            catch (Exception ex)
            {
                var error = ServiceError.ValidationFailed($"{operationDescription} failed: {ex.Message}");
                return (default(TNewData)!, false, error);
            }
        }
        
        return new ChainedServiceValidationBuilder<TNewData, TResult>(_innerBuilder, NewPipeline);
    }
    
    /// <summary>
    /// Executes the entire pipeline and returns the final result.
    /// </summary>
    public async Task<ServiceResult<TResult>> MatchAsync(
        Func<TData, ServiceResult<TResult>> whenValid,
        Func<ServiceError, ServiceResult<TResult>> whenInvalid)
    {
        var (data, isValid, error) = await _pipeline();
        
        if (!isValid && error != null)
        {
            return whenInvalid(error);
        }
        
        // Use the inner builder's validation results
        return await _innerBuilder.MatchAsync(
            async () => await Task.FromResult(whenValid((TData)data)),
            errors => whenInvalid(errors.First()));
    }
    
    /// <summary>
    /// Executes the entire pipeline and returns the final result (async version).
    /// </summary>
    public async Task<ServiceResult<TResult>> MatchAsync(
        Func<TData, Task<ServiceResult<TResult>>> whenValid,
        Func<ServiceError, ServiceResult<TResult>> whenInvalid)
    {
        var (data, isValid, error) = await _pipeline();
        
        if (!isValid && error != null)
        {
            return whenInvalid(error);
        }
        
        // Use the inner builder's validation results
        return await _innerBuilder.MatchAsync(
            async () => await whenValid((TData)data),
            errors => whenInvalid(errors.First()));
    }
}

/// <summary>
/// Extension methods to integrate chained validation with ServiceValidationBuilder.
/// </summary>
public static class ChainedValidationExtensions
{
    /// <summary>
    /// Validates that an entity is not empty and starts a chain with that entity.
    /// </summary>
    public static ChainedServiceValidationBuilder<TEntity, TResult> EnsureNotEmpty<TResult, TEntity>(
        this ServiceValidationBuilder<TResult> builder,
        TEntity entity,
        ServiceError error)
        where TEntity : class, IEmpty
    {
        if (entity.IsEmpty)
        {
            return new ChainedServiceValidationBuilder<TEntity, TResult>(
                builder,
                entity,
                false,
                error);
        }

        // Add the validation to the inner builder
        builder.Ensure(() => !entity.IsEmpty, error);
        
        return new ChainedServiceValidationBuilder<TEntity, TResult>(
            builder,
            entity,
            true);
    }

    /// <summary>
    /// Creates a duplicate entity using the Handler pattern.
    /// Alias for ThenCreate to make intent clearer.
    /// </summary>
    public static ChainedServiceValidationBuilder<TNewEntity, TResult> ThenCreateDuplicate<TData, TResult, TNewEntity>(
        this ChainedServiceValidationBuilder<TData, TResult> chain,
        Func<TData, EntityResult<TNewEntity>> createFunc)
        where TNewEntity : class, IEmptyEntity<TNewEntity>
    {
        return chain.ThenCreate(createFunc, "Create duplicate");
    }

    /// <summary>
    /// Adds an entity to the repository.
    /// Deferred execution - returns immediately, executes in MatchAsync.
    /// </summary>
    public static ChainedServiceValidationBuilder<TEntity, TResult> ThenAddAsync<TEntity, TResult>(
        this ChainedServiceValidationBuilder<TEntity, TResult> chain,
        Func<TEntity, Task> addFunc)
    {
        return chain.ThenPerformAsync(addFunc, "Add to repository");
    }

    /// <summary>
    /// Reloads an entity with full details.
    /// Deferred execution - returns immediately, executes in MatchAsync.
    /// </summary>
    public static ChainedServiceValidationBuilder<TEntity, TResult> ThenReloadAsync<TEntity, TResult>(
        this ChainedServiceValidationBuilder<TEntity, TResult> chain,
        Func<TEntity, Task<TEntity>> reloadFunc)
    {
        return chain.ThenTransformAsync(reloadFunc, "Reload entity");
    }
}