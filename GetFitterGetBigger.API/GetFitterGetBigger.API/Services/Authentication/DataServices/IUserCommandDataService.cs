using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Authentication.DataServices;

/// <summary>
/// Command data service interface for User write operations
/// Handles all write operations for User entities
/// </summary>
public interface IUserCommandDataService
{
    /// <summary>
    /// Creates a new user with associated claim in a transaction
    /// </summary>
    /// <param name="email">The user's email address</param>
    /// <param name="claimType">The initial claim type for the user</param>
    /// <returns>The created user DTO</returns>
    Task<ServiceResult<UserDto>> CreateUserWithClaimAsync(string email, string claimType);
    
    /// <summary>
    /// Creates a new user without claims
    /// </summary>
    /// <param name="email">The user's email address</param>
    /// <returns>The created user DTO</returns>
    Task<ServiceResult<UserDto>> CreateUserAsync(string email);
}