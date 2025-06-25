using System;

namespace GetFitterGetBigger.API.DTOs
{
    public record ClaimInfo(string ClaimId, DateTime? ExpirationDate, string? Resource);
}
