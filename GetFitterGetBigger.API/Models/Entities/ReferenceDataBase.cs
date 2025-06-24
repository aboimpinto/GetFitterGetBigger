namespace GetFitterGetBigger.API.Models.Entities;

public abstract record ReferenceDataBase
{
    public string Value { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; } = true;
    
    protected ReferenceDataBase() { }
}
