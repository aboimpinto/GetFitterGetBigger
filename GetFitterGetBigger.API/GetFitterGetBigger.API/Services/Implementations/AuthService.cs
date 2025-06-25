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

        public AuthService(
            IJwtService jwtService,
            IUnitOfWorkProvider<Models.FitnessDbContext> unitOfWorkProvider)
        {
            _jwtService = jwtService;
            _unitOfWorkProvider = unitOfWorkProvider;
        }

        public async Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request)
        {
            using var unitOfWork = _unitOfWorkProvider.CreateWritable();
            var userRepository = unitOfWork.GetRepository<IUserRepository>();
            var claimRepository = unitOfWork.GetRepository<IClaimRepository>();

            var user = await userRepository.GetUserByEmailAsync(request.Email);

            if (user == null)
            {
                user = new User { Id = UserId.New(), Email = request.Email };
                await userRepository.AddUserAsync(user);

                var claim = new Claim
                {
                    Id = ClaimId.New(),
                    UserId = user.Id,
                    ClaimType = "Free-Tier",
                    ExpirationDate = null,
                    Resource = null
                };
                await claimRepository.AddClaimAsync(claim);
            }

            var token = _jwtService.GenerateToken(user);
            var claims = user.Claims
                .Where(c => c.ExpirationDate == null || c.ExpirationDate > DateTime.UtcNow)
                .Select(c => new ClaimInfo(c.Id.ToString(), c.ExpirationDate, c.Resource))
                .ToList();

            await unitOfWork.CommitAsync();

            return new AuthenticationResponse(token, claims);
        }
    }
}
