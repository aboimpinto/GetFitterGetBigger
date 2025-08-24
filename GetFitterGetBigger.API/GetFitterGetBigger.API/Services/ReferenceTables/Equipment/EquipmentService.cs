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
public class EquipmentService(
    IEquipmentDataService dataService,
    ILogger<EquipmentService> logger) : IEquipmentService
{
    private readonly IEquipmentDataService _dataService = dataService;
    private readonly ILogger<EquipmentService> _logger = logger;

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
            .EnsureNotEmpty(id, EquipmentErrorMessages.Validation.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await LoadByIdFromDatabaseAsync(id)
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
                whenValid: async () => await LoadByNameFromDatabaseAsync(value)
            );
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(EquipmentId id)
    {
        return await ServiceValidate.For<BooleanResultDto>()
            .EnsureNotEmpty(id, EquipmentErrorMessages.Validation.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await CheckExistenceAsync(id)
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
        return await ServiceValidate.Build<EquipmentDto>()
            .EnsureNotNull(command, EquipmentErrorMessages.CommandCannotBeNull)
            .EnsureNotWhiteSpace(command?.Name, EquipmentErrorMessages.Validation.NameCannotBeEmpty)
            .EnsureNameIsUniqueAsync(
                async () => await IsNameUniqueAsync(command!.Name),
                "Equipment",
                command?.Name ?? string.Empty)
            .WhenValidAsync(async () => await _dataService.CreateAsync(command!));
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<EquipmentDto>> UpdateAsync(EquipmentId id, UpdateEquipmentCommand command)
    {
        return await ServiceValidate.Build<EquipmentDto>()
            .EnsureNotEmpty(id, EquipmentErrorMessages.Validation.InvalidIdFormat)
            .EnsureNotNull(command, EquipmentErrorMessages.CommandCannotBeNull)
            .EnsureNotWhiteSpace(command?.Name, EquipmentErrorMessages.Validation.NameCannotBeEmpty)
            .EnsureAsync(
                async () => await ExistsInternalAsync(id),
                ServiceError.NotFound("Equipment", id.ToString()))
            .EnsureNameIsUniqueAsync(
                async () => await IsNameUniqueForUpdateAsync(command!.Name, id),
                "Equipment",
                command?.Name ?? string.Empty)
            .WhenValidAsync(async () => await _dataService.UpdateAsync(id, command!));
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> DeleteAsync(EquipmentId id)
    {
        return await ServiceValidate.Build<BooleanResultDto>()
            .EnsureNotEmpty(id, EquipmentErrorMessages.Validation.InvalidIdFormat)
            .EnsureAsync(
                async () => await ExistsInternalAsync(id),
                ServiceError.NotFound("Equipment", id.ToString()))
            .EnsureAsync(
                async () => await CanDeleteInternalAsync(id),
                new ServiceError(ServiceErrorCode.DependencyExists, EquipmentErrorMessages.BusinessRules.CannotDeleteInUse))
            .WhenValidAsync(async () => await _dataService.DeleteAsync(id));
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

    // Private helper methods for single operations
    private async Task<ServiceResult<EquipmentDto>> LoadByIdFromDatabaseAsync(EquipmentId id)
    {
        // No caching for CRUD-enabled lookup tables
        var result = await _dataService.GetByIdAsync(id);
        // Convert Empty to NotFound at the service layer
        return result.IsSuccess && result.Data.IsEmpty
            ? ServiceResult<EquipmentDto>.Failure(
                EquipmentDto.Empty,
                ServiceError.NotFound("Equipment", id.ToString()))
            : result;
    }

    private async Task<ServiceResult<EquipmentDto>> LoadByNameFromDatabaseAsync(string value)
    {
        // No caching for CRUD-enabled lookup tables
        var result = await _dataService.GetByNameAsync(value);
        // Convert Empty to NotFound at the service layer
        return result.IsSuccess && result.Data.IsEmpty
            ? ServiceResult<EquipmentDto>.Failure(
                EquipmentDto.Empty,
                ServiceError.NotFound("Equipment", value))
            : result;
    }

    private async Task<ServiceResult<BooleanResultDto>> CheckExistenceAsync(EquipmentId id)
    {
        // Leverage the GetById for existence checks
        var result = await GetByIdAsync(id);
        return ServiceResult<BooleanResultDto>.Success(
            BooleanResultDto.Create(result.IsSuccess && !result.Data.IsEmpty)
        );
    }
}