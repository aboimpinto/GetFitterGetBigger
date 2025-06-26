using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Olimpo.EntityFramework.Persistency;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Utilities;
using GetFitterGetBigger.API.Configuration;
using Microsoft.Extensions.Options;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Base controller for all reference table controllers
/// </summary>
[ApiController]
[Route("api/ReferenceTables/[controller]")]
public abstract class ReferenceTablesBaseController : ControllerBase
{
    protected readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    protected readonly ICacheService _cacheService;
    protected readonly CacheConfiguration _cacheConfiguration;
    protected readonly ILogger<ReferenceTablesBaseController> _logger;

    protected ReferenceTablesBaseController(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ICacheService cacheService,
        IOptions<CacheConfiguration> cacheConfiguration,
        ILogger<ReferenceTablesBaseController> logger)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _cacheService = cacheService;
        _cacheConfiguration = cacheConfiguration.Value;
        _logger = logger;
    }

    /// <summary>
    /// Maps a reference data entity to a DTO
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <param name="entity">The entity to map</param>
    /// <returns>A DTO representing the entity</returns>
    protected ReferenceDataDto MapToDto<TEntity>(TEntity entity) where TEntity : ReferenceDataBase
    {
        // Get the Id property using reflection
        var idProperty = entity.GetType().GetProperty("Id");
        if (idProperty == null)
            throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} does not have an Id property");
            
        // Get the ID value
        var idValue = idProperty.GetValue(entity);
        if (idValue == null)
            throw new InvalidOperationException($"Id value is null for entity {typeof(TEntity).Name}");
            
        // Use the ToString() method to get the formatted ID
        var formattedId = idValue.ToString() ?? throw new InvalidOperationException($"ToString() returned null for ID of entity {typeof(TEntity).Name}");
        
        return new ReferenceDataDto
        {
            Id = formattedId,
            Value = entity.Value,
            Description = entity.Description
        };
    }

    /// <summary>
    /// Gets the table name for the current controller
    /// </summary>
    /// <returns>The table name</returns>
    protected virtual string GetTableName()
    {
        // Get the controller name without the "Controller" suffix
        var controllerName = GetType().Name.Replace("Controller", "");
        return controllerName;
    }

    /// <summary>
    /// Gets the cache duration for the current table
    /// </summary>
    /// <returns>The cache duration</returns>
    protected TimeSpan GetCacheDuration()
    {
        var tableName = GetTableName();
        return _cacheConfiguration.GetCacheDuration(tableName);
    }

    /// <summary>
    /// Gets all entities from cache or database
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <param name="getAllFunc">Function to get all entities from database</param>
    /// <returns>Collection of entities</returns>
    protected async Task<IEnumerable<TEntity>> GetAllWithCacheAsync<TEntity>(
        Func<Task<IEnumerable<TEntity>>> getAllFunc) where TEntity : class
    {
        var tableName = GetTableName();
        var cacheKey = CacheKeyGenerator.GetAllKey(tableName);
        var cacheDuration = GetCacheDuration();

        return await _cacheService.GetOrCreateAsync(
            cacheKey,
            getAllFunc,
            cacheDuration);
    }

    /// <summary>
    /// Gets an entity by ID from cache or database
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <param name="id">The entity ID</param>
    /// <param name="getByIdFunc">Function to get entity by ID from database</param>
    /// <returns>The entity if found</returns>
    protected async Task<TEntity?> GetByIdWithCacheAsync<TEntity>(
        string id,
        Func<Task<TEntity?>> getByIdFunc) where TEntity : class
    {
        var tableName = GetTableName();
        var cacheKey = CacheKeyGenerator.GetByIdKey(tableName, id);
        var cacheDuration = GetCacheDuration();

        var result = await _cacheService.GetOrCreateAsync(
            cacheKey,
            async () => await getByIdFunc() ?? null as TEntity,
            cacheDuration);

        return result;
    }

    /// <summary>
    /// Gets an entity by value from cache or database
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <param name="value">The value to search for</param>
    /// <param name="getByValueFunc">Function to get entity by value from database</param>
    /// <returns>The entity if found</returns>
    protected async Task<TEntity?> GetByValueWithCacheAsync<TEntity>(
        string value,
        Func<Task<TEntity?>> getByValueFunc) where TEntity : class
    {
        var tableName = GetTableName();
        var cacheKey = CacheKeyGenerator.GetByValueKey(tableName, value);
        var cacheDuration = GetCacheDuration();

        var result = await _cacheService.GetOrCreateAsync(
            cacheKey,
            async () => await getByValueFunc() ?? null as TEntity,
            cacheDuration);

        return result;
    }

    /// <summary>
    /// Invalidates all cache entries for the current table
    /// </summary>
    protected async Task InvalidateTableCacheAsync()
    {
        var tableName = GetTableName();
        var pattern = CacheKeyGenerator.GetTablePattern(tableName);
        await _cacheService.RemoveByPatternAsync(pattern);
        _logger.LogInformation("Cache invalidated for table: {TableName}", tableName);
    }
}
