using System;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Implementations;
using Moq;
using Olimpo.EntityFramework.Persistency;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services
{
    public class ClaimServiceTests
    {
        private readonly Mock<IWritableUnitOfWork<FitnessDbContext>> _mockUnitOfWork;
        private readonly Mock<IClaimRepository> _mockClaimRepository;
        private readonly ClaimService _claimService;

        public ClaimServiceTests()
        {
            _mockUnitOfWork = new Mock<IWritableUnitOfWork<FitnessDbContext>>();
            _mockClaimRepository = new Mock<IClaimRepository>();

            _mockUnitOfWork
                .Setup(x => x.GetRepository<IClaimRepository>())
                .Returns(_mockClaimRepository.Object);

            _claimService = new ClaimService();
        }

        [Fact]
        public async Task CreateUserClaimAsync_WithValidData_CreatesClaimAndReturnsId()
        {
            // Arrange
            var userId = UserId.New();
            var claimType = "Free-Tier";
            Claim? capturedClaim = null;

            _mockClaimRepository
                .Setup(x => x.AddClaimAsync(It.IsAny<Claim>()))
                .Callback<Claim>(c => capturedClaim = c);

            // Act
            var claimId = await _claimService.CreateUserClaimAsync(userId, claimType, _mockUnitOfWork.Object);

            // Assert
            Assert.NotNull(capturedClaim);
            Assert.Equal(userId, capturedClaim.UserId);
            Assert.Equal(claimType, capturedClaim.ClaimType);
            Assert.Null(capturedClaim.ExpirationDate);
            Assert.Null(capturedClaim.Resource);
            Assert.Equal(claimId, capturedClaim.Id);
            
            _mockUnitOfWork.Verify(x => x.GetRepository<IClaimRepository>(), Times.Once);
            _mockClaimRepository.Verify(x => x.AddClaimAsync(It.IsAny<Claim>()), Times.Once);
        }

        [Fact]
        public async Task CreateUserClaimAsync_DifferentClaimTypes_CreatesCorrectClaims()
        {
            // Arrange
            var userId = UserId.New();
            var claimTypes = new[] { "Free-Tier", "Admin-Tier", "PT-Tier" };

            foreach (var claimType in claimTypes)
            {
                Claim? capturedClaim = null;

                _mockClaimRepository
                    .Setup(x => x.AddClaimAsync(It.IsAny<Claim>()))
                    .Callback<Claim>(c => capturedClaim = c);

                // Act
                var claimId = await _claimService.CreateUserClaimAsync(userId, claimType, _mockUnitOfWork.Object);

                // Assert
                Assert.NotNull(capturedClaim);
                Assert.Equal(claimType, capturedClaim.ClaimType);
                Assert.NotEqual(default(ClaimId), claimId);
            }
        }

        [Fact]
        public async Task CreateUserClaimAsync_MultipleCallsForSameUser_CreatesDistinctClaims()
        {
            // Arrange
            var userId = UserId.New();
            var claimType = "Free-Tier";
            var capturedClaims = new System.Collections.Generic.List<Claim>();

            _mockClaimRepository
                .Setup(x => x.AddClaimAsync(It.IsAny<Claim>()))
                .Callback<Claim>(c => capturedClaims.Add(c));

            // Act
            var claimId1 = await _claimService.CreateUserClaimAsync(userId, claimType, _mockUnitOfWork.Object);
            var claimId2 = await _claimService.CreateUserClaimAsync(userId, claimType, _mockUnitOfWork.Object);

            // Assert
            Assert.Equal(2, capturedClaims.Count);
            Assert.NotEqual(claimId1, claimId2);
            Assert.All(capturedClaims, c => Assert.Equal(userId, c.UserId));
            Assert.All(capturedClaims, c => Assert.Equal(claimType, c.ClaimType));
            
            _mockClaimRepository.Verify(x => x.AddClaimAsync(It.IsAny<Claim>()), Times.Exactly(2));
        }
    }
}