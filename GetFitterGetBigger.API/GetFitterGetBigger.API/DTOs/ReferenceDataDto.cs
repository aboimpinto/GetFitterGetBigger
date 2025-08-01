using System;
using GetFitterGetBigger.API.DTOs.Interfaces;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for reference data entities
/// </summary>
public record ReferenceDataDto : IEmptyDto<ReferenceDataDto>
{
    /// <summary>
    /// The ID of the reference data entity in the format "{referencetable}-{guid}"
    /// </summary>
    /// <example>exerciseweighttype-a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a</example>
    public string Id { get; init; } = string.Empty;
    
    /// <summary>
    /// The value of the reference data entity
    /// </summary>
    /// <example>Bodyweight Only</example>
    public string Value { get; init; } = string.Empty;
    
    /// <summary>
    /// The description of the reference data entity
    /// </summary>
    /// <example>Exercises that cannot have external weight added</example>
    public string? Description { get; init; }

    /// <summary>
    /// Whether this DTO represents an empty/default state
    /// </summary>
    public bool IsEmpty => Id == string.Empty;

    /// <summary>
    /// Gets an empty ReferenceDataDto instance for the Empty Object Pattern
    /// </summary>
    public static ReferenceDataDto Empty => new()
    {
        Id = string.Empty,
        Value = string.Empty,
        Description = null
    };
}
