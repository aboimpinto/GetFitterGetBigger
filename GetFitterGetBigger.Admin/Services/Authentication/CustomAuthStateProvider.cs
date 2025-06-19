using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using System.Security.Claims;

namespace GetFitterGetBigger.Admin.Services.Authentication
{
    /// <summary>
    /// Custom authentication state provider for the application
    /// </summary>
    public class CustomAuthStateProvider : ServerAuthenticationStateProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomAuthStateProvider"/> class
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP context accessor</param>
        public CustomAuthStateProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc/>
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var identity = httpContext?.User.Identity;

            if (identity == null || !identity.IsAuthenticated)
            {
                return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            }

            return Task.FromResult(new AuthenticationState(httpContext!.User));
        }
    }
}
