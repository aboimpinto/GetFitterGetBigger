using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GetFitterGetBigger.Admin.Controllers
{
    /// <summary>
    /// Controller for handling authentication
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class
        /// </summary>
        /// <param name="logger">The logger</param>
        public AuthController(ILogger<AuthController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Initiates the login process with the specified provider
        /// </summary>
        /// <param name="provider">The authentication provider (google, facebook)</param>
        /// <param name="returnUrl">The URL to return to after authentication</param>
        /// <returns>The challenge result</returns>
        [HttpGet("login/{provider}")]
        public IActionResult Login(string provider, [FromQuery] string? returnUrl = null)
        {
            if (string.IsNullOrEmpty(provider))
            {
                return BadRequest("Provider is required");
            }

            returnUrl ??= Url.Content("~/");

            // Configure the redirect URL after external login
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(Callback), new { returnUrl }),
                Items =
                {
                    { "scheme", provider },
                    { "returnUrl", returnUrl }
                }
            };

            return Challenge(properties, provider);
        }

        /// <summary>
        /// Handles the callback from the authentication provider
        /// </summary>
        /// <param name="returnUrl">The URL to return to after authentication</param>
        /// <returns>A redirect to the return URL</returns>
        [HttpGet("callback")]
        public async Task<IActionResult> Callback(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!authenticateResult.Succeeded)
            {
                _logger.LogError("External authentication error: {Error}", authenticateResult.Failure);
                return Redirect("/login");
            }

            // Get the external login information
            var externalLoginInfo = await HttpContext.AuthenticateAsync(authenticateResult.Properties?.Items["scheme"] ?? string.Empty);
            if (!externalLoginInfo.Succeeded)
            {
                _logger.LogError("External authentication error: {Error}", externalLoginInfo.Failure);
                return Redirect("/login");
            }

            // Extract claims from the external login
            var claims = new List<Claim>();
            var nameIdentifier = externalLoginInfo.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = externalLoginInfo.Principal.FindFirstValue(ClaimTypes.Email);
            var name = externalLoginInfo.Principal.FindFirstValue(ClaimTypes.Name);
            var picture = externalLoginInfo.Principal.FindFirstValue("picture");
            var provider = authenticateResult.Properties?.Items["scheme"];

            if (!string.IsNullOrEmpty(nameIdentifier))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, nameIdentifier));
            }

            if (!string.IsNullOrEmpty(email))
            {
                claims.Add(new Claim(ClaimTypes.Email, email));
            }

            if (!string.IsNullOrEmpty(name))
            {
                claims.Add(new Claim(ClaimTypes.Name, name));
            }

            if (!string.IsNullOrEmpty(picture))
            {
                claims.Add(new Claim("picture", picture));
            }

            if (!string.IsNullOrEmpty(provider))
            {
                claims.Add(new Claim("provider", provider));
            }

            // Create the identity and sign in
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return Redirect(returnUrl);
        }

        /// <summary>
        /// Logs the user out
        /// </summary>
        /// <returns>A redirect to the login page</returns>
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/login");
        }
    }
}
