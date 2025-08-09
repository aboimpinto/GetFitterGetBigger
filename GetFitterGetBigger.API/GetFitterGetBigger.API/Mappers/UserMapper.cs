using System.Linq;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
using System;

namespace GetFitterGetBigger.API.Mappers;

/// <summary>
/// Maps between User entity and UserDto
/// </summary>
public static class UserMapper
{
    /// <summary>
    /// Maps User entity to UserDto
    /// Always returns a valid DTO object, never null
    /// </summary>
    public static UserDto ToDto(this User? user)
    {
        if (user == null)
            return UserDto.Empty;

        var claims = user.Claims?
            .Where(c => c.ExpirationDate == null || c.ExpirationDate > DateTime.UtcNow)
            .Select(c => new ClaimInfo(c.Id.ToString(), c.ClaimType, c.ExpirationDate, c.Resource))
            .ToList() ?? new List<ClaimInfo>();

        return new UserDto(
            user.Id.ToString(),
            user.Email,
            claims);
    }
}