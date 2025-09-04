using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.Enums;
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
        var entityResult = ExerciseLink.Handler.CreateNew(
            sourceId,
            targetId,
            linkDto.LinkType,
            linkDto.DisplayOrder
        );
        
        if (entityResult.IsFailure)
        {
            return ServiceResult<ExerciseLinkDto>.Failure(
                ExerciseLinkDto.Empty, 
                ServiceError.ValidationFailed(entityResult.FirstError));
        }
        
        var createdLink = await repository.AddAsync(entityResult.Value);
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
        
        // Parse primary link data
        var primarySourceId = ExerciseId.ParseOrEmpty(primaryLinkDto.SourceExerciseId);
        var primaryTargetId = ExerciseId.ParseOrEmpty(primaryLinkDto.TargetExerciseId);
        var primaryLinkTypeString = ConvertLinkTypeToEnumString(primaryLinkDto.LinkType);
        
        // TRANSACTION-AWARE DUPLICATE CHECK: Check within the same transaction
        // This addresses the transaction isolation issue where validation in separate 
        // transactions cannot see uncommitted changes
        if (Enum.TryParse<ExerciseLinkType>(primaryLinkTypeString, out var primaryLinkType))
        {
            var isDuplicate = await repository.ExistsBidirectionalAsync(primarySourceId, primaryTargetId, primaryLinkType);
            
            if (isDuplicate)
            {
                // Return a structured error that matches the validation pattern
                var error = ServiceError.ValidationFailed(ExerciseLinkErrorMessages.BidirectionalLinkExists);
                return ServiceResult<ExerciseLinkDto>.Failure(ExerciseLinkDto.Empty, error);
            }
        }
        
        var primaryLinkResult = ExerciseLink.Handler.CreateNew(
            primarySourceId,
            primaryTargetId,
            primaryLinkTypeString,
            primaryLinkDto.DisplayOrder
        );
        
        if (primaryLinkResult.IsFailure)
        {
            return ServiceResult<ExerciseLinkDto>.Failure(
                ExerciseLinkDto.Empty, 
                ServiceError.ValidationFailed(primaryLinkResult.FirstError));
        }
        
        // Create the primary link
        var createdPrimaryLink = await repository.AddAsync(primaryLinkResult.Value);
        
        // Create the reverse link if provided
        if (reverseLinkDto != null)
        {
            var reverseSourceId = ExerciseId.ParseOrEmpty(reverseLinkDto.SourceExerciseId);
            var reverseTargetId = ExerciseId.ParseOrEmpty(reverseLinkDto.TargetExerciseId);
            
            // Convert legacy link type strings to enum-compatible strings
            var reverseLinkTypeString = ConvertLinkTypeToEnumString(reverseLinkDto.LinkType);
            
            var reverseLinkResult = ExerciseLink.Handler.CreateNew(
                reverseSourceId,
                reverseTargetId,
                reverseLinkTypeString,
                reverseLinkDto.DisplayOrder
            );
            
            if (reverseLinkResult.IsFailure)
            {
                return ServiceResult<ExerciseLinkDto>.Failure(
                    ExerciseLinkDto.Empty, 
                    ServiceError.ValidationFailed(reverseLinkResult.FirstError));
            }
            
            await repository.AddAsync(reverseLinkResult.Value);
        }
        
        // Commit both operations atomically
        await unitOfWork.CommitAsync();
        
        // Return the primary link as DTO
        var dto = createdPrimaryLink.ToDto();
        return ServiceResult<ExerciseLinkDto>.Success(dto);
    }
    
    /// <summary>
    /// Converts legacy link type strings to enum-compatible strings
    /// </summary>
    private static string ConvertLinkTypeToEnumString(string linkType)
    {
        return linkType switch
        {
            "Warmup" => "WARMUP",
            "Cooldown" => "COOLDOWN", 
            "WARMUP" => "WARMUP",
            "COOLDOWN" => "COOLDOWN",
            "WORKOUT" => "WORKOUT",
            "ALTERNATIVE" => "ALTERNATIVE",
            _ => linkType // Pass through any other values
        };
    }
}