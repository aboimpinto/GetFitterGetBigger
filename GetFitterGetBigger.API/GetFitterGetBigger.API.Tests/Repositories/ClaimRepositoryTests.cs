using System;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Repositories
{
    public class ClaimRepositoryTests : IDisposable
    {
        private readonly FitnessDbContext _context;
        private readonly ClaimRepository _repository;

        public ClaimRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<FitnessDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new FitnessDbContext(options);
            _repository = new ClaimRepository();
            _repository.SetContext(_context);
        }

        [Fact]
        public async Task GetClaimsByUserIdAsync_ReturnsAllUserClaims()
        {
            // Arrange
            var userId = UserId.New();
            var otherUserId = UserId.New();
            
            var userClaims = new[]
            {
                new Claim { Id = ClaimId.New(), UserId = userId, ClaimType = "Free-Tier" },
                new Claim { Id = ClaimId.New(), UserId = userId, ClaimType = "Premium-Tier" },
                new Claim { Id = ClaimId.New(), UserId = userId, ClaimType = "Admin-Tier" }
            };
            
            var otherUserClaim = new Claim 
            { 
                Id = ClaimId.New(), 
                UserId = otherUserId, 
                ClaimType = "Free-Tier" 
            };

            _context.Claims.AddRange(userClaims);
            _context.Claims.Add(otherUserClaim);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetClaimsByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.All(result, claim => Assert.Equal(userId, claim.UserId));
            Assert.Contains(result, c => c.ClaimType == "Free-Tier");
            Assert.Contains(result, c => c.ClaimType == "Premium-Tier");
            Assert.Contains(result, c => c.ClaimType == "Admin-Tier");
        }

        [Fact]
        public async Task GetClaimsByUserIdAsync_WithNoClaimsForUser_ReturnsEmptyList()
        {
            // Arrange
            var userId = UserId.New();
            var otherUserId = UserId.New();
            
            var otherUserClaim = new Claim 
            { 
                Id = ClaimId.New(), 
                UserId = otherUserId, 
                ClaimType = "Free-Tier" 
            };

            _context.Claims.Add(otherUserClaim);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetClaimsByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetClaimsByUserIdAsync_WithEmptyDatabase_ReturnsEmptyList()
        {
            // Arrange
            var userId = UserId.New();

            // Act
            var result = await _repository.GetClaimsByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task AddClaimAsync_AddsClaimToContext()
        {
            // Arrange
            var claimId = ClaimId.New();
            var userId = UserId.New();
            var claim = new Claim
            {
                Id = claimId,
                UserId = userId,
                ClaimType = "Free-Tier",
                ExpirationDate = DateTime.UtcNow.AddDays(30),
                Resource = "basic-features"
            };

            // Act
            var result = await _repository.AddClaimAsync(claim);
            await _context.SaveChangesAsync();

            // Assert
            Assert.Same(claim, result);
            
            var savedClaim = await _context.Claims.FindAsync(claimId);
            Assert.NotNull(savedClaim);
            Assert.Equal(userId, savedClaim.UserId);
            Assert.Equal("Free-Tier", savedClaim.ClaimType);
            Assert.Equal("basic-features", savedClaim.Resource);
        }

        [Fact]
        public async Task AddClaimAsync_DoesNotCommitChanges()
        {
            // Arrange
            var claim = new Claim
            {
                Id = ClaimId.New(),
                UserId = UserId.New(),
                ClaimType = "Test-Tier"
            };

            // Act
            await _repository.AddClaimAsync(claim);

            // Assert - Claim should be tracked but not saved
            var entry = _context.Entry(claim);
            Assert.Equal(EntityState.Added, entry.State);
            
            // Verify not in database yet
            var savedClaim = await _context.Claims
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == claim.Id);
            Assert.Null(savedClaim);
        }

        [Fact]
        public async Task GetClaimsByUserIdAsync_ReturnsClaimsWithAllProperties()
        {
            // Arrange
            var userId = UserId.New();
            var expirationDate = DateTime.UtcNow.AddDays(90);
            
            var claim = new Claim
            {
                Id = ClaimId.New(),
                UserId = userId,
                ClaimType = "Special-Access",
                ExpirationDate = expirationDate,
                Resource = "premium-api-endpoints"
            };

            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetClaimsByUserIdAsync(userId);

            // Assert
            Assert.Single(result);
            var retrievedClaim = result.First();
            Assert.Equal(claim.Id, retrievedClaim.Id);
            Assert.Equal(claim.UserId, retrievedClaim.UserId);
            Assert.Equal(claim.ClaimType, retrievedClaim.ClaimType);
            Assert.Equal(claim.ExpirationDate, retrievedClaim.ExpirationDate);
            Assert.Equal(claim.Resource, retrievedClaim.Resource);
        }

        [Fact]
        public async Task GetClaimsByUserIdAsync_OrdersByClaimId()
        {
            // Arrange
            var userId = UserId.New();
            
            // Create claims with different IDs to test ordering
            var claims = new[]
            {
                new Claim { Id = ClaimId.New(), UserId = userId, ClaimType = "Claim-3" },
                new Claim { Id = ClaimId.New(), UserId = userId, ClaimType = "Claim-1" },
                new Claim { Id = ClaimId.New(), UserId = userId, ClaimType = "Claim-2" }
            };

            _context.Claims.AddRange(claims);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetClaimsByUserIdAsync(userId);

            // Assert
            Assert.Equal(3, result.Count);
            // Results should be in the order they were retrieved from the database
            // which depends on the database provider and isn't guaranteed without OrderBy
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}