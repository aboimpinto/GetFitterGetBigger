using System;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities
{
    public class Claim
    {
        public ClaimId Id { get; set; }
        public UserId UserId { get; set; }
        public string ClaimType { get; set; } = string.Empty;
        public DateTime? ExpirationDate { get; set; }
        public string? Resource { get; set; }
        public User User { get; set; } = null!;
        
        /// <summary>
        /// Returns an Empty Claim instance following the Empty Object Pattern
        /// </summary>
        public static Claim Empty => new()
        {
            Id = ClaimId.Empty,
            UserId = UserId.Empty,
            ClaimType = string.Empty,
            ExpirationDate = null,
            Resource = null,
            User = User.Empty
        };
        
        /// <summary>
        /// Determines if this is an empty instance
        /// </summary>
        public bool IsEmpty => Id.IsEmpty || UserId.IsEmpty;
    }
}
