using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands;
using GetFitterGetBigger.API.Services.Interfaces;

namespace GetFitterGetBigger.API.Services.Exercise.Handlers;

/// <summary>
/// Handler for exercise type-related validation logic.
/// Validates REST exercise rules and related constraints.
/// </summary>
public class ExerciseTypeValidationHandler
{
    private readonly IExerciseTypeService _exerciseTypeService;
    
    public ExerciseTypeValidationHandler(IExerciseTypeService exerciseTypeService)
    {
        _exerciseTypeService = exerciseTypeService;
    }
    
    /// <summary>
    /// Validates that the exercise types configuration is valid
    /// </summary>
    public async Task<bool> ValidateExerciseTypesAsync(List<ExerciseTypeId> exerciseTypeIds)
    {
        // Empty list is valid
        var hasNoTypes = exerciseTypeIds.Count == 0;
        
        // Filter out empty IDs
        var validIds = exerciseTypeIds.Where(id => !id.IsEmpty).ToList();
        var allIdsAreEmpty = exerciseTypeIds.Count > 0 && validIds.Count == 0;
        
        // Check REST exercise business rule
        var isRestExercise = validIds.Count > 0 
            && await _exerciseTypeService.AnyIsRestTypeAsync(validIds.Select(id => id.ToString()).ToList());
        var restExerciseHasMultipleTypes = isRestExercise && validIds.Count > 1;
        
        // Single exit point with clear boolean logic
        return hasNoTypes || (!allIdsAreEmpty && !restExerciseHasMultipleTypes);
    }
    
    /// <summary>
    /// Validates kinetic chain configuration for the given exercise types
    /// </summary>
    public async Task<bool> ValidateKineticChainAsync(
        List<ExerciseTypeId> exerciseTypeIds, 
        KineticChainTypeId kineticChainId)
    {
        // Filter out empty IDs
        var validIds = exerciseTypeIds.Where(id => !id.IsEmpty).ToList();
        
        // If no valid exercise types, validation passes
        if (validIds.Count == 0) return true;
        
        // Check if this is a REST exercise
        var isRestExercise = await _exerciseTypeService.AnyIsRestTypeAsync(
            validIds.Select(id => id.ToString()).ToList());
        var hasKineticChain = !kineticChainId.IsEmpty;
        
        if (isRestExercise)
        {
            // REST exercises cannot have kinetic chain
            return !hasKineticChain;
        }
        
        // Non-REST exercises must have kinetic chain
        return hasKineticChain;
    }
    
    /// <summary>
    /// Validates weight type configuration for the given exercise types
    /// </summary>
    public async Task<bool> ValidateWeightTypeAsync(
        List<ExerciseTypeId> exerciseTypeIds, 
        ExerciseWeightTypeId weightTypeId)
    {
        // Filter out empty IDs
        var validIds = exerciseTypeIds.Where(id => !id.IsEmpty).ToList();
        
        // If no valid exercise types, validation passes
        if (validIds.Count == 0) return true;
        
        // Check if this is a REST exercise
        var isRestExercise = await _exerciseTypeService.AnyIsRestTypeAsync(
            validIds.Select(id => id.ToString()).ToList());
        var hasWeightType = !weightTypeId.IsEmpty;
        
        if (isRestExercise)
        {
            // REST exercises cannot have weight type
            return !hasWeightType;
        }
        
        // Non-REST exercises should have weight type (optional for now)
        return true; // Allow non-REST without weight type for flexibility
    }
    
    /// <summary>
    /// Validates muscle groups configuration for the given exercise types
    /// </summary>
    public async Task<bool> ValidateMuscleGroupsAsync(
        List<ExerciseTypeId> exerciseTypeIds, 
        List<MuscleGroupAssignment> muscleGroups)
    {
        // Filter out empty IDs
        var validIds = exerciseTypeIds.Where(id => !id.IsEmpty).ToList();
        
        // If no valid exercise types, validation passes
        if (validIds.Count == 0) return true;
        
        // Check if this is a REST exercise
        var isRestExercise = await _exerciseTypeService.AnyIsRestTypeAsync(
            validIds.Select(id => id.ToString()).ToList());
        var hasMuscleGroups = muscleGroups.Count > 0;
        
        if (isRestExercise)
        {
            // REST exercises should not have muscle groups
            return !hasMuscleGroups;
        }
        
        // Non-REST exercises must have at least one muscle group
        return hasMuscleGroups;
    }
}