using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities;

public record User
{
    public UserId Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    
    // Navigation properties
    public ICollection<WorkoutLog> WorkoutLogs { get; init; } = new List<WorkoutLog>();
    
    private User() { }
    
    public static class Handler
    {
        public static User CreateNew(string username, string email)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException("Username cannot be empty", nameof(username));
            }
            
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("Email cannot be empty", nameof(email));
            }
            
            return new()
            {
                Id = UserId.New(),
                Username = username,
                Email = email
            };
        }
        
        public static User Create(UserId id, string username, string email) =>
            new()
            {
                Id = id,
                Username = username,
                Email = email
            };
    }
}
