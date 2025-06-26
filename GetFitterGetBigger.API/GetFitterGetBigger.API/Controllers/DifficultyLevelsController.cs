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
/// Controller for retrieving difficulty level reference data
/// </summary>
public class DifficultyLevelsController : ReferenceTablesBaseController
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DifficultyLevelsController"/> class
    /// </summary>
    /// <param name="unitOfWorkProvider">The unit of work provider</param>
    /// <param name="cacheService">The cache service</param>
    /// <param name="cacheConfiguration">The cache configuration</param>
    /// <param name="logger">The logger</param>
    public DifficultyLevelsController(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ICacheService cacheService,
        IOptions<CacheConfiguration> cacheConfiguration,
        ILogger<DifficultyLevelsController> logger)
        : base(unitOfWorkProvider, cacheService, cacheConfiguration, logger)
    {
    }

    /// <summary>
    /// Gets all active difficulty levels (cached for 24 hours)
    /// </summary>
    /// <returns>A collection of active difficulty levels</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDifficultyLevels()
    {
        var difficultyLevels = await GetAllWithCacheAsync(async () =>
        {
            using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
            var repository = unitOfWork.GetRepository<IDifficultyLevelRepository>();
            return await repository.GetAllActiveAsync();
        });
        
        // Map to DTOs
        var result = difficultyLevels.Select(MapToDto);
        
        return Ok(result);
    }

    /// <summary>
    /// Gets a difficulty level by ID
    /// </summary>
    /// <param name="id">The ID of the difficulty level to retrieve in the format "difficultylevel-{guid}"</param>
    /// <returns>The difficulty level if found, 404 Not Found otherwise</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDifficultyLevelById(string id)
    {
        // Try to parse the ID from the format "difficultylevel-{guid}"
        if (!DifficultyLevelId.TryParse(id, out var difficultyLevelId))
        {
            return BadRequest($"Invalid ID format. Expected format: 'difficultylevel-{{guid}}', got: '{id}'");
        }
        
        var difficultyLevel = await GetByIdWithCacheAsync(id, async () =>
        {
            using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
            var repository = unitOfWork.GetRepository<IDifficultyLevelRepository>();
            var entity = await repository.GetByIdAsync(difficultyLevelId);
            return (entity != null && entity.IsActive) ? entity : null;
        });
        
        if (difficultyLevel == null)
            return NotFound();
            
        // Map to DTO
        var result = MapToDto(difficultyLevel);
        
        return Ok(result);
    }

    /// <summary>
    /// Gets a difficulty level by value
    /// </summary>
    /// <param name="value">The value of the difficulty level to retrieve</param>
    /// <returns>The difficulty level if found, 404 Not Found otherwise</returns>
    [HttpGet("ByValue/{value}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDifficultyLevelByValue(string value)
    {
        var difficultyLevel = await GetByValueWithCacheAsync(value, async () =>
        {
            using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
            var repository = unitOfWork.GetRepository<IDifficultyLevelRepository>();
            return await repository.GetByValueAsync(value);
        });
        
        if (difficultyLevel == null)
            return NotFound();
            
        // Map to DTO
        var result = MapToDto(difficultyLevel);
        
        return Ok(result);
    }
}
