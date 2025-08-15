using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Authentication.DataServices;

/// <summary>
/// Query data service interface for Claim read operations
/// Handles all read operations for Claim entities
/// </summary>
public interface IClaimQueryDataService
{
    /// <summary>
    /// Gets all claims for a specific user
    /// </summary>
    /// <param name="userId">The user ID to get claims for</param>
    /// <returns>Collection of claim information</returns>
    Task<ServiceResult<IEnumerable<ClaimInfo>>> GetClaimsByUserIdAsync(UserId userId);
    
    /// <summary>
    /// Checks if a user has a specific claim type
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="claimType">The claim type to check</param>
    /// <returns>Boolean result indicating if the user has the claim</returns>
    Task<ServiceResult<BooleanResultDto>> UserHasClaimAsync(UserId userId, string claimType);
}