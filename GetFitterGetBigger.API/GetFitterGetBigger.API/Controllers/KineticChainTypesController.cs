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
/// Controller for retrieving kinetic chain type reference data
/// </summary>
public class KineticChainTypesController : ReferenceTablesBaseController
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KineticChainTypesController"/> class
    /// </summary>
    /// <param name="unitOfWorkProvider">The unit of work provider</param>
    /// <param name="cacheService">The cache service</param>
    /// <param name="cacheConfiguration">The cache configuration</param>
    /// <param name="logger">The logger</param>
    public KineticChainTypesController(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ICacheService cacheService,
        IOptions<CacheConfiguration> cacheConfiguration,
        ILogger<KineticChainTypesController> logger)
        : base(unitOfWorkProvider, cacheService, cacheConfiguration, logger)
    {
    }

    /// <summary>
    /// Gets all active kinetic chain types
    /// </summary>
    /// <returns>A collection of active kinetic chain types</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetKineticChainTypes()
    {
        var kineticChainTypes = await GetAllWithCacheAsync(async () =>
        {
            using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
            var repository = unitOfWork.GetRepository<IKineticChainTypeRepository>();
            return await repository.GetAllActiveAsync();
        });
        
        // Map to DTOs
        var result = kineticChainTypes.Select(MapToDto);
        
        return Ok(result);
    }

    /// <summary>
    /// Gets a kinetic chain type by ID
    /// </summary>
    /// <param name="id">The ID of the kinetic chain type to retrieve in the format "kineticchaintype-{guid}"</param>
    /// <returns>The kinetic chain type if found, 404 Not Found otherwise</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetKineticChainTypeById(string id)
    {
        // Try to parse the ID from the format "kineticchaintype-{guid}"
        if (!KineticChainTypeId.TryParse(id, out var kineticChainTypeId))
        {
            return BadRequest($"Invalid ID format. Expected format: 'kineticchaintype-{{guid}}', got: '{id}'");
        }
        
        var kineticChainType = await GetByIdWithCacheAsync(id, async () =>
        {
            using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
            var repository = unitOfWork.GetRepository<IKineticChainTypeRepository>();
            var entity = await repository.GetByIdAsync(kineticChainTypeId);
            return (entity != null && entity.IsActive) ? entity : null;
        });
        
        if (kineticChainType == null)
            return NotFound();
            
        // Map to DTO
        var result = MapToDto(kineticChainType);
        
        return Ok(result);
    }

    /// <summary>
    /// Gets a kinetic chain type by value
    /// </summary>
    /// <param name="value">The value of the kinetic chain type to retrieve</param>
    /// <returns>The kinetic chain type if found, 404 Not Found otherwise</returns>
    [HttpGet("ByValue/{value}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetKineticChainTypeByValue(string value)
    {
        var kineticChainType = await GetByValueWithCacheAsync(value, async () =>
        {
            using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
            var repository = unitOfWork.GetRepository<IKineticChainTypeRepository>();
            return await repository.GetByValueAsync(value);
        });
        
        if (kineticChainType == null)
            return NotFound();
            
        // Map to DTO
        var result = MapToDto(kineticChainType);
        
        return Ok(result);
    }
}
