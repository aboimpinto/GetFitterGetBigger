using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Services.Commands.Authentication;

namespace GetFitterGetBigger.API.Mappers;

/// <summary>
/// Maps between web request DTOs (with string IDs) and service commands.
/// This enforces proper separation between web layer and service layer concerns.
/// </summary>
public static class AuthRequestMapper
{
    /// <summary>
    /// Maps AuthenticationRequest (web DTO) to AuthenticationCommand (service command)
    /// Always returns a valid command object, never null
    /// </summary>
    public static AuthenticationCommand ToCommand(this AuthenticationRequest request)
    {
        return new AuthenticationCommand
        {
            Email = request?.Email ?? string.Empty
        };
    }
}