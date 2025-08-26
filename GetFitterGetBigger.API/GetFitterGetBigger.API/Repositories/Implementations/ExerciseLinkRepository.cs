using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.Enums;
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
    public async Task<ExerciseLink> GetByIdAsync(ExerciseLinkId id)
    {
        var exerciseLink = await Context.ExerciseLinks
            .Include(el => el.SourceExercise)
            .Include(el => el.TargetExercise)
            .AsNoTracking()
            .FirstOrDefaultAsync(el => el.Id == id);
        
        return exerciseLink ?? ExerciseLink.Empty;
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
    
    // ===== ENHANCED BIDIRECTIONAL REPOSITORY METHODS IMPLEMENTATION =====
    
    /// <summary>
    /// Gets bidirectional links for an exercise (both source and target links of specified type)
    /// </summary>
    public async Task<IEnumerable<ExerciseLink>> GetBidirectionalLinksAsync(ExerciseId exerciseId, ExerciseLinkType linkType)
    {
        return await Context.ExerciseLinks
            .Include(el => el.SourceExercise)
            .Include(el => el.TargetExercise)
            .Where(el => el.IsActive && 
                        (el.SourceExerciseId == exerciseId || el.TargetExerciseId == exerciseId) &&
                        ((el.LinkTypeEnum != null && el.LinkTypeEnum == linkType) ||
                         (el.LinkTypeEnum == null && 
                          ((linkType == ExerciseLinkType.WARMUP && el.LinkType == "Warmup") ||
                           (linkType == ExerciseLinkType.COOLDOWN && el.LinkType == "Cooldown")))))
            .OrderBy(el => el.LinkTypeEnum ?? (el.LinkType == "Warmup" ? ExerciseLinkType.WARMUP : ExerciseLinkType.COOLDOWN))
            .ThenBy(el => el.DisplayOrder)
            .AsNoTracking()
            .ToListAsync();
    }
    
    /// <summary>
    /// Checks if bidirectional links exist between two exercises for the specified type
    /// This checks both forward and reverse directions with proper bidirectional type mapping
    /// </summary>
    public async Task<bool> ExistsBidirectionalAsync(ExerciseId sourceId, ExerciseId targetId, ExerciseLinkType linkType)
    {
        // For ALTERNATIVE type, we need to check both directions with ALTERNATIVE
        if (linkType == ExerciseLinkType.ALTERNATIVE)
        {
            // Check if either A→B or B→A exists with ALTERNATIVE type
            var exists = await Context.ExerciseLinks
                .AnyAsync(el => el.IsActive &&
                              ((el.SourceExerciseId == sourceId && el.TargetExerciseId == targetId) ||
                               (el.SourceExerciseId == targetId && el.TargetExerciseId == sourceId)) &&
                              (el.LinkType == "ALTERNATIVE" || el.LinkTypeEnum == ExerciseLinkType.ALTERNATIVE));
            return exists;
        }
        
        // Check forward link: source → target with specified linkType
        var forwardExists = await ExistsAsync(sourceId, targetId, linkType);
        if (forwardExists)
        {
            return true;
        }
        
        // For WARMUP/COOLDOWN, check if the reverse WORKOUT link exists
        var reverseLinkType = GetReverseLinkType(linkType);
        if (reverseLinkType.HasValue)
        {
            var reverseExists = await ExistsAsync(targetId, sourceId, reverseLinkType.Value);
            return reverseExists;
        }
        
        return false;
    }
    
    /// <summary>
    /// Maps link types to their bidirectional reverse types
    /// </summary>
    private static ExerciseLinkType? GetReverseLinkType(ExerciseLinkType linkType)
    {
        return linkType switch
        {
            ExerciseLinkType.WARMUP => ExerciseLinkType.WORKOUT,
            ExerciseLinkType.COOLDOWN => ExerciseLinkType.WORKOUT,
            ExerciseLinkType.ALTERNATIVE => ExerciseLinkType.ALTERNATIVE, // ALTERNATIVE is bidirectional with itself
            ExerciseLinkType.WORKOUT => null, // WORKOUT links are only created as reverse, never as primary
            _ => null
        };
    }
    
    /// <summary>
    /// Gets links by source exercise using enum-based filtering
    /// </summary>
    public async Task<IEnumerable<ExerciseLink>> GetBySourceExerciseAsync(ExerciseId sourceId, ExerciseLinkType? linkType = null)
    {
        var query = Context.ExerciseLinks
            .Include(el => el.SourceExercise)
            .Include(el => el.TargetExercise)
            .Where(el => el.SourceExerciseId == sourceId && el.IsActive);
            
        if (linkType.HasValue)
        {
            // Use database-translatable fields instead of computed property
            query = query.Where(el => 
                (el.LinkTypeEnum != null && el.LinkTypeEnum == linkType.Value) ||
                (el.LinkTypeEnum == null && 
                 ((linkType.Value == ExerciseLinkType.WARMUP && el.LinkType == "Warmup") ||
                  (linkType.Value == ExerciseLinkType.COOLDOWN && el.LinkType == "Cooldown"))));
        }
        
        return await query
            .OrderBy(el => el.LinkTypeEnum ?? (el.LinkType == "Warmup" ? ExerciseLinkType.WARMUP : ExerciseLinkType.COOLDOWN))
            .ThenBy(el => el.DisplayOrder)
            .AsNoTracking()
            .ToListAsync();
    }
    
    /// <summary>
    /// Checks if a link exists using enum-based type matching
    /// </summary>
    public async Task<bool> ExistsAsync(ExerciseId sourceId, ExerciseId targetId, ExerciseLinkType linkType)
    {
        // For ALTERNATIVE type, use the database constraint logic - check string match
        if (linkType == ExerciseLinkType.ALTERNATIVE)
        {
            return await Context.ExerciseLinks
                .AnyAsync(el => el.SourceExerciseId == sourceId && 
                               el.TargetExerciseId == targetId && 
                               el.IsActive &&
                               el.LinkType == "ALTERNATIVE");
        }
        
        // For other types, use the original logic
        var linkTypeString = linkType.ToString();
        var legacyString = linkType switch
        {
            ExerciseLinkType.WARMUP => "Warmup",
            ExerciseLinkType.COOLDOWN => "Cooldown", 
            _ => linkTypeString
        };

        return await Context.ExerciseLinks
            .AnyAsync(el => el.SourceExerciseId == sourceId && 
                           el.TargetExerciseId == targetId && 
                           el.IsActive &&
                           (el.LinkTypeEnum == linkType ||
                            el.LinkType == linkTypeString ||
                            el.LinkType == legacyString));
    }
    
    /// <summary>
    /// Gets links by source exercise and type for display order calculation
    /// </summary>
    public async Task<IEnumerable<ExerciseLink>> GetBySourceAndTypeAsync(ExerciseId sourceId, ExerciseLinkType linkType)
    {
        return await Context.ExerciseLinks
            .Include(el => el.SourceExercise)
            .Include(el => el.TargetExercise)
            .Where(el => el.SourceExerciseId == sourceId && 
                        ((el.LinkTypeEnum != null && el.LinkTypeEnum == linkType) ||
                         (el.LinkTypeEnum == null && 
                          ((linkType == ExerciseLinkType.WARMUP && el.LinkType == "Warmup") ||
                           (linkType == ExerciseLinkType.COOLDOWN && el.LinkType == "Cooldown")))) &&
                        el.IsActive)
            .OrderBy(el => el.DisplayOrder)
            .AsNoTracking()
            .ToListAsync();
    }
}