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
