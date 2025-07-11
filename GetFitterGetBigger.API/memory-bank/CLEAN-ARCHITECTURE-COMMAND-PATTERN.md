# Clean Architecture: Request-Command Separation Pattern

## 🎯 **Overview**
This document describes the architectural pattern implemented to properly separate web layer concerns from service layer concerns, ensuring clean boundaries and proper domain modeling.

## 🚨 **The Problem We Solved**

### **Before: Architectural Violations**
```csharp
// ❌ BAD: Service layer directly depends on web request DTOs
public async Task<ExerciseDto> CreateAsync(CreateExerciseRequest request)
{
    // Service working with string IDs from web layer
    var difficultyId = DifficultyLevelId.Parse(request.DifficultyId);
    // Mixing web concerns with domain logic
}

// ❌ BAD: Tests using string IDs instead of domain objects
var request = UpdateExerciseRequestBuilderV2.ForWorkoutExercise()
    .AddCoachNote(existingNoteId1.ToString(), "Updated note 1", 1) // String conversion!
    .Build();
```

**Problems:**
1. **String IDs at Domain Level**: Services worked with string IDs instead of proper domain types
2. **Request DTOs in Service Layer**: Web request objects leaked into business logic
3. **Tight Coupling**: Services coupled to web request formats
4. **Assembly Boundary Violation**: Services depended on web layer DTOs

## ✅ **The Solution: Request-Command Separation**

### **Architecture Flow:**
```
Web Layer (Request DTOs) → Mapper → Service Layer (Commands) → Domain Layer
```

### **After: Clean Architecture**
```csharp
// ✅ GOOD: Service layer works with domain commands
public async Task<ExerciseDto> CreateAsync(CreateExerciseCommand command)
{
    // Service working with specialized IDs - proper domain types
    var difficulty = command.DifficultyId; // Already a DifficultyLevelId!
    // Pure domain logic, no web concerns
}

// ✅ GOOD: Tests using domain objects
var request = UpdateExerciseRequestBuilderV2.ForWorkoutExercise()
    .AddCoachNote(existingNoteId1, "Updated note 1", 1) // CoachNoteId object!
    .Build();
```

## 🏗️ **Implementation Components**

### **1. Service Command Objects**
Located in: `/Services/Commands/`

```csharp
public class CreateExerciseCommand
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    
    // ✅ Domain IDs - not strings!
    public required DifficultyLevelId DifficultyId { get; init; }
    public KineticChainTypeId? KineticChainId { get; init; }
    public required List<ExerciseTypeId> ExerciseTypeIds { get; init; }
    public required List<MuscleGroupAssignment> MuscleGroups { get; init; }
    public List<CoachNoteCommand> CoachNotes { get; init; } = new();
}
```

**Key Benefits:**
- **Type Safety**: Compile-time validation of ID types
- **Domain Modeling**: Represents business concepts, not web formats
- **Assembly Independence**: Can be moved to separate domain assembly

### **2. Request-to-Command Mappers**
Located in: `/Mappers/ExerciseRequestMapper.cs`

```csharp
public static CreateExerciseCommand ToCommand(this CreateExerciseRequest request)
{
    return new CreateExerciseCommand
    {
        Name = request.Name,
        // ✅ Convert string IDs to specialized IDs with validation
        DifficultyId = ParseDifficultyLevelId(request.DifficultyId),
        ExerciseTypeIds = ParseExerciseTypeIds(request.ExerciseTypeIds),
        CoachNotes = MapCoachNotes(request.CoachNotes),
        // ... other mappings
    };
}
```

**Responsibilities:**
- **ID Validation**: Parse and validate string IDs from JSON
- **Type Conversion**: Convert web DTOs to domain objects
- **Error Handling**: Graceful handling of invalid IDs

### **3. Updated Controller Layer**
```csharp
[HttpPost]
public async Task<IActionResult> CreateExercise([FromBody] CreateExerciseRequest request)
{
    try
    {
        // ✅ Map web request DTO to service command
        var command = request.ToCommand();
        var exercise = await _exerciseService.CreateAsync(command);
        return CreatedAtAction(nameof(GetExercise), new { id = exercise.Id }, exercise);
    }
    catch (ArgumentException ex)
    {
        // Handle mapping/validation errors
        return BadRequest(new { error = ex.Message });
    }
}
```

### **4. Enhanced Test Builders**
```csharp
// ✅ GOOD: Expressive, type-safe builder methods
var request = UpdateExerciseRequestBuilderV2.ForWorkoutExercise()
    .WithName("Updated Exercise")
    .WithKineticChain(KineticChainTypeTestBuilder.Compound())
    .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
    .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
    .AddCoachNote(existingNoteId, "Updated note", 1) // CoachNoteId object!
    .AddCoachNoteWithInvalidFormat("Test invalid", 2) // Expressive error testing
    .Build();
```

**Key Improvements:**
- **Domain Objects**: Uses `CoachNoteId`, not `string`
- **Expressive APIs**: Self-documenting methods for error scenarios
- **Type Safety**: Compile-time validation of test data

## 🎯 **Benefits Achieved**

### **1. Proper Layer Separation**
- **Web Layer**: Only knows about JSON serialization and HTTP concerns
- **Service Layer**: Only knows about domain objects and business logic
- **Domain Layer**: Pure domain concepts with no external dependencies

### **2. Type Safety**
```csharp
// ❌ Before: Runtime errors possible
service.UpdateCoachNote("invalid-id", "text", 1); // String could be anything

// ✅ After: Compile-time safety
service.UpdateCoachNote(coachNoteId, "text", 1); // Must be valid CoachNoteId
```

### **3. Assembly Independence**
Services can now be moved to separate assemblies without depending on web DTOs:
```
GetFitterGetBigger.Domain.dll      // Domain objects, IDs
GetFitterGetBigger.Services.dll    // Service layer with commands
GetFitterGetBigger.Web.dll         // Controllers, DTOs, mappers
```

### **4. Improved Testing**
- **Domain-Focused**: Tests work with business concepts
- **Self-Documenting**: Intent is clear from method names
- **Type-Safe**: Impossible to pass wrong ID types

### **5. Better Error Handling**
- **Early Validation**: ID parsing happens at web boundary
- **Clear Error Messages**: Specific feedback about invalid formats
- **Graceful Degradation**: Invalid IDs are handled appropriately

## 📝 **Implementation Guidelines**

### **For New Features:**

1. **Create Command Objects First**
   ```csharp
   public class CreateFeatureCommand
   {
       // Use specialized IDs, not strings
       public required FeatureTypeId TypeId { get; init; }
   }
   ```

2. **Create Mappers**
   ```csharp
   public static CreateFeatureCommand ToCommand(this CreateFeatureRequest request)
   {
       return new CreateFeatureCommand
       {
           TypeId = ParseFeatureTypeId(request.TypeId)
       };
   }
   ```

3. **Update Service Interface**
   ```csharp
   Task<FeatureDto> CreateAsync(CreateFeatureCommand command);
   ```

4. **Update Controller**
   ```csharp
   var command = request.ToCommand();
   var result = await _service.CreateAsync(command);
   ```

5. **Create Type-Safe Builders**
   ```csharp
   .AddRelatedItem(relatedItemId) // Not relatedItemId.ToString()!
   ```

### **For Existing Features:**

1. **Create Command Objects** for existing request DTOs
2. **Create Mappers** between requests and commands  
3. **Update Service Signatures** to use commands
4. **Update Controllers** to use mappers
5. **Update Test Builders** to use specialized IDs
6. **Update Tests** to use domain objects

## 🔍 **Code Review Checklist**

### **✅ Good Patterns:**
- [ ] Service methods accept `Command` objects, not `Request` DTOs
- [ ] Commands use specialized IDs (`ExerciseTypeId`), not strings
- [ ] Mappers handle ID parsing and validation
- [ ] Controllers use `.ToCommand()` extension methods
- [ ] Test builders accept domain objects (`CoachNoteId`)
- [ ] Tests are self-documenting with expressive methods

### **❌ Anti-Patterns to Avoid:**
- [ ] Service methods accepting web request DTOs
- [ ] String IDs in service layer or domain logic
- [ ] Direct parsing of string IDs in services
- [ ] Test builders requiring `.ToString()` calls
- [ ] Magic string IDs in tests without clear intent

## 🎯 **Long-Term Vision**

This pattern enables:

1. **Microservices**: Services can be extracted to separate deployments
2. **Domain-Driven Design**: Pure domain layer without web dependencies  
3. **Assembly Separation**: Clear boundaries between layers
4. **Better Testing**: Domain-focused, expressive tests
5. **Type Safety**: Compile-time validation throughout the stack

## 📚 **Related Documentation**

- **Test Builder Patterns**: See `ENHANCED-BUILDERS-README.md`
- **Domain Modeling**: See `databaseModelPattern.md`
- **Service Patterns**: See `service-implementation-checklist.md`

---

**Remember**: The web layer should only know about JSON and HTTP. The service layer should only know about domain objects and business logic. Mappers are the bridge between these worlds, ensuring clean separation and proper abstraction.