using GetFitterGetBigger.API.Models.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.EntityFrameworkCore;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Validation;

/// <summary>
/// Extension methods for TransactionalServiceValidationBuilder that leverage DynamicChainContext
/// for repository management and data flow through the validation chain.
/// </summary>
public static class TransactionalChainExtensions
{
    /// <summary>
    /// Creates a read-only repository and stores it in the context with automatic naming.
    /// The repository can be retrieved using context.GetRepository&lt;T&gt;(isReadOnly: true)
    /// </summary>
    /// <typeparam name="TContext">The DbContext type</typeparam>
    /// <typeparam name="TResult">The result type of the chain</typeparam>
    /// <typeparam name="TRepo">The repository interface type</typeparam>
    /// <param name="builder">The validation builder</param>
    /// <param name="customKey">Optional custom key for storing the repository</param>
    /// <returns>The builder for method chaining</returns>
    public static TransactionalServiceValidationBuilder<TContext, TResult> ThenCreateReadOnlyRepository<TContext, TResult, TRepo>(
        this TransactionalServiceValidationBuilder<TContext, TResult> builder,
        string? customKey = null)
        where TContext : DbContext
        where TRepo : class, IRepository
    {
        var unitOfWorkProvider = builder.Context.Get<IUnitOfWorkProvider<TContext>>("UnitOfWorkProvider");
        
        using var readOnlyUow = unitOfWorkProvider.CreateReadOnly();
        var repository = readOnlyUow.GetRepository<TRepo>();
        
        if (customKey != null)
        {
            builder.Context.Store(customKey, repository);
        }
        else
        {
            builder.Context.StoreRepository(repository, isReadOnly: true);
        }
        
        return builder;
    }
    
    /// <summary>
    /// Creates a writable repository and stores it in the context with automatic naming.
    /// The repository can be retrieved using context.GetRepository&lt;T&gt;(isReadOnly: false)
    /// </summary>
    /// <typeparam name="TContext">The DbContext type</typeparam>
    /// <typeparam name="TResult">The result type of the chain</typeparam>
    /// <typeparam name="TRepo">The repository interface type</typeparam>
    /// <param name="builder">The validation builder</param>
    /// <param name="customKey">Optional custom key for storing the repository</param>
    /// <returns>The builder for method chaining</returns>
    public static TransactionalServiceValidationBuilder<TContext, TResult> ThenCreateWritableRepository<TContext, TResult, TRepo>(
        this TransactionalServiceValidationBuilder<TContext, TResult> builder,
        string? customKey = null)
        where TContext : DbContext
        where TRepo : class, IRepository
    {
        // Get or create the writable unit of work (this is cached in the builder)
        var unitOfWork = builder.Context.Get<IWritableUnitOfWork<TContext>>("UnitOfWork");
        if (unitOfWork == null)
        {
            // Force creation of unit of work by accessing a private method through reflection
            // or better, we should use the existing pattern
            var unitOfWorkProvider = builder.Context.Get<IUnitOfWorkProvider<TContext>>("UnitOfWorkProvider");
            unitOfWork = unitOfWorkProvider.CreateWritable();
            builder.Context.Store("UnitOfWork", unitOfWork);
        }
        
        var repository = unitOfWork.GetRepository<TRepo>();
        
        if (customKey != null)
        {
            builder.Context.Store(customKey, repository);
        }
        else
        {
            builder.Context.StoreRepository(repository, isReadOnly: false);
        }
        
        return builder;
    }
    
    /// <summary>
    /// Loads data asynchronously and stores it in the context with a required key.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type</typeparam>
    /// <typeparam name="TResult">The result type of the chain</typeparam>
    /// <typeparam name="TData">The type of data to load</typeparam>
    /// <param name="builder">The validation builder</param>
    /// <param name="storeAs">The key to store the data under</param>
    /// <param name="loadFunc">The async function to load the data, with access to context</param>
    /// <returns>The builder for method chaining</returns>
    public static async Task<TransactionalServiceValidationBuilder<TContext, TResult>> ThenLoadAsync<TContext, TResult, TData>(
        this TransactionalServiceValidationBuilder<TContext, TResult> builder,
        string storeAs,
        Func<DynamicChainContext, Task<TData>> loadFunc)
        where TContext : DbContext
    {
        var data = await loadFunc(builder.Context);
        builder.Context.Store(storeAs, data);
        return builder;
    }
    
    /// <summary>
    /// Performs validation with access to the context for repository and data access.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type</typeparam>
    /// <typeparam name="TResult">The result type of the chain</typeparam>
    /// <param name="builder">The validation builder</param>
    /// <param name="predicate">The async validation predicate with context access</param>
    /// <param name="error">The error to add if validation fails</param>
    /// <returns>The builder for method chaining</returns>
    public static async Task<TransactionalServiceValidationBuilder<TContext, TResult>> ThenEnsureAsync<TContext, TResult>(
        this TransactionalServiceValidationBuilder<TContext, TResult> builder,
        Func<DynamicChainContext, Task<bool>> predicate,
        ServiceError error)
        where TContext : DbContext
    {
        return await builder.EnsureAsync(
            async () => await predicate(builder.Context),
            error);
    }
    
    /// <summary>
    /// Performs validation with access to the context for repository and data access.
    /// Convenience overload that accepts a string error message.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type</typeparam>
    /// <typeparam name="TResult">The result type of the chain</typeparam>
    /// <param name="builder">The validation builder</param>
    /// <param name="predicate">The async validation predicate with context access</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The builder for method chaining</returns>
    public static async Task<TransactionalServiceValidationBuilder<TContext, TResult>> ThenEnsureAsync<TContext, TResult>(
        this TransactionalServiceValidationBuilder<TContext, TResult> builder,
        Func<DynamicChainContext, Task<bool>> predicate,
        string errorMessage)
        where TContext : DbContext
    {
        return await builder.EnsureAsync(
            async () => await predicate(builder.Context),
            ServiceError.ValidationFailed(errorMessage));
    }
    
    /// <summary>
    /// Executes an action with access to both the unit of work and the context.
    /// This is an enhancement of the existing ThenExecuteWithUnitOfWorkAsync pattern.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type</typeparam>
    /// <typeparam name="TResult">The result type of the chain</typeparam>
    /// <param name="builder">The validation builder</param>
    /// <param name="action">The action to execute with unit of work and context</param>
    /// <returns>The result of the execution</returns>
    public static async Task<ServiceResult<TResult>> ThenExecuteWithContextAsync<TContext, TResult>(
        this TransactionalServiceValidationBuilder<TContext, TResult> builder,
        Func<IWritableUnitOfWork<TContext>, DynamicChainContext, Task<TResult>> action)
        where TContext : DbContext
    {
        return await builder.ThenExecuteWithUnitOfWorkAsync(async unitOfWork =>
        {
            // Store the unit of work in context if not already there
            if (!builder.Context.Contains("UnitOfWork"))
            {
                builder.Context.Store("UnitOfWork", unitOfWork);
            }
            
            return await action(unitOfWork, builder.Context);
        });
    }
    
    /// <summary>
    /// Executes an action with access to the context only (unit of work is available through context).
    /// Simplified version when you don't need direct unit of work access.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type</typeparam>
    /// <typeparam name="TResult">The result type of the chain</typeparam>
    /// <param name="builder">The validation builder</param>
    /// <param name="action">The action to execute with context</param>
    /// <returns>The result of the execution</returns>
    public static async Task<ServiceResult<TResult>> ThenExecuteAsync<TContext, TResult>(
        this TransactionalServiceValidationBuilder<TContext, TResult> builder,
        Func<DynamicChainContext, Task<TResult>> action)
        where TContext : DbContext
    {
        return await builder.ThenExecuteWithUnitOfWorkAsync(async unitOfWork =>
        {
            // Store the unit of work in context if not already there
            if (!builder.Context.Contains("UnitOfWork"))
            {
                builder.Context.Store("UnitOfWork", unitOfWork);
            }
            
            return await action(builder.Context);
        });
    }
    
    /// <summary>
    /// Conditionally executes an action on the builder based on a predicate.
    /// If the condition is false, the builder passes through unchanged.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type</typeparam>
    /// <typeparam name="TResult">The result type of the chain</typeparam>
    /// <param name="builder">The validation builder</param>
    /// <param name="condition">The condition to evaluate</param>
    /// <param name="action">The action to execute if condition is true</param>
    /// <returns>The builder for method chaining</returns>
    public static TransactionalServiceValidationBuilder<TContext, TResult> ThenExecuteIf<TContext, TResult>(
        this TransactionalServiceValidationBuilder<TContext, TResult> builder,
        Func<bool> condition,
        Func<TransactionalServiceValidationBuilder<TContext, TResult>, TransactionalServiceValidationBuilder<TContext, TResult>> action)
        where TContext : DbContext
    {
        return condition() ? action(builder) : builder;
    }
    
    /// <summary>
    /// Conditionally executes an action on the builder based on a predicate with context access.
    /// If the condition is false, the builder passes through unchanged.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type</typeparam>
    /// <typeparam name="TResult">The result type of the chain</typeparam>
    /// <param name="builder">The validation builder</param>
    /// <param name="condition">The condition to evaluate with context access</param>
    /// <param name="action">The action to execute if condition is true</param>
    /// <returns>The builder for method chaining</returns>
    public static TransactionalServiceValidationBuilder<TContext, TResult> ThenExecuteIf<TContext, TResult>(
        this TransactionalServiceValidationBuilder<TContext, TResult> builder,
        Func<DynamicChainContext, bool> condition,
        Func<TransactionalServiceValidationBuilder<TContext, TResult>, DynamicChainContext, TransactionalServiceValidationBuilder<TContext, TResult>> action)
        where TContext : DbContext
    {
        return condition(builder.Context) ? action(builder, builder.Context) : builder;
    }
    
    /// <summary>
    /// Conditionally executes an async action on the builder based on a predicate with context access.
    /// If the condition is false, the builder passes through unchanged.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type</typeparam>
    /// <typeparam name="TResult">The result type of the chain</typeparam>
    /// <param name="builder">The validation builder</param>
    /// <param name="condition">The condition to evaluate with context access</param>
    /// <param name="action">The async action to execute if condition is true</param>
    /// <returns>The builder for method chaining</returns>
    public static async Task<TransactionalServiceValidationBuilder<TContext, TResult>> ThenExecuteIfAsync<TContext, TResult>(
        this TransactionalServiceValidationBuilder<TContext, TResult> builder,
        Func<DynamicChainContext, bool> condition,
        Func<TransactionalServiceValidationBuilder<TContext, TResult>, DynamicChainContext, Task<TransactionalServiceValidationBuilder<TContext, TResult>>> action)
        where TContext : DbContext
    {
        return condition(builder.Context) ? await action(builder, builder.Context) : builder;
    }
    
    /// <summary>
    /// Conditionally executes an async action on the builder based on an async predicate with context access.
    /// If the condition is false, the builder passes through unchanged.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type</typeparam>
    /// <typeparam name="TResult">The result type of the chain</typeparam>
    /// <param name="builder">The validation builder</param>
    /// <param name="condition">The async condition to evaluate with context access</param>
    /// <param name="action">The async action to execute if condition is true</param>
    /// <returns>The builder for method chaining</returns>
    public static async Task<TransactionalServiceValidationBuilder<TContext, TResult>> ThenExecuteIfAsync<TContext, TResult>(
        this TransactionalServiceValidationBuilder<TContext, TResult> builder,
        Func<DynamicChainContext, Task<bool>> condition,
        Func<TransactionalServiceValidationBuilder<TContext, TResult>, DynamicChainContext, Task<TransactionalServiceValidationBuilder<TContext, TResult>>> action)
        where TContext : DbContext
    {
        return await condition(builder.Context) ? await action(builder, builder.Context) : builder;
    }
    
    /// <summary>
    /// Conditionally performs an action with context access.
    /// This is a simpler version that doesn't require returning a builder.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type</typeparam>
    /// <typeparam name="TResult">The result type of the chain</typeparam>
    /// <param name="builder">The validation builder</param>
    /// <param name="condition">The condition to evaluate with context access</param>
    /// <param name="action">The async action to execute with context if condition is true</param>
    /// <returns>The builder for method chaining</returns>
    public static async Task<TransactionalServiceValidationBuilder<TContext, TResult>> ThenPerformIfAsync<TContext, TResult>(
        this TransactionalServiceValidationBuilder<TContext, TResult> builder,
        Func<DynamicChainContext, bool> condition,
        Func<DynamicChainContext, Task> action)
        where TContext : DbContext
    {
        if (condition(builder.Context))
        {
            await action(builder.Context);
        }
        return builder;
    }
}