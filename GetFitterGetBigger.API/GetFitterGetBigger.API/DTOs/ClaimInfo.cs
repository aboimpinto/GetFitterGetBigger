using System;
using GetFitterGetBigger.API.DTOs.Interfaces;

namespace GetFitterGetBigger.API.DTOs
{
    public record ClaimInfo(
        string Id, 
        string ClaimType, 
        DateTime? ExpirationDate, 
        string? Resource) : IEmptyDto<ClaimInfo>
    {
        public static ClaimInfo Empty => new(
            string.Empty,
            string.Empty,
            null,
            null);
            
        public bool IsEmpty => string.IsNullOrEmpty(Id) || Id == "claim-00000000-0000-0000-0000-000000000000";
    }
}