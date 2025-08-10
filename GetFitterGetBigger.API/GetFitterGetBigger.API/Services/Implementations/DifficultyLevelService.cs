using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Service implementation for difficulty level operations
/// This service focuses solely on business logic and data access
/// Caching is handled by the wrapping DifficultyLevelReferenceService layer
/// </summary>
public class DifficultyLevelService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    ILogger<DifficultyLevelService> logger) : IDifficultyLevelService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;
    private readonly ILogger<DifficultyLevelService> _logger = logger;

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync()
    {
        return await ServiceValidate.Build<IEnumerable<ReferenceDataDto>>()
            .WhenValidAsync(async () => await LoadAllActiveFromDatabaseAsync());
    }
    
    /// <summary>
    /// Loads all active DifficultyLevels from the database and maps to DTOs
    /// </summary>
    private async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> LoadAllActiveFromDatabaseAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IDifficultyLevelRepository>();
        var entities = await repository.GetAllActiveAsync();
        
        var dtos = entities.Select(MapToDto);
        
        _logger.LogInformation("Loaded {Count} active difficulty levels", dtos.Count());
        return ServiceResult<IEnumerable<ReferenceDataDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(DifficultyLevelId id)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotEmpty(id, DifficultyLevelErrorMessages.InvalidIdFormat)
            .WithServiceResultAsync(() => LoadByIdFromDatabaseAsync(id))
            .ThenMatchDataAsync<ReferenceDataDto, ReferenceDataDto>(
                whenEmpty: () => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Failure(
                        ReferenceDataDto.Empty,
                        ServiceError.NotFound("DifficultyLevel", id.ToString()))),
                whenNotEmpty: dto => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Success(dto))
            );
    }
    
    /// <summary>
    /// Gets a difficulty level by its ID string
    /// </summary>
    /// <param name="id">The difficulty level ID as a string</param>
    /// <returns>A service result containing the difficulty level if found</returns>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(string id)
    {
        var difficultyLevelId = DifficultyLevelId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotEmpty(difficultyLevelId, DifficultyLevelErrorMessages.InvalidIdFormat)
            .WithServiceResultAsync(() => LoadByIdFromDatabaseAsync(difficultyLevelId))
            .ThenMatchDataAsync<ReferenceDataDto, ReferenceDataDto>(
                whenEmpty: () => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Failure(
                        ReferenceDataDto.Empty,
                        ServiceError.NotFound("DifficultyLevel", id))),
                whenNotEmpty: dto => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Success(dto))
            );
    }
    
    /// <summary>
    /// Loads a DifficultyLevel by ID from the database and maps to DTO
    /// Returns Success with Empty if the entity is not active - validation happens in the caller
    /// </summary>
    private async Task<ServiceResult<ReferenceDataDto>> LoadByIdFromDatabaseAsync(DifficultyLevelId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IDifficultyLevelRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        // Simply return what we found - let the caller decide how to handle empty results
        return entity.IsActive
            ? ServiceResult<ReferenceDataDto>.Success(MapToDto(entity))
            : ServiceResult<ReferenceDataDto>.Success(ReferenceDataDto.Empty);
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotWhiteSpace(value, DifficultyLevelErrorMessages.ValueCannotBeEmpty)
            .WithServiceResultAsync(() => LoadByValueFromDatabaseAsync(value))
            .ThenMatchDataAsync<ReferenceDataDto, ReferenceDataDto>(
                whenEmpty: () => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Failure(
                        ReferenceDataDto.Empty,
                        ServiceError.NotFound("DifficultyLevel", value))),
                whenNotEmpty: dto => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Success(dto))
            );
    }
    
    /// <summary>
    /// Loads a DifficultyLevel by value from the database and maps to DTO
    /// Returns Success with Empty if the entity is not active - validation happens in the caller
    /// </summary>
    private async Task<ServiceResult<ReferenceDataDto>> LoadByValueFromDatabaseAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IDifficultyLevelRepository>();
        var entity = await repository.GetByValueAsync(value);
        
        // Simply return what we found - let the caller decide how to handle empty results
        return entity.IsActive
            ? ServiceResult<ReferenceDataDto>.Success(MapToDto(entity))
            : ServiceResult<ReferenceDataDto>.Success(ReferenceDataDto.Empty);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(DifficultyLevelId id)
    {
        return await ServiceValidate.For<BooleanResultDto>()
            .EnsureNotEmpty(id, DifficultyLevelErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await CheckExistsInDatabaseAsync(id)
            );
    }
    
    /// <summary>
    /// Checks if a DifficultyLevel exists in the database by ID
    /// </summary>
    private async Task<ServiceResult<BooleanResultDto>> CheckExistsInDatabaseAsync(DifficultyLevelId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IDifficultyLevelRepository>();
        var exists = await repository.ExistsAsync(id);
        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(exists));
    }
    
    /// <summary>
    /// Maps a DifficultyLevel entity to its DTO representation
    /// Entity stays within the service layer - only DTO is exposed
    /// </summary>
    private ReferenceDataDto MapToDto(DifficultyLevel entity)
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