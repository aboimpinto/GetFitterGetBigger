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
    private async Task ValidateRestExclusivityAsync(IEnumerable<string> exerciseTypeIds, IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork)
    {
        if (exerciseTypeIds == null || !exerciseTypeIds.Any())
            return;
            
        var typeIds = exerciseTypeIds.ToList();
        if (typeIds.Count <= 1)
            return;
            
        // Get the actual ExerciseType entities to check their values
        var exerciseTypeRepo = unitOfWork.GetRepository<IExerciseTypeRepository>();
        var hasRestType = false;
        
        foreach (var typeIdStr in typeIds)
        {
            if (ExerciseTypeId.TryParse(typeIdStr, out var typeId))
            {
                var exerciseType = await exerciseTypeRepo.GetByIdAsync(typeId);
                if (exerciseType != null && exerciseType.Value.ToLowerInvariant() == "rest")
                {
                    hasRestType = true;
                    break;
                }
            }
        }
        
        if (hasRestType)
        {
            throw new InvalidOperationException("Exercise type 'Rest' cannot be combined with other exercise types.");
        }
    }

    private async Task<bool> IsRestExerciseAsync(IEnumerable<string> exerciseTypeIds, IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork)
    {
        if (exerciseTypeIds == null || !exerciseTypeIds.Any())
            return false;

        var exerciseTypeRepo = unitOfWork.GetRepository<IExerciseTypeRepository>();
        
        foreach (var typeIdStr in exerciseTypeIds)
        {
            if (ExerciseTypeId.TryParse(typeIdStr, out var typeId))
            {
                var exerciseType = await exerciseTypeRepo.GetByIdAsync(typeId);
                if (exerciseType != null && exerciseType.Value.ToLowerInvariant() == "rest")
                {
                    return true;
                }
            }
        }
        
        return false;
    }

    private async Task ValidateMuscleGroupsAsync(CreateExerciseRequest request, IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork)
    {
        var isRestExercise = await IsRestExerciseAsync(request.ExerciseTypeIds, unitOfWork);
        
        if (!isRestExercise && (request.MuscleGroups == null || !request.MuscleGroups.Any()))
        {
            throw new InvalidOperationException("At least one muscle group must be specified for non-REST exercises.");
        }
    }

    private async Task ValidateMuscleGroupsAsync(UpdateExerciseRequest request, IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork)
    {
        var isRestExercise = await IsRestExerciseAsync(request.ExerciseTypeIds, unitOfWork);
        
        if (!isRestExercise && (request.MuscleGroups == null || !request.MuscleGroups.Any()))
        {
            throw new InvalidOperationException("At least one muscle group must be specified for non-REST exercises.");
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
        
        // Validate exercise types and muscle groups
        using (var validationUow = _unitOfWorkProvider.CreateReadOnly())
        {
            await ValidateRestExclusivityAsync(request.ExerciseTypeIds, validationUow);
            await ValidateMuscleGroupsAsync(request, validationUow);
        }
        
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
            foreach (var noteRequest in request.CoachNotes)
            {
                var coachNote = CoachNote.Handler.CreateNew(exercise.Id, noteRequest.Text, noteRequest.Order);
                exercise.CoachNotes.Add(coachNote);
            }
        }
        
        // Validate and add exercise types
        using (var readOnlyUow = _unitOfWorkProvider.CreateReadOnly())
        {
            var exerciseTypeRepo = readOnlyUow.GetRepository<IExerciseTypeRepository>();
            var uniqueExerciseTypeIds = request.ExerciseTypeIds.Distinct();
            
            foreach (var exerciseTypeIdStr in uniqueExerciseTypeIds)
            {
                if (ExerciseTypeId.TryParse(exerciseTypeIdStr, out var exerciseTypeId))
                {
                    // Verify the exercise type exists
                    var exerciseType = await exerciseTypeRepo.GetByIdAsync(exerciseTypeId);
                    if (exerciseType != null)
                    {
                        exercise.ExerciseExerciseTypes.Add(
                            ExerciseExerciseType.Handler.Create(exercise.Id, exerciseTypeId));
                    }
                }
            }
        }
        
        // Add relationships
        AddRelationshipsToExercise(exercise, request);
        
        // Save to database
        Exercise savedExercise;
        using (var unitOfWork = _unitOfWorkProvider.CreateWritable())
        {
            var repository = unitOfWork.GetRepository<IExerciseRepository>();
            
            savedExercise = await repository.AddAsync(exercise);
            await unitOfWork.CommitAsync();
        }
        
        // Map the reloaded exercise with navigation properties to DTO
        return MapToDto(savedExercise);
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
        
        // Validate exercise types and muscle groups
        using (var validationUow = _unitOfWorkProvider.CreateReadOnly())
        {
            await ValidateRestExclusivityAsync(request.ExerciseTypeIds, validationUow);
            await ValidateMuscleGroupsAsync(request, validationUow);
        }
        
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
            foreach (var noteRequest in request.CoachNotes)
            {
                if (!string.IsNullOrEmpty(noteRequest.Id))
                {
                    // Existing note - preserve ID
                    if (CoachNoteId.TryParse(noteRequest.Id, out var coachNoteId))
                    {
                        var coachNote = CoachNote.Handler.Create(coachNoteId, exercise.Id, noteRequest.Text, noteRequest.Order);
                        exercise.CoachNotes.Add(coachNote);
                    }
                }
                else
                {
                    // New note
                    var coachNote = CoachNote.Handler.CreateNew(exercise.Id, noteRequest.Text, noteRequest.Order);
                    exercise.CoachNotes.Add(coachNote);
                }
            }
        }
        
        // Validate and add exercise types
        using (var readOnlyUow = _unitOfWorkProvider.CreateReadOnly())
        {
            var exerciseTypeRepo = readOnlyUow.GetRepository<IExerciseTypeRepository>();
            var uniqueExerciseTypeIds = request.ExerciseTypeIds.Distinct();
            
            foreach (var exerciseTypeIdStr in uniqueExerciseTypeIds)
            {
                if (ExerciseTypeId.TryParse(exerciseTypeIdStr, out var exerciseTypeId))
                {
                    // Verify the exercise type exists
                    var exerciseType = await exerciseTypeRepo.GetByIdAsync(exerciseTypeId);
                    if (exerciseType != null)
                    {
                        exercise.ExerciseExerciseTypes.Add(
                            ExerciseExerciseType.Handler.Create(exercise.Id, exerciseTypeId));
                    }
                }
            }
        }
        
        // Add relationships
        AddRelationshipsToExercise(exercise, request);
        
        // Update in database
        Exercise updatedExercise;
        using (var unitOfWork = _unitOfWorkProvider.CreateWritable())
        {
            var repository = unitOfWork.GetRepository<IExerciseRepository>();
            
            updatedExercise = await repository.UpdateAsync(exercise);
            await unitOfWork.CommitAsync();
        }
        
        // Map the reloaded exercise with navigation properties to DTO
        return MapToDto(updatedExercise);
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
        if (exercise == null)
            throw new ArgumentNullException(nameof(exercise));
            
        var dto = new ExerciseDto
        {
            Id = exercise.Id.ToString(),
            Name = exercise.Name,
            Description = exercise.Description,
            CoachNotes = exercise.CoachNotes?.OrderBy(cn => cn.Order).Select(cn => new CoachNoteDto
            {
                Id = cn.Id.ToString(),
                Text = cn.Text,
                Order = cn.Order
            }).ToList() ?? new List<CoachNoteDto>(),
            ExerciseTypes = exercise.ExerciseExerciseTypes?.Select(eet => new ReferenceDataDto
            {
                Id = eet.ExerciseTypeId.ToString(),
                Value = eet.ExerciseType?.Value ?? string.Empty,
                Description = eet.ExerciseType?.Description
            }).ToList() ?? new List<ReferenceDataDto>(),
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
        if (exercise.ExerciseMuscleGroups != null)
        {
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
        }
        
        // Map equipment
        if (exercise.ExerciseEquipment != null)
        {
            foreach (var ee in exercise.ExerciseEquipment)
            {
                dto.Equipment.Add(new ReferenceDataDto
                {
                    Id = ee.Equipment?.Id.ToString() ?? string.Empty,
                    Value = ee.Equipment?.Name ?? string.Empty,
                    Description = null
                });
            }
        }
        
        // Map movement patterns
        if (exercise.ExerciseMovementPatterns != null)
        {
            foreach (var emp in exercise.ExerciseMovementPatterns)
            {
                dto.MovementPatterns.Add(new ReferenceDataDto
                {
                    Id = emp.MovementPattern?.Id.ToString() ?? string.Empty,
                    Value = emp.MovementPattern?.Name ?? string.Empty,
                    Description = emp.MovementPattern?.Description
                });
            }
        }
        
        // Map body parts
        if (exercise.ExerciseBodyParts != null)
        {
            foreach (var ebp in exercise.ExerciseBodyParts)
            {
                dto.BodyParts.Add(new ReferenceDataDto
                {
                    Id = ebp.BodyPart?.Id.ToString() ?? string.Empty,
                    Value = ebp.BodyPart?.Value ?? string.Empty,
                    Description = ebp.BodyPart?.Description
                });
            }
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