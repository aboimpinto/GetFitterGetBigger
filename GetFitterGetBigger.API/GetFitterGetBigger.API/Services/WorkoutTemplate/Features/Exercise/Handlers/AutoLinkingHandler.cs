using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.Enums;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.DataServices;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise.Handlers;

public class AutoLinkingHandler : IAutoLinkingHandler
{
    private readonly IExerciseLinkQueryDataService _exerciseLinkDataService;
    private readonly ILogger<AutoLinkingHandler> _logger;

    public AutoLinkingHandler(
        IExerciseLinkQueryDataService exerciseLinkDataService,
        ILogger<AutoLinkingHandler> logger)
    {
        _exerciseLinkDataService = exerciseLinkDataService;
        _logger = logger;
    }

    public async Task<List<WorkoutTemplateExercise>> AddAutoLinkedExercisesAsync(
        IWorkoutTemplateExerciseRepository repository,
        WorkoutTemplateId templateId,
        ExerciseId workoutExerciseId)
    {
        var addedExercises = new List<WorkoutTemplateExercise>();

        // Query linked exercises for warmup and cooldown
        var warmupLinks = await _exerciseLinkDataService.GetBySourceExerciseAsync(
            workoutExerciseId, 
            ExerciseLinkType.WARMUP.ToString());
        var cooldownLinks = await _exerciseLinkDataService.GetBySourceExerciseAsync(
            workoutExerciseId, 
            ExerciseLinkType.COOLDOWN.ToString());

        // Process warmup links
        if (warmupLinks.IsSuccess && warmupLinks.Data.Any())
        {
            foreach (var link in warmupLinks.Data)
            {
                var warmupExercise = await AddLinkedExerciseIfNotExistsAsync(
                    repository, 
                    templateId, 
                    ExerciseId.ParseOrEmpty(link.TargetExerciseId), 
                    "Warmup");
                if (warmupExercise != null)
                {
                    addedExercises.Add(warmupExercise);
                }
            }
        }

        // Process cooldown links
        if (cooldownLinks.IsSuccess && cooldownLinks.Data.Any())
        {
            foreach (var link in cooldownLinks.Data)
            {
                var cooldownExercise = await AddLinkedExerciseIfNotExistsAsync(
                    repository, 
                    templateId, 
                    ExerciseId.ParseOrEmpty(link.TargetExerciseId), 
                    "Cooldown");
                if (cooldownExercise != null)
                {
                    addedExercises.Add(cooldownExercise);
                }
            }
        }

        return addedExercises;
    }

    public async Task<List<WorkoutTemplateExercise>> FindOrphanedExercisesAsync(
        IWorkoutTemplateExerciseRepository repository,
        WorkoutTemplateId templateId,
        ExerciseId removedWorkoutExerciseId)
    {
        var orphanedExercises = new List<WorkoutTemplateExercise>();

        // Get all linked exercises for the removed workout exercise
        var warmupLinks = await _exerciseLinkDataService.GetBySourceExerciseAsync(
            removedWorkoutExerciseId, 
            ExerciseLinkType.WARMUP.ToString());
        var cooldownLinks = await _exerciseLinkDataService.GetBySourceExerciseAsync(
            removedWorkoutExerciseId, 
            ExerciseLinkType.COOLDOWN.ToString());

        // Check warmup links
        if (warmupLinks.IsSuccess && warmupLinks.Data.Any())
        {
            foreach (var link in warmupLinks.Data)
            {
                var isOrphaned = await IsExerciseOrphanedAsync(
                    repository, 
                    templateId, 
                    ExerciseId.ParseOrEmpty(link.TargetExerciseId), 
                    removedWorkoutExerciseId,
                    "Warmup");
                if (isOrphaned != null)
                {
                    orphanedExercises.Add(isOrphaned);
                }
            }
        }

        // Check cooldown links
        if (cooldownLinks.IsSuccess && cooldownLinks.Data.Any())
        {
            foreach (var link in cooldownLinks.Data)
            {
                var isOrphaned = await IsExerciseOrphanedAsync(
                    repository, 
                    templateId, 
                    ExerciseId.ParseOrEmpty(link.TargetExerciseId), 
                    removedWorkoutExerciseId,
                    "Cooldown");
                if (isOrphaned != null)
                {
                    orphanedExercises.Add(isOrphaned);
                }
            }
        }

        return orphanedExercises;
    }

    private async Task<WorkoutTemplateExercise?> AddLinkedExerciseIfNotExistsAsync(
        IWorkoutTemplateExerciseRepository repository,
        WorkoutTemplateId templateId,
        ExerciseId exerciseId,
        string phase)
    {
        // Map phase string to WorkoutZone enum
        var zone = phase.ToLower() switch
        {
            "warmup" => WorkoutZone.Warmup,
            "cooldown" => WorkoutZone.Cooldown,
            _ => WorkoutZone.Main
        };

        // Check if already exists in template for this zone
        var existingExercises = await repository.GetByWorkoutTemplateAsync(templateId);
        var alreadyExists = existingExercises.Any(e => e.ExerciseId == exerciseId && e.Zone == zone);
        if (alreadyExists)
        {
            return null;
        }

        // Get max sequence order for the zone
        var maxSequenceOrder = existingExercises
            .Where(e => e.Zone == zone)
            .Select(e => e.SequenceOrder)
            .DefaultIfEmpty(0)
            .Max();

        var autoLinkedExercise = WorkoutTemplateExercise.Handler.CreateNew(
            templateId,
            exerciseId,
            zone,
            maxSequenceOrder + 1,
            null); // No notes for auto-linked exercises

        if (autoLinkedExercise.IsSuccess)
        {
            await repository.AddAsync(autoLinkedExercise.Value);
            return autoLinkedExercise.Value;
        }

        return null;
    }

    private async Task<WorkoutTemplateExercise?> IsExerciseOrphanedAsync(
        IWorkoutTemplateExerciseRepository repository,
        WorkoutTemplateId templateId,
        ExerciseId targetExerciseId,
        ExerciseId removedWorkoutExerciseId,
        string targetPhase)
    {
        // Map phase to WorkoutZone
        var targetZone = targetPhase.ToLower() switch
        {
            "warmup" => WorkoutZone.Warmup,
            "cooldown" => WorkoutZone.Cooldown,
            _ => WorkoutZone.Main
        };

        // Get all workout exercises in the template
        var allExercises = await repository.GetByWorkoutTemplateAsync(templateId);
        
        // Get workout exercises (excluding the one being removed)
        var otherWorkoutExercises = allExercises
            .Where(e => e.Zone == WorkoutZone.Main && e.ExerciseId != removedWorkoutExerciseId)
            .ToList();

        // Check if any other workout exercise links to this target exercise
        foreach (var otherWorkout in otherWorkoutExercises)
        {
            var linkType = targetPhase.Equals("Warmup", StringComparison.OrdinalIgnoreCase) 
                ? ExerciseLinkType.WARMUP.ToString() 
                : ExerciseLinkType.COOLDOWN.ToString();
            
            var otherLinks = await _exerciseLinkDataService.GetBySourceExerciseAsync(
                otherWorkout.ExerciseId, 
                linkType);

            if (otherLinks.IsSuccess && otherLinks.Data.Any(l => l.TargetExerciseId == targetExerciseId.ToString()))
            {
                // This exercise is still needed by another workout exercise
                return null;
            }
        }

        // Find and return the orphaned exercise
        return allExercises.FirstOrDefault(e => e.ExerciseId == targetExerciseId && e.Zone == targetZone);
    }
}