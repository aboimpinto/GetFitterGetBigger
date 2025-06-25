using System.Collections.Generic;
using System.Security.Claims;
using GetFitterGetBigger.API.Models.Entities;

namespace GetFitterGetBigger.API.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        ClaimsPrincipal? GetPrincipalFromToken(string token);
    }
}
