using System;
using System.Collections.Generic;
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
    public class UserRepositoryTests : IDisposable
    {
        private readonly FitnessDbContext _context;
        private readonly UserRepository _repository;

        public UserRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<FitnessDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new FitnessDbContext(options);
            _repository = new UserRepository();
            _repository.SetContext(_context);
        }

        [Fact]
        public async Task GetUserByEmailAsync_WithExistingUser_ReturnsUserWithClaims()
        {
            // Arrange
            var userId = UserId.New();
            var email = "test@example.com";
            var user = new User { Id = userId, Email = email };
            var claim1 = new Claim 
            { 
                Id = ClaimId.New(), 
                UserId = userId, 
                ClaimType = "Free-Tier",
                User = user 
            };
            var claim2 = new Claim 
            { 
                Id = ClaimId.New(), 
                UserId = userId, 
                ClaimType = "Premium-Tier",
                User = user 
            };

            _context.Users.Add(user);
            _context.Claims.AddRange(claim1, claim2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserByEmailAsync(email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.Equal(email, result.Email);
            Assert.NotNull(result.Claims);
            Assert.Equal(2, result.Claims.Count);
            Assert.Contains(result.Claims, c => c.ClaimType == "Free-Tier");
            Assert.Contains(result.Claims, c => c.ClaimType == "Premium-Tier");
        }

        [Fact]
        public async Task GetUserByEmailAsync_WithNonExistentUser_ReturnsNull()
        {
            // Arrange
            var email = "nonexistent@example.com";

            // Act
            var result = await _repository.GetUserByEmailAsync(email);

            // Assert
            Assert.True(result.IsEmpty);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ExactEmailMatch_ReturnsUser()
        {
            // Arrange
            var userId = UserId.New();
            var email = "test@example.com";
            var user = new User { Id = userId, Email = email };
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserByEmailAsync(email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.Equal(email, result.Email);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithExistingUser_ReturnsUserWithClaims()
        {
            // Arrange
            var userId = UserId.New();
            var user = new User { Id = userId, Email = "test@example.com" };
            var claim = new Claim 
            { 
                Id = ClaimId.New(), 
                UserId = userId, 
                ClaimType = "Free-Tier",
                User = user 
            };

            _context.Users.Add(user);
            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.NotNull(result.Claims);
            Assert.Single(result.Claims);
            Assert.Equal("Free-Tier", result.Claims.First().ClaimType);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithNonExistentUser_ReturnsNull()
        {
            // Arrange
            var userId = UserId.New();

            // Act
            var result = await _repository.GetUserByIdAsync(userId);

            // Assert
            Assert.True(result.IsEmpty);
        }

        [Fact]
        public async Task AddUserAsync_AddsUserToContext()
        {
            // Arrange
            var userId = UserId.New();
            var email = "newuser@example.com";
            var user = new User { Id = userId, Email = email };

            // Act
            var result = await _repository.AddUserAsync(user);
            await _context.SaveChangesAsync();

            // Assert
            Assert.Same(user, result);
            
            var savedUser = await _context.Users.FindAsync(userId);
            Assert.NotNull(savedUser);
            Assert.Equal(email, savedUser.Email);
        }

        [Fact]
        public async Task AddUserAsync_DoesNotCommitChanges()
        {
            // Arrange
            var user = new User { Id = UserId.New(), Email = "test@example.com" };

            // Act
            await _repository.AddUserAsync(user);

            // Assert - User should be tracked but not saved
            var entry = _context.Entry(user);
            Assert.Equal(EntityState.Added, entry.State);
            
            // Verify not in database yet
            var savedUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == user.Id);
            Assert.Null(savedUser);
        }

        [Fact]
        public async Task GetUserByEmailAsync_IncludesAllRelatedClaims()
        {
            // Arrange
            var userId = UserId.New();
            var user = new User { Id = userId, Email = "test@example.com" };
            
            var claims = Enumerable.Range(1, 5).Select(i => new Claim
            {
                Id = ClaimId.New(),
                UserId = userId,
                ClaimType = $"Claim-{i}",
                User = user
            }).ToList();

            _context.Users.Add(user);
            _context.Claims.AddRange(claims);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserByEmailAsync(user.Email);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Claims);
            Assert.Equal(5, result.Claims.Count);
            for (int i = 1; i <= 5; i++)
            {
                Assert.Contains(result.Claims, c => c.ClaimType == $"Claim-{i}");
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}