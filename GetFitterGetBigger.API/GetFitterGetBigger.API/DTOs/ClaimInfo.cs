using System;

namespace GetFitterGetBigger.API.DTOs
{
    public record ClaimInfo(string Id, string ClaimType, DateTime? ExpirationDate, string? Resource);
}