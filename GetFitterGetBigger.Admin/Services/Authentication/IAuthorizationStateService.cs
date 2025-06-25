using System;
using System.Threading.Tasks;

namespace GetFitterGetBigger.Admin.Services.Authentication
{
    public interface IAuthorizationStateService
    {
        bool IsReady { get; }
        string? ClaimId { get; }
        event Action? OnChange;
        Task InitializeAsync();
    }
}
