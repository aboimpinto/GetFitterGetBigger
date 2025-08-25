using GetFitterGetBigger.API.DTOs.Interfaces;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Validation;

/// <summary>
/// Extension methods for ServiceValidation that enable loading and validating entities
/// as part of the validation chain, carrying them through for use in subsequent operations.
/// </summary>
public static class ServiceValidationEntityExtensions
{
    /// <summary>
    /// Loads an entity as part of the validation chain and validates it exists (non-empty).
    /// If the entity is not found or empty, adds a validation error and continues the chain.
    /// The loaded entity is carried forward for use in subsequent operations.
    /// </summary>
    /// <typeparam name="TResult">The result DTO type that implements IEmptyDto</typeparam>
    /// <typeparam name="TEntity">The entity DTO type that implements IEmptyDto</typeparam>
    /// <param name="validationTask">The async validation task</param>
    /// <param name="loadEntityFunc">Function that loads the entity</param>
    /// <param name="errorMessage">Error message if entity is not found or empty</param>
    /// <returns>A validation chain that carries the loaded entity data</returns>
    public static async Task<ServiceValidationWithData<TResult, TEntity>> 
        EnsureEntityExists<TResult, TEntity>(
            this Task<ServiceValidation<TResult>> validationTask,
            Func<Task<ServiceResult<TEntity>>> loadEntityFunc,
            string errorMessage)
        where TResult : class, IEmptyDto<TResult>
        where TEntity : class, IEmptyDto<TEntity>
    {
        var validation = await validationTask;
        
        // Skip entity loading if validation already has errors
        if (ShouldSkipEntityLoading(validation))
            return CreateValidationWithoutData<TResult, TEntity>(validation);
        
        // Attempt to load the entity
        var loadResult = await loadEntityFunc();
        
        // Process the load result and return appropriate validation state
        return ProcessEntityLoadResult(validation, loadResult, errorMessage);
    }

    /// <summary>
    /// Determines if entity loading should be skipped due to existing validation errors
    /// </summary>
    /// <typeparam name="TResult">The result DTO type</typeparam>
    /// <param name="validation">The validation to check</param>
    /// <returns>True if entity loading should be skipped</returns>
    private static bool ShouldSkipEntityLoading<TResult>(ServiceValidation<TResult> validation)
        where TResult : class
    {
        return validation.HasErrors;
    }

    /// <summary>
    /// Creates a ServiceValidationWithData without entity data
    /// </summary>
    /// <typeparam name="TResult">The result DTO type</typeparam>
    /// <typeparam name="TEntity">The entity DTO type</typeparam>
    /// <param name="validation">The validation instance</param>
    /// <returns>ServiceValidationWithData with no entity data</returns>
    private static ServiceValidationWithData<TResult, TEntity> CreateValidationWithoutData<TResult, TEntity>(
        ServiceValidation<TResult> validation)
        where TResult : class, IEmptyDto<TResult>
        where TEntity : class, IEmptyDto<TEntity>
    {
        return new ServiceValidationWithData<TResult, TEntity>(validation, default);
    }

    /// <summary>
    /// Processes the entity load result and creates appropriate validation response
    /// </summary>
    /// <typeparam name="TResult">The result DTO type</typeparam>
    /// <typeparam name="TEntity">The entity DTO type</typeparam>
    /// <param name="validation">The validation instance</param>
    /// <param name="loadResult">The result from loading the entity</param>
    /// <param name="errorMessage">Error message if entity load fails</param>
    /// <returns>ServiceValidationWithData based on load result</returns>
    private static ServiceValidationWithData<TResult, TEntity> ProcessEntityLoadResult<TResult, TEntity>(
        ServiceValidation<TResult> validation,
        ServiceResult<TEntity> loadResult,
        string errorMessage)
        where TResult : class, IEmptyDto<TResult>
        where TEntity : class, IEmptyDto<TEntity>
    {
        if (IsEntityLoadSuccessful(loadResult))
        {
            return CreateValidationWithData(validation, loadResult.Data!);
        }

        // Entity load failed or returned empty - add validation error
        validation.Ensure(() => false, errorMessage);
        return CreateValidationWithoutData<TResult, TEntity>(validation);
    }

    /// <summary>
    /// Determines if the entity load was successful and returned valid data
    /// </summary>
    /// <typeparam name="TEntity">The entity DTO type</typeparam>
    /// <param name="loadResult">The result from loading the entity</param>
    /// <returns>True if load was successful and data is not empty</returns>
    private static bool IsEntityLoadSuccessful<TEntity>(ServiceResult<TEntity> loadResult)
        where TEntity : class, IEmptyDto<TEntity>
    {
        return loadResult.IsSuccess && loadResult.Data?.IsEmpty == false;
    }

    /// <summary>
    /// Creates a ServiceValidationWithData with valid entity data
    /// </summary>
    /// <typeparam name="TResult">The result DTO type</typeparam>
    /// <typeparam name="TEntity">The entity DTO type</typeparam>
    /// <param name="validation">The validation instance</param>
    /// <param name="entityData">The loaded entity data</param>
    /// <returns>ServiceValidationWithData with entity data</returns>
    private static ServiceValidationWithData<TResult, TEntity> CreateValidationWithData<TResult, TEntity>(
        ServiceValidation<TResult> validation,
        TEntity entityData)
        where TResult : class, IEmptyDto<TResult>
        where TEntity : class, IEmptyDto<TEntity>
    {
        return new ServiceValidationWithData<TResult, TEntity>(validation, entityData);
    }
    
    /// <summary>
    /// Loads an entity as part of the validation chain and validates it exists (non-empty).
    /// If the entity is not found or empty, adds a ServiceError and continues the chain.
    /// The loaded entity is carried forward for use in subsequent operations.
    /// </summary>
    /// <typeparam name="TResult">The result DTO type that implements IEmptyDto</typeparam>
    /// <typeparam name="TEntity">The entity DTO type that implements IEmptyDto</typeparam>
    /// <param name="validationTask">The async validation task</param>
    /// <param name="loadEntityFunc">Function that loads the entity</param>
    /// <param name="serviceError">ServiceError if entity is not found or empty</param>
    /// <returns>A validation chain that carries the loaded entity data</returns>
    public static async Task<ServiceValidationWithData<TResult, TEntity>> 
        EnsureEntityExists<TResult, TEntity>(
            this Task<ServiceValidation<TResult>> validationTask,
            Func<Task<ServiceResult<TEntity>>> loadEntityFunc,
            ServiceError serviceError)
        where TResult : class, IEmptyDto<TResult>
        where TEntity : class, IEmptyDto<TEntity>
    {
        var validation = await validationTask;
        
        // If validation already has errors, don't execute the load
        if (validation.HasErrors)
            return new ServiceValidationWithData<TResult, TEntity>(validation, default);
        
        var result = await loadEntityFunc();
        
        // Check if load failed or returned empty
        if (!result.IsSuccess || result.Data?.IsEmpty != false)
        {
            validation.Ensure(() => false, serviceError);
            return new ServiceValidationWithData<TResult, TEntity>(validation, default);
        }
        
        return new ServiceValidationWithData<TResult, TEntity>(validation, result.Data);
    }
    
    /// <summary>
    /// Continues the validation chain with the loaded entity, executing an action if validation succeeds.
    /// This is the terminal operation that consumes the loaded entity and produces the final result.
    /// </summary>
    /// <typeparam name="TResult">The result DTO type that implements IEmptyDto</typeparam>
    /// <typeparam name="TEntity">The entity DTO type that implements IEmptyDto</typeparam>
    /// <param name="validationTask">The async validation with data task</param>
    /// <param name="action">Action to execute with the loaded entity when validation succeeds</param>
    /// <returns>The result from either validation failure or the successful action</returns>
    public static async Task<ServiceResult<TResult>> ThenWithEntity<TResult, TEntity>(
        this Task<ServiceValidationWithData<TResult, TEntity>> validationTask,
        Func<TEntity, Task<ServiceResult<TResult>>> action)
        where TResult : class, IEmptyDto<TResult>
        where TEntity : class, IEmptyDto<TEntity>
    {
        var validationWithData = await validationTask;
        
        // Check if validation has errors
        if (validationWithData.Validation.HasErrors)
            return validationWithData.Validation.CreateFailureWithEmpty(TResult.Empty);
        
        // Data is guaranteed to be non-null here due to our validation
        return await action(validationWithData.Data!);
    }
    
    /// <summary>
    /// Adds a synchronous validation rule that uses the loaded entity.
    /// The validation only runs if previous validations passed and entity was loaded successfully.
    /// </summary>
    /// <typeparam name="TResult">The result DTO type that implements IEmptyDto</typeparam>
    /// <typeparam name="TEntity">The entity DTO type that implements IEmptyDto</typeparam>
    /// <param name="validationTask">The async validation with data task</param>
    /// <param name="predicate">Predicate that validates using the loaded entity</param>
    /// <param name="errorMessage">Error message if validation fails</param>
    /// <returns>The validation chain for continued processing</returns>
    public static async Task<ServiceValidationWithData<TResult, TEntity>>
        EnsureWithEntity<TResult, TEntity>(
            this Task<ServiceValidationWithData<TResult, TEntity>> validationTask,
            Func<TEntity, bool> predicate,
            string errorMessage)
        where TResult : class, IEmptyDto<TResult>
        where TEntity : class, IEmptyDto<TEntity>
    {
        var validationWithData = await validationTask;
        
        // Skip if already has errors or no data
        if (validationWithData.Validation.HasErrors || validationWithData.Data == null)
            return validationWithData;
        
        validationWithData.Validation.Ensure(() => predicate(validationWithData.Data), errorMessage);
        return validationWithData;
    }
    
    /// <summary>
    /// Adds an asynchronous validation rule that uses the loaded entity.
    /// The validation only runs if previous validations passed and entity was loaded successfully.
    /// </summary>
    /// <typeparam name="TResult">The result DTO type that implements IEmptyDto</typeparam>
    /// <typeparam name="TEntity">The entity DTO type that implements IEmptyDto</typeparam>
    /// <param name="validationTask">The async validation with data task</param>
    /// <param name="predicateAsync">Async predicate that validates using the loaded entity</param>
    /// <param name="errorMessage">Error message if validation fails</param>
    /// <returns>The validation chain for continued processing</returns>
    public static async Task<ServiceValidationWithData<TResult, TEntity>>
        EnsureAsyncWithEntity<TResult, TEntity>(
            this Task<ServiceValidationWithData<TResult, TEntity>> validationTask,
            Func<TEntity, Task<bool>> predicateAsync,
            string errorMessage)
        where TResult : class, IEmptyDto<TResult>
        where TEntity : class, IEmptyDto<TEntity>
    {
        var validationWithData = await validationTask;
        
        // Skip if already has errors or no data
        if (validationWithData.Validation.HasErrors || validationWithData.Data == null)
            return validationWithData;
        
        var result = await predicateAsync(validationWithData.Data);
        validationWithData.Validation.Ensure(() => result, errorMessage);
        return validationWithData;
    }
    
    /// <summary>
    /// Transforms the loaded entity into a different type while maintaining the validation chain.
    /// Useful for converting entities to command models or other representations.
    /// </summary>
    /// <typeparam name="TResult">The result DTO type that implements IEmptyDto</typeparam>
    /// <typeparam name="TEntity">The entity DTO type that implements IEmptyDto</typeparam>
    /// <typeparam name="TTransformed">The transformed type</typeparam>
    /// <param name="validationTask">The async validation with data task</param>
    /// <param name="transformer">Function that transforms the entity</param>
    /// <returns>A validation chain with the transformed data</returns>
    public static async Task<ServiceValidationWithData<TResult, TTransformed>>
        Transform<TResult, TEntity, TTransformed>(
            this Task<ServiceValidationWithData<TResult, TEntity>> validationTask,
            Func<TEntity, TTransformed> transformer)
        where TResult : class, IEmptyDto<TResult>
        where TEntity : class, IEmptyDto<TEntity>
        where TTransformed : class
    {
        var validationWithData = await validationTask;
        
        // If has errors or no data, return with default transformed
        if (validationWithData.Validation.HasErrors || validationWithData.Data == null)
            return new ServiceValidationWithData<TResult, TTransformed>(validationWithData.Validation, default);
        
        var transformed = transformer(validationWithData.Data);
        return new ServiceValidationWithData<TResult, TTransformed>(validationWithData.Validation, transformed);
    }
    
    /// <summary>
    /// Conditionally loads an entity based on a predicate.
    /// If the condition is false, continues the chain without loading the entity.
    /// </summary>
    /// <typeparam name="TResult">The result DTO type that implements IEmptyDto</typeparam>
    /// <typeparam name="TEntity">The entity DTO type that implements IEmptyDto</typeparam>
    /// <param name="validationTask">The async validation task</param>
    /// <param name="condition">Condition that determines if entity should be loaded</param>
    /// <param name="loadEntityFunc">Function that loads the entity</param>
    /// <param name="errorMessage">Error message if entity is required but not found</param>
    /// <returns>A validation chain that may or may not carry entity data</returns>
    public static async Task<ServiceValidationWithData<TResult, TEntity>>
        EnsureEntityExistsIf<TResult, TEntity>(
            this Task<ServiceValidation<TResult>> validationTask,
            bool condition,
            Func<Task<ServiceResult<TEntity>>> loadEntityFunc,
            string errorMessage)
        where TResult : class, IEmptyDto<TResult>
        where TEntity : class, IEmptyDto<TEntity>
    {
        // If condition is false, skip loading
        if (!condition)
        {
            var validation = await validationTask;
            return new ServiceValidationWithData<TResult, TEntity>(validation, default);
        }
        
        // Otherwise, proceed with normal entity loading
        return await validationTask.EnsureEntityExists(loadEntityFunc, errorMessage);
    }
}