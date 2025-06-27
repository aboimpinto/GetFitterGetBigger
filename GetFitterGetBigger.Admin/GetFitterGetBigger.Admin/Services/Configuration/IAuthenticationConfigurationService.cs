using Microsoft.AspNetCore.Authentication;

namespace GetFitterGetBigger.Admin.Services.Configuration
{
    public interface IAuthenticationConfigurationService
    {
        void ConfigureAuthenticationOptions(AuthenticationBuilder authBuilder, IConfiguration configuration);
    }
}