using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Commands;
using GetFitterGetBigger.API.Services.Interfaces;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Temporary simplified service implementation for Exercise business logic
/// This version is compatible with the new Command pattern but simplified to get the build working
/// </summary>
public class ExerciseServiceTemp : IExerciseService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly IExerciseTypeService _exerciseTypeService;
    
    public ExerciseServiceTemp(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        IExerciseTypeService exerciseTypeService)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _exerciseTypeService = exerciseTypeService;
    }
    
    public async Task<PagedResponse<ExerciseDto>> GetPagedAsync(GetExercisesCommand filterParams)
    {
        // Simplified implementation - just return empty for now to get building
        return new PagedResponse<ExerciseDto>
        {
            Items = new List<ExerciseDto>(),
            TotalCount = 0
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
        
        return new ExerciseDto
        {
            Id = exercise.Id.ToString(),
            Name = exercise.Name,
            Description = exercise.Description,
            VideoUrl = exercise.VideoUrl,
            ImageUrl = exercise.ImageUrl,
            IsUnilateral = exercise.IsUnilateral,
            IsActive = exercise.IsActive,
            CoachNotes = exercise.CoachNotes?.OrderBy(cn => cn.Order).Select(cn => new CoachNoteDto
            {
                Id = cn.Id.ToString(),
                Text = cn.Text,
                Order = cn.Order
            }).ToList() ?? new List<CoachNoteDto>(),
            ExerciseTypes = exercise.ExerciseExerciseTypes?.Select(eet => new ReferenceDataDto
            {
                Id = eet.ExerciseType?.Id.ToString() ?? "",
                Value = eet.ExerciseType?.Value ?? "",
                Description = eet.ExerciseType?.Description
            }).ToList() ?? new List<ReferenceDataDto>(),
            MuscleGroups = new List<MuscleGroupWithRoleDto>(),
            Equipment = new List<ReferenceDataDto>(),
            BodyParts = new List<ReferenceDataDto>(),
            MovementPatterns = new List<ReferenceDataDto>(),
            Difficulty = exercise.Difficulty != null ? new ReferenceDataDto
            {
                Id = exercise.Difficulty.Id.ToString(),
                Value = exercise.Difficulty.Value,
                Description = exercise.Difficulty.Description
            } : null,
            KineticChain = exercise.KineticChain != null ? new ReferenceDataDto
            {
                Id = exercise.KineticChain.Id.ToString(),
                Value = exercise.KineticChain.Value,
                Description = exercise.KineticChain.Description
            } : null,
            ExerciseWeightType = exercise.ExerciseWeightType != null ? new ReferenceDataDto
            {
                Id = exercise.ExerciseWeightType.Id.ToString(),
                Value = exercise.ExerciseWeightType.Value,
                Description = exercise.ExerciseWeightType.Description
            } : null
        };
    }

    public async Task<ExerciseDto> CreateAsync(CreateExerciseRequest request)
    {
        // Simplified implementation - create basic exercise
        using var writableUow = _unitOfWorkProvider.CreateWritable();
        var repository = writableUow.GetRepository<IExerciseRepository>();
        
        // Check for duplicate name
        if (await repository.ExistsAsync(request.Name, null))
        {
            throw new InvalidOperationException($"Exercise with name '{request.Name}' already exists.");
        }
        
        // Parse difficulty ID
        if (!DifficultyLevelId.TryParse(request.DifficultyId, out var difficultyId))
        {
            throw new ArgumentException($"Invalid difficulty ID: {request.DifficultyId}");
        }
        
        // Parse kinetic chain ID if provided
        KineticChainTypeId? kineticChainId = null;
        if (!string.IsNullOrEmpty(request.KineticChainId))
        {
            if (!KineticChainTypeId.TryParse(request.KineticChainId, out var parsedKineticChainId))
            {
                throw new ArgumentException($"Invalid kinetic chain ID: {request.KineticChainId}");
            }
            kineticChainId = parsedKineticChainId;
        }
        
        // Business validation: Check if exercise types contain REST
        bool isRestExercise = false;
        if (request.ExerciseTypeIds?.Any() == true)
        {
            isRestExercise = await _exerciseTypeService.AnyIsRestTypeAsync(request.ExerciseTypeIds);
        }
        
        // REST exercises validation
        if (isRestExercise)
        {
            if (kineticChainId.HasValue)
            {
                throw new InvalidOperationException("Kinetic chain type must not be specified for REST exercises.");
            }
            
            if (!string.IsNullOrEmpty(request.ExerciseWeightTypeId))
            {
                throw new InvalidOperationException("REST exercises cannot have a weight type.");
            }
            
            // Check if REST is mixed with other types
            if (request.ExerciseTypeIds.Count() > 1)
            {
                throw new InvalidOperationException("REST exercises cannot be combined with other exercise types.");
            }
        }
        else
        {
            // Non-REST exercises validation
            if (!kineticChainId.HasValue)
            {
                throw new InvalidOperationException("Non-REST exercises must have a kinetic chain.");
            }
            
            if (string.IsNullOrEmpty(request.ExerciseWeightTypeId))
            {
                throw new InvalidOperationException("Non-REST exercises must have a weight type.");
            }
            
            // Must have muscle groups for non-REST exercises
            if (request.MuscleGroups?.Any() != true)
            {
                throw new InvalidOperationException("Non-REST exercises must have at least one muscle group.");
            }
        }
        
        // Create exercise with minimal data
        var exercise = Exercise.Handler.CreateNew(
            request.Name,
            request.Description,
            request.VideoUrl,
            request.ImageUrl,  
            request.IsUnilateral,
            difficultyId,
            kineticChainId);
            
        var createdExercise = await repository.AddAsync(exercise);
        await writableUow.CommitAsync();
        
        // Return basic DTO
        return new ExerciseDto
        {
            Id = createdExercise.Id.ToString(),
            Name = createdExercise.Name,
            Description = createdExercise.Description,
            CoachNotes = new List<CoachNoteDto>(),
            ExerciseTypes = new List<ReferenceDataDto>(),
            MuscleGroups = new List<MuscleGroupWithRoleDto>(),
            Equipment = new List<ReferenceDataDto>(),
            BodyParts = new List<ReferenceDataDto>(),
            MovementPatterns = new List<ReferenceDataDto>()
        };
    }

    public async Task<ExerciseDto> UpdateAsync(string id, UpdateExerciseRequest request)
    {
        if (!ExerciseId.TryParse(id, out var exerciseId))
        {
            throw new ArgumentException($"Invalid exercise ID: {id}");
        }
        
        using var writableUow = _unitOfWorkProvider.CreateWritable();
        var repository = writableUow.GetRepository<IExerciseRepository>();
        
        // Check for duplicate name (excluding current exercise)
        if (await repository.ExistsAsync(request.Name, exerciseId))
        {
            throw new InvalidOperationException($"Exercise with name '{request.Name}' already exists.");
        }
        
        var exercise = await repository.GetByIdAsync(exerciseId);
        if (exercise == null)
        {
            throw new InvalidOperationException("Exercise not found");
        }
        
        // Parse difficulty ID
        if (!DifficultyLevelId.TryParse(request.DifficultyId, out var difficultyId))
        {
            throw new ArgumentException($"Invalid difficulty ID: {request.DifficultyId}");
        }
        
        // Parse kinetic chain ID if provided
        KineticChainTypeId? kineticChainId = null;
        if (!string.IsNullOrEmpty(request.KineticChainId))
        {
            if (!KineticChainTypeId.TryParse(request.KineticChainId, out var parsedKineticChainId))
            {
                throw new ArgumentException($"Invalid kinetic chain ID: {request.KineticChainId}");
            }
            kineticChainId = parsedKineticChainId;
        }
        
        // Business validation: Check if exercise types contain REST
        bool isRestExercise = false;
        if (request.ExerciseTypeIds?.Any() == true)
        {
            isRestExercise = await _exerciseTypeService.AnyIsRestTypeAsync(request.ExerciseTypeIds);
        }
        
        // REST exercises validation
        if (isRestExercise)
        {
            if (kineticChainId.HasValue)
            {
                throw new InvalidOperationException("Kinetic chain type must not be specified for REST exercises.");
            }
            
            if (!string.IsNullOrEmpty(request.ExerciseWeightTypeId))
            {
                throw new InvalidOperationException("REST exercises cannot have a weight type.");
            }
            
            // Check if REST is mixed with other types
            if (request.ExerciseTypeIds?.Count() > 1)
            {
                throw new InvalidOperationException("REST exercises cannot be combined with other exercise types.");
            }
        }
        else
        {
            // Non-REST exercises validation
            if (!kineticChainId.HasValue)
            {
                throw new InvalidOperationException("Non-REST exercises must have a kinetic chain.");
            }
            
            if (string.IsNullOrEmpty(request.ExerciseWeightTypeId))
            {
                throw new InvalidOperationException("Non-REST exercises must have a weight type.");
            }
            
            // Must have muscle groups for non-REST exercises
            if (request.MuscleGroups?.Any() != true)
            {
                throw new InvalidOperationException("Non-REST exercises must have at least one muscle group.");
            }
        }
        
        // Update exercise (simplified)
        var updatedExercise = Exercise.Handler.CreateNew(
            request.Name,
            request.Description,
            request.VideoUrl,
            request.ImageUrl,
            request.IsUnilateral.GetValueOrDefault(false),
            difficultyId,
            kineticChainId);
            
        // Copy the original ID and set active status
        var finalExercise = updatedExercise with { 
            Id = exerciseId,
            IsActive = request.IsActive.GetValueOrDefault(true)
        };
        
        await repository.UpdateAsync(finalExercise);
        await writableUow.CommitAsync();
        
        // Return DTO
        return new ExerciseDto
        {
            Id = finalExercise.Id.ToString(),
            Name = finalExercise.Name,
            Description = finalExercise.Description,
            VideoUrl = finalExercise.VideoUrl,
            ImageUrl = finalExercise.ImageUrl,
            IsUnilateral = finalExercise.IsUnilateral,
            IsActive = finalExercise.IsActive,
            CoachNotes = new List<CoachNoteDto>(),
            ExerciseTypes = new List<ReferenceDataDto>(),
            MuscleGroups = new List<MuscleGroupWithRoleDto>(),
            Equipment = new List<ReferenceDataDto>(),
            BodyParts = new List<ReferenceDataDto>(),
            MovementPatterns = new List<ReferenceDataDto>()
        };
    }

    public async Task<bool> DeleteAsync(string id)
    {
        // Simplified implementation
        await Task.CompletedTask;
        return true;
    }
}