using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.ReferenceTables.DifficultyLevel.DataServices;

/// <summary>
/// Data service implementation for DifficultyLevel database operations
/// Manages all data access concerns including UnitOfWork and Repository interactions
/// </summary>
public class DifficultyLevelDataService : IDifficultyLevelDataService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly ILogger<DifficultyLevelDataService> _logger;

    public DifficultyLevelDataService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ILogger<DifficultyLevelDataService> logger)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IDifficultyLevelRepository>();
        var entities = await repository.GetAllActiveAsync();
        
        var dtos = entities.Select(MapToDto).ToList();
        
        _logger.LogInformation("Loaded {Count} active difficulty levels from database", dtos.Count);
        return ServiceResult<IEnumerable<ReferenceDataDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(DifficultyLevelId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IDifficultyLevelRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        var dto = entity.IsActive ? MapToDto(entity) : ReferenceDataDto.Empty;
        
        _logger.LogDebug("Retrieved difficulty level by ID {Id}: {Found}", id, !dto.IsEmpty);
        return ServiceResult<ReferenceDataDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IDifficultyLevelRepository>();
        var entity = await repository.GetByValueAsync(value);

        var dto = entity.IsActive ? MapToDto(entity) : ReferenceDataDto.Empty;
        
        _logger.LogDebug("Retrieved difficulty level by value '{Value}': {Found}", value, !dto.IsEmpty);
        return ServiceResult<ReferenceDataDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(DifficultyLevelId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IDifficultyLevelRepository>();
        var exists = await repository.ExistsAsync(id);
        
        _logger.LogDebug("Checked existence of difficulty level {Id}: {Exists}", id, exists);
        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(exists));
    }
    
    /// <summary>
    /// Maps a DifficultyLevel entity to its DTO representation
    /// Entity stays within the data layer - only DTO is exposed
    /// </summary>
    private static ReferenceDataDto MapToDto(Models.Entities.DifficultyLevel entity)
    {
        if (entity.IsEmpty)
            return ReferenceDataDto.Empty;
            
        return new ReferenceDataDto
        {
            Id = entity.DifficultyLevelId.ToString(),
            Value = entity.Value,
            Description = entity.Description
        };
    }
}