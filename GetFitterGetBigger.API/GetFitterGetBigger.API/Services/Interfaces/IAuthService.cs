using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;

namespace GetFitterGetBigger.API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request);
    }
}
