using System.Text.Json.Serialization;

namespace GetFitterGetBigger.Admin.Models.Dtos;

public class ExecutionProtocolDto
{
    [JsonPropertyName("executionProtocolId")]
    public required string ExecutionProtocolId { get; set; }
    
    [JsonPropertyName("code")]
    public required string Code { get; set; }
    
    [JsonPropertyName("value")]
    public required string Value { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("timeBase")]
    public bool TimeBase { get; set; }
    
    [JsonPropertyName("repBase")]
    public bool RepBase { get; set; }
    
    [JsonPropertyName("restPattern")]
    public string? RestPattern { get; set; }
    
    [JsonPropertyName("intensityLevel")]
    public string? IntensityLevel { get; set; }
    
    [JsonPropertyName("displayOrder")]
    public int DisplayOrder { get; set; }
    
    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
}