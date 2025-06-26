using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Olimpo.EntityFramework.Persistency;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Configuration;
using Microsoft.Extensions.Options;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for retrieving body part reference data
/// </summary>
public class BodyPartsController : ReferenceTablesBaseController
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BodyPartsController"/> class
    /// </summary>
    /// <param name="unitOfWorkProvider">The unit of work provider</param>
    /// <param name="cacheService">The cache service</param>
    /// <param name="cacheConfiguration">The cache configuration</param>
    /// <param name="logger">The logger</param>
    public BodyPartsController(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ICacheService cacheService,
        IOptions<CacheConfiguration> cacheConfiguration,
        ILogger<BodyPartsController> logger)
        : base(unitOfWorkProvider, cacheService, cacheConfiguration, logger)
    {
    }

    /// <summary>
    /// Gets all active body parts
    /// </summary>
    /// <returns>A collection of active body parts</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBodyParts()
    {
        var bodyParts = await GetAllWithCacheAsync(async () =>
        {
            using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
            var repository = unitOfWork.GetRepository<IBodyPartRepository>();
            return await repository.GetAllActiveAsync();
        });
        
        // Map to DTOs
        var result = bodyParts.Select(MapToDto);
        
        return Ok(result);
    }

    /// <summary>
    /// Gets a body part by ID
    /// </summary>
    /// <param name="id">The ID of the body part to retrieve in the format "bodypart-{guid}"</param>
    /// <returns>The body part if found, 404 Not Found otherwise</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetBodyPartById(string id)
    {
        // Try to parse the ID from the format "bodypart-{guid}"
        if (!BodyPartId.TryParse(id, out var bodyPartId))
        {
            return BadRequest($"Invalid ID format. Expected format: 'bodypart-{{guid}}', got: '{id}'");
        }
        
        var bodyPart = await GetByIdWithCacheAsync(id, async () =>
        {
            using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
            var repository = unitOfWork.GetRepository<IBodyPartRepository>();
            var entity = await repository.GetByIdAsync(bodyPartId);
            return (entity != null && entity.IsActive) ? entity : null;
        });
        
        if (bodyPart == null)
            return NotFound();
            
        // Map to DTO
        var result = MapToDto(bodyPart);
        
        return Ok(result);
    }

    /// <summary>
    /// Gets a body part by value
    /// </summary>
    /// <param name="value">The value of the body part to retrieve</param>
    /// <returns>The body part if found, 404 Not Found otherwise</returns>
    [HttpGet("ByValue/{value}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBodyPartByValue(string value)
    {
        var bodyPart = await GetByValueWithCacheAsync(value, async () =>
        {
            using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
            var repository = unitOfWork.GetRepository<IBodyPartRepository>();
            return await repository.GetByValueAsync(value);
        });
        
        if (bodyPart == null)
            return NotFound();
            
        // Map to DTO
        var result = MapToDto(bodyPart);
        
        return Ok(result);
    }
}
