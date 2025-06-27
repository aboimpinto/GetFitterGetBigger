using FluentAssertions;
using GetFitterGetBigger.Admin.Services.UI;
using GetFitterGetBigger.Admin.Tests.Builders;

namespace GetFitterGetBigger.Admin.Tests.Services.UI
{
    public class UserProfileDisplayServiceTests
    {
        private readonly UserProfileDisplayService _service;

        public UserProfileDisplayServiceTests()
        {
            _service = new UserProfileDisplayService();
        }

        [Theory]
        [InlineData("John Doe", "John Doe")]
        [InlineData("", "test@example.com")]
        [InlineData(null, "test@example.com")]
        public void GetDisplayName_ShouldReturnCorrectDisplayName(string? displayName, string expected)
        {
            // Arrange
            var user = new AuthUserBuilder()
                .WithDisplayName(displayName)
                .WithEmail("test@example.com")
                .Build();

            // Act
            var result = _service.GetDisplayName(user);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("John Doe", "J")]
        [InlineData("A", "A")]
        [InlineData("", "?")]
        [InlineData(null, "?")]
        public void GetProfileInitial_ShouldReturnCorrectInitial(string? displayName, string expected)
        {
            // Arrange
            var user = new AuthUserBuilder()
                .WithDisplayName(displayName)
                .Build();

            // Act
            var result = _service.GetProfileInitial(user);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("https://example.com/pic.jpg", true)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void HasProfilePicture_ShouldReturnCorrectValue(string? profilePictureUrl, bool expected)
        {
            // Arrange
            var user = new AuthUserBuilder()
                .WithProfilePictureUrl(profilePictureUrl)
                .Build();

            // Act
            var result = _service.HasProfilePicture(user);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(true, "Ready")]
        [InlineData(false, "Not Ready")]
        public void GetStatusText_ShouldReturnCorrectText(bool isReady, string expected)
        {
            // Act
            var result = _service.GetStatusText(isReady);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(true, "text-green-500")]
        [InlineData(false, "text-red-500")]
        public void GetStatusCssClass_ShouldReturnCorrectClass(bool isReady, string expected)
        {
            // Act
            var result = _service.GetStatusCssClass(isReady);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void Service_ShouldImplementInterface()
        {
            // Assert
            _service.Should().BeAssignableTo<IUserProfileDisplayService>();
        }

        [Fact]
        public void GetProfileInitial_WithMultipleWordsDisplayName_ShouldReturnFirstCharacter()
        {
            // Arrange
            var user = new AuthUserBuilder()
                .WithDisplayName("John William Doe")
                .Build();

            // Act
            var result = _service.GetProfileInitial(user);

            // Assert
            result.Should().Be("J");
        }

        [Fact]
        public void GetDisplayName_WithWhitespaceDisplayName_ShouldReturnEmail()
        {
            // Arrange
            var user = new AuthUserBuilder()
                .WithDisplayName("   ")
                .WithEmail("test@example.com")
                .Build();

            // Act
            var result = _service.GetDisplayName(user);

            // Assert
            result.Should().Be("test@example.com");
        }

        [Fact]
        public void HasProfilePicture_WithWhitespaceUrl_ShouldReturnFalse()
        {
            // Arrange
            var user = new AuthUserBuilder()
                .WithProfilePictureUrl("   ")
                .Build();

            // Act
            var result = _service.HasProfilePicture(user);

            // Assert
            result.Should().BeFalse();
        }
    }
}