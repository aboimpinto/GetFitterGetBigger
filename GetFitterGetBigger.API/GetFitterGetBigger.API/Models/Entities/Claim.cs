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
    }
}
