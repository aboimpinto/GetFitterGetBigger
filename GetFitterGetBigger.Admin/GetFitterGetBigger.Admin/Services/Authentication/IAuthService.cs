using GetFitterGetBigger.Admin.Models.Authentication;
using Microsoft.AspNetCore.Components.Authorization;

namespace GetFitterGetBigger.Admin.Services.Authentication
{
    /// <summary>
    /// Interface for authentication services
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Gets the current authenticated user
        /// </summary>
        /// <returns>The authenticated user or null if not authenticated</returns>
        Task<AuthUser?> GetCurrentUserAsync();

        /// <summary>
        /// Checks if the user is authenticated
        /// </summary>
        /// <returns>True if the user is authenticated, false otherwise</returns>
        Task<bool> IsAuthenticatedAsync();

        /// <summary>
        /// Logs the user out
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        Task LogoutAsync();

        /// <summary>
        /// Gets the authentication state
        /// </summary>
        /// <returns>The authentication state</returns>
        Task<AuthenticationState> GetAuthenticationStateAsync();

        /// <summary>
        /// Gets the claims from the server
        /// </summary>
        /// <param name="request">The request containing the user's email</param>
        /// <returns>The claims response</returns>
        Task<ClaimResponse?> GetClaimsAsync(ClaimRequest request);
    }
}
