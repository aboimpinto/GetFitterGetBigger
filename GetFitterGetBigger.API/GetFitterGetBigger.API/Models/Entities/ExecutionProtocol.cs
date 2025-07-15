using System;
using System.Text.RegularExpressions;
using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Models.Results;
using GetFitterGetBigger.API.Models.Validation;

namespace GetFitterGetBigger.API.Models.Entities;

public record ExecutionProtocol : ReferenceDataBase, IPureReference, IEmptyEntity<ExecutionProtocol>
{
    public ExecutionProtocolId ExecutionProtocolId { get; init; }
    public string Code { get; init; } = string.Empty;
    public bool TimeBase { get; init; }
    public bool RepBase { get; init; }
    public string? RestPattern { get; init; }
    public string? IntensityLevel { get; init; }
    
    public string Id => ExecutionProtocolId.ToString();
    
    public bool IsEmpty => ExecutionProtocolId.IsEmpty;
    
    private ExecutionProtocol() { }
    
    public CacheStrategy GetCacheStrategy() => CacheStrategy.Eternal;
    
    public TimeSpan? GetCacheDuration() => null; // Eternal caching
    
    public static ExecutionProtocol Empty { get; } = new()
    {
        ExecutionProtocolId = ExecutionProtocolId.Empty,
        Value = string.Empty,
        Description = null,
        Code = string.Empty,
        TimeBase = false,
        RepBase = false,
        RestPattern = null,
        IntensityLevel = null,
        DisplayOrder = 0,
        IsActive = false
    };
    
    public static class Handler
    {
        private static readonly Regex CodeRegex = new(@"^[A-Z]+(_[A-Z]+)*$", RegexOptions.Compiled);
        
        public static EntityResult<ExecutionProtocol> CreateNew(
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
            return Create(
                ExecutionProtocolId.New(),
                value,
                description,
                code,
                timeBase,
                repBase,
                restPattern,
                intensityLevel,
                displayOrder,
                isActive
            );
        }
        
        public static EntityResult<ExecutionProtocol> Create(
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
            return Validate.For<ExecutionProtocol>()
                .EnsureNotEmpty(value, ExecutionProtocolErrorMessages.ValueCannotBeEmptyEntity)
                .EnsureMaxLength(value, 100, ExecutionProtocolErrorMessages.ValueTooLong)
                .EnsureNotEmpty(code, ExecutionProtocolErrorMessages.CodeCannotBeEmpty)
                .EnsureMaxLength(code, 50, ExecutionProtocolErrorMessages.CodeTooLong)
                .Ensure(() => CodeRegex.IsMatch(code), ExecutionProtocolErrorMessages.CodeInvalidFormat)
                .EnsureMinValue(displayOrder, 0, ExecutionProtocolErrorMessages.DisplayOrderMustBeNonNegative)
                .OnSuccess(() => new ExecutionProtocol
                {
                    ExecutionProtocolId = id,
                    Value = value,
                    Description = description,
                    Code = code,
                    TimeBase = timeBase,
                    RepBase = repBase,
                    RestPattern = restPattern,
                    IntensityLevel = intensityLevel,
                    DisplayOrder = displayOrder,
                    IsActive = isActive
                });
        }
    }
}