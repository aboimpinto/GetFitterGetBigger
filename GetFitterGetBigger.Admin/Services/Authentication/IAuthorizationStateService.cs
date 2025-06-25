namespace GetFitterGetBigger.Admin.Services.Authentication;

public interface IAuthorizationStateService
{
    bool UserHasAdminAccess { get; }
    bool IsReady { get; }
    event Action? OnChange;
    Task InitializeAsync();
}
