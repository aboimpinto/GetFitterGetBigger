namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Response DTO for bidirectional link creation
/// </summary>
public class BidirectionalLinkResponseDto
{
    /// <summary>
    /// The primary link that was requested by the client
    /// </summary>
    public ExerciseLinkDto PrimaryLink { get; set; } = ExerciseLinkDto.Empty;
    
    /// <summary>
    /// The automatically created reverse link (null if no reverse link created)
    /// </summary>
    public ExerciseLinkDto? ReverseLink { get; set; }
    
    /// <summary>
    /// Indicates if a bidirectional pair was created
    /// </summary>
    public bool IsBidirectional => ReverseLink != null;
    
    /// <summary>
    /// Description of the link creation process
    /// </summary>
    public string Description { get; set; } = string.Empty;
}