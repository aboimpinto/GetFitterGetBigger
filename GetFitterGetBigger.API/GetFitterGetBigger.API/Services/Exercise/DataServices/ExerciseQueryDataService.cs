using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Commands;
using GetFitterGetBigger.API.Services.Implementations.Extensions;
using GetFitterGetBigger.API.Services.Results;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Exercise.DataServices;

/// <summary>
/// Data service implementation for Exercise read operations.
/// Encapsulates all database queries and entity-to-DTO mapping.
/// </summary>
public class ExerciseQueryDataService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider) : IExerciseQueryDataService
{
    public async Task<ServiceResult<PagedResponse<ExerciseDto>>> GetPagedAsync(GetExercisesCommand filterParams)
    {
        using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseRepository>();

        var (exercises, totalCount) = await repository.GetPagedAsync(
            filterParams.Page,
            filterParams.PageSize,
            filterParams.Name,
            filterParams.DifficultyLevelId,
            filterParams.MuscleGroupIds,
            filterParams.EquipmentIds,
            filterParams.MovementPatternIds,
            filterParams.BodyPartIds,
            filterParams.IsActive);

        var exerciseDtos = exercises
            .Select(e => e.ToDto())
            .ToList();

        var response = new PagedResponse<ExerciseDto>
        {
            Items = exerciseDtos,
            TotalCount = totalCount,
            PageSize = filterParams.PageSize,
            CurrentPage = filterParams.Page
        };

        return ServiceResult<PagedResponse<ExerciseDto>>.Success(response);
    }
    
    public async Task<ServiceResult<ExerciseDto>> GetByIdAsync(ExerciseId id)
    {
        using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseRepository>();
        
        var exercise = await repository.GetByIdAsync(id);
        var dto = exercise.IsEmpty ? ExerciseDto.Empty : exercise.ToDto();
        
        return ServiceResult<ExerciseDto>.Success(dto);
    }
    
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(ExerciseId id)
    {
        using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseRepository>();
        
        var exercise = await repository.GetByIdAsync(id);
        var exists = !exercise.IsEmpty;
        
        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(exists));
    }
    
    public async Task<ServiceResult<BooleanResultDto>> ExistsByNameAsync(string name, ExerciseId? excludeId = null)
    {
        using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseRepository>();
        
        var exists = await repository.ExistsAsync(name, excludeId);
        
        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(exists));
    }
}