using GetFitterGetBigger.API.Models;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Infrastructure;

/// <summary>
/// Internal implementation of ITransactionScope that wraps UnitOfWork.
/// This class is internal - Services only see the ITransactionScope interface.
/// </summary>
internal class ReadOnlyTransactionScope : ITransactionScope
{
    internal IReadOnlyUnitOfWork<FitnessDbContext> UnitOfWork { get; }
    
    public Guid TransactionId { get; }
    public bool IsReadOnly => true;
    
    public ReadOnlyTransactionScope(IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork)
    {
        UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        TransactionId = Guid.NewGuid();
    }
    
    public void Dispose()
    {
        UnitOfWork?.Dispose();
    }
}

/// <summary>
/// Internal implementation for writable transactions.
/// </summary>
internal class WritableTransactionScope : ITransactionScope
{
    internal IWritableUnitOfWork<FitnessDbContext> UnitOfWork { get; }
    
    public Guid TransactionId { get; }
    public bool IsReadOnly => false;
    
    public WritableTransactionScope(IWritableUnitOfWork<FitnessDbContext> unitOfWork)
    {
        UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        TransactionId = Guid.NewGuid();
    }
    
    public void Dispose()
    {
        UnitOfWork?.Dispose();
    }
}