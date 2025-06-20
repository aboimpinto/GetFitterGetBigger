# Technical Context

## Technology Stack

The GetFitterGetBigger ecosystem uses the following technologies across its components:

### Core Technologies

| Technology | Version | Used In | Purpose |
|------------|---------|---------|---------|
| .NET | 9.0 | All Components | Core development platform |
| C# | 12.0 | All Components | Primary programming language |
| JSON | - | All Components | Data interchange format |
| JWT | - | All Components | Authentication mechanism |

### Component-Specific Technologies

#### Admin Application
- **Blazor**: Web framework for building interactive web UIs using C#
- **Tailwind CSS**: Utility-first CSS framework for styling
- **HttpClient**: For API communication

#### API Application
- **ASP.NET Core Minimal API**: Lightweight framework for building HTTP APIs
- **Swagger/OpenAPI**: API documentation and testing
- **Entity Framework Core**: ORM for database access

#### Client Applications
- **Avalonia UI**: Cross-platform UI framework for .NET
- **MVVM Pattern**: Architecture pattern for UI development
- **Olimpo Libraries**: Custom libraries for bootstrapping, navigation, and event aggregation

### Shared Models (This Project)
- **C# Class Library**: For defining shared data models
- **XML Documentation**: For documenting model properties and usage

## Development Setup

### Prerequisites
- .NET 9.0 SDK
- Visual Studio 2025 or Visual Studio Code with C# extension
- Git for version control

### Project Structure
- **GetFitterGetBigger.Admin**: Blazor web application for Personal Trainers
- **GetFitterGetBigger.API**: C# Minimal API for data processing
- **GetFitterGetBigger.Clients**: Avalonia UI applications for clients
- **Shared**: Shared models and utilities (this project)

## Technical Constraints

### Cross-Component Constraints
- All components must use the shared models for data consistency
- API endpoints must be documented in the api-docs folder
- Authentication must be handled consistently across all components

### API Constraints
- Must provide endpoints for both Admin and Client applications
- Must handle all database operations
- Must implement proper validation and error handling

### Admin Application Constraints
- Must work in modern browsers
- Must be responsive for different screen sizes
- Must communicate exclusively through the API

### Client Applications Constraints
- Must work on Android, iOS, Web, and Desktop
- Must handle offline scenarios gracefully
- Must provide a consistent user experience across platforms

## Dependencies

### External Dependencies
- **Microsoft.Extensions.DependencyInjection**: For dependency injection
- **System.Text.Json**: For JSON serialization/deserialization
- **Microsoft.AspNetCore.Authentication.JwtBearer**: For JWT authentication

### Internal Dependencies
- **Shared Models**: Used by all components for data consistency
- **API Endpoints**: Used by Admin and Client applications for data access
- **Authentication Services**: Used by all components for secure access

## Tool Usage Patterns

### API Documentation
- Each API endpoint is documented in its own file in the api-docs folder
- Documentation includes metadata indicating which projects use the endpoint
- The memory bank update script propagates changes to the relevant projects

### Shared Models
- Models are defined in the Shared project
- XML documentation is used to document model properties and usage
- Models are referenced by all components

### Version Control
- Git is used for version control
- Each component has its own folder in the repository
- Shared code is in the Shared folder

### Continuous Integration/Deployment
- CI/CD pipeline builds and tests all components
- Automated deployment to development, staging, and production environments
- Versioning follows semantic versioning principles
