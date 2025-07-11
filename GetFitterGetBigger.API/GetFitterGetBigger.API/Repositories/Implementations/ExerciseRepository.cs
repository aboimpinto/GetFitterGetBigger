using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Repositories.Implementations;

/// <summary>
/// Repository implementation for Exercise data with advanced querying capabilities
/// </summary>
public class ExerciseRepository : RepositoryBase<FitnessDbContext>, IExerciseRepository
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
        var query = Context.Exercises
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
            .AsNoTracking();
        
        // Apply filters
        if (!includeInactive)
        {
            query = query.Where(e => e.IsActive);
        }
        
        if (!string.IsNullOrEmpty(name))
        {
            query = query.Where(e => e.Name.ToLower().Contains(name.ToLower()));
        }
        
        if (!difficultyId.IsEmpty)
        {
            query = query.Where(e => e.DifficultyId == difficultyId);
        }
        
        query = query.Where(e => !muscleGroupIds.Any() || 
            e.ExerciseMuscleGroups.Any(emg => muscleGroupIds.Contains(emg.MuscleGroupId)));
        
        query = query.Where(e => !equipmentIds.Any() || 
            e.ExerciseEquipment.Any(ee => equipmentIds.Contains(ee.EquipmentId)));
        
        query = query.Where(e => !movementPatternIds.Any() || 
            e.ExerciseMovementPatterns.Any(emp => movementPatternIds.Contains(emp.MovementPatternId)));
        
        query = query.Where(e => !bodyPartIds.Any() || 
            e.ExerciseBodyParts.Any(ebp => bodyPartIds.Contains(ebp.BodyPartId)));
        
        // Get total count before pagination
        var totalCount = await query.CountAsync();
        
        // Apply pagination
        var exercises = await query
            .OrderBy(e => e.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return (exercises, totalCount);
    }
    
    /// <summary>
    /// Gets an exercise by its ID with all related data
    /// </summary>
    public async Task<Exercise?> GetByIdAsync(ExerciseId id)
    {
        var exercise = await Context.Exercises
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
            .FirstOrDefaultAsync(e => e.Id == id);
        
        return exercise;
    }
    
    /// <summary>
    /// Gets an exercise by its name (case-insensitive)
    /// </summary>
    public async Task<Exercise?> GetByNameAsync(string name) =>
        await Context.Exercises
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Name.ToLower() == name.ToLower());
    
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
        
        // Load navigation properties explicitly for the join entities
        foreach (var eet in exercise.ExerciseExerciseTypes)
        {
            await Context.Entry(eet)
                .Reference(x => x.ExerciseType)
                .LoadAsync();
        }
        
        // Load Difficulty navigation property
        await Context.Entry(exercise)
            .Reference(e => e.Difficulty)
            .LoadAsync();
        
        // Load KineticChain navigation property if present
        if (exercise.KineticChainId.HasValue)
        {
            await Context.Entry(exercise)
                .Reference(e => e.KineticChain)
                .LoadAsync();
        }
        
        return exercise;
    }
    
    /// <summary>
    /// Updates an existing exercise
    /// </summary>
    public async Task<Exercise> UpdateAsync(Exercise exercise)
    {
        // First, remove all existing relationships
        var existingExercise = await Context.Exercises
            .Include(e => e.CoachNotes)
            .Include(e => e.ExerciseExerciseTypes)
            .Include(e => e.ExerciseMuscleGroups)
            .Include(e => e.ExerciseEquipment)
            .Include(e => e.ExerciseMovementPatterns)
            .Include(e => e.ExerciseBodyParts)
            .FirstOrDefaultAsync(e => e.Id == exercise.Id);
        
        if (existingExercise == null)
        {
            throw new InvalidOperationException($"Exercise with ID {exercise.Id} not found");
        }
        
        // Clear existing relationships
        existingExercise.CoachNotes.Clear();
        existingExercise.ExerciseExerciseTypes.Clear();
        existingExercise.ExerciseMuscleGroups.Clear();
        existingExercise.ExerciseEquipment.Clear();
        existingExercise.ExerciseMovementPatterns.Clear();
        existingExercise.ExerciseBodyParts.Clear();
        
        // Update the exercise
        Context.Entry(existingExercise).CurrentValues.SetValues(exercise);
        
        // Add new relationships
        foreach (var cn in exercise.CoachNotes)
        {
            existingExercise.CoachNotes.Add(cn);
        }
        foreach (var eet in exercise.ExerciseExerciseTypes)
        {
            existingExercise.ExerciseExerciseTypes.Add(eet);
        }
        foreach (var emg in exercise.ExerciseMuscleGroups)
        {
            existingExercise.ExerciseMuscleGroups.Add(emg);
        }
        foreach (var ee in exercise.ExerciseEquipment)
        {
            existingExercise.ExerciseEquipment.Add(ee);
        }
        foreach (var emp in exercise.ExerciseMovementPatterns)
        {
            existingExercise.ExerciseMovementPatterns.Add(emp);
        }
        foreach (var ebp in exercise.ExerciseBodyParts)
        {
            existingExercise.ExerciseBodyParts.Add(ebp);
        }
        
        await Context.SaveChangesAsync();
        
        // Reload with all related data
        return (await GetByIdAsync(exercise.Id))!;
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