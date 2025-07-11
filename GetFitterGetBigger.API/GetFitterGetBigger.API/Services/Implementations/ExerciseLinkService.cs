using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Interfaces;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Service implementation for managing exercise links
/// </summary>
public class ExerciseLinkService : IExerciseLinkService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly IExerciseService _exerciseService;
    private readonly IExerciseTypeService _exerciseTypeService;
    
    public ExerciseLinkService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        IExerciseService exerciseService,
        IExerciseTypeService exerciseTypeService)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _exerciseService = exerciseService;
        _exerciseTypeService = exerciseTypeService;
    }
    
    /// <summary>
    /// Creates a new link between exercises
    /// </summary>
    public async Task<ExerciseLinkDto> CreateLinkAsync(string sourceExerciseId, CreateExerciseLinkDto dto)
    {
        // Validate input IDs
        if (!ExerciseId.TryParse(sourceExerciseId, out var sourceId))
        {
            throw new ArgumentException($"Invalid source exercise ID format: {sourceExerciseId}");
        }
        
        if (!ExerciseId.TryParse(dto.TargetExerciseId, out var targetId))
        {
            throw new ArgumentException($"Invalid target exercise ID format: {dto.TargetExerciseId}");
        }
        
        // Validate source and target are different
        if (sourceId == targetId)
        {
            throw new ArgumentException("Cannot link an exercise to itself");
        }
        
        // Validate using ReadOnly unit of work
        using (var readOnlyUow = _unitOfWorkProvider.CreateReadOnly())
        {
            var exerciseRepo = readOnlyUow.GetRepository<IExerciseRepository>();
            var linkRepo = readOnlyUow.GetRepository<IExerciseLinkRepository>();
            
            // Get source exercise and validate it's a Workout type
            var sourceExercise = await exerciseRepo.GetByIdAsync(sourceId);
            if (sourceExercise == null || !sourceExercise.IsActive)
            {
                throw new ArgumentException($"Source exercise {sourceExerciseId} not found or inactive");
            }
            
            // Check if source exercise has Workout type
            var sourceHasWorkoutType = sourceExercise.ExerciseExerciseTypes
                .Any(eet => eet.ExerciseType?.Value == "Workout");
            
            if (!sourceHasWorkoutType)
            {
                throw new ArgumentException("Source exercise must be of type 'Workout'");
            }
            
            // Check if source has REST type (REST exercises cannot be linked)
            var sourceHasRestType = sourceExercise.ExerciseExerciseTypes
                .Any(eet => eet.ExerciseType?.Value == "Rest");
            
            if (sourceHasRestType)
            {
                throw new ArgumentException("REST exercises cannot have links");
            }
            
            // Get target exercise and validate it has the appropriate type
            var targetExercise = await exerciseRepo.GetByIdAsync(targetId);
            if (targetExercise == null || !targetExercise.IsActive)
            {
                throw new ArgumentException($"Target exercise {dto.TargetExerciseId} not found or inactive");
            }
            
            // Check if target has the matching type (Warmup or Cooldown)
            var targetHasMatchingType = targetExercise.ExerciseExerciseTypes
                .Any(eet => eet.ExerciseType?.Value == dto.LinkType);
            
            if (!targetHasMatchingType)
            {
                throw new ArgumentException($"Target exercise must be of type '{dto.LinkType}'");
            }
            
            // Check if target has REST type
            var targetHasRestType = targetExercise.ExerciseExerciseTypes
                .Any(eet => eet.ExerciseType?.Value == "Rest");
            
            if (targetHasRestType)
            {
                throw new ArgumentException("REST exercises cannot be linked");
            }
            
            // Check for duplicate link
            var linkExists = await linkRepo.ExistsAsync(sourceId, targetId, dto.LinkType);
            if (linkExists)
            {
                throw new ArgumentException($"A {dto.LinkType} link already exists between these exercises");
            }
            
            // Validate no circular reference
            var isValidNoCircularReference = await ValidateNoCircularReferenceAsync(sourceId, targetId);
            if (!isValidNoCircularReference)
            {
                throw new ArgumentException("This link would create a circular reference");
            }
            
            // Validate maximum links per type (10 warmups, 10 cooldowns)
            var existingLinks = await linkRepo.GetBySourceExerciseAsync(sourceId, dto.LinkType);
            if (existingLinks.Count() >= 10)
            {
                throw new ArgumentException($"Maximum number of {dto.LinkType} links (10) has been reached");
            }
        }
        
        // Create the link using WritableUnitOfWork
        using var writableUow = _unitOfWorkProvider.CreateWritable();
        var repository = writableUow.GetRepository<IExerciseLinkRepository>();
        
        var exerciseLink = ExerciseLink.Handler.CreateNew(
            sourceId,
            targetId,
            dto.LinkType,
            dto.DisplayOrder
        );
        
        var createdLink = await repository.AddAsync(exerciseLink);
        await writableUow.CommitAsync();
        
        return MapToDto(createdLink);
    }
    
    /// <summary>
    /// Gets all links for a specific exercise
    /// </summary>
    public async Task<ExerciseLinksResponseDto> GetLinksAsync(string exerciseId, string? linkType = null, bool includeExerciseDetails = false)
    {
        if (!ExerciseId.TryParse(exerciseId, out var parsedExerciseId))
        {
            throw new ArgumentException($"Invalid exercise ID format: {exerciseId}");
        }
        
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseLinkRepository>();
        
        var links = await repository.GetBySourceExerciseAsync(parsedExerciseId, linkType);
        
        var linkDtos = new List<ExerciseLinkDto>();
        foreach (var link in links)
        {
            var dto = MapToDto(link);
            
            if (includeExerciseDetails && link.TargetExercise != null)
            {
                // Get full exercise details from ExerciseService
                var exerciseDto = await _exerciseService.GetByIdAsync(link.TargetExerciseId);
                dto.TargetExercise = exerciseDto;
            }
            
            linkDtos.Add(dto);
        }
        
        return new ExerciseLinksResponseDto
        {
            ExerciseId = exerciseId,
            Links = linkDtos
        };
    }
    
    /// <summary>
    /// Updates an existing exercise link
    /// </summary>
    public async Task<ExerciseLinkDto> UpdateLinkAsync(string exerciseId, string linkId, UpdateExerciseLinkDto dto)
    {
        if (!ExerciseId.TryParse(exerciseId, out var parsedExerciseId))
        {
            throw new ArgumentException($"Invalid exercise ID format: {exerciseId}");
        }
        
        if (!ExerciseLinkId.TryParse(linkId, out var parsedLinkId))
        {
            throw new ArgumentException($"Invalid link ID format: {linkId}");
        }
        
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IExerciseLinkRepository>();
        
        var existingLink = await repository.GetByIdAsync(parsedLinkId);
        if (existingLink == null)
        {
            throw new ArgumentException($"Exercise link {linkId} not found");
        }
        
        if (existingLink.SourceExerciseId != parsedExerciseId)
        {
            throw new ArgumentException("Link does not belong to the specified exercise");
        }
        
        var updatedLink = ExerciseLink.Handler.Create(
            existingLink.Id,
            existingLink.SourceExerciseId,
            existingLink.TargetExerciseId,
            existingLink.LinkType,
            dto.DisplayOrder,
            dto.IsActive,
            existingLink.CreatedAt,
            DateTime.UtcNow
        );
        
        await repository.UpdateAsync(updatedLink);
        await unitOfWork.CommitAsync();
        
        return MapToDto(updatedLink);
    }
    
    /// <summary>
    /// Deletes an exercise link
    /// </summary>
    public async Task<bool> DeleteLinkAsync(string exerciseId, string linkId)
    {
        if (!ExerciseId.TryParse(exerciseId, out var parsedExerciseId))
        {
            throw new ArgumentException($"Invalid exercise ID format: {exerciseId}");
        }
        
        if (!ExerciseLinkId.TryParse(linkId, out var parsedLinkId))
        {
            throw new ArgumentException($"Invalid link ID format: {linkId}");
        }
        
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IExerciseLinkRepository>();
        
        var existingLink = await repository.GetByIdAsync(parsedLinkId);
        if (existingLink == null)
        {
            return false;
        }
        
        if (existingLink.SourceExerciseId != parsedExerciseId)
        {
            throw new ArgumentException("Link does not belong to the specified exercise");
        }
        
        var result = await repository.DeleteAsync(parsedLinkId);
        await unitOfWork.CommitAsync();
        
        return result;
    }
    
    /// <summary>
    /// Gets suggested links based on common usage patterns
    /// </summary>
    public async Task<List<ExerciseLinkDto>> GetSuggestedLinksAsync(string exerciseId, int count = 5)
    {
        if (!ExerciseId.TryParse(exerciseId, out var parsedExerciseId))
        {
            throw new ArgumentException($"Invalid exercise ID format: {exerciseId}");
        }
        
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseLinkRepository>();
        
        // For now, return the most commonly used links
        // In the future, this could be enhanced with ML-based recommendations
        var mostUsedLinks = await repository.GetMostUsedLinksAsync(count);
        
        return mostUsedLinks
            .Select(tuple => MapToDto(tuple.link))
            .ToList();
    }
    
    /// <summary>
    /// Validates that no circular reference would be created
    /// </summary>
    public async Task<bool> ValidateNoCircularReferenceAsync(ExerciseId sourceId, ExerciseId targetId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseLinkRepository>();
        
        // Check direct circular reference (target links back to source)
        var reverseLinks = await repository.GetBySourceExerciseAsync(targetId);
        if (reverseLinks.Any(link => link.TargetExerciseId == sourceId))
        {
            return false;
        }
        
        // Check indirect circular references using depth-first search
        var visited = new HashSet<ExerciseId>();
        return await CheckNoCircularReferenceRecursive(repository, targetId, sourceId, visited);
    }
    
    private async Task<bool> CheckNoCircularReferenceRecursive(
        IExerciseLinkRepository repository,
        ExerciseId currentId,
        ExerciseId originalSourceId,
        HashSet<ExerciseId> visited)
    {
        if (visited.Contains(currentId))
        {
            return true; // Already checked this node
        }
        
        visited.Add(currentId);
        
        var links = await repository.GetBySourceExerciseAsync(currentId);
        foreach (var link in links)
        {
            if (link.TargetExerciseId == originalSourceId)
            {
                return false; // Found circular reference
            }
            
            var isValid = await CheckNoCircularReferenceRecursive(
                repository,
                link.TargetExerciseId,
                originalSourceId,
                visited);
                
            if (!isValid)
            {
                return false;
            }
        }
        
        return true;
    }
    
    private static ExerciseLinkDto MapToDto(ExerciseLink exerciseLink)
    {
        return new ExerciseLinkDto
        {
            Id = exerciseLink.Id.ToString(),
            TargetExerciseId = exerciseLink.TargetExerciseId.ToString(),
            LinkType = exerciseLink.LinkType,
            DisplayOrder = exerciseLink.DisplayOrder,
            IsActive = exerciseLink.IsActive
        };
    }
}