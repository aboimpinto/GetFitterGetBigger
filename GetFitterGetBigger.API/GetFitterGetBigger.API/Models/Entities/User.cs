using System.Collections.Generic;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities
{
    public class User
    {
        public UserId Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public ICollection<Claim> Claims { get; set; } = new List<Claim>();
        public ICollection<WorkoutLog> WorkoutLogs { get; set; } = new List<WorkoutLog>();
    }
}
