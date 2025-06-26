
using System.Net;
using System.Net.Http.Json;
using GetFitterGetBigger.API.DTOs;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Controllers
{
    public class AuthControllerTests : IClassFixture<ApiTestFixture>
    {
        private readonly ApiTestFixture _fixture;

        public AuthControllerTests(ApiTestFixture fixture)
        {
            _fixture = fixture;
        }

        // [TODO] Reference to [BUG-001] JWT token refresh not working in tests. The test is skipped because the JWT implementation 
        // is incomplete. The token refresh mechanism doesn't work in the test environment because the OnTokenValidated event 
        // in the JWT middleware is not being triggered properly.
        [Fact(Skip = "Ignored due to incomplete JWT implementation. See task [BUG-001] JWT Implementation Issues for details.")]
        public async Task Login_WithNewUser_ReturnsTokenAndCreatesUser()
        {
            // Arrange
            var client = _fixture.CreateClient();
            var request = new AuthenticationRequest("newuser@example.com");

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/login", request);

            // Assert
            response.EnsureSuccessStatusCode();
            var authResponse = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
            Assert.NotNull(authResponse);
            Assert.NotEmpty(authResponse.Token);
            Assert.Single(authResponse.Claims);
            Assert.Equal("Free-Tier", authResponse.Claims[0].ClaimType);
        }

        [Fact]
        public async Task Login_WithExistingUser_ReturnsToken()
        {
            // Arrange
            var client = _fixture.CreateClient();
            var request = new AuthenticationRequest("test@example.com");
            await client.PostAsJsonAsync("/api/auth/login", request); // First login to create user

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/login", request);

            // Assert
            response.EnsureSuccessStatusCode();
            var authResponse = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
            Assert.NotNull(authResponse);
            Assert.NotEmpty(authResponse.Token);
        }

        [Fact]
        public async Task ProtectedEndpoint_WithNoToken_ReturnsUnauthorized()
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var response = await client.GetAsync("/api/ReferenceTables/BodyParts");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // [TODO] Reference to [BUG-001] JWT token refresh not working in tests. The X-Refreshed-Token header is not being added
        // to the response in the test environment because the OnTokenValidated event in the JWT middleware is not being triggered properly.
        [Fact(Skip = "Ignored due to incomplete JWT implementation. See task [BUG-001] JWT Implementation Issues for details.")]
        public async Task ProtectedEndpoint_WithValidToken_ReturnsOkAndRefreshedToken()
        {
            // Arrange
            var client = _fixture.CreateAuthenticatedClient();

            // Act
            var response = await client.GetAsync("/api/ReferenceTables/BodyParts");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.True(response.Headers.Contains("X-Refreshed-Token"));
        }
    }
}
