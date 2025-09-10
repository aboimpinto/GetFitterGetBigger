using GetFitterGetBigger.API.Constants.ErrorMessages;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise.Extensions;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise.Handlers;

/// <summary>
/// Handler for enhanced workout template exercise operations with phase/round support
/// </summary>
public class EnhancedMethodsHandler : IEnhancedMethodsHandler
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly IAutoLinkingHandler _autoLinkingHandler;
    private readonly IReorderExerciseHandler _reorderExerciseHandler;
    private readonly IValidationHandler _validationHandler;
    private readonly ILogger<EnhancedMethodsHandler> _logger;

    public EnhancedMethodsHandler(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        IAutoLinkingHandler autoLinkingHandler,
        IReorderExerciseHandler reorderExerciseHandler,
        IValidationHandler validationHandler,
        ILogger<EnhancedMethodsHandler> logger)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _autoLinkingHandler = autoLinkingHandler;
        _reorderExerciseHandler = reorderExerciseHandler;
        _validationHandler = validationHandler;
        _logger = logger;
    }

    /// <summary>
    /// Adds an exercise to a workout template with auto-linking support
    /// Restored and adapted from LegacyMethodsHandler - migrated from Command to DTO pattern
    /// </summary>
    public async Task<ServiceResult<AddExerciseResultDto>> AddExerciseAsync(WorkoutTemplateId templateId, AddExerciseDto dto)
    {
        return await ServiceValidate.BuildTransactional<FitnessDbContext, AddExerciseResultDto>(_unitOfWorkProvider)
            // External validations
            .EnsureNotEmpty(templateId, WorkoutTemplateExerciseErrorMessages.InvalidWorkoutTemplateId)
            .EnsureNotEmpty(dto.ExerciseId, WorkoutTemplateExerciseErrorMessages.InvalidExerciseId)
            .EnsureNotWhiteSpace(dto.Phase, WorkoutTemplateExerciseErrorMessages.InvalidZone)
            .Ensure(() => dto.RoundNumber >= 1, "Round number must be at least 1")
            .EnsureNotWhiteSpace(dto.Metadata, "Metadata is required for exercise configuration")
            // Validate phase is valid (Warmup, Workout, Cooldown)
            .EnsureAsync(async () => await IsValidPhaseAsync(dto.Phase), 
                string.Format(WorkoutTemplateExerciseErrorMessages.InvalidZoneWarmupMainCooldown, dto.Phase))
            // Template validations
            .EnsureAsyncChained(async () => await _validationHandler.DoesTemplateExistAsync(templateId), 
                ServiceError.NotFound(WorkoutTemplateExerciseErrorMessages.WorkoutTemplateNotFound))
            .EnsureAsyncChained(async () => await _validationHandler.IsTemplateInDraftStateAsync(templateId), 
                WorkoutTemplateExerciseErrorMessages.CanOnlyAddExercisesToDraftTemplates)
            // Exercise validation
            .EnsureAsyncChained(async () => await _validationHandler.IsExerciseActiveAsync(dto.ExerciseId), 
                ServiceError.NotFound(WorkoutTemplateExerciseErrorMessages.ExerciseNotFound))
            // Create writable repository
            .ThenCreateWritableRepositoryChained<AddExerciseResultDto, IWorkoutTemplateExerciseRepository>()
            // Calculate order and create entity
            .ThenLoadAsyncChained<AddExerciseResultDto, WorkoutTemplateExercise>("NewExercise", async context =>
            {
                var repository = context.GetRepository<IWorkoutTemplateExerciseRepository>(isReadOnly: false);
                
                // Map Phase to Zone for backward compatibility
                var zone = MapPhaseToZone(dto.Phase);
                
                // Calculate order in round - for now using SequenceOrder until full phase/round migration
                var maxSequence = await repository.GetMaxSequenceOrderAsync(templateId, zone);
                var sequenceOrder = maxSequence + 1;
                
                // Create the exercise with metadata instead of notes
                var createResult = WorkoutTemplateExercise.Handler.CreateNew(
                    templateId, 
                    dto.ExerciseId, 
                    zone, 
                    sequenceOrder, 
                    dto.Metadata); // Using metadata instead of notes
                    
                return createResult.IsSuccess ? createResult.Value : WorkoutTemplateExercise.Empty;
            })
            // Ensure entity was created successfully
            .ThenEnsureAsyncChained(
                async context => await Task.FromResult(!context.Get<WorkoutTemplateExercise>("NewExercise").IsEmpty),
                "Failed to create workout template exercise")
            // Add to repository
            .ThenPerformAsyncChained(async context =>
            {
                var repository = context.GetRepository<IWorkoutTemplateExerciseRepository>(isReadOnly: false);
                var newExercise = context.Get<WorkoutTemplateExercise>("NewExercise");
                await repository.AddAsync(newExercise);
                context.Store("CreatedId", newExercise.Id);
                context.Store("AddedExercises", new List<WorkoutTemplateExercise> { newExercise });
            })
            // Handle auto-linking for Workout phase exercises
            .ThenPerformIfAsyncChained(
                context => 
                {
                    var newExercise = context.Get<WorkoutTemplateExercise>("NewExercise");
                    return newExercise.Zone == WorkoutZone.Main; // "Workout" phase maps to Main zone
                },
                async context =>
                {
                    var repository = context.GetRepository<IWorkoutTemplateExerciseRepository>(isReadOnly: false);
                    var newExercise = context.Get<WorkoutTemplateExercise>("NewExercise");
                    var linkedExercises = await _autoLinkingHandler.AddAutoLinkedExercisesAsync(
                        repository, 
                        newExercise.WorkoutTemplateId, 
                        newExercise.ExerciseId);
                    
                    // Add linked exercises to the list
                    var allAdded = context.Get<List<WorkoutTemplateExercise>>("AddedExercises");
                    allAdded.AddRange(linkedExercises);
                    context.Store("AddedExercises", allAdded);
                })
            // Return result with all added exercises
            .ThenExecuteAsyncChained(async context =>
            {
                var repository = context.GetRepository<IWorkoutTemplateExerciseRepository>(isReadOnly: false);
                var addedExercises = context.Get<List<WorkoutTemplateExercise>>("AddedExercises");
                
                // Reload all exercises with details for proper DTOs
                var detailedExercises = new List<WorkoutTemplateExerciseDto>();
                foreach (var exercise in addedExercises)
                {
                    var detailed = await repository.GetByIdWithDetailsAsync(exercise.Id);
                    detailedExercises.Add(detailed.ToDto());
                }
                
                return new AddExerciseResultDto(
                    detailedExercises,
                    $"Successfully added {detailedExercises.Count} exercise(s) to the template");
            });
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
    /// Removes an exercise from a workout template with orphan cleanup support
    /// Restored and adapted from LegacyMethodsHandler
    /// </summary>
    public async Task<ServiceResult<RemoveExerciseResultDto>> RemoveExerciseAsync(WorkoutTemplateId templateId, WorkoutTemplateExerciseId exerciseId)
    {
        return await ServiceValidate.BuildTransactional<FitnessDbContext, RemoveExerciseResultDto>(_unitOfWorkProvider)
            .EnsureNotEmpty(templateId, WorkoutTemplateExerciseErrorMessages.InvalidWorkoutTemplateId)
            .EnsureNotEmpty(exerciseId, WorkoutTemplateExerciseErrorMessages.InvalidExerciseId)
            // Validate exercise exists first (async operation breaks the chain)
            .EnsureAsync(
                async () => await Task.FromResult(true), // Dummy async to start chain
                "Starting removal process")
            // Now we can use Chained methods
            .ThenCreateWritableRepositoryChained<RemoveExerciseResultDto, IWorkoutTemplateExerciseRepository>()
            // Load the exercise to remove
            .ThenLoadAsyncChained<RemoveExerciseResultDto, WorkoutTemplateExercise>("ExerciseToRemove", async context =>
            {
                var repository = context.GetRepository<IWorkoutTemplateExerciseRepository>(isReadOnly: false);
                return await repository.GetByIdWithDetailsAsync(exerciseId);
            })
            // Ensure exercise exists
            .ThenEnsureAsyncChained(
                async context => await Task.FromResult(!context.Get<WorkoutTemplateExercise>("ExerciseToRemove").IsEmpty),
                ServiceError.NotFound(WorkoutTemplateExerciseErrorMessages.TemplateExerciseNotFound))
            // Ensure template is in draft state
            .ThenEnsureAsyncChained(
                async context => 
                {
                    var exercise = context.Get<WorkoutTemplateExercise>("ExerciseToRemove");
                    return await _validationHandler.IsTemplateInDraftStateAsync(exercise.WorkoutTemplateId);
                },
                WorkoutTemplateExerciseErrorMessages.CanOnlyRemoveExercisesFromDraftTemplates)
            // Check if exercise belongs to the template
            .ThenEnsureAsyncChained(
                async context =>
                {
                    var exercise = context.Get<WorkoutTemplateExercise>("ExerciseToRemove");
                    return await Task.FromResult(exercise.WorkoutTemplateId == templateId);
                },
                "Exercise does not belong to the specified template")
            // Handle orphaned exercises for Main zone exercises
            .ThenPerformAsyncChained(async context =>
            {
                var repository = context.GetRepository<IWorkoutTemplateExerciseRepository>(isReadOnly: false);
                var exerciseToRemove = context.Get<WorkoutTemplateExercise>("ExerciseToRemove");
                var removedExercises = new List<WorkoutTemplateExercise> { exerciseToRemove };
                
                // If it's a Main (Workout) exercise, find and remove orphaned warmup/cooldown exercises
                if (exerciseToRemove.Zone == WorkoutZone.Main)
                {
                    var orphanedExercises = await _autoLinkingHandler.FindOrphanedExercisesAsync(
                        repository, 
                        exerciseToRemove.WorkoutTemplateId, 
                        exerciseToRemove.ExerciseId);
                    
                    // Remove orphaned exercises
                    foreach (var orphaned in orphanedExercises)
                    {
                        await repository.DeleteAsync(orphaned.Id);
                        removedExercises.Add(orphaned);
                    }
                }
                
                // Remove the main exercise
                await repository.DeleteAsync(exerciseId);
                
                // Reorder remaining exercises
                await _reorderExerciseHandler.ReorderAfterRemovalAsync(
                    repository, 
                    exerciseToRemove.WorkoutTemplateId, 
                    removedExercises);
                
                context.Store("RemovedExercises", removedExercises);
            })
            // Return result
            .ThenExecuteAsyncChained(async context =>
            {
                var removedExercises = context.Get<List<WorkoutTemplateExercise>>("RemovedExercises");
                var removedDtos = removedExercises.Select(e => e.ToDto()).ToList();
                
                return await Task.FromResult(new RemoveExerciseResultDto(
                    removedDtos,
                    $"Successfully removed {removedDtos.Count} exercise(s) from the template"));
            });
    }

    /// <summary>
    /// Reorders an exercise within its round
    /// </summary>
    public async Task<ServiceResult<ReorderResultDto>> ReorderExerciseAsync(
        WorkoutTemplateId templateId, 
        WorkoutTemplateExerciseId exerciseId, 
        int newOrderInRound)
    {
        // Implementation will be added after verifying the basic operations work
        return await Task.FromResult(ServiceResult<ReorderResultDto>.Failure(
            ReorderResultDto.Empty,
            ServiceError.ValidationFailed("ReorderExerciseAsync will be implemented after core operations are verified")));
    }

    /// <summary>
    /// Copies all exercises from one round to another
    /// </summary>
    public async Task<ServiceResult<CopyRoundResultDto>> CopyRoundAsync(
        WorkoutTemplateId templateId, 
        CopyRoundDto dto)
    {
        // Implementation will be added after verifying the basic operations work
        return await Task.FromResult(ServiceResult<CopyRoundResultDto>.Failure(
            CopyRoundResultDto.Empty,
            ServiceError.ValidationFailed("CopyRoundAsync will be implemented after core operations are verified")));
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

    /// <summary>
    /// Maps Phase string to WorkoutZone enum for backward compatibility
    /// </summary>
    private static WorkoutZone MapPhaseToZone(string phase)
    {
        return phase?.ToLower() switch
        {
            "warmup" => WorkoutZone.Warmup,
            "workout" => WorkoutZone.Main,
            "main" => WorkoutZone.Main,
            "cooldown" => WorkoutZone.Cooldown,
            _ => WorkoutZone.Main // Default to Main if unknown
        };
    }

    /// <summary>
    /// Validates if the phase string is valid
    /// </summary>
    private static async Task<bool> IsValidPhaseAsync(string phase)
    {
        await Task.CompletedTask; // Make async for consistency with validation pattern
        var validPhases = new[] { "warmup", "workout", "main", "cooldown" };
        return !string.IsNullOrWhiteSpace(phase) && 
               validPhases.Contains(phase.ToLower());
    }
}