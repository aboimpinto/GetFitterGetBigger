using System.ComponentModel.DataAnnotations;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for coach notes
/// </summary>
public class CoachNoteDto
{
    /// <summary>
    /// The ID of the coach note in the format "coachnote-{guid}"
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// The instructional text of the coach note
    /// </summary>
    public string Text { get; set; } = string.Empty;
    
    /// <summary>
    /// The display order of the coach note (0-based)
    /// </summary>
    public int Order { get; set; }
}

/// <summary>
/// Request model for creating or updating coach notes
/// </summary>
public class CoachNoteRequest
{
    /// <summary>
    /// The ID of the coach note (optional for new notes)
    /// </summary>
    public string? Id { get; set; }
    
    /// <summary>
    /// The instructional text of the coach note
    /// </summary>
    [Required(ErrorMessage = "Coach note text is required")]
    [MaxLength(1000, ErrorMessage = "Coach note text cannot exceed 1000 characters")]
    public string Text { get; set; } = string.Empty;
    
    /// <summary>
    /// The display order of the coach note
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Order must be a non-negative number")]
    public int Order { get; set; }
}