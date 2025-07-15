using System.Collections.Generic;
using System.Linq;
using GetFitterGetBigger.API.Models.Entities;

namespace GetFitterGetBigger.API.Models.Results;

/// <summary>
/// Represents the result of an entity creation operation, encapsulating either a successful entity or validation errors.
/// This pattern allows entity factory methods to return validation failures without throwing exceptions.
/// Only works with entities that implement IEmptyEntity to ensure a non-null Value is always available.
/// </summary>
/// <typeparam name="T">The type of entity being created (must implement IEmptyEntity)</typeparam>
public class EntityResult<T> where T : class, IEmptyEntity<T>
{
    /// <summary>
    /// Gets the created entity if the operation was successful; otherwise, the Empty entity.
    /// This property is never null due to the IEmptyEntity constraint.
    /// </summary>
    public T Value { get; }
    
    /// <summary>
    /// Gets a value indicating whether the entity creation was successful.
    /// </summary>
    public bool IsSuccess { get; }
    
    /// <summary>
    /// Gets a value indicating whether the entity creation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;
    
    /// <summary>
    /// Gets the validation errors if the operation failed; otherwise, an empty collection.
    /// </summary>
    public IReadOnlyList<string> Errors { get; }
    
    /// <summary>
    /// Gets the first error message if any exist; otherwise, an empty string.
    /// </summary>
    public string FirstError => Errors.FirstOrDefault() ?? string.Empty;

    private EntityResult(T value, bool isSuccess, IEnumerable<string> errors)
    {
        Value = value;
        IsSuccess = isSuccess;
        Errors = errors.ToList().AsReadOnly();
    }

    /// <summary>
    /// Creates a successful entity result.
    /// </summary>
    /// <param name="value">The successfully created entity</param>
    /// <returns>A successful EntityResult containing the entity</returns>
    public static EntityResult<T> Success(T value)
    {
        if (value == null)
            throw new System.ArgumentNullException(nameof(value), "Success result cannot have a null value");
            
        return new EntityResult<T>(value, true, Enumerable.Empty<string>());
    }

    /// <summary>
    /// Creates a failed entity result with validation errors.
    /// The Value property will contain the Empty entity.
    /// </summary>
    /// <param name="errors">The validation error messages</param>
    /// <returns>A failed EntityResult containing the errors and Empty entity</returns>
    public static EntityResult<T> Failure(params string[] errors)
    {
        if (errors == null || errors.Length == 0)
            throw new System.ArgumentException("Failure result must have at least one error", nameof(errors));
            
        return new EntityResult<T>(T.Empty, false, errors);
    }

    /// <summary>
    /// Creates a failed entity result with a single validation error.
    /// The Value property will contain the Empty entity.
    /// </summary>
    /// <param name="error">The validation error message</param>
    /// <returns>A failed EntityResult containing the error and Empty entity</returns>
    public static EntityResult<T> Failure(string error)
    {
        if (string.IsNullOrWhiteSpace(error))
            throw new System.ArgumentException("Error message cannot be empty", nameof(error));
            
        return new EntityResult<T>(T.Empty, false, new[] { error });
    }

}