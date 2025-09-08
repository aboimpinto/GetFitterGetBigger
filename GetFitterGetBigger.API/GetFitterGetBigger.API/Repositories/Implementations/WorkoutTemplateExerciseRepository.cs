using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Repositories.Implementations;

/// <summary>
/// Repository implementation for managing WorkoutTemplateExercise entities with phase/round-based organization
/// </summary>
public class WorkoutTemplateExerciseRepository : RepositoryBase<FitnessDbContext>, IWorkoutTemplateExerciseRepository
{
    // CRUD Operations
    /// <inheritdoc/>
    public async Task<WorkoutTemplateExercise> GetByIdAsync(WorkoutTemplateExerciseId id)
    {
        var exercise = await Context.WorkoutTemplateExercises
            .AsNoTracking()
            .FirstOrDefaultAsync(wte => wte.Id == id);

        return exercise ?? WorkoutTemplateExercise.Empty;
    }

    /// <inheritdoc/>
    public async Task<List<WorkoutTemplateExercise>> GetByWorkoutTemplateAsync(WorkoutTemplateId workoutTemplateId)
    {
        // NOTE: Using Zone mapping until entity is updated to Phase/Round structure
        return await Context.WorkoutTemplateExercises
            .Include(wte => wte.Exercise)
            .Where(wte => wte.WorkoutTemplateId == workoutTemplateId)
            .OrderBy(wte => wte.Zone)
            .ThenBy(wte => wte.SequenceOrder)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<List<WorkoutTemplateExercise>> GetByTemplateAndPhaseAsync(WorkoutTemplateId workoutTemplateId, string phase)
    {
        // NOTE: Temporary mapping until entity updated - mapping phase to zone
        var zone = MapPhaseToZone(phase);
        return await Context.WorkoutTemplateExercises
            .Include(wte => wte.Exercise)
            .Where(wte => wte.WorkoutTemplateId == workoutTemplateId && wte.Zone == zone)
            .OrderBy(wte => wte.SequenceOrder)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<List<WorkoutTemplateExercise>> GetByTemplatePhaseAndRoundAsync(WorkoutTemplateId workoutTemplateId, string phase, int roundNumber)
    {
        // NOTE: Temporary implementation until entity supports rounds
        var zone = MapPhaseToZone(phase);
        return await Context.WorkoutTemplateExercises
            .Include(wte => wte.Exercise)
            .Where(wte => wte.WorkoutTemplateId == workoutTemplateId && wte.Zone == zone)
            .OrderBy(wte => wte.SequenceOrder)
            .AsNoTracking()
            .ToListAsync();
    }

    // Auto-linking support queries
    /// <inheritdoc/>
    public async Task<List<WorkoutTemplateExercise>> GetWorkoutExercisesAsync(WorkoutTemplateId workoutTemplateId)
    {
        return await Context.WorkoutTemplateExercises
            .Include(wte => wte.Exercise)
            .Where(wte => wte.WorkoutTemplateId == workoutTemplateId && wte.Zone == WorkoutZone.Main)
            .OrderBy(wte => wte.SequenceOrder)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsInTemplateAsync(WorkoutTemplateId workoutTemplateId, ExerciseId exerciseId)
    {
        return await Context.WorkoutTemplateExercises
            .AnyAsync(wte => wte.WorkoutTemplateId == workoutTemplateId && wte.ExerciseId == exerciseId);
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsInPhaseAsync(WorkoutTemplateId workoutTemplateId, string phase, ExerciseId exerciseId)
    {
        var zone = MapPhaseToZone(phase);
        return await Context.WorkoutTemplateExercises
            .AnyAsync(wte => wte.WorkoutTemplateId == workoutTemplateId && 
                           wte.Zone == zone && 
                           wte.ExerciseId == exerciseId);
    }

    // Order management
    /// <inheritdoc/>
    public async Task<int> GetMaxOrderInRoundAsync(WorkoutTemplateId workoutTemplateId, string phase, int roundNumber)
    {
        // NOTE: Temporary implementation - using SequenceOrder until entity supports OrderInRound
        var zone = MapPhaseToZone(phase);
        var maxOrder = await Context.WorkoutTemplateExercises
            .Where(wte => wte.WorkoutTemplateId == workoutTemplateId && wte.Zone == zone)
            .Select(wte => wte.SequenceOrder)
            .DefaultIfEmpty(0)
            .MaxAsync();
            
        return maxOrder;
    }

    /// <inheritdoc/>
    public async Task ReorderExercisesInRoundAsync(WorkoutTemplateId workoutTemplateId, string phase, int roundNumber, Dictionary<WorkoutTemplateExerciseId, int> newOrders)
    {
        var zone = MapPhaseToZone(phase);
        var exercises = await Context.WorkoutTemplateExercises
            .Where(wte => wte.WorkoutTemplateId == workoutTemplateId && wte.Zone == zone)
            .ToListAsync();
        
        foreach (var exercise in exercises)
        {
            if (newOrders.TryGetValue(exercise.Id, out var newOrder))
            {
                // Use record 'with' syntax to update
                var updatedExercise = exercise with { SequenceOrder = newOrder };
                Context.WorkoutTemplateExercises.Update(updatedExercise);
            }
        }
        
        await Context.SaveChangesAsync();
    }

    // Round management
    /// <inheritdoc/>
    public async Task<List<WorkoutTemplateExercise>> GetRoundExercisesAsync(WorkoutTemplateId workoutTemplateId, string phase, int roundNumber)
    {
        // NOTE: Temporary implementation until rounds are supported
        return await GetByTemplateAndPhaseAsync(workoutTemplateId, phase);
    }

    /// <inheritdoc/>
    public Task<int> GetMaxRoundNumberAsync(WorkoutTemplateId workoutTemplateId, string phase)
    {
        // NOTE: Temporary implementation - always return 1 until rounds are supported
        return Task.FromResult(1);
    }

    // Modification operations
    /// <inheritdoc/>
    public async Task AddAsync(WorkoutTemplateExercise exercise)
    {
        await Context.WorkoutTemplateExercises.AddAsync(exercise);
        // SaveChangesAsync removed - UnitOfWork handles transaction management
    }

    /// <inheritdoc/>
    public async Task AddRangeAsync(List<WorkoutTemplateExercise> exercises)
    {
        await Context.WorkoutTemplateExercises.AddRangeAsync(exercises);
        // SaveChangesAsync removed - UnitOfWork handles transaction management
    }

    /// <inheritdoc/>
    public Task UpdateAsync(WorkoutTemplateExercise exercise)
    {
        Context.WorkoutTemplateExercises.Update(exercise);
        // SaveChangesAsync removed - UnitOfWork handles transaction management
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(WorkoutTemplateExerciseId id)
    {
        var exercise = await Context.WorkoutTemplateExercises.FindAsync(id);
        if (exercise != null)
        {
            Context.WorkoutTemplateExercises.Remove(exercise);
            // SaveChangesAsync removed - UnitOfWork handles transaction management
        }
    }

    /// <inheritdoc/>
    public async Task DeleteRangeAsync(List<WorkoutTemplateExerciseId> ids)
    {
        var exercises = await Context.WorkoutTemplateExercises
            .Where(wte => ids.Contains(wte.Id))
            .ToListAsync();
        
        Context.WorkoutTemplateExercises.RemoveRange(exercises);
        // SaveChangesAsync removed - UnitOfWork handles transaction management
    }

    // LEGACY METHODS - For backward compatibility until service layer is updated
    /// <inheritdoc/>
    public async Task<WorkoutTemplateExercise> GetByIdWithDetailsAsync(WorkoutTemplateExerciseId id)
    {
        var exercise = await Context.WorkoutTemplateExercises
            .Include(wte => wte.Exercise)
            .Include(wte => wte.Configurations)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(wte => wte.Id == id);

        return exercise ?? WorkoutTemplateExercise.Empty;
    }

    /// <inheritdoc/>
    public async Task<int> GetMaxSequenceOrderAsync(WorkoutTemplateId workoutTemplateId, WorkoutZone zone)
    {
        var maxOrder = await Context.WorkoutTemplateExercises
            .Where(wte => wte.WorkoutTemplateId == workoutTemplateId && wte.Zone == zone)
            .Select(wte => wte.SequenceOrder)
            .DefaultIfEmpty(0)
            .MaxAsync();

        return maxOrder;
    }

    /// <inheritdoc/>
    public async Task<bool> ReorderExercisesAsync(WorkoutTemplateId workoutTemplateId, WorkoutZone zone, Dictionary<WorkoutTemplateExerciseId, int> exerciseOrders)
    {
        var exercises = await Context.WorkoutTemplateExercises
            .Where(wte => wte.WorkoutTemplateId == workoutTemplateId && wte.Zone == zone)
            .ToListAsync();

        foreach (var exercise in exercises)
        {
            if (exerciseOrders.TryGetValue(exercise.Id, out var newOrder))
            {
                var updatedExercise = exercise with { SequenceOrder = newOrder };
                Context.WorkoutTemplateExercises.Update(updatedExercise);
            }
        }

        var result = await Context.SaveChangesAsync();
        return result > 0;
    }

    // Helper method to map phase strings to WorkoutZone enum until entity is updated
    private static WorkoutZone MapPhaseToZone(string phase) => phase switch
    {
        "Warmup" => WorkoutZone.Warmup,
        "Workout" => WorkoutZone.Main,
        "Cooldown" => WorkoutZone.Cooldown,
        _ => WorkoutZone.Main
    };
}