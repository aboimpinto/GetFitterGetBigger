using System;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for execution protocol
/// </summary>
public record ExecutionProtocolDto
{
    /// <summary>
    /// The ID of the execution protocol in the format "executionprotocol-{guid}"
    /// </summary>
    /// <example>executionprotocol-a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a</example>
    public string ExecutionProtocolId { get; init; } = string.Empty;
    
    /// <summary>
    /// The value of the execution protocol
    /// </summary>
    /// <example>Standard</example>
    public string Value { get; init; } = string.Empty;
    
    /// <summary>
    /// The description of the execution protocol
    /// </summary>
    /// <example>Standard protocol with balanced rep and time components</example>
    public string? Description { get; init; }
    
    /// <summary>
    /// The unique code for the execution protocol (uppercase with underscores)
    /// </summary>
    /// <example>STANDARD</example>
    public string Code { get; init; } = string.Empty;
    
    /// <summary>
    /// Indicates if this protocol is time-based
    /// </summary>
    /// <example>true</example>
    public bool TimeBase { get; init; }
    
    /// <summary>
    /// Indicates if this protocol is repetition-based
    /// </summary>
    /// <example>true</example>
    public bool RepBase { get; init; }
    
    /// <summary>
    /// The rest pattern description for this protocol
    /// </summary>
    /// <example>60-90 seconds between sets</example>
    public string? RestPattern { get; init; }
    
    /// <summary>
    /// The intensity level description for this protocol
    /// </summary>
    /// <example>Moderate to High</example>
    public string? IntensityLevel { get; init; }
    
    /// <summary>
    /// The display order for sorting
    /// </summary>
    /// <example>1</example>
    public int DisplayOrder { get; init; }
    
    /// <summary>
    /// Indicates whether the execution protocol is active
    /// </summary>
    /// <example>true</example>
    public bool IsActive { get; init; }
}