using GetFitterGetBigger.Admin.Models.Authentication;

namespace GetFitterGetBigger.Admin.Tests.Builders
{
    public class AuthUserBuilder
    {
        private string _id = "test-user-id";
        private string _email = "test@example.com";
        private string _displayName = "Test User";
        private string _profilePictureUrl = "https://example.com/profile.jpg";
        private string _provider = "Google";

        public AuthUserBuilder WithId(string id)
        {
            _id = id;
            return this;
        }

        public AuthUserBuilder WithEmail(string email)
        {
            _email = email;
            return this;
        }

        public AuthUserBuilder WithDisplayName(string? displayName)
        {
            _displayName = displayName ?? string.Empty;
            return this;
        }

        public AuthUserBuilder WithProfilePictureUrl(string? url)
        {
            _profilePictureUrl = url ?? string.Empty;
            return this;
        }

        public AuthUserBuilder WithProvider(string provider)
        {
            _provider = provider;
            return this;
        }

        public AuthUser Build()
        {
            return new AuthUser
            {
                Id = _id,
                Email = _email,
                DisplayName = _displayName,
                ProfilePictureUrl = _profilePictureUrl,
                Provider = _provider
            };
        }
    }
}