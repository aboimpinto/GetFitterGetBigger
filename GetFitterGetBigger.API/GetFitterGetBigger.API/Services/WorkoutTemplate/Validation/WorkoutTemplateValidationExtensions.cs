using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.DTOs.Interfaces;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using GetFitterGetBigger.API.Services.WorkoutTemplate.DataServices;
using GetFitterGetBigger.API.Constants.ErrorMessages;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Validation;

/// <summary>
/// WorkoutTemplate-specific validation extensions that provide domain-specific
/// validation methods while leveraging the generic entity loading pattern.
/// </summary>
public static class WorkoutTemplateValidationExtensions
{
    /// <summary>
    /// Loads and validates that a workout template exists.
    /// Adds a validation error if the template is not found or empty.
    /// </summary>
    /// <typeparam name="TResult">The result DTO type that implements IEmptyDto</typeparam>
    /// <param name="validationTask">The async validation task</param>
    /// <param name="queryService">The query service to load the template</param>
    /// <param name="templateId">The ID of the template to load</param>
    /// <returns>A validation chain carrying the loaded workout template</returns>
    public static Task<ServiceValidationWithData<TResult, WorkoutTemplateDto>> 
        EnsureWorkoutTemplateExists<TResult>(
            this Task<ServiceValidation<TResult>> validationTask,
            IWorkoutTemplateQueryDataService queryService,
            WorkoutTemplateId templateId)
        where TResult : class, IEmptyDto<TResult>
    {
        return validationTask.EnsureEntityExists<TResult, WorkoutTemplateDto>(
            async () => await queryService.GetByIdWithDetailsAsync(templateId),
            ServiceError.NotFound(WorkoutTemplateErrorMessages.NotFound)
        );
    }
    
    /// <summary>
    /// Loads and validates that a workout template exists with custom error message.
    /// Adds a validation error if the template is not found or empty.
    /// </summary>
    /// <typeparam name="TResult">The result DTO type that implements IEmptyDto</typeparam>
    /// <param name="validationTask">The async validation task</param>
    /// <param name="queryService">The query service to load the template</param>
    /// <param name="templateId">The ID of the template to load</param>
    /// <param name="notFoundMessage">Custom error message for when template is not found</param>
    /// <returns>A validation chain carrying the loaded workout template</returns>
    public static Task<ServiceValidationWithData<TResult, WorkoutTemplateDto>> 
        EnsureWorkoutTemplateExists<TResult>(
            this Task<ServiceValidation<TResult>> validationTask,
            IWorkoutTemplateQueryDataService queryService,
            WorkoutTemplateId templateId,
            string notFoundMessage)
        where TResult : class, IEmptyDto<TResult>
    {
        return validationTask.EnsureEntityExists<TResult, WorkoutTemplateDto>(
            async () => await queryService.GetByIdWithDetailsAsync(templateId),
            ServiceError.NotFound(notFoundMessage)
        );
    }
    
    /// <summary>
    /// Continues the validation chain with the loaded workout template.
    /// This is the terminal operation for workout template validations.
    /// </summary>
    /// <typeparam name="TResult">The result DTO type that implements IEmptyDto</typeparam>
    /// <param name="validationTask">The async validation with workout template data</param>
    /// <param name="action">Action to execute with the loaded template</param>
    /// <returns>The result from either validation failure or the successful action</returns>
    public static Task<ServiceResult<TResult>> ThenWithWorkoutTemplate<TResult>(
        this Task<ServiceValidationWithData<TResult, WorkoutTemplateDto>> validationTask,
        Func<WorkoutTemplateDto, Task<ServiceResult<TResult>>> action)
        where TResult : class, IEmptyDto<TResult>
    {
        return validationTask.ThenWithEntity(action);
    }
    
    /// <summary>
    /// Ensures that a template name is unique for duplication purposes.
    /// This validation uses the loaded template context if needed.
    /// </summary>
    /// <typeparam name="TResult">The result DTO type that implements IEmptyDto</typeparam>
    /// <param name="validationTask">The async validation with workout template data</param>
    /// <param name="queryService">The query service to check name uniqueness</param>
    /// <param name="newName">The new name to check for uniqueness</param>
    /// <returns>The validation chain for continued processing</returns>
    public static Task<ServiceValidationWithData<TResult, WorkoutTemplateDto>>
        EnsureNameIsUniqueForDuplication<TResult>(
            this Task<ServiceValidationWithData<TResult, WorkoutTemplateDto>> validationTask,
            IWorkoutTemplateQueryDataService queryService,
            string newName)
        where TResult : class, IEmptyDto<TResult>
    {
        return validationTask.EnsureAsyncWithEntity(
            async template => 
            {
                // Check if name is unique (doesn't exist yet)
                var existsResult = await queryService.ExistsByNameAsync(newName);
                return existsResult?.IsSuccess == true && !existsResult.Data.Value;
            },
            WorkoutTemplateErrorMessages.NameAlreadyExists
        );
    }
    
    /// <summary>
    /// Ensures that a workout template is in a modifiable state.
    /// Templates in certain states (like PUBLISHED) cannot be modified.
    /// </summary>
    /// <typeparam name="TResult">The result DTO type that implements IEmptyDto</typeparam>
    /// <param name="validationTask">The async validation with workout template data</param>
    /// <param name="allowedStates">List of states that allow modification</param>
    /// <param name="errorMessage">Error message if template is in wrong state</param>
    /// <returns>The validation chain for continued processing</returns>
    public static Task<ServiceValidationWithData<TResult, WorkoutTemplateDto>>
        EnsureTemplateInModifiableState<TResult>(
            this Task<ServiceValidationWithData<TResult, WorkoutTemplateDto>> validationTask,
            string[] allowedStates,
            string? errorMessage = null)
        where TResult : class, IEmptyDto<TResult>
    {
        return validationTask.EnsureWithEntity(
            template => allowedStates.Contains(template.WorkoutState.Value),
            errorMessage ?? $"Template must be in one of these states: {string.Join(", ", allowedStates)}"
        );
    }
    
    /// <summary>
    /// Ensures that a workout template belongs to a specific user or is public.
    /// Useful for authorization checks within the validation chain.
    /// </summary>
    /// <typeparam name="TResult">The result DTO type that implements IEmptyDto</typeparam>
    /// <param name="validationTask">The async validation with workout template data</param>
    /// <param name="userId">The user ID to check ownership against</param>
    /// <param name="allowPublic">Whether public templates are allowed</param>
    /// <returns>The validation chain for continued processing</returns>
    public static Task<ServiceValidationWithData<TResult, WorkoutTemplateDto>>
        EnsureUserCanAccessTemplate<TResult>(
            this Task<ServiceValidationWithData<TResult, WorkoutTemplateDto>> validationTask,
            UserId userId,
            bool allowPublic = true)
        where TResult : class, IEmptyDto<TResult>
    {
        return validationTask.EnsureWithEntity(
            template => 
            {
                // For now, return true since we don't have CreatedById yet
                // TODO: Implement once CreatedById is added to WorkoutTemplate
                // return template.CreatedById == userId || (allowPublic && template.IsPublic);
                return allowPublic && template.IsPublic;
            },
            "You don't have permission to access this template"
        );
    }
    
    /// <summary>
    /// Transforms a workout template into a duplication model with a new name.
    /// This is useful for preparing the template for duplication.
    /// </summary>
    /// <typeparam name="TResult">The result DTO type that implements IEmptyDto</typeparam>
    /// <param name="validationTask">The async validation with workout template data</param>
    /// <param name="newName">The new name for the duplicated template</param>
    /// <returns>A validation chain with a duplication model</returns>
    public static Task<ServiceValidationWithData<TResult, WorkoutTemplateDuplicationModel>>
        PrepareForDuplication<TResult>(
            this Task<ServiceValidationWithData<TResult, WorkoutTemplateDto>> validationTask,
            string newName)
        where TResult : class, IEmptyDto<TResult>
    {
        return validationTask.Transform<TResult, WorkoutTemplateDto, WorkoutTemplateDuplicationModel>(
            template => new WorkoutTemplateDuplicationModel(template, newName)
        );
    }
}

/// <summary>
/// Model that carries workout template data prepared for duplication
/// </summary>
public class WorkoutTemplateDuplicationModel
{
    public WorkoutTemplateDto OriginalTemplate { get; }
    public string NewName { get; }
    
    public WorkoutTemplateDuplicationModel(WorkoutTemplateDto originalTemplate, string newName)
    {
        OriginalTemplate = originalTemplate;
        NewName = newName;
    }
}