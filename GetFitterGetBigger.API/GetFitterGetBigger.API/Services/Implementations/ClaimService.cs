using System;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Interfaces;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Service implementation for claim operations
/// </summary>
public class ClaimService : IClaimService
{
    /// <inheritdoc/>
    public async Task<ClaimId> CreateUserClaimAsync(UserId userId, string claimType, IWritableUnitOfWork<FitnessDbContext> unitOfWork)
    {
        var claimRepository = unitOfWork.GetRepository<IClaimRepository>();
        
        var claim = new Claim
        {
            Id = ClaimId.New(),
            UserId = userId,
            ClaimType = claimType,
            ExpirationDate = null,
            Resource = null
        };
        
        await claimRepository.AddClaimAsync(claim);
        
        return claim.Id;
    }
}