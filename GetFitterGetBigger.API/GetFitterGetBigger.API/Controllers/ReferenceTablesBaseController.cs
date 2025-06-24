using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Olimpo.EntityFramework.Persistency;
using System.Reflection;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Base controller for all reference table controllers
/// </summary>
[ApiController]
[Route("api/ReferenceTables/[controller]")]
public abstract class ReferenceTablesBaseController : ControllerBase
{
    protected readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;

    protected ReferenceTablesBaseController(IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
    }

    /// <summary>
    /// Maps a reference data entity to a DTO
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <param name="entity">The entity to map</param>
    /// <returns>A DTO representing the entity</returns>
    protected ReferenceDataDto MapToDto<TEntity>(TEntity entity) where TEntity : ReferenceDataBase
    {
        // Get the Id property using reflection
        var idProperty = entity.GetType().GetProperty("Id");
        if (idProperty == null)
            throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} does not have an Id property");
            
        // Get the ID value
        var idValue = idProperty.GetValue(entity);
        if (idValue == null)
            throw new InvalidOperationException($"Id value is null for entity {typeof(TEntity).Name}");
            
        // Use the ToString() method to get the formatted ID
        var formattedId = idValue.ToString() ?? throw new InvalidOperationException($"ToString() returned null for ID of entity {typeof(TEntity).Name}");
        
        return new ReferenceDataDto
        {
            Id = formattedId,
            Value = entity.Value,
            Description = entity.Description
        };
    }
}
