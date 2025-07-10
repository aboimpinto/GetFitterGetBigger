using GetFitterGetBigger.API.Models.SpecializedIds;
using System.Threading.Tasks;

namespace GetFitterGetBigger.API.Validators;

/// <summary>
/// Interface for validating exercise weight based on the exercise's weight type
/// </summary>
public interface IExerciseWeightValidator
{
    /// <summary>
    /// Validates whether the provided weight is valid for the given exercise
    /// </summary>
    /// <param name="exerciseId">The ID of the exercise</param>
    /// <param name="weightKg">The weight in kilograms (null for bodyweight exercises)</param>
    /// <returns>A validation result containing success status and error message if invalid</returns>
    Task<WeightValidationResult> ValidateWeightAsync(ExerciseId exerciseId, decimal? weightKg);
    
    /// <summary>
    /// Validates whether the provided weight is valid for the given exercise weight type
    /// </summary>
    /// <param name="exerciseWeightTypeId">The ID of the exercise weight type</param>
    /// <param name="weightKg">The weight in kilograms (null for bodyweight exercises)</param>
    /// <returns>A validation result containing success status and error message if invalid</returns>
    Task<WeightValidationResult> ValidateWeightByTypeAsync(ExerciseWeightTypeId exerciseWeightTypeId, decimal? weightKg);
}

/// <summary>
/// Result of weight validation
/// </summary>
public record WeightValidationResult
{
    /// <summary>
    /// Whether the weight is valid
    /// </summary>
    public bool IsValid { get; init; }
    
    /// <summary>
    /// Error message if validation failed
    /// </summary>
    public string? ErrorMessage { get; init; }
    
    /// <summary>
    /// Creates a successful validation result
    /// </summary>
    public static WeightValidationResult Success() => new() { IsValid = true };
    
    /// <summary>
    /// Creates a failed validation result with an error message
    /// </summary>
    public static WeightValidationResult Failure(string errorMessage) => new() 
    { 
        IsValid = false, 
        ErrorMessage = errorMessage 
    };
}