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
            using var unitOfWork = _unitOfWorkProvider.CreateWritable();
            var userRepository = unitOfWork.GetRepository<IUserRepository>();

            var user = await userRepository.GetUserByEmailAsync(request.Email);

            if (user == null)
            {
                user = new User { Id = UserId.New(), Email = request.Email };
                await userRepository.AddUserAsync(user);

                // Use ClaimService with the same UnitOfWork for transactional consistency
                await _claimService.CreateUserClaimAsync(user.Id, "Free-Tier", unitOfWork);
            }

            var token = _jwtService.GenerateToken(user);
            var claims = user.Claims
                .Where(c => c.ExpirationDate == null || c.ExpirationDate > DateTime.UtcNow)
                .Select(c => new ClaimInfo(c.Id.ToString(), c.ClaimType, c.ExpirationDate, c.Resource))
                .ToList();

            await unitOfWork.CommitAsync();

            return new AuthenticationResponse(token, claims);
        }
    }
}