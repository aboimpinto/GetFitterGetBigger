using GetFitterGetBigger.API.Constants.ErrorMessages;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise.Extensions;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise.Handlers;

/// <summary>
/// Handler for enhanced workout template exercise operations with phase/round support
/// </summary>
public class EnhancedMethodsHandler : IEnhancedMethodsHandler
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly ILogger<EnhancedMethodsHandler> _logger;

    public EnhancedMethodsHandler(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ILogger<EnhancedMethodsHandler> logger)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<UpdateMetadataResultDto>> UpdateExerciseMetadataAsync(WorkoutTemplateId templateId, WorkoutTemplateExerciseId exerciseId, string metadata)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();

        // Get the exercise
        var exercise = await repository.GetByIdWithDetailsAsync(exerciseId);
        if (exercise.IsEmpty)
        {
            return ServiceResult<UpdateMetadataResultDto>.Failure(
                UpdateMetadataResultDto.Empty,
                ServiceError.NotFound(WorkoutTemplateExerciseErrorMessages.TemplateExerciseNotFound));
        }

        // For now, we'll store the metadata in the Notes field since the current entity doesn't have a Metadata field
        // This is a transitional approach until the entity is migrated to phase/round structure
        var updatedExercise = WorkoutTemplateExercise.Handler.UpdateNotes(exercise, metadata);
        if (!updatedExercise.IsSuccess)
        {
            return ServiceResult<UpdateMetadataResultDto>.Failure(
                UpdateMetadataResultDto.Empty,
                ServiceError.ValidationFailed("Failed to update exercise metadata"));
        }

        await repository.UpdateAsync(updatedExercise.Value);
        await unitOfWork.CommitAsync();

        // Reload to get fresh data
        var reloadedExercise = await repository.GetByIdWithDetailsAsync(exerciseId);
        var resultDto = new UpdateMetadataResultDto(
            reloadedExercise.ToDto(),
            "Exercise metadata updated successfully");

        return ServiceResult<UpdateMetadataResultDto>.Success(resultDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<BooleanResultDto>> ValidateExerciseMetadataAsync(ExerciseId exerciseId, ExecutionProtocolId protocolId, string metadata)
    {
        // Basic JSON validation - more sophisticated validation can be added later
        try
        {
            System.Text.Json.JsonDocument.Parse(metadata);
            return await Task.FromResult(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true)));
        }
        catch (System.Text.Json.JsonException)
        {
            return await Task.FromResult(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(false)));
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult<WorkoutTemplateExercisesDto>> GetTemplateExercisesAsync(WorkoutTemplateId templateId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
        var templateRepository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();

        // Get template details for metadata
        var template = await templateRepository.GetByIdAsync(templateId);
        if (template.IsEmpty)
        {
            return ServiceResult<WorkoutTemplateExercisesDto>.Failure(
                WorkoutTemplateExercisesDto.Empty,
                ServiceError.NotFound(WorkoutTemplateExerciseErrorMessages.WorkoutTemplateNotFound));
        }

        // Get all exercises for the template
        var exercises = (await repository.GetByWorkoutTemplateAsync(templateId)).ToList();

        // For now, map current Zone-based system to Phase/Round structure
        // This is a transitional approach until entity migration
        var warmupPhase = new WorkoutPhaseDto(
            new List<RoundDto> { new(1, MapExercisesByZone(exercises, WorkoutZone.Warmup)) });
            
        var workoutPhase = new WorkoutPhaseDto(
            new List<RoundDto> { new(1, MapExercisesByZone(exercises, WorkoutZone.Main)) });
            
        var cooldownPhase = new WorkoutPhaseDto(
            new List<RoundDto> { new(1, MapExercisesByZone(exercises, WorkoutZone.Cooldown)) });

        var result = new WorkoutTemplateExercisesDto(
            templateId,
            template.Name,
            ExecutionProtocolDto.Empty, // Will need to be properly mapped when ExecutionProtocol integration is complete
            warmupPhase,
            workoutPhase,
            cooldownPhase);

        return ServiceResult<WorkoutTemplateExercisesDto>.Success(result);
    }
    
    /// <summary>
    /// Maps exercises by zone to the new DTO structure
    /// </summary>
    private static List<WorkoutTemplateExerciseDto> MapExercisesByZone(List<WorkoutTemplateExercise> exercises, WorkoutZone zone)
    {
        return exercises
            .Where(e => e.Zone == zone)
            .OrderBy(e => e.SequenceOrder)
            .Select(e => e.ToDto())
            .ToList();
    }
}