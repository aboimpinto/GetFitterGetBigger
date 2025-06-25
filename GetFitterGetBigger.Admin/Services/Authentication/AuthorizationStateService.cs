using GetFitterGetBigger.Admin.Models.Authentication;
using System;
using System.Threading.Tasks;

namespace GetFitterGetBigger.Admin.Services.Authentication
{
    public class AuthorizationStateService : IAuthorizationStateService
    {
        private readonly IAuthService _authService;
        public bool IsReady { get; private set; }
        public string? ClaimId { get; private set; }
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
                    if (response?.Claims?.Count > 0)
                    {
                        IsReady = true;
                        // Taking the first claim as the primary one.
                        ClaimId = response.Claims[0].ClaimId;
                    }
                    else
                    {
                        IsReady = false;
                        ClaimId = string.Empty;
                    }
                    NotifyStateChanged();
                }
                catch (Exception)
                {
                    // Handle error, maybe log it
                    IsReady = false;
                    ClaimId = string.Empty;
                    NotifyStateChanged();
                }
            }
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}