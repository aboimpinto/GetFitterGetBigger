using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.Extensions;
using GetFitterGetBigger.API.Services.Results;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Exercise.Features.Links.DataServices;

/// <summary>
/// Data service implementation for ExerciseLink write operations.
/// Encapsulates all database modifications and entity-to-DTO mapping.
/// </summary>
public class ExerciseLinkCommandDataService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider) : IExerciseLinkCommandDataService
{
    public async Task<ServiceResult<ExerciseLinkDto>> CreateAsync(ExerciseLinkDto linkDto)
    {
        using var unitOfWork = unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IExerciseLinkRepository>();
        
        // Parse IDs from the DTO
        var sourceId = ExerciseId.ParseOrEmpty(linkDto.SourceExerciseId);
        var targetId = ExerciseId.ParseOrEmpty(linkDto.TargetExerciseId);
        
        // Create the entity internally
        var exerciseLink = ExerciseLink.Handler.CreateNew(
            sourceId,
            targetId,
            linkDto.LinkType,
            linkDto.DisplayOrder
        );
        
        var createdLink = await repository.AddAsync(exerciseLink);
        await unitOfWork.CommitAsync();
        
        var dto = createdLink.ToDto();
        
        return ServiceResult<ExerciseLinkDto>.Success(dto);
    }
    
    public async Task<ServiceResult<ExerciseLinkDto>> UpdateAsync(
        ExerciseLinkId linkId,
        Func<ExerciseLink, ExerciseLink> updateAction)
    {
        // TRUST THE INFRASTRUCTURE! All validation has been done at the service layer
        using var unitOfWork = unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IExerciseLinkRepository>();
        
        // Get the existing entity (we trust it exists) - SINGLE DATABASE CALL
        var existingLink = await repository.GetByIdAsync(linkId);
        
        // Apply the transformation function provided by the service
        var updatedLink = updateAction(existingLink);
        
        // Save the updated entity
        var result = await repository.UpdateAsync(updatedLink);
        await unitOfWork.CommitAsync();
        
        var dto = result.ToDto();
        
        return ServiceResult<ExerciseLinkDto>.Success(dto);
    }
    
    public async Task<ServiceResult<BooleanResultDto>> DeleteAsync(ExerciseLinkId id)
    {
        using var unitOfWork = unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IExerciseLinkRepository>();
        
        var result = await repository.DeleteAsync(id);
        await unitOfWork.CommitAsync();
        
        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(result));
    }
    
    public async Task<ServiceResult<ExerciseLinkDto>> CreateBidirectionalAsync(
        ExerciseLinkDto primaryLinkDto, 
        ExerciseLinkDto? reverseLinkDto = null)
    {
        using var unitOfWork = unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IExerciseLinkRepository>();
        
        // Create the primary link entity from DTO
        var primarySourceId = ExerciseId.ParseOrEmpty(primaryLinkDto.SourceExerciseId);
        var primaryTargetId = ExerciseId.ParseOrEmpty(primaryLinkDto.TargetExerciseId);
        
        var primaryLink = ExerciseLink.Handler.CreateNew(
            primarySourceId,
            primaryTargetId,
            primaryLinkDto.LinkType,
            primaryLinkDto.DisplayOrder
        );
        
        // Create the primary link
        var createdPrimaryLink = await repository.AddAsync(primaryLink);
        
        // Create the reverse link if provided
        if (reverseLinkDto != null)
        {
            var reverseSourceId = ExerciseId.ParseOrEmpty(reverseLinkDto.SourceExerciseId);
            var reverseTargetId = ExerciseId.ParseOrEmpty(reverseLinkDto.TargetExerciseId);
            
            var reverseLink = ExerciseLink.Handler.CreateNew(
                reverseSourceId,
                reverseTargetId,
                reverseLinkDto.LinkType,
                reverseLinkDto.DisplayOrder
            );
            
            await repository.AddAsync(reverseLink);
        }
        
        // Commit both operations atomically
        await unitOfWork.CommitAsync();
        
        // Return the primary link as DTO
        var dto = createdPrimaryLink.ToDto();
        return ServiceResult<ExerciseLinkDto>.Success(dto);
    }
}