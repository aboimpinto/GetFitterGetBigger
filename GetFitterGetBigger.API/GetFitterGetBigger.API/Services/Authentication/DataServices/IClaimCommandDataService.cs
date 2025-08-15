using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Authentication.DataServices;

/// <summary>
/// Command data service interface for Claim write operations
/// Handles all write operations for Claim entities
/// </summary>
public interface IClaimCommandDataService
{
    /// <summary>
    /// Creates a new claim for a user
    /// </summary>
    /// <param name="userId">The user ID to create the claim for</param>
    /// <param name="claimType">The type of claim</param>
    /// <param name="expirationDate">Optional expiration date for the claim</param>
    /// <param name="resource">Optional resource associated with the claim</param>
    /// <returns>The created claim information</returns>
    Task<ServiceResult<ClaimInfo>> CreateClaimAsync(
        UserId userId, 
        string claimType, 
        DateTime? expirationDate = null, 
        string? resource = null);
    
    /// <summary>
    /// Removes a specific claim from a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="claimId">The claim ID to remove</param>
    /// <returns>Boolean result indicating success</returns>
    Task<ServiceResult<BooleanResultDto>> RemoveClaimAsync(UserId userId, ClaimId claimId);
}