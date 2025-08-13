using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
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
        WorkoutTemplateExercise entity,
        ITransactionScope? scope = null)
    {
        using var unitOfWork = unitOfWorkProvider.CreateWritable();
            
        var repository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
        
        var createdEntity = await repository.AddAsync(entity);
        
        await unitOfWork.CommitAsync();
        
        logger.LogInformation(
            "Created WorkoutTemplateExercise for template {TemplateId} with exercise {ExerciseId}",
            entity.WorkoutTemplateId, entity.ExerciseId);
        
        return ServiceResult<WorkoutTemplateExerciseDto>.Success(createdEntity.ToDto());
    }
}