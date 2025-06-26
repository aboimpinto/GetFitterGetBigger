using GetFitterGetBigger.API.Models.Entities;

namespace GetFitterGetBigger.API.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}