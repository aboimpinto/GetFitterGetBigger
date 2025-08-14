using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.ReferenceTables.WorkoutCategory.DataServices;

/// <summary>
/// Data service implementation for WorkoutCategory database operations
/// Manages all data access concerns including UnitOfWork and Repository interactions
/// WorkoutCategories are pure reference data (read-only) that never changes after deployment
/// </summary>
public class WorkoutCategoryDataService : IWorkoutCategoryDataService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly ILogger<WorkoutCategoryDataService> _logger;

    public WorkoutCategoryDataService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ILogger<WorkoutCategoryDataService> logger)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<WorkoutCategoryDto>>> GetAllActiveAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutCategoryRepository>();
        var entities = await repository.GetAllActiveAsync();
        
        var dtos = entities.Select(MapToDto).ToList();
        
        _logger.LogInformation("Loaded {Count} active workout categories from database", dtos.Count);
        return ServiceResult<IEnumerable<WorkoutCategoryDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<WorkoutCategoryDto>> GetByIdAsync(WorkoutCategoryId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutCategoryRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        // Only return active entities - inactive entities should be treated as not found
        if (entity != null && !entity.IsActive)
        {
            _logger.LogDebug("Found inactive workout category by ID {Id} - returning Empty", id);
            entity = Models.Entities.WorkoutCategory.Empty;
        }
        
        var dto = MapToDto(entity);
        
        _logger.LogDebug("Retrieved workout category by ID {Id}: {Found}", id, !dto.IsEmpty);
        return ServiceResult<WorkoutCategoryDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<WorkoutCategoryDto>> GetByValueAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutCategoryRepository>();
        var entity = await repository.GetByValueAsync(value);
        
        // Only return active entities - inactive entities should be treated as not found
        if (entity != null && !entity.IsActive)
        {
            _logger.LogDebug("Found inactive workout category by value '{Value}' - returning Empty", value);
            entity = Models.Entities.WorkoutCategory.Empty;
        }
        
        var dto = MapToDto(entity);
        
        _logger.LogDebug("Retrieved workout category by value '{Value}': {Found}", value, !dto.IsEmpty);
        return ServiceResult<WorkoutCategoryDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(WorkoutCategoryId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutCategoryRepository>();
        var entity = await repository.GetByIdAsync(id);
        var exists = entity != null && entity.IsActive;
        
        _logger.LogDebug("Checked existence of workout category {Id}: {Exists}", id, exists);
        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(exists));
    }

    /// <summary>
    /// Maps a WorkoutCategory entity to its DTO representation
    /// Entity stays within the data layer - only DTO is exposed
    /// </summary>
    private static WorkoutCategoryDto MapToDto(Models.Entities.WorkoutCategory entity)
    {
        if (entity == null || entity.IsEmpty)
            return WorkoutCategoryDto.Empty;
            
        return new WorkoutCategoryDto
        {
            WorkoutCategoryId = entity.WorkoutCategoryId.ToString(),
            Value = entity.Value,
            Description = entity.Description,
            Icon = entity.Icon,
            Color = entity.Color,
            PrimaryMuscleGroups = entity.PrimaryMuscleGroups,
            DisplayOrder = entity.DisplayOrder,
            IsActive = entity.IsActive
        };
    }
}