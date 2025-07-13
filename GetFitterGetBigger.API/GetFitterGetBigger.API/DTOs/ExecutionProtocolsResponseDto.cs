using System.Collections.Generic;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Response DTO for getting execution protocols
/// </summary>
public class ExecutionProtocolsResponseDto
{
    /// <summary>
    /// The list of execution protocols
    /// </summary>
    public List<ExecutionProtocolDto> ExecutionProtocols { get; set; } = new();
}