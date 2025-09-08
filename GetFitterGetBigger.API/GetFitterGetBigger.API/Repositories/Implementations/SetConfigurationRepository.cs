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
/// Repository implementation for managing SetConfiguration entities
/// </summary>
public class SetConfigurationRepository : DomainRepository<SetConfiguration, SetConfigurationId, FitnessDbContext>, ISetConfigurationRepository
{
    /// <inheritdoc/>
    public override async Task<SetConfiguration> GetByIdAsync(SetConfigurationId id)
    {
        var setConfiguration = await Context.SetConfigurations
            .Include(sc => sc.WorkoutTemplateExercise)
            .AsNoTracking()
            .FirstOrDefaultAsync(sc => sc.Id == id);

        return setConfiguration ?? SetConfiguration.Empty;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<SetConfiguration>> GetByWorkoutTemplateExerciseAsync(WorkoutTemplateExerciseId workoutTemplateExerciseId)
    {
        return await Context.SetConfigurations
            .Where(sc => sc.WorkoutTemplateExerciseId == workoutTemplateExerciseId)
            .OrderBy(sc => sc.SetNumber)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Dictionary<WorkoutTemplateExerciseId, IEnumerable<SetConfiguration>>> GetByWorkoutTemplateExercisesAsync(IEnumerable<WorkoutTemplateExerciseId> workoutTemplateExerciseIds)
    {
        var exerciseIdsList = workoutTemplateExerciseIds.ToList();
        
        var setConfigurations = await Context.SetConfigurations
            .Where(sc => exerciseIdsList.Contains(sc.WorkoutTemplateExerciseId))
            .OrderBy(sc => sc.WorkoutTemplateExerciseId)
            .ThenBy(sc => sc.SetNumber)
            .AsNoTracking()
            .ToListAsync();

        return setConfigurations
            .GroupBy(sc => sc.WorkoutTemplateExerciseId)
            .ToDictionary(g => g.Key, g => g.AsEnumerable());
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<SetConfiguration>> GetByWorkoutTemplateAsync(WorkoutTemplateId workoutTemplateId)
    {
        return await Context.SetConfigurations
            .Include(sc => sc.WorkoutTemplateExercise)
            .Where(sc => sc.WorkoutTemplateExercise != null && sc.WorkoutTemplateExercise.WorkoutTemplateId == workoutTemplateId)
            .OrderBy(sc => sc.WorkoutTemplateExercise != null ? sc.WorkoutTemplateExercise.Zone : WorkoutZone.Main)
            .ThenBy(sc => sc.WorkoutTemplateExercise != null ? sc.WorkoutTemplateExercise.SequenceOrder : 0)
            .ThenBy(sc => sc.SetNumber)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<int> GetMaxSetNumberAsync(WorkoutTemplateExerciseId workoutTemplateExerciseId)
    {
        var maxSetNumber = await Context.SetConfigurations
            .Where(sc => sc.WorkoutTemplateExerciseId == workoutTemplateExerciseId)
            .MaxAsync(sc => (int?)sc.SetNumber);

        return maxSetNumber ?? 0;
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(WorkoutTemplateExerciseId workoutTemplateExerciseId, int setNumber)
    {
        return await Context.SetConfigurations
            .AnyAsync(sc => sc.WorkoutTemplateExerciseId == workoutTemplateExerciseId && sc.SetNumber == setNumber);
    }

    /// <inheritdoc/>
    public async Task<SetConfiguration> AddAsync(SetConfiguration setConfiguration)
    {
        Context.SetConfigurations.Add(setConfiguration);
        await Context.SaveChangesAsync();
        return setConfiguration;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<SetConfiguration>> AddRangeAsync(IEnumerable<SetConfiguration> setConfigurations)
    {
        var configurationsList = setConfigurations.ToList();
        Context.SetConfigurations.AddRange(configurationsList);
        await Context.SaveChangesAsync();
        return configurationsList;
    }

    /// <inheritdoc/>
    public async Task<SetConfiguration> UpdateAsync(SetConfiguration setConfiguration)
    {
        Context.SetConfigurations.Update(setConfiguration);
        await Context.SaveChangesAsync();
        return setConfiguration;
    }

    /// <inheritdoc/>
    public async Task<int> UpdateRangeAsync(IEnumerable<SetConfiguration> setConfigurations)
    {
        Context.SetConfigurations.UpdateRange(setConfigurations);
        return await Context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(SetConfigurationId id)
    {
        var setConfiguration = await Context.SetConfigurations.FindAsync(id);
        
        return setConfiguration switch
        {
            null => false,
            _ => await DeleteConfigurationAndSaveAsync(setConfiguration)
        };
    }
    
    private async Task<bool> DeleteConfigurationAndSaveAsync(SetConfiguration setConfiguration)
    {
        Context.SetConfigurations.Remove(setConfiguration);
        var result = await Context.SaveChangesAsync();
        return result > 0;
    }

    /// <inheritdoc/>
    public async Task<int> DeleteByWorkoutTemplateExerciseAsync(WorkoutTemplateExerciseId workoutTemplateExerciseId)
    {
        var setConfigurations = await Context.SetConfigurations
            .Where(sc => sc.WorkoutTemplateExerciseId == workoutTemplateExerciseId)
            .ToListAsync();

        Context.SetConfigurations.RemoveRange(setConfigurations);
        return await Context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<int> DeleteByWorkoutTemplateAsync(WorkoutTemplateId workoutTemplateId)
    {
        var setConfigurations = await Context.SetConfigurations
            .Include(sc => sc.WorkoutTemplateExercise)
            .Where(sc => sc.WorkoutTemplateExercise != null && sc.WorkoutTemplateExercise.WorkoutTemplateId == workoutTemplateId)
            .ToListAsync();

        Context.SetConfigurations.RemoveRange(setConfigurations);
        return await Context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<bool> ReorderSetsAsync(WorkoutTemplateExerciseId workoutTemplateExerciseId, Dictionary<SetConfigurationId, int> setReorders)
    {
        var setConfigurations = await Context.SetConfigurations
            .Where(sc => sc.WorkoutTemplateExerciseId == workoutTemplateExerciseId)
            .ToListAsync();

        foreach (var setConfiguration in setConfigurations)
        {
            if (setReorders.TryGetValue(setConfiguration.Id, out var newSetNumber))
            {
                var updatedSetConfiguration = setConfiguration with { SetNumber = newSetNumber };
                Context.SetConfigurations.Update(updatedSetConfiguration);
            }
        }

        var result = await Context.SaveChangesAsync();
        return result > 0;
    }
}