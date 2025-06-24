using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for retrieving muscle role reference data
/// </summary>
public class MuscleRolesController : ReferenceTablesBaseController
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MuscleRolesController"/> class
    /// </summary>
    /// <param name="unitOfWorkProvider">The unit of work provider</param>
    public MuscleRolesController(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider)
        : base(unitOfWorkProvider)
    {
    }

    /// <summary>
    /// Gets all active muscle roles
    /// </summary>
    /// <returns>A collection of active muscle roles</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMuscleRoles()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleRoleRepository>();
        var muscleRoles = await repository.GetAllActiveAsync();
        
        // Map to DTOs
        var result = muscleRoles.Select(MapToDto);
        
        return Ok(result);
    }

    /// <summary>
    /// Gets a muscle role by ID
    /// </summary>
    /// <param name="id">The ID of the muscle role to retrieve in the format "musclerole-{guid}"</param>
    /// <returns>The muscle role if found, 404 Not Found otherwise</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMuscleRoleById(string id)
    {
        // Try to parse the ID from the format "musclerole-{guid}"
        if (!MuscleRoleId.TryParse(id, out var muscleRoleId))
        {
            return BadRequest($"Invalid ID format. Expected format: 'musclerole-{{guid}}', got: '{id}'");
        }
        
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleRoleRepository>();
        var muscleRole = await repository.GetByIdAsync(muscleRoleId);
        
        if (muscleRole == null || !muscleRole.IsActive)
            return NotFound();
            
        // Map to DTO
        var result = MapToDto(muscleRole);
        
        return Ok(result);
    }

    /// <summary>
    /// Gets a muscle role by value
    /// </summary>
    /// <param name="value">The value of the muscle role to retrieve</param>
    /// <returns>The muscle role if found, 404 Not Found otherwise</returns>
    [HttpGet("ByValue/{value}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMuscleRoleByValue(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleRoleRepository>();
        var muscleRole = await repository.GetByValueAsync(value);
        
        if (muscleRole == null)
            return NotFound();
            
        // Map to DTO
        var result = MapToDto(muscleRole);
        
        return Ok(result);
    }
}
