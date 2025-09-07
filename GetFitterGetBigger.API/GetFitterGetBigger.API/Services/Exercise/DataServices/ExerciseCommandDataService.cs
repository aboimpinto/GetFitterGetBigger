using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Implementations.Extensions;
using GetFitterGetBigger.API.Services.Infrastructure;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using Olimpo.EntityFramework.Persistency;
using ExerciseEntity = GetFitterGetBigger.API.Models.Entities.Exercise;

namespace GetFitterGetBigger.API.Services.Exercise.DataServices;

/// <summary>
/// Data service implementation for Exercise write operations.
/// Encapsulates all database modifications and entity-to-DTO mapping.
/// </summary>
public class ExerciseCommandDataService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider) : IExerciseCommandDataService
{
    public async Task<ServiceResult<ExerciseDto>> CreateAsync(
        ExerciseEntity entity,
        ITransactionScope? scope = null)
    {
        return scope == null 
            ? await CreateWithoutScopeAsync(entity)
            : await CreateWithScopeAsync(entity, scope);
    }
    
    public async Task<ServiceResult<ExerciseDto>> UpdateAsync(
        ExerciseId id,
        Func<ExerciseEntity, ExerciseEntity> updateAction,
        ITransactionScope? scope = null)
    {
        return scope == null
            ? await UpdateWithoutScopeAsync(id, updateAction)
            : await UpdateWithScopeAsync(id, updateAction, scope);
    }
    
    public async Task<ServiceResult<BooleanResultDto>> SoftDeleteAsync(
        ExerciseId id,
        ITransactionScope? scope = null)
    {
        return scope == null
            ? await SoftDeleteWithoutScopeAsync(id)
            : await SoftDeleteWithScopeAsync(id, scope);
    }
    
    public async Task<ServiceResult<BooleanResultDto>> HardDeleteAsync(
        ExerciseId id,
        ITransactionScope? scope = null)
    {
        return scope == null
            ? await HardDeleteWithoutScopeAsync(id)
            : await HardDeleteWithScopeAsync(id, scope);
    }
    
    // Private helper methods for working with provided transaction scopes
    
    private async Task<ServiceResult<ExerciseDto>> CreateWithoutScopeAsync(
        ExerciseEntity entity)
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
    
    private async Task<ServiceResult<ExerciseDto>> CreateWithScopeAsync(
        ExerciseEntity entity,
        ITransactionScope scope)
    {
        // Validate scope
        if (scope.IsReadOnly)
        {
            return ServiceResult<ExerciseDto>.Failure(
                ExerciseDto.Empty,
                ServiceError.ValidationFailed("Cannot perform write operations with a read-only transaction scope"));
        }
        
        var unitOfWork = ((WritableTransactionScope)scope).UnitOfWork;
        var repository = unitOfWork.GetRepository<IExerciseRepository>();
        var createdExercise = await repository.AddAsync(entity);
        // Don't commit here - let the scope owner commit
        
        return ServiceResult<ExerciseDto>.Success(createdExercise.ToDto());
    }
    
    private async Task<ServiceResult<ExerciseDto>> UpdateWithoutScopeAsync(
        ExerciseId id,
        Func<ExerciseEntity, ExerciseEntity> updateAction)
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
    
    private async Task<ServiceResult<ExerciseDto>> UpdateWithScopeAsync(
        ExerciseId id,
        Func<ExerciseEntity, ExerciseEntity> updateAction,
        ITransactionScope scope)
    {
        // Validate scope
        if (scope.IsReadOnly)
        {
            return ServiceResult<ExerciseDto>.Failure(
                ExerciseDto.Empty,
                ServiceError.ValidationFailed("Cannot perform write operations with a read-only transaction scope"));
        }
        
        var unitOfWork = ((WritableTransactionScope)scope).UnitOfWork;
        var repository = unitOfWork.GetRepository<IExerciseRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        // Check if entity exists
        if (entity.IsEmpty)
        {
            return ServiceResult<ExerciseDto>.Failure(
                ExerciseDto.Empty,
                ServiceError.NotFound("Exercise", id.ToString()));
        }
        
        // Apply the update
        var updatedEntity = updateAction(entity);
        
        await repository.UpdateAsync(updatedEntity);
        // Don't commit here - let the scope owner commit
        
        // Reload the exercise with all navigation properties for proper mapping
        var reloadedExercise = await repository.GetByIdAsync(id);
        
        return ServiceResult<ExerciseDto>.Success(reloadedExercise.ToDto());
    }
    
    private async Task<ServiceResult<BooleanResultDto>> SoftDeleteWithoutScopeAsync(
        ExerciseId id)
    {
        using var unitOfWork = unitOfWorkProvider.CreateWritable();
        var scope = new WritableTransactionScope(unitOfWork);
        
        var result = await SoftDeleteWithScopeAsync(id, scope);
        
        if (result.IsSuccess)
        {
            await unitOfWork.CommitAsync();
        }
        
        return result;
    }
    
    private async Task<ServiceResult<BooleanResultDto>> SoftDeleteWithScopeAsync(
        ExerciseId id,
        ITransactionScope scope)
    {
        // Validate scope
        if (scope.IsReadOnly)
        {
            return ServiceResult<BooleanResultDto>.Failure(
                BooleanResultDto.Create(false),
                ServiceError.ValidationFailed("Cannot perform write operations with a read-only transaction scope"));
        }
        
        var unitOfWork = ((WritableTransactionScope)scope).UnitOfWork;
        var repository = unitOfWork.GetRepository<IExerciseRepository>();
        
        // Use repository method for soft delete to avoid relationship clearing
        await repository.SoftDeleteAsync(id);
        // Don't commit here - let the scope owner commit
        
        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true));
    }
    
    private async Task<ServiceResult<BooleanResultDto>> HardDeleteWithoutScopeAsync(
        ExerciseId id)
    {
        using var unitOfWork = unitOfWorkProvider.CreateWritable();
        var scope = new WritableTransactionScope(unitOfWork);
        
        var result = await HardDeleteWithScopeAsync(id, scope);
        
        if (result.IsSuccess)
        {
            await unitOfWork.CommitAsync();
        }
        
        return result;
    }
    
    private async Task<ServiceResult<BooleanResultDto>> HardDeleteWithScopeAsync(
        ExerciseId id,
        ITransactionScope scope)
    {
        // Validate scope
        if (scope.IsReadOnly)
        {
            return ServiceResult<BooleanResultDto>.Failure(
                BooleanResultDto.Create(false),
                ServiceError.ValidationFailed("Cannot perform write operations with a read-only transaction scope"));
        }
        
        var unitOfWork = ((WritableTransactionScope)scope).UnitOfWork;
        var repository = unitOfWork.GetRepository<IExerciseRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        // If entity is empty, return false (soft failure - not an error)
        if (entity.IsEmpty)
        {
            return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(false));
        }
        
        await repository.DeleteAsync(id);
        // Don't commit here - let the scope owner commit
        
        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true));
    }
}