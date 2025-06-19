using GetFitterGetBigger.Admin.Models.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using System.Security.Claims;

namespace GetFitterGetBigger.Admin.Services.Authentication
{
    /// <summary>
    /// Implementation of the authentication service
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthService"/> class
        /// </summary>
        /// <param name="authenticationStateProvider">The authentication state provider</param>
        /// <param name="httpContextAccessor">The HTTP context accessor</param>
        public AuthService(
            AuthenticationStateProvider authenticationStateProvider,
            IHttpContextAccessor httpContextAccessor)
        {
            _authenticationStateProvider = authenticationStateProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc/>
        public async Task<AuthUser?> GetCurrentUserAsync()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            return new AuthUser
            {
                Id = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
                Email = user.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
                DisplayName = user.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
                ProfilePictureUrl = user.FindFirstValue("picture") ?? string.Empty,
                Provider = user.FindFirstValue("provider") ?? string.Empty
            };
        }

        /// <inheritdoc/>
        public async Task<bool> IsAuthenticatedAsync()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            return authState.User.Identity?.IsAuthenticated == true;
        }

        /// <inheritdoc/>
        public async Task LogoutAsync()
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                await _httpContextAccessor.HttpContext.SignOutAsync();
            }
        }

        /// <inheritdoc/>
        public Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return _authenticationStateProvider.GetAuthenticationStateAsync();
        }
    }
}
