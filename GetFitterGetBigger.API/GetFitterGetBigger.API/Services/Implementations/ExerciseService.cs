using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Commands;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using GetFitterGetBigger.API.Builders;
using GetFitterGetBigger.API.Constants;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Service for Exercise business logic using clean architecture command pattern
/// </summary>
public class ExerciseService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    IExerciseTypeService exerciseTypeService) : IExerciseService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;
    private readonly IExerciseTypeService _exerciseTypeService = exerciseTypeService;

    public async Task<ServiceResult<PagedResponse<ExerciseDto>>> GetPagedAsync(GetExercisesCommand filterParams)
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

        var response = new PagedResponse<ExerciseDto>
        {
            Items = exerciseDtos,
            TotalCount = totalCount,
            PageSize = filterParams.PageSize,
            CurrentPage = filterParams.Page
        };

        return ServiceResult<PagedResponse<ExerciseDto>>.Success(response);
    }

    public async Task<ServiceResult<ExerciseDto>> GetByIdAsync(ExerciseId id)
    {
        return await ServiceValidate.Build<ExerciseDto>()
            .EnsureNotEmpty(id, ExerciseErrorMessages.InvalidIdFormat)
            .EnsureAsync(
                async () => await ExerciseExistsAsync(id),
                ServiceError.NotFound("Exercise", id.ToString()))
            .MatchAsync(
                whenValid: async () => await GetExerciseByIdInternalAsync(id));
    }
    
    private async Task<ServiceResult<ExerciseDto>> GetExerciseByIdInternalAsync(ExerciseId id)
    {
        using var readOnlyUow = _unitOfWorkProvider.CreateReadOnly();
        var repository = readOnlyUow.GetRepository<IExerciseRepository>();
        
        var exercise = await repository.GetByIdAsync(id);
        return ServiceResult<ExerciseDto>.Success(MapToExerciseDto(exercise));
    }

    public async Task<ServiceResult<ExerciseDto>> CreateAsync(CreateExerciseCommand command)
    {
        return await ServiceValidate.Build<ExerciseDto>()
            .EnsureNotWhiteSpace(command.Name, ExerciseErrorMessages.ExerciseNameRequired)
            .EnsureNotWhiteSpace(command.Description, ExerciseErrorMessages.ExerciseDescriptionRequired)
            .EnsureNotEmpty(command.DifficultyId, ExerciseErrorMessages.DifficultyLevelRequired)
            .EnsureMaxLength(command.Name, 255, ExerciseErrorMessages.ExerciseNameMaxLength)
            .EnsureNameIsUniqueAsync(
                async () => await IsNameUniqueAsync(command.Name, null),
                "Exercise",
                command.Name)
            .EnsureHasValidAsync(
                async () => await HasValidExerciseTypesAsync(command.ExerciseTypeIds),
                ExerciseErrorMessages.InvalidExerciseTypeConfiguration)
            .EnsureHasValidAsync(
                async () => await ValidateKineticChainForExerciseTypesAsync(command.ExerciseTypeIds, command.KineticChainId),
                "REST exercises cannot have kinetic chain; Non-REST exercises must have kinetic chain")
            .EnsureHasValidAsync(
                async () => await ValidateWeightTypeForExerciseTypesAsync(command.ExerciseTypeIds, command.ExerciseWeightTypeId),
                "REST exercises cannot have weight type")
            .EnsureHasValidAsync(
                async () => await ValidateMuscleGroupsForExerciseTypesAsync(command.ExerciseTypeIds, command.MuscleGroups),
                "REST exercises cannot have muscle groups; Non-REST exercises must have at least one muscle group")
            .MatchAsync(
                whenValid: async () => await CreateExerciseInternalAsync(command)
            );
    }

    private async Task<ServiceResult<ExerciseDto>> CreateExerciseInternalAsync(CreateExerciseCommand command)
    {
        using var writableUow = _unitOfWorkProvider.CreateWritable();
        var repository = writableUow.GetRepository<IExerciseRepository>();
        
        // Create exercise with all data, not just basic fields
        var exercise = Exercise.Handler.CreateNew(
            command.Name,
            command.Description,
            command.VideoUrl,
            command.ImageUrl,
            command.IsUnilateral,
            command.DifficultyId,
            command.KineticChainId.IsEmpty ? null : command.KineticChainId,
            command.ExerciseWeightTypeId.IsEmpty ? null : command.ExerciseWeightTypeId);
        
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

    private async Task<bool> IsNameUniqueAsync(string name, ExerciseId? excludeId)
    {
        using var readOnlyUow = _unitOfWorkProvider.CreateReadOnly();
        var repository = readOnlyUow.GetRepository<IExerciseRepository>();
        var exists = await repository.ExistsAsync(name, excludeId);
        return !exists; // Return true when name IS unique
    }

    private async Task<bool> HasValidExerciseTypesAsync(List<ExerciseTypeId> exerciseTypeIds)
    {
        // Empty list is valid
        var hasNoTypes = exerciseTypeIds.Count == 0;
        
        // Filter out empty IDs
        var validIds = exerciseTypeIds.Where(id => !id.IsEmpty).ToList();
        var allIdsAreEmpty = exerciseTypeIds.Count > 0 && validIds.Count == 0;
        
        // Check REST exercise business rule
        var isRestExercise = validIds.Count > 0 
            && await _exerciseTypeService.AnyIsRestTypeAsync(validIds.Select(id => id.ToString()).ToList());
        var restExerciseHasMultipleTypes = isRestExercise && validIds.Count > 1;
        
        // Single exit point with clear boolean logic
        return hasNoTypes || (!allIdsAreEmpty && !restExerciseHasMultipleTypes);
    }

    public async Task<ServiceResult<ExerciseDto>> UpdateAsync(ExerciseId id, UpdateExerciseCommand command)
    {
        return await ServiceValidate.Build<ExerciseDto>()
            .EnsureNotEmpty(id, ExerciseErrorMessages.InvalidIdFormat)
            .EnsureNotWhiteSpace(command.Name, ExerciseErrorMessages.ExerciseNameRequired)
            .EnsureNotWhiteSpace(command.Description, ExerciseErrorMessages.ExerciseDescriptionRequired)
            .EnsureNotEmpty(command.DifficultyId, ExerciseErrorMessages.DifficultyLevelRequired)
            .EnsureMaxLength(command.Name, 255, ExerciseErrorMessages.ExerciseNameMaxLength)
            .EnsureNameIsUniqueAsync(
                async () => await IsNameUniqueAsync(command.Name, id),
                "Exercise",
                command.Name)
            .EnsureHasValidAsync(
                async () => await HasValidExerciseTypesAsync(command.ExerciseTypeIds),
                ExerciseErrorMessages.InvalidExerciseTypeConfiguration)
            .EnsureHasValidAsync(
                async () => await ValidateKineticChainForExerciseTypesAsync(command.ExerciseTypeIds, command.KineticChainId),
                "REST exercises cannot have kinetic chain; Non-REST exercises must have kinetic chain")
            .EnsureHasValidAsync(
                async () => await ValidateWeightTypeForExerciseTypesAsync(command.ExerciseTypeIds, command.ExerciseWeightTypeId),
                "REST exercises cannot have weight type")
            .EnsureHasValidAsync(
                async () => await ValidateMuscleGroupsForExerciseTypesAsync(command.ExerciseTypeIds, command.MuscleGroups),
                "REST exercises cannot have muscle groups; Non-REST exercises must have at least one muscle group")
            .MatchAsync(
                whenValid: async () => await UpdateExerciseInternalAsync(id, command)
            );
    }

    private async Task<ServiceResult<ExerciseDto>> UpdateExerciseInternalAsync(ExerciseId id, UpdateExerciseCommand command)
    {
        using var writableUow = _unitOfWorkProvider.CreateWritable();
        var repository = writableUow.GetRepository<IExerciseRepository>();
        
        var exercise = await repository.GetByIdAsync(id);
        if (exercise.IsEmpty)
        {
            return ServiceResult<ExerciseDto>.Failure(ExerciseDto.Empty, ServiceError.NotFound("Exercise", id.ToString()));
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
            KineticChainId = command.KineticChainId.IsEmpty ? null : command.KineticChainId,
            ExerciseWeightTypeId = command.ExerciseWeightTypeId.IsEmpty ? null : command.ExerciseWeightTypeId,
            
            // UPDATE related collections instead of losing them:
            ExerciseExerciseTypes = MapToExerciseTypes(command.ExerciseTypeIds, id),
            ExerciseMuscleGroups = MapToMuscleGroups(command.MuscleGroups, id),
            CoachNotes = MapToCoachNotes(command.CoachNotes, id),
            ExerciseEquipment = MapToEquipment(command.EquipmentIds, id),
            ExerciseBodyParts = MapToBodyParts(command.BodyPartIds, id),
            ExerciseMovementPatterns = MapToMovementPatterns(command.MovementPatternIds, id)
        };
        
        await repository.UpdateAsync(updatedExercise);
        await writableUow.CommitAsync();
        
        // Reload the exercise with all navigation properties for proper mapping
        var reloadedExercise = await repository.GetByIdAsync(id);
        
        return ServiceResult<ExerciseDto>.Success(MapToExerciseDto(reloadedExercise));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(ExerciseId id)
    {
        return await ServiceValidate.Build<bool>()
            .EnsureNotEmpty(id, ExerciseErrorMessages.InvalidIdFormat)
            .EnsureAsync(
                async () => await ExerciseExistsAsync(id),
                ServiceError.NotFound("Exercise", id.ToString()))
            .MatchAsync(
                whenValid: async () => await DeleteExerciseInternalAsync(id),
                whenInvalid: errors => ServiceResult<bool>.Failure(false, errors.FirstOrDefault() ?? ServiceError.ValidationFailed("Unknown error"))
            );
    }

    private async Task<ServiceResult<bool>> DeleteExerciseInternalAsync(ExerciseId id)
    {
        using var writableUow = _unitOfWorkProvider.CreateWritable();
        var repository = writableUow.GetRepository<IExerciseRepository>();
        
        // For now, just soft delete (mark as inactive)
        // Use direct Entity Framework update to avoid relationship clearing
        await repository.SoftDeleteAsync(id);
        await writableUow.CommitAsync();
        
        return ServiceResult<bool>.Success(true);
    }
    
    private async Task<bool> ExerciseExistsAsync(ExerciseId id)
    {
        using var readOnlyUow = _unitOfWorkProvider.CreateReadOnly();
        var repository = readOnlyUow.GetRepository<IExerciseRepository>();
        var exercise = await repository.GetByIdAsync(id);
        return !exercise.IsEmpty;
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

    #region Validation Helper Methods

    private async Task<bool> ValidateKineticChainForExerciseTypesAsync(List<ExerciseTypeId> exerciseTypeIds, KineticChainTypeId kineticChainId)
    {
        // Filter out empty IDs
        var validIds = exerciseTypeIds.Where(id => !id.IsEmpty).ToList();
        
        // If no valid exercise types, validation passes
        if (validIds.Count == 0) return true;
        
        // Check if this is a REST exercise
        var isRestExercise = await _exerciseTypeService.AnyIsRestTypeAsync(validIds.Select(id => id.ToString()).ToList());
        var hasKineticChain = !kineticChainId.IsEmpty;
        
        if (isRestExercise)
        {
            // REST exercises cannot have kinetic chain
            return !hasKineticChain;
        }
        
        // Non-REST exercises must have kinetic chain
        return hasKineticChain;
    }

    private async Task<bool> ValidateWeightTypeForExerciseTypesAsync(List<ExerciseTypeId> exerciseTypeIds, ExerciseWeightTypeId weightTypeId)
    {
        // Filter out empty IDs
        var validIds = exerciseTypeIds.Where(id => !id.IsEmpty).ToList();
        
        // If no valid exercise types, validation passes
        if (validIds.Count == 0) return true;
        
        // Check if this is a REST exercise
        var isRestExercise = await _exerciseTypeService.AnyIsRestTypeAsync(validIds.Select(id => id.ToString()).ToList());
        var hasWeightType = !weightTypeId.IsEmpty;
        
        if (isRestExercise)
        {
            // REST exercises cannot have weight type
            return !hasWeightType;
        }
        
        // Non-REST exercises should have weight type (optional for now)
        return true; // Allow non-REST without weight type for flexibility
    }

    private async Task<bool> ValidateMuscleGroupsForExerciseTypesAsync(List<ExerciseTypeId> exerciseTypeIds, List<MuscleGroupAssignment> muscleGroups)
    {
        // Filter out empty IDs
        var validIds = exerciseTypeIds.Where(id => !id.IsEmpty).ToList();
        
        // If no valid exercise types, validation passes
        if (validIds.Count == 0) return true;
        
        // Check if this is a REST exercise
        var isRestExercise = await _exerciseTypeService.AnyIsRestTypeAsync(validIds.Select(id => id.ToString()).ToList());
        var hasMuscleGroups = muscleGroups.Count > 0;
        
        if (isRestExercise)
        {
            // REST exercises should not have muscle groups
            return !hasMuscleGroups;
        }
        
        // Non-REST exercises must have at least one muscle group
        return hasMuscleGroups;
    }

    #endregion


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

}