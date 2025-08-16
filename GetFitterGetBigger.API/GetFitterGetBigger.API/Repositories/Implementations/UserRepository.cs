using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Repositories.Implementations
{
    public class UserRepository : DomainRepository<User, UserId, FitnessDbContext>, IUserRepository
    {
        public async Task<User> GetUserByEmailAsync(string email)
        {
            var user = await Context.Users
                .Include(u => u.Claims)
                .FirstOrDefaultAsync(u => u.Email == email);
            
            // Apply Empty pattern at the data boundary
            return user ?? User.Empty;
        }

        public async Task<User> GetUserByIdAsync(UserId userId)
        {
            // Override base GetByIdAsync to include Claims
            var user = await Context.Users
                .Include(u => u.Claims)
                .FirstOrDefaultAsync(u => u.Id == userId);
            
            // Apply Empty pattern at the data boundary
            return user ?? User.Empty;
        }

        public async Task<User> AddUserAsync(User user)
        {
            await Context.Users.AddAsync(user);
            return user;
        }
    }
}
