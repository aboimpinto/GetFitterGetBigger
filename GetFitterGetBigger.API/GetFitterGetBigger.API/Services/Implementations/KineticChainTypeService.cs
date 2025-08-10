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
/// Service implementation for kinetic_chain_type operations
/// This service focuses solely on business logic and data access
/// Caching is handled by the wrapping KineticChainTypeReferenceService layer
/// </summary>
public class KineticChainTypeService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    ILogger<KineticChainTypeService> logger) : IKineticChainTypeService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;
    private readonly ILogger<KineticChainTypeService> _logger = logger;

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IKineticChainTypeRepository>();
        var entities = await repository.GetAllActiveAsync();
        
        var dtos = entities.Select(MapToDto).ToList();
        
        _logger.LogInformation("Loaded {Count} active kinetic_chain_types", dtos.Count);
        return ServiceResult<IEnumerable<ReferenceDataDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(KineticChainTypeId id)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotEmpty(id, KineticChainTypeErrorMessages.InvalidIdFormat)
            .WithServiceResultAsync(() => LoadByIdFromDatabaseAsync(id))
            .ThenMatchDataAsync<ReferenceDataDto, ReferenceDataDto>(
                whenEmpty: () => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Failure(
                        ReferenceDataDto.Empty,
                        ServiceError.NotFound("KineticChainType", id.ToString()))),
                whenNotEmpty: dto => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Success(dto))
            );
    }
    
    /// <summary>
    /// Gets a kinetic_chain_type by its ID string
    /// </summary>
    /// <param name="id">The kinetic_chain_type ID as a string</param>
    /// <returns>A service result containing the kinetic_chain_type if found</returns>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(string id)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotWhiteSpace(id, KineticChainTypeErrorMessages.IdCannotBeEmpty)
            .MatchAsync(
                whenValid: async () =>
                {
                    var kinetic_chain_typeId = KineticChainTypeId.ParseOrEmpty(id);
                    return await ServiceValidate.For<ReferenceDataDto>()
                        .EnsureNotEmpty(kinetic_chain_typeId, KineticChainTypeErrorMessages.InvalidIdFormat)
                        .WithServiceResultAsync(() => LoadByIdFromDatabaseAsync(kinetic_chain_typeId))
                        .ThenMatchDataAsync<ReferenceDataDto, ReferenceDataDto>(
                            whenEmpty: () => Task.FromResult(
                                ServiceResult<ReferenceDataDto>.Failure(
                                    ReferenceDataDto.Empty,
                                    ServiceError.NotFound("KineticChainType", id))),
                            whenNotEmpty: dto => Task.FromResult(
                                ServiceResult<ReferenceDataDto>.Success(dto))
                        );
                }
            );
    }
    
    private async Task<ServiceResult<ReferenceDataDto>> LoadByIdFromDatabaseAsync(KineticChainTypeId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IKineticChainTypeRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        return entity.IsActive
            ? ServiceResult<ReferenceDataDto>.Success(MapToDto(entity))
            : ServiceResult<ReferenceDataDto>.Success(ReferenceDataDto.Empty);
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotWhiteSpace(value, KineticChainTypeErrorMessages.ValueCannotBeEmptyEntity)
            .WithServiceResultAsync(() => LoadByValueFromDatabaseAsync(value))
            .ThenMatchDataAsync<ReferenceDataDto, ReferenceDataDto>(
                whenEmpty: () => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Failure(
                        ReferenceDataDto.Empty,
                        ServiceError.NotFound("KineticChainType", value))),
                whenNotEmpty: dto => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Success(dto))
            );
    }
    
    private async Task<ServiceResult<ReferenceDataDto>> LoadByValueFromDatabaseAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IKineticChainTypeRepository>();
        var entity = await repository.GetByValueAsync(value);
        
        return entity.IsActive
            ? ServiceResult<ReferenceDataDto>.Success(MapToDto(entity))
            : ServiceResult<ReferenceDataDto>.Success(ReferenceDataDto.Empty);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(KineticChainTypeId id)
    {
        return await ServiceValidate.For<BooleanResultDto>()
            .EnsureNotEmpty(id, KineticChainTypeErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () =>
                {
                    using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
                    var repository = unitOfWork.GetRepository<IKineticChainTypeRepository>();
                    var exists = await repository.ExistsAsync(id);
                    return ServiceResult<BooleanResultDto>.Success(new BooleanResultDto { Value = exists });
                }
            );
    }
    
    /// <summary>
    /// Maps a KineticChainType entity to its DTO representation
    /// Entity stays within the service layer - only DTO is exposed
    /// </summary>
    private ReferenceDataDto MapToDto(KineticChainType entity)
    {
        return new ReferenceDataDto
        {
            Id = entity.KineticChainTypeId.ToString(),
            Value = entity.Value,
            Description = entity.Description
        };
    }
}
