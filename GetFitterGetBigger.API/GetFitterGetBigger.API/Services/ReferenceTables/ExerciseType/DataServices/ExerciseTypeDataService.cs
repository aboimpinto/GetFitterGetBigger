using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.ReferenceTables.ExerciseType.DataServices;

/// <summary>
/// Data service implementation for ExerciseType database operations
/// Manages all data access concerns including UnitOfWork and Repository interactions
/// </summary>
public class ExerciseTypeDataService : IExerciseTypeDataService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly ILogger<ExerciseTypeDataService> _logger;

    public ExerciseTypeDataService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ILogger<ExerciseTypeDataService> logger)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ExerciseTypeDto>>> GetAllActiveAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseTypeRepository>();
        var entities = await repository.GetAllActiveAsync();
        
        var dtos = entities.Select(MapToDto).ToList();
        
        _logger.LogInformation("Loaded {Count} active exercise types from database", dtos.Count);
        return ServiceResult<IEnumerable<ExerciseTypeDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ExerciseTypeDto>> GetByIdAsync(ExerciseTypeId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseTypeRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        var dto = entity.IsActive ? MapToDto(entity) : ExerciseTypeDto.Empty;
        
        _logger.LogDebug("Retrieved exercise type by ID {Id}: {Found}", id, !dto.IsEmpty);
        return ServiceResult<ExerciseTypeDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ExerciseTypeDto>> GetByValueAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseTypeRepository>();
        var entity = await repository.GetByValueAsync(value);

        var dto = entity.IsActive ? MapToDto(entity) : ExerciseTypeDto.Empty;
        
        _logger.LogDebug("Retrieved exercise type by value '{Value}': {Found}", value, !dto.IsEmpty);
        return ServiceResult<ExerciseTypeDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(ExerciseTypeId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseTypeRepository>();
        var exists = await repository.ExistsAsync(id);
        
        _logger.LogDebug("Checked existence of exercise type {Id}: {Exists}", id, exists);
        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(exists));
    }
    
    /// <inheritdoc/>
    public async Task<bool> AllExistAsync(IEnumerable<ExerciseTypeId> ids)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseTypeRepository>();
        
        foreach (var id in ids)
        {
            var exists = await repository.ExistsAsync(id);
            if (!exists)
                return false;
        }
        
        return true;
    }
    
    /// <inheritdoc/>
    public async Task<bool> AnyIsRestTypeAsync(IEnumerable<ExerciseTypeId> ids)
    {
        // The REST exercise type has a fixed ID that never changes
        var restTypeId = ExerciseTypeId.ParseOrEmpty("exercisetype-d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a");
        
        // Simply check if any of the provided IDs match the REST type ID
        return ids.Any(id => id.Equals(restTypeId));
    }
    
    /// <summary>
    /// Maps an ExerciseType entity to its DTO representation
    /// Entity stays within the data layer - only DTO is exposed
    /// </summary>
    private static ExerciseTypeDto MapToDto(Models.Entities.ExerciseType entity)
    {
        if (entity.IsEmpty)
            return ExerciseTypeDto.Empty;
            
        return new ExerciseTypeDto
        {
            Id = entity.ExerciseTypeId.ToString(),
            Value = entity.Value,
            Description = entity.Description
        };
    }
}