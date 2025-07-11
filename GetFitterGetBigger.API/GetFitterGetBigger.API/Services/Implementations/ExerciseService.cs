using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Mappers;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Commands;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Builders;
using GetFitterGetBigger.API.Constants;
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
            filterParams.Name,
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

    public async Task<ExerciseDto> GetByIdAsync(ExerciseId id)
    {
        if (id.IsEmpty)
        {
            return ExerciseDto.Empty;
        }
        
        using var readOnlyUow = _unitOfWorkProvider.CreateReadOnly();
        var repository = readOnlyUow.GetRepository<IExerciseRepository>();
        
        var exercise = await repository.GetByIdAsync(id);
        return MapToExerciseDto(exercise);
    }

    public async Task<ServiceResult<ExerciseDto>> CreateAsync(CreateExerciseCommand command)
    {
        // Business validation
        var validationResult = await ValidateCreateCommand(command);
        if (!validationResult.IsValid)
        {
            return ServiceResult<ExerciseDto>.Failure(ExerciseDto.Empty, validationResult.Errors);
        }
        
        using var writableUow = _unitOfWorkProvider.CreateWritable();
        var repository = writableUow.GetRepository<IExerciseRepository>();
        
        // Check for duplicate name
        if (await repository.ExistsAsync(command.Name, null))
        {
            return ServiceResult<ExerciseDto>.Failure(ExerciseDto.Empty, string.Format(ExerciseErrorMessages.DuplicateNameFormat, command.Name));
        }
        
        // Create exercise with all data, not just basic fields
        var exercise = Exercise.Handler.CreateNew(
            command.Name,
            command.Description,
            command.VideoUrl,
            command.ImageUrl,
            command.IsUnilateral,
            command.DifficultyId,
            command.KineticChainId,
            command.ExerciseWeightTypeId);
        
        // ADD all related data:
        var exerciseWithRelations = exercise with {
            ExerciseExerciseTypes = MapToExerciseTypes(command.ExerciseTypeIds, exercise.Id),
            ExerciseMuscleGroups = MapToMuscleGroups(command.MuscleGroups, exercise.Id),
            CoachNotes = MapToCoachNotes(command.CoachNotes, exercise.Id),
            ExerciseEquipment = MapToEquipment(command.EquipmentIds, exercise.Id),
            ExerciseBodyParts = MapToBodyParts(command.BodyPartIds, exercise.Id),
            ExerciseMovementPatterns = MapToMovementPatterns(command.MovementPatternIds, exercise.Id)
        };
            
        var createdExercise = await repository.AddAsync(exerciseWithRelations);
        await writableUow.CommitAsync();
        
        return ServiceResult<ExerciseDto>.Success(MapToExerciseDto(createdExercise));
    }

    public async Task<ServiceResult<ExerciseDto>> UpdateAsync(ExerciseId id, UpdateExerciseCommand command)
    {
        // Validate the ID first
        if (id.IsEmpty)
        {
            return ServiceResult<ExerciseDto>.Failure(ExerciseDto.Empty, ExerciseErrorMessages.InvalidExerciseId);
        }
        
        // Business validation
        var validationResult = await ValidateUpdateCommand(command);
        if (!validationResult.IsValid)
        {
            return ServiceResult<ExerciseDto>.Failure(ExerciseDto.Empty, validationResult.Errors);
        }
        
        using var writableUow = _unitOfWorkProvider.CreateWritable();
        var repository = writableUow.GetRepository<IExerciseRepository>();
        
        // Check for duplicate name (excluding current exercise)
        if (await repository.ExistsAsync(command.Name, id))
        {
            return ServiceResult<ExerciseDto>.Failure(ExerciseDto.Empty, string.Format(ExerciseErrorMessages.DuplicateNameFormat, command.Name));
        }
        
        var exercise = await repository.GetByIdAsync(id);
        if (exercise.IsEmpty)
        {
            return ServiceResult<ExerciseDto>.Failure(ExerciseDto.Empty, ExerciseErrorMessages.ExerciseNotFound);
        }
        
        // PROPER UPDATE: Modify existing exercise instead of creating new one
        var updatedExercise = exercise with {
            Name = command.Name,
            Description = command.Description,
            VideoUrl = command.VideoUrl,
            ImageUrl = command.ImageUrl,
            IsUnilateral = command.IsUnilateral,
            IsActive = command.IsActive,
            DifficultyId = command.DifficultyId,
            KineticChainId = command.KineticChainId,
            ExerciseWeightTypeId = command.ExerciseWeightTypeId,
            
            // UPDATE related collections instead of losing them:
            ExerciseExerciseTypes = MapToExerciseTypes(command.ExerciseTypeIds, id),
            ExerciseMuscleGroups = MapToMuscleGroups(command.MuscleGroups, id),
            CoachNotes = MapToCoachNotes(command.CoachNotes, id),
            ExerciseEquipment = MapToEquipment(command.EquipmentIds, id),
            ExerciseBodyParts = MapToBodyParts(command.BodyPartIds, id),
            ExerciseMovementPatterns = MapToMovementPatterns(command.MovementPatternIds, id)
        };
        
        var finalExercise = updatedExercise;
        
        await repository.UpdateAsync(finalExercise);
        await writableUow.CommitAsync();
        
        // Reload the exercise with all navigation properties for proper mapping
        var reloadedExercise = await repository.GetByIdAsync(id);
        
        return ServiceResult<ExerciseDto>.Success(MapToExerciseDto(reloadedExercise));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(ExerciseId id)
    {
        // Validate the ID first
        if (id.IsEmpty)
        {
            return ServiceResult<bool>.Failure(false, ExerciseErrorMessages.InvalidExerciseId);
        }
        
        using var writableUow = _unitOfWorkProvider.CreateWritable();
        var repository = writableUow.GetRepository<IExerciseRepository>();
        
        var exercise = await repository.GetByIdAsync(id);
        if (exercise.IsEmpty)
        {
            return ServiceResult<bool>.Failure(false, ExerciseErrorMessages.ExerciseNotFound);
        }
        
        // For now, just soft delete (mark as inactive)
        // Use direct Entity Framework update to avoid relationship clearing
        await repository.SoftDeleteAsync(id);
        await writableUow.CommitAsync();
        
        return ServiceResult<bool>.Success(true);
    }

    #region Private Methods

    #region Mapping Helper Methods
    
    /// <summary>
    /// Maps exercise type IDs to ExerciseExerciseType entities
    /// </summary>
    private static ICollection<ExerciseExerciseType> MapToExerciseTypes(List<ExerciseTypeId> exerciseTypeIds, ExerciseId exerciseId)
    {
        return exerciseTypeIds
            .Where(id => !id.IsEmpty)
            .Select(id => ExerciseExerciseType.Handler.Create(exerciseId, id))
            .ToList();
    }
    
    /// <summary>
    /// Maps muscle group assignments to ExerciseMuscleGroup entities
    /// </summary>
    private static ICollection<ExerciseMuscleGroup> MapToMuscleGroups(List<MuscleGroupAssignment> muscleGroups, ExerciseId exerciseId)
    {
        return muscleGroups
            .Where(mg => !mg.MuscleGroupId.IsEmpty && !mg.MuscleRoleId.IsEmpty)
            .Select(mg => ExerciseMuscleGroup.Handler.Create(exerciseId, mg.MuscleGroupId, mg.MuscleRoleId))
            .ToList();
    }
    
    /// <summary>
    /// Maps coach note commands to CoachNote entities
    /// </summary>
    private static ICollection<CoachNote> MapToCoachNotes(List<CoachNoteCommand> coachNotes, ExerciseId exerciseId)
    {
        return coachNotes
            .Where(cn => !string.IsNullOrWhiteSpace(cn.Text))
            .Select(cn => cn.Id.HasValue && !cn.Id.Value.IsEmpty
                ? CoachNote.Handler.Create(cn.Id.Value, exerciseId, cn.Text, cn.Order)
                : CoachNote.Handler.CreateNew(exerciseId, cn.Text, cn.Order))
            .ToList();
    }
    
    /// <summary>
    /// Maps equipment IDs to ExerciseEquipment entities
    /// </summary>
    private static ICollection<ExerciseEquipment> MapToEquipment(List<EquipmentId> equipmentIds, ExerciseId exerciseId)
    {
        return equipmentIds
            .Where(id => !id.IsEmpty)
            .Select(id => ExerciseEquipment.Handler.Create(exerciseId, id))
            .ToList();
    }
    
    /// <summary>
    /// Maps body part IDs to ExerciseBodyPart entities
    /// </summary>
    private static ICollection<ExerciseBodyPart> MapToBodyParts(List<BodyPartId> bodyPartIds, ExerciseId exerciseId)
    {
        return bodyPartIds
            .Where(id => !id.IsEmpty)
            .Select(id => ExerciseBodyPart.Handler.Create(exerciseId, id))
            .ToList();
    }
    
    /// <summary>
    /// Maps movement pattern IDs to ExerciseMovementPattern entities
    /// </summary>
    private static ICollection<ExerciseMovementPattern> MapToMovementPatterns(List<MovementPatternId> movementPatternIds, ExerciseId exerciseId)
    {
        return movementPatternIds
            .Where(id => !id.IsEmpty)
            .Select(id => ExerciseMovementPattern.Handler.Create(exerciseId, id))
            .ToList();
    }
    
    #endregion

    private async Task<ValidationResult> ValidateCreateCommand(CreateExerciseCommand command)
    {
        var errors = new List<string>();
        
        // Validate required fields
        if (string.IsNullOrWhiteSpace(command.Name))
            errors.Add(ExerciseErrorMessages.ExerciseNameRequired);
            
        if (string.IsNullOrWhiteSpace(command.Description))
            errors.Add(ExerciseErrorMessages.ExerciseDescriptionRequired);
            
        if (command.DifficultyId.IsEmpty)
            errors.Add(ExerciseErrorMessages.DifficultyLevelRequired);
        
        // Validate exercise type IDs are not empty
        var validExerciseTypeIds = command.ExerciseTypeIds.Where(id => !id.IsEmpty).ToList();
        if (command.ExerciseTypeIds.Any() && !validExerciseTypeIds.Any())
        {
            errors.Add(ExerciseErrorMessages.InvalidExerciseTypeIds);
        }
        
        // Business validation: Check if exercise types contain REST
        bool isRestExercise = false;
        if (validExerciseTypeIds.Any())
        {
            var exerciseTypeIdStrings = validExerciseTypeIds.Select(id => id.ToString()).ToList();
            isRestExercise = await _exerciseTypeService.AnyIsRestTypeAsync(exerciseTypeIdStrings);
        }
        
        // Exercise type-specific validation using pattern matching
        var typeSpecificErrors = isRestExercise switch
        {
            true => ValidateRestExercise(command, validExerciseTypeIds),
            false => ValidateNonRestExercise(command)
        };
        
        errors.AddRange(typeSpecificErrors);
        
        return errors.Any() 
            ? ValidationResult.Failure(errors.ToArray()) 
            : ValidationResult.Success();
    }
    
    private async Task<ValidationResult> ValidateUpdateCommand(UpdateExerciseCommand command)
    {
        var errors = new List<string>();
        
        // Validate required fields
        if (string.IsNullOrWhiteSpace(command.Name))
            errors.Add(ExerciseErrorMessages.ExerciseNameRequired);
            
        if (string.IsNullOrWhiteSpace(command.Description))
            errors.Add(ExerciseErrorMessages.ExerciseDescriptionRequired);
            
        if (command.DifficultyId.IsEmpty)
            errors.Add(ExerciseErrorMessages.DifficultyLevelRequired);
        
        // Same validation logic as create
        bool isRestExercise = false;
        if (command.ExerciseTypeIds.Any())
        {
            var exerciseTypeIdStrings = command.ExerciseTypeIds.Select(id => id.ToString()).ToList();
            isRestExercise = await _exerciseTypeService.AnyIsRestTypeAsync(exerciseTypeIdStrings);
        }
        
        // Exercise type-specific validation using pattern matching
        var typeSpecificErrors = isRestExercise switch
        {
            true => ValidateRestExercise(command, command.ExerciseTypeIds),
            false => ValidateNonRestExercise(command)
        };
        
        errors.AddRange(typeSpecificErrors);
        
        return errors.Any() 
            ? ValidationResult.Failure(errors.ToArray()) 
            : ValidationResult.Success();
    }

    private static ExerciseDto MapToExerciseDto(Exercise exercise)
    {
        if (exercise.IsEmpty)
        {
            return ExerciseDto.Empty;
        }
        
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

    #region Exercise Type Validation Methods

    private static List<string> ValidateRestExercise(CreateExerciseCommand command, List<ExerciseTypeId> exerciseTypeIds)
    {
        return ValidateRestExerciseInternal(
            command.KineticChainId,
            command.ExerciseWeightTypeId,
            exerciseTypeIds);
    }

    private static List<string> ValidateRestExercise(UpdateExerciseCommand command, List<ExerciseTypeId> exerciseTypeIds)
    {
        return ValidateRestExerciseInternal(
            command.KineticChainId,
            command.ExerciseWeightTypeId,
            exerciseTypeIds);
    }

    private static List<string> ValidateRestExerciseInternal(
        KineticChainTypeId kineticChainId,
        ExerciseWeightTypeId exerciseWeightTypeId,
        List<ExerciseTypeId> exerciseTypeIds)
    {
        var errors = new List<string>();

        if (!kineticChainId.IsEmpty)
        {
            errors.Add(ExerciseErrorMessages.RestExerciseCannotHaveKineticChain);
        }
        
        if (!exerciseWeightTypeId.IsEmpty)
        {
            errors.Add(ExerciseErrorMessages.RestExerciseCannotHaveWeightType);
        }
        
        // Check if REST is mixed with other types
        if (exerciseTypeIds.Count > 1)
        {
            errors.Add(ExerciseErrorMessages.RestExerciseCannotBeCombined);
        }

        return errors;
    }

    private static List<string> ValidateNonRestExercise(CreateExerciseCommand command)
    {
        return ValidateNonRestExerciseInternal(
            command.KineticChainId,
            command.ExerciseWeightTypeId,
            command.MuscleGroups,
            ExerciseErrorMessages.NonRestExerciseMustHaveKineticChain);
    }

    private static List<string> ValidateNonRestExercise(UpdateExerciseCommand command)
    {
        return ValidateNonRestExerciseInternal(
            command.KineticChainId,
            command.ExerciseWeightTypeId,
            command.MuscleGroups,
            ExerciseErrorMessages.NonRestExerciseMustHaveKineticChainUpdate);
    }

    private static List<string> ValidateNonRestExerciseInternal(
        KineticChainTypeId kineticChainId,
        ExerciseWeightTypeId exerciseWeightTypeId,
        List<MuscleGroupAssignment> muscleGroups,
        string kineticChainErrorMessage)
    {
        var errors = new List<string>();

        if (kineticChainId.IsEmpty)
        {
            errors.Add(kineticChainErrorMessage);
        }
        
        if (exerciseWeightTypeId.IsEmpty)
        {
            errors.Add(ExerciseErrorMessages.NonRestExerciseMustHaveWeightType);
        }
        
        // Must have muscle groups for non-REST exercises
        if (!muscleGroups.Any())
        {
            errors.Add(ExerciseErrorMessages.NonRestExerciseMustHaveMuscleGroups);
        }
        else
        {
            // Validate muscle groups don't have empty IDs
            var muscleGroupErrors = ValidateMuscleGroups(muscleGroups);
            errors.AddRange(muscleGroupErrors);
        }

        return errors;
    }

    private static List<string> ValidateMuscleGroups(List<MuscleGroupAssignment> muscleGroups)
    {
        var errors = new List<string>();
        
        foreach (var mg in muscleGroups)
        {
            if (mg.MuscleGroupId.IsEmpty)
                errors.Add(ExerciseErrorMessages.InvalidMuscleGroupId);
            if (mg.MuscleRoleId.IsEmpty)
                errors.Add(ExerciseErrorMessages.InvalidMuscleRoleId);
        }
        
        return errors;
    }

    #endregion
}