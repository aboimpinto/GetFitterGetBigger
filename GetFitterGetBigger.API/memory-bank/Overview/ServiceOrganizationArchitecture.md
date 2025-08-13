# Service Organization Architecture

## Overview
This document defines the folder structure and organization patterns for services in the GetFitterGetBigger API. This architecture is designed to scale from a single service to hundreds of services while maintaining clarity, avoiding redundancy, and preventing the creation of God Objects.

## Core Principles

### 1. **Singular Domain Names**
- Use singular form for domain folders: `WorkoutTemplate`, NOT `WorkoutTemplates`
- The folder represents the domain concept, not a collection

### 2. **Context Through Folders, Not Class Names**
- Avoid redundant naming: `EquipmentRequirementsService` NOT `WorkoutTemplateEquipmentRequirementsService`
- The namespace provides context: `Services.WorkoutTemplate.Features.Equipment`

### 3. **Features vs Handlers**
- **Features**: Complex services with interfaces, DI, and external consumers
- **Handlers**: Simple internal classes for offloading logic from main services

## Folder Structure Pattern

```
Services/
└── [DomainName]/                           // e.g., WorkoutTemplate
    ├── I[DomainName]Service.cs            // Core facade interface
    ├── [DomainName]Service.cs             // Core implementation
    │
    ├── Features/                          // Complex sub-services
    │   └── [FeatureName]/                 // Only if multiple files
    │       ├── I[Role]Service.cs          // Interface with role-based name
    │       └── [Role]Service.cs           // Implementation
    │
    ├── Handlers/                          // Simple internal handlers
    │   ├── [Action]Handler.cs             // No interface needed
    │   └── [Process]Handler.cs            // Single responsibility
    │
    ├── Models/                            // Domain-specific models
    │   ├── DTOs/                          // Data transfer objects
    │   ├── Enums/                         // Enumerations
    │   └── Constants/                     // Domain constants
    │
    └── Extensions/                        // Extension methods
        └── [Domain]Extensions.cs          // Query, validation, etc.
```

## Decision Tree: Feature vs Handler

### Create a Feature Service when:
- ✅ Other services/controllers need to call it
- ✅ It has complex logic with multiple public methods
- ✅ It requires its own dependencies (repositories, external services)
- ✅ It needs to be mocked in unit tests
- ✅ It represents a distinct business capability

### Create a Handler when:
- ✅ Only the parent service uses it
- ✅ It has a single responsibility
- ✅ It's extracting complexity from the main service
- ✅ It has minimal or no dependencies
- ✅ It's an internal implementation detail

## Naming Conventions

### Features Services
Use **role-based names** that describe what the service does:

```csharp
// ✅ GOOD - Clear role-based names
EquipmentRequirementsService   // Manages equipment requirements
StatisticsService              // Calculates statistics
ProfitabilityService          // Analyzes profitability
ReputationService             // Manages reputation

// ❌ BAD - Redundant domain prefix
WorkoutTemplateEquipmentService
WorkoutTemplateStatisticsService
```

### Handlers
Use **action-based names** that describe the operation:

```csharp
// ✅ GOOD - Clear action names
DuplicationHandler            // Handles duplication logic
StateTransitionHandler        // Manages state transitions
ValidationHandler             // Performs validations
SuggestionEngine             // Generates suggestions
```

## Real Example: WorkoutTemplate Service

```
Services/
└── WorkoutTemplate/
    ├── IWorkoutTemplateService.cs
    ├── WorkoutTemplateService.cs           // ~200 lines (down from 771!)
    │
    ├── Features/
    │   ├── Equipment/
    │   │   ├── IEquipmentRequirementsService.cs
    │   │   └── EquipmentRequirementsService.cs   // ~230 lines
    │   ├── Statistics/                     // Future: workout execution stats
    │   │   ├── IStatisticsService.cs
    │   │   └── StatisticsService.cs
    │   ├── Profitability/                  // Future: earnings analysis
    │   │   ├── IProfitabilityService.cs
    │   │   └── ProfitabilityService.cs
    │   └── Reputation/                      // Future: ratings & reviews
    │       ├── IReputationService.cs
    │       └── ReputationService.cs
    │
    ├── Handlers/
    │   ├── DuplicationHandler.cs           // ~80 lines extracted
    │   ├── StateTransitionHandler.cs       // ~100 lines extracted
    │   ├── SearchQueryBuilder.cs           // ~150 lines extracted
    │   └── SuggestionEngine.cs            // ~80 lines extracted
    │
    ├── Models/
    │   └── DTOs/
    │       ├── EquipmentUsageDto.cs
    │       ├── EquipmentAvailabilityDto.cs
    │       └── WorkoutStatisticsDto.cs
    │
    └── Extensions/
        ├── WorkoutTemplateQueryExtensions.cs
        └── WorkoutTemplateValidationExtensions.cs
```

## Dependency Injection Registration

```csharp
// Program.cs or ServiceExtensions.cs
services.AddTransient<IWorkoutTemplateService, WorkoutTemplateService>();

// Feature services (have interfaces)
services.AddTransient<IEquipmentRequirementsService, EquipmentRequirementsService>();
services.AddTransient<IStatisticsService, StatisticsService>();

// Handlers (typically not registered, created in service constructor)
// OR registered if they have dependencies:
services.AddTransient<DuplicationHandler>();
```

## Future Domain Examples

### Exercise Domain
```
Services/
└── Exercise/
    ├── IExerciseService.cs
    ├── ExerciseService.cs
    ├── Features/
    │   ├── Links/                    // Exercise relationships
    │   ├── Coaching/                 // Coach notes & tips
    │   └── Biomechanics/            // Movement analysis
    ├── Handlers/
    │   ├── MuscleGroupAssignmentHandler.cs
    │   └── RestExerciseValidationHandler.cs
    └── Models/
```

### WorkoutPlan Domain (Future)
```
Services/
└── WorkoutPlan/
    ├── IWorkoutPlanService.cs
    ├── WorkoutPlanService.cs
    ├── Features/
    │   ├── Scheduling/               // Calendar integration
    │   ├── Progress/                 // Progress tracking
    │   └── Adaptation/              // Auto-adjustment based on performance
    ├── Handlers/
    │   ├── PlanGenerationHandler.cs
    │   └── ProgressCalculationHandler.cs
    └── Models/
```

### Meal Domain (Future)
```
Services/
└── Meal/
    ├── IMealService.cs
    ├── MealService.cs
    ├── Features/
    │   ├── Nutrition/                // Nutritional analysis
    │   ├── Recipes/                  // Recipe management
    │   └── Shopping/                 // Shopping list generation
    ├── Handlers/
    │   ├── MacroCalculationHandler.cs
    │   └── IngredientSubstitutionHandler.cs
    └── Models/
```

## Benefits of This Architecture

1. **Eliminates God Objects**: Main service stays under 300 lines
2. **Scales Naturally**: Easy to add new features without cluttering
3. **Clear Boundaries**: Obvious where code belongs
4. **No Redundancy**: Context from folders, not repetitive class names
5. **Team Friendly**: Multiple developers can work without conflicts
6. **Testable**: Clear separation of concerns
7. **Discoverable**: Related code is grouped logically

## Migration Strategy

### Phase 1: Extract Features (High Value)
1. Identify external consumers → Create Feature Service
2. Move logic and create interface
3. Update DI registration
4. Update consuming services

### Phase 2: Extract Handlers (Cleanup)
1. Identify internal complexity → Create Handler
2. Move logic (no interface needed)
3. Instantiate in main service
4. Simplify main service code

### Phase 3: Organize Models
1. Move DTOs to Models/DTOs
2. Move enums to Models/Enums
3. Update namespaces

## Anti-Patterns to Avoid

❌ **Single-file folders**: Don't create a folder for one file
❌ **Interface everything**: Handlers don't need interfaces
❌ **Redundant naming**: WorkoutTemplateWorkoutTemplateService
❌ **Deep nesting**: Keep it at 3-4 levels max
❌ **Generic names**: Avoid "Manager", "Helper", "Utility"
❌ **Mixed responsibilities**: Keep Features and Handlers separate

## Success Metrics

- Main service < 300 lines
- Features services < 250 lines  
- Handlers < 100 lines
- Clear single responsibility for each class
- No circular dependencies
- Easy to find code based on functionality

## Conclusion

This architecture provides a scalable, maintainable approach to organizing services that will grow with the application. It prevents God Objects while avoiding over-engineering, striking the right balance between simplicity and structure.