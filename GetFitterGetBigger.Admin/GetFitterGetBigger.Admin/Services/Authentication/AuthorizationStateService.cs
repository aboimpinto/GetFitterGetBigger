using GetFitterGetBigger.Admin.Models.Authentication;
using System.Security.Claims;

namespace GetFitterGetBigger.Admin.Services.Authentication
{
    public class AuthorizationStateService : IAuthorizationStateService
    {
        private readonly IAuthService _authService;
        private List<Models.Authentication.Claim> _userClaims = new();

        public bool IsReady { get; private set; }
        public bool UserHasAdminAccess => _userClaims.Any(c => c.ClaimType == "Admin-Tier" || c.ClaimType == "PT-Tier");

        public event Action? OnChange;

        public AuthorizationStateService(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task InitializeAsync()
        {
            await LoadUserClaimsAsync();
            IsReady = true;
            NotifyStateChanged();
        }

        private async Task LoadUserClaimsAsync()
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                return;
            }

            _userClaims = await FetchUserClaimsAsync(user.Email);
        }

        private async Task<List<Models.Authentication.Claim>> FetchUserClaimsAsync(string email)
        {
            try
            {
                var request = new ClaimRequest { Email = email };
                var response = await _authService.GetClaimsAsync(request);
                return response?.Claims ?? new List<Models.Authentication.Claim>();
            }
            catch (Exception)
            {
                // Handle error, maybe log it
                return new List<Models.Authentication.Claim>();
            }
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
