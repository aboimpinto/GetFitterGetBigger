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
    public async Task<ServiceResult<ExerciseLinkDto>> CreateAsync(ExerciseLink exerciseLink)
    {
        using var unitOfWork = unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IExerciseLinkRepository>();
        
        var createdLink = await repository.AddAsync(exerciseLink);
        await unitOfWork.CommitAsync();
        
        var dto = createdLink.ToDto();
        
        return ServiceResult<ExerciseLinkDto>.Success(dto);
    }
    
    public async Task<ServiceResult<ExerciseLinkDto>> UpdateAsync(ExerciseLink exerciseLink)
    {
        using var unitOfWork = unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IExerciseLinkRepository>();
        
        var updatedLink = await repository.UpdateAsync(exerciseLink);
        await unitOfWork.CommitAsync();
        
        var dto = updatedLink.ToDto();
        
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
}