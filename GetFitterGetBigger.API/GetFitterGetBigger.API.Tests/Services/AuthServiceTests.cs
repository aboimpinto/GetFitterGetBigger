using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Implementations;
using GetFitterGetBigger.API.Services.Interfaces;
using Moq;
using Olimpo.EntityFramework.Persistency;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IJwtService> _mockJwtService;
        private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _mockUnitOfWorkProvider;
        private readonly Mock<IWritableUnitOfWork<FitnessDbContext>> _mockUnitOfWork;
        private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _mockReadOnlyUnitOfWork;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IUserRepository> _mockReadOnlyUserRepository;
        private readonly Mock<IClaimService> _mockClaimService;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockJwtService = new Mock<IJwtService>();
            _mockUnitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
            _mockUnitOfWork = new Mock<IWritableUnitOfWork<FitnessDbContext>>();
            _mockReadOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockReadOnlyUserRepository = new Mock<IUserRepository>();
            _mockClaimService = new Mock<IClaimService>();

            _mockUnitOfWorkProvider
                .Setup(x => x.CreateWritable())
                .Returns(_mockUnitOfWork.Object);

            _mockUnitOfWorkProvider
                .Setup(x => x.CreateReadOnly())
                .Returns(_mockReadOnlyUnitOfWork.Object);

            _mockUnitOfWork
                .Setup(x => x.GetRepository<IUserRepository>())
                .Returns(_mockUserRepository.Object);

            _mockReadOnlyUnitOfWork
                .Setup(x => x.GetRepository<IUserRepository>())
                .Returns(_mockReadOnlyUserRepository.Object);

            _authService = new AuthService(_mockJwtService.Object, _mockUnitOfWorkProvider.Object, _mockClaimService.Object);
        }

        [Fact]
        public async Task AuthenticateAsync_WithNewUser_CreatesUserAndFreeTierClaim()
        {
            // Arrange
            var email = "newuser@example.com";
            var request = new AuthenticationRequest(email);
            var expectedToken = "jwt-token";

            _mockUserRepository
                .Setup(x => x.GetUserByEmailAsync(email))
                .ReturnsAsync((User?)null);

            _mockJwtService
                .Setup(x => x.GenerateToken(It.IsAny<User>()))
                .Returns(expectedToken);

            User? capturedUser = null;
            ClaimId? capturedClaimId = null;
            
            _mockUserRepository
                .Setup(x => x.AddUserAsync(It.IsAny<User>()))
                .Callback<User>(u => 
                {
                    capturedUser = u;
                    // Initialize Claims collection for new user
                    u.Claims = new List<Claim>();
                })
                .ReturnsAsync((User u) => u);

            _mockClaimService
                .Setup(x => x.CreateUserClaimAsync(It.IsAny<UserId>(), "Free-Tier", _mockUnitOfWork.Object))
                .Callback<UserId, string, IWritableUnitOfWork<FitnessDbContext>>((userId, claimType, uow) => 
                {
                    capturedClaimId = ClaimId.New();
                    // Add the claim to the user's claims collection
                    if (capturedUser != null && capturedUser.Claims != null)
                    {
                        capturedUser.Claims.Add(new Claim 
                        { 
                            Id = capturedClaimId.Value,
                            UserId = userId,
                            ClaimType = claimType,
                            ExpirationDate = null,
                            Resource = null
                        });
                    }
                })
                .ReturnsAsync(() => capturedClaimId!.Value);

            // Act
            var response = await _authService.AuthenticateAsync(request);

            // Assert
            Assert.NotNull(capturedUser);
            Assert.Equal(email, capturedUser.Email);

            Assert.NotNull(capturedClaimId);
            _mockClaimService.Verify(x => x.CreateUserClaimAsync(capturedUser.Id, "Free-Tier", _mockUnitOfWork.Object), Times.Once);

            Assert.Equal(expectedToken, response.Token);
            Assert.Single(response.Claims);
            Assert.Equal("Free-Tier", response.Claims[0].ClaimType);

            _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WithExistingUser_ReturnsUserClaims()
        {
            // Arrange
            var email = "existing@example.com";
            var request = new AuthenticationRequest(email);
            var userId = UserId.New();
            var expectedToken = "jwt-token";

            var claims = new List<Claim>
            {
                new Claim 
                { 
                    Id = ClaimId.New(), 
                    UserId = userId, 
                    ClaimType = "Free-Tier", 
                    ExpirationDate = null,
                    Resource = null
                },
                new Claim 
                { 
                    Id = ClaimId.New(), 
                    UserId = userId, 
                    ClaimType = "Premium-Tier", 
                    ExpirationDate = DateTime.UtcNow.AddDays(30),
                    Resource = "premium-features"
                }
            };

            var existingUser = new User 
            { 
                Id = userId, 
                Email = email,
                Claims = claims
            };

            _mockReadOnlyUserRepository
                .Setup(x => x.GetUserByEmailAsync(email))
                .ReturnsAsync(existingUser);

            _mockJwtService
                .Setup(x => x.GenerateToken(existingUser))
                .Returns(expectedToken);

            // Act
            var response = await _authService.AuthenticateAsync(request);

            // Assert
            Assert.Equal(expectedToken, response.Token);
            Assert.Equal(2, response.Claims.Count);
            Assert.Contains(response.Claims, c => c.ClaimType == "Free-Tier");
            Assert.Contains(response.Claims, c => c.ClaimType == "Premium-Tier");

            _mockUserRepository.Verify(x => x.AddUserAsync(It.IsAny<User>()), Times.Never);
            _mockClaimService.Verify(x => x.CreateUserClaimAsync(It.IsAny<UserId>(), It.IsAny<string>(), It.IsAny<IWritableUnitOfWork<FitnessDbContext>>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Never);
            _mockReadOnlyUserRepository.Verify(x => x.GetUserByEmailAsync(email), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_FiltersExpiredClaims()
        {
            // Arrange
            var email = "user@example.com";
            var request = new AuthenticationRequest(email);
            var userId = UserId.New();

            var claims = new List<Claim>
            {
                new Claim 
                { 
                    Id = ClaimId.New(), 
                    UserId = userId, 
                    ClaimType = "Active-Claim", 
                    ExpirationDate = DateTime.UtcNow.AddDays(30),
                    Resource = null
                },
                new Claim 
                { 
                    Id = ClaimId.New(), 
                    UserId = userId, 
                    ClaimType = "Expired-Claim", 
                    ExpirationDate = DateTime.UtcNow.AddDays(-1),
                    Resource = null
                },
                new Claim 
                { 
                    Id = ClaimId.New(), 
                    UserId = userId, 
                    ClaimType = "No-Expiry-Claim", 
                    ExpirationDate = null,
                    Resource = null
                }
            };

            var existingUser = new User 
            { 
                Id = userId, 
                Email = email,
                Claims = claims
            };

            _mockUserRepository
                .Setup(x => x.GetUserByEmailAsync(email))
                .ReturnsAsync(existingUser);

            _mockJwtService
                .Setup(x => x.GenerateToken(existingUser))
                .Returns("jwt-token");

            // Act
            var response = await _authService.AuthenticateAsync(request);

            // Assert
            Assert.Equal(2, response.Claims.Count);
            Assert.Contains(response.Claims, c => c.ClaimType == "Active-Claim");
            Assert.Contains(response.Claims, c => c.ClaimType == "No-Expiry-Claim");
            Assert.DoesNotContain(response.Claims, c => c.ClaimType == "Expired-Claim");
        }

        [Fact]
        public async Task AuthenticateAsync_DisposesUnitOfWork()
        {
            // Arrange
            var request = new AuthenticationRequest("test@example.com");
            _mockUserRepository
                .Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            _mockJwtService
                .Setup(x => x.GenerateToken(It.IsAny<User>()))
                .Returns("token");

            // Act
            await _authService.AuthenticateAsync(request);

            // Assert
            _mockUnitOfWork.Verify(x => x.Dispose(), Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task AuthenticateAsync_WithInvalidEmail_StillCreatesUser(string email)
        {
            // Arrange
            var request = new AuthenticationRequest(email);
            
            _mockUserRepository
                .Setup(x => x.GetUserByEmailAsync(email))
                .ReturnsAsync((User?)null);

            _mockJwtService
                .Setup(x => x.GenerateToken(It.IsAny<User>()))
                .Returns("token");

            _mockUserRepository
                .Setup(x => x.AddUserAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => 
                {
                    u.Claims = new List<Claim>();
                    return u;
                });

            _mockClaimService
                .Setup(x => x.CreateUserClaimAsync(It.IsAny<UserId>(), "Free-Tier", _mockUnitOfWork.Object))
                .ReturnsAsync(ClaimId.New());

            // Act
            var response = await _authService.AuthenticateAsync(request);

            // Assert
            Assert.NotNull(response);
            _mockUserRepository.Verify(x => x.AddUserAsync(It.Is<User>(u => u.Email == email)), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WhenUserRepositoryThrows_DisposesUnitOfWorkAndRethrows()
        {
            // Arrange
            var request = new AuthenticationRequest("test@example.com");
            _mockUserRepository
                .Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.AuthenticateAsync(request));
            _mockUnitOfWork.Verify(x => x.Dispose(), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WhenJwtServiceThrows_DisposesUnitOfWorkAndRethrows()
        {
            // Arrange
            var request = new AuthenticationRequest("test@example.com");
            var user = new User { Id = UserId.New(), Email = request.Email, Claims = new List<Claim>() };
            
            _mockUserRepository
                .Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _mockJwtService
                .Setup(x => x.GenerateToken(It.IsAny<User>()))
                .Throws(new InvalidOperationException("JWT configuration error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.AuthenticateAsync(request));
            _mockUnitOfWork.Verify(x => x.Dispose(), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WhenCommitFails_DisposesUnitOfWorkAndRethrows()
        {
            // Arrange
            var request = new AuthenticationRequest("test@example.com");
            
            _mockUserRepository
                .Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            _mockJwtService
                .Setup(x => x.GenerateToken(It.IsAny<User>()))
                .Returns("token");

            _mockUserRepository
                .Setup(x => x.AddUserAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => 
                {
                    u.Claims = new List<Claim>();
                    return u;
                });

            _mockClaimService
                .Setup(x => x.CreateUserClaimAsync(It.IsAny<UserId>(), "Free-Tier", _mockUnitOfWork.Object))
                .ReturnsAsync(ClaimId.New());

            _mockUnitOfWork
                .Setup(x => x.CommitAsync())
                .ThrowsAsync(new InvalidOperationException("Commit failed"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.AuthenticateAsync(request));
            _mockUnitOfWork.Verify(x => x.Dispose(), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WithUserHavingNullClaimsList_ThrowsNullReferenceException()
        {
            // Arrange
            var request = new AuthenticationRequest("test@example.com");
            var user = new User 
            { 
                Id = UserId.New(), 
                Email = request.Email,
                Claims = null! // Null claims collection
            };
            
            _mockUserRepository
                .Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _mockJwtService
                .Setup(x => x.GenerateToken(It.IsAny<User>()))
                .Returns("token");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _authService.AuthenticateAsync(request));
        }
    }
}