using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands;
using GetFitterGetBigger.API.Services.Exercise.DataServices;
using GetFitterGetBigger.API.Services.Exercise.Handlers;
using GetFitterGetBigger.API.Services.Implementations.Extensions;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.Services.Exercise.Extensions;

namespace GetFitterGetBigger.API.Services.Exercise;

/// <summary>
/// Service for Exercise business logic using clean architecture command pattern.
/// Delegates data access to DataServices and complex logic to Handlers.
/// </summary>
public class ExerciseService(
    IExerciseQueryDataService queryDataService,
    IExerciseCommandDataService commandDataService,
    IExerciseTypeService exerciseTypeService) : IExerciseService
{
    private readonly IExerciseQueryDataService _queryDataService = queryDataService;
    private readonly IExerciseCommandDataService _commandDataService = commandDataService;
    private readonly ExerciseTypeValidationHandler _typeValidationHandler = new(exerciseTypeService);
    private readonly DeleteHandler _deleteHandler = new(commandDataService);
    
    public async Task<ServiceResult<PagedResponse<ExerciseDto>>> GetPagedAsync(GetExercisesCommand filterParams)
    {
        return await _queryDataService.GetPagedAsync(filterParams);
    }
    
    public async Task<ServiceResult<ExerciseDto>> GetByIdAsync(ExerciseId id)
    {
        return await ServiceValidate.Build<ExerciseDto>()
            .EnsureNotEmpty(id, ExerciseErrorMessages.InvalidIdFormat)
            .EnsureAsync(
                async () => (await _queryDataService.ExistsAsync(id)).Data.Value,
                ServiceError.NotFound("Exercise", id.ToString()))
            .MatchAsync(
                whenValid: async () => await _queryDataService.GetByIdAsync(id));
    }
    
    public async Task<ServiceResult<ExerciseDto>> CreateAsync(CreateExerciseCommand command)
    {
        return await ServiceValidate.Build<ExerciseDto>()
            .EnsureNotWhiteSpace(command.Name, ExerciseErrorMessages.ExerciseNameRequired)
            .EnsureNotWhiteSpace(command.Description, ExerciseErrorMessages.ExerciseDescriptionRequired)
            .EnsureNotEmpty(command.DifficultyId, ExerciseErrorMessages.DifficultyLevelRequired)
            .EnsureMaxLength(command.Name, 255, ExerciseErrorMessages.ExerciseNameMaxLength)
            .EnsureNameIsUniqueAsync(
                async () => await _queryDataService.IsExerciseNameUniqueAsync(command.Name),
                "Exercise",
                command.Name)
            .EnsureHasValidAsync(
                async () => await _typeValidationHandler.AreExerciseTypesValidAsync(command.ExerciseTypeIds),
                ExerciseErrorMessages.InvalidExerciseTypeConfiguration)
            .EnsureHasValidAsync(
                async () => await _typeValidationHandler.IsKineticChainValidAsync(
                    command.ExerciseTypeIds, command.KineticChainId),
                ExerciseErrorMessages.InvalidKineticChainForExerciseType)
            .EnsureHasValidAsync(
                async () => await _typeValidationHandler.IsWeightTypeValidAsync(
                    command.ExerciseTypeIds, command.ExerciseWeightTypeId),
                ExerciseErrorMessages.InvalidWeightTypeForExerciseType)
            .EnsureHasValidAsync(
                async () => await _typeValidationHandler.AreMuscleGroupsValidAsync(
                    command.ExerciseTypeIds, command.MuscleGroups),
                ExerciseErrorMessages.InvalidMuscleGroupsForExerciseType)
            .MatchAsync(
                whenValid: async () => await CreateExerciseInternalAsync(command)
            );
    }
    
    private async Task<ServiceResult<ExerciseDto>> CreateExerciseInternalAsync(CreateExerciseCommand command)
    {
        // Create exercise entity with all data
        var exercise = Models.Entities.Exercise.Handler.CreateNew(
            command.Name,
            command.Description,
            command.VideoUrl,
            command.ImageUrl,
            command.IsUnilateral,
            command.DifficultyId,
            command.KineticChainId.IsEmpty ? null : command.KineticChainId,
            command.ExerciseWeightTypeId.IsEmpty ? null : command.ExerciseWeightTypeId);
        
        // Add all related data
        var exerciseWithRelations = exercise with {
            ExerciseExerciseTypes = command.ExerciseTypeIds.ToExerciseTypes(exercise.Id),
            ExerciseMuscleGroups = command.MuscleGroups.ToExerciseMuscleGroups(exercise.Id),
            CoachNotes = command.CoachNotes.ToCoachNotes(exercise.Id),
            ExerciseEquipment = command.EquipmentIds.ToExerciseEquipment(exercise.Id),
            ExerciseBodyParts = command.BodyPartIds.ToExerciseBodyParts(exercise.Id),
            ExerciseMovementPatterns = command.MovementPatternIds.ToExerciseMovementPatterns(exercise.Id)
        };
        
        return await _commandDataService.CreateAsync(exerciseWithRelations);
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
                async () => await _queryDataService.IsExerciseNameUniqueAsync(command.Name, id),
                "Exercise",
                command.Name)
            .EnsureHasValidAsync(
                async () => await _typeValidationHandler.AreExerciseTypesValidAsync(command.ExerciseTypeIds),
                ExerciseErrorMessages.InvalidExerciseTypeConfiguration)
            .EnsureHasValidAsync(
                async () => await _typeValidationHandler.IsKineticChainValidAsync(
                    command.ExerciseTypeIds, command.KineticChainId),
                ExerciseErrorMessages.InvalidKineticChainForExerciseType)
            .EnsureHasValidAsync(
                async () => await _typeValidationHandler.IsWeightTypeValidAsync(
                    command.ExerciseTypeIds, command.ExerciseWeightTypeId),
                ExerciseErrorMessages.InvalidWeightTypeForExerciseType)
            .EnsureHasValidAsync(
                async () => await _typeValidationHandler.AreMuscleGroupsValidAsync(
                    command.ExerciseTypeIds, command.MuscleGroups),
                ExerciseErrorMessages.InvalidMuscleGroupsForExerciseType)
            .MatchAsync(
                whenValid: async () => await UpdateExerciseInternalAsync(id, command)
            );
    }
    
    private async Task<ServiceResult<ExerciseDto>> UpdateExerciseInternalAsync(ExerciseId id, UpdateExerciseCommand command)
    {
        return await _commandDataService.UpdateAsync(id, exercise =>
        {
            // Update the exercise with new values
            return exercise with {
                Name = command.Name,
                Description = command.Description,
                VideoUrl = command.VideoUrl,
                ImageUrl = command.ImageUrl,
                IsUnilateral = command.IsUnilateral,
                IsActive = command.IsActive,
                DifficultyId = command.DifficultyId,
                KineticChainId = command.KineticChainId.IsEmpty ? null : command.KineticChainId,
                ExerciseWeightTypeId = command.ExerciseWeightTypeId.IsEmpty ? null : command.ExerciseWeightTypeId,
                
                // Update related collections
                ExerciseExerciseTypes = command.ExerciseTypeIds.ToExerciseTypes(id),
                ExerciseMuscleGroups = command.MuscleGroups.ToExerciseMuscleGroups(id),
                CoachNotes = command.CoachNotes.ToCoachNotes(id),
                ExerciseEquipment = command.EquipmentIds.ToExerciseEquipment(id),
                ExerciseBodyParts = command.BodyPartIds.ToExerciseBodyParts(id),
                ExerciseMovementPatterns = command.MovementPatternIds.ToExerciseMovementPatterns(id)
            };
        });
    }
    
    public async Task<ServiceResult<bool>> DeleteAsync(ExerciseId id)
    {
        return await ServiceValidate.Build<bool>()
            .EnsureNotEmpty(id, ExerciseErrorMessages.InvalidIdFormat)
            .EnsureAsync(
                async () => (await _queryDataService.ExistsAsync(id)).Data.Value,
                ServiceError.NotFound("Exercise", id.ToString()))
            .MatchAsync(
                whenValid: async () => await _deleteHandler.DeleteAsync(id),
                whenInvalid: errors => ServiceResult<bool>.Failure(false, errors.FirstOrDefault() ?? ServiceError.ValidationFailed("Unknown error")));
    }
}