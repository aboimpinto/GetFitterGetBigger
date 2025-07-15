using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Models.Results;
using GetFitterGetBigger.API.Models.Validation;

namespace GetFitterGetBigger.API.Models.Entities;

/// <summary>
/// Represents a type of weight requirement for exercises
/// </summary>
/// <remarks>
/// Exercise weight types define how weight is used in different exercises:
/// - BODYWEIGHT_ONLY: Exercises that cannot have external weight added (e.g., running, planks)
/// - BODYWEIGHT_OPTIONAL: Exercises that can be performed with or without additional weight (e.g., pull-ups, dips)
/// - WEIGHT_REQUIRED: Exercises that must have external weight specified (e.g., barbell bench press)
/// - MACHINE_WEIGHT: Exercises performed on machines with weight stacks (e.g., lat pulldown)
/// - NO_WEIGHT: Exercises that do not use weight as a metric (e.g., stretching, mobility work)
/// </remarks>
public record ExerciseWeightType : ReferenceDataBase, IEmptyEntity<ExerciseWeightType>, IPureReference
{
    /// <summary>
    /// The unique identifier for the exercise weight type
    /// </summary>
    public ExerciseWeightTypeId Id { get; init; }
    
    /// <summary>
    /// Gets the unique identifier as a string (required by IEntity)
    /// </summary>
    string IEntity.Id => Id.ToString();
    
    /// <summary>
    /// The code that uniquely identifies this weight type (e.g., "BODYWEIGHT_ONLY", "WEIGHT_REQUIRED")
    /// </summary>
    /// <example>BODYWEIGHT_ONLY</example>
    public string Code { get; init; } = string.Empty;
    
    // Navigation properties
    public ICollection<Exercise> Exercises { get; init; } = new List<Exercise>();
    
    /// <summary>
    /// Indicates if this is an empty/null object instance
    /// </summary>
    public bool IsEmpty => Id.IsEmpty;
    
    /// <summary>
    /// Static factory for creating an empty ExerciseWeightType instance
    /// </summary>
    public static ExerciseWeightType Empty { get; } = new() 
    { 
        Id = ExerciseWeightTypeId.Empty,
        Code = string.Empty,
        Value = string.Empty,
        Description = null,
        DisplayOrder = 0,
        IsActive = false
    };
    
    private ExerciseWeightType() { }
    
    /// <summary>
    /// Gets the caching strategy for this entity type
    /// </summary>
    public CacheStrategy GetCacheStrategy() => CacheStrategy.Eternal;
    
    /// <summary>
    /// Gets the cache duration for this entity type
    /// </summary>
    public TimeSpan? GetCacheDuration() => null; // Eternal caching
    
    /// <summary>
    /// Handler for creating ExerciseWeightType instances
    /// </summary>
    public static class Handler
    {
        /// <summary>
        /// Creates a new ExerciseWeightType with a generated ID
        /// </summary>
        /// <param name="code">The unique code for this weight type</param>
        /// <param name="value">The display value/name</param>
        /// <param name="description">Optional description</param>
        /// <param name="displayOrder">The order for display purposes</param>
        /// <param name="isActive">Whether this weight type is active</param>
        /// <returns>An EntityResult containing the new ExerciseWeightType or validation errors</returns>
        public static EntityResult<ExerciseWeightType> CreateNew(
            string code,
            string value,
            string? description,
            int displayOrder,
            bool isActive = true)
        {
            return Validate.For<ExerciseWeightType>()
                .EnsureNotEmpty(code, ExerciseWeightTypeErrorMessages.CodeCannotBeEmpty)
                .EnsureNotEmpty(value, ExerciseWeightTypeErrorMessages.ValueCannotBeEmptyEntity)
                .EnsureMinValue(displayOrder, 0, ExerciseWeightTypeErrorMessages.DisplayOrderMustBeNonNegative)
                .OnSuccess(() => new ExerciseWeightType
                {
                    Id = ExerciseWeightTypeId.New(),
                    Code = code,
                    Value = value,
                    Description = description,
                    DisplayOrder = displayOrder,
                    IsActive = isActive
                });
        }
        
        /// <summary>
        /// Creates an ExerciseWeightType with a specific ID
        /// </summary>
        /// <param name="id">The specific ID to use</param>
        /// <param name="code">The unique code for this weight type</param>
        /// <param name="value">The display value/name</param>
        /// <param name="description">Optional description</param>
        /// <param name="displayOrder">The order for display purposes</param>
        /// <param name="isActive">Whether this weight type is active</param>
        /// <returns>An EntityResult containing the ExerciseWeightType or validation errors</returns>
        public static EntityResult<ExerciseWeightType> Create(
            ExerciseWeightTypeId id,
            string code,
            string value,
            string? description,
            int displayOrder,
            bool isActive = true)
        {
            return Validate.For<ExerciseWeightType>()
                .EnsureNotEmpty(code, ExerciseWeightTypeErrorMessages.CodeCannotBeEmpty)
                .EnsureNotEmpty(value, ExerciseWeightTypeErrorMessages.ValueCannotBeEmptyEntity)
                .EnsureMinValue(displayOrder, 0, ExerciseWeightTypeErrorMessages.DisplayOrderMustBeNonNegative)
                .OnSuccess(() => new ExerciseWeightType
                {
                    Id = id,
                    Code = code,
                    Value = value,
                    Description = description,
                    DisplayOrder = displayOrder,
                    IsActive = isActive
                });
        }
    }
}