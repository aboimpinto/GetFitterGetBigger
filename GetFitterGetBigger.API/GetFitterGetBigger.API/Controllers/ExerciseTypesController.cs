using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for managing exercise types reference data
/// </summary>
[ApiController]
[Route("api/ReferenceTables/[controller]")]
[Produces("application/json")]
[Tags("Reference Tables")]
public class ExerciseTypesController : ReferenceTablesBaseController<ExerciseType, ExerciseTypeId, IExerciseTypeRepository>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExerciseTypesController"/> class
    /// </summary>
    /// <param name="repository">The exercise type repository</param>
    public ExerciseTypesController(IExerciseTypeRepository repository) : base(repository)
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
    public override Task<IActionResult> GetAll()
    {
        return base.GetAll();
    }
    
    /// <summary>
    /// Gets an exercise type by ID
    /// </summary>
    /// <param name="id">The exercise type ID in format "exercisetype-{guid}"</param>
    /// <returns>The exercise type if found</returns>
    /// <response code="200">Returns the exercise type</response>
    /// <response code="404">If the exercise type is not found</response>
    [HttpGet("{id}")]
    public override Task<IActionResult> GetById(string id)
    {
        return base.GetById(id);
    }
    
    /// <summary>
    /// Gets an exercise type by value
    /// </summary>
    /// <param name="value">The exercise type value (e.g., "Warmup", "Workout", "Cooldown", "Rest")</param>
    /// <returns>The exercise type if found</returns>
    /// <response code="200">Returns the exercise type</response>
    /// <response code="404">If the exercise type is not found</response>
    [HttpGet("by-value/{value}")]
    public override Task<IActionResult> GetByValue(string value)
    {
        return base.GetByValue(value);
    }
}