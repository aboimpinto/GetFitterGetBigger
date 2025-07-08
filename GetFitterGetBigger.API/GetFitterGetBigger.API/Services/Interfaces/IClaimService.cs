using System.Threading.Tasks;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Interfaces;

/// <summary>
/// Service interface for claim operations
/// </summary>
public interface IClaimService
{
    /// <summary>
    /// Creates a new claim for a user using the provided unit of work
    /// </summary>
    /// <param name="userId">The user ID to create the claim for</param>
    /// <param name="claimType">The type of claim (e.g., "Free-Tier")</param>
    /// <param name="unitOfWork">The writable unit of work to use for the operation</param>
    /// <returns>The created claim ID</returns>
    Task<ClaimId> CreateUserClaimAsync(UserId userId, string claimType, IWritableUnitOfWork<Models.FitnessDbContext> unitOfWork);
}