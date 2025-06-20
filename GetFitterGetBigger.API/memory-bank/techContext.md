# Technical Context

## Technology Stack
- **Backend**: C# Minimal API
- **API Documentation**: Swagger
- **Target Framework**: .NET (version to be determined from project files)

## Architecture
- **API-First Design**: The system is built around a central API that handles all data operations
- **Client-Server Model**: Mobile and admin applications act as clients to the API server
- **Database Abstraction**: The API encapsulates all database operations, providing a clean interface for client applications

## Key Technical Components
- **RESTful Endpoints**: For communication between clients and the API
- **Authentication/Authorization**: (To be determined based on project requirements)
- **Data Persistence**: Database access is exclusively handled by the API
- **Swagger Integration**: For API documentation and testing

## Client Applications
1. **Mobile App**:
   - Used by fitness clients
   - Consumes API endpoints for workout execution and tracking
   - No direct database access

2. **Admin App**:
   - Used by Personal Trainers
   - Consumes API endpoints for content management
   - Allows creation and management of exercises, workouts, and fitness plans
   - No direct database access
