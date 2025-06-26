using System.Collections.Generic;

namespace GetFitterGetBigger.API.DTOs
{
    public record AuthenticationResponse(string Token, List<ClaimInfo> Claims);
}