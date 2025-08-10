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
/// Service implementation for muscle_role operations
/// This service focuses solely on business logic and data access
/// Caching is handled by the wrapping MuscleRoleReferenceService layer
/// </summary>
public class MuscleRoleService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    ILogger<MuscleRoleService> logger) : IMuscleRoleService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;
    private readonly ILogger<MuscleRoleService> _logger = logger;

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleRoleRepository>();
        var entities = await repository.GetAllActiveAsync();
        
        var dtos = entities.Select(MapToDto).ToList();
        
        _logger.LogInformation("Loaded {Count} active muscle_roles", dtos.Count);
        return ServiceResult<IEnumerable<ReferenceDataDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(MuscleRoleId id)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotEmpty(id, MuscleRoleErrorMessages.InvalidIdFormat)
            .WithServiceResultAsync(() => LoadByIdFromDatabaseAsync(id))
            .ThenMatchDataAsync<ReferenceDataDto, ReferenceDataDto>(
                whenEmpty: () => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Failure(
                        ReferenceDataDto.Empty,
                        ServiceError.NotFound("MuscleRole", id.ToString()))),
                whenNotEmpty: dto => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Success(dto))
            );
    }
    
    /// <summary>
    /// Gets a muscle_role by its ID string
    /// </summary>
    /// <param name="id">The muscle_role ID as a string</param>
    /// <returns>A service result containing the muscle_role if found</returns>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(string id)
    {
        var muscleRoleId = MuscleRoleId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotEmpty(muscleRoleId, MuscleRoleErrorMessages.InvalidIdFormat)
            .WithServiceResultAsync(() => LoadByIdFromDatabaseAsync(muscleRoleId))
            .ThenMatchDataAsync<ReferenceDataDto, ReferenceDataDto>(
                whenEmpty: () => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Failure(
                        ReferenceDataDto.Empty,
                        ServiceError.NotFound("MuscleRole", muscleRoleId.ToString()))),
                whenNotEmpty: dto => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Success(dto))
            );
    }
    
    private async Task<ServiceResult<ReferenceDataDto>> LoadByIdFromDatabaseAsync(MuscleRoleId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleRoleRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        return entity.IsActive
            ? ServiceResult<ReferenceDataDto>.Success(MapToDto(entity))
            : ServiceResult<ReferenceDataDto>.Success(ReferenceDataDto.Empty);
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotWhiteSpace(value, MuscleRoleErrorMessages.ValueCannotBeEmpty)
            .WithServiceResultAsync(() => LoadByValueFromDatabaseAsync(value))
            .ThenMatchDataAsync<ReferenceDataDto, ReferenceDataDto>(
                whenEmpty: () => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Failure(
                        ReferenceDataDto.Empty,
                        ServiceError.NotFound("MuscleRole", value))),
                whenNotEmpty: dto => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Success(dto))
            );
    }
    
    private async Task<ServiceResult<ReferenceDataDto>> LoadByValueFromDatabaseAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleRoleRepository>();
        var entity = await repository.GetByValueAsync(value);
        
        return entity.IsActive
            ? ServiceResult<ReferenceDataDto>.Success(MapToDto(entity))
            : ServiceResult<ReferenceDataDto>.Success(ReferenceDataDto.Empty);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(MuscleRoleId id)
    {
        return await ServiceValidate.For<BooleanResultDto>()
            .EnsureNotEmpty(id, MuscleRoleErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () =>
                {
                    using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
                    var repository = unitOfWork.GetRepository<IMuscleRoleRepository>();
                    var exists = await repository.ExistsAsync(id);
                    return ServiceResult<BooleanResultDto>.Success(new BooleanResultDto { Value = exists });
                });
    }
    
    /// <summary>
    /// Maps a MuscleRole entity to its DTO representation
    /// Entity stays within the service layer - only DTO is exposed
    /// </summary>
    private ReferenceDataDto MapToDto(MuscleRole entity)
    {
        return new ReferenceDataDto
        {
            Id = entity.MuscleRoleId.ToString(),
            Value = entity.Value,
            Description = entity.Description
        };
    }
}
