using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Services.Commands.Authentication;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<AuthenticationResponse>> AuthenticateAsync(AuthenticationCommand command);
}