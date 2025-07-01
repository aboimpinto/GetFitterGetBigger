using System.Net;
using System.Security.Claims;
using FluentAssertions;
using GetFitterGetBigger.Admin.Models.Authentication;
using GetFitterGetBigger.Admin.Services.Authentication;
using GetFitterGetBigger.Admin.Tests.Builders;
using GetFitterGetBigger.Admin.Tests.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace GetFitterGetBigger.Admin.Tests.Services.Authentication
{
    public class AuthServiceTests
    {
        private readonly Mock<AuthenticationStateProvider> _authStateProviderMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly MockHttpMessageHandler _httpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _authStateProviderMock = new Mock<AuthenticationStateProvider>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _httpMessageHandler = new MockHttpMessageHandler();
            _httpClient = new HttpClient(_httpMessageHandler)
            {
                BaseAddress = new Uri("http://localhost:5214")
            };

            _authService = new AuthService(
                _authStateProviderMock.Object,
                _httpContextAccessorMock.Object,
                _httpClient);
        }

        [Fact]
        public async Task GetCurrentUserAsync_WhenUserAuthenticated_ReturnsAuthUser()
        {
            // Arrange
            var expectedUser = new AuthUserBuilder()
                .WithEmail("test@example.com")
                .WithDisplayName("Test User")
                .Build();

            var claims = new[]
            {
                new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, expectedUser.Id),
                new System.Security.Claims.Claim(ClaimTypes.Email, expectedUser.Email),
                new System.Security.Claims.Claim(ClaimTypes.Name, expectedUser.DisplayName),
                new System.Security.Claims.Claim("picture", expectedUser.ProfilePictureUrl),
                new System.Security.Claims.Claim("provider", expectedUser.Provider)
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            var authState = new AuthenticationState(principal);

            _authStateProviderMock
                .Setup(x => x.GetAuthenticationStateAsync())
                .ReturnsAsync(authState);

            // Act
            var result = await _authService.GetCurrentUserAsync();

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(expectedUser.Id);
            result.Email.Should().Be(expectedUser.Email);
            result.DisplayName.Should().Be(expectedUser.DisplayName);
            result.ProfilePictureUrl.Should().Be(expectedUser.ProfilePictureUrl);
            result.Provider.Should().Be(expectedUser.Provider);
        }

        [Fact]
        public async Task GetCurrentUserAsync_WhenUserNotAuthenticated_ReturnsNull()
        {
            // Arrange
            var identity = new ClaimsIdentity(); // No authentication type = not authenticated
            var principal = new ClaimsPrincipal(identity);
            var authState = new AuthenticationState(principal);

            _authStateProviderMock
                .Setup(x => x.GetAuthenticationStateAsync())
                .ReturnsAsync(authState);

            // Act
            var result = await _authService.GetCurrentUserAsync();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetCurrentUserAsync_WhenClaimsMissing_ReturnsUserWithEmptyStrings()
        {
            // Arrange
            var claims = new[]
            {
                new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, "user-id")
                // Missing other claims
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            var authState = new AuthenticationState(principal);

            _authStateProviderMock
                .Setup(x => x.GetAuthenticationStateAsync())
                .ReturnsAsync(authState);

            // Act
            var result = await _authService.GetCurrentUserAsync();

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be("user-id");
            result.Email.Should().BeEmpty();
            result.DisplayName.Should().BeEmpty();
            result.ProfilePictureUrl.Should().BeEmpty();
            result.Provider.Should().BeEmpty();
        }

        [Fact]
        public async Task IsAuthenticatedAsync_WhenUserAuthenticated_ReturnsTrue()
        {
            // Arrange
            var identity = new ClaimsIdentity("TestAuth");
            var principal = new ClaimsPrincipal(identity);
            var authState = new AuthenticationState(principal);

            _authStateProviderMock
                .Setup(x => x.GetAuthenticationStateAsync())
                .ReturnsAsync(authState);

            // Act
            var result = await _authService.IsAuthenticatedAsync();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsAuthenticatedAsync_WhenUserNotAuthenticated_ReturnsFalse()
        {
            // Arrange
            var identity = new ClaimsIdentity(); // No authentication type
            var principal = new ClaimsPrincipal(identity);
            var authState = new AuthenticationState(principal);

            _authStateProviderMock
                .Setup(x => x.GetAuthenticationStateAsync())
                .ReturnsAsync(authState);

            // Act
            var result = await _authService.IsAuthenticatedAsync();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsAuthenticatedAsync_WhenIdentityIsNull_ReturnsFalse()
        {
            // Arrange
            var principal = new ClaimsPrincipal(); // No identity
            var authState = new AuthenticationState(principal);

            _authStateProviderMock
                .Setup(x => x.GetAuthenticationStateAsync())
                .ReturnsAsync(authState);

            // Act
            var result = await _authService.IsAuthenticatedAsync();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task LogoutAsync_WhenHttpContextExists_CallsSignOutAsync()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var authServiceMock = new Mock<IAuthenticationService>();

            httpContext.RequestServices = new ServiceCollection()
                .AddSingleton(authServiceMock.Object)
                .BuildServiceProvider();

            _httpContextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns(httpContext);

            authServiceMock
                .Setup(x => x.SignOutAsync(httpContext, It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            // Act
            await _authService.LogoutAsync();

            // Assert
            authServiceMock.Verify(
                x => x.SignOutAsync(httpContext, It.IsAny<string>(), It.IsAny<AuthenticationProperties>()),
                Times.Once);
        }

        [Fact]
        public async Task LogoutAsync_WhenHttpContextIsNull_DoesNotThrow()
        {
            // Arrange
            _httpContextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns((HttpContext?)null);

            // Act
            var act = () => _authService.LogoutAsync();

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetAuthenticationStateAsync_DelegatesToAuthenticationStateProvider()
        {
            // Arrange
            var expectedAuthState = new AuthenticationState(new ClaimsPrincipal());

            _authStateProviderMock
                .Setup(x => x.GetAuthenticationStateAsync())
                .ReturnsAsync(expectedAuthState);

            // Act
            var result = await _authService.GetAuthenticationStateAsync();

            // Assert
            result.Should().BeSameAs(expectedAuthState);
            _authStateProviderMock.Verify(x => x.GetAuthenticationStateAsync(), Times.Once);
        }

        [Fact]
        public async Task GetClaimsAsync_WhenSuccessful_ReturnsClaimResponse()
        {
            // Arrange
            var request = new ClaimRequest { Email = "test@example.com" };
            var expectedClaims = new[]
            {
                new ClaimBuilder().AsAdminTier().Build(),
                new ClaimBuilder().WithType("Custom-Claim").Build()
            };
            var expectedResponse = ClaimBuilder.BuildClaimResponse(expectedClaims);

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedResponse);

            // Act
            var result = await _authService.GetClaimsAsync(request);

            // Assert
            result.Should().NotBeNull();
            result!.Claims.Should().HaveCount(2);
            result.Claims[0].ClaimType.Should().Be("Admin-Tier");
            result.Claims[1].ClaimType.Should().Be("Custom-Claim");
        }

        [Fact]
        public async Task GetClaimsAsync_WhenApiReturnsError_ThrowsHttpRequestException()
        {
            // Arrange
            var request = new ClaimRequest { Email = "test@example.com" };
            _httpMessageHandler.SetupResponse(HttpStatusCode.Unauthorized);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(
                () => _authService.GetClaimsAsync(request));
        }

        [Fact]
        public async Task GetClaimsAsync_VerifiesCorrectEndpointCalled()
        {
            // Arrange
            var request = new ClaimRequest { Email = "test@example.com" };
            var response = ClaimBuilder.BuildClaimResponse();
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, response);

            // Act
            await _authService.GetClaimsAsync(request);

            // Assert
            _httpMessageHandler.VerifyRequest(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri!.ToString().Contains("api/Auth/login"));
        }

        [Fact]
        public async Task GetClaimsAsync_WhenApiReturnsEmptyContent_ThrowsJsonException()
        {
            // Arrange
            var request = new ClaimRequest { Email = "test@example.com" };
            var emptyResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("", System.Text.Encoding.UTF8, "application/json")
            };
            _httpMessageHandler.SetupResponse(emptyResponse);

            // Act & Assert
            await Assert.ThrowsAsync<System.Text.Json.JsonException>(
                () => _authService.GetClaimsAsync(request));
        }
    }
}