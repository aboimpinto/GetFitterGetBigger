using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Security.Claims;

namespace GetFitterGetBigger.Admin.Services.Configuration
{
    public class AuthenticationConfigurationService : IAuthenticationConfigurationService
    {
        public void ConfigureAuthenticationOptions(AuthenticationBuilder authBuilder, IConfiguration configuration)
        {
            authBuilder
                .AddCookie(ConfigureCookieOptions)
                .AddGoogle(options => ConfigureGoogleOptions(options, configuration))
                .AddFacebook(options => ConfigureFacebookOptions(options, configuration));
        }

        private void ConfigureCookieOptions(CookieAuthenticationOptions options)
        {
            options.LoginPath = "/login";
            options.LogoutPath = "/api/auth/logout";
        }

        private void ConfigureGoogleOptions(GoogleOptions options, IConfiguration configuration)
        {
            options.ClientId = configuration["Authentication:Google:ClientId"] ?? "YOUR_GOOGLE_CLIENT_ID";
            options.ClientSecret = configuration["Authentication:Google:ClientSecret"] ?? "YOUR_GOOGLE_CLIENT_SECRET";
            options.SaveTokens = true;
            options.Scope.Add("profile");
            options.Scope.Add("email");
            options.Events.OnCreatingTicket = HandleGoogleProfilePicture;
        }

        private void ConfigureFacebookOptions(FacebookOptions options, IConfiguration configuration)
        {
            options.AppId = configuration["Authentication:Facebook:AppId"] ?? "YOUR_FACEBOOK_APP_ID";
            options.AppSecret = configuration["Authentication:Facebook:AppSecret"] ?? "YOUR_FACEBOOK_APP_SECRET";
            options.SaveTokens = true;
            options.Scope.Add("email");
            options.Scope.Add("public_profile");
            options.Events.OnCreatingTicket = HandleFacebookProfilePicture;
        }

        private Task HandleGoogleProfilePicture(OAuthCreatingTicketContext context)
        {
            if (context.User.TryGetProperty("picture", out var picture))
            {
                context.Identity?.AddClaim(new Claim("picture", picture.ToString()));
            }
            return Task.CompletedTask;
        }

        private Task HandleFacebookProfilePicture(OAuthCreatingTicketContext context)
        {
            var id = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(id))
            {
                var pictureUrl = $"https://graph.facebook.com/{id}/picture?type=normal";
                context.Identity?.AddClaim(new Claim("picture", pictureUrl));
            }
            return Task.CompletedTask;
        }
    }
}