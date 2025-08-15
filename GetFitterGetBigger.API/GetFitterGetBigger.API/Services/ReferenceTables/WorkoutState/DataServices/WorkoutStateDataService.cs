using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.ReferenceTables.WorkoutState.DataServices;

/// <summary>
/// Data service implementation for WorkoutState database operations
/// Manages all data access concerns including UnitOfWork and Repository interactions
/// WorkoutStates are pure reference data (read-only) that never changes after deployment
/// </summary>
public class WorkoutStateDataService : IWorkoutStateDataService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly ILogger<WorkoutStateDataService> _logger;

    public WorkoutStateDataService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ILogger<WorkoutStateDataService> logger)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<WorkoutStateDto>>> GetAllActiveAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutStateRepository>();
        var entities = await repository.GetAllActiveAsync();
        
        var dtos = entities.Select(MapToDto).ToList();
        
        _logger.LogInformation("Loaded {Count} active workout states from database", dtos.Count);
        return ServiceResult<IEnumerable<WorkoutStateDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<WorkoutStateDto>> GetByIdAsync(WorkoutStateId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutStateRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        // Pattern matching - trust repository returns Empty, never null
        var dto = entity switch
        {
            { IsActive: false } => MapToDto(Models.Entities.WorkoutState.Empty),
            _ => MapToDto(entity)
        };
        
        _logger.LogDebug("Retrieved workout state by ID {Id}: {Found}", id, !dto.IsEmpty);
        return ServiceResult<WorkoutStateDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<WorkoutStateDto>> GetByValueAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutStateRepository>();
        var entity = await repository.GetByValueAsync(value);
        
        // Clean pattern matching approach
        var dto = entity switch
        {
            { IsActive: false } => MapToDto(Models.Entities.WorkoutState.Empty),
            _ => MapToDto(entity)
        };
        
        _logger.LogDebug("Retrieved workout state by value '{Value}': {Found}", value, !dto.IsEmpty);
        return ServiceResult<WorkoutStateDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(WorkoutStateId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutStateRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        // Pattern matching for clean existence check
        var exists = entity is { IsActive: true };
        
        _logger.LogDebug("Checked existence of workout state {Id}: {Exists}", id, exists);
        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(exists));
    }

    /// <summary>
    /// Maps a WorkoutState entity to its DTO representation
    /// Entity stays within the data layer - only DTO is exposed
    /// </summary>
    private static WorkoutStateDto MapToDto(Models.Entities.WorkoutState entity)
    {
        if (entity == null || entity.IsEmpty)
            return WorkoutStateDto.Empty;
            
        return new WorkoutStateDto
        {
            Id = entity.WorkoutStateId.ToString(),
            Value = entity.Value,
            Description = entity.Description
        };
    }
}