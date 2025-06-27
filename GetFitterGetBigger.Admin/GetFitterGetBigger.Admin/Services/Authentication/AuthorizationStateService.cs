using GetFitterGetBigger.Admin.Models.Authentication;
using System.Security.Claims;

namespace GetFitterGetBigger.Admin.Services.Authentication
{
    public class AuthorizationStateService : IAuthorizationStateService
    {
        private readonly IAuthService _authService;
        private List<Models.Authentication.Claim> _userClaims = new();

        public bool IsReady { get; private set; }
        public bool UserHasAdminAccess => _userClaims.Any(c => c.ClaimType == "Admin-Tier");

        public event Action? OnChange;

        public AuthorizationStateService(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task InitializeAsync()
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user != null)
            {
                var request = new ClaimRequest { Email = user.Email };
                try
                {
                    var response = await _authService.GetClaimsAsync(request);
                    if (response?.Claims != null)
                    {
                        _userClaims = response.Claims;
                    }
                }
                catch (Exception)
                {
                    // Handle error, maybe log it
                    _userClaims = new List<Models.Authentication.Claim>();
                }
            }
            
            IsReady = true;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
