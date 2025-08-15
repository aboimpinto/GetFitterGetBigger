using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Repositories.Implementations
{
    public class ClaimRepository : RepositoryBase<FitnessDbContext>, IClaimRepository
    {
        public async Task<List<Claim>> GetClaimsByUserIdAsync(UserId userId)
        {
            return await Context.Claims
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        public async Task<Claim> AddClaimAsync(Claim claim)
        {
            await Context.Claims.AddAsync(claim);
            return claim;
        }
    }
}
