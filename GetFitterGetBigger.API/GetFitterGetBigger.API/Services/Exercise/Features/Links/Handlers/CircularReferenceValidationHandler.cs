using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.DataServices;

namespace GetFitterGetBigger.API.Services.Exercise.Features.Links.Handlers;

/// <summary>
/// Handles circular reference validation for exercise links
/// </summary>
public class CircularReferenceValidationHandler(
    IExerciseLinkQueryDataService queryDataService,
    ILogger<CircularReferenceValidationHandler> logger)
{
    /// <summary>
    /// Validates that creating a link won't cause a circular reference
    /// </summary>
    public async Task<bool> IsNoCircularReferenceAsync(ExerciseId source, ExerciseId target)
    {
        logger.LogDebug(
            "Checking for circular reference: {Source} -> {Target}",
            source, target);
            
        return await ValidateNoCircularReferenceRecursiveAsync(target, source, new HashSet<ExerciseId>());
    }
    
    private async Task<bool> ValidateNoCircularReferenceRecursiveAsync(
        ExerciseId currentId,
        ExerciseId originalSourceId,
        HashSet<ExerciseId> visited)
    {
        if (visited.Contains(currentId))
        {
            return true; // Already checked this node
        }
        
        visited.Add(currentId);
        
        var linksResult = await queryDataService.GetBySourceExerciseAsync(currentId);
        if (!linksResult.IsSuccess)
            return true;
        
        foreach (var link in linksResult.Data)
        {
            var targetId = ExerciseId.ParseOrEmpty(link.TargetExerciseId);
            if (targetId == originalSourceId)
            {
                logger.LogWarning(
                    "Circular reference detected: {Current} links back to {Original}",
                    currentId, originalSourceId);
                return false; // Found circular reference
            }
            
            var isValid = await ValidateNoCircularReferenceRecursiveAsync(
                targetId,
                originalSourceId,
                visited);
                
            if (!isValid)
            {
                return false;
            }
        }
        
        return true;
    }
}