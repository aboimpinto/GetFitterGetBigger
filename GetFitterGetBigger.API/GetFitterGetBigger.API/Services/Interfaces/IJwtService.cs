using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;

namespace GetFitterGetBigger.API.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        string GenerateToken(UserDto userDto);
    }
}