using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Mappers;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Authentication.DataServices;

/// <summary>
/// Implementation of User query data service for read operations
/// </summary>
public class UserQueryDataService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider) : IUserQueryDataService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;

    public async Task<ServiceResult<UserDto>> GetByEmailAsync(string email)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IUserRepository>();
        var entity = await repository.GetUserByEmailAsync(email);
        
        // Trust the Empty pattern - repository never returns null
        var dto = entity.ToDto();
        return ServiceResult<UserDto>.Success(dto);
    }

    public async Task<ServiceResult<UserDto>> GetByIdAsync(UserId userId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IUserRepository>();
        var entity = await repository.GetUserByIdAsync(userId);
        
        // Trust the Empty pattern - repository never returns null
        var dto = entity.ToDto();
        return ServiceResult<UserDto>.Success(dto);
    }

    public async Task<ServiceResult<BooleanResultDto>> ExistsByEmailAsync(string email)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IUserRepository>();
        var entity = await repository.GetUserByEmailAsync(email);
        
        // Check using IsEmpty property instead of null check
        var exists = !entity.IsEmpty;
        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(exists));
    }
}