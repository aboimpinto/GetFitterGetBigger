using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Authentication.DataServices;
using GetFitterGetBigger.API.Services.Authentication.Models.DTOs;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;

namespace GetFitterGetBigger.API.Services.Authentication;

/// <summary>
/// Service implementation for claim operations
/// </summary>
public class ClaimService(
    IClaimQueryDataService claimQueryDataService,
    IClaimCommandDataService claimCommandDataService,
    ILogger<ClaimService> logger) : IClaimService
{
    private readonly IClaimQueryDataService _claimQueryDataService = claimQueryDataService;
    private readonly IClaimCommandDataService _claimCommandDataService = claimCommandDataService;
    private readonly ILogger<ClaimService> _logger = logger;

    public async Task<ServiceResult<ClaimInfo>> CreateClaimAsync(
        UserId userId, 
        string claimType,
        DateTime? expirationDate = null,
        string? resource = null)
    {
        return await ServiceValidate.For<ClaimInfo>()
            .EnsureNotEmpty(userId, AuthenticationErrorMessages.Validation.UserIdCannotBeEmpty)
            .EnsureNotWhiteSpace(claimType, AuthenticationErrorMessages.Validation.ClaimTypeCannotBeEmpty)
            .MatchAsync(
                whenValid: async () => await _claimCommandDataService.CreateClaimAsync(
                    userId, 
                    claimType, 
                    expirationDate, 
                    resource)
            );
    }
    
    public async Task<ServiceResult<ClaimInfoCollectionDto>> GetUserClaimsAsync(UserId userId)
    {
        return await ServiceValidate.For<ClaimInfoCollectionDto>()
            .EnsureNotEmpty(userId, AuthenticationErrorMessages.Validation.UserIdCannotBeEmpty)
            .MatchAsync(
                whenValid: async () =>
                {
                    var result = await _claimQueryDataService.GetClaimsByUserIdAsync(userId);
                    return result.IsSuccess
                        ? ServiceResult<ClaimInfoCollectionDto>.Success(ClaimInfoCollectionDto.Create(result.Data))
                        : ServiceResult<ClaimInfoCollectionDto>.Failure(ClaimInfoCollectionDto.Empty, result.Errors);
                }
            );
    }
    
    public async Task<ServiceResult<BooleanResultDto>> UserHasClaimAsync(UserId userId, string claimType)
    {
        return await ServiceValidate.For<BooleanResultDto>()
            .EnsureNotEmpty(userId, AuthenticationErrorMessages.Validation.UserIdCannotBeEmpty)
            .EnsureNotWhiteSpace(claimType, AuthenticationErrorMessages.Validation.ClaimTypeCannotBeEmpty)
            .MatchAsync(
                whenValid: async () => await _claimQueryDataService.UserHasClaimAsync(userId, claimType)
            );
    }
}