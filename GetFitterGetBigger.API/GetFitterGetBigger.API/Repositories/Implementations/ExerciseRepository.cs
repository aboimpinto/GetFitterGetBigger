using GetFitterGetBigger.API.Extensions;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Extensions;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Exercise.Extensions;
using Microsoft.EntityFrameworkCore;

namespace GetFitterGetBigger.API.Repositories.Implementations;

/// <summary>
/// Repository implementation for Exercise data with advanced querying capabilities
/// </summary>
public class ExerciseRepository : DomainRepository<Exercise, ExerciseId, FitnessDbContext>, IExerciseRepository
{
    /// <summary>
    /// Gets a paginated list of exercises with optional filtering
    /// </summary>
    public async Task<(IEnumerable<Exercise> exercises, int totalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string name,
        DifficultyLevelId difficultyId,
        IEnumerable<MuscleGroupId> muscleGroupIds,
        IEnumerable<EquipmentId> equipmentIds,
        IEnumerable<MovementPatternId> movementPatternIds,
        IEnumerable<BodyPartId> bodyPartIds,
        bool includeInactive = false)
    {
        // Build query with explicit filters - each filter is visible
        var query = Context.Exercises
            .FilterByActiveStatus(includeInactive)
            .FilterByNamePattern(name)
            .FilterByDifficulty(difficultyId)
            .FilterByMuscleGroups(muscleGroupIds)
            .FilterByEquipment(equipmentIds)
            .FilterByMovementPatterns(movementPatternIds)
            .FilterByBodyParts(bodyPartIds)
            .OrderBy(e => e.Name);
        
        // Get total count before pagination
        var totalCount = await query.CountAsync();
        
        // Apply pagination and explicit includes
        var exercises = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(e => e.Difficulty)
            .Include(e => e.KineticChain)
            .Include(e => e.ExerciseWeightType)
            .Include(e => e.CoachNotes)
            .Include(e => e.ExerciseExerciseTypes)
                .ThenInclude(eet => eet.ExerciseType)
            .Include(e => e.ExerciseMuscleGroups)
                .ThenInclude(emg => emg.MuscleGroup)
            .Include(e => e.ExerciseMuscleGroups)
                .ThenInclude(emg => emg.MuscleRole)
            .Include(e => e.ExerciseEquipment)
                .ThenInclude(ee => ee.Equipment)
            .Include(e => e.ExerciseMovementPatterns)
                .ThenInclude(emp => emp.MovementPattern)
            .Include(e => e.ExerciseBodyParts)
                .ThenInclude(ebp => ebp.BodyPart)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync();
        
        return (exercises, totalCount);
    }
    
    /// <summary>
    /// Gets an exercise by its ID with all related data
    /// </summary>
    public override async Task<Exercise> GetByIdAsync(ExerciseId id)
    {
        var exercise = await Context.Exercises
            .FilterByActiveStatus(includeInactive: false)
            .Where(e => e.Id == id)
            .Include(e => e.Difficulty)
            .Include(e => e.KineticChain)
            .Include(e => e.ExerciseWeightType)
            .Include(e => e.CoachNotes)
            .Include(e => e.ExerciseExerciseTypes)
                .ThenInclude(eet => eet.ExerciseType)
            .Include(e => e.ExerciseMuscleGroups)
                .ThenInclude(emg => emg.MuscleGroup)
            .Include(e => e.ExerciseMuscleGroups)
                .ThenInclude(emg => emg.MuscleRole)
            .Include(e => e.ExerciseEquipment)
                .ThenInclude(ee => ee.Equipment)
            .Include(e => e.ExerciseMovementPatterns)
                .ThenInclude(emp => emp.MovementPattern)
            .Include(e => e.ExerciseBodyParts)
                .ThenInclude(ebp => ebp.BodyPart)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync();
        
        return exercise ?? Exercise.Empty;
    }
    
    /// <summary>
    /// Gets an exercise by its name (case-insensitive)
    /// </summary>
    public async Task<Exercise> GetByNameAsync(string name)
    {
        var exercise = await Context.Exercises
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Name.ToLower() == name.ToLower());
        
        return exercise ?? Exercise.Empty;
    }
    
    /// <summary>
    /// Checks if an exercise name already exists (case-insensitive)
    /// </summary>
    public async Task<bool> ExistsAsync(string name, ExerciseId? excludeId = null)
    {
        var query = Context.Exercises
            .Where(e => e.Name.ToLower() == name.ToLower());
        
        if (excludeId.HasValue)
        {
            query = query.Where(e => e.Id != excludeId.Value);
        }
        
        return await query.AnyAsync();
    }
    
    /// <summary>
    /// Checks if an exercise has any references in workouts or user data
    /// </summary>
    public async Task<bool> HasReferencesAsync(ExerciseId id)
    {
        // Check if the exercise is referenced in any workout log sets
        // TODO: Uncomment when WorkoutLogSet entity is properly configured in test context
        // var hasWorkoutReferences = await Context.WorkoutLogSets
        //     .AnyAsync(wls => wls.ExerciseId == id);
        
        // In the future, add checks for other references like:
        // - UserFavoriteExercises
        // - WorkoutTemplates
        // - ExercisePrograms
        
        // For now, return false until these entities are implemented
        return await Task.FromResult(false);
    }
    
    /// <summary>
    /// Adds a new exercise to the repository
    /// </summary>
    public async Task<Exercise> AddAsync(Exercise exercise)
    {
        Context.Exercises.Add(exercise);
        await Context.SaveChangesAsync();

        // Load navigation properties using fluent API - each property is explicitly visible
        return await Context.Entry(exercise)
            .LoadNavigation()
            .IncludeDifficulty()
            .IncludeKineticChain()
            .IncludeExerciseWeightType()
            .IncludeCoachNotes()
            .IncludeExerciseTypes()
            .IncludeMuscleGroups()
            .IncludeEquipment()
            .IncludeBodyParts()
            .IncludeMovementPatterns()
            .LoadAsync();
    }
    
    /// <summary>
    /// Updates an existing exercise
    /// </summary>
    public async Task<Exercise> UpdateAsync(Exercise exercise)
    {
        // Load the existing exercise with all relationships
        var existingExercise = await Context.Exercises
            .Include(e => e.CoachNotes)
            .Include(e => e.ExerciseExerciseTypes)
            .Include(e => e.ExerciseMuscleGroups)
            .Include(e => e.ExerciseEquipment)
            .Include(e => e.ExerciseMovementPatterns)
            .Include(e => e.ExerciseBodyParts)
            .FirstOrDefaultAsync(e => e.Id == exercise.Id)
                ?? throw new InvalidOperationException($"Exercise with ID {exercise.Id} not found");

        // Update scalar properties
        Context.Entry(existingExercise).CurrentValues.SetValues(exercise);
        
        // Update all relationships using ReplaceWith extension
        existingExercise.CoachNotes.ReplaceWith(exercise.CoachNotes);
        existingExercise.ExerciseExerciseTypes.ReplaceWith(exercise.ExerciseExerciseTypes);
        existingExercise.ExerciseMuscleGroups.ReplaceWith(exercise.ExerciseMuscleGroups);
        existingExercise.ExerciseEquipment.ReplaceWith(exercise.ExerciseEquipment);
        existingExercise.ExerciseMovementPatterns.ReplaceWith(exercise.ExerciseMovementPatterns);
        existingExercise.ExerciseBodyParts.ReplaceWith(exercise.ExerciseBodyParts);
        
        await Context.SaveChangesAsync();
        
        // Reload the exercise with all navigation properties
        return await GetByIdAsync(exercise.Id);
    }
    
    /// <summary>
    /// Soft deletes an exercise by marking it as inactive
    /// </summary>
    public async Task SoftDeleteAsync(ExerciseId id)
    {
        await Context.Exercises
            .Where(e => e.Id == id)
            .ExecuteUpdateAsync(e => e.SetProperty(x => x.IsActive, false));
    }
    
    /// <summary>
    /// Deletes an exercise from the repository
    /// </summary>
    public async Task<bool> DeleteAsync(ExerciseId id)
    {
        var exercise = await Context.Exercises.FindAsync(id);
        
        if (exercise == null)
        {
            return false;
        }
        
        Context.Exercises.Remove(exercise);
        await Context.SaveChangesAsync();
        
        return true;
    }
}