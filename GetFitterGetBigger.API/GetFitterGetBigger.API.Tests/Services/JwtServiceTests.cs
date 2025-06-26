using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services
{
    public class JwtServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly JwtService _jwtService;
        private const string TestKey = "this_is_a_test_key_that_is_at_least_32_characters_long";
        private const string TestIssuer = "TestIssuer";
        private const string TestAudience = "TestAudience";

        public JwtServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            
            _mockConfiguration
                .Setup(x => x["Jwt:Key"])
                .Returns(TestKey);
            
            _mockConfiguration
                .Setup(x => x["Jwt:Issuer"])
                .Returns(TestIssuer);
            
            _mockConfiguration
                .Setup(x => x["Jwt:Audience"])
                .Returns(TestAudience);

            _jwtService = new JwtService(_mockConfiguration.Object);
        }

        [Fact]
        public void GenerateToken_WithValidUser_ReturnsValidJwtToken()
        {
            // Arrange
            var userId = UserId.New();
            var user = new User
            {
                Id = userId,
                Email = "test@example.com"
            };

            // Act
            var token = _jwtService.GenerateToken(user);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);
            
            // Verify it's a valid JWT format (3 parts separated by dots)
            var parts = token.Split('.');
            Assert.Equal(3, parts.Length);
        }

        [Fact]
        public void GenerateToken_IncludesUserIdAndEmailClaims()
        {
            // Arrange
            var userId = UserId.New();
            var email = "test@example.com";
            var user = new User
            {
                Id = userId,
                Email = email
            };

            // Act
            var token = _jwtService.GenerateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var userIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "nameid");
            Assert.NotNull(userIdClaim);
            Assert.Equal(userId.ToString(), userIdClaim.Value);

            var emailClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == "email");
            Assert.NotNull(emailClaim);
            Assert.Equal(email, emailClaim.Value);
        }

        [Fact]
        public void GenerateToken_SetsCorrectIssuerAndAudience()
        {
            // Arrange
            var user = new User
            {
                Id = UserId.New(),
                Email = "test@example.com"
            };

            // Act
            var token = _jwtService.GenerateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            Assert.Equal(TestIssuer, jwt.Issuer);
            Assert.Contains(TestAudience, jwt.Audiences);
        }

        [Fact]
        public void GenerateToken_SetsExpirationToOneHour()
        {
            // Arrange
            var user = new User
            {
                Id = UserId.New(),
                Email = "test@example.com"
            };
            var beforeGeneration = DateTime.UtcNow;

            // Act
            var token = _jwtService.GenerateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var expiration = jwt.ValidTo;
            
            // Allow for some time variance during test execution
            Assert.True(expiration > beforeGeneration.AddMinutes(59));
            Assert.True(expiration < beforeGeneration.AddMinutes(61));
        }

        [Fact]
        public void GenerateToken_ThrowsWhenKeyNotConfigured()
        {
            // Arrange
            var mockConfig = new Mock<IConfiguration>();
            mockConfig
                .Setup(x => x["Jwt:Key"])
                .Returns((string?)null);

            var service = new JwtService(mockConfig.Object);
            var user = new User
            {
                Id = UserId.New(),
                Email = "test@example.com"
            };

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => service.GenerateToken(user));
            Assert.Equal("JWT Key not configured.", exception.Message);
        }

        [Fact]
        public void GenerateToken_UsesHmacSha256Signature()
        {
            // Arrange
            var user = new User
            {
                Id = UserId.New(),
                Email = "test@example.com"
            };

            // Act
            var token = _jwtService.GenerateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            
            Assert.Equal("HS256", jwt.Header.Alg);
        }

        [Fact]
        public void GenerateToken_WithNullIssuer_StillGeneratesValidToken()
        {
            // Arrange
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(x => x["Jwt:Key"]).Returns(TestKey);
            mockConfig.Setup(x => x["Jwt:Issuer"]).Returns((string?)null);
            mockConfig.Setup(x => x["Jwt:Audience"]).Returns(TestAudience);

            var service = new JwtService(mockConfig.Object);
            var user = new User
            {
                Id = UserId.New(),
                Email = "test@example.com"
            };

            // Act
            var token = service.GenerateToken(user);

            // Assert
            Assert.NotNull(token);
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            Assert.Null(jwt.Issuer);
        }

        [Fact]
        public void GenerateToken_WithNullAudience_StillGeneratesValidToken()
        {
            // Arrange
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(x => x["Jwt:Key"]).Returns(TestKey);
            mockConfig.Setup(x => x["Jwt:Issuer"]).Returns(TestIssuer);
            mockConfig.Setup(x => x["Jwt:Audience"]).Returns((string?)null);

            var service = new JwtService(mockConfig.Object);
            var user = new User
            {
                Id = UserId.New(),
                Email = "test@example.com"
            };

            // Act
            var token = service.GenerateToken(user);

            // Assert
            Assert.NotNull(token);
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            Assert.Empty(jwt.Audiences);
        }

        [Fact]
        public void GenerateToken_WithShortKey_ThrowsCryptographicException()
        {
            // Arrange
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(x => x["Jwt:Key"]).Returns("short"); // Too short for HS256
            mockConfig.Setup(x => x["Jwt:Issuer"]).Returns(TestIssuer);
            mockConfig.Setup(x => x["Jwt:Audience"]).Returns(TestAudience);

            var service = new JwtService(mockConfig.Object);
            var user = new User
            {
                Id = UserId.New(),
                Email = "test@example.com"
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => service.GenerateToken(user));
            Assert.Contains("IDX10653", exception.Message);
        }

        [Fact]
        public void GenerateToken_WithNullUser_ThrowsNullReferenceException()
        {
            // Arrange
            User? user = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => _jwtService.GenerateToken(user!));
        }

        [Fact]
        public void GenerateToken_WithEmptyEmail_StillGeneratesToken()
        {
            // Arrange
            var user = new User
            {
                Id = UserId.New(),
                Email = ""
            };

            // Act
            var token = _jwtService.GenerateToken(user);

            // Assert
            Assert.NotNull(token);
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            
            var emailClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == "email");
            Assert.NotNull(emailClaim);
            Assert.Equal("", emailClaim.Value);
        }

        [Fact]
        public void GenerateToken_WithNullEmail_ThrowsArgumentNullException()
        {
            // Arrange
            var user = new User
            {
                Id = UserId.New(),
                Email = null!
            };

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _jwtService.GenerateToken(user));
        }
    }
}