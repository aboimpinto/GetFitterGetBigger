using System.Security.Claims;
using FluentAssertions;
using GetFitterGetBigger.Admin.Services.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Components.Authorization;
using Moq;

namespace GetFitterGetBigger.Admin.Tests.Services.Authentication
{
    public class CustomAuthStateProviderTests
    {
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly CustomAuthStateProvider _authStateProvider;

        public CustomAuthStateProviderTests()
        {
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _authStateProvider = new CustomAuthStateProvider(_httpContextAccessorMock.Object);
        }

        [Fact]
        public async Task GetAuthenticationStateAsync_WhenHttpContextIsNull_ReturnsAnonymousUser()
        {
            // Arrange
            _httpContextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns((HttpContext?)null);

            // Act
            var authState = await _authStateProvider.GetAuthenticationStateAsync();

            // Assert
            authState.Should().NotBeNull();
            authState.User.Should().NotBeNull();
            authState.User.Identity.Should().NotBeNull();
            authState.User.Identity!.IsAuthenticated.Should().BeFalse();
            authState.User.Claims.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAuthenticationStateAsync_WhenUserNotAuthenticated_ReturnsAnonymousUser()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var identity = new ClaimsIdentity(); // No authentication type = not authenticated
            httpContext.User = new ClaimsPrincipal(identity);

            _httpContextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns(httpContext);

            // Act
            var authState = await _authStateProvider.GetAuthenticationStateAsync();

            // Assert
            authState.User.Identity!.IsAuthenticated.Should().BeFalse();
        }

        [Fact]
        public async Task GetAuthenticationStateAsync_WhenUserAuthenticated_ReturnsAuthenticatedUser()
        {
            // Arrange
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user123"),
                new Claim(ClaimTypes.Email, "user@example.com"),
                new Claim(ClaimTypes.Name, "Test User")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext();
            httpContext.User = claimsPrincipal;

            _httpContextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns(httpContext);

            // Act
            var authState = await _authStateProvider.GetAuthenticationStateAsync();

            // Assert
            authState.User.Should().BeSameAs(claimsPrincipal);
            authState.User.Identity!.IsAuthenticated.Should().BeTrue();
            authState.User.Identity.Name.Should().Be("Test User");
            authState.User.Claims.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetAuthenticationStateAsync_WhenIdentityIsNull_ReturnsAnonymousUser()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(); // Principal with no identity

            _httpContextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns(httpContext);

            // Act
            var authState = await _authStateProvider.GetAuthenticationStateAsync();

            // Assert
            authState.User.Identity.Should().NotBeNull();
            authState.User.Identity!.IsAuthenticated.Should().BeFalse();
        }

        [Fact]
        public async Task GetAuthenticationStateAsync_PreservesAllUserClaims()
        {
            // Arrange
            var claims = new[]
            {
                new Claim("custom-claim", "custom-value"),
                new Claim("role", "admin"),
                new Claim("permission", "read"),
                new Claim("permission", "write")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext();
            httpContext.User = claimsPrincipal;

            _httpContextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns(httpContext);

            // Act
            var authState = await _authStateProvider.GetAuthenticationStateAsync();

            // Assert
            authState.User.Claims.Should().HaveCount(4);
            authState.User.HasClaim("custom-claim", "custom-value").Should().BeTrue();
            authState.User.HasClaim("role", "admin").Should().BeTrue();
            authState.User.Claims.Count(c => c.Type == "permission").Should().Be(2);
        }

        [Fact]
        public async Task GetAuthenticationStateAsync_WithMultipleIdentities_ReturnsAllIdentities()
        {
            // Arrange
            var identity1 = new ClaimsIdentity(new[] { new Claim("source", "identity1") }, "TestAuth1");
            var identity2 = new ClaimsIdentity(new[] { new Claim("source", "identity2") }, "TestAuth2");
            var claimsPrincipal = new ClaimsPrincipal(new[] { identity1, identity2 });

            var httpContext = new DefaultHttpContext();
            httpContext.User = claimsPrincipal;

            _httpContextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns(httpContext);

            // Act
            var authState = await _authStateProvider.GetAuthenticationStateAsync();

            // Assert
            authState.User.Identities.Should().HaveCount(2);
            authState.User.Claims.Should().HaveCount(2);
            authState.User.HasClaim("source", "identity1").Should().BeTrue();
            authState.User.HasClaim("source", "identity2").Should().BeTrue();
        }

        [Theory]
        [InlineData("Bearer")]
        [InlineData("Cookies")]
        [InlineData("CustomScheme")]
        public async Task GetAuthenticationStateAsync_WithDifferentAuthenticationTypes_ReturnsAuthenticatedUser(string authenticationType)
        {
            // Arrange
            var identity = new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, "Test User") }, 
                authenticationType);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext();
            httpContext.User = claimsPrincipal;

            _httpContextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns(httpContext);

            // Act
            var authState = await _authStateProvider.GetAuthenticationStateAsync();

            // Assert
            authState.User.Identity!.IsAuthenticated.Should().BeTrue();
            authState.User.Identity.AuthenticationType.Should().Be(authenticationType);
        }
    }
}