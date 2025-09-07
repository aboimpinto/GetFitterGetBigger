using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Results;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Infrastructure;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Extensions;
using Olimpo.EntityFramework.Persistency;
using WorkoutTemplateEntity = GetFitterGetBigger.API.Models.Entities.WorkoutTemplate;
using WorkoutTemplateExerciseEntity = GetFitterGetBigger.API.Models.Entities.WorkoutTemplateExercise;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.DataServices;

/// <summary>
/// Data service implementation for WorkoutTemplate write operations.
/// Encapsulates all database modifications and entity-to-DTO mapping.
/// </summary>
public class WorkoutTemplateCommandDataService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider) : IWorkoutTemplateCommandDataService
{
    
    public async Task<ServiceResult<WorkoutTemplateDto>> CreateAsync(
        WorkoutTemplateEntity entity,
        ITransactionScope? scope = null)
    {
        return scope == null 
            ? await CreateWithoutScopeAsync(entity)
            : await CreateWithScopeAsync(entity, scope);
    }
    
    public async Task<ServiceResult<WorkoutTemplateDto>> UpdateAsync(
        WorkoutTemplateId id,
        Func<WorkoutTemplateEntity, WorkoutTemplateEntity> updateAction,
        ITransactionScope? scope = null)
    {
        return scope == null
            ? await UpdateWithoutScopeAsync(id, updateAction)
            : await UpdateWithScopeAsync(id, updateAction, scope);
    }
    
    public async Task<ServiceResult<WorkoutTemplateDto>> ChangeStateAsync(
        WorkoutTemplateId id,
        WorkoutStateId newStateId,
        ITransactionScope? scope = null)
    {
        return await UpdateAsync(id, entity =>
        {
            // Use entity handler method for state change
            var updateResult = WorkoutTemplateEntity.Handler.ChangeState(entity, newStateId);
            if (!updateResult.IsSuccess) return entity;
            entity = updateResult.Value;
            // Note: UpdatedAt should be handled via entity update methods, not direct assignment
            return entity;
        }, scope);
    }
    
    public async Task<ServiceResult<WorkoutTemplateDto>> DuplicateAsync(
        WorkoutTemplateId sourceId,
        string newName,
        UserId createdById,
        ITransactionScope? scope = null)
    {
        return scope == null
            ? await DuplicateWithoutScopeAsync(sourceId, newName, createdById)
            : await DuplicateWithScopeAsync(sourceId, newName, createdById, scope);
    }
    
    public async Task<ServiceResult<BooleanResultDto>> SoftDeleteAsync(
        WorkoutTemplateId id,
        ITransactionScope? scope = null)
    {
        var result = await ChangeStateAsync(id, WorkoutStateConstants.ArchivedId, scope);
        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(result.IsSuccess));
    }
    
    public async Task<ServiceResult<BooleanResultDto>> DeleteAsync(
        WorkoutTemplateId id,
        ITransactionScope? scope = null)
    {
        return scope == null
            ? await DeleteWithoutScopeAsync(id)
            : await DeleteWithScopeAsync(id, scope);
    }
    
    public async Task<ServiceResult<WorkoutTemplateDto>> AddExerciseAsync(
        WorkoutTemplateId templateId,
        ExerciseId exerciseId,
        int orderInWorkout,
        int sets,
        int reps,
        int restSeconds,
        ITransactionScope? scope = null)
    {
        return await UpdateAsync(templateId, entity =>
        {
            // Create exercise using proper entity factory
            var exerciseResult = WorkoutTemplateExerciseEntity.Handler.CreateNew(
                templateId,
                exerciseId,
GetFitterGetBigger.API.Models.Entities.WorkoutZone.Main, // Default to Main zone
                orderInWorkout
            );
            
            if (!exerciseResult.IsSuccess)
            {
                return entity; // Skip if creation fails
            }
            
            var exercise = exerciseResult.Value;
            // Note: SetConfigurations with sets/reps/rest would need to be handled separately
            
            entity.Exercises.Add(exercise);
            // Note: UpdatedAt should be handled via entity update methods, not direct assignment
            return entity;
        }, scope);
    }
    
    public async Task<ServiceResult<WorkoutTemplateDto>> RemoveExerciseAsync(
        WorkoutTemplateId templateId,
        ExerciseId exerciseId,
        ITransactionScope? scope = null)
    {
        return await UpdateAsync(templateId, entity =>
        {
            var exercise = entity.Exercises.FirstOrDefault(e => e.ExerciseId == exerciseId);
            if (exercise != null)
            {
                entity.Exercises.Remove(exercise);
                // Note: UpdatedAt should be handled via entity update methods, not direct assignment
            }
            return entity;
        }, scope);
    }
    
    public async Task<ServiceResult<WorkoutTemplateDto>> UpdateExerciseConfigurationAsync(
        WorkoutTemplateId templateId,
        ExerciseId exerciseId,
        int sets,
        int reps,
        int restSeconds,
        ITransactionScope? scope = null)
    {
        return await UpdateAsync(templateId, entity =>
        {
            var exercise = entity.Exercises.FirstOrDefault(e => e.ExerciseId == exerciseId);
            if (exercise != null)
            {
                // Note: Sets/Reps/RestSeconds are now handled via SetConfiguration entities in Configurations collection
                // This would require proper SetConfiguration creation and management
                // Note: UpdatedAt should be handled via entity update methods, not direct assignment
            }
            return entity;
        }, scope);
    }
    
    public async Task<ServiceResult<WorkoutTemplateDto>> ReorderExercisesAsync(
        WorkoutTemplateId templateId,
        Dictionary<ExerciseId, int> exerciseOrders,
        ITransactionScope? scope = null)
    {
        return await UpdateAsync(templateId, entity =>
        {
            foreach (var (exerciseId, newOrder) in exerciseOrders)
            {
                var exercise = entity.Exercises.FirstOrDefault(e => e.ExerciseId == exerciseId);
                if (exercise != null)
                {
                    // Update using entity handler method
                    var updateResult = WorkoutTemplateExerciseEntity.Handler.UpdateSequenceOrder(exercise, newOrder);
                    if (updateResult.IsSuccess)
                    {
                        // Replace the exercise in the collection
                        var index = entity.Exercises.ToList().IndexOf(exercise);
                        if (index >= 0)
                        {
                            var exercisesList = entity.Exercises.ToList();
                            exercisesList[index] = updateResult.Value;
                            // Note: This is a simplified approach - proper collection handling may be needed
                        }
                    }
                }
            }
            // Note: UpdatedAt should be handled via entity update methods, not direct assignment
            return entity;
        }, scope);
    }
    
    // Private helper methods for working with provided transaction scopes
    
    private async Task<ServiceResult<WorkoutTemplateDto>> CreateWithoutScopeAsync(
        WorkoutTemplateEntity entity)
    {
        using var unitOfWork = unitOfWorkProvider.CreateWritable();
        var scope = new WritableTransactionScope(unitOfWork);
        
        var result = await CreateWithScopeAsync(entity, scope);
        
        if (result.IsSuccess)
        {
            await unitOfWork.CommitAsync();
        }
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> CreateWithScopeAsync(
        WorkoutTemplateEntity entity,
        ITransactionScope scope)
    {
        var unitOfWork = GetUnitOfWorkFromScope(scope);
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        await repository.AddAsync(entity);
        // Don't commit here - let the scope owner commit
        
        // Reload with full details
        var created = await repository.GetByIdWithDetailsAsync(entity.Id);
        var dto = created.ToDto();
        
        return ServiceResult<WorkoutTemplateDto>.Success(dto);
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> UpdateWithoutScopeAsync(
        WorkoutTemplateId id,
        Func<WorkoutTemplateEntity, WorkoutTemplateEntity> updateAction)
    {
        using var unitOfWork = unitOfWorkProvider.CreateWritable();
        var scope = new WritableTransactionScope(unitOfWork);
        
        var result = await UpdateWithScopeAsync(id, updateAction, scope);
        
        if (result.IsSuccess)
        {
            await unitOfWork.CommitAsync();
        }
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> UpdateWithScopeAsync(
        WorkoutTemplateId id,
        Func<WorkoutTemplateEntity, WorkoutTemplateEntity> updateAction,
        ITransactionScope scope)
    {
        var unitOfWork = GetUnitOfWorkFromScope(scope);
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var entity = await repository.GetByIdWithDetailsAsync(id);
        if (entity.IsEmpty)
        {
            return ServiceResult<WorkoutTemplateDto>.Success(WorkoutTemplateDto.Empty);
        }
        
        var updated = updateAction(entity);
        await repository.UpdateAsync(updated);
        // Don't commit here - let the scope owner commit
        
        // Reload with full details
        var reloaded = await repository.GetByIdWithDetailsAsync(id);
        var dto = reloaded.ToDto();
        
        return ServiceResult<WorkoutTemplateDto>.Success(dto);
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> DuplicateWithoutScopeAsync(
        WorkoutTemplateId sourceId,
        string newName,
        UserId createdById)
    {
        using var unitOfWork = unitOfWorkProvider.CreateWritable();
        var scope = new WritableTransactionScope(unitOfWork);
        
        var result = await DuplicateWithScopeAsync(sourceId, newName, createdById, scope);
        
        if (result.IsSuccess)
        {
            await unitOfWork.CommitAsync();
        }
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> DuplicateWithScopeAsync(
        WorkoutTemplateId sourceId,
        string newName,
        UserId createdById,
        ITransactionScope scope)
    {
        var unitOfWork = GetUnitOfWorkFromScope(scope);
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        // Load the source entity
        var source = await repository.GetByIdWithDetailsAsync(sourceId);
        
        // Use chained validation pattern with single exit point
        var chain = ServiceValidate.Build<WorkoutTemplateDto>()
            .EnsureNotEmpty(
                source, 
                ServiceError.NotFound("WorkoutTemplate", sourceId.ToString()))
            .ThenCreateDuplicate(src => 
                WorkoutTemplateEntity.Handler.CreateNew(
                    newName,
                    src.Description,
                    src.CategoryId,
                    src.DifficultyId,
                    src.EstimatedDurationMinutes,
                    src.Tags?.ToList(),
                    src.IsPublic,
                    WorkoutStateConstants.DraftId,
                    src.ExecutionProtocolId));
        
        // Continue the async chain
        var chainAfterAdd = await chain.ThenAddAsync(async duplicate => 
            await repository.AddAsync(duplicate));
            
        var chainAfterReload = await chainAfterAdd.ThenReloadAsync(async duplicate => 
            await repository.GetByIdWithDetailsAsync(duplicate.Id));
            
        return await chainAfterReload.MatchAsync(
            reloadedTemplate => ServiceResult<WorkoutTemplateDto>.Success(reloadedTemplate.ToDto()),
            error => ServiceResult<WorkoutTemplateDto>.Failure(WorkoutTemplateDto.Empty, error));
    }
    
    private async Task<ServiceResult<BooleanResultDto>> DeleteWithoutScopeAsync(
        WorkoutTemplateId id)
    {
        using var unitOfWork = unitOfWorkProvider.CreateWritable();
        var scope = new WritableTransactionScope(unitOfWork);
        
        var result = await DeleteWithScopeAsync(id, scope);
        
        if (result.IsSuccess)
        {
            await unitOfWork.CommitAsync();
        }
        
        return result;
    }
    
    private async Task<ServiceResult<BooleanResultDto>> DeleteWithScopeAsync(
        WorkoutTemplateId id,
        ITransactionScope scope)
    {
        var unitOfWork = GetUnitOfWorkFromScope(scope);
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var entity = await repository.GetByIdAsync(id);
        if (entity.IsEmpty)
        {
            return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(false));
        }
        
        await repository.DeleteAsync(entity.Id);
        // Don't commit here - let the scope owner commit
        
        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true));
    }
    
    private IWritableUnitOfWork<FitnessDbContext> GetUnitOfWorkFromScope(ITransactionScope scope)
    {
        if (scope.IsReadOnly)
        {
            throw new InvalidOperationException("Cannot perform write operations with a read-only transaction scope");
        }
        
        return ((WritableTransactionScope)scope).UnitOfWork;
    }
}