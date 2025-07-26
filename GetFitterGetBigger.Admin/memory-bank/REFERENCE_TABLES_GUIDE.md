# Reference Tables Guide - GetFitterGetBigger

**🎯 PURPOSE**: This guide explains the reference table pattern, the difference between reference tables and lookups, and how to implement new reference tables in the GetFitterGetBigger ecosystem.

## 📚 Understanding Reference Tables vs Lookups

### Reference Tables
Reference tables are **read-only data sets** that provide dropdown options, configuration values, or categorization data. They are:
- Managed by administrators (not end users)
- Relatively static (change infrequently)
- Cached for performance
- Used across multiple features

**Examples:**
- Body Parts (Arms, Legs, Core, etc.)
- Equipment Types (Barbell, Dumbbell, Cable, etc.)
- Difficulty Levels (Beginner, Intermediate, Advanced)
- Movement Patterns (Push, Pull, Squat, Hinge)

### Lookups
Lookups are **dynamic queries** against business entities to find specific records. They are:
- Based on user data or business logic
- Not cached (data changes frequently)
- Often filtered by context (user, date, status)
- Used for specific business operations

**Examples:**
- Finding a user's active workout plans
- Looking up exercises by muscle group
- Searching for available trainers
- Finding equipment by gym location

## 🏗️ Reference Table Architecture

### Overview
The reference table system uses a **Strategy Pattern** to provide a scalable, maintainable solution for managing reference data with built-in caching.

```
┌─────────────────┐
│ Component/Page  │
└────────┬────────┘
         │ Requests reference data
         ▼
┌─────────────────────────┐
│ ReferenceDataService    │ ◄── Single generic method
├─────────────────────────┤
│ - GetReferenceDataAsync │
│   <T>() where T :       │
│   IReferenceTableEntity │
└────────┬────────────────┘
         │ Uses strategy
         ▼
┌─────────────────────────┐
│ Strategy Pattern        │
├─────────────────────────┤
│ - BodyPartsStrategy     │
│ - EquipmentStrategy     │
│ - [Your Strategy]       │
└─────────────────────────┘
         │
         ▼
┌─────────────────────────┐
│ Cache Layer             │ ◄── 24-hour cache
├─────────────────────────┤
│ IMemoryCache            │
└─────────────────────────┘
```

### Key Components

1. **ReferenceDataService**: Core service with a single generic method
2. **IReferenceTableEntity**: Marker interface for type safety
3. **IReferenceTableStrategy**: Strategy interface defining the contract
4. **Individual Strategies**: One per reference table type
5. **Automatic Registration**: Uses assembly scanning (Scrutor)

## 🚀 Adding a New Reference Table

### Step 1: Create the Type Marker

Add your new reference table type to `/Models/ReferenceData/ReferenceTableTypes.cs`:

```csharp
/// <summary>
/// Reference table for training phases (e.g., Hypertrophy, Strength, Endurance)
/// </summary>
public class TrainingPhases : IReferenceTableEntity { }
```

### Step 2: Create the Strategy

Create a new strategy in `/Services/Strategies/ReferenceTableStrategies/`:

```csharp
using GetFitterGetBigger.Admin.Models.ReferenceData;

namespace GetFitterGetBigger.Admin.Services.Strategies.ReferenceTableStrategies
{
    public class TrainingPhasesStrategy : BaseReferenceTableStrategy<TrainingPhases>
    {
        public override string Endpoint => "/api/ReferenceTables/TrainingPhases";
        public override string CacheKey => "RefData_TrainingPhases";
        
        // Optional: Override CacheDuration if different from 24 hours
        // public override TimeSpan CacheDuration => TimeSpan.FromHours(12);
        
        // Optional: Override TransformDataAsync for custom transformation
        // public override async Task<IEnumerable<ReferenceDataDto>> TransformDataAsync(string jsonResponse)
        // {
        //     // Custom transformation logic
        // }
    }
}
```

### Step 3: Use in Components

```csharp
@inject IGenericReferenceDataService ReferenceDataService

@code {
    private IEnumerable<ReferenceDataDto> trainingPhases = new List<ReferenceDataDto>();
    
    protected override async Task OnInitializedAsync()
    {
        trainingPhases = await ReferenceDataService.GetReferenceDataAsync<TrainingPhases>();
    }
}
```

### Step 4: Create API Endpoint (API Project)

In the API project, add the endpoint to `ReferenceTablesController`:

```csharp
[HttpGet("TrainingPhases")]
public async Task<ActionResult<IEnumerable<ReferenceDataDto>>> GetTrainingPhases()
{
    // Implementation to fetch from database
}
```

## 📋 Complete Example: Adding "Tempo Types" Reference Table

### 1. Type Marker
```csharp
// In ReferenceTableTypes.cs
/// <summary>
/// Reference table for exercise tempo patterns (e.g., 2-0-2, 3-1-3)
/// </summary>
public class TempoTypes : IReferenceTableEntity { }
```

### 2. Strategy Implementation
```csharp
// In /Services/Strategies/ReferenceTableStrategies/TempoTypesStrategy.cs
namespace GetFitterGetBigger.Admin.Services.Strategies.ReferenceTableStrategies
{
    public class TempoTypesStrategy : BaseReferenceTableStrategy<TempoTypes>
    {
        public override string Endpoint => "/api/ReferenceTables/TempoTypes";
        public override string CacheKey => "RefData_TempoTypes";
    }
}
```

### 3. Component Usage
```razor
@page "/exercises/create"
@inject IGenericReferenceDataService ReferenceDataService

<h3>Create Exercise</h3>

<EditForm Model="exercise" OnValidSubmit="HandleSubmit">
    <div class="form-group">
        <label>Tempo Type:</label>
        <InputSelect @bind-Value="exercise.TempoTypeId">
            <option value="">Select tempo...</option>
            @foreach (var tempo in tempoTypes)
            {
                <option value="@tempo.Id">@tempo.Value</option>
            }
        </InputSelect>
    </div>
</EditForm>

@code {
    private Exercise exercise = new();
    private IEnumerable<ReferenceDataDto> tempoTypes = new List<ReferenceDataDto>();
    
    protected override async Task OnInitializedAsync()
    {
        tempoTypes = await ReferenceDataService.GetReferenceDataAsync<TempoTypes>();
    }
}
```

### 4. Testing
```csharp
[Fact]
public async Task GetReferenceDataAsync_TempoTypes_ReturnsCorrectData()
{
    // Arrange
    var expectedData = new List<ReferenceDataDto>
    {
        new() { Id = "1", Value = "2-0-2", Description = "2 sec down, 0 pause, 2 sec up" }
    };
    _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedData);
    
    // Act
    var result = await _referenceDataService.GetReferenceDataAsync<TempoTypes>();
    
    // Assert
    result.Should().HaveCount(1);
    result.First().Value.Should().Be("2-0-2");
}
```

## 🔑 Key Benefits

1. **Scalability**: Add new reference tables without modifying the service
2. **Type Safety**: Compile-time checking with marker interfaces
3. **Performance**: Built-in 24-hour caching
4. **Maintainability**: Each reference table has its own strategy
5. **Testability**: Easy to mock and test individual strategies
6. **Consistency**: All reference tables follow the same pattern

## ⚠️ Important Considerations

### Cache Keys
- Must be unique across the application
- Follow pattern: `RefData_[EntityName]`
- Avoid conflicts with other services

### Strategy Registration
- Automatic via assembly scanning in `Program.cs`
- No manual registration needed
- Strategies discovered at startup

### Error Handling
- Service returns empty collection on errors
- Logs errors for diagnostics
- Never throws exceptions to UI

### Performance
- First call fetches from API
- Subsequent calls use cache (24 hours)
- Cache cleared on application restart

## 🧪 Testing Reference Tables

### Unit Testing the Service
```csharp
[Fact]
public async Task NewReferenceTable_FollowsExpectedPattern()
{
    // Test cache hit/miss
    // Test error handling
    // Test data transformation
}
```

### Integration Testing
```csharp
[Fact]
public async Task ReferenceTable_EndToEnd_WorksCorrectly()
{
    // Test from UI component to API
    // Verify caching behavior
    // Test concurrent access
}
```

## 📚 Related Documentation

- `CODE_QUALITY_STANDARDS.md` - General coding standards
- `FEATURE_IMPLEMENTATION_PROCESS.md` - How to implement features
- `API-INTEGRATION-GUIDE.md` - API integration patterns

---

## 💡 Quick Reference Card

**Adding a new reference table? Follow these 4 steps:**

1. ✅ Add type to `ReferenceTableTypes.cs`
2. ✅ Create strategy in `/Services/Strategies/ReferenceTableStrategies/`
3. ✅ Use `GetReferenceDataAsync<YourType>()` in components
4. ✅ Implement API endpoint in API project

**Remember:**
- Strategies are auto-registered
- Cache is automatic (24 hours)
- Always returns data (empty on error)
- Type safety via marker interfaces