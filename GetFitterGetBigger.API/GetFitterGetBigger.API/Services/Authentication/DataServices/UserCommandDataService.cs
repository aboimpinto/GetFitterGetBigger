using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Mappers;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Authentication.DataServices;

/// <summary>
/// Implementation of User command data service for write operations
/// </summary>
public class UserCommandDataService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider) : IUserCommandDataService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;

    public async Task<ServiceResult<UserDto>> CreateUserWithClaimAsync(string email, string claimType)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var userRepository = unitOfWork.GetRepository<IUserRepository>();
        var claimRepository = unitOfWork.GetRepository<IClaimRepository>();
        
        // Create user
        var user = new User 
        { 
            Id = UserId.New(),
            Email = email 
        };
        await userRepository.AddUserAsync(user);
        
        // Create claim
        var claim = new Claim
        {
            Id = ClaimId.New(),
            UserId = user.Id,
            ClaimType = claimType,
            ExpirationDate = null,
            Resource = null
        };
        await claimRepository.AddClaimAsync(claim);
        
        // Commit transaction
        await unitOfWork.CommitAsync();
        
        // Load the complete user with claims
        var createdUser = await userRepository.GetUserByIdAsync(user.Id);
        // Trust the Empty pattern - repository never returns null
        var dto = createdUser.ToDto();
        
        return ServiceResult<UserDto>.Success(dto);
    }

    public async Task<ServiceResult<UserDto>> CreateUserAsync(string email)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IUserRepository>();
        
        var user = new User 
        { 
            Id = UserId.New(),
            Email = email 
        };
        
        await repository.AddUserAsync(user);
        await unitOfWork.CommitAsync();
        
        var dto = user.ToDto();
        return ServiceResult<UserDto>.Success(dto);
    }
}