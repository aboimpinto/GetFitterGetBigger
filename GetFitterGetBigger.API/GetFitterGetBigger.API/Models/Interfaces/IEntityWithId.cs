using GetFitterGetBigger.API.Models.Interfaces;

namespace GetFitterGetBigger.API.Models;

/// <summary>
/// Generic entity interface that uses strongly-typed IDs instead of strings
/// </summary>
/// <typeparam name="TId">The specialized ID type for this entity</typeparam>
public interface IEntity<TId> : IEntity 
    where TId : struct, ISpecializedId<TId>
{
    /// <summary>
    /// Gets the strongly-typed unique identifier of the entity
    /// </summary>
    new TId Id { get; }
}

/// <summary>
/// Enhanced reference entity with strongly-typed ID
/// </summary>
/// <typeparam name="TId">The specialized ID type for this entity</typeparam>
public interface IEnhancedReference<TId> : IEnhancedReference, IEntity<TId>
    where TId : struct, ISpecializedId<TId>
{
}

/// <summary>
/// Empty entity pattern with strongly-typed ID
/// </summary>
/// <typeparam name="TSelf">The entity type itself</typeparam>
/// <typeparam name="TId">The specialized ID type for this entity</typeparam>
public interface IEmptyEntity<TSelf, TId> : IEmptyEntity<TSelf>, IEntity<TId>
    where TSelf : IEmptyEntity<TSelf, TId>
    where TId : struct, ISpecializedId<TId>
{
}