using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.DTOs.Interfaces;

namespace GetFitterGetBigger.API.Services.Validation;

/// <summary>
/// Represents a validation state that carries two exercise entities through the validation chain.
/// This specialized class allows validating relationships between two exercises without multiple database round-trips.
/// </summary>
/// <typeparam name="T">The DTO type that implements IEmptyDto</typeparam>
public class ServiceValidationWithExercises<T>
    where T : class, IEmptyDto<T>
{
    /// <summary>
    /// The underlying validation state
    /// </summary>
    public ServiceValidation<T> Validation { get; }
    
    /// <summary>
    /// The source exercise loaded during validation (may be null if not yet loaded or validation failed)
    /// </summary>
    public ExerciseDto? SourceExercise { get; }
    
    /// <summary>
    /// The target exercise loaded during validation (may be null if not yet loaded or validation failed)
    /// </summary>
    public ExerciseDto? TargetExercise { get; }
    
    /// <summary>
    /// Indicates if validation has errors
    /// </summary>
    public bool HasErrors => Validation.HasErrors;
    
    /// <summary>
    /// Creates a new instance with only validation state (no exercises loaded yet)
    /// </summary>
    public ServiceValidationWithExercises(ServiceValidation<T> validation)
    {
        Validation = validation;
        SourceExercise = null;
        TargetExercise = null;
    }
    
    /// <summary>
    /// Creates a new instance with validation state and source exercise
    /// </summary>
    public ServiceValidationWithExercises(
        ServiceValidation<T> validation, 
        ExerciseDto? sourceExercise)
    {
        Validation = validation;
        SourceExercise = sourceExercise;
        TargetExercise = null;
    }
    
    /// <summary>
    /// Creates a new instance with validation state and both exercises
    /// </summary>
    public ServiceValidationWithExercises(
        ServiceValidation<T> validation,
        ExerciseDto? sourceExercise,
        ExerciseDto? targetExercise)
    {
        Validation = validation;
        SourceExercise = sourceExercise;
        TargetExercise = targetExercise;
    }
    
    /// <summary>
    /// Creates a new instance with updated source exercise
    /// </summary>
    public ServiceValidationWithExercises<T> WithSourceExercise(ExerciseDto? sourceExercise)
    {
        return new ServiceValidationWithExercises<T>(Validation, sourceExercise, TargetExercise);
    }
    
    /// <summary>
    /// Creates a new instance with updated target exercise
    /// </summary>
    public ServiceValidationWithExercises<T> WithTargetExercise(ExerciseDto? targetExercise)
    {
        return new ServiceValidationWithExercises<T>(Validation, SourceExercise, targetExercise);
    }
}