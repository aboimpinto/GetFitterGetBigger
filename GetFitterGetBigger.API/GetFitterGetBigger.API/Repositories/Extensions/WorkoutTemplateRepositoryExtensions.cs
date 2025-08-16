using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace GetFitterGetBigger.API.Repositories.Extensions;

/// <summary>
/// Extension methods for loading WorkoutTemplate navigation properties
/// </summary>
public static class WorkoutTemplateRepositoryExtensions
{
    /// <summary>
    /// Loads the Category navigation property for a workout template
    /// </summary>
    public static async Task LoadCategoryAsync(this EntityEntry<WorkoutTemplate> entry)
    {
        await entry
            .Reference(w => w.Category)
            .LoadAsync();
    }
    
    /// <summary>
    /// Loads the Difficulty navigation property for a workout template
    /// </summary>
    public static async Task LoadDifficultyAsync(this EntityEntry<WorkoutTemplate> entry)
    {
        await entry
            .Reference(w => w.Difficulty)
            .LoadAsync();
    }
    
    /// <summary>
    /// Loads the WorkoutState navigation property for a workout template
    /// </summary>
    public static async Task LoadWorkoutStateAsync(this EntityEntry<WorkoutTemplate> entry)
    {
        await entry
            .Reference(w => w.WorkoutState)
            .LoadAsync();
    }
    
    /// <summary>
    /// Loads the Objectives collection with WorkoutObjective navigation properties
    /// </summary>
    public static async Task LoadObjectivesAsync(this EntityEntry<WorkoutTemplate> entry)
    {
        await entry
            .Collection(w => w.Objectives)
            .Query()
            .Include(o => o.WorkoutObjective)
            .LoadAsync();
    }
    
    /// <summary>
    /// Loads the Exercises collection for a workout template
    /// </summary>
    public static async Task LoadExercisesAsync(this EntityEntry<WorkoutTemplate> entry)
    {
        // First load the collection if not loaded
        if (!entry.Collection(w => w.Exercises).IsLoaded)
        {
            await entry
                .Collection(w => w.Exercises)
                .LoadAsync();
        }
    }
    
    /// <summary>
    /// Loads the Exercise navigation property for each WorkoutTemplateExercise using the repository
    /// </summary>
    public static async Task LoadExerciseDetailsAsync(
        this EntityEntry<WorkoutTemplate> entry,
        IExerciseRepository exerciseRepository)
    {
        // Ensure exercises collection is loaded first
        if (!entry.Collection(w => w.Exercises).IsLoaded)
        {
            await entry.LoadExercisesAsync();
        }
        
        // Load each exercise using the repository
        foreach (var templateExercise in entry.Entity.Exercises)
        {
            if (!templateExercise.ExerciseId.IsEmpty)
            {
                var exercise = await exerciseRepository.GetByIdAsync(templateExercise.ExerciseId);
                
                // Use reflection or a writable property pattern to set the Exercise
                // Since Exercise is init-only, we need to update it via EF tracking
                await entry.Context.Entry(templateExercise)
                    .Reference(te => te.Exercise)
                    .LoadAsync();
            }
        }
    }
    
    /// <summary>
    /// Loads the Configurations collection for each WorkoutTemplateExercise
    /// </summary>
    public static async Task LoadExerciseConfigurationsAsync(this EntityEntry<WorkoutTemplate> entry)
    {
        // Ensure exercises collection is loaded first
        if (!entry.Collection(w => w.Exercises).IsLoaded)
        {
            await entry.LoadExercisesAsync();
        }
        
        // Load configurations for each exercise
        foreach (var templateExercise in entry.Entity.Exercises)
        {
            await entry.Context.Entry(templateExercise)
                .Collection(te => te.Configurations)
                .LoadAsync();
        }
    }
    
    /// <summary>
    /// Loads the Exercise navigation property with its nested properties for each WorkoutTemplateExercise
    /// </summary>
    public static async Task LoadExerciseDetailsWithNestedPropertiesAsync(
        this EntityEntry<WorkoutTemplate> entry,
        IExerciseRepository exerciseRepository)
    {
        // Ensure exercises collection is loaded first
        if (!entry.Collection(w => w.Exercises).IsLoaded)
        {
            await entry.LoadExercisesAsync();
        }
        
        // Load each exercise with full details using the repository
        // GetByIdAsync in ExerciseRepository already loads all nested properties
        foreach (var templateExercise in entry.Entity.Exercises)
        {
            if (!templateExercise.ExerciseId.IsEmpty)
            {
                var exercise = await exerciseRepository.GetByIdAsync(templateExercise.ExerciseId);
                
                // Since GetByIdAsync returns a fully loaded exercise with all navigation properties,
                // we need to attach it to the context properly
                if (!exercise.IsEmpty)
                {
                    // Detach any existing tracked instance
                    var tracked = entry.Context.ChangeTracker.Entries<Exercise>()
                        .FirstOrDefault(e => e.Entity.Id == exercise.Id);
                    
                    if (tracked != null)
                    {
                        tracked.State = EntityState.Detached;
                    }
                    
                    // Attach the fully loaded exercise
                    entry.Context.Attach(exercise);
                    
                    // Update the reference in WorkoutTemplateExercise
                    var wteEntry = entry.Context.Entry(templateExercise);
                    wteEntry.Reference(te => te.Exercise).CurrentValue = exercise;
                }
            }
        }
    }
}