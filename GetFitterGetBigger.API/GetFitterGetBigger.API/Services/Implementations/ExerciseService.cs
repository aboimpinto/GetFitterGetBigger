using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Mappers;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Commands;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Builders;
using Olimpo.EntityFramework.Persistency;
using System.Linq;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Service for Exercise business logic using clean architecture command pattern
/// </summary>
public class ExerciseService : IExerciseService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly IExerciseTypeService _exerciseTypeService;

    public ExerciseService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        IExerciseTypeService exerciseTypeService)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _exerciseTypeService = exerciseTypeService;
    }

    public async Task<PagedResponse<ExerciseDto>> GetPagedAsync(GetExercisesCommand filterParams)
    {
        using var readOnlyUow = _unitOfWorkProvider.CreateReadOnly();
        var repository = readOnlyUow.GetRepository<IExerciseRepository>();

        var (exercises, totalCount) = await repository.GetPagedAsync(
            filterParams.Page,
            filterParams.PageSize,
            filterParams.SearchTerm,
            filterParams.DifficultyLevelId,
            filterParams.MuscleGroupIds,
            filterParams.EquipmentIds,
            filterParams.MovementPatternIds,
            filterParams.BodyPartIds,
            filterParams.IsActive);

        var exerciseDtos = exercises
            .Select(MapToExerciseDto)
            .ToList();

        return new PagedResponse<ExerciseDto>
        {
            Items = exerciseDtos,
            TotalCount = totalCount,
            PageSize = filterParams.PageSize,
            CurrentPage = filterParams.Page
        };
    }

    public async Task<ExerciseDto?> GetByIdAsync(string id)
    {
        using var readOnlyUow = _unitOfWorkProvider.CreateReadOnly();
        var repository = readOnlyUow.GetRepository<IExerciseRepository>();
        
        if (!ExerciseId.TryParse(id, out var exerciseId))
        {
            return null;
        }
        
        var exercise = await repository.GetByIdAsync(exerciseId);
        if (exercise == null)
        {
            return null;
        }
        
        return MapToExerciseDto(exercise);
    }

    public async Task<ExerciseDto> CreateAsync(CreateExerciseRequest request)
    {
        // Map DTO to Command (with validation)
        var command = request.ToCommand();
        
        // Business validation
        await ValidateCreateCommand(command);
        
        using var writableUow = _unitOfWorkProvider.CreateWritable();
        var repository = writableUow.GetRepository<IExerciseRepository>();
        
        // Check for duplicate name
        if (await repository.ExistsAsync(command.Name, null))
        {
            throw new InvalidOperationException($"Exercise with name '{command.Name}' already exists.");
        }
        
        // Create exercise with strongly-typed IDs
        var exercise = Exercise.Handler.CreateNew(
            command.Name,
            command.Description,
            command.VideoUrl,
            command.ImageUrl,
            command.IsUnilateral,
            command.DifficultyId,
            command.KineticChainId);
            
        var createdExercise = await repository.AddAsync(exercise);
        await writableUow.CommitAsync();
        
        return MapToExerciseDto(createdExercise);
    }

    public async Task<ExerciseDto> UpdateAsync(string id, UpdateExerciseRequest request)
    {
        if (!ExerciseId.TryParse(id, out var exerciseId))
        {
            throw new ArgumentException($"Invalid exercise ID: {id}");
        }
        
        // Map DTO to Command (with validation)
        var command = request.ToCommand();
        
        // Business validation
        await ValidateUpdateCommand(command);
        
        using var writableUow = _unitOfWorkProvider.CreateWritable();
        var repository = writableUow.GetRepository<IExerciseRepository>();
        
        // Check for duplicate name (excluding current exercise)
        if (await repository.ExistsAsync(command.Name, exerciseId))
        {
            throw new InvalidOperationException($"Exercise with name '{command.Name}' already exists.");
        }
        
        var exercise = await repository.GetByIdAsync(exerciseId);
        if (exercise == null)
        {
            throw new InvalidOperationException("Exercise not found");
        }
        
        // Update exercise (simplified - would need proper update logic)
        var updatedExercise = Exercise.Handler.CreateNew(
            command.Name,
            command.Description,
            command.VideoUrl,
            command.ImageUrl,
            command.IsUnilateral,
            command.DifficultyId,
            command.KineticChainId);
            
        // Copy the original ID and set active status
        var finalExercise = updatedExercise with { 
            Id = exerciseId,
            IsActive = command.IsActive
        };
        
        await repository.UpdateAsync(finalExercise);
        await writableUow.CommitAsync();
        
        return MapToExerciseDto(finalExercise);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (!ExerciseId.TryParse(id, out var exerciseId))
        {
            return false;
        }
        
        using var writableUow = _unitOfWorkProvider.CreateWritable();
        var repository = writableUow.GetRepository<IExerciseRepository>();
        
        var exercise = await repository.GetByIdAsync(exerciseId);
        if (exercise == null)
        {
            return false;
        }
        
        // For now, just soft delete (mark as inactive)
        var softDeletedExercise = exercise with { IsActive = false };
        await repository.UpdateAsync(softDeletedExercise);
        await writableUow.CommitAsync();
        
        return true;
    }

    #region Private Methods

    private async Task ValidateCreateCommand(CreateExerciseCommand command)
    {
        // Business validation: Check if exercise types contain REST
        bool isRestExercise = false;
        if (command.ExerciseTypeIds.Any())
        {
            var exerciseTypeIdStrings = command.ExerciseTypeIds.Select(id => id.ToString()).ToList();
            isRestExercise = await _exerciseTypeService.AnyIsRestTypeAsync(exerciseTypeIdStrings);
        }
        
        // REST exercises validation
        if (isRestExercise)
        {
            if (command.KineticChainId.HasValue)
            {
                throw new InvalidOperationException("Kinetic chain type must not be specified for REST exercises.");
            }
            
            if (command.ExerciseWeightTypeId.HasValue)
            {
                throw new InvalidOperationException("REST exercises cannot have a weight type.");
            }
            
            // Check if REST is mixed with other types
            if (command.ExerciseTypeIds.Count > 1)
            {
                throw new InvalidOperationException("REST exercises cannot be combined with other exercise types.");
            }
        }
        else
        {
            // Non-REST exercises validation
            if (!command.KineticChainId.HasValue)
            {
                throw new InvalidOperationException("Non-REST exercises must have a kinetic chain.");
            }
            
            if (!command.ExerciseWeightTypeId.HasValue)
            {
                throw new InvalidOperationException("Non-REST exercises must have a weight type.");
            }
            
            // Must have muscle groups for non-REST exercises
            if (!command.MuscleGroups.Any())
            {
                throw new InvalidOperationException("Non-REST exercises must have at least one muscle group.");
            }
        }
    }
    
    private async Task ValidateUpdateCommand(UpdateExerciseCommand command)
    {
        // Same validation logic as create
        bool isRestExercise = false;
        if (command.ExerciseTypeIds.Any())
        {
            var exerciseTypeIdStrings = command.ExerciseTypeIds.Select(id => id.ToString()).ToList();
            isRestExercise = await _exerciseTypeService.AnyIsRestTypeAsync(exerciseTypeIdStrings);
        }
        
        if (isRestExercise)
        {
            if (command.KineticChainId.HasValue)
            {
                throw new InvalidOperationException("Kinetic chain type must not be specified for REST exercises.");
            }
            
            if (command.ExerciseWeightTypeId.HasValue)
            {
                throw new InvalidOperationException("REST exercises cannot have a weight type.");
            }
            
            if (command.ExerciseTypeIds.Count > 1)
            {
                throw new InvalidOperationException("REST exercises cannot be combined with other exercise types.");
            }
        }
        else
        {
            if (!command.KineticChainId.HasValue)
            {
                throw new InvalidOperationException("Non-REST exercises must have a kinetic chain.");
            }
            
            if (!command.ExerciseWeightTypeId.HasValue)
            {
                throw new InvalidOperationException("Non-REST exercises must have a weight type.");
            }
            
            if (!command.MuscleGroups.Any())
            {
                throw new InvalidOperationException("Non-REST exercises must have at least one muscle group.");
            }
        }
    }

    private static ExerciseDto MapToExerciseDto(Exercise exercise)
    {
        return new ExerciseDtoBuilder(exercise)
            .WithBasicInfo()
            .WithCoachNotes()
            .WithExerciseTypes()
            .WithMuscleGroups()
            .WithEquipment()
            .WithBodyParts()
            .WithMovementPatterns()
            .WithReferenceData()
            .Build();
    }

    #endregion
}