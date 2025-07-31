using System;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Interfaces;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IJwtService _jwtService;
        private readonly IUnitOfWorkProvider<Models.FitnessDbContext> _unitOfWorkProvider;
        private readonly IClaimService _claimService;

        public AuthService(
            IJwtService jwtService,
            IUnitOfWorkProvider<Models.FitnessDbContext> unitOfWorkProvider,
            IClaimService claimService)
        {
            _jwtService = jwtService;
            _unitOfWorkProvider = unitOfWorkProvider;
            _claimService = claimService;
        }

        public async Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request)
        {
            // First, try to get existing user with read-only context
            using (var readOnlyUow = _unitOfWorkProvider.CreateReadOnly())
            {
                var readOnlyUserRepo = readOnlyUow.GetRepository<IUserRepository>();
                var existingUser = await readOnlyUserRepo.GetUserByEmailAsync(request.Email);
                
                if (existingUser != null)
                {
                    // User exists, generate token and return
                    var token = _jwtService.GenerateToken(existingUser);
                    var claims = existingUser.Claims
                        .Where(c => c.ExpirationDate == null || c.ExpirationDate > DateTime.UtcNow)
                        .Select(c => new ClaimInfo(c.Id.ToString(), c.ClaimType, c.ExpirationDate, c.Resource))
                        .ToList();
                    
                    return new AuthenticationResponse(token, claims);
                }
            }

            // User doesn't exist, create new user
            using var unitOfWork = _unitOfWorkProvider.CreateWritable();
            var userRepository = unitOfWork.GetRepository<IUserRepository>();

            // Double-check in case of race condition
            var user = await userRepository.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                user = new User { Id = UserId.New(), Email = request.Email };
                await userRepository.AddUserAsync(user);

                // Use ClaimService with the same UnitOfWork for transactional consistency
                await _claimService.CreateUserClaimAsync(user.Id, "Free-Tier", unitOfWork);
                
                await unitOfWork.CommitAsync();
            }

            var finalToken = _jwtService.GenerateToken(user);
            var finalClaims = user.Claims
                .Where(c => c.ExpirationDate == null || c.ExpirationDate > DateTime.UtcNow)
                .Select(c => new ClaimInfo(c.Id.ToString(), c.ClaimType, c.ExpirationDate, c.Resource))
                .ToList();

            return new AuthenticationResponse(finalToken, finalClaims);
        }
    }
}