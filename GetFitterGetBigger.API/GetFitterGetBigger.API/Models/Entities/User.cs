using System.Collections.Generic;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities
{
    public class User : IEmptyEntity<User>
    {
        public UserId Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public ICollection<Claim> Claims { get; set; } = new List<Claim>();
        public ICollection<WorkoutLog> WorkoutLogs { get; set; } = new List<WorkoutLog>();
        
        // IEntity implementation
        string IEntity.Id => Id.ToString();
        bool IEntity.IsActive => true; // Users don't have an IsActive flag in this system
        
        /// <summary>
        /// Returns an Empty User instance following the Empty Object Pattern
        /// </summary>
        public static User Empty => new()
        {
            Id = UserId.Empty,
            Email = string.Empty,
            Claims = new List<Claim>(),
            WorkoutLogs = new List<WorkoutLog>()
        };
        
        /// <summary>
        /// Determines if this is an empty instance
        /// </summary>
        public bool IsEmpty => Id.IsEmpty || string.IsNullOrEmpty(Email);
    }
}
