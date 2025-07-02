using GetFitterGetBigger.Admin.Models.Authentication;

namespace GetFitterGetBigger.Admin.Services.UI
{
    public class UserProfileDisplayService : IUserProfileDisplayService
    {
        public string GetDisplayName(AuthUser user)
        {
            return string.IsNullOrWhiteSpace(user.DisplayName) ? user.Email : user.DisplayName;
        }

        public string GetProfileInitial(AuthUser user)
        {
            if (!string.IsNullOrWhiteSpace(user.DisplayName))
            {
                return user.DisplayName[0].ToString();
            }

            return "?";
        }

        public bool HasProfilePicture(AuthUser user)
        {
            return !string.IsNullOrWhiteSpace(user.ProfilePictureUrl);
        }

        public string GetStatusText(bool isReady)
        {
            return isReady ? "Ready" : "Not Ready";
        }

        public string GetStatusCssClass(bool isReady)
        {
            return isReady ? "text-green-500" : "text-red-500";
        }
    }
}