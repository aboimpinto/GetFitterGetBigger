using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.MuscleGroup;
using GetFitterGetBigger.API.Services.ReferenceTables.MuscleGroup.DataServices;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;

namespace GetFitterGetBigger.API.Services.ReferenceTables.MuscleGroup;

/// <summary>
/// Service implementation for muscle group operations (CRUD-enabled lookup table)
/// MuscleGroups support Create/Update/Delete operations, so NO caching is used
/// NO UnitOfWork here - all data access through IMuscleGroupDataService
/// </summary>
public class MuscleGroupService(
    IMuscleGroupDataService dataService,
    ILogger<MuscleGroupService> logger) : IMuscleGroupService
{
    private readonly IMuscleGroupDataService _dataService = dataService;
    private readonly ILogger<MuscleGroupService> _logger = logger;

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<MuscleGroupDto>>> GetAllActiveAsync()
    {
        // No caching for CRUD-enabled lookup tables
        return await _dataService.GetAllActiveAsync();
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<MuscleGroupDto>> GetByIdAsync(MuscleGroupId id)
    {
        return await ServiceValidate.For<MuscleGroupDto>()
            .EnsureNotEmpty(id, MuscleGroupErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () =>
                {
                    // No caching for CRUD-enabled lookup tables
                    var result = await _dataService.GetByIdAsync(id);
                    // Convert Empty to NotFound at the service layer
                    if (result.IsSuccess && result.Data.IsEmpty)
                    {
                        return ServiceResult<MuscleGroupDto>.Failure(
                            MuscleGroupDto.Empty,
                            ServiceError.NotFound("MuscleGroup", id.ToString()));
                    }
                    return result;
                }
            );
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<MuscleGroupDto>> GetByIdAsync(string id)
    {
        var muscleGroupId = MuscleGroupId.ParseOrEmpty(id);
        return await GetByIdAsync(muscleGroupId);
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<MuscleGroupDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<MuscleGroupDto>()
            .EnsureNotWhiteSpace(value, MuscleGroupErrorMessages.ValueCannotBeEmpty)
            .MatchAsync(
                whenValid: async () =>
                {
                    // No caching for CRUD-enabled lookup tables
                    var result = await _dataService.GetByNameAsync(value);
                    // Convert Empty to NotFound at the service layer
                    if (result.IsSuccess && result.Data.IsEmpty)
                    {
                        return ServiceResult<MuscleGroupDto>.Failure(
                            MuscleGroupDto.Empty,
                            ServiceError.NotFound("MuscleGroup", value));
                    }
                    return result;
                }
            );
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(MuscleGroupId id)
    {
        return await ServiceValidate.For<BooleanResultDto>()
            .EnsureNotEmpty(id, MuscleGroupErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () =>
                {
                    // Leverage the GetById cache for existence checks
                    var result = await GetByIdAsync(id);
                    return ServiceResult<BooleanResultDto>.Success(
                        BooleanResultDto.Create(result.IsSuccess && !result.Data.IsEmpty)
                    );
                }
            );
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<MuscleGroupDto>>> GetAllAsync()
    {
        return await GetAllActiveAsync();
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<MuscleGroupDto>> GetByNameAsync(string name)
    {
        return await GetByValueAsync(name);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<MuscleGroupDto>>> GetByBodyPartAsync(BodyPartId bodyPartId)
    {
        // Validate input
        if (bodyPartId.IsEmpty)
        {
            return ServiceResult<IEnumerable<MuscleGroupDto>>.Failure(Enumerable.Empty<MuscleGroupDto>(), 
                ServiceError.ValidationFailed(MuscleGroupErrorMessages.BodyPartIdCannotBeEmpty));
        }

        // No caching for CRUD-enabled lookup tables
        return await _dataService.GetByBodyPartAsync(bodyPartId);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<MuscleGroupDto>> CreateAsync(CreateMuscleGroupCommand command)
    {
        return await ServiceValidate.Build<MuscleGroupDto>()
            .EnsureNotWhiteSpace(command.Name, MuscleGroupErrorMessages.NameCannotBeEmpty)
            .EnsureNotEmpty(command.BodyPartId, MuscleGroupErrorMessages.BodyPartIdCannotBeEmpty)
            .EnsureNameIsUniqueAsync(
                async () => await IsNameUniqueAsync(command.Name),
                "MuscleGroup", command.Name)
            .WhenValidAsync(async () => await _dataService.CreateAsync(command));
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<MuscleGroupDto>> UpdateAsync(MuscleGroupId id, UpdateMuscleGroupCommand command)
    {
        return await ServiceValidate.Build<MuscleGroupDto>()
            .EnsureNotEmpty(id, MuscleGroupErrorMessages.InvalidIdFormat)
            .EnsureNotWhiteSpace(command.Name, MuscleGroupErrorMessages.NameCannotBeEmpty)
            .EnsureNotEmpty(command.BodyPartId, MuscleGroupErrorMessages.BodyPartIdCannotBeEmpty)
            .EnsureAsync(
                async () => await ExistsInternalAsync(id),
                ServiceError.NotFound("MuscleGroup", id.ToString()))
            .EnsureNameIsUniqueAsync(
                async () => await IsNameUniqueForUpdateAsync(command.Name, id),
                "MuscleGroup", command.Name)
            .WhenValidAsync(async () => await _dataService.UpdateAsync(id, command));
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> DeleteAsync(MuscleGroupId id)
    {
        return await ServiceValidate.Build<BooleanResultDto>()
            .EnsureNotEmpty(id, MuscleGroupErrorMessages.InvalidIdFormat)
            .EnsureAsync(
                async () => await ExistsInternalAsync(id),
                ServiceError.NotFound("MuscleGroup", id.ToString()))
            .EnsureAsync(
                async () => await CanDeleteInternalAsync(id),
                new ServiceError(ServiceErrorCode.DependencyExists, MuscleGroupErrorMessages.BusinessRules.CannotDeleteInUse))
            .WhenValidAsync(async () => await _dataService.DeleteAsync(id));
    }

    // Private helper methods for validation
    private async Task<bool> IsNameUniqueAsync(string name)
    {
        return await _dataService.IsNameUniqueAsync(name);
    }

    private async Task<bool> IsNameUniqueForUpdateAsync(string name, MuscleGroupId excludeId)
    {
        return await _dataService.IsNameUniqueAsync(name, excludeId);
    }

    private async Task<bool> ExistsInternalAsync(MuscleGroupId id)
    {
        var result = await _dataService.ExistsAsync(id);
        return result.IsSuccess && result.Data.Value;
    }

    private async Task<bool> CanDeleteInternalAsync(MuscleGroupId id)
    {
        return await _dataService.CanDeleteAsync(id);
    }
}