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
/// Controller for retrieving movement pattern data
/// </summary>
public class MovementPatternsController : ReferenceTablesBaseController
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MovementPatternsController"/> class
    /// </summary>
    /// <param name="unitOfWorkProvider">The unit of work provider</param>
    /// <param name="cacheService">The cache service</param>
    /// <param name="cacheConfiguration">The cache configuration</param>
    /// <param name="logger">The logger</param>
    public MovementPatternsController(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ICacheService cacheService,
        IOptions<CacheConfiguration> cacheConfiguration,
        ILogger<MovementPatternsController> logger)
        : base(unitOfWorkProvider, cacheService, cacheConfiguration, logger)
    {
    }

    /// <summary>
    /// Gets all movement patterns
    /// </summary>
    /// <returns>A collection of movement patterns</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var movementPatterns = await GetAllWithCacheAsync(async () =>
        {
            using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
            var repository = unitOfWork.GetRepository<IMovementPatternRepository>();
            return await repository.GetAllAsync();
        });
        
        // Map to DTOs
        var result = movementPatterns.Select(mp => new ReferenceDataDto
        {
            Id = mp.Id.ToString(),
            Value = mp.Name,
            Description = mp.Description
        });
        
        return Ok(result);
    }

    /// <summary>
    /// Gets a movement pattern by ID
    /// </summary>
    /// <param name="id">The ID of the movement pattern to retrieve in the format "movementpattern-{guid}"</param>
    /// <returns>The movement pattern if found, 404 Not Found otherwise</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetById(string id)
    {
        // Try to parse the ID from the format "movementpattern-{guid}"
        if (!MovementPatternId.TryParse(id, out var movementPatternId))
        {
            return BadRequest($"Invalid ID format. Expected format: 'movementpattern-{{guid}}', got: '{id}'");
        }
        
        var movementPattern = await GetByIdWithCacheAsync(id, async () =>
        {
            using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
            var repository = unitOfWork.GetRepository<IMovementPatternRepository>();
            return await repository.GetByIdAsync(movementPatternId);
        });
        
        if (movementPattern == null)
            return NotFound();
            
        return Ok(new ReferenceDataDto
        {
            Id = movementPattern.Id.ToString(),
            Value = movementPattern.Name,
            Description = movementPattern.Description
        });
    }

    /// <summary>
    /// Gets a movement pattern by name
    /// </summary>
    /// <param name="name">The name of the movement pattern to retrieve</param>
    /// <returns>The movement pattern if found, 404 Not Found otherwise</returns>
    [HttpGet("ByName/{name}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByName(string name)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMovementPatternRepository>();
        var movementPattern = await repository.GetByNameAsync(name);
        
        if (movementPattern == null)
            return NotFound();
            
        return Ok(new ReferenceDataDto
        {
            Id = movementPattern.Id.ToString(),
            Value = movementPattern.Name,
            Description = movementPattern.Description
        });
    }
    
    /// <summary>
    /// Gets a movement pattern by value (name)
    /// </summary>
    /// <param name="value">The value (name) of the movement pattern to retrieve</param>
    /// <returns>The movement pattern if found, 404 Not Found otherwise</returns>
    [HttpGet("ByValue/{value}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByValue(string value)
    {
        var movementPattern = await GetByValueWithCacheAsync(value, async () =>
        {
            using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
            var repository = unitOfWork.GetRepository<IMovementPatternRepository>();
            return await repository.GetByNameAsync(value);
        });
        
        if (movementPattern == null)
            return NotFound();
            
        return Ok(new ReferenceDataDto
        {
            Id = movementPattern.Id.ToString(),
            Value = movementPattern.Name,
            Description = movementPattern.Description
        });
    }
}
