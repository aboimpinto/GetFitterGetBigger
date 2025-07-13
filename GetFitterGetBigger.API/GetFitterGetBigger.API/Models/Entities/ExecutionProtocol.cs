using System;
using System.Text.RegularExpressions;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities;

public record ExecutionProtocol : ReferenceDataBase
{
    public ExecutionProtocolId Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public bool TimeBase { get; init; }
    public bool RepBase { get; init; }
    public string? RestPattern { get; init; }
    public string? IntensityLevel { get; init; }
    
    private ExecutionProtocol() { }
    
    public static class Handler
    {
        private static readonly Regex CodeRegex = new(@"^[A-Z]+(_[A-Z]+)*$", RegexOptions.Compiled);
        
        public static ExecutionProtocol CreateNew(
            string value,
            string? description,
            string code,
            bool timeBase,
            bool repBase,
            string? restPattern,
            string? intensityLevel,
            int displayOrder,
            bool isActive = true)
        {
            ValidateParameters(value, code);
                
            return new()
            {
                Id = ExecutionProtocolId.New(),
                Value = value,
                Description = description,
                Code = code,
                TimeBase = timeBase,
                RepBase = repBase,
                RestPattern = restPattern,
                IntensityLevel = intensityLevel,
                DisplayOrder = displayOrder,
                IsActive = isActive
            };
        }
        
        public static ExecutionProtocol Create(
            ExecutionProtocolId id,
            string value,
            string? description,
            string code,
            bool timeBase,
            bool repBase,
            string? restPattern,
            string? intensityLevel,
            int displayOrder,
            bool isActive = true)
        {
            ValidateParameters(value, code);
            
            return new()
            {
                Id = id,
                Value = value,
                Description = description,
                Code = code,
                TimeBase = timeBase,
                RepBase = repBase,
                RestPattern = restPattern,
                IntensityLevel = intensityLevel,
                DisplayOrder = displayOrder,
                IsActive = isActive
            };
        }
            
        public static ExecutionProtocol Update(
            ExecutionProtocol protocol,
            string? value = null,
            string? description = null,
            string? code = null,
            bool? timeBase = null,
            bool? repBase = null,
            string? restPattern = null,
            string? intensityLevel = null,
            int? displayOrder = null,
            bool? isActive = null)
        {
            var newValue = value ?? protocol.Value;
            var newCode = code ?? protocol.Code;
            
            ValidateParameters(newValue, newCode);
            
            return protocol with
            {
                Value = newValue,
                Description = description ?? protocol.Description,
                Code = newCode,
                TimeBase = timeBase ?? protocol.TimeBase,
                RepBase = repBase ?? protocol.RepBase,
                RestPattern = restPattern ?? protocol.RestPattern,
                IntensityLevel = intensityLevel ?? protocol.IntensityLevel,
                DisplayOrder = displayOrder ?? protocol.DisplayOrder,
                IsActive = isActive ?? protocol.IsActive
            };
        }
            
        public static ExecutionProtocol Deactivate(ExecutionProtocol protocol) =>
            protocol with { IsActive = false };
            
        private static void ValidateParameters(string value, string code)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value cannot be empty", nameof(value));
            
            if (value.Length > 100)
                throw new ArgumentException("Value cannot exceed 100 characters", nameof(value));
                
            if (string.IsNullOrEmpty(code))
                throw new ArgumentException("Code is required", nameof(code));
                
            if (code.Length > 50)
                throw new ArgumentException("Code cannot exceed 50 characters", nameof(code));
                
            if (!CodeRegex.IsMatch(code))
                throw new ArgumentException("Code must be uppercase with underscores only", nameof(code));
        }
    }
}