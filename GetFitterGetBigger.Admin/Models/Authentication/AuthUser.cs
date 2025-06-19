namespace GetFitterGetBigger.Admin.Models.Authentication
{
    /// <summary>
    /// Represents an authenticated user in the system
    /// </summary>
    public class AuthUser
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email address of the user
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the display name of the user
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the URL to the user's profile picture
        /// </summary>
        public string ProfilePictureUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the authentication provider (Google, Facebook, etc.)
        /// </summary>
        public string Provider { get; set; } = string.Empty;
    }
}
