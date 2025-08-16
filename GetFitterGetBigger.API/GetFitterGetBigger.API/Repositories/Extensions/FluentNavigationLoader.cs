using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace GetFitterGetBigger.API.Repositories.Extensions;

/// <summary>
/// Fluent API wrapper for loading navigation properties on WorkoutTemplate
/// </summary>
public class WorkoutTemplateNavigationLoader
{
    private readonly EntityEntry<WorkoutTemplate> _entry;
    private readonly List<Func<Task>> _loadOperations = new();

    public WorkoutTemplateNavigationLoader(EntityEntry<WorkoutTemplate> entry)
    {
        _entry = entry;
    }

    /// <summary>
    /// Includes the Category navigation property
    /// </summary>
    public WorkoutTemplateNavigationLoader IncludeCategory()
    {
        _loadOperations.Add(async () => await _entry.LoadCategoryAsync());
        return this;
    }

    /// <summary>
    /// Includes the Difficulty navigation property
    /// </summary>
    public WorkoutTemplateNavigationLoader IncludeDifficulty()
    {
        _loadOperations.Add(async () => await _entry.LoadDifficultyAsync());
        return this;
    }

    /// <summary>
    /// Includes the WorkoutState navigation property
    /// </summary>
    public WorkoutTemplateNavigationLoader IncludeWorkoutState()
    {
        _loadOperations.Add(async () => await _entry.LoadWorkoutStateAsync());
        return this;
    }

    /// <summary>
    /// Includes the Objectives collection with WorkoutObjective navigation properties
    /// </summary>
    public WorkoutTemplateNavigationLoader IncludeObjectives()
    {
        _loadOperations.Add(async () => await _entry.LoadObjectivesAsync());
        return this;
    }

    /// <summary>
    /// Includes the Exercises collection with optional nested properties
    /// </summary>
    /// <param name="exerciseRepository">Repository to load exercise details (optional, only needed if loadNestedProperties is true)</param>
    /// <param name="loadNestedProperties">If true, loads full exercise details with all navigation properties</param>
    public WorkoutTemplateNavigationLoader IncludeExercises(IExerciseRepository? exerciseRepository = null, bool loadNestedProperties = false)
    {
        _loadOperations.Add(async () =>
        {
            // First, load the exercises collection
            await _entry.LoadExercisesAsync();
            
            // If requested, load nested properties using the repository
            if (loadNestedProperties)
            {
                if (exerciseRepository == null)
                    throw new InvalidOperationException("ExerciseRepository is required when loading nested exercise properties");
                    
                await _entry.LoadExerciseDetailsAsync(exerciseRepository);
            }
            
            // Always load exercise configurations (they're part of the WorkoutTemplateExercise, not nested)
            await _entry.LoadExerciseConfigurationsAsync();
        });
        
        return this;
    }

    /// <summary>
    /// Executes all the included load operations
    /// </summary>
    public async Task<WorkoutTemplate> LoadAsync()
    {
        foreach (var operation in _loadOperations)
        {
            await operation();
        }
        return _entry.Entity;
    }
}

/// <summary>
/// Fluent API wrapper for loading navigation properties on Exercise
/// </summary>
public class ExerciseNavigationLoader
{
    private readonly EntityEntry<Exercise> _entry;
    private readonly List<Func<Task>> _loadOperations = new();

    public ExerciseNavigationLoader(EntityEntry<Exercise> entry)
    {
        _entry = entry;
    }

    /// <summary>
    /// Includes the Difficulty navigation property
    /// </summary>
    public ExerciseNavigationLoader IncludeDifficulty()
    {
        _loadOperations.Add(async () => await _entry.LoadDifficultyAsync());
        return this;
    }

    /// <summary>
    /// Includes the KineticChain navigation property if present
    /// </summary>
    public ExerciseNavigationLoader IncludeKineticChain()
    {
        _loadOperations.Add(async () => await _entry.LoadKineticChainAsync());
        return this;
    }

    /// <summary>
    /// Includes the ExerciseWeightType navigation property if present
    /// </summary>
    public ExerciseNavigationLoader IncludeExerciseWeightType()
    {
        _loadOperations.Add(async () => await _entry.LoadExerciseWeightTypeAsync());
        return this;
    }

    /// <summary>
    /// Includes the CoachNotes collection
    /// </summary>
    public ExerciseNavigationLoader IncludeCoachNotes()
    {
        _loadOperations.Add(async () => await _entry.LoadCoachNotesAsync());
        return this;
    }

    /// <summary>
    /// Includes the ExerciseTypes with their navigation properties
    /// </summary>
    public ExerciseNavigationLoader IncludeExerciseTypes()
    {
        _loadOperations.Add(async () => await _entry.LoadExerciseTypesAsync());
        return this;
    }

    /// <summary>
    /// Includes the MuscleGroups with their navigation properties
    /// </summary>
    public ExerciseNavigationLoader IncludeMuscleGroups()
    {
        _loadOperations.Add(async () => await _entry.LoadMuscleGroupsAsync());
        return this;
    }

    /// <summary>
    /// Includes the Equipment with their navigation properties
    /// </summary>
    public ExerciseNavigationLoader IncludeEquipment()
    {
        _loadOperations.Add(async () => await _entry.LoadEquipmentAsync());
        return this;
    }

    /// <summary>
    /// Includes the BodyParts with their navigation properties
    /// </summary>
    public ExerciseNavigationLoader IncludeBodyParts()
    {
        _loadOperations.Add(async () => await _entry.LoadBodyPartsAsync());
        return this;
    }

    /// <summary>
    /// Includes the MovementPatterns with their navigation properties
    /// </summary>
    public ExerciseNavigationLoader IncludeMovementPatterns()
    {
        _loadOperations.Add(async () => await _entry.LoadMovementPatternsAsync());
        return this;
    }

    /// <summary>
    /// Includes all navigation properties
    /// </summary>
    public ExerciseNavigationLoader IncludeAll()
    {
        return this
            .IncludeDifficulty()
            .IncludeKineticChain()
            .IncludeExerciseWeightType()
            .IncludeCoachNotes()
            .IncludeExerciseTypes()
            .IncludeMuscleGroups()
            .IncludeEquipment()
            .IncludeBodyParts()
            .IncludeMovementPatterns();
    }

    /// <summary>
    /// Executes all the included load operations
    /// </summary>
    public async Task<Exercise> LoadAsync()
    {
        foreach (var operation in _loadOperations)
        {
            await operation();
        }
        return _entry.Entity;
    }
}

/// <summary>
/// Extension methods to create fluent navigation loaders
/// </summary>
public static class NavigationLoaderExtensions
{
    /// <summary>
    /// Creates a fluent navigation loader for WorkoutTemplate
    /// </summary>
    public static WorkoutTemplateNavigationLoader LoadNavigation(this EntityEntry<WorkoutTemplate> entry)
    {
        return new WorkoutTemplateNavigationLoader(entry);
    }

    /// <summary>
    /// Creates a fluent navigation loader for Exercise
    /// </summary>
    public static ExerciseNavigationLoader LoadNavigation(this EntityEntry<Exercise> entry)
    {
        return new ExerciseNavigationLoader(entry);
    }
}