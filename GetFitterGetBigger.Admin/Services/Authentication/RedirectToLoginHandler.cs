using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Rendering;
using System.Security.Claims;

namespace GetFitterGetBigger.Admin.Services.Authentication
{
    /// <summary>
    /// Handles redirecting unauthenticated users to the login page
    /// </summary>
    public class RedirectToLoginHandler : AuthorizationHandler<AuthorizationRequirement>
    {
        private readonly NavigationManager _navigationManager;
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectToLoginHandler"/> class
        /// </summary>
        /// <param name="navigationManager">The navigation manager</param>
        /// <param name="authenticationStateProvider">The authentication state provider</param>
        public RedirectToLoginHandler(
            NavigationManager navigationManager,
            AuthenticationStateProvider authenticationStateProvider)
        {
            _navigationManager = navigationManager;
            _authenticationStateProvider = authenticationStateProvider;
        }

        /// <inheritdoc/>
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            AuthorizationRequirement requirement)
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated != true)
            {
                var returnUrl = _navigationManager.Uri;
                var encodedReturnUrl = Uri.EscapeDataString(returnUrl);
                _navigationManager.NavigateTo($"/login?returnUrl={encodedReturnUrl}", true);
            }
            else
            {
                context.Succeed(requirement);
            }
        }
    }

    /// <summary>
    /// Authorization requirement for authenticated users
    /// </summary>
    public class AuthorizationRequirement : IAuthorizationRequirement
    {
    }
}
