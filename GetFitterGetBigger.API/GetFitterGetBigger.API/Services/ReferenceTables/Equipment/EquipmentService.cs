using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.Equipment;
using GetFitterGetBigger.API.Services.ReferenceTables.Equipment.DataServices;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;

namespace GetFitterGetBigger.API.Services.ReferenceTables.Equipment;

/// <summary>
/// Service implementation for equipment operations (CRUD-enabled lookup table)
/// Equipment supports Create/Update/Delete operations, so NO caching is used
/// NO UnitOfWork here - all data access through IEquipmentDataService
/// </summary>
public class EquipmentService : IEquipmentService
{
    private readonly IEquipmentDataService _dataService;
    private readonly ILogger<EquipmentService> _logger;

    public EquipmentService(
        IEquipmentDataService dataService,
        ILogger<EquipmentService> logger)
    {
        _dataService = dataService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<EquipmentDto>>> GetAllActiveAsync()
    {
        // No caching for CRUD-enabled lookup tables
        return await _dataService.GetAllActiveAsync();
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<EquipmentDto>> GetByIdAsync(EquipmentId id)
    {
        return await ServiceValidate.For<EquipmentDto>()
            .EnsureNotEmpty(id, EquipmentErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () =>
                {
                    // No caching for CRUD-enabled lookup tables
                    var result = await _dataService.GetByIdAsync(id);
                    // Convert Empty to NotFound at the service layer
                    if (result.IsSuccess && result.Data.IsEmpty)
                    {
                        return ServiceResult<EquipmentDto>.Failure(
                            EquipmentDto.Empty,
                            ServiceError.NotFound("Equipment", id.ToString()));
                    }
                    return result;
                }
            );
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<EquipmentDto>> GetByIdAsync(string id)
    {
        var equipmentId = EquipmentId.ParseOrEmpty(id);
        return await GetByIdAsync(equipmentId);
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<EquipmentDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<EquipmentDto>()
            .EnsureNotWhiteSpace(value, EquipmentErrorMessages.ValueCannotBeEmpty)
            .MatchAsync(
                whenValid: async () =>
                {
                    // No caching for CRUD-enabled lookup tables
                    var result = await _dataService.GetByNameAsync(value);
                    // Convert Empty to NotFound at the service layer
                    if (result.IsSuccess && result.Data.IsEmpty)
                    {
                        return ServiceResult<EquipmentDto>.Failure(
                            EquipmentDto.Empty,
                            ServiceError.NotFound("Equipment", value));
                    }
                    return result;
                }
            );
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(EquipmentId id)
    {
        return await ServiceValidate.For<BooleanResultDto>()
            .EnsureNotEmpty(id, EquipmentErrorMessages.InvalidIdFormat)
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
    public async Task<ServiceResult<IEnumerable<EquipmentDto>>> GetAllAsync()
    {
        return await GetAllActiveAsync();
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<EquipmentDto>> GetByNameAsync(string name)
    {
        return await GetByValueAsync(name);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<EquipmentDto>> CreateAsync(CreateEquipmentCommand command)
    {
        // Validate basic input
        if (command == null)
        {
            return ServiceResult<EquipmentDto>.Failure(EquipmentDto.Empty, 
                ServiceError.ValidationFailed(EquipmentErrorMessages.CommandCannotBeNull));
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return ServiceResult<EquipmentDto>.Failure(EquipmentDto.Empty, 
                ServiceError.ValidationFailed(EquipmentErrorMessages.NameCannotBeEmpty));
        }

        // Validate business rules asynchronously
        if (!await IsNameUniqueAsync(command!.Name))
        {
            return ServiceResult<EquipmentDto>.Failure(EquipmentDto.Empty, 
                ServiceError.AlreadyExists("Equipment", command.Name));
        }

        // Perform the operation
        var result = await _dataService.CreateAsync(command!);
        // Note: For eternal reference data, cache invalidation is not needed
        // Reference data is immutable after deployment
        return result;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<EquipmentDto>> UpdateAsync(EquipmentId id, UpdateEquipmentCommand command)
    {
        // Validate basic input
        if (id.IsEmpty)
        {
            return ServiceResult<EquipmentDto>.Failure(EquipmentDto.Empty, 
                ServiceError.ValidationFailed(EquipmentErrorMessages.InvalidIdFormat));
        }

        if (command == null)
        {
            return ServiceResult<EquipmentDto>.Failure(EquipmentDto.Empty, 
                ServiceError.ValidationFailed(EquipmentErrorMessages.CommandCannotBeNull));
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return ServiceResult<EquipmentDto>.Failure(EquipmentDto.Empty, 
                ServiceError.ValidationFailed(EquipmentErrorMessages.NameCannotBeEmpty));
        }

        // Validate business rules asynchronously
        if (!await ExistsInternalAsync(id))
        {
            return ServiceResult<EquipmentDto>.Failure(EquipmentDto.Empty, 
                ServiceError.NotFound("Equipment", id.ToString()));
        }

        if (!await IsNameUniqueForUpdateAsync(command!.Name, id))
        {
            return ServiceResult<EquipmentDto>.Failure(EquipmentDto.Empty, 
                ServiceError.AlreadyExists("Equipment", command.Name));
        }

        // Perform the operation
        var result = await _dataService.UpdateAsync(id, command!);
        // Note: For eternal reference data, cache invalidation is not needed
        // Reference data is immutable after deployment
        return result;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> DeleteAsync(EquipmentId id)
    {
        // Validate basic input
        if (id.IsEmpty)
        {
            return ServiceResult<BooleanResultDto>.Failure(BooleanResultDto.Create(false), 
                ServiceError.ValidationFailed(EquipmentErrorMessages.InvalidIdFormat));
        }

        // Validate business rules asynchronously
        if (!await ExistsInternalAsync(id))
        {
            return ServiceResult<BooleanResultDto>.Failure(BooleanResultDto.Create(false), 
                ServiceError.NotFound("Equipment", id.ToString()));
        }

        if (!await CanDeleteInternalAsync(id))
        {
            return ServiceResult<BooleanResultDto>.Failure(BooleanResultDto.Create(false), 
                ServiceError.DependencyExists("Equipment", "dependent exercises"));
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

    private async Task<bool> IsNameUniqueForUpdateAsync(string name, EquipmentId excludeId)
    {
        return await _dataService.IsNameUniqueAsync(name, excludeId);
    }

    private async Task<bool> ExistsInternalAsync(EquipmentId id)
    {
        var result = await _dataService.ExistsAsync(id);
        return result.IsSuccess && result.Data.Value;
    }

    private async Task<bool> CanDeleteInternalAsync(EquipmentId id)
    {
        return await _dataService.CanDeleteAsync(id);
    }
}