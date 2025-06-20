# Technical Context

> For a comprehensive overview of the technology stack across the entire ecosystem, please refer to the [Shared Memory Bank](/Shared/memory-bank/techContext.md).

## Admin-Specific Technology Stack

The GetFitterGetBigger Admin Application is built using the following technologies:

### Core Framework
- **Blazor**: Microsoft's web framework for building interactive web UIs using C# instead of JavaScript
- **.NET 9.0**: The latest version of Microsoft's development platform

### Frontend
- **Tailwind CSS**: Utility-first CSS framework for rapid UI development
- **Blazor Components**: Custom and built-in UI components
- **Browser APIs**: For client-side functionality when needed

### Backend Communication
- **HttpClient**: For API communication
- **System.Text.Json**: For JSON serialization/deserialization
- **IConfiguration**: For managing application settings

## Development Setup

### Prerequisites
- .NET 9.0 SDK
- Node.js and npm (for Tailwind CSS processing)
- Git

### Configuration Files
- **appsettings.json**: Contains production configuration
- **appsettings.Development.json**: Contains development-specific overrides
- **GetFitterGetBigger.Admin.csproj**: Project configuration
- **tailwind.config.js**: Tailwind CSS configuration

## Technical Constraints

### API Dependency
- The application is entirely dependent on the API for data persistence
- No local database or storage mechanisms are used for business data
- API availability is critical for application functionality

### Browser Compatibility
- The application targets modern browsers with WebAssembly support
- IE11 and older browsers are not supported
- Progressive enhancement techniques should be used where possible

### Performance Considerations
- Large datasets (exercise libraries, client lists) must be paginated
- Image and media handling should be optimized
- Initial load time should be minimized

## Dependencies

### External Packages
- **Microsoft.AspNetCore.Components.WebAssembly**: Core Blazor WebAssembly framework
- **Microsoft.Extensions.Http**: HTTP client factory for API communication
- **Microsoft.AspNetCore.Authentication.Google**: Google authentication provider
- **Microsoft.AspNetCore.Authentication.Facebook**: Facebook authentication provider
- **Microsoft.AspNetCore.Authentication.Cookies**: Cookie-based authentication
- **Microsoft.AspNetCore.Authorization**: Authorization policies and handlers
- **Tailwind CSS**: Styling framework
- Additional NuGet packages as required for specific functionality

### Internal Dependencies
- API endpoints for all data operations
- Authentication services for user management
- Shared models and DTOs for data consistency

## Tool Usage Patterns

### Authentication System

The authentication system uses external providers and cookie-based authentication:

1. **External Authentication Providers**
   - Google and Facebook OAuth integration
   - Configuration in appsettings.json
   - Claims mapping for user information

2. **Authentication Flow**
   - Redirect to login page for unauthenticated users
   - External provider authentication
   - Cookie-based session management
   - Automatic redirection to original requested URL

3. **Authorization**
   - Policy-based authorization
   - Route protection with AuthorizeRouteView
   - Custom authorization handlers

4. **User Profile**
   - Display of user information (email, profile picture)
   - Logout functionality
   - Profile information stored in claims

### Tailwind CSS Integration

The integration of Tailwind CSS with Blazor requires specific setup and usage patterns:

1. **Installation and Configuration**
   - Node.js and npm for Tailwind CSS tooling
   - PostCSS for processing Tailwind directives
   - Configuration file (tailwind.config.js) for customization

2. **Build Process**
   - Tailwind CLI to process CSS during build
   - Integration with .NET build pipeline
   - Purging unused CSS in production builds

3. **Usage in Blazor**
   - Utility classes applied directly to HTML elements
   - Component-specific styles using Blazor's scoped CSS
   - Responsive design using Tailwind's breakpoint utilities

4. **Best Practices**
   - Consistent use of utility classes
   - Component extraction for repeated UI patterns
   - Theme customization through Tailwind configuration

### API Communication

1. **Configuration**
   - Base URL configured in appsettings.json
   - Endpoint composition handled by service layer
   - Environment-specific configurations

2. **Service Pattern**
   - Interface-based services for API communication
   - Dependency injection for service consumption
   - Typed HTTP clients for different API areas

3. **Error Handling**
   - Consistent error handling across API calls
   - User-friendly error messages
   - Retry mechanisms for transient failures

### State Management

1. **Component State**
   - Parameters and cascading values for component configuration
   - EventCallbacks for component communication

2. **Application State**
   - Services for cross-component state
   - Local storage for persistent state where appropriate
   - State containers for complex state management
