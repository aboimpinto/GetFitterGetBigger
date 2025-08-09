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
            .MatchAsync(
                whenValid: async () => await LoadByIdFromDatabaseAsync(id)
            );
    }
    
    /// <summary>
    /// Gets a muscle_role by its ID string
    /// </summary>
    /// <param name="id">The muscle_role ID as a string</param>
    /// <returns>A service result containing the muscle_role if found</returns>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return ServiceResult<ReferenceDataDto>.Failure(
                ReferenceDataDto.Empty,
                ServiceError.ValidationFailed(MuscleRoleErrorMessages.IdCannotBeEmpty));
        }
        
        var muscle_roleId = MuscleRoleId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotEmpty(muscle_roleId, MuscleRoleErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await LoadByIdFromDatabaseAsync(muscle_roleId)
            );
    }
    
    private async Task<ServiceResult<ReferenceDataDto>> LoadByIdFromDatabaseAsync(MuscleRoleId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleRoleRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        return (entity == null || entity.IsEmpty || !entity.IsActive) switch
        {
            true => ServiceResult<ReferenceDataDto>.Failure(
                ReferenceDataDto.Empty,
                ServiceError.NotFound("MuscleRole", id.ToString())),
            false => ServiceResult<ReferenceDataDto>.Success(MapToDto(entity))
        };
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotWhiteSpace(value, MuscleRoleErrorMessages.ValueCannotBeEmpty)
            .MatchAsync(
                whenValid: async () => await LoadByValueFromDatabaseAsync(value)
            );
    }
    
    private async Task<ServiceResult<ReferenceDataDto>> LoadByValueFromDatabaseAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleRoleRepository>();
        var entity = await repository.GetByValueAsync(value);
        
        return (entity == null || entity.IsEmpty || !entity.IsActive) switch
        {
            true => ServiceResult<ReferenceDataDto>.Failure(
                ReferenceDataDto.Empty,
                ServiceError.NotFound("MuscleRole", value)),
            false => ServiceResult<ReferenceDataDto>.Success(MapToDto(entity))
        };
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<bool>> ExistsAsync(MuscleRoleId id)
    {
        return await ServiceValidate.Build<bool>()
            .EnsureNotEmpty(id, MuscleRoleErrorMessages.InvalidIdFormat)
            .WhenValidAsync(async () =>
            {
                using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
                var repository = unitOfWork.GetRepository<IMuscleRoleRepository>();
                var exists = await repository.ExistsAsync(id);
                return ServiceResult<bool>.Success(exists);
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
