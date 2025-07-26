# Table Component Strategies

This directory contains strategy classes that map reference table names to their corresponding Blazor components.

## Design Principles

Each reference table has its own strategy class, even if they use the same generic component. This design provides several benefits:

### 1. **No Magic Strings**
Display names and table names are encapsulated within each strategy class, eliminating magic strings from Program.cs.

### 2. **Localization Ready**
Each strategy can implement its own localization logic:
```csharp
public class BodyPartsTableStrategy : TableComponentStrategyBase
{
    private readonly IStringLocalizer<BodyPartsTableStrategy> _localizer;
    
    public BodyPartsTableStrategy(IStringLocalizer<BodyPartsTableStrategy> localizer)
    {
        _localizer = localizer;
    }
    
    public override string DisplayName => _localizer["DisplayName"];
}
```

### 3. **Single Responsibility**
Each strategy is responsible for one and only one table, making the code easy to understand and maintain.

### 4. **Open/Closed Principle**
Adding a new table requires only:
1. Create a new strategy class
2. Register it in Program.cs
3. No modification to existing code

### 5. **Future Extensibility**
Individual strategies allow for table-specific behavior:
- Custom component parameters
- Table-specific validation
- Different components for different scenarios
- Feature flags per table

## Adding a New Reference Table

1. Create a new strategy class:
```csharp
public class YourTableNameTableStrategy : TableComponentStrategyBase
{
    public override string TableName => "YourTableName";
    public override string DisplayName => "Your Table Display Name";
    public override Type ComponentType => typeof(Components.Pages.ReferenceTableComponents.YourComponent);
}
```

2. Register in Program.cs:
```csharp
builder.Services.AddScoped<ITableComponentStrategy, YourTableNameTableStrategy>();
```

3. Update test helper if needed

## Scalability

This pattern scales well to 100+ tables because:
- Each table is independent
- No central registry to modify
- Can be organized into subdirectories by domain
- Supports assembly scanning for auto-registration
- Can be generated from database schema or configuration