using System.Collections.Generic;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Repositories.Interfaces
{
    public interface IClaimRepository : IRepository
    {
        Task<List<Claim>> GetClaimsByUserIdAsync(UserId userId);
        Task<Claim> AddClaimAsync(Claim claim);
    }
}
