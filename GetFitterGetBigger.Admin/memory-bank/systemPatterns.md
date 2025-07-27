# System Patterns

> For a comprehensive overview of the entire ecosystem architecture, please refer to the [Shared Memory Bank](/Shared/memory-bank/systemPatterns.md).

## Admin Application Architecture

The GetFitterGetBigger Admin Application is a Blazor-based web application that communicates exclusively with the API layer. It has no direct database access and focuses on content creation and management for Personal Trainers.

### Key Technical Decisions

#### 1. Configuration-Based API Connection

The Admin application uses a configuration-based approach for API connectivity:

- **Configuration File**: API endpoint URL stored in application settings
- **Business Logic Layer**: Responsible for composing specific endpoint addresses
- **Benefits**: 
  - Environments can be easily switched (development, staging, production)
  - API versioning can be managed centrally
  - Authentication tokens and other connection details can be configured

#### 2. Blazor Framework Selection

Blazor was selected as the framework for the Admin application for several reasons:

- **C# Throughout**: Allows for shared code and models with the API layer
- **Component-Based Architecture**: Facilitates reusable UI elements
- **Rich UI Capabilities**: Supports complex interfaces needed for workout builders
- **Microsoft Ecosystem**: Integrates well with other Microsoft technologies

#### 3. Tailwind CSS Integration

Tailwind CSS was chosen as the styling framework:

- **Utility-First**: Provides flexibility in design without custom CSS
- **Consistency**: Enforces design system constraints
- **Performance**: Can be optimized for production builds
- **Responsive Design**: Built-in support for different screen sizes

## Component Relationships

### Data Flow Patterns

1. **Create/Update Flow**:
   - Admin App creates/modifies content
   - API validates and processes the data
   - API stores data in the database
   - Changes propagate to client applications

2. **Read Flow**:
   - Admin App requests data
   - API retrieves from database
   - API formats and returns data
   - Admin App renders the information

### State Management

- **Local State**: UI state managed within Blazor components
- **Application State**: Shared state managed through services
- **Persistence State**: Managed exclusively through API calls

## Critical Implementation Paths

1. **Authentication System**
   - **Federated Authentication**: External providers (Google, Facebook) handle the initial user login.
   - **API-Driven Sessions**: The client exchanges the user's email for a JWT from the API, which is used for all subsequent requests. This includes a sliding expiration mechanism. [Details](/memory-bank/features/api-authentication-flow.md)
   - **Authorization Policies**: Client-side and server-side policies control access to features.
   - **User Profile Management**: Handled via API interactions.
   - **Secure Routing**: Automatic redirection to the login page for unauthorized access or expired sessions.

2. **API Communication Layer**
   - HTTP client configuration
   - Authentication handling
   - Request/response processing
   - Error handling and retry logic

3. **Exercise Management Module**
   - Exercise creation and editing
   - Categorization and tagging
   - Media attachment (images, videos)
   - Search and filtering

4. **Workout Builder**
   - Exercise selection and sequencing
   - Set/rep/rest parameter configuration
   - Workout templates and variations
   - Validation and completeness checking

5. **Training Plan Composer**
   - Calendar-based workout assignment
   - Plan templates and customization
   - Client assignment and tracking
   - Progress visualization

## Layered Architecture Pattern

The Admin application implements a strict layered architecture that ensures proper separation of concerns and maintainability:

### Architecture Layers

```
┌─────────────────────────────────────┐
│      UI Layer (Blazor Components)   │
├─────────────────────────────────────┤
│    Business Layer (Services)        │
├─────────────────────────────────────┤
│    Data Layer (Data Providers)      │
├─────────────────────────────────────┤
│  Data Transport (HTTP Base Classes) │
└─────────────────────────────────────┘
```

#### 1. **UI Layer** (Presentation)
- **Location**: `/Components/`, `/Pages/`
- **Responsibilities**: 
  - User interface rendering
  - User interaction handling
  - Input validation (UI-level only)
  - State management for UI concerns
- **Example**: `WorkoutTemplateList.razor`, `ExerciseForm.razor`
- **Dependencies**: Only depends on Business Layer interfaces

#### 2. **Business Layer** (Domain Logic)
- **Location**: `/Services/`
- **Responsibilities**:
  - Business rule enforcement
  - Orchestration of data operations
  - Business validation
  - Result transformation
- **Example**: `WorkoutTemplateService`, `ExerciseService`
- **Dependencies**: Depends on Data Layer interfaces

#### 3. **Data Layer** (Data Access)
- **Location**: `/Services/DataProviders/`
- **Responsibilities**:
  - Data retrieval and persistence
  - Data source abstraction
  - Caching strategies
  - Data mapping to/from DTOs
- **Example**: `HttpWorkoutTemplateDataProvider`
- **Dependencies**: Depends on Data Transport layer

#### 4. **Data Transport Layer** (Infrastructure)
- **Location**: `/Services/DataProviders/HttpDataProviderBase.cs`
- **Responsibilities**:
  - HTTP communication mechanics
  - Error handling and mapping
  - Response deserialization
  - Logging and monitoring
- **Example**: `HttpDataProviderBase`
- **Dependencies**: None (uses framework libraries only)

### Key Principles

#### Dependency Direction
- Dependencies flow **downward only** (UI → Business → Data → Transport)
- Lower layers never know about higher layers
- Use interfaces to define contracts between layers

#### Layer Isolation
Each layer is isolated and can be tested independently:
```csharp
// UI Layer - Depends on IWorkoutTemplateService
@inject IWorkoutTemplateService WorkoutService

// Business Layer - Depends on IWorkoutTemplateDataProvider
public WorkoutTemplateService(IWorkoutTemplateDataProvider dataProvider)

// Data Layer - Depends on HttpDataProviderBase
public class HttpWorkoutTemplateDataProvider : HttpDataProviderBase

// Transport Layer - No business dependencies
public abstract class HttpDataProviderBase
```

#### Benefits of This Architecture

1. **Testability**: Each layer can be mocked and tested in isolation
2. **Maintainability**: Changes in one layer don't cascade to others
3. **Flexibility**: Easy to swap implementations (e.g., HTTP to gRPC)
4. **Clarity**: Clear responsibility boundaries
5. **Reusability**: Transport layer can be used by any data provider

### Implementation Example

```csharp
// Transport Layer - Generic, knows nothing about business entities
protected async Task<DataServiceResult<T>> ExecuteHttpGetRequestAsync<T>(
    string endpoint,
    [CallerMemberName] string callerMemberName = "")

// Data Layer - Knows about DTOs and endpoints
public async Task<DataServiceResult<WorkoutTemplateDto>> GetWorkoutTemplateByIdAsync(string id)
{
    return await ExecuteHttpGetRequestAsync<WorkoutTemplateDto>($"api/workout-templates/{id}");
}

// Business Layer - Knows about business rules and orchestration
public async Task<ServiceResult<WorkoutTemplateDto>> GetWorkoutTemplateAsync(string id)
{
    var result = await _dataProvider.GetWorkoutTemplateByIdAsync(id);
    // Apply business rules, transformations, etc.
    return result.ToServiceResult();
}

// UI Layer - Knows about user interaction
private async Task LoadWorkoutTemplate()
{
    var result = await WorkoutService.GetWorkoutTemplateAsync(TemplateId);
    // Update UI state
}
```

### Anti-Patterns to Avoid

❌ **Layer Jumping**: UI directly calling data providers
❌ **Upward Dependencies**: Data layer referencing business logic
❌ **Business Logic in UI**: Validation rules in Blazor components
❌ **Infrastructure in Business**: HTTP concerns in services
❌ **God Classes**: Classes that span multiple layers

### When to Apply This Pattern

This layered architecture should be applied to all major features in the Admin application:
- ✅ CRUD operations (Exercises, Workouts, etc.)
- ✅ Complex workflows (Workout building, Plan creation)
- ✅ Integration features (API communication)
- ✅ Reference data management
