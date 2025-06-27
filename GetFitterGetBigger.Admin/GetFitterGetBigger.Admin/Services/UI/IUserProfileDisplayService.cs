using GetFitterGetBigger.Admin.Models.Authentication;

namespace GetFitterGetBigger.Admin.Services.UI
{
    public interface IUserProfileDisplayService
    {
        string GetDisplayName(AuthUser user);
        string GetProfileInitial(AuthUser user);
        bool HasProfilePicture(AuthUser user);
        string GetStatusText(bool isReady);
        string GetStatusCssClass(bool isReady);
    }
}