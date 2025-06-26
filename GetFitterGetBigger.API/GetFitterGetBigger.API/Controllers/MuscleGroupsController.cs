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
/// Controller for retrieving muscle group data
/// </summary>
public class MuscleGroupsController : ReferenceTablesBaseController
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MuscleGroupsController"/> class
    /// </summary>
    /// <param name="unitOfWorkProvider">The unit of work provider</param>
    /// <param name="cacheService">The cache service</param>
    /// <param name="cacheConfiguration">The cache configuration</param>
    /// <param name="logger">The logger</param>
    public MuscleGroupsController(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ICacheService cacheService,
        IOptions<CacheConfiguration> cacheConfiguration,
        ILogger<MuscleGroupsController> logger)
        : base(unitOfWorkProvider, cacheService, cacheConfiguration, logger)
    {
    }

    /// <summary>
    /// Gets all muscle groups
    /// </summary>
    /// <returns>A collection of muscle groups</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var muscleGroups = await GetAllWithCacheAsync(async () =>
        {
            using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
            var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
            return await repository.GetAllAsync();
        });
        
        // Map to DTOs
        var result = muscleGroups.Select(mg => new ReferenceDataDto
        {
            Id = mg.Id.ToString(),
            Value = mg.Name
        });
        
        return Ok(result);
    }

    /// <summary>
    /// Gets a muscle group by ID
    /// </summary>
    /// <param name="id">The ID of the muscle group to retrieve in the format "musclegroup-{guid}"</param>
    /// <returns>The muscle group if found, 404 Not Found otherwise</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetById(string id)
    {
        // Try to parse the ID from the format "musclegroup-{guid}"
        if (!MuscleGroupId.TryParse(id, out var muscleGroupId))
        {
            return BadRequest($"Invalid ID format. Expected format: 'musclegroup-{{guid}}', got: '{id}'");
        }
        
        var muscleGroup = await GetByIdWithCacheAsync(id, async () =>
        {
            using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
            var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
            return await repository.GetByIdAsync(muscleGroupId);
        });
        
        if (muscleGroup == null)
            return NotFound();
            
        return Ok(new ReferenceDataDto
        {
            Id = muscleGroup.Id.ToString(),
            Value = muscleGroup.Name
        });
    }

    /// <summary>
    /// Gets a muscle group by name
    /// </summary>
    /// <param name="name">The name of the muscle group to retrieve</param>
    /// <returns>The muscle group if found, 404 Not Found otherwise</returns>
    [HttpGet("ByName/{name}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByName(string name)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        var muscleGroup = await repository.GetByNameAsync(name);
        
        if (muscleGroup == null)
            return NotFound();
            
        return Ok(new ReferenceDataDto
        {
            Id = muscleGroup.Id.ToString(),
            Value = muscleGroup.Name
        });
    }
    
    /// <summary>
    /// Gets a muscle group by value (name)
    /// </summary>
    /// <param name="value">The value (name) of the muscle group to retrieve</param>
    /// <returns>The muscle group if found, 404 Not Found otherwise</returns>
    [HttpGet("ByValue/{value}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByValue(string value)
    {
        var muscleGroup = await GetByValueWithCacheAsync(value, async () =>
        {
            using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
            var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
            return await repository.GetByNameAsync(value);
        });
        
        if (muscleGroup == null)
            return NotFound();
            
        return Ok(new ReferenceDataDto
        {
            Id = muscleGroup.Id.ToString(),
            Value = muscleGroup.Name
        });
    }
    
    /// <summary>
    /// Gets all muscle groups for a specific body part
    /// </summary>
    /// <param name="bodyPartId">The ID of the body part in the format "bodypart-{guid}"</param>
    /// <returns>A collection of muscle groups for the specified body part</returns>
    [HttpGet("ByBodyPart/{bodyPartId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByBodyPart(string bodyPartId)
    {
        // Try to parse the ID from the format "bodypart-{guid}"
        if (!BodyPartId.TryParse(bodyPartId, out var bodyPartIdObj))
        {
            return BadRequest($"Invalid ID format. Expected format: 'bodypart-{{guid}}', got: '{bodyPartId}'");
        }
        
        var cacheKey = $"MuscleGroups:ByBodyPart:{bodyPartId}";
        var muscleGroups = await _cacheService.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
                var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
                return await repository.GetByBodyPartAsync(bodyPartIdObj);
            },
            GetCacheDuration());
        
        // Map to DTOs
        var result = muscleGroups.Select(mg => new ReferenceDataDto
        {
            Id = mg.Id.ToString(),
            Value = mg.Name
        });
        
        return Ok(result);
    }
}
