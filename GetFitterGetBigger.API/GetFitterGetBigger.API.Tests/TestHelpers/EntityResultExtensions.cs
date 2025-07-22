using System;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Results;

namespace GetFitterGetBigger.API.Tests.TestHelpers;

/// <summary>
/// Extension methods for EntityResult to simplify test code where entity creation is expected to succeed.
/// </summary>
public static class EntityResultExtensions
{
    /// <summary>
    /// Unwraps the EntityResult if successful, otherwise throws an exception.
    /// Use this in tests where you expect the entity creation to succeed.
    /// </summary>
    /// <typeparam name="T">The type of entity</typeparam>
    /// <param name="result">The EntityResult to unwrap</param>
    /// <returns>The entity if successful</returns>
    /// <exception cref="InvalidOperationException">Thrown if the result is a failure</exception>
    public static T Unwrap<T>(this EntityResult<T> result) where T : class, IEmptyEntity<T>
    {
        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Cannot unwrap a failed EntityResult. Errors: {string.Join(", ", result.Errors)}");
        }
        
        return result.Value!;
    }
    
    /// <summary>
    /// Unwraps the EntityResult if successful, otherwise returns the provided default value.
    /// Use this in tests where you want to handle both success and failure cases.
    /// </summary>
    /// <typeparam name="T">The type of entity</typeparam>
    /// <param name="result">The EntityResult to unwrap</param>
    /// <param name="defaultValue">The value to return if the result is a failure</param>
    /// <returns>The entity if successful, otherwise the default value</returns>
    public static T UnwrapOr<T>(this EntityResult<T> result, T defaultValue) where T : class, IEmptyEntity<T>
    {
        return result.IsSuccess ? result.Value! : defaultValue;
    }
}