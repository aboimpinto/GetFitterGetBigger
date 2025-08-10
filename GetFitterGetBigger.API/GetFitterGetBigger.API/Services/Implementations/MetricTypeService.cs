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
/// Service implementation for metric_type operations
/// This service focuses solely on business logic and data access
/// Caching is handled by the wrapping MetricTypeReferenceService layer
/// </summary>
public class MetricTypeService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    ILogger<MetricTypeService> logger) : IMetricTypeService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;
    private readonly ILogger<MetricTypeService> _logger = logger;

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMetricTypeRepository>();
        var entities = await repository.GetAllActiveAsync();
        
        var dtos = entities.Select(MapToDto).ToList();
        
        _logger.LogInformation("Loaded {Count} active metric_types", dtos.Count);
        return ServiceResult<IEnumerable<ReferenceDataDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(MetricTypeId id)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotEmpty(id, MetricTypeErrorMessages.InvalidIdFormat)
            .WithServiceResultAsync(() => LoadByIdFromDatabaseAsync(id))
            .ThenMatchDataAsync<ReferenceDataDto, ReferenceDataDto>(
                whenEmpty: () => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Failure(
                        ReferenceDataDto.Empty,
                        ServiceError.NotFound("MetricType", id.ToString()))),
                whenNotEmpty: dto => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Success(dto))
            );
    }
    
    /// <summary>
    /// Gets a metric_type by its ID string
    /// </summary>
    /// <param name="id">The metric_type ID as a string</param>
    /// <returns>A service result containing the metric_type if found</returns>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(string id)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotWhiteSpace(id, MetricTypeErrorMessages.IdCannotBeEmpty)
            .Ensure(() => MetricTypeId.ParseOrEmpty(id) != MetricTypeId.Empty, MetricTypeErrorMessages.InvalidIdFormat)
            .WithServiceResultAsync(() => LoadByIdFromDatabaseAsync(MetricTypeId.ParseOrEmpty(id)))
            .ThenMatchDataAsync<ReferenceDataDto, ReferenceDataDto>(
                whenEmpty: () => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Failure(
                        ReferenceDataDto.Empty,
                        ServiceError.NotFound("MetricType", id))),
                whenNotEmpty: dto => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Success(dto))
            );
    }
    
    private async Task<ServiceResult<ReferenceDataDto>> LoadByIdFromDatabaseAsync(MetricTypeId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMetricTypeRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        // Clean separation: business logic only, no IsEmpty check
        return entity.IsActive
            ? ServiceResult<ReferenceDataDto>.Success(MapToDto(entity))
            : ServiceResult<ReferenceDataDto>.Success(ReferenceDataDto.Empty);
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotWhiteSpace(value, MetricTypeErrorMessages.ValueCannotBeEmpty)
            .WithServiceResultAsync(() => LoadByValueFromDatabaseAsync(value))
            .ThenMatchDataAsync<ReferenceDataDto, ReferenceDataDto>(
                whenEmpty: () => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Failure(
                        ReferenceDataDto.Empty,
                        ServiceError.NotFound("MetricType", value))),
                whenNotEmpty: dto => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Success(dto))
            );
    }
    
    private async Task<ServiceResult<ReferenceDataDto>> LoadByValueFromDatabaseAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMetricTypeRepository>();
        var entity = await repository.GetByValueAsync(value);
        
        // Clean separation: business logic only, no IsEmpty check
        return entity.IsActive
            ? ServiceResult<ReferenceDataDto>.Success(MapToDto(entity))
            : ServiceResult<ReferenceDataDto>.Success(ReferenceDataDto.Empty);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(MetricTypeId id)
    {
        return await ServiceValidate.For<BooleanResultDto>()
            .EnsureNotEmpty(id, MetricTypeErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () =>
                {
                    using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
                    var repository = unitOfWork.GetRepository<IMetricTypeRepository>();
                    var exists = await repository.ExistsAsync(id);
                    return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(exists));
                });
    }
    
    /// <summary>
    /// Maps a MetricType entity to its DTO representation
    /// Entity stays within the service layer - only DTO is exposed
    /// </summary>
    private ReferenceDataDto MapToDto(MetricType entity)
    {
        return new ReferenceDataDto
        {
            Id = entity.MetricTypeId.ToString(),
            Value = entity.Value,
            Description = entity.Description
        };
    }
}
