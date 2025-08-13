namespace GetFitterGetBigger.API.Services.Infrastructure;

/// <summary>
/// Abstraction for transaction scope that hides UnitOfWork from Services.
/// Services receive this interface without knowing about the underlying data access implementation.
/// </summary>
public interface ITransactionScope : IDisposable
{
    /// <summary>
    /// Unique identifier for this transaction scope
    /// </summary>
    Guid TransactionId { get; }
    
    /// <summary>
    /// Indicates if this scope is for read-only operations
    /// </summary>
    bool IsReadOnly { get; }
}