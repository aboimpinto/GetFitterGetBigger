using GetFitterGetBigger.API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace GetFitterGetBigger.API.Repositories.Extensions;

/// <summary>
/// Extension methods for loading Exercise navigation properties
/// </summary>
public static class ExerciseRepositoryExtensions
{
    /// <summary>
    /// Loads the Difficulty navigation property for an exercise
    /// </summary>
    public static async Task LoadDifficultyAsync(this EntityEntry<Exercise> entry)
    {
        await entry
            .Reference(e => e.Difficulty)
            .LoadAsync();
    }
    
    /// <summary>
    /// Loads the KineticChain navigation property for an exercise if present
    /// </summary>
    public static async Task LoadKineticChainAsync(this EntityEntry<Exercise> entry)
    {
        if (entry.Entity.KineticChainId.HasValue)
        {
            await entry
                .Reference(e => e.KineticChain)
                .LoadAsync();
        }
    }
    
    /// <summary>
    /// Loads the ExerciseWeightType navigation property for an exercise if present
    /// </summary>
    public static async Task LoadExerciseWeightTypeAsync(this EntityEntry<Exercise> entry)
    {
        if (entry.Entity.ExerciseWeightTypeId.HasValue)
        {
            await entry
                .Reference(e => e.ExerciseWeightType)
                .LoadAsync();
        }
    }
    
    /// <summary>
    /// Loads the CoachNotes collection for an exercise
    /// </summary>
    public static async Task LoadCoachNotesAsync(this EntityEntry<Exercise> entry)
    {
        await entry
            .Collection(e => e.CoachNotes)
            .LoadAsync();
    }
    
    /// <summary>
    /// Loads the ExerciseExerciseTypes collection with ExerciseType navigation properties
    /// </summary>
    public static async Task LoadExerciseTypesAsync(this EntityEntry<Exercise> entry)
    {
        // First load the collection if not loaded
        if (!entry.Collection(e => e.ExerciseExerciseTypes).IsLoaded)
        {
            await entry
                .Collection(e => e.ExerciseExerciseTypes)
                .LoadAsync();
        }
        
        // Then load the ExerciseType for each item
        foreach (var eet in entry.Entity.ExerciseExerciseTypes)
        {
            await entry.Context.Entry(eet)
                .Reference(x => x.ExerciseType)
                .LoadAsync();
        }
    }
    
    /// <summary>
    /// Loads the ExerciseMuscleGroups collection with MuscleGroup and MuscleRole navigation properties
    /// </summary>
    public static async Task LoadMuscleGroupsAsync(this EntityEntry<Exercise> entry)
    {
        await entry
            .Collection(e => e.ExerciseMuscleGroups)
            .Query()
            .Include(emg => emg.MuscleGroup)
            .Include(emg => emg.MuscleRole)
            .LoadAsync();
    }
    
    /// <summary>
    /// Loads the ExerciseEquipment collection with Equipment navigation properties
    /// </summary>
    public static async Task LoadEquipmentAsync(this EntityEntry<Exercise> entry)
    {
        await entry
            .Collection(e => e.ExerciseEquipment)
            .Query()
            .Include(ee => ee.Equipment)
            .LoadAsync();
    }
    
    /// <summary>
    /// Loads the ExerciseBodyParts collection with BodyPart navigation properties
    /// </summary>
    public static async Task LoadBodyPartsAsync(this EntityEntry<Exercise> entry)
    {
        await entry
            .Collection(e => e.ExerciseBodyParts)
            .Query()
            .Include(ebp => ebp.BodyPart)
            .LoadAsync();
    }
    
    /// <summary>
    /// Loads the ExerciseMovementPatterns collection with MovementPattern navigation properties
    /// </summary>
    public static async Task LoadMovementPatternsAsync(this EntityEntry<Exercise> entry)
    {
        await entry
            .Collection(e => e.ExerciseMovementPatterns)
            .Query()
            .Include(emp => emp.MovementPattern)
            .LoadAsync();
    }
    
    /// <summary>
    /// Loads all navigation properties for an exercise
    /// </summary>
    public static async Task LoadAllNavigationPropertiesAsync(this EntityEntry<Exercise> entry)
    {
        // Load reference properties
        await entry.LoadDifficultyAsync();
        await entry.LoadKineticChainAsync();
        await entry.LoadExerciseWeightTypeAsync();
        
        // Load collections
        await entry.LoadCoachNotesAsync();
        await entry.LoadExerciseTypesAsync();
        await entry.LoadMuscleGroupsAsync();
        await entry.LoadEquipmentAsync();
        await entry.LoadBodyPartsAsync();
        await entry.LoadMovementPatternsAsync();
    }
}