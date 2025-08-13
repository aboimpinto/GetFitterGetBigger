using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Services.Infrastructure;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.DataServices;

/// <summary>
/// Data service interface for WorkoutTemplateExercise write operations.
/// Handles all database modifications for workout template exercises.
/// </summary>
public interface IWorkoutTemplateExerciseCommandDataService
{
    /// <summary>
    /// Creates a new workout template exercise
    /// </summary>
    /// <param name="entity">The workout template exercise entity to create</param>
    /// <param name="scope">Optional transaction scope for participating in larger transactions</param>
    /// <returns>Service result containing the created workout template exercise DTO</returns>
    Task<ServiceResult<WorkoutTemplateExerciseDto>> CreateAsync(
        WorkoutTemplateExercise entity,
        ITransactionScope? scope = null);
}