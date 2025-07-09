using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Repositories.Implementations;

/// <summary>
/// Repository implementation for ExerciseLink data operations
/// </summary>
public class ExerciseLinkRepository : RepositoryBase<FitnessDbContext>, IExerciseLinkRepository
{
    /// <summary>
    /// Gets all links for a source exercise, optionally filtered by link type
    /// </summary>
    public async Task<IEnumerable<ExerciseLink>> GetBySourceExerciseAsync(ExerciseId sourceExerciseId, string? linkType = null)
    {
        var query = Context.ExerciseLinks
            .Include(el => el.TargetExercise)
            .Where(el => el.SourceExerciseId == sourceExerciseId && el.IsActive);
        
        if (!string.IsNullOrWhiteSpace(linkType))
        {
            query = query.Where(el => el.LinkType == linkType);
        }
        
        return await query
            .OrderBy(el => el.DisplayOrder)
            .ThenBy(el => el.TargetExercise!.Name)
            .AsNoTracking()
            .ToListAsync();
    }
    
    /// <summary>
    /// Gets all links where the specified exercise is the target
    /// </summary>
    public async Task<IEnumerable<ExerciseLink>> GetByTargetExerciseAsync(ExerciseId targetExerciseId)
    {
        return await Context.ExerciseLinks
            .Include(el => el.SourceExercise)
            .Where(el => el.TargetExerciseId == targetExerciseId && el.IsActive)
            .OrderBy(el => el.SourceExercise!.Name)
            .AsNoTracking()
            .ToListAsync();
    }
    
    /// <summary>
    /// Checks if a link already exists between two exercises with a specific type
    /// </summary>
    public async Task<bool> ExistsAsync(ExerciseId sourceExerciseId, ExerciseId targetExerciseId, string linkType)
    {
        return await Context.ExerciseLinks
            .AnyAsync(el => el.SourceExerciseId == sourceExerciseId 
                && el.TargetExerciseId == targetExerciseId 
                && el.LinkType == linkType 
                && el.IsActive);
    }
    
    /// <summary>
    /// Gets the most commonly used links across all exercises
    /// </summary>
    public async Task<IEnumerable<(ExerciseLink link, int usageCount)>> GetMostUsedLinksAsync(int count)
    {
        // Simplified query for in-memory database compatibility
        var allLinks = await Context.ExerciseLinks
            .Where(el => el.IsActive)
            .Include(el => el.TargetExercise)
            .AsNoTracking()
            .ToListAsync();
        
        var linkGroups = allLinks
            .GroupBy(el => new { el.TargetExerciseId, el.LinkType })
            .Select(g => new 
            { 
                TargetExerciseId = g.Key.TargetExerciseId,
                LinkType = g.Key.LinkType,
                UsageCount = g.Count(),
                FirstLink = g.FirstOrDefault()
            })
            .OrderByDescending(g => g.UsageCount)
            .Take(count)
            .ToList();
        
        var results = new List<(ExerciseLink link, int usageCount)>();
        foreach (var group in linkGroups)
        {
            if (group.FirstLink != null)
            {
                results.Add((group.FirstLink, group.UsageCount));
            }
        }
        
        return results;
    }
    
    /// <summary>
    /// Gets a specific exercise link by ID
    /// </summary>
    public async Task<ExerciseLink?> GetByIdAsync(ExerciseLinkId id)
    {
        return await Context.ExerciseLinks
            .Include(el => el.SourceExercise)
            .Include(el => el.TargetExercise)
            .AsNoTracking()
            .FirstOrDefaultAsync(el => el.Id == id);
    }
    
    /// <summary>
    /// Adds a new exercise link
    /// </summary>
    public async Task<ExerciseLink> AddAsync(ExerciseLink exerciseLink)
    {
        await Context.ExerciseLinks.AddAsync(exerciseLink);
        return exerciseLink;
    }
    
    /// <summary>
    /// Updates an existing exercise link
    /// </summary>
    public async Task<ExerciseLink> UpdateAsync(ExerciseLink exerciseLink)
    {
        var existingLink = await Context.ExerciseLinks.FindAsync(exerciseLink.Id);
        if (existingLink != null)
        {
            Context.Entry(existingLink).CurrentValues.SetValues(exerciseLink);
        }
        else
        {
            Context.ExerciseLinks.Update(exerciseLink);
        }
        return exerciseLink;
    }
    
    /// <summary>
    /// Deletes an exercise link (hard delete)
    /// </summary>
    public async Task<bool> DeleteAsync(ExerciseLinkId id)
    {
        var exerciseLink = await Context.ExerciseLinks.FirstOrDefaultAsync(el => el.Id == id);
        if (exerciseLink == null)
        {
            return false;
        }
        
        // Hard delete - remove from database
        Context.ExerciseLinks.Remove(exerciseLink);
        
        return true;
    }
}