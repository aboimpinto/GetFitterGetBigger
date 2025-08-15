using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.DTOs.Interfaces;

namespace GetFitterGetBigger.API.Services.Authentication.Models.DTOs;

/// <summary>
/// Wrapper DTO for collections of ClaimInfo to maintain consistency with Empty pattern
/// </summary>
public class ClaimInfoCollectionDto : IEmptyDto<ClaimInfoCollectionDto>
{
    /// <summary>
    /// The collection of claim information
    /// </summary>
    public IEnumerable<ClaimInfo> Claims { get; set; } = [];
    
    /// <summary>
    /// A collection is considered empty if it has no items
    /// </summary>
    public bool IsEmpty => !Claims.Any();
    
    /// <summary>
    /// Returns an Empty instance with no claims
    /// </summary>
    public static ClaimInfoCollectionDto Empty => new() { Claims = [] };
    
    /// <summary>
    /// Creates a new ClaimInfoCollectionDto with the specified claims
    /// </summary>
    public static ClaimInfoCollectionDto Create(IEnumerable<ClaimInfo> claims) => new() { Claims = claims };
}