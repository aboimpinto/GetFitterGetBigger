using FluentAssertions;
using GetFitterGetBigger.Admin.Models.Authentication;
using GetFitterGetBigger.Admin.Services.Authentication;
using GetFitterGetBigger.Admin.Tests.Builders;
using Moq;

namespace GetFitterGetBigger.Admin.Tests.Services.Authentication
{
    public class AuthorizationStateServiceTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly AuthorizationStateService _authorizationStateService;

        public AuthorizationStateServiceTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _authorizationStateService = new AuthorizationStateService(_authServiceMock.Object);
        }

        [Fact]
        public async Task InitializeAsync_WhenUserAuthenticated_FetchesAndStoresClaims()
        {
            // Arrange
            var user = new AuthUserBuilder()
                .WithEmail("admin@example.com")
                .Build();

            var claims = new[]
            {
                new ClaimBuilder().AsAdminTier().Build(),
                new ClaimBuilder().WithType("Custom-Claim").Build()
            };
            var claimResponse = ClaimBuilder.BuildClaimResponse(claims);

            _authServiceMock
                .Setup(x => x.GetCurrentUserAsync())
                .ReturnsAsync(user);

            _authServiceMock
                .Setup(x => x.GetClaimsAsync(It.Is<ClaimRequest>(r => r.Email == user.Email)))
                .ReturnsAsync(claimResponse);

            var stateChangedCalled = false;
            _authorizationStateService.OnChange += () => stateChangedCalled = true;

            // Act
            await _authorizationStateService.InitializeAsync();

            // Assert
            _authorizationStateService.IsReady.Should().BeTrue();
            _authorizationStateService.UserHasAdminAccess.Should().BeTrue();
            stateChangedCalled.Should().BeTrue();

            _authServiceMock.Verify(x => x.GetClaimsAsync(It.IsAny<ClaimRequest>()), Times.Once);
        }

        [Fact]
        public async Task InitializeAsync_WhenUserNotAuthenticated_DoesNotFetchClaims()
        {
            // Arrange
            _authServiceMock
                .Setup(x => x.GetCurrentUserAsync())
                .ReturnsAsync((AuthUser?)null);

            var stateChangedCalled = false;
            _authorizationStateService.OnChange += () => stateChangedCalled = true;

            // Act
            await _authorizationStateService.InitializeAsync();

            // Assert
            _authorizationStateService.IsReady.Should().BeTrue();
            _authorizationStateService.UserHasAdminAccess.Should().BeFalse();
            stateChangedCalled.Should().BeTrue();

            _authServiceMock.Verify(x => x.GetClaimsAsync(It.IsAny<ClaimRequest>()), Times.Never);
        }

        [Fact]
        public async Task InitializeAsync_WhenClaimsFetchThrows_HandlesGracefully()
        {
            // Arrange
            var user = new AuthUserBuilder().Build();

            _authServiceMock
                .Setup(x => x.GetCurrentUserAsync())
                .ReturnsAsync(user);

            _authServiceMock
                .Setup(x => x.GetClaimsAsync(It.IsAny<ClaimRequest>()))
                .ThrowsAsync(new HttpRequestException("API Error"));

            // Act
            await _authorizationStateService.InitializeAsync();

            // Assert
            _authorizationStateService.IsReady.Should().BeTrue();
            _authorizationStateService.UserHasAdminAccess.Should().BeFalse();
        }

        [Fact]
        public async Task UserHasAdminAccess_WhenAdminTierClaimExists_ReturnsTrue()
        {
            // Arrange
            var user = new AuthUserBuilder().Build();
            var claims = new[] { new ClaimBuilder().AsAdminTier().Build() };
            var claimResponse = ClaimBuilder.BuildClaimResponse(claims);

            _authServiceMock.Setup(x => x.GetCurrentUserAsync()).ReturnsAsync(user);
            _authServiceMock.Setup(x => x.GetClaimsAsync(It.IsAny<ClaimRequest>())).ReturnsAsync(claimResponse);

            // Act
            await _authorizationStateService.InitializeAsync();

            // Assert
            _authorizationStateService.UserHasAdminAccess.Should().BeTrue();
        }

        [Fact]
        public async Task UserHasAdminAccess_WhenOnlyPTTierClaimExists_ReturnsTrue()
        {
            // Arrange
            var user = new AuthUserBuilder().Build();
            var claims = new[] { new ClaimBuilder().AsPTTier().Build() };
            var claimResponse = ClaimBuilder.BuildClaimResponse(claims);

            _authServiceMock.Setup(x => x.GetCurrentUserAsync()).ReturnsAsync(user);
            _authServiceMock.Setup(x => x.GetClaimsAsync(It.IsAny<ClaimRequest>())).ReturnsAsync(claimResponse);

            // Act
            await _authorizationStateService.InitializeAsync();

            // Assert
            _authorizationStateService.UserHasAdminAccess.Should().BeTrue();
        }

        [Fact]
        public async Task UserHasAdminAccess_WhenNoClaimsExist_ReturnsFalse()
        {
            // Arrange
            var user = new AuthUserBuilder().Build();
            var claimResponse = ClaimBuilder.BuildClaimResponse(); // Empty claims

            _authServiceMock.Setup(x => x.GetCurrentUserAsync()).ReturnsAsync(user);
            _authServiceMock.Setup(x => x.GetClaimsAsync(It.IsAny<ClaimRequest>())).ReturnsAsync(claimResponse);

            // Act
            await _authorizationStateService.InitializeAsync();

            // Assert
            _authorizationStateService.UserHasAdminAccess.Should().BeFalse();
        }

        [Fact]
        public void IsReady_BeforeInitialization_ReturnsFalse()
        {
            // Arrange & Act
            var isReady = _authorizationStateService.IsReady;

            // Assert
            isReady.Should().BeFalse();
        }

        [Fact]
        public async Task OnChange_EventFiredOnceAfterInitialization()
        {
            // Arrange
            _authServiceMock.Setup(x => x.GetCurrentUserAsync()).ReturnsAsync((AuthUser?)null);

            var callCount = 0;
            _authorizationStateService.OnChange += () => callCount++;

            // Act
            await _authorizationStateService.InitializeAsync();

            // Assert
            callCount.Should().Be(1);
        }

        [Fact]
        public async Task InitializeAsync_WhenClaimsResponseIsNull_HandlesGracefully()
        {
            // Arrange
            var user = new AuthUserBuilder().Build();

            _authServiceMock
                .Setup(x => x.GetCurrentUserAsync())
                .ReturnsAsync(user);

            _authServiceMock
                .Setup(x => x.GetClaimsAsync(It.IsAny<ClaimRequest>()))
                .ReturnsAsync((ClaimResponse?)null);

            // Act
            await _authorizationStateService.InitializeAsync();

            // Assert
            _authorizationStateService.IsReady.Should().BeTrue();
            _authorizationStateService.UserHasAdminAccess.Should().BeFalse();
        }
    }
}