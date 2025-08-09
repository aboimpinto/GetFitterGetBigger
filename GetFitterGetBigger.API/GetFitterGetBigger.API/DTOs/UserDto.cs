using GetFitterGetBigger.API.DTOs.Interfaces;

namespace GetFitterGetBigger.API.DTOs;

public record UserDto(
    string Id,
    string Email,
    List<ClaimInfo> Claims) : IEmptyDto<UserDto>
{
    public static UserDto Empty => new(
        string.Empty,
        string.Empty,
        new List<ClaimInfo>());

    public bool IsEmpty => string.IsNullOrEmpty(Id) || Id == "user-00000000-0000-0000-0000-000000000000";
}