using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Models.SpecializedIds;

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
public record ExerciseWeightType : ReferenceDataBase
{
    /// <summary>
    /// The unique identifier for the exercise weight type
    /// </summary>
    public ExerciseWeightTypeId Id { get; init; }
    
    /// <summary>
    /// The code that uniquely identifies this weight type (e.g., "BODYWEIGHT_ONLY", "WEIGHT_REQUIRED")
    /// </summary>
    /// <example>BODYWEIGHT_ONLY</example>
    public string Code { get; init; } = string.Empty;
    
    // Navigation properties
    public ICollection<Exercise> Exercises { get; init; } = new List<Exercise>();
    
    private ExerciseWeightType() { }
    
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
        /// <returns>A new ExerciseWeightType instance</returns>
        /// <exception cref="ArgumentException">Thrown when code or value is null or empty</exception>
        public static ExerciseWeightType CreateNew(
            string code,
            string value,
            string? description,
            int displayOrder,
            bool isActive = true)
        {
            if (string.IsNullOrEmpty(code))
                throw new ArgumentException("Code cannot be empty", nameof(code));
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value cannot be empty", nameof(value));
                
            return new()
            {
                Id = ExerciseWeightTypeId.New(),
                Code = code,
                Value = value,
                Description = description,
                DisplayOrder = displayOrder,
                IsActive = isActive
            };
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
        /// <returns>An ExerciseWeightType instance with the specified ID</returns>
        public static ExerciseWeightType Create(
            ExerciseWeightTypeId id,
            string code,
            string value,
            string? description,
            int displayOrder,
            bool isActive = true) =>
            new()
            {
                Id = id,
                Code = code,
                Value = value,
                Description = description,
                DisplayOrder = displayOrder,
                IsActive = isActive
            };
    }
}