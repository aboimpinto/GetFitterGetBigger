using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.Enums;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.Commands;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.Extensions;
using GetFitterGetBigger.API.Services.Results;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Exercise.Features.Links.DataServices;

/// <summary>
/// Data service implementation for ExerciseLink read operations.
/// Encapsulates all database queries and entity-to-DTO mapping.
/// </summary>
public class ExerciseLinkQueryDataService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    IExerciseService exerciseService) : IExerciseLinkQueryDataService
{
    public async Task<ServiceResult<ExerciseLinksResponseDto>> GetLinksAsync(GetExerciseLinksCommand command)
    {
        var exerciseId = ExerciseId.ParseOrEmpty(command.ExerciseId);
        if (exerciseId.IsEmpty)
        {
            return ServiceResult<ExerciseLinksResponseDto>.Success(ExerciseLinksResponseDto.Empty);
        }
        
        using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseLinkRepository>();
        
        var links = await repository.GetBySourceExerciseAsync(exerciseId, command.LinkType);
        
        var linkDtos = new List<ExerciseLinkDto>();
        foreach (var link in links)
        {
            var dto = link.ToDto();
            
            if (command.IncludeExerciseDetails && link.TargetExercise != null)
            {
                var exerciseResult = await exerciseService.GetByIdAsync(link.TargetExerciseId);
                dto.TargetExercise = exerciseResult.Data;
            }
            
            linkDtos.Add(dto);
        }
        
        var response = new ExerciseLinksResponseDto
        {
            ExerciseId = command.ExerciseId,
            Links = linkDtos
        };
        
        return ServiceResult<ExerciseLinksResponseDto>.Success(response);
    }
    
    public async Task<ServiceResult<ExerciseLinkDto>> GetByIdAsync(ExerciseLinkId id)
    {
        using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseLinkRepository>();
        
        var link = await repository.GetByIdAsync(id);
        var dto = link.IsEmpty ? ExerciseLinkDto.Empty : link.ToDto();
        
        return ServiceResult<ExerciseLinkDto>.Success(dto);
    }
    
    public async Task<ServiceResult<ExerciseLink>> GetEntityByIdAsync(ExerciseLinkId id)
    {
        using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseLinkRepository>();
        
        var link = await repository.GetByIdAsync(id);
        
        return ServiceResult<ExerciseLink>.Success(link);
    }
    
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(ExerciseId sourceId, ExerciseId targetId, string linkType)
    {
        using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseLinkRepository>();
        
        var exists = await repository.ExistsAsync(sourceId, targetId, linkType);
        
        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(exists));
    }
    
    public async Task<ServiceResult<List<ExerciseLinkDto>>> GetBySourceExerciseAsync(ExerciseId sourceId, string? linkType = null)
    {
        using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseLinkRepository>();
        
        var links = await repository.GetBySourceExerciseAsync(sourceId, linkType);
        var dtos = links.Select(link => link.ToDto()).ToList();
        
        return ServiceResult<List<ExerciseLinkDto>>.Success(dtos);
    }
    
    public async Task<ServiceResult<int>> GetLinkCountAsync(ExerciseId sourceId, string linkType)
    {
        using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseLinkRepository>();
        
        var links = await repository.GetBySourceExerciseAsync(sourceId, linkType);
        var count = links.Count();
        
        return ServiceResult<int>.Success(count);
    }
    
    public async Task<ServiceResult<List<ExerciseLinkDto>>> GetSuggestedLinksAsync(string exerciseId, int count)
    {
        var parsedExerciseId = ExerciseId.ParseOrEmpty(exerciseId);
        if (parsedExerciseId.IsEmpty)
        {
            return ServiceResult<List<ExerciseLinkDto>>.Success(new List<ExerciseLinkDto>());
        }
        
        using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseLinkRepository>();
        
        var mostUsedLinks = await repository.GetMostUsedLinksAsync(count);
        var dtos = mostUsedLinks
            .Select(tuple => tuple.link.ToDto())
            .ToList();
        
        return ServiceResult<List<ExerciseLinkDto>>.Success(dtos);
    }
    
    // ===== ENHANCED BIDIRECTIONAL QUERY METHODS IMPLEMENTATION =====
    
    public async Task<ServiceResult<List<ExerciseLinkDto>>> GetByTargetExerciseAsync(ExerciseId targetExerciseId)
    {
        using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseLinkRepository>();
        
        var links = await repository.GetByTargetExerciseAsync(targetExerciseId);
        var dtos = links.Select(link => link.ToDto()).ToList();
        
        return ServiceResult<List<ExerciseLinkDto>>.Success(dtos);
    }
    
    public async Task<ServiceResult<List<ExerciseLinkDto>>> GetBidirectionalLinksAsync(
        ExerciseId exerciseId, 
        ExerciseLinkType linkType)
    {
        using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseLinkRepository>();
        
        var links = await repository.GetBidirectionalLinksAsync(exerciseId, linkType);
        var dtos = links.Select(link => link.ToDto()).ToList();
        
        return ServiceResult<List<ExerciseLinkDto>>.Success(dtos);
    }
    
    public async Task<ServiceResult<BooleanResultDto>> ExistsBidirectionalAsync(
        ExerciseId sourceId, 
        ExerciseId targetId, 
        ExerciseLinkType linkType)
    {
        using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseLinkRepository>();
        
        var exists = await repository.ExistsBidirectionalAsync(sourceId, targetId, linkType);
        
        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(exists));
    }
    
    public async Task<ServiceResult<List<ExerciseLinkDto>>> GetBySourceExerciseWithEnumAsync(ExerciseId sourceId, ExerciseLinkType? linkType = null)
    {
        using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseLinkRepository>();
        
        var links = await repository.GetBySourceExerciseAsync(sourceId, linkType);
        var dtos = links.Select(link => link.ToDto()).ToList();
        
        return ServiceResult<List<ExerciseLinkDto>>.Success(dtos);
    }
    
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(ExerciseId sourceId, ExerciseId targetId, ExerciseLinkType linkType)
    {
        using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseLinkRepository>();
        
        var exists = await repository.ExistsAsync(sourceId, targetId, linkType);
        
        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(exists));
    }
    
    public async Task<ServiceResult<List<ExerciseLinkDto>>> GetBySourceAndTypeAsync(
        ExerciseId sourceId, 
        ExerciseLinkType linkType)
    {
        using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseLinkRepository>();
        
        var links = await repository.GetBySourceAndTypeAsync(sourceId, linkType);
        var dtos = links.Select(link => link.ToDto()).ToList();
        
        return ServiceResult<List<ExerciseLinkDto>>.Success(dtos);
    }
}