using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.DTOs.Interfaces;
using GetFitterGetBigger.API.Models.Enums;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;

namespace GetFitterGetBigger.API.Services.Exercise.Features.Links.Validation;

/// <summary>
/// Validation extensions specifically for ExerciseLink operations that need to validate
/// relationships between two exercises. These extensions optimize database access by
/// loading each exercise only once and carrying them through the validation chain.
/// </summary>
public static class ExerciseLinkValidationExtensions
{
    /// <summary>
    /// Transitions from ServiceValidationBuilder to ServiceValidationWithExercises for dual-entity validation
    /// </summary>
    public static ServiceValidationWithExercises<T> AsExerciseLinkValidation<T>(
        this ServiceValidationBuilder<T> builder)
        where T : class, IEmptyDto<T>
    {
        return new ServiceValidationWithExercises<T>(builder.Validation);
    }
    
    /// <summary>
    /// Loads the source exercise and validates it exists and is active.
    /// This makes ONE database call and carries the loaded exercise through the chain.
    /// </summary>
    public static async Task<ServiceValidationWithExercises<T>> EnsureSourceExerciseExists<T>(
        this ServiceValidationWithExercises<T> validation,
        IExerciseService exerciseService,
        ExerciseId sourceId,
        string errorMessage)
        where T : class, IEmptyDto<T>
    {
        // Skip if validation already has errors
        if (validation.HasErrors)
            return validation;
        
        // Load the source exercise (1 DB call)
        var result = await exerciseService.GetByIdAsync(sourceId);
        
        // Check if load failed or returned empty/inactive
        if (!result.IsSuccess || result.Data?.IsEmpty != false || !result.Data.IsActive)
        {
            validation.Validation.Ensure(() => false, errorMessage);
            return validation;
        }
        
        // Return validation with loaded source exercise
        return validation.WithSourceExercise(result.Data);
    }
    
    /// <summary>
    /// Loads the target exercise and validates it exists and is active.
    /// This makes ONE database call and carries both exercises through the chain.
    /// </summary>
    public static async Task<ServiceValidationWithExercises<T>> EnsureTargetExerciseExists<T>(
        this Task<ServiceValidationWithExercises<T>> validationTask,
        IExerciseService exerciseService,
        ExerciseId targetId,
        string errorMessage)
        where T : class, IEmptyDto<T>
    {
        var validation = await validationTask;
        
        // Skip if validation already has errors
        if (validation.HasErrors)
            return validation;
        
        // Load the target exercise (1 DB call)
        var result = await exerciseService.GetByIdAsync(targetId);
        
        // Check if load failed or returned empty/inactive
        if (!result.IsSuccess || result.Data?.IsEmpty != false || !result.Data.IsActive)
        {
            validation.Validation.Ensure(() => false, errorMessage);
            return validation;
        }
        
        // Return validation with both exercises
        return validation.WithTargetExercise(result.Data);
    }
    
    /// <summary>
    /// Validates that the source exercise is not a REST type exercise.
    /// Uses the already-loaded source exercise (NO database call).
    /// </summary>
    public static async Task<ServiceValidationWithExercises<T>> EnsureSourceExerciseIsNotRest<T>(
        this Task<ServiceValidationWithExercises<T>> validationTask,
        string errorMessage)
        where T : class, IEmptyDto<T>
    {
        var validation = await validationTask;
        
        // Skip if validation has errors or no source loaded
        if (validation.HasErrors || validation.SourceExercise == null)
            return validation;
        
        // Check if source is REST type
        var isRest = validation.SourceExercise.ExerciseTypes.Any(et => 
            string.Equals(et.Value, "Rest", StringComparison.OrdinalIgnoreCase));
        
        validation.Validation.Ensure(() => !isRest, errorMessage);
        return validation;
    }
    
    /// <summary>
    /// Validates that the source exercise is a WORKOUT type exercise.
    /// Uses the already-loaded source exercise (NO database call).
    /// </summary>
    public static async Task<ServiceValidationWithExercises<T>> EnsureSourceExerciseIsWorkoutType<T>(
        this Task<ServiceValidationWithExercises<T>> validationTask,
        string errorMessage)
        where T : class, IEmptyDto<T>
    {
        var validation = await validationTask;
        
        // Skip if validation has errors or no source loaded
        if (validation.HasErrors || validation.SourceExercise == null)
            return validation;
        
        // Check if source is Workout type
        var isWorkout = validation.SourceExercise.ExerciseTypes.Any(et => 
            string.Equals(et.Value, "Workout", StringComparison.OrdinalIgnoreCase));
        
        validation.Validation.Ensure(() => isWorkout, errorMessage);
        return validation;
    }
    
    /// <summary>
    /// Validates that the target exercise is not a REST type exercise.
    /// Uses the already-loaded target exercise (NO database call).
    /// </summary>
    public static async Task<ServiceValidationWithExercises<T>> EnsureTargetExerciseIsNotRest<T>(
        this Task<ServiceValidationWithExercises<T>> validationTask,
        string errorMessage)
        where T : class, IEmptyDto<T>
    {
        var validation = await validationTask;
        
        // Skip if validation has errors or no target loaded
        if (validation.HasErrors || validation.TargetExercise == null)
            return validation;
        
        // Check if target is REST type
        var isRest = validation.TargetExercise.ExerciseTypes.Any(et => 
            string.Equals(et.Value, "Rest", StringComparison.OrdinalIgnoreCase));
        
        validation.Validation.Ensure(() => !isRest, errorMessage);
        return validation;
    }
    
    /// <summary>
    /// Validates that the target exercise matches the expected type for the link.
    /// For traditional string-based link types (Warmup/Cooldown).
    /// Uses the already-loaded target exercise (NO database call).
    /// </summary>
    public static async Task<ServiceValidationWithExercises<T>> EnsureTargetExerciseMatchesLinkType<T>(
        this Task<ServiceValidationWithExercises<T>> validationTask,
        string linkType,
        string errorMessage)
        where T : class, IEmptyDto<T>
    {
        var validation = await validationTask;
        
        // Skip if validation has errors or no target loaded
        if (validation.HasErrors || validation.TargetExercise == null)
            return validation;
        
        // Check if target matches the expected type
        var hasMatchingType = validation.TargetExercise.ExerciseTypes.Any(et => 
            string.Equals(et.Value, linkType, StringComparison.OrdinalIgnoreCase));
        
        validation.Validation.Ensure(() => hasMatchingType, errorMessage);
        return validation;
    }
    
    /// <summary>
    /// Validates that both exercises are compatible for the specified link type.
    /// Uses BOTH already-loaded exercises (NO database calls).
    /// </summary>
    public static async Task<ServiceValidationWithExercises<T>> EnsureExercisesAreCompatibleForLinkType<T>(
        this Task<ServiceValidationWithExercises<T>> validationTask,
        ExerciseLinkType linkType,
        string errorMessage)
        where T : class, IEmptyDto<T>
    {
        var validation = await validationTask;
        
        // Skip if validation has errors or exercises not loaded
        if (validation.HasErrors || validation.SourceExercise == null || validation.TargetExercise == null)
            return validation;
        
        var isCompatible = IsLinkTypeCompatible(
            validation.SourceExercise, 
            validation.TargetExercise, 
            linkType);
        
        validation.Validation.Ensure(() => isCompatible, errorMessage);
        return validation;
    }
    
    /// <summary>
    /// Adds a standard async validation that doesn't depend on loaded exercises.
    /// This allows continuing to use existing validation logic alongside exercise-specific validations.
    /// </summary>
    public static async Task<ServiceValidationWithExercises<T>> EnsureAsync<T>(
        this Task<ServiceValidationWithExercises<T>> validationTask,
        Func<Task<bool>> predicateAsync,
        string errorMessage)
        where T : class, IEmptyDto<T>
    {
        var validation = await validationTask;
        
        // Skip if validation already has errors
        if (validation.HasErrors)
            return validation;
        
        var result = await predicateAsync();
        validation.Validation.Ensure(() => result, errorMessage);
        return validation;
    }
    
    /// <summary>
    /// Terminal operation that executes an action when validation succeeds.
    /// Provides access to both loaded exercises if needed.
    /// </summary>
    public static async Task<ServiceResult<T>> MatchAsyncWithExercises<T>(
        this Task<ServiceValidationWithExercises<T>> validationTask,
        Func<Task<ServiceResult<T>>> whenValid)
        where T : class, IEmptyDto<T>
    {
        var validation = await validationTask;
        
        // Use the underlying validation's Match method for proper error handling
        return await validation.Validation.Match(
            whenValid: whenValid,
            whenInvalid: errors => 
            {
                // Use the static interface method to get empty instance
                var emptyInstance = T.Empty;
                return ServiceResult<T>.Failure(emptyInstance, errors.ToArray());
            });
    }
    
    /// <summary>
    /// Terminal operation that executes an action using the loaded exercises when validation succeeds.
    /// </summary>
    public static async Task<ServiceResult<T>> MatchAsyncWithExercises<T>(
        this Task<ServiceValidationWithExercises<T>> validationTask,
        Func<ExerciseDto, ExerciseDto, Task<ServiceResult<T>>> whenValidWithExercises)
        where T : class, IEmptyDto<T>
    {
        var validation = await validationTask;
        
        // Use the underlying validation's Match method for proper error handling
        return await validation.Validation.Match(
            whenValid: async () => 
            {
                // Ensure both exercises are loaded
                if (validation.SourceExercise == null || validation.TargetExercise == null)
                {
                    var emptyInstance = T.Empty;
                    return ServiceResult<T>.Failure(emptyInstance, "Exercises not loaded during validation");
                }
                return await whenValidWithExercises(validation.SourceExercise, validation.TargetExercise);
            },
            whenInvalid: errors => 
            {
                // Use the static interface method to get empty instance
                var emptyInstance = T.Empty;
                return ServiceResult<T>.Failure(emptyInstance, errors.ToArray());
            });
    }
    
    // Helper method for link type compatibility checking
    private static bool IsLinkTypeCompatible(
        ExerciseDto sourceExercise,
        ExerciseDto targetExercise,
        ExerciseLinkType linkType)
    {
        // REST exercises cannot have any links
        var sourceIsRest = sourceExercise.ExerciseTypes.Any(et => 
            string.Equals(et.Value, "Rest", StringComparison.OrdinalIgnoreCase));
        var targetIsRest = targetExercise.ExerciseTypes.Any(et => 
            string.Equals(et.Value, "Rest", StringComparison.OrdinalIgnoreCase));
        
        if (sourceIsRest || targetIsRest)
            return false;
        
        var targetIsWorkout = targetExercise.ExerciseTypes.Any(et => 
            string.Equals(et.Value, "Workout", StringComparison.OrdinalIgnoreCase));
        
        // Implement compatibility matrix from feature requirements
        return linkType switch
        {
            // WARMUP can only link to WORKOUT exercises
            ExerciseLinkType.WARMUP => targetIsWorkout,
            
            // COOLDOWN can only link to WORKOUT exercises
            ExerciseLinkType.COOLDOWN => targetIsWorkout,
            
            // ALTERNATIVE can link to any non-REST exercise (already checked above)
            ExerciseLinkType.ALTERNATIVE => true,
            
            // WORKOUT links are only created automatically as reverse links
            ExerciseLinkType.WORKOUT => false,
            
            _ => false
        };
    }
}