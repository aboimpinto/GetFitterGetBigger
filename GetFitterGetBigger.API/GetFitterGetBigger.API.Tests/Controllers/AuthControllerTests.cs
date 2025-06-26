using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Controllers;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _controller = new AuthController(_mockAuthService.Object);
        }

        [Fact]
        public async Task Login_WithValidRequest_ReturnsOkResult()
        {
            // Arrange
            var request = new AuthenticationRequest("test@example.com");
            var expectedResponse = new AuthenticationResponse(
                "jwt-token",
                new List<ClaimInfo>
                {
                    new ClaimInfo("claim-id", "Free-Tier", null, null)
                }
            );

            _mockAuthService
                .Setup(x => x.AuthenticateAsync(It.IsAny<AuthenticationRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AuthenticationResponse>(okResult.Value);
            Assert.Equal(expectedResponse.Token, response.Token);
            Assert.Equal(expectedResponse.Claims.Count, response.Claims.Count);
        }

        [Fact]
        public async Task Login_CallsAuthServiceWithCorrectEmail()
        {
            // Arrange
            var email = "user@example.com";
            var request = new AuthenticationRequest(email);
            var expectedResponse = new AuthenticationResponse(
                "jwt-token",
                new List<ClaimInfo>()
            );

            _mockAuthService
                .Setup(x => x.AuthenticateAsync(It.Is<AuthenticationRequest>(r => r.Email == email)))
                .ReturnsAsync(expectedResponse)
                .Verifiable();

            // Act
            await _controller.Login(request);

            // Assert
            _mockAuthService.Verify();
        }

        [Fact]
        public async Task Login_ReturnsResponseFromAuthService()
        {
            // Arrange
            var request = new AuthenticationRequest("test@example.com");
            var claims = new List<ClaimInfo>
            {
                new ClaimInfo("claim1", "Free-Tier", null, null),
                new ClaimInfo("claim2", "Admin-Tier", DateTime.UtcNow.AddDays(30), "resource1")
            };
            var expectedResponse = new AuthenticationResponse("test-token", claims);

            _mockAuthService
                .Setup(x => x.AuthenticateAsync(It.IsAny<AuthenticationRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AuthenticationResponse>(okResult.Value);
            Assert.Equal("test-token", response.Token);
            Assert.Equal(2, response.Claims.Count);
            Assert.Equal("Free-Tier", response.Claims[0].ClaimType);
            Assert.Equal("Admin-Tier", response.Claims[1].ClaimType);
        }

        [Fact]
        public async Task Login_WithNullRequest_ShouldHandleGracefully()
        {
            // Arrange
            AuthenticationRequest? request = null;
            var expectedResponse = new AuthenticationResponse("token", new List<ClaimInfo>());

            _mockAuthService
                .Setup(x => x.AuthenticateAsync(It.IsAny<AuthenticationRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act & Assert - The framework will handle null validation
            // In real scenario, ASP.NET Core model binding would reject null
            var result = await _controller.Login(request!);
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Login_WithInvalidEmail_ShouldStillCallService(string email)
        {
            // Arrange
            var request = new AuthenticationRequest(email);
            var expectedResponse = new AuthenticationResponse("token", new List<ClaimInfo>());

            _mockAuthService
                .Setup(x => x.AuthenticateAsync(It.IsAny<AuthenticationRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Login(request);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            _mockAuthService.Verify(x => x.AuthenticateAsync(It.IsAny<AuthenticationRequest>()), Times.Once);
        }

        [Fact]
        public async Task Login_WhenServiceThrowsException_ShouldPropagate()
        {
            // Arrange
            var request = new AuthenticationRequest("test@example.com");
            _mockAuthService
                .Setup(x => x.AuthenticateAsync(It.IsAny<AuthenticationRequest>()))
                .ThrowsAsync(new InvalidOperationException("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Login(request));
        }
    }
}