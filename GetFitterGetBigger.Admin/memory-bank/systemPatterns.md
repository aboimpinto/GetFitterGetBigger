# System Patterns

## System Architecture

The GetFitterGetBigger ecosystem follows a distributed architecture pattern with specialized applications communicating through a central API layer:

```
┌─────────────────────┐      ┌─────────────────────┐
│                     │      │                     │
│  Admin Application  │◄────►│    API Service      │◄────┐
│  (Blazor Web App)   │      │                     │     │
│                     │      │                     │     │
└─────────────────────┘      └─────────────────────┘     │
                                      ▲                  │
                                      │                  │
                                      ▼                  ▼
                             ┌─────────────────────┐    ┌─────────────────────┐
                             │                     │    │                     │
                             │  Mobile Application │    │      Database       │
                             │  (Client-facing)    │    │                     │
                             │                     │    │                     │
                             └─────────────────────┘    └─────────────────────┘
```

### Key Components

1. **Admin Application (Current Project)**
   - Blazor-based web application
   - Responsible for content creation and management
   - Communicates exclusively through the API layer
   - No direct database access

2. **API Service**
   - Central communication hub
   - Implements business logic and data validation
   - Manages database interactions
   - Provides standardized endpoints for all applications

3. **Mobile Application**
   - Client-facing interface
   - Consumes training content created in the Admin app
   - Provides progress tracking and feedback mechanisms
   - Communicates through the same API layer

4. **Database**
   - Centralized data storage
   - Only accessed through the API layer
   - Stores all system data (exercises, workouts, plans, users, etc.)

## Key Technical Decisions

### 1. API-First Architecture

The decision to implement an API-first architecture provides several benefits:

- **Separation of Concerns**: Each application focuses on its specific role
- **Consistency**: Business logic centralized in the API layer
- **Scalability**: Applications can be scaled independently
- **Technology Flexibility**: Different technologies can be used for each application
- **Future-Proofing**: New client applications can be added without changing the core system

### 2. Configuration-Based API Connection

The Admin application uses a configuration-based approach for API connectivity:

- **Configuration File**: API endpoint URL stored in application settings
- **Business Logic Layer**: Responsible for composing specific endpoint addresses
- **Benefits**: 
  - Environments can be easily switched (development, staging, production)
  - API versioning can be managed centrally
  - Authentication tokens and other connection details can be configured

### 3. Blazor Framework Selection

Blazor was selected as the framework for the Admin application for several reasons:

- **C# Throughout**: Allows for shared code and models with the API layer
- **Component-Based Architecture**: Facilitates reusable UI elements
- **Rich UI Capabilities**: Supports complex interfaces needed for workout builders
- **Microsoft Ecosystem**: Integrates well with other Microsoft technologies

### 4. Tailwind CSS Integration

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
   - External authentication providers (Google, Facebook)
   - Cookie-based authentication
   - Authorization policies
   - User profile management
   - Secure routing with automatic redirection

2. **API Communication Layer**
   - HTTP client configuration
   - Authentication handling
   - Request/response processing
   - Error handling and retry logic

2. **Exercise Management Module**
   - Exercise creation and editing
   - Categorization and tagging
   - Media attachment (images, videos)
   - Search and filtering

3. **Workout Builder**
   - Exercise selection and sequencing
   - Set/rep/rest parameter configuration
   - Workout templates and variations
   - Validation and completeness checking

4. **Training Plan Composer**
   - Calendar-based workout assignment
   - Plan templates and customization
   - Client assignment and tracking
   - Progress visualization
