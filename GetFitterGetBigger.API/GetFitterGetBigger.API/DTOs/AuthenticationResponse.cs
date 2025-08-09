using System.Collections.Generic;
using GetFitterGetBigger.API.DTOs.Interfaces;

namespace GetFitterGetBigger.API.DTOs;

public record AuthenticationResponse(string Token, List<ClaimInfo> Claims) : IEmptyDto<AuthenticationResponse>
{
    public static AuthenticationResponse Empty => new(string.Empty, new List<ClaimInfo>());
    
    public bool IsEmpty => string.IsNullOrEmpty(Token) && (Claims == null || Claims.Count == 0);
}