using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Authentication.DataServices;

/// <summary>
/// Implementation of Claim query data service for read operations
/// </summary>
public class ClaimQueryDataService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider) : IClaimQueryDataService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;

    public async Task<ServiceResult<IEnumerable<ClaimInfo>>> GetClaimsByUserIdAsync(UserId userId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IClaimRepository>();
        var claims = await repository.GetClaimsByUserIdAsync(userId);
        
        var claimInfos = claims.Select(c => new ClaimInfo(
            c.Id.ToString(),
            c.ClaimType,
            c.ExpirationDate,
            c.Resource
        ));
        
        return ServiceResult<IEnumerable<ClaimInfo>>.Success(claimInfos);
    }

    public async Task<ServiceResult<BooleanResultDto>> UserHasClaimAsync(UserId userId, string claimType)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IClaimRepository>();
        var claims = await repository.GetClaimsByUserIdAsync(userId);
        
        var hasClaim = claims.Any(c => 
            c.ClaimType.Equals(claimType, StringComparison.OrdinalIgnoreCase) &&
            (c.ExpirationDate == null || c.ExpirationDate > DateTime.UtcNow));
        
        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(hasClaim));
    }
}