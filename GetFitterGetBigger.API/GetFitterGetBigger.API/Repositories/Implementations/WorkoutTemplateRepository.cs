using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Repositories.Implementations;

/// <summary>
/// Repository implementation for WorkoutTemplate data with advanced querying capabilities
/// </summary>
public class WorkoutTemplateRepository : RepositoryBase<FitnessDbContext>, IWorkoutTemplateRepository
{
    /// <summary>
    /// Gets an IQueryable of workout templates with necessary includes for querying and filtering
    /// </summary>
    public IQueryable<WorkoutTemplate> GetWorkoutTemplatesQueryable()
    {
        return Context.WorkoutTemplates
            .Include(w => w.Category)
            .Include(w => w.Difficulty)
            .Include(w => w.WorkoutState)
            .Include(w => w.Objectives)
                .ThenInclude(o => o.WorkoutObjective)
            .AsNoTracking();
    }
    
    /// <summary>
    /// Gets a workout template by ID
    /// </summary>
    public async Task<WorkoutTemplate> GetByIdAsync(WorkoutTemplateId id)
    {
        var result = id.IsEmpty switch
        {
            true => Task.FromResult(WorkoutTemplate.Empty),
            false => LoadTemplateByIdAsync(id)
        };
        
        return await result;
    }
    
    private async Task<WorkoutTemplate> LoadTemplateByIdAsync(WorkoutTemplateId id)
    {
        var template = await Context.WorkoutTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id);

        return template ?? WorkoutTemplate.Empty;
    }

    /// <summary>
    /// Gets a workout template by ID with all related data
    /// </summary>
    public async Task<WorkoutTemplate> GetByIdWithDetailsAsync(WorkoutTemplateId id)
    {
        var template = await Context.WorkoutTemplates
            .Include(w => w.WorkoutState)
            .Include(w => w.Category)
            .Include(w => w.Difficulty)
            .Include(w => w.Exercises)
                .ThenInclude(e => e.Exercise)
                    .ThenInclude(ex => ex!.Difficulty)
            .Include(w => w.Exercises)
                .ThenInclude(e => e.Exercise)
                    .ThenInclude(ex => ex!.ExerciseMuscleGroups)
                        .ThenInclude(emg => emg.MuscleGroup)
            .Include(w => w.Exercises)
                .ThenInclude(e => e.Exercise)
                    .ThenInclude(ex => ex!.ExerciseEquipment)
                        .ThenInclude(ee => ee.Equipment)
            .Include(w => w.Exercises)
                .ThenInclude(e => e.Configurations)
            .Include(w => w.Objectives)
                .ThenInclude(o => o.WorkoutObjective)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id);

        return template ?? WorkoutTemplate.Empty;
    }



    /// <summary>
    /// Checks if a workout template exists by ID
    /// </summary>
    public async Task<bool> ExistsAsync(WorkoutTemplateId id)
    {
        var result = id.IsEmpty switch
        {
            true => Task.FromResult(false),
            false => Context.WorkoutTemplates
                .AsNoTracking()
                .AnyAsync(w => w.Id == id)
        };
        
        return await result;
    }

    /// <summary>
    /// Checks if a workout template with the given name exists
    /// </summary>
    public async Task<bool> ExistsByNameAsync(
        string name, 
        WorkoutTemplateId excludeTemplateId = default)
    {
        var query = Context.WorkoutTemplates
            .Where(w => w.Name.ToLower() == name.ToLower());

        // Exclude specific template if provided
        query = excludeTemplateId.IsEmpty
            ? query
            : query.Where(w => w.Id != excludeTemplateId);

        return await query.AnyAsync();
    }

    /// <summary>
    /// Adds a new workout template
    /// </summary>
    public async Task<WorkoutTemplate> AddAsync(WorkoutTemplate workoutTemplate)
    {
        Context.WorkoutTemplates.Add(workoutTemplate);
        await Context.SaveChangesAsync();

        // Load navigation properties
        await Context.Entry(workoutTemplate)
            .Reference(w => w.WorkoutState)
            .LoadAsync();

        await Context.Entry(workoutTemplate)
            .Reference(w => w.Category)
            .LoadAsync();

        await Context.Entry(workoutTemplate)
            .Reference(w => w.Difficulty)
            .LoadAsync();

        // Load exercises with their related data
        await Context.Entry(workoutTemplate)
            .Collection(w => w.Exercises)
            .Query()
            .Include(e => e.Exercise)
                .ThenInclude(ex => ex!.Difficulty)
            .Include(e => e.Exercise)
                .ThenInclude(ex => ex!.ExerciseMuscleGroups)
                    .ThenInclude(emg => emg.MuscleGroup)
            .Include(e => e.Exercise)
                .ThenInclude(ex => ex!.ExerciseEquipment)
                    .ThenInclude(ee => ee.Equipment)
            .Include(e => e.Configurations)
            .LoadAsync();

        // Load objectives
        await Context.Entry(workoutTemplate)
            .Collection(w => w.Objectives)
            .Query()
            .Include(o => o.WorkoutObjective)
            .LoadAsync();

        return workoutTemplate;
    }

    /// <summary>
    /// Updates an existing workout template
    /// </summary>
    public async Task<WorkoutTemplate> UpdateAsync(WorkoutTemplate workoutTemplate)
    {
        // First, get the existing template with all relationships
        var existingTemplate = await Context.WorkoutTemplates
            .Include(w => w.Exercises)
                .ThenInclude(e => e.Configurations)
            .Include(w => w.Objectives)
            .FirstOrDefaultAsync(w => w.Id == workoutTemplate.Id);

        var result = existingTemplate switch
        {
            null => WorkoutTemplate.Empty,
            _ => await UpdateTemplateInternal(existingTemplate, workoutTemplate)
        };

        return result;
    }

    /// <summary>
    /// Soft deletes a workout template (marks as inactive)
    /// </summary>
    public async Task<bool> SoftDeleteAsync(WorkoutTemplateId id)
    {
        var template = await Context.WorkoutTemplates
            .Include(w => w.WorkoutState)
            .FirstOrDefaultAsync(w => w.Id == id);

        var result = (template == null || template.IsEmpty) switch
        {
            true => Task.FromResult(false),
            false => ProcessSoftDeleteAsync(template)
        };
        
        return await result;
    }
    
    private async Task<bool> ProcessSoftDeleteAsync(WorkoutTemplate template)
    {
        // Set WorkoutState to ARCHIVED
        var archivedState = await Context.WorkoutStates
            .FirstOrDefaultAsync(ws => ws.Value == "ARCHIVED");

        var result = (archivedState == null || archivedState.IsEmpty) switch
        {
            true => false,
            false => await ArchiveTemplateAsync(template, archivedState)
        };
        
        return result;
    }
    
    private async Task<bool> ArchiveTemplateAsync(WorkoutTemplate template, WorkoutState archivedState)
    {
        // Since WorkoutStateId is init-only, we need to remove and re-add the entity
        Context.WorkoutTemplates.Remove(template);
        await Context.SaveChangesAsync();
        
        var updatedTemplate = template with { WorkoutStateId = archivedState.WorkoutStateId };
        Context.WorkoutTemplates.Add(updatedTemplate);
        await Context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Permanently deletes a workout template
    /// </summary>
    public async Task<bool> DeleteAsync(WorkoutTemplateId id)
    {
        var template = await Context.WorkoutTemplates.FindAsync(id);

        var result = template switch
        {
            null => false,
            _ => await PerformDeleteAsync(template)
        };
        
        return result;
    }
    
    private async Task<bool> PerformDeleteAsync(WorkoutTemplate template)
    {
        Context.WorkoutTemplates.Remove(template);
        await Context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Internal method to update template relationships
    /// </summary>
    private async Task<WorkoutTemplate> UpdateTemplateInternal(
        WorkoutTemplate existingTemplate,
        WorkoutTemplate workoutTemplate)
    {
        // Clear existing relationships
        existingTemplate.Exercises.Clear();
        existingTemplate.Objectives.Clear();

        // Update scalar properties
        Context.Entry(existingTemplate).CurrentValues.SetValues(workoutTemplate);

        // Add new exercises
        foreach (var exercise in workoutTemplate.Exercises)
        {
            existingTemplate.Exercises.Add(exercise);
        }

        // Add new objectives
        foreach (var objective in workoutTemplate.Objectives)
        {
            existingTemplate.Objectives.Add(objective);
        }

        await Context.SaveChangesAsync();

        // Reload with all related data
        return await GetByIdWithDetailsAsync(workoutTemplate.Id);
    }
}