# Database Model Implementation Pattern

This document outlines the standardized approach for implementing database models in the GetFitterGetBigger API using Entity Framework Core. It covers the entity design pattern, specialized ID types, handler classes, and DbContext configuration.

## Table of Contents

1. [Entity Framework Core Setup](#entity-framework-core-setup)
2. [Entity Design Pattern](#entity-design-pattern)
3. [Specialized ID Types](#specialized-id-types)
4. [Handler Class Pattern](#handler-class-pattern)
5. [DbContext Configuration](#dbcontext-configuration)
6. [Integration with UnitOfWork](#integration-with-unitofwork)
7. [Coding Standards](#coding-standards)

## Entity Framework Core Setup

### Required NuGet Packages

```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
```

### Configuration in Program.cs

```csharp
// Add DbContext to the service collection
builder.Services.AddDbContext<FitnessDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### Connection String Setup (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=GetFitterGetBigger;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### Migration Commands

```bash
# Create a new migration
dotnet ef migrations add InitialCreate

# Apply migrations to the database
dotnet ef database update

# Remove the last migration
dotnet ef migrations remove
```

## Entity Design Pattern

All entities in the GetFitterGetBigger API should be implemented as C# records with the following characteristics:

### Key Characteristics

- Implemented as C# records for built-in value equality and immutability
- Each entity has a specialized ID type (not just a raw Guid)
- Properties are immutable (using `init` setters)
- Each entity has a static Handler class for creation and manipulation
- Private constructor to force usage of the Handler class

### Basic Entity Structure

```csharp
public record EntityName
{
    public EntityNameId Id { get; init; }
    public string Name { get; init; } = string.Empty;
    // Other properties...
    
    // Navigation properties if applicable
    
    // Private constructor to force usage of Handler
    private EntityName() { }
    
    // Static Handler class (see Handler Class Pattern section)
    public static class Handler
    {
        // Handler methods...
    }
}
```

### Benefits of Using Records

- Automatic value equality (two entities with the same property values are considered equal)
- Immutability helps prevent unintended state changes
- Concise syntax for creating and copying entities
- Built-in `ToString()` method that displays all properties

## Specialized ID Types

Each entity should have its own specialized ID type that wraps a Guid. This provides type safety and domain-specific string representation.

### Key Characteristics

- Implemented as a readonly record struct
- Contains a private Guid field
- Provides factory methods for creation
- Overrides `ToString()` to return a domain-specific string representation
- Provides parsing methods to convert from string back to ID

### Implementation Template

```csharp
public readonly record struct EntityNameId
{
    private readonly Guid _value;
    
    private EntityNameId(Guid value)
    {
        this._value = value;
    }
    
    public static EntityNameId New() => new(Guid.NewGuid());
    
    public static EntityNameId From(Guid guid) => new(guid);
    
    public static bool TryParse(string? input, out EntityNameId result)
    {
        result = default;
        if (string.IsNullOrEmpty(input) || !input.StartsWith("entityname-"))
            return false;
            
        string guidPart = input["entityname-".Length..];
        if (Guid.TryParse(guidPart, out Guid guid))
        {
            result = From(guid);
            return true;
        }
        
        return false;
    }
    
    public override string ToString() => $"entityname-{this._value}";
    
    // Conversion to/from Guid for EF Core
    public static implicit operator Guid(EntityNameId id) => id._value;
}
```

### Example: WorkoutId

```csharp
public readonly record struct WorkoutId
{
    private readonly Guid _value;
    
    private WorkoutId(Guid value)
    {
        this._value = value;
    }
    
    public static WorkoutId New() => new(Guid.NewGuid());
    
    public static WorkoutId From(Guid guid) => new(guid);
    
    public static bool TryParse(string? input, out WorkoutId result)
    {
        result = default;
        if (string.IsNullOrEmpty(input) || !input.StartsWith("workout-"))
            return false;
            
        string guidPart = input["workout-".Length..];
        if (Guid.TryParse(guidPart, out Guid guid))
        {
            result = From(guid);
            return true;
        }
        
        return false;
    }
    
    public override string ToString() => $"workout-{this._value}";
    
    // Conversion to/from Guid for EF Core
    public static implicit operator Guid(WorkoutId id) => id._value;
}
```

## Handler Class Pattern

Each entity should contain a static Handler class that encapsulates the logic for creating and manipulating the entity.

### Key Characteristics

- Implemented as a static nested class within the entity record
- Provides methods for creating new entities
- Encapsulates validation logic
- Enforces business rules
- Provides two primary creation methods:
  - `CreateNew()`: Creates a new entity with a new ID
  - `Create()`: Creates an entity from existing database data

### Implementation Template

```csharp
public record EntityName
{
    // Entity properties...
    
    private EntityName() { }
    
    public static class Handler
    {
        // Creates a completely new entity
        public static EntityName CreateNew(string name /*, other parameters */)
        {
            // Validation logic
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name cannot be empty", nameof(name));
            }
            
            return new EntityName
            {
                Id = EntityNameId.New(),
                Name = name,
                // Set other properties...
            };
        }
        
        // Creates an entity from existing database data
        public static EntityName Create(EntityNameId id, string name /*, other parameters */)
        {
            return new EntityName
            {
                Id = id,
                Name = name,
                // Set other properties...
            };
        }
        
        // Other handler methods as needed...
    }
}
```

### Example: Workout Handler

```csharp
public record Workout
{
    public WorkoutId Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public TimeSpan Duration { get; init; }
    
    // Navigation properties
    public ICollection<Exercise> Exercises { get; init; } = new List<Exercise>();
    
    private Workout() { }
    
    public static class Handler
    {
        public static Workout CreateNew(string name, string description, TimeSpan duration)
        {
            // Validation logic
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name cannot be empty", nameof(name));
            }
            
            return new Workout
            {
                Id = WorkoutId.New(),
                Name = name,
                Description = description,
                Duration = duration
            };
        }
        
        public static Workout Create(WorkoutId id, string name, string description, TimeSpan duration)
        {
            return new Workout
            {
                Id = id,
                Name = name,
                Description = description,
                Duration = duration
            };
        }
    }
}
```

## DbContext Configuration

The DbContext should be configured to handle the specialized ID types and entity relationships.

### Key Characteristics

- DbSet properties for each entity
- Configuration for specialized ID types using value converters
- Configuration for entity relationships
- Configuration for any additional constraints or indexes

### Implementation Template

```csharp
public class FitnessDbContext : DbContext
{
    // DbSet properties
    public DbSet<EntityName1> EntityName1s => Set<EntityName1>();
    public DbSet<EntityName2> EntityName2s => Set<EntityName2>();
    // Other DbSets...
    
    public FitnessDbContext(DbContextOptions<FitnessDbContext> options) 
        : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure specialized ID types
        modelBuilder.Entity<EntityName1>()
            .Property(e => e.Id)
            .HasConversion(
                id => (Guid)id,
                guid => EntityName1Id.From(guid));
                
        // Similar configurations for other ID types...
        
        // Entity relationship configurations...
    }
}
```

### Example: FitnessDbContext

```csharp
public class FitnessDbContext : DbContext
{
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<Plan> Plans => Set<Plan>();
    
    public FitnessDbContext(DbContextOptions<FitnessDbContext> options) 
        : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure WorkoutId to be stored as Guid
        modelBuilder.Entity<Workout>()
            .Property(w => w.Id)
            .HasConversion(
                id => (Guid)id,
                guid => WorkoutId.From(guid));
                
        // Configure ExerciseId to be stored as Guid
        modelBuilder.Entity<Exercise>()
            .Property(e => e.Id)
            .HasConversion(
                id => (Guid)id,
                guid => ExerciseId.From(guid));
                
        // Configure PlanId to be stored as Guid
        modelBuilder.Entity<Plan>()
            .Property(p => p.Id)
            .HasConversion(
                id => (Guid)id,
                guid => PlanId.From(guid));
                
        // Entity relationship configurations...
    }
}
```

## Integration with UnitOfWork

The database model implementation will integrate with an existing UnitOfWork pattern. This section acknowledges this future integration.

### Key Points

- The UnitOfWork pattern will be implemented in a separate project
- Entities and DbContext will be designed to work with the UnitOfWork pattern
- The UnitOfWork will handle transaction management and repository access
- Detailed documentation for the UnitOfWork pattern will be provided separately

## Coding Standards

To maintain consistency across the codebase, the following coding standards should be followed:

### Naming Conventions

- **Private Fields**: Use underscore prefix
  ```csharp
  private readonly string _name;
  private int _count;
  ```

- **Properties**: Use PascalCase without prefixes
  ```csharp
  public string Name { get; set; }
  public int Count { get; private set; }
  ```

### this. Qualifier Usage

- **Always use `this.` qualifier for:**
  - Accessing class fields
    ```csharp
    this._name = "Example";
    ```
  - Accessing properties
    ```csharp
    var length = this.Name.Length;
    ```
  - Calling methods
    ```csharp
    this.Initialize();
    ```

- **Benefits of explicit `this.` usage:**
  - Clearly distinguishes class members from local variables
  - Improves code readability, especially in complex methods
  - Helps avoid naming conflicts
  - Makes the code more self-documenting
  - Consistent style across the codebase

### Example of Consistent Style

```csharp
public class Example
{
    private readonly string _name;
    private int _count;
    
    public string Name => this._name;
    
    public Example(string name)
    {
        this._name = name;
        this._count = 0;
        this.Initialize();
    }
    
    private void Initialize()
    {
        this._count++;
        this.LogInitialization();
    }
    
    public void Process(string input)
    {
        // Local variable (no this.)
        var processedInput = input.Trim();
        
        // Class member (with this.)
        this._count++;
        
        // Method call (with this.)
        this.LogProcessing(processedInput);
    }
    
    private void LogInitialization()
    {
        Console.WriteLine($"Initialized with name: {this._name}");
    }
    
    private void LogProcessing(string input)
    {
        Console.WriteLine($"Processing: {input}, Count: {this._count}");
    }
}
```

## Implementation Steps

When implementing this pattern for a new entity, follow these steps:

1. Create the specialized ID type for the entity
2. Create the entity record with a static Handler class
3. Add a DbSet property to the DbContext
4. Configure the specialized ID type in the DbContext
5. Configure any entity relationships in the DbContext
6. Create and apply a migration
7. Test the implementation

## Conclusion

This document outlines the standardized approach for implementing database models in the GetFitterGetBigger API. By following these patterns and standards, we ensure consistency, type safety, and maintainability across the codebase.
