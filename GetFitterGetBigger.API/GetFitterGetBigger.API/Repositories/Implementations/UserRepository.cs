using System.Threading.Tasks;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Repositories.Implementations
{
    public class UserRepository : RepositoryBase<FitnessDbContext>, IUserRepository
    {
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await Context.Users
                .Include(u => u.Claims)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> AddUserAsync(User user)
        {
            await Context.Users.AddAsync(user);
            return user;
        }
    }
}
