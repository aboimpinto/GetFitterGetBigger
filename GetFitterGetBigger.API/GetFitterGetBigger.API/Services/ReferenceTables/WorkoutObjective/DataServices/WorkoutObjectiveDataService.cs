using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.ReferenceTables.WorkoutObjective.DataServices;

/// <summary>
/// Data service implementation for WorkoutObjective database operations
/// Manages all data access concerns including UnitOfWork and Repository interactions
/// WorkoutObjectives are pure reference data (read-only) that never changes after deployment
/// </summary>
public class WorkoutObjectiveDataService : IWorkoutObjectiveDataService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly ILogger<WorkoutObjectiveDataService> _logger;

    public WorkoutObjectiveDataService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ILogger<WorkoutObjectiveDataService> logger)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutObjectiveRepository>();
        var entities = await repository.GetAllActiveAsync();
        
        var dtos = entities.Select(MapToDto).ToList();
        
        _logger.LogInformation("Loaded {Count} active workout objectives from database", dtos.Count);
        return ServiceResult<IEnumerable<ReferenceDataDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(WorkoutObjectiveId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutObjectiveRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        // Only return active entities - inactive entities should be treated as not found
        if (entity != null && !entity.IsActive)
        {
            _logger.LogDebug("Found inactive workout objective by ID {Id} - returning Empty", id);
            entity = Models.Entities.WorkoutObjective.Empty;
        }
        
        var dto = MapToDto(entity);
        
        _logger.LogDebug("Retrieved workout objective by ID {Id}: {Found}", id, !dto.IsEmpty);
        return ServiceResult<ReferenceDataDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutObjectiveRepository>();
        var entity = await repository.GetByValueAsync(value);
        
        // Only return active entities - inactive entities should be treated as not found
        if (entity != null && !entity.IsActive)
        {
            _logger.LogDebug("Found inactive workout objective by value '{Value}' - returning Empty", value);
            entity = Models.Entities.WorkoutObjective.Empty;
        }
        
        var dto = MapToDto(entity);
        
        _logger.LogDebug("Retrieved workout objective by value '{Value}': {Found}", value, !dto.IsEmpty);
        return ServiceResult<ReferenceDataDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(WorkoutObjectiveId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutObjectiveRepository>();
        var entity = await repository.GetByIdAsync(id);
        var exists = entity != null && entity.IsActive;
        
        _logger.LogDebug("Checked existence of workout objective {Id}: {Exists}", id, exists);
        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(exists));
    }

    /// <summary>
    /// Maps a WorkoutObjective entity to its DTO representation
    /// Entity stays within the data layer - only DTO is exposed
    /// </summary>
    private static ReferenceDataDto MapToDto(Models.Entities.WorkoutObjective entity)
    {
        if (entity == null || entity.IsEmpty)
            return ReferenceDataDto.Empty;
            
        return new ReferenceDataDto
        {
            Id = entity.WorkoutObjectiveId.ToString(),
            Value = entity.Value,
            Description = entity.Description
        };
    }
}