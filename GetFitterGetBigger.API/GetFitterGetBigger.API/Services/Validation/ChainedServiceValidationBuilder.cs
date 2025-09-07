using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Interfaces;
using GetFitterGetBigger.API.Models.Results;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Validation;

/// <summary>
/// A chained validation builder that allows data transformations through a pipeline.
/// Each step only executes if all previous validations pass.
/// </summary>
public class ChainedServiceValidationBuilder<TData, TResult>
{
    private readonly ServiceValidationBuilder<TResult> _innerBuilder;
    private readonly TData _data;
    private readonly bool _isValid;
    private readonly ServiceError? _error;

    internal ChainedServiceValidationBuilder(
        ServiceValidationBuilder<TResult> innerBuilder,
        TData data,
        bool isValid = true,
        ServiceError? error = null)
    {
        _innerBuilder = innerBuilder;
        _data = data;
        _isValid = isValid;
        _error = error;
    }

    /// <summary>
    /// Gets the current data in the pipeline.
    /// </summary>
    public TData Data => _data;

    /// <summary>
    /// Gets whether the chain is still valid.
    /// </summary>
    public bool IsValid => _isValid;

    /// <summary>
    /// Transforms the current data using an EntityResult-returning function.
    /// Only executes if all previous steps were valid.
    /// </summary>
    public ChainedServiceValidationBuilder<TNewData, TResult> ThenCreate<TNewData>(
        Func<TData, EntityResult<TNewData>> transformFunc,
        string operationDescription)
        where TNewData : class, IEmptyEntity<TNewData>
    {
        if (!_isValid)
        {
            // Pass through the error state
            return new ChainedServiceValidationBuilder<TNewData, TResult>(
                _innerBuilder,
                default!,
                false,
                _error);
        }

        var result = transformFunc(_data);
        if (!result.IsSuccess)
        {
            var error = ServiceError.ValidationFailed($"{operationDescription}: {result.FirstError}");
            return new ChainedServiceValidationBuilder<TNewData, TResult>(
                _innerBuilder,
                result.Value,
                false,
                error);
        }

        return new ChainedServiceValidationBuilder<TNewData, TResult>(
            _innerBuilder,
            result.Value,
            true);
    }

    /// <summary>
    /// Performs an async operation on the current data.
    /// The operation doesn't transform the data type.
    /// </summary>
    public async Task<ChainedServiceValidationBuilder<TData, TResult>> ThenPerformAsync(
        Func<TData, Task> operation,
        string operationDescription)
    {
        if (!_isValid)
        {
            return this;
        }

        try
        {
            await operation(_data);
            return this;
        }
        catch (Exception ex)
        {
            var error = ServiceError.ValidationFailed($"{operationDescription} failed: {ex.Message}");
            return new ChainedServiceValidationBuilder<TData, TResult>(
                _innerBuilder,
                _data,
                false,
                error);
        }
    }

    /// <summary>
    /// Transforms the current data asynchronously.
    /// </summary>
    public async Task<ChainedServiceValidationBuilder<TNewData, TResult>> ThenTransformAsync<TNewData>(
        Func<TData, Task<TNewData>> transformFunc,
        string operationDescription)
    {
        if (!_isValid)
        {
            // Pass through the error state
            return new ChainedServiceValidationBuilder<TNewData, TResult>(
                _innerBuilder,
                default!,
                false,
                _error);
        }

        try
        {
            var newData = await transformFunc(_data);
            return new ChainedServiceValidationBuilder<TNewData, TResult>(
                _innerBuilder,
                newData,
                true);
        }
        catch (Exception ex)
        {
            var error = ServiceError.ValidationFailed($"{operationDescription} failed: {ex.Message}");
            return new ChainedServiceValidationBuilder<TNewData, TResult>(
                _innerBuilder,
                default!,
                false,
                error);
        }
    }

    /// <summary>
    /// Completes the chain and returns the final result.
    /// </summary>
    public async Task<ServiceResult<TResult>> MatchAsync(
        Func<TData, ServiceResult<TResult>> whenValid,
        Func<ServiceError, ServiceResult<TResult>> whenInvalid)
    {
        if (!_isValid && _error != null)
        {
            return whenInvalid(_error);
        }

        // Use the inner builder's validation results
        return await _innerBuilder.MatchAsync(
            async () => await Task.FromResult(whenValid(_data)),
            errors => whenInvalid(errors.First()));
    }

    /// <summary>
    /// Completes the chain and returns the final result (async version).
    /// </summary>
    public async Task<ServiceResult<TResult>> MatchAsync(
        Func<TData, Task<ServiceResult<TResult>>> whenValid,
        Func<ServiceError, ServiceResult<TResult>> whenInvalid)
    {
        if (!_isValid && _error != null)
        {
            return whenInvalid(_error);
        }

        // Use the inner builder's validation results
        return await _innerBuilder.MatchAsync(
            async () => await whenValid(_data),
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
    /// </summary>
    public static async Task<ChainedServiceValidationBuilder<TEntity, TResult>> ThenAddAsync<TEntity, TResult>(
        this ChainedServiceValidationBuilder<TEntity, TResult> chain,
        Func<TEntity, Task> addFunc)
    {
        return await chain.ThenPerformAsync(addFunc, "Add to repository");
    }

    /// <summary>
    /// Reloads an entity with full details.
    /// </summary>
    public static async Task<ChainedServiceValidationBuilder<TEntity, TResult>> ThenReloadAsync<TEntity, TResult>(
        this ChainedServiceValidationBuilder<TEntity, TResult> chain,
        Func<TEntity, Task<TEntity>> reloadFunc)
    {
        return await chain.ThenTransformAsync(reloadFunc, "Reload entity");
    }
}