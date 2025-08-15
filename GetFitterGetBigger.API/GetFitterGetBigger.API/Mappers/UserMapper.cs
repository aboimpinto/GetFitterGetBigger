using System.Linq;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
using System;
using System.Collections.Generic;

namespace GetFitterGetBigger.API.Mappers;

/// <summary>
/// Maps between User entity and UserDto
/// </summary>
public static class UserMapper
{
    /// <summary>
    /// Maps User entity to UserDto
    /// Always returns a valid DTO object following the Empty pattern
    /// </summary>
    public static UserDto ToDto(this User user)
    {
        // Check using IsEmpty property instead of null check
        if (user.IsEmpty)
            return UserDto.Empty;

        // Trust that Claims collection is never null (initialized in entity)
        var claims = user.Claims
            .Where(c => c.ExpirationDate == null || c.ExpirationDate > DateTime.UtcNow)
            .Select(c => new ClaimInfo(c.Id.ToString(), c.ClaimType, c.ExpirationDate, c.Resource))
            .ToList();

        return new UserDto(
            user.Id.ToString(),
            user.Email,
            claims);
    }
}