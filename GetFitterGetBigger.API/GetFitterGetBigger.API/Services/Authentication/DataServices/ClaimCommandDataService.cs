using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Authentication.DataServices;

/// <summary>
/// Implementation of Claim command data service for write operations
/// </summary>
public class ClaimCommandDataService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider) : IClaimCommandDataService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;

    public async Task<ServiceResult<ClaimInfo>> CreateClaimAsync(
        UserId userId, 
        string claimType, 
        DateTime? expirationDate = null, 
        string? resource = null)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IClaimRepository>();
        
        var claim = new Claim
        {
            Id = ClaimId.New(),
            UserId = userId,
            ClaimType = claimType,
            ExpirationDate = expirationDate,
            Resource = resource
        };
        
        await repository.AddClaimAsync(claim);
        await unitOfWork.CommitAsync();
        
        var claimInfo = new ClaimInfo(
            claim.Id.ToString(),
            claim.ClaimType,
            claim.ExpirationDate,
            claim.Resource
        );
        
        return ServiceResult<ClaimInfo>.Success(claimInfo);
    }

    public async Task<ServiceResult<BooleanResultDto>> RemoveClaimAsync(UserId userId, ClaimId claimId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IClaimRepository>();
        
        // For now, we'll need to implement a delete method in the repository
        // This is a placeholder - the actual implementation would depend on the repository
        
        await unitOfWork.CommitAsync();
        
        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true));
    }
}