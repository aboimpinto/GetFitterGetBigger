using GetFitterGetBigger.API.Models;
using Microsoft.EntityFrameworkCore;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Validation;

/// <summary>
/// Static methods for creating transactional validation chains as part of ServiceValidate
/// </summary>
public static partial class ServiceValidate
{
    /// <summary>
    /// Starts a transactional validation chain that manages UnitOfWork lifecycle
    /// </summary>
    public static TransactionalServiceValidationBuilder<TContext, TResult> BuildTransactional<TContext, TResult>(
        IUnitOfWorkProvider<TContext> unitOfWorkProvider)
        where TContext : DbContext
    {
        return new TransactionalServiceValidationBuilder<TContext, TResult>(unitOfWorkProvider);
    }
    
    /// <summary>
    /// Shorthand for FitnessDbContext transactional chains
    /// </summary>
    public static TransactionalServiceValidationBuilder<FitnessDbContext, TResult> BuildTransactional<TResult>(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider)
    {
        return new TransactionalServiceValidationBuilder<FitnessDbContext, TResult>(unitOfWorkProvider);
    }
}