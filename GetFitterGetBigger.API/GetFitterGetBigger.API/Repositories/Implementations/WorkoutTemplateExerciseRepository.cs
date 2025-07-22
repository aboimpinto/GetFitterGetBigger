using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Repositories.Implementations;

/// <summary>
/// Repository implementation for managing WorkoutTemplateExercise entities
/// </summary>
public class WorkoutTemplateExerciseRepository : RepositoryBase<FitnessDbContext>, IWorkoutTemplateExerciseRepository
{

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
    public async Task<IEnumerable<WorkoutTemplateExercise>> GetByWorkoutTemplateAsync(WorkoutTemplateId workoutTemplateId)
    {
        return await Context.WorkoutTemplateExercises
            .Include(wte => wte.Exercise)
            .Include(wte => wte.Configurations)
            .Where(wte => wte.WorkoutTemplateId == workoutTemplateId)
            .OrderBy(wte => wte.Zone)
            .ThenBy(wte => wte.SequenceOrder)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<WorkoutTemplateExercise>> GetByZoneAsync(WorkoutTemplateId workoutTemplateId, WorkoutZone zone)
    {
        return await Context.WorkoutTemplateExercises
            .Include(wte => wte.Exercise)
            .Include(wte => wte.Configurations)
            .Where(wte => wte.WorkoutTemplateId == workoutTemplateId && wte.Zone == zone)
            .OrderBy(wte => wte.SequenceOrder)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<bool> IsExerciseInUseAsync(ExerciseId exerciseId)
    {
        return await Context.WorkoutTemplateExercises
            .AnyAsync(wte => wte.ExerciseId == exerciseId);
    }

    /// <inheritdoc/>
    public async Task<int> GetTemplateCountByExerciseAsync(ExerciseId exerciseId)
    {
        return await Context.WorkoutTemplateExercises
            .Where(wte => wte.ExerciseId == exerciseId)
            .Select(wte => wte.WorkoutTemplateId)
            .Distinct()
            .CountAsync();
    }

    /// <inheritdoc/>
    public async Task<int> GetMaxSequenceOrderAsync(WorkoutTemplateId workoutTemplateId, WorkoutZone zone)
    {
        var maxOrder = await Context.WorkoutTemplateExercises
            .Where(wte => wte.WorkoutTemplateId == workoutTemplateId && wte.Zone == zone)
            .MaxAsync(wte => (int?)wte.SequenceOrder);

        return maxOrder ?? 0;
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

    /// <inheritdoc/>
    public async Task<WorkoutTemplateExercise> AddAsync(WorkoutTemplateExercise workoutTemplateExercise)
    {
        Context.WorkoutTemplateExercises.Add(workoutTemplateExercise);
        await Context.SaveChangesAsync();
        return workoutTemplateExercise;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<WorkoutTemplateExercise>> AddRangeAsync(IEnumerable<WorkoutTemplateExercise> workoutTemplateExercises)
    {
        var exercisesList = workoutTemplateExercises.ToList();
        Context.WorkoutTemplateExercises.AddRange(exercisesList);
        await Context.SaveChangesAsync();
        return exercisesList;
    }

    /// <inheritdoc/>
    public async Task<WorkoutTemplateExercise> UpdateAsync(WorkoutTemplateExercise workoutTemplateExercise)
    {
        Context.WorkoutTemplateExercises.Update(workoutTemplateExercise);
        await Context.SaveChangesAsync();
        return workoutTemplateExercise;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(WorkoutTemplateExerciseId id)
    {
        var exercise = await Context.WorkoutTemplateExercises.FindAsync(id);
        
        return exercise switch
        {
            null => false,
            _ => await DeleteExerciseAndSaveAsync(exercise)
        };
    }
    
    private async Task<bool> DeleteExerciseAndSaveAsync(WorkoutTemplateExercise exercise)
    {
        Context.WorkoutTemplateExercises.Remove(exercise);
        var result = await Context.SaveChangesAsync();
        return result > 0;
    }

    /// <inheritdoc/>
    public async Task<int> DeleteAllByWorkoutTemplateAsync(WorkoutTemplateId workoutTemplateId)
    {
        var exercises = await Context.WorkoutTemplateExercises
            .Where(wte => wte.WorkoutTemplateId == workoutTemplateId)
            .ToListAsync();

        Context.WorkoutTemplateExercises.RemoveRange(exercises);
        return await Context.SaveChangesAsync();
    }
}