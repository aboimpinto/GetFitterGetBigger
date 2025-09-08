using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Infrastructure;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Extensions;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.DataServices;

/// <summary>
/// Data service implementation for WorkoutTemplateExercise write operations.
/// </summary>
public class WorkoutTemplateExerciseCommandDataService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    ILogger<WorkoutTemplateExerciseCommandDataService> logger) : IWorkoutTemplateExerciseCommandDataService
{
    /// <inheritdoc />
    public async Task<ServiceResult<WorkoutTemplateExerciseDto>> CreateAsync(
        WorkoutTemplateId workoutTemplateId,
        ExerciseId exerciseId,
        WorkoutZone zone,
        int sequenceOrder,
        string? notes = null,
        ITransactionScope? scope = null)
    {
        return scope == null 
            ? await CreateWithoutScopeAsync(workoutTemplateId, exerciseId, zone, sequenceOrder, notes)
            : await CreateWithScopeAsync(workoutTemplateId, exerciseId, zone, sequenceOrder, notes, scope);
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseDto>> CreateWithoutScopeAsync(
        WorkoutTemplateId workoutTemplateId,
        ExerciseId exerciseId,
        WorkoutZone zone,
        int sequenceOrder,
        string? notes)
    {
        using var unitOfWork = unitOfWorkProvider.CreateWritable();
        var scope = new WritableTransactionScope(unitOfWork);
        
        var result = await CreateWithScopeAsync(workoutTemplateId, exerciseId, zone, sequenceOrder, notes, scope);
        
        if (result.IsSuccess)
        {
            await unitOfWork.CommitAsync();
        }
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseDto>> CreateWithScopeAsync(
        WorkoutTemplateId workoutTemplateId,
        ExerciseId exerciseId,
        WorkoutZone zone,
        int sequenceOrder,
        string? notes,
        ITransactionScope scope)
    {
        // Validate scope
        if (scope.IsReadOnly)
        {
            return ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                WorkoutTemplateExerciseDto.Empty,
                ServiceError.ValidationFailed("Cannot perform write operations with a read-only transaction scope"));
        }
        
        var unitOfWork = ((WritableTransactionScope)scope).UnitOfWork;
        var repository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
        
        // Create entity internally - entity never crosses boundary
        var entityResult = WorkoutTemplateExercise.Handler.Create(
            WorkoutTemplateExerciseId.New(),
            workoutTemplateId,
            exerciseId,
            zone,
            sequenceOrder,
            notes);
        
        if (!entityResult.IsSuccess)
        {
            return ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                WorkoutTemplateExerciseDto.Empty,
                ServiceError.ValidationFailed(string.Join(", ", entityResult.Errors)));
        }
        
        await repository.AddAsync(entityResult.Value);
        // Don't commit here - let the scope owner commit
        
        logger.LogInformation(
            "Created WorkoutTemplateExercise for template {TemplateId} with exercise {ExerciseId}",
            workoutTemplateId, exerciseId);
        
        return ServiceResult<WorkoutTemplateExerciseDto>.Success(entityResult.Value.ToDto());
    }
}