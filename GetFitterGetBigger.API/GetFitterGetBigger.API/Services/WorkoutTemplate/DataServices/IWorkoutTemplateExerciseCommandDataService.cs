using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
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
    /// <param name="workoutTemplateId">The workout template ID</param>
    /// <param name="exerciseId">The exercise ID</param>
    /// <param name="zone">The workout zone</param>
    /// <param name="sequenceOrder">The sequence order</param>
    /// <param name="notes">Optional notes</param>
    /// <param name="scope">Optional transaction scope for participating in larger transactions</param>
    /// <returns>Service result containing the created workout template exercise DTO</returns>
    Task<ServiceResult<WorkoutTemplateExerciseDto>> CreateAsync(
        WorkoutTemplateId workoutTemplateId,
        ExerciseId exerciseId,
        WorkoutZone zone,
        int sequenceOrder,
        string? notes = null,
        ITransactionScope? scope = null);
}