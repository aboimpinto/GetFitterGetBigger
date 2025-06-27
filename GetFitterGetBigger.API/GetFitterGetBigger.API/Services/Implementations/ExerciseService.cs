using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Interfaces;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Service implementation for Exercise business logic
/// </summary>
public class ExerciseService : IExerciseService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    
    public ExerciseService(IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
    }
    
    /// <summary>
    /// Gets a paginated list of exercises with filtering
    /// </summary>
    public async Task<PagedResponse<ExerciseDto>> GetPagedAsync(ExerciseFilterParams filterParams)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseRepository>();
        
        // Parse filter IDs
        var difficultyId = string.IsNullOrEmpty(filterParams.DifficultyId) 
            ? null 
            : ParseDifficultyId(filterParams.DifficultyId);
            
        var muscleGroupIds = ParseMuscleGroupIds(filterParams.MuscleGroupIds);
        var equipmentIds = ParseEquipmentIds(filterParams.EquipmentIds);
        var movementPatternIds = ParseMovementPatternIds(filterParams.MovementPatternIds);
        var bodyPartIds = ParseBodyPartIds(filterParams.BodyPartIds);
        
        // Get paginated results
        var (exercises, totalCount) = await repository.GetPagedAsync(
            filterParams.Page,
            filterParams.PageSize,
            filterParams.Name,
            difficultyId,
            muscleGroupIds,
            equipmentIds,
            movementPatternIds,
            bodyPartIds,
            filterParams.IncludeInactive);
        
        // Map to DTOs
        var exerciseDtos = exercises.Select(MapToDto).ToList();
        
        return new PagedResponse<ExerciseDto>(
            exerciseDtos,
            filterParams.Page,
            filterParams.PageSize,
            totalCount);
    }
    
    /// <summary>
    /// Gets an exercise by its ID
    /// </summary>
    public async Task<ExerciseDto?> GetByIdAsync(string id)
    {
        if (!ExerciseId.TryParse(id, out var exerciseId))
        {
            return null;
        }
        
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseRepository>();
        
        var exercise = await repository.GetByIdAsync(exerciseId);
        
        return exercise == null ? null : MapToDto(exercise);
    }
    
    /// <summary>
    /// Validates that if any exercise type is "Rest", it must be the only type assigned
    /// </summary>
    private void ValidateRestExclusivity(IEnumerable<string> exerciseTypeIds)
    {
        if (exerciseTypeIds == null || !exerciseTypeIds.Any())
            return;
            
        var typeIds = exerciseTypeIds.ToList();
        if (typeIds.Count <= 1)
            return;
            
        // Check if any of the IDs is for a "Rest" type
        // We need to parse the IDs and check if any has "Rest" value
        // For now, we'll check if the ID contains "rest" (case-insensitive)
        // TODO: This should ideally check against the actual ExerciseType entity value
        var hasRestType = typeIds.Any(id => id.ToLowerInvariant().Contains("rest"));
        
        if (hasRestType)
        {
            throw new InvalidOperationException("Exercise type 'Rest' cannot be combined with other exercise types.");
        }
    }
    
    /// <summary>
    /// Creates a new exercise
    /// </summary>
    public async Task<ExerciseDto> CreateAsync(CreateExerciseRequest request)
    {
        // Validate unique name
        using (var readOnlyUow = _unitOfWorkProvider.CreateReadOnly())
        {
            var readRepository = readOnlyUow.GetRepository<IExerciseRepository>();
            if (await readRepository.ExistsAsync(request.Name))
            {
                throw new InvalidOperationException($"An exercise with the name '{request.Name}' already exists.");
            }
        }
        
        // Parse IDs
        if (!DifficultyLevelId.TryParse(request.DifficultyId, out var difficultyId))
        {
            throw new ArgumentException($"Invalid difficulty ID: {request.DifficultyId}");
        }
        
        // Validate exercise types
        ValidateRestExclusivity(request.ExerciseTypeIds);
        
        // Create the exercise entity
        var exercise = Exercise.Handler.CreateNew(
            request.Name,
            request.Description,
            request.VideoUrl,
            request.ImageUrl,
            request.IsUnilateral,
            difficultyId);
        
        // Add coach notes
        if (request.CoachNotes != null)
        {
            var order = 1;
            foreach (var noteRequest in request.CoachNotes.OrderBy(cn => cn.Order))
            {
                var coachNote = CoachNote.Handler.CreateNew(exercise.Id, noteRequest.Text, order);
                exercise.CoachNotes.Add(coachNote);
                order++;
            }
        }
        
        // Add exercise types (deduplicate first)
        var uniqueExerciseTypeIds = request.ExerciseTypeIds.Distinct();
        foreach (var exerciseTypeIdStr in uniqueExerciseTypeIds)
        {
            if (ExerciseTypeId.TryParse(exerciseTypeIdStr, out var exerciseTypeId))
            {
                exercise.ExerciseExerciseTypes.Add(
                    ExerciseExerciseType.Handler.Create(exercise.Id, exerciseTypeId));
            }
        }
        
        // Add relationships
        AddRelationshipsToExercise(exercise, request);
        
        // Save to database
        using (var unitOfWork = _unitOfWorkProvider.CreateWritable())
        {
            var repository = unitOfWork.GetRepository<IExerciseRepository>();
            
            await repository.AddAsync(exercise);
            await unitOfWork.CommitAsync();
        }
        
        // For now, return a simple DTO without reloading
        // TODO: Fix this when the in-memory database issue is resolved
        return new ExerciseDto
        {
            Id = exercise.Id.ToString(),
            Name = exercise.Name,
            Description = exercise.Description,
            CoachNotes = exercise.CoachNotes.OrderBy(cn => cn.Order).Select(cn => new CoachNoteDto
            {
                Id = cn.Id.ToString(),
                Text = cn.Text,
                Order = cn.Order
            }).ToList(),
            ExerciseTypes = exercise.ExerciseExerciseTypes.Select(eet => new ReferenceDataDto
            {
                Id = eet.ExerciseTypeId.ToString(),
                Value = eet.ExerciseType?.Value ?? "Unknown",
                Description = eet.ExerciseType?.Description
            }).ToList(),
            VideoUrl = exercise.VideoUrl,
            ImageUrl = exercise.ImageUrl,
            IsUnilateral = exercise.IsUnilateral,
            IsActive = exercise.IsActive,
            Difficulty = new ReferenceDataDto
            {
                Id = exercise.DifficultyId.ToString(),
                Value = exercise.Difficulty?.Value ?? "Unknown",
                Description = exercise.Difficulty?.Description
            },
            MuscleGroups = exercise.ExerciseMuscleGroups.Select(emg => new MuscleGroupWithRoleDto
            {
                MuscleGroup = new ReferenceDataDto
                {
                    Id = emg.MuscleGroupId.ToString(),
                    Value = emg.MuscleGroup?.Name ?? "Unknown",
                    Description = null
                },
                Role = new ReferenceDataDto
                {
                    Id = emg.MuscleRoleId.ToString(),
                    Value = emg.MuscleRole?.Value ?? "Unknown",
                    Description = emg.MuscleRole?.Description
                }
            }).ToList(),
            Equipment = exercise.ExerciseEquipment.Select(ee => new ReferenceDataDto
            {
                Id = ee.EquipmentId.ToString(),
                Value = ee.Equipment?.Name ?? "Unknown",
                Description = null
            }).ToList(),
            MovementPatterns = exercise.ExerciseMovementPatterns.Select(emp => new ReferenceDataDto
            {
                Id = emp.MovementPatternId.ToString(),
                Value = emp.MovementPattern?.Name ?? "Unknown",
                Description = emp.MovementPattern?.Description
            }).ToList(),
            BodyParts = exercise.ExerciseBodyParts.Select(ebp => new ReferenceDataDto
            {
                Id = ebp.BodyPartId.ToString(),
                Value = ebp.BodyPart?.Value ?? "Unknown",
                Description = ebp.BodyPart?.Description
            }).ToList()
        };
    }
    
    /// <summary>
    /// Updates an existing exercise
    /// </summary>
    public async Task<ExerciseDto> UpdateAsync(string id, UpdateExerciseRequest request)
    {
        if (!ExerciseId.TryParse(id, out var exerciseId))
        {
            throw new ArgumentException($"Invalid exercise ID: {id}");
        }
        
        // Get existing exercise first
        Exercise? existingExercise;
        using (var readOnlyUow = _unitOfWorkProvider.CreateReadOnly())
        {
            var readRepository = readOnlyUow.GetRepository<IExerciseRepository>();
            
            // Validate unique name (excluding current exercise)
            if (await readRepository.ExistsAsync(request.Name, exerciseId))
            {
                throw new InvalidOperationException($"An exercise with the name '{request.Name}' already exists.");
            }
            
            // Get the existing exercise
            existingExercise = await readRepository.GetByIdAsync(exerciseId);
            if (existingExercise == null)
            {
                throw new InvalidOperationException($"Exercise with ID {id} not found.");
            }
        }
        
        // Parse IDs
        if (!DifficultyLevelId.TryParse(request.DifficultyId, out var difficultyId))
        {
            throw new ArgumentException($"Invalid difficulty ID: {request.DifficultyId}");
        }
        
        // Validate exercise types
        ValidateRestExclusivity(request.ExerciseTypeIds);
        
        // Create updated exercise entity, using existing values for nullable fields if not provided
        var exercise = Exercise.Handler.Create(
            exerciseId,
            request.Name,
            request.Description,
            request.VideoUrl,
            request.ImageUrl,
            request.IsUnilateral ?? existingExercise.IsUnilateral,
            request.IsActive ?? existingExercise.IsActive,
            difficultyId);
        
        // Synchronize coach notes
        if (request.CoachNotes != null)
        {
            var order = 1;
            foreach (var noteRequest in request.CoachNotes.OrderBy(cn => cn.Order))
            {
                if (!string.IsNullOrEmpty(noteRequest.Id))
                {
                    // Existing note - preserve ID
                    if (CoachNoteId.TryParse(noteRequest.Id, out var coachNoteId))
                    {
                        var coachNote = CoachNote.Handler.Create(coachNoteId, exercise.Id, noteRequest.Text, order);
                        exercise.CoachNotes.Add(coachNote);
                    }
                }
                else
                {
                    // New note
                    var coachNote = CoachNote.Handler.CreateNew(exercise.Id, noteRequest.Text, order);
                    exercise.CoachNotes.Add(coachNote);
                }
                order++;
            }
        }
        
        // Update exercise types (deduplicate first)
        var uniqueExerciseTypeIds = request.ExerciseTypeIds.Distinct();
        foreach (var exerciseTypeIdStr in uniqueExerciseTypeIds)
        {
            if (ExerciseTypeId.TryParse(exerciseTypeIdStr, out var exerciseTypeId))
            {
                exercise.ExerciseExerciseTypes.Add(
                    ExerciseExerciseType.Handler.Create(exercise.Id, exerciseTypeId));
            }
        }
        
        // Add relationships
        AddRelationshipsToExercise(exercise, request);
        
        // Update in database
        using (var unitOfWork = _unitOfWorkProvider.CreateWritable())
        {
            var repository = unitOfWork.GetRepository<IExerciseRepository>();
            
            await repository.UpdateAsync(exercise);
            await unitOfWork.CommitAsync();
        }
        
        // For now, return a simple DTO without reloading
        // TODO: Fix this when the in-memory database issue is resolved
        return new ExerciseDto
        {
            Id = exercise.Id.ToString(),
            Name = exercise.Name,
            Description = exercise.Description,
            CoachNotes = exercise.CoachNotes.OrderBy(cn => cn.Order).Select(cn => new CoachNoteDto
            {
                Id = cn.Id.ToString(),
                Text = cn.Text,
                Order = cn.Order
            }).ToList(),
            ExerciseTypes = exercise.ExerciseExerciseTypes.Select(eet => new ReferenceDataDto
            {
                Id = eet.ExerciseTypeId.ToString(),
                Value = eet.ExerciseType?.Value ?? "Unknown",
                Description = eet.ExerciseType?.Description
            }).ToList(),
            VideoUrl = exercise.VideoUrl,
            ImageUrl = exercise.ImageUrl,
            IsUnilateral = exercise.IsUnilateral,
            IsActive = exercise.IsActive,
            Difficulty = new ReferenceDataDto
            {
                Id = exercise.DifficultyId.ToString(),
                Value = "Unknown",
                Description = null
            },
            MuscleGroups = exercise.ExerciseMuscleGroups.Select(emg => new MuscleGroupWithRoleDto
            {
                MuscleGroup = new ReferenceDataDto
                {
                    Id = emg.MuscleGroupId.ToString(),
                    Value = emg.MuscleGroup?.Name ?? "Unknown",
                    Description = null
                },
                Role = new ReferenceDataDto
                {
                    Id = emg.MuscleRoleId.ToString(),
                    Value = emg.MuscleRole?.Value ?? "Unknown",
                    Description = emg.MuscleRole?.Description
                }
            }).ToList(),
            Equipment = exercise.ExerciseEquipment.Select(ee => new ReferenceDataDto
            {
                Id = ee.EquipmentId.ToString(),
                Value = ee.Equipment?.Name ?? "Unknown",
                Description = null
            }).ToList(),
            MovementPatterns = exercise.ExerciseMovementPatterns.Select(emp => new ReferenceDataDto
            {
                Id = emp.MovementPatternId.ToString(),
                Value = emp.MovementPattern?.Name ?? "Unknown",
                Description = emp.MovementPattern?.Description
            }).ToList(),
            BodyParts = exercise.ExerciseBodyParts.Select(ebp => new ReferenceDataDto
            {
                Id = ebp.BodyPartId.ToString(),
                Value = ebp.BodyPart?.Value ?? "Unknown",
                Description = ebp.BodyPart?.Description
            }).ToList()
        };
    }
    
    /// <summary>
    /// Deletes an exercise
    /// </summary>
    public async Task<bool> DeleteAsync(string id)
    {
        if (!ExerciseId.TryParse(id, out var exerciseId))
        {
            return false;
        }
        
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IExerciseRepository>();
        
        // Check if exercise exists
        var exercise = await repository.GetByIdAsync(exerciseId);
        if (exercise == null)
        {
            return false;
        }
        
        // Check for references
        var hasReferences = await repository.HasReferencesAsync(exerciseId);
        
        if (hasReferences)
        {
            // Soft delete - mark as inactive
            var updatedExercise = Exercise.Handler.Create(
                exercise.Id,
                exercise.Name,
                exercise.Description,
                exercise.VideoUrl,
                exercise.ImageUrl,
                exercise.IsUnilateral,
                false, // Mark as inactive
                exercise.DifficultyId);
            
            // Preserve existing relationships
            foreach (var emg in exercise.ExerciseMuscleGroups)
            {
                updatedExercise.ExerciseMuscleGroups.Add(emg);
            }
            foreach (var ee in exercise.ExerciseEquipment)
            {
                updatedExercise.ExerciseEquipment.Add(ee);
            }
            foreach (var emp in exercise.ExerciseMovementPatterns)
            {
                updatedExercise.ExerciseMovementPatterns.Add(emp);
            }
            foreach (var ebp in exercise.ExerciseBodyParts)
            {
                updatedExercise.ExerciseBodyParts.Add(ebp);
            }
            
            await repository.UpdateAsync(updatedExercise);
        }
        else
        {
            // Hard delete
            await repository.DeleteAsync(exerciseId);
        }
        
        await unitOfWork.CommitAsync();
        return true;
    }
    
    private void AddRelationshipsToExercise(Exercise exercise, CreateExerciseRequest request)
    {
        // Add muscle groups
        foreach (var mg in request.MuscleGroups)
        {
            if (MuscleGroupId.TryParse(mg.MuscleGroupId, out var muscleGroupId) &&
                MuscleRoleId.TryParse(mg.MuscleRoleId, out var roleId))
            {
                exercise.ExerciseMuscleGroups.Add(
                    ExerciseMuscleGroup.Handler.Create(exercise.Id, muscleGroupId, roleId));
            }
        }
        
        // Add equipment
        foreach (var equipmentIdStr in request.EquipmentIds)
        {
            if (EquipmentId.TryParse(equipmentIdStr, out var equipmentId))
            {
                exercise.ExerciseEquipment.Add(
                    ExerciseEquipment.Handler.Create(exercise.Id, equipmentId));
            }
        }
        
        // Add movement patterns
        foreach (var patternIdStr in request.MovementPatternIds)
        {
            if (MovementPatternId.TryParse(patternIdStr, out var patternId))
            {
                exercise.ExerciseMovementPatterns.Add(
                    ExerciseMovementPattern.Handler.Create(exercise.Id, patternId));
            }
        }
        
        // Add body parts
        foreach (var bodyPartIdStr in request.BodyPartIds)
        {
            if (BodyPartId.TryParse(bodyPartIdStr, out var bodyPartId))
            {
                exercise.ExerciseBodyParts.Add(
                    ExerciseBodyPart.Handler.Create(exercise.Id, bodyPartId));
            }
        }
    }
    
    private void AddRelationshipsToExercise(Exercise exercise, UpdateExerciseRequest request)
    {
        // Add muscle groups
        foreach (var mg in request.MuscleGroups)
        {
            if (MuscleGroupId.TryParse(mg.MuscleGroupId, out var muscleGroupId) &&
                MuscleRoleId.TryParse(mg.MuscleRoleId, out var roleId))
            {
                exercise.ExerciseMuscleGroups.Add(
                    ExerciseMuscleGroup.Handler.Create(exercise.Id, muscleGroupId, roleId));
            }
        }
        
        // Add equipment
        foreach (var equipmentIdStr in request.EquipmentIds)
        {
            if (EquipmentId.TryParse(equipmentIdStr, out var equipmentId))
            {
                exercise.ExerciseEquipment.Add(
                    ExerciseEquipment.Handler.Create(exercise.Id, equipmentId));
            }
        }
        
        // Add movement patterns
        foreach (var patternIdStr in request.MovementPatternIds)
        {
            if (MovementPatternId.TryParse(patternIdStr, out var patternId))
            {
                exercise.ExerciseMovementPatterns.Add(
                    ExerciseMovementPattern.Handler.Create(exercise.Id, patternId));
            }
        }
        
        // Add body parts
        foreach (var bodyPartIdStr in request.BodyPartIds)
        {
            if (BodyPartId.TryParse(bodyPartIdStr, out var bodyPartId))
            {
                exercise.ExerciseBodyParts.Add(
                    ExerciseBodyPart.Handler.Create(exercise.Id, bodyPartId));
            }
        }
    }
    
    private ExerciseDto MapToDto(Exercise exercise)
    {
        var dto = new ExerciseDto
        {
            Id = exercise.Id.ToString(),
            Name = exercise.Name,
            Description = exercise.Description,
            CoachNotes = exercise.CoachNotes.OrderBy(cn => cn.Order).Select(cn => new CoachNoteDto
            {
                Id = cn.Id.ToString(),
                Text = cn.Text,
                Order = cn.Order
            }).ToList(),
            ExerciseTypes = exercise.ExerciseExerciseTypes.Select(eet => new ReferenceDataDto
            {
                Id = eet.ExerciseType?.Id.ToString() ?? string.Empty,
                Value = eet.ExerciseType?.Value ?? string.Empty,
                Description = eet.ExerciseType?.Description
            }).ToList(),
            VideoUrl = exercise.VideoUrl,
            ImageUrl = exercise.ImageUrl,
            IsUnilateral = exercise.IsUnilateral,
            IsActive = exercise.IsActive,
            Difficulty = new ReferenceDataDto
            {
                Id = exercise.Difficulty?.Id.ToString() ?? string.Empty,
                Value = exercise.Difficulty?.Value ?? string.Empty,
                Description = exercise.Difficulty?.Description
            }
        };
        
        // Map muscle groups with roles
        foreach (var emg in exercise.ExerciseMuscleGroups)
        {
            dto.MuscleGroups.Add(new MuscleGroupWithRoleDto
            {
                MuscleGroup = new ReferenceDataDto
                {
                    Id = emg.MuscleGroup?.Id.ToString() ?? string.Empty,
                    Value = emg.MuscleGroup?.Name ?? string.Empty,
                    Description = null
                },
                Role = new ReferenceDataDto
                {
                    Id = emg.MuscleRole?.Id.ToString() ?? string.Empty,
                    Value = emg.MuscleRole?.Value ?? string.Empty,
                    Description = emg.MuscleRole?.Description
                }
            });
        }
        
        // Map equipment
        foreach (var ee in exercise.ExerciseEquipment)
        {
            dto.Equipment.Add(new ReferenceDataDto
            {
                Id = ee.Equipment?.Id.ToString() ?? string.Empty,
                Value = ee.Equipment?.Name ?? string.Empty,
                Description = null
            });
        }
        
        // Map movement patterns
        foreach (var emp in exercise.ExerciseMovementPatterns)
        {
            dto.MovementPatterns.Add(new ReferenceDataDto
            {
                Id = emp.MovementPattern?.Id.ToString() ?? string.Empty,
                Value = emp.MovementPattern?.Name ?? string.Empty,
                Description = emp.MovementPattern?.Description
            });
        }
        
        // Map body parts
        foreach (var ebp in exercise.ExerciseBodyParts)
        {
            dto.BodyParts.Add(new ReferenceDataDto
            {
                Id = ebp.BodyPart?.Id.ToString() ?? string.Empty,
                Value = ebp.BodyPart?.Value ?? string.Empty,
                Description = ebp.BodyPart?.Description
            });
        }
        
        return dto;
    }
    
    private DifficultyLevelId? ParseDifficultyId(string? idStr)
    {
        if (string.IsNullOrEmpty(idStr)) return null;
        return DifficultyLevelId.TryParse(idStr, out var id) ? id : null;
    }
    
    private IEnumerable<MuscleGroupId>? ParseMuscleGroupIds(List<string>? idStrings)
    {
        if (idStrings == null || !idStrings.Any()) return null;
        
        var ids = new List<MuscleGroupId>();
        foreach (var idStr in idStrings)
        {
            if (MuscleGroupId.TryParse(idStr, out var id))
            {
                ids.Add(id);
            }
        }
        return ids.Any() ? ids : null;
    }
    
    private IEnumerable<EquipmentId>? ParseEquipmentIds(List<string>? idStrings)
    {
        if (idStrings == null || !idStrings.Any()) return null;
        
        var ids = new List<EquipmentId>();
        foreach (var idStr in idStrings)
        {
            if (EquipmentId.TryParse(idStr, out var id))
            {
                ids.Add(id);
            }
        }
        return ids.Any() ? ids : null;
    }
    
    private IEnumerable<MovementPatternId>? ParseMovementPatternIds(List<string>? idStrings)
    {
        if (idStrings == null || !idStrings.Any()) return null;
        
        var ids = new List<MovementPatternId>();
        foreach (var idStr in idStrings)
        {
            if (MovementPatternId.TryParse(idStr, out var id))
            {
                ids.Add(id);
            }
        }
        return ids.Any() ? ids : null;
    }
    
    private IEnumerable<BodyPartId>? ParseBodyPartIds(List<string>? idStrings)
    {
        if (idStrings == null || !idStrings.Any()) return null;
        
        var ids = new List<BodyPartId>();
        foreach (var idStr in idStrings)
        {
            if (BodyPartId.TryParse(idStr, out var id))
            {
                ids.Add(id);
            }
        }
        return ids.Any() ? ids : null;
    }
}