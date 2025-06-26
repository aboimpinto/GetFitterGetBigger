using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Olimpo.EntityFramework.Persistency;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Configuration;
using Microsoft.Extensions.Options;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for retrieving metric type data
/// </summary>
public class MetricTypesController : ReferenceTablesBaseController
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MetricTypesController"/> class
    /// </summary>
    /// <param name="unitOfWorkProvider">The unit of work provider</param>
    /// <param name="cacheService">The cache service</param>
    /// <param name="cacheConfiguration">The cache configuration</param>
    /// <param name="logger">The logger</param>
    public MetricTypesController(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ICacheService cacheService,
        IOptions<CacheConfiguration> cacheConfiguration,
        ILogger<MetricTypesController> logger)
        : base(unitOfWorkProvider, cacheService, cacheConfiguration, logger)
    {
    }

    /// <summary>
    /// Gets all metric types
    /// </summary>
    /// <returns>A collection of metric types</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var metricTypes = await GetAllWithCacheAsync(async () =>
        {
            using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
            var repository = unitOfWork.GetRepository<IMetricTypeRepository>();
            return await repository.GetAllAsync();
        });
        
        // Map to DTOs
        var result = metricTypes.Select(mt => new ReferenceDataDto
        {
            Id = mt.Id.ToString(),
            Value = mt.Name
        });
        
        return Ok(result);
    }

    /// <summary>
    /// Gets a metric type by ID
    /// </summary>
    /// <param name="id">The ID of the metric type to retrieve in the format "metrictype-{guid}"</param>
    /// <returns>The metric type if found, 404 Not Found otherwise</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetById(string id)
    {
        // Try to parse the ID from the format "metrictype-{guid}"
        if (!MetricTypeId.TryParse(id, out var metricTypeId))
        {
            return BadRequest($"Invalid ID format. Expected format: 'metrictype-{{guid}}', got: '{id}'");
        }
        
        var metricType = await GetByIdWithCacheAsync(id, async () =>
        {
            using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
            var repository = unitOfWork.GetRepository<IMetricTypeRepository>();
            return await repository.GetByIdAsync(metricTypeId);
        });
        
        if (metricType == null)
            return NotFound();
            
        return Ok(new ReferenceDataDto
        {
            Id = metricType.Id.ToString(),
            Value = metricType.Name
        });
    }

    /// <summary>
    /// Gets a metric type by name
    /// </summary>
    /// <param name="name">The name of the metric type to retrieve</param>
    /// <returns>The metric type if found, 404 Not Found otherwise</returns>
    [HttpGet("ByName/{name}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByName(string name)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMetricTypeRepository>();
        var metricType = await repository.GetByNameAsync(name);
        
        if (metricType == null)
            return NotFound();
            
        return Ok(new ReferenceDataDto
        {
            Id = metricType.Id.ToString(),
            Value = metricType.Name
        });
    }
    
    /// <summary>
    /// Gets a metric type by value (name)
    /// </summary>
    /// <param name="value">The value (name) of the metric type to retrieve</param>
    /// <returns>The metric type if found, 404 Not Found otherwise</returns>
    [HttpGet("ByValue/{value}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByValue(string value)
    {
        var metricType = await GetByValueWithCacheAsync(value, async () =>
        {
            using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
            var repository = unitOfWork.GetRepository<IMetricTypeRepository>();
            return await repository.GetByNameAsync(value);
        });
        
        if (metricType == null)
            return NotFound();
            
        return Ok(new ReferenceDataDto
        {
            Id = metricType.Id.ToString(),
            Value = metricType.Name
        });
    }
}
