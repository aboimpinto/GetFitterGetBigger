using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Authentication.Models.DTOs;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Authentication;

/// <summary>
/// Service interface for claim operations
/// </summary>
public interface IClaimService
{
    /// <summary>
    /// Creates a new claim for a user
    /// </summary>
    /// <param name="userId">The user ID to create the claim for</param>
    /// <param name="claimType">The type of claim (e.g., "Free-Tier")</param>
    /// <param name="expirationDate">Optional expiration date for the claim</param>
    /// <param name="resource">Optional resource associated with the claim</param>
    /// <returns>The created claim information</returns>
    Task<ServiceResult<ClaimInfo>> CreateClaimAsync(
        UserId userId, 
        string claimType,
        DateTime? expirationDate = null,
        string? resource = null);
    
    /// <summary>
    /// Gets all claims for a specific user
    /// </summary>
    /// <param name="userId">The user ID to get claims for</param>
    /// <returns>Collection of claim information</returns>
    Task<ServiceResult<ClaimInfoCollectionDto>> GetUserClaimsAsync(UserId userId);
    
    /// <summary>
    /// Checks if a user has a specific claim type
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="claimType">The claim type to check</param>
    /// <returns>Boolean result indicating if the user has the claim</returns>
    Task<ServiceResult<BooleanResultDto>> UserHasClaimAsync(UserId userId, string claimType);
}