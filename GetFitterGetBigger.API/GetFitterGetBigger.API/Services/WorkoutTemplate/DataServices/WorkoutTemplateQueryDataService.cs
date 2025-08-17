using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Extensions;
using Microsoft.EntityFrameworkCore;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.DataServices;

/// <summary>
/// Data service implementation for WorkoutTemplate read operations.
/// Encapsulates all database queries and entity-to-DTO mapping.
/// </summary>
public class WorkoutTemplateQueryDataService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider) : IWorkoutTemplateQueryDataService
{
    
    public async Task<ServiceResult<WorkoutTemplateDto>> GetByIdWithDetailsAsync(WorkoutTemplateId id)
    {
        using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var workoutTemplate = await repository.GetByIdWithDetailsAsync(id);
        var dto = workoutTemplate.ToDto();
        return ServiceResult<WorkoutTemplateDto>.Success(dto);
    }
    
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(WorkoutTemplateId id)
    {
        using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var exists = await repository.ExistsAsync(id);
        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(exists));
    }
    
    public async Task<ServiceResult<BooleanResultDto>> ExistsByNameAsync(string name)
    {
        using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var exists = await repository.ExistsByNameAsync(name);
        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(exists));
    }
    
    public async Task<ServiceResult<PagedResponse<WorkoutTemplateDto>>> SearchAsync(
        int page,
        int pageSize,
        string namePattern,
        WorkoutCategoryId categoryId,
        WorkoutObjectiveId objectiveId,
        DifficultyLevelId difficultyId,
        WorkoutStateId stateId,
        string sortBy,
        string sortOrder)
    {
        using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        // Build query with filters and elegant sorting chain
        var query = repository.GetWorkoutTemplatesQueryable()
            .FilterByNamePattern(namePattern)
            .FilterByCategory(categoryId)
            .FilterByObjective(objectiveId)
            .FilterByDifficulty(difficultyId)
            .FilterByState(stateId)
            .ToSortable()
            .ApplySortByName(sortBy, sortOrder)
            .ApplySortByCreatedAt(sortBy, sortOrder)
            .ApplySortByUpdatedAt(sortBy, sortOrder)
            .ApplySortByDifficulty(sortBy, sortOrder)
            .ApplySortByCategory(sortBy, sortOrder)
            .WithDefaultWorkoutTemplateSort();
        
        // Get total count
        var totalCount = await query.CountAsync();
        
        // Apply pagination
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .IncludeStandardData()
            .ToListAsync();
        
        var dtos = items.Select(wt => wt.ToDto()).ToList();
        
        var pagedResponse = new PagedResponse<WorkoutTemplateDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            CurrentPage = page,
            PageSize = pageSize,
            // TotalPages is computed property
        };
        
        return ServiceResult<PagedResponse<WorkoutTemplateDto>>.Success(pagedResponse);
    }
    
    public async Task<ServiceResult<int>> GetCountAsync(
        string? namePattern = null,
        WorkoutCategoryId? categoryId = null,
        WorkoutObjectiveId? objectiveId = null,
        DifficultyLevelId? difficultyId = null,
        WorkoutStateId? stateId = null)
    {
        using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        // Build query with individual, visible filter methods
        var query = repository.GetWorkoutTemplatesQueryable()
            .FilterByNamePattern(namePattern)
            .FilterByCategory(categoryId)
            .FilterByObjective(objectiveId)
            .FilterByDifficulty(difficultyId)
            .FilterByState(stateId);
        
        var count = await query.CountAsync();
        return ServiceResult<int>.Success(count);
    }
    
    public Task<ServiceResult<BooleanResultDto>> HasExecutionLogsAsync(WorkoutTemplateId id)
    {
        using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        // This would need to check WorkoutLog table when it exists
        // For now, returning false as placeholder
        var hasLogs = false; // await repository.HasExecutionLogsAsync(id);
        
        return Task.FromResult(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(hasLogs)));
    }
    
    public async Task<ServiceResult<WorkoutStateId>> GetStateAsync(WorkoutTemplateId id)
    {
        using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var template = await repository.GetByIdAsync(id);
        var stateId = template.IsEmpty ? WorkoutStateId.Empty : template.WorkoutStateId;
        return ServiceResult<WorkoutStateId>.Success(stateId);
    }
    
}