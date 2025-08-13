using GetFitterGetBigger.API.Models.Interfaces;

namespace GetFitterGetBigger.API.Models;

/// <summary>
/// Core interface for all entities in the system
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Gets the unique identifier of the entity
    /// </summary>
    string Id { get; }
    
    /// <summary>
    /// Gets a value indicating whether the entity is active
    /// </summary>
    bool IsActive { get; }
}

/// <summary>
/// Interface for entities that support the Empty/Null Object pattern
/// </summary>
/// <typeparam name="TSelf">The entity type itself (for static polymorphism)</typeparam>
public interface IEmptyEntity<TSelf> : IEntity, IEmpty where TSelf : IEmptyEntity<TSelf>
{
    /// <summary>
    /// Gets the empty instance of this entity type
    /// </summary>
    static abstract TSelf Empty { get; }
}

/// <summary>
/// Interface for entities that track creation and modification times
/// </summary>
public interface ITrackedEntity : IEntity
{
    /// <summary>
    /// Gets the date and time when the entity was created
    /// </summary>
    DateTime CreatedAt { get; }
    
    /// <summary>
    /// Gets the date and time when the entity was last updated
    /// </summary>
    DateTime UpdatedAt { get; }
}

/// <summary>
/// Interface for reference data entities
/// </summary>
public interface IReferenceEntity : IEntity
{
    /// <summary>
    /// Gets the value (code/key) of the reference data
    /// </summary>
    string Value { get; }
    
    /// <summary>
    /// Gets the human-readable description of the reference data
    /// </summary>
    string? Description { get; }
}

/// <summary>
/// Interface for domain entities with complex behavior
/// </summary>
public interface IDomainEntity : ITrackedEntity
{
    // Domain entities have complex behavior and relationships
    // This is a marker interface to distinguish them from reference data
}

/// <summary>
/// Marker interface for pure reference data that never changes after deployment
/// Examples: BodyPart, DifficultyLevel, WorkoutObjective
/// </summary>
public interface IPureReference : IReferenceEntity, ICacheableEntity
{
    // Pure references are seeded at system inception and only change with deployments
    // They define the "laws of physics" of the domain
}

/// <summary>
/// Marker interface for enhanced reference data that can be modified by admins
/// Examples: MuscleGroup, ExerciseType, Equipment
/// </summary>
public interface IEnhancedReference : IReferenceEntity, ICacheableEntity, ITrackedEntity
{
    // Enhanced references can be modified by administrators
    // They define the configurable vocabulary of the system
}