using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Olimpo.EntityFramework.Persistency;

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
    public DifficultyLevelsController(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider)
        : base(unitOfWorkProvider)
    {
    }

    /// <summary>
    /// Gets all active difficulty levels
    /// </summary>
    /// <returns>A collection of active difficulty levels</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDifficultyLevels()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IDifficultyLevelRepository>();
        var difficultyLevels = await repository.GetAllActiveAsync();
        
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
        
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IDifficultyLevelRepository>();
        var difficultyLevel = await repository.GetByIdAsync(difficultyLevelId);
        
        if (difficultyLevel == null || !difficultyLevel.IsActive)
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
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IDifficultyLevelRepository>();
        var difficultyLevel = await repository.GetByValueAsync(value);
        
        if (difficultyLevel == null)
            return NotFound();
            
        // Map to DTO
        var result = MapToDto(difficultyLevel);
        
        return Ok(result);
    }
}
