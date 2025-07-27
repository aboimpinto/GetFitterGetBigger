using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Models.Errors;
using GetFitterGetBigger.Admin.Models.Results;
using GetFitterGetBigger.Admin.Services.DataProviders;
using GetFitterGetBigger.Admin.Services.Validation;
using GetFitterGetBigger.Admin.Extensions;

namespace GetFitterGetBigger.Admin.Services
{
    public class WorkoutTemplateService : IWorkoutTemplateService
    {
        private readonly IWorkoutTemplateDataProvider _dataProvider;
        private readonly ILogger<WorkoutTemplateService> _logger;

        public WorkoutTemplateService(
            IWorkoutTemplateDataProvider dataProvider,
            ILogger<WorkoutTemplateService> logger)
        {
            _dataProvider = dataProvider;
            _logger = logger;
        }

        public async Task<ServiceResult<WorkoutTemplatePagedResultDto>> GetWorkoutTemplatesAsync(WorkoutTemplateFilterDto filter)
        {
            // Validate and process using Result pattern
            return await ValidationBuilder<WorkoutTemplateFilterDto>.For(filter)
                .EnsureCappedRange(f => f.PageSize, 1, 100, _logger)
                .OnSuccessAsync(async validFilter =>
                {
                    var dataResult = await _dataProvider.GetWorkoutTemplatesAsync(validFilter);
                    return dataResult.ToServiceResult();
                });
        }

        public async Task<ServiceResult<WorkoutTemplateDto>> GetWorkoutTemplateByIdAsync(string id)
        {
            // Validate and process using Result pattern
            return await ValidationBuilder<string>.For(id)
                .EnsureNotEmpty()
                .OnSuccessAsync(async validId =>
                {
                    var dataResult = await _dataProvider.GetWorkoutTemplateByIdAsync(validId);
                    
                    // Pattern matching for result handling
                    return dataResult switch
                    {
                        { IsSuccess: false } when dataResult.HasError(DataErrorCode.NotFound) 
                            => ServiceResult<WorkoutTemplateDto>.Success(WorkoutTemplateDto.Empty),
                        _ => dataResult.ToServiceResult()
                    };
                });
        }

        public async Task<ServiceResult<WorkoutTemplateDto>> CreateWorkoutTemplateAsync(CreateWorkoutTemplateDto template)
        {
            // Explicit validation using ValidationBuilder
            return await ValidationBuilder<CreateWorkoutTemplateDto>.For(template)
                .EnsureNotEmpty(t => t.Name)
                .EnsureMaxLength(t => t.Name, 100, "Template name")
                .OnSuccessAsync(async validTemplate =>
                {
                    var dataResult = await _dataProvider.CreateWorkoutTemplateAsync(validTemplate);
                    
                    return dataResult switch
                    {
                        { IsSuccess: true } => ServiceResult<WorkoutTemplateDto>.Success(dataResult.Data!),
                        { IsSuccess: false } when dataResult.HasError(DataErrorCode.Conflict) 
                            => ServiceResult<WorkoutTemplateDto>.Failure(ServiceError.DuplicateName(validTemplate.Name)),
                        _ => dataResult.ToServiceResult()
                    };
                });
        }

        public async Task<ServiceResult<WorkoutTemplateDto>> UpdateWorkoutTemplateAsync(string id, UpdateWorkoutTemplateDto template)
        {
            // Use ValidationBuilder pattern with explicit validation
            return await ValidationBuilder<UpdateWorkoutTemplateDto>.For(template)
                .EnsureNotEmpty(t => t.Name)
                .EnsureMaxLength(t => t.Name, 100, "Template name")
                .OnSuccessAsync(async validTemplate =>
                {
                    var dataResult = await _dataProvider.UpdateWorkoutTemplateAsync(id, validTemplate);
                    
                    var result = dataResult switch
                    {
                        { IsSuccess: true } => ServiceResult<WorkoutTemplateDto>.Success(dataResult.Data!),
                        { IsSuccess: false } when dataResult.HasError(DataErrorCode.NotFound) 
                            => ServiceResult<WorkoutTemplateDto>.Failure(ServiceError.TemplateNotFound(id)),
                        _ => dataResult.ToServiceResult()
                    };
                    
                    if (result.IsSuccess)
                    {
                        _logger.LogInformation("Updated workout template {Id} - {Name}", result.Data!.Id, result.Data.Name);
                    }
                    
                    return result;
                });
        }

        public async Task<ServiceResult<bool>> DeleteWorkoutTemplateAsync(string id)
        {
            return await ValidationBuilder<string>.For(id)
                .EnsureNotEmpty()
                .OnSuccessAsync(async validId =>
                {
                    var dataResult = await _dataProvider.DeleteWorkoutTemplateAsync(validId);
                    
                    var result = dataResult switch
                    {
                        { IsSuccess: true } => ServiceResult<bool>.Success(true),
                        { IsSuccess: false } when dataResult.HasError(DataErrorCode.NotFound) 
                            => ServiceResult<bool>.Failure(ServiceError.TemplateNotFound(validId)),
                        _ => dataResult.ToServiceResult()
                    };
                    
                    if (result.IsSuccess)
                    {
                        _logger.LogInformation("Deleted workout template {Id}", validId);
                    }
                    
                    return result;
                });
        }

        public async Task<ServiceResult<WorkoutTemplateDto>> ChangeWorkoutTemplateStateAsync(string id, ChangeWorkoutStateDto changeState)
        {
            // Use ValidationBuilder pattern with explicit validation
            return await ValidationBuilder<ChangeWorkoutStateDto>.For(changeState)
                .EnsureNotEmpty(cs => cs.WorkoutStateId, "Workout State ID")
                .OnSuccessAsync(async validChangeState =>
                {
                    var dataResult = await _dataProvider.ChangeWorkoutTemplateStateAsync(id, validChangeState);
                    
                    var result = dataResult switch
                    {
                        { IsSuccess: true } => ServiceResult<WorkoutTemplateDto>.Success(dataResult.Data!),
                        { IsSuccess: false } when dataResult.HasError(DataErrorCode.NotFound) 
                            => ServiceResult<WorkoutTemplateDto>.Failure(ServiceError.TemplateNotFound(id)),
                        _ => dataResult.ToServiceResult()
                    };
                    
                    if (result.IsSuccess)
                    {
                        _logger.LogInformation("Changed workout template {Id} state to {WorkoutStateId}", id, validChangeState.WorkoutStateId);
                    }
                    
                    return result;
                });
        }

        public async Task<ServiceResult<WorkoutTemplateDto>> DuplicateWorkoutTemplateAsync(string id, DuplicateWorkoutTemplateDto duplicate)
        {
            // Use ValidationBuilder pattern with explicit validation
            return await ValidationBuilder<DuplicateWorkoutTemplateDto>.For(duplicate)
                .EnsureNotEmpty(d => d.NewName)
                .EnsureMaxLength(d => d.NewName, 100, "Template name")
                .OnSuccessAsync(async validDuplicate =>
                {
                    var dataResult = await _dataProvider.DuplicateWorkoutTemplateAsync(id, validDuplicate);
                    
                    var result = dataResult switch
                    {
                        { IsSuccess: true } => ServiceResult<WorkoutTemplateDto>.Success(dataResult.Data!),
                        { IsSuccess: false } when dataResult.HasError(DataErrorCode.NotFound) 
                            => ServiceResult<WorkoutTemplateDto>.Failure(ServiceError.TemplateNotFound(id)),
                        { IsSuccess: false } when dataResult.HasError(DataErrorCode.Conflict) 
                            => ServiceResult<WorkoutTemplateDto>.Failure(ServiceError.DuplicateName(validDuplicate.NewName)),
                        _ => dataResult.ToServiceResult()
                    };
                    
                    if (result.IsSuccess)
                    {
                        _logger.LogInformation("Duplicated workout template {OriginalId} to new template {NewId} - {NewName}", 
                            id, result.Data!.Id, result.Data.Name);
                    }
                    
                    return result;
                });
        }


        public async Task<ServiceResult<bool>> CheckTemplateNameExistsAsync(string name)
        {
            // Use ValidationBuilder pattern consistent with other methods
            return await ValidationBuilder<string>.For(name)
                .EnsureNotEmpty()
                .OnSuccessAsync(async validName =>
                {
                    var dataResult = await _dataProvider.CheckTemplateNameExistsAsync(validName);
                    return dataResult.ToServiceResult();
                });
        }

    }
}