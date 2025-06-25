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
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthService"/> class
        /// </summary>
        /// <param name="authenticationStateProvider">The authentication state provider</param>
        /// <param name="httpContextAccessor">The HTTP context accessor</param>
        /// <param name="httpClient">The HTTP client</param>
        public AuthService(
            AuthenticationStateProvider authenticationStateProvider,
            IHttpContextAccessor httpContextAccessor,
            HttpClient httpClient)
        {
            _authenticationStateProvider = authenticationStateProvider;
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClient;
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

        /// <inheritdoc/>
        public async Task<ClaimResponse?> GetClaimsAsync(ClaimRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/login", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ClaimResponse>();
        }
    }
}
