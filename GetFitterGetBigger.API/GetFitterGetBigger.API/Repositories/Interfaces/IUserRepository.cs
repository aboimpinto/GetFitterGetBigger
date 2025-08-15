using System.Threading.Tasks;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Repositories.Interfaces
{
    public interface IUserRepository : IRepository
    {
        /// <summary>
        /// Gets a user by email address. Returns User.Empty if not found.
        /// </summary>
        Task<User> GetUserByEmailAsync(string email);
        
        /// <summary>
        /// Gets a user by ID. Returns User.Empty if not found.
        /// </summary>
        Task<User> GetUserByIdAsync(UserId userId);
        
        /// <summary>
        /// Adds a new user to the repository.
        /// </summary>
        Task<User> AddUserAsync(User user);
    }
}
