using GetFitterGetBigger.API.Configuration;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for managing exercise types reference data
/// </summary>
[ApiController]
[Route("api/ReferenceTables/[controller]")]
[Produces("application/json")]
[Tags("Reference Tables")]
public class ExerciseTypesController : ReferenceTablesBaseController
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExerciseTypesController"/> class
    /// </summary>
    /// <param name="unitOfWorkProvider">The unit of work provider</param>
    /// <param name="cacheService">The cache service</param>
    /// <param name="cacheConfiguration">The cache configuration</param>
    /// <param name="logger">The logger</param>
    public ExerciseTypesController(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ICacheService cacheService,
        IOptions<CacheConfiguration> cacheConfiguration,
        ILogger<ExerciseTypesController> logger)
        : base(unitOfWorkProvider, cacheService, cacheConfiguration, logger)
    {
    }
    
    /// <summary>
    /// Gets all exercise types
    /// </summary>
    /// <returns>List of all exercise types</returns>
    /// <response code="200">Returns the list of exercise types</response>
    /// <remarks>
    /// Exercise types include:
    /// - Warmup: Exercises performed to prepare the body for more intense activity
    /// - Workout: Main exercises that form the core of the training session
    /// - Cooldown: Exercises performed to help the body recover after intense activity
    /// - Rest: Periods of rest between exercises or sets
    /// 
    /// Note: The "Rest" type has special business rules - it cannot be combined with other exercise types
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var exerciseTypes = await GetAllWithCacheAsync(async () =>
        {
            using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
            var repository = unitOfWork.GetRepository<IExerciseTypeRepository>();
            return await repository.GetAllActiveAsync();
        });
        
        var result = exerciseTypes.Select(MapToDto);
        return Ok(result);
    }
    
    /// <summary>
    /// Gets an exercise type by ID
    /// </summary>
    /// <param name="id">The exercise type ID in format "exercisetype-{guid}"</param>
    /// <returns>The exercise type if found</returns>
    /// <response code="200">Returns the exercise type</response>
    /// <response code="404">If the exercise type is not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var exerciseType = await GetByIdWithCacheAsync(id, async () =>
        {
            using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
            var repository = unitOfWork.GetRepository<IExerciseTypeRepository>();
            
            if (!ExerciseTypeId.TryParse(id, out var exerciseTypeId))
            {
                return null;
            }
            
            return await repository.GetByIdAsync(exerciseTypeId);
        });
        
        if (exerciseType == null)
        {
            return NotFound();
        }
        
        return Ok(MapToDto(exerciseType));
    }
    
    /// <summary>
    /// Gets an exercise type by value
    /// </summary>
    /// <param name="value">The exercise type value (e.g., "Warmup", "Workout", "Cooldown", "Rest")</param>
    /// <returns>The exercise type if found</returns>
    /// <response code="200">Returns the exercise type</response>
    /// <response code="404">If the exercise type is not found</response>
    [HttpGet("ByValue/{value}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByValue(string value)
    {
        var exerciseType = await GetByValueWithCacheAsync(value, async () =>
        {
            using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
            var repository = unitOfWork.GetRepository<IExerciseTypeRepository>();
            return await repository.GetByValueAsync(value);
        });
        
        if (exerciseType == null)
        {
            return NotFound();
        }
        
        return Ok(MapToDto(exerciseType));
    }
}