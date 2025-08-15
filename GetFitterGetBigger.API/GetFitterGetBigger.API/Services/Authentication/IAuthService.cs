using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Services.Commands.Authentication;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Authentication;

/// <summary>
/// Service interface for user authentication operations
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user with their credentials
    /// </summary>
    /// <param name="command">The authentication command containing user credentials</param>
    /// <returns>Authentication response with JWT token and user claims</returns>
    Task<ServiceResult<AuthenticationResponse>> AuthenticateAsync(AuthenticationCommand command);
}