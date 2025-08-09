using GetFitterGetBigger.API.DTOs.Interfaces;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Wrapper DTO for boolean results to maintain consistency with Empty pattern.
/// Used when service methods need to return boolean values while adhering to IEmptyDto interface.
/// </summary>
public class BooleanResultDto : IEmptyDto<BooleanResultDto>
{
    /// <summary>
    /// The boolean value result
    /// </summary>
    public bool Value { get; set; }
    
    /// <summary>
    /// A BooleanResultDto is never considered empty as it always has a valid boolean state
    /// </summary>
    public bool IsEmpty => false;
    
    /// <summary>
    /// Returns an Empty instance with Value = false
    /// </summary>
    public static BooleanResultDto Empty => new() { Value = false };
    
    /// <summary>
    /// Creates a new BooleanResultDto with the specified value
    /// </summary>
    public static BooleanResultDto Create(bool value) => new() { Value = value };
    
    /// <summary>
    /// Implicitly converts a boolean to BooleanResultDto
    /// </summary>
    public static implicit operator BooleanResultDto(bool value) => Create(value);
    
    /// <summary>
    /// Implicitly converts BooleanResultDto to boolean
    /// </summary>
    public static implicit operator bool(BooleanResultDto dto) => dto?.Value ?? false;
}