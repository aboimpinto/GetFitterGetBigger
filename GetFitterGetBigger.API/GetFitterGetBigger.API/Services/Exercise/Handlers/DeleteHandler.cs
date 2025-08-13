using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Exercise.DataServices;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Exercise.Handlers;

/// <summary>
/// Handler for exercise deletion operations.
/// Manages both soft and hard delete logic.
/// </summary>
public class DeleteHandler
{
    private readonly IExerciseCommandDataService _commandDataService;
    
    public DeleteHandler(IExerciseCommandDataService commandDataService)
    {
        _commandDataService = commandDataService;
    }
    
    /// <summary>
    /// Performs a soft delete (marks exercise as inactive)
    /// </summary>
    /// <param name="id">The exercise ID to soft delete</param>
    /// <returns>Service result indicating success or failure</returns>
    public async Task<ServiceResult<bool>> SoftDeleteAsync(ExerciseId id)
    {
        var result = await _commandDataService.SoftDeleteAsync(id);
        return ServiceResult<bool>.Success(result.Data.Value);
    }
    
    /// <summary>
    /// Performs a hard delete (permanently removes from database)
    /// </summary>
    /// <param name="id">The exercise ID to hard delete</param>
    /// <returns>Service result indicating success or failure</returns>
    public async Task<ServiceResult<bool>> HardDeleteAsync(ExerciseId id)
    {
        var result = await _commandDataService.HardDeleteAsync(id);
        return ServiceResult<bool>.Success(result.Data.Value);
    }
    
    /// <summary>
    /// Determines the appropriate deletion strategy based on business rules
    /// For now, defaults to soft delete to preserve data integrity
    /// </summary>
    /// <param name="id">The exercise ID to delete</param>
    /// <returns>Service result indicating success or failure</returns>
    public async Task<ServiceResult<bool>> DeleteAsync(ExerciseId id)
    {
        // For now, just soft delete (mark as inactive)
        // This preserves relationships and historical data
        return await SoftDeleteAsync(id);
    }
}