using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Interfaces;
using GetFitterGetBigger.API.Models.Results;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.EntityFrameworkCore;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Validation;

/// <summary>
/// A transactional validation builder that manages UnitOfWork lifecycle and provides
/// automatic transaction rollback on failure. All operations are executed within
/// a single database transaction that commits only when all validations pass.
/// </summary>
public class TransactionalServiceValidationBuilder<TContext, TResult> : IDisposable
    where TContext : DbContext
{
    private readonly IUnitOfWorkProvider<TContext> _unitOfWorkProvider;
    private readonly List<ServiceError> _errors = new();
    private readonly DynamicChainContext _context;
    private IWritableUnitOfWork<TContext>? _unitOfWork;
    private bool _isValid = true;
    private bool _disposed = false;
    
    internal TransactionalServiceValidationBuilder(IUnitOfWorkProvider<TContext> unitOfWorkProvider)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _context = new DynamicChainContext();
        _context.Store("UnitOfWorkProvider", unitOfWorkProvider);
    }
    
    /// <summary>
    /// Gets the context for storing and retrieving data throughout the validation chain
    /// </summary>
    public DynamicChainContext Context => _context;
    
    /// <summary>
    /// Gets or creates the unit of work for this transaction
    /// </summary>
    private IWritableUnitOfWork<TContext> GetUnitOfWork()
    {
        if (_unitOfWork == null)
        {
            _unitOfWork = _unitOfWorkProvider.CreateWritable();
            _context.Store("UnitOfWork", _unitOfWork);
        }
        return _unitOfWork;
    }
    
    /// <summary>
    /// Validates that a value is not null
    /// </summary>
    public TransactionalServiceValidationBuilder<TContext, TResult> EnsureNotNull<T>(
        T value,
        ServiceError error)
        where T : class
    {
        if (_isValid && value == null)
        {
            _errors.Add(error);
            _isValid = false;
        }
        return this;
    }
    
    /// <summary>
    /// Validates a condition and adds error if validation fails
    /// </summary>
    public TransactionalServiceValidationBuilder<TContext, TResult> Ensure(
        Func<bool> predicate,
        ServiceError error)
    {
        if (_isValid && !predicate())
        {
            _errors.Add(error);
            _isValid = false;
        }
        return this;
    }
    
    /// <summary>
    /// Validates a condition and adds error if validation fails.
    /// Creates a ServiceError with ValidationFailed code using the provided error message.
    /// </summary>
    public TransactionalServiceValidationBuilder<TContext, TResult> Ensure(
        Func<bool> predicate,
        string errorMessage)
    {
        if (_isValid && !predicate())
        {
            _errors.Add(ServiceError.ValidationFailed(errorMessage));
            _isValid = false;
        }
        return this;
    }
    
    /// <summary>
    /// Validates a condition asynchronously and adds error if validation fails
    /// </summary>
    public async Task<TransactionalServiceValidationBuilder<TContext, TResult>> EnsureAsync(
        Func<Task<bool>> predicate,
        ServiceError error)
    {
        if (_isValid && !await predicate())
        {
            _errors.Add(error);
            _isValid = false;
        }
        return this;
    }
    
    /// <summary>
    /// Validates a condition asynchronously and adds error if validation fails.
    /// Creates a ServiceError with ValidationFailed code using the provided error message.
    /// </summary>
    public async Task<TransactionalServiceValidationBuilder<TContext, TResult>> EnsureAsync(
        Func<Task<bool>> predicate,
        string errorMessage)
    {
        if (_isValid && !await predicate())
        {
            _errors.Add(ServiceError.ValidationFailed(errorMessage));
            _isValid = false;
        }
        return this;
    }
    
    /// <summary>
    /// Loads an entity from repository and starts a transactional chain
    /// </summary>
    public async Task<TransactionalEntityChain<TContext, TEntity, TResult>> ThenLoadAsync<TEntity, TRepo>(
        Func<TRepo, Task<TEntity>> loadFunc)
        where TEntity : class, IEmpty
        where TRepo : class, IRepository
    {
        if (!_isValid)
        {
            return new TransactionalEntityChain<TContext, TEntity, TResult>(
                this, default!, GetUnitOfWork(), _errors);
        }
        
        var unitOfWork = GetUnitOfWork();
        var repository = unitOfWork.GetRepository<TRepo>();
        var entity = await loadFunc(repository);
        
        return new TransactionalEntityChain<TContext, TEntity, TResult>(
            this, entity, unitOfWork, _errors);
    }
    
    /// <summary>
    /// Executes repository operations and commits the transaction
    /// </summary>
    public async Task<ServiceResult<TResult>> ThenExecuteAsync<TRepo>(
        Func<TRepo, Task<TResult>> executeFunc)
        where TRepo : class, IRepository
    {
        if (!_isValid)
        {
            if (_unitOfWork != null)
                await _unitOfWork.RollbackAsync();
            return ServiceResult<TResult>.Failure(
                default!,
                _errors.FirstOrDefault() ?? ServiceError.ValidationFailed("Validation failed"));
        }
        
        try
        {
            var unitOfWork = GetUnitOfWork();
            var repository = unitOfWork.GetRepository<TRepo>();
            var result = await executeFunc(repository);
            await unitOfWork.CommitAsync();
            return ServiceResult<TResult>.Success(result);
        }
        catch (Exception ex)
        {
            if (_unitOfWork != null)
                await _unitOfWork.RollbackAsync();
            _errors.Add(ServiceError.InternalError($"Operation failed: {ex.Message}"));
            return ServiceResult<TResult>.Failure(
                default!,
                _errors.FirstOrDefault() ?? ServiceError.InternalError("Operation failed"));
        }
        finally
        {
            Dispose();
        }
    }
    
    /// <summary>
    /// Executes complex operations with access to the unit of work
    /// </summary>
    public async Task<ServiceResult<TResult>> ThenExecuteWithUnitOfWorkAsync(
        Func<IWritableUnitOfWork<TContext>, Task<TResult>> executeFunc)
    {
        if (!_isValid)
        {
            if (_unitOfWork != null)
                await _unitOfWork.RollbackAsync();
            return ServiceResult<TResult>.Failure(
                default!,
                _errors.FirstOrDefault() ?? ServiceError.ValidationFailed("Validation failed"));
        }
        
        try
        {
            var unitOfWork = GetUnitOfWork();
            var result = await executeFunc(unitOfWork);
            await unitOfWork.CommitAsync();
            return ServiceResult<TResult>.Success(result);
        }
        catch (Exception ex)
        {
            if (_unitOfWork != null)
                await _unitOfWork.RollbackAsync();
            _errors.Add(ServiceError.InternalError($"Operation failed: {ex.Message}"));
            return ServiceResult<TResult>.Failure(
                default!,
                _errors.FirstOrDefault() ?? ServiceError.InternalError("Operation failed"));
        }
        finally
        {
            Dispose();
        }
    }
    
    /// <summary>
    /// Executes the transaction with proper commit/rollback handling
    /// </summary>
    public async Task<ServiceResult<TResult>> MatchAsync(
        Func<Task<ServiceResult<TResult>>> whenValid,
        Func<IReadOnlyList<ServiceError>, ServiceResult<TResult>> whenInvalid)
    {
        try
        {
            if (!_isValid)
            {
                // Rollback if we have an active unit of work
                if (_unitOfWork != null)
                {
                    await _unitOfWork.RollbackAsync();
                }
                return whenInvalid(_errors);
            }
            
            var result = await whenValid();
            
            if (_unitOfWork != null)
            {
                if (result.IsSuccess)
                {
                    await _unitOfWork.CommitAsync();
                }
                else
                {
                    await _unitOfWork.RollbackAsync();
                }
            }
            
            return result;
        }
        catch (Exception ex)
        {
            // Rollback on exception
            if (_unitOfWork != null)
            {
                await _unitOfWork.RollbackAsync();
            }
            
            // Log exception if needed
            _errors.Add(ServiceError.InternalError($"Transaction failed: {ex.Message}"));
            return whenInvalid(_errors);
        }
        finally
        {
            Dispose();
        }
    }
    
    /// <summary>
    /// Loads data asynchronously into the context
    /// </summary>
    public async Task<TransactionalServiceValidationBuilder<TContext, TResult>> ThenLoadAsync(
        string storeAs,
        Func<DynamicChainContext, Task<object>> loadFunc)
    {
        if (!_isValid)
            return this;
        
        try
        {
            var result = await loadFunc(_context);
            _context.Store(storeAs, result);
            return this;
        }
        catch (Exception ex)
        {
            _errors.Add(ServiceError.InternalError($"Load operation failed: {ex.Message}"));
            _isValid = false;
            return this;
        }
    }
    
    /// <summary>
    /// Performs an async action with the context
    /// </summary>
    public async Task<TransactionalServiceValidationBuilder<TContext, TResult>> ThenPerformAsync(
        Func<DynamicChainContext, Task> action)
    {
        if (!_isValid)
            return this;
        
        try
        {
            await action(_context);
            return this;
        }
        catch (Exception ex)
        {
            _errors.Add(ServiceError.InternalError($"Perform operation failed: {ex.Message}"));
            _isValid = false;
            return this;
        }
    }
    
    /// <summary>
    /// Conditionally performs an async action with the context
    /// </summary>
    public async Task<TransactionalServiceValidationBuilder<TContext, TResult>> ThenPerformIfAsync(
        Func<DynamicChainContext, bool> condition,
        Func<DynamicChainContext, Task> action)
    {
        if (!_isValid)
            return this;
        
        try
        {
            if (condition(_context))
            {
                await action(_context);
            }
            return this;
        }
        catch (Exception ex)
        {
            _errors.Add(ServiceError.InternalError($"Conditional perform operation failed: {ex.Message}"));
            _isValid = false;
            return this;
        }
    }
    
    /// <summary>
    /// Ensures a condition is met asynchronously with the context
    /// </summary>
    public async Task<TransactionalServiceValidationBuilder<TContext, TResult>> ThenEnsureAsync(
        Func<DynamicChainContext, Task<bool>> predicate,
        ServiceError error)
    {
        if (!_isValid)
            return this;
        
        try
        {
            if (!await predicate(_context))
            {
                _errors.Add(error);
                _isValid = false;
            }
            return this;
        }
        catch (Exception ex)
        {
            _errors.Add(ServiceError.InternalError($"Ensure operation failed: {ex.Message}"));
            _isValid = false;
            return this;
        }
    }
    
    /// <summary>
    /// Ensures a condition is met asynchronously with the context.
    /// Creates a ServiceError with ValidationFailed code using the provided error message.
    /// </summary>
    public async Task<TransactionalServiceValidationBuilder<TContext, TResult>> ThenEnsureAsync(
        Func<DynamicChainContext, Task<bool>> predicate,
        string errorMessage)
    {
        if (!_isValid)
            return this;
        
        try
        {
            if (!await predicate(_context))
            {
                _errors.Add(ServiceError.ValidationFailed(errorMessage));
                _isValid = false;
            }
            return this;
        }
        catch (Exception ex)
        {
            _errors.Add(ServiceError.InternalError($"Ensure operation failed: {ex.Message}"));
            _isValid = false;
            return this;
        }
    }
    
    /// <summary>
    /// Executes a function with the context and returns the result
    /// </summary>
    public async Task<ServiceResult<TResult>> ThenExecuteAsync(
        Func<DynamicChainContext, Task<TResult>> executeFunc)
    {
        if (!_isValid)
        {
            if (_unitOfWork != null)
                await _unitOfWork.RollbackAsync();
            return ServiceResult<TResult>.Failure(
                default!,
                _errors.FirstOrDefault() ?? ServiceError.ValidationFailed("Validation failed"));
        }
        
        try
        {
            var result = await executeFunc(_context);
            if (_unitOfWork != null)
                await _unitOfWork.CommitAsync();
            return ServiceResult<TResult>.Success(result);
        }
        catch (Exception ex)
        {
            if (_unitOfWork != null)
                await _unitOfWork.RollbackAsync();
            _errors.Add(ServiceError.InternalError($"Execute operation failed: {ex.Message}"));
            return ServiceResult<TResult>.Failure(
                default!,
                _errors.FirstOrDefault() ?? ServiceError.InternalError("Execute operation failed"));
        }
        finally
        {
            Dispose();
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _unitOfWork?.Dispose();
            _context?.Dispose();
            _disposed = true;
        }
    }
}

/// <summary>
/// Represents a chain of operations on an entity within a transaction
/// </summary>
public class TransactionalEntityChain<TContext, TEntity, TResult>
    where TContext : DbContext
    where TEntity : class
{
    private readonly TransactionalServiceValidationBuilder<TContext, TResult> _parent;
    private readonly IWritableUnitOfWork<TContext> _unitOfWork;
    private readonly List<ServiceError> _errors;
    private readonly TEntity _entity;
    private bool _isValid;
    
    internal TransactionalEntityChain(
        TransactionalServiceValidationBuilder<TContext, TResult> parent,
        TEntity entity,
        IWritableUnitOfWork<TContext> unitOfWork,
        List<ServiceError> errors)
    {
        _parent = parent;
        _entity = entity;
        _unitOfWork = unitOfWork;
        _errors = errors;
        _isValid = errors.Count == 0;
    }
    
    /// <summary>
    /// Ensures the entity is not empty
    /// </summary>
    public TransactionalEntityChain<TContext, TEntity, TResult> ThenEnsureNotEmpty(
        ServiceError error)
    {
        if (_isValid && (_entity == null || (_entity as IEmpty)?.IsEmpty == true))
        {
            _errors.Add(error);
            _isValid = false;
        }
        return this;
    }
    
    /// <summary>
    /// Validates the entity asynchronously
    /// </summary>
    public async Task<TransactionalEntityChain<TContext, TEntity, TResult>> ThenEnsureAsync(
        Func<TEntity, Task<bool>> predicate,
        ServiceError error)
    {
        if (_isValid && !await predicate(_entity))
        {
            _errors.Add(error);
            _isValid = false;
        }
        return this;
    }
    
    /// <summary>
    /// Validates the entity asynchronously.
    /// Creates a ServiceError with ValidationFailed code using the provided error message.
    /// </summary>
    public async Task<TransactionalEntityChain<TContext, TEntity, TResult>> ThenEnsureAsync(
        Func<TEntity, Task<bool>> predicate,
        string errorMessage)
    {
        if (_isValid && !await predicate(_entity))
        {
            _errors.Add(ServiceError.ValidationFailed(errorMessage));
            _isValid = false;
        }
        return this;
    }
    
    /// <summary>
    /// Transforms the entity to a new type
    /// </summary>
    public TransactionalEntityChain<TContext, TNewEntity, TResult> ThenTransform<TNewEntity>(
        Func<TEntity, EntityResult<TNewEntity>> transformFunc,
        string operationDescription)
        where TNewEntity : class, IEmptyEntity<TNewEntity>
    {
        if (!_isValid)
        {
            return new TransactionalEntityChain<TContext, TNewEntity, TResult>(
                _parent, default!, _unitOfWork, _errors);
        }
        
        var result = transformFunc(_entity);
        if (!result.IsSuccess)
        {
            _errors.Add(ServiceError.ValidationFailed($"{operationDescription}: {result.FirstError}"));
            _isValid = false;
            return new TransactionalEntityChain<TContext, TNewEntity, TResult>(
                _parent, default!, _unitOfWork, _errors);
        }
        
        return new TransactionalEntityChain<TContext, TNewEntity, TResult>(
            _parent, result.Value, _unitOfWork, _errors);
    }
    
    
    /// <summary>
    /// Performs an operation on the entity within the repository
    /// </summary>
    public async Task<TransactionalEntityChain<TContext, TEntity, TResult>> ThenPerformAsync<TRepo>(
        Func<TRepo, TEntity, Task> operation,
        string operationDescription)
        where TRepo : class, IRepository
    {
        if (_isValid)
        {
            try
            {
                var repository = _unitOfWork.GetRepository<TRepo>();
                await operation(repository, _entity);
            }
            catch (Exception ex)
            {
                _errors.Add(ServiceError.ValidationFailed($"{operationDescription} failed: {ex.Message}"));
                _isValid = false;
            }
        }
        return this;
    }
    
    /// <summary>
    /// Transforms the entity and performs repository operations based on the result
    /// </summary>
    public async Task<TransactionalEntityChain<TContext, TEntity, TResult>> ThenTransformAndUpdateAsync<TRepo, TTransformEntity>(
        Func<TRepo, TEntity, Task<EntityResult<TTransformEntity>>> transformFunc,
        Func<TRepo, TTransformEntity, Task> updateFunc,
        string operationDescription)
        where TRepo : class, IRepository
        where TTransformEntity : class, IEmptyEntity<TTransformEntity>
    {
        if (!_isValid)
        {
            return this;
        }
        
        var repository = _unitOfWork.GetRepository<TRepo>();
        var result = await transformFunc(repository, _entity);
        
        if (!result.IsSuccess)
        {
            _errors.Add(ServiceError.ValidationFailed($"{operationDescription}: {result.FirstError}"));
            _isValid = false;
            return this;
        }
        
        // Perform the update in the repository with the transformed entity
        await updateFunc(repository, result.Value);
        
        // Note: The entity in the chain remains the original, but it has been updated in the repository.
        // The reload operation later in the chain will fetch the updated version.
        
        return this;
    }
    
    /// <summary>
    /// Saves the entity to the repository
    /// </summary>
    public async Task<TransactionalEntityChain<TContext, TEntity, TResult>> ThenSaveAsync<TRepo>(
        Func<TRepo, TEntity, Task> saveFunc)
        where TRepo : class, IRepository
    {
        if (_isValid)
        {
            var repository = _unitOfWork.GetRepository<TRepo>();
            await saveFunc(repository, _entity);
        }
        return this;
    }
    
    /// <summary>
    /// Reloads the entity with additional details
    /// </summary>
    public async Task<TransactionalEntityChain<TContext, TNewEntity, TResult>> ThenReloadAsync<TRepo, TNewEntity>(
        Func<TRepo, TEntity, Task<TNewEntity>> reloadFunc)
        where TRepo : class, IRepository
        where TNewEntity : class
    {
        if (!_isValid)
        {
            return new TransactionalEntityChain<TContext, TNewEntity, TResult>(
                _parent, default!, _unitOfWork, _errors);
        }
        
        var repository = _unitOfWork.GetRepository<TRepo>();
        var reloaded = await reloadFunc(repository, _entity);
        
        return new TransactionalEntityChain<TContext, TNewEntity, TResult>(
            _parent, reloaded, _unitOfWork, _errors);
    }
    
    /// <summary>
    /// Commits the transaction and returns the final result
    /// </summary>
    public async Task<ServiceResult<TResult>> ThenCommitAsync(
        Func<TEntity, TResult> mapToResult)
    {
        if (!_isValid)
        {
            // Rollback the transaction since validation failed (if it was started)
            if (_unitOfWork != null)
                await _unitOfWork.RollbackAsync();
            
            return ServiceResult<TResult>.Failure(
                default!,
                _errors.FirstOrDefault() ?? ServiceError.ValidationFailed("Transaction failed"));
        }
        
        await _unitOfWork.CommitAsync();
        return ServiceResult<TResult>.Success(mapToResult(_entity));
    }
    
    /// <summary>
    /// Returns to the parent builder for final matching
    /// </summary>
    public TransactionalServiceValidationBuilder<TContext, TResult> EndChain()
    {
        return _parent;
    }
}