using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Extensions;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Handlers;

/// <summary>
/// Handles search queries for workout templates
/// </summary>
public class SearchQueryBuilder(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    ILogger<SearchQueryBuilder> logger)
{

    /// <summary>
    /// Searches for workout templates with filtering, sorting, and pagination
    /// </summary>
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
        
        // Build query with filters and sorting (but NOT paging yet)
        var workoutTemplatesQuery = repository
            .GetWorkoutTemplatesQueryable()
            .ApplyNamePatternFilter(namePattern)
            .ApplyCategoryFilter(categoryId)
            .ApplyDifficultyFilter(difficultyId)
            .ApplyObjectiveFilter(objectiveId);
        
        // Apply state filter or exclude archived by default
        workoutTemplatesQuery = stateId.IsEmpty 
            ? workoutTemplatesQuery.ExcludeArchived()
            : workoutTemplatesQuery.ApplyStateFilter(stateId);
            
        workoutTemplatesQuery = workoutTemplatesQuery.ApplySorting(sortBy, sortOrder);
        
        // Log for SQL verification (will help verify EF Core behavior)
        logger.LogDebug("Executing count query for workout templates search");
        var totalCount = await workoutTemplatesQuery.CountAsync();
        logger.LogDebug("Count query completed. Total: {TotalCount}", totalCount);
        
        // Now apply paging and execute
        logger.LogDebug("Executing paged query for workout templates");
        var items = await workoutTemplatesQuery
            .ApplyPaging(page, pageSize)
            .ToListAsync();
        logger.LogDebug("Paged query completed. Retrieved: {ItemCount} items", items.Count);
        
        var response = new PagedResponse<WorkoutTemplateDto>
        {
            Items = items.Select(t => t.ToDto()).ToList(),
            TotalCount = totalCount,
            CurrentPage = page,
            PageSize = pageSize
        };
        
        return ServiceResult<PagedResponse<WorkoutTemplateDto>>.Success(response);
    }

    /// <summary>
    /// Gets paged workout templates
    /// </summary>
    public async Task<ServiceResult<PagedResponse<WorkoutTemplateDto>>> GetPagedAsync(
        int page,
        int pageSize)
    {
        using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var query = repository
            .GetWorkoutTemplatesQueryable()
            .OrderByDescending(t => t.UpdatedAt);
        
        var totalCount = await query.CountAsync();
        
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        var response = new PagedResponse<WorkoutTemplateDto>
        {
            Items = items.Select(t => t.ToDto()).ToList(),
            TotalCount = totalCount,
            CurrentPage = page,
            PageSize = pageSize
        };
        
        return ServiceResult<PagedResponse<WorkoutTemplateDto>>.Success(response);
    }

    /// <summary>
    /// Gets recent workout templates
    /// </summary>
    public async Task<ServiceResult<IEnumerable<WorkoutTemplateDto>>> GetRecentAsync(int count = 10)
    {
        if (count <= 0)
        {
            return ServiceResult<IEnumerable<WorkoutTemplateDto>>.Failure(
                [],
                ServiceError.ValidationFailed("Count must be greater than 0"));
        }

        using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var templates = await repository
            .GetWorkoutTemplatesQueryable()
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .ToListAsync();
        
        var dtos = templates.Select(t => t.ToDto()).ToList();
        
        return ServiceResult<IEnumerable<WorkoutTemplateDto>>.Success(dtos);
    }

    /// <summary>
    /// Gets popular workout templates based on usage count
    /// </summary>
    public async Task<ServiceResult<IEnumerable<WorkoutTemplateDto>>> GetPopularAsync(int count = 10)
    {
        if (count <= 0)
        {
            return ServiceResult<IEnumerable<WorkoutTemplateDto>>.Failure(
                [],
                ServiceError.ValidationFailed("Count must be greater than 0"));
        }

        using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        // For now, we'll return public templates ordered by update time
        // In the future, this could incorporate workout completion counts, ratings, etc.
        var templates = await repository
            .GetWorkoutTemplatesQueryable()
            .Where(t => t.IsPublic)
            .OrderByDescending(t => t.UpdatedAt)
            .Take(count)
            .ToListAsync();
        
        var dtos = templates.Select(t => t.ToDto()).ToList();
        
        return ServiceResult<IEnumerable<WorkoutTemplateDto>>.Success(dtos);
    }
}