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
public class MuscleGroupService : IMuscleGroupService
{
    private readonly IMuscleGroupDataService _dataService;
    private readonly ILogger<MuscleGroupService> _logger;

    public MuscleGroupService(
        IMuscleGroupDataService dataService,
        ILogger<MuscleGroupService> logger)
    {
        _dataService = dataService;
        _logger = logger;
    }

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
        // Validate basic input
        if (command == null)
        {
            return ServiceResult<MuscleGroupDto>.Failure(MuscleGroupDto.Empty, 
                ServiceError.ValidationFailed(MuscleGroupErrorMessages.CommandCannotBeNull));
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return ServiceResult<MuscleGroupDto>.Failure(MuscleGroupDto.Empty, 
                ServiceError.ValidationFailed(MuscleGroupErrorMessages.NameCannotBeEmpty));
        }

        if (command.BodyPartId.IsEmpty)
        {
            return ServiceResult<MuscleGroupDto>.Failure(MuscleGroupDto.Empty, 
                ServiceError.ValidationFailed(MuscleGroupErrorMessages.BodyPartIdCannotBeEmpty));
        }

        // Validate business rules asynchronously
        if (!await IsNameUniqueAsync(command!.Name))
        {
            return ServiceResult<MuscleGroupDto>.Failure(MuscleGroupDto.Empty, 
                ServiceError.AlreadyExists("MuscleGroup", command.Name));
        }

        // Perform the operation
        var result = await _dataService.CreateAsync(command!);
        // Note: For eternal reference data, cache invalidation is not needed
        // Reference data is immutable after deployment
        return result;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<MuscleGroupDto>> UpdateAsync(MuscleGroupId id, UpdateMuscleGroupCommand command)
    {
        // Validate basic input
        if (id.IsEmpty)
        {
            return ServiceResult<MuscleGroupDto>.Failure(MuscleGroupDto.Empty, 
                ServiceError.ValidationFailed(MuscleGroupErrorMessages.InvalidIdFormat));
        }

        if (command == null)
        {
            return ServiceResult<MuscleGroupDto>.Failure(MuscleGroupDto.Empty, 
                ServiceError.ValidationFailed(MuscleGroupErrorMessages.CommandCannotBeNull));
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return ServiceResult<MuscleGroupDto>.Failure(MuscleGroupDto.Empty, 
                ServiceError.ValidationFailed(MuscleGroupErrorMessages.NameCannotBeEmpty));
        }

        if (command.BodyPartId.IsEmpty)
        {
            return ServiceResult<MuscleGroupDto>.Failure(MuscleGroupDto.Empty, 
                ServiceError.ValidationFailed(MuscleGroupErrorMessages.BodyPartIdCannotBeEmpty));
        }

        // Validate business rules asynchronously
        if (!await ExistsInternalAsync(id))
        {
            return ServiceResult<MuscleGroupDto>.Failure(MuscleGroupDto.Empty, 
                ServiceError.NotFound("MuscleGroup", id.ToString()));
        }

        if (!await IsNameUniqueForUpdateAsync(command!.Name, id))
        {
            return ServiceResult<MuscleGroupDto>.Failure(MuscleGroupDto.Empty, 
                ServiceError.AlreadyExists("MuscleGroup", command.Name));
        }

        // Perform the operation
        var result = await _dataService.UpdateAsync(id, command!);
        // Note: For eternal reference data, cache invalidation is not needed
        // Reference data is immutable after deployment
        return result;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> DeleteAsync(MuscleGroupId id)
    {
        // Validate basic input
        if (id.IsEmpty)
        {
            return ServiceResult<BooleanResultDto>.Failure(BooleanResultDto.Create(false), 
                ServiceError.ValidationFailed(MuscleGroupErrorMessages.InvalidIdFormat));
        }

        // Validate business rules asynchronously
        if (!await ExistsInternalAsync(id))
        {
            return ServiceResult<BooleanResultDto>.Failure(BooleanResultDto.Create(false), 
                ServiceError.NotFound("MuscleGroup", id.ToString()));
        }

        if (!await CanDeleteInternalAsync(id))
        {
            return ServiceResult<BooleanResultDto>.Failure(BooleanResultDto.Create(false), 
                ServiceError.DependencyExists("MuscleGroup", "dependent exercises"));
        }

        // Perform the operation
        var result = await _dataService.DeleteAsync(id);
        // Note: For eternal reference data, cache invalidation is not needed
        // Reference data is immutable after deployment
        return result;
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