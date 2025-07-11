using System;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct CoachNoteId
{
    private readonly Guid _value;
    
    private CoachNoteId(Guid value)
    {
        this._value = value;
    }
    
    public static CoachNoteId New() => new(Guid.NewGuid());
    
    public static CoachNoteId From(Guid guid) => new(guid);
    
    public static CoachNoteId Empty => new(Guid.Empty);
    
    public bool IsEmpty => _value == Guid.Empty;
    
    public static bool TryParse(string? input, out CoachNoteId result)
    {
        result = default;
        if (string.IsNullOrEmpty(input) || !input.StartsWith("coachnote-"))
            return false;
            
        string guidPart = input["coachnote-".Length..];
        if (Guid.TryParse(guidPart, out Guid guid))
        {
            result = From(guid);
            return true;
        }
        
        return false;
    }
    
    public static CoachNoteId ParseOrEmpty(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return Empty;
            
        if (TryParse(input, out var result))
            return result;
            
        return Empty;
    }
    
    public override string ToString() => IsEmpty ? string.Empty : $"coachnote-{this._value}";
    
    // Conversion to/from Guid for EF Core
    public static implicit operator Guid(CoachNoteId id) => id._value;
}