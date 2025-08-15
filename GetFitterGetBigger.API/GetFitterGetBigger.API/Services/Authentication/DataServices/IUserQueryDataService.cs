using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Authentication.DataServices;

/// <summary>
/// Query data service interface for User read operations
/// Handles all read operations for User entities
/// </summary>
public interface IUserQueryDataService
{
    /// <summary>
    /// Gets a user by email address
    /// </summary>
    /// <param name="email">The email address to search for</param>
    /// <returns>User DTO or Empty if not found</returns>
    Task<ServiceResult<UserDto>> GetByEmailAsync(string email);
    
    /// <summary>
    /// Gets a user by their ID
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>User DTO or Empty if not found</returns>
    Task<ServiceResult<UserDto>> GetByIdAsync(UserId userId);
    
    /// <summary>
    /// Checks if a user exists by email
    /// </summary>
    /// <param name="email">The email address to check</param>
    /// <returns>Boolean result indicating existence</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsByEmailAsync(string email);
}