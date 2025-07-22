---
allowed-tools:
  - Read
  - Write
  - Edit
  - MultiEdit
  - Bash
  - Grep
  - Glob
  - Task
  - TodoWrite
description: Create a new API endpoint with controller, service, repository, and tests
argument-hint: <feature-name> <endpoint-path> <http-method>
---

# Create API Endpoint for GetFitterGetBigger

This command creates a new API endpoint following the project's clean architecture pattern and established conventions.

## Purpose
Rapidly scaffold a complete API endpoint with all required layers, following SOLID principles and the project's architectural patterns.

## Arguments Required
1. **Feature name** (e.g., "WorkoutTemplate", "MealPlan")
2. **Endpoint path** (e.g., "/api/workout-templates", "/api/meal-plans")
3. **HTTP method** (e.g., "GET", "POST", "PUT", "DELETE")

## Implementation Layers

### 1. Controller Layer
- Location: `GetFitterGetBigger.API/Controllers/`
- Responsibilities:
  - HTTP request handling
  - Authorization enforcement
  - Request validation
  - Response formatting
- Patterns:
  - Dependency injection
  - Attribute-based routing
  - Swagger documentation
  - Proper HTTP status codes

### 2. Service Layer
- Location: `GetFitterGetBigger.API/Services/`
- Responsibilities:
  - Business logic implementation
  - Validation rules
  - Cross-cutting concerns
  - Entity-to-DTO mapping
- Patterns:
  - ServiceResult<T> for responses
  - Interface-based design
  - Single responsibility
  - Comprehensive error handling

### 3. Repository Layer
- Location: `GetFitterGetBigger.API/Repositories/`
- Responsibilities:
  - Data access logic
  - Query optimization
  - Database transactions
  - Entity Framework operations
- Patterns:
  - Generic repository base
  - Unit of Work integration
  - Async/await throughout
  - Include strategies for related data

### 4. Data Transfer Objects (DTOs)
- Location: `GetFitterGetBigger.API/DTOs/`
- Components:
  - Request DTOs with validation attributes
  - Response DTOs with proper serialization
  - Paged response wrappers
  - Error response models
- Naming conventions:
  - `Create{Feature}Request`
  - `Update{Feature}Request`
  - `{Feature}Dto`
  - `{Feature}FilterParams`

### 5. Testing Infrastructure

#### Unit Tests
- Service layer tests with mocking
- Repository tests with in-memory database
- Validation logic tests
- Mapping tests

#### Integration Tests
- Full request/response cycle
- Authorization scenarios
- Error handling paths
- Database state verification

#### Test Builders
- Fluent builders for test data
- Reusable test scenarios
- Mother object pattern

## Authorization Configuration
- PT-Tier: Admin operations
- Admin-Tier: Full access
- Free-Tier: Read operations
- Specific claim requirements per endpoint

## Caching Strategy
Based on entity tier:
- Pure References: Aggressive caching (24h)
- Enhanced References: Moderate caching (1h)
- Domain Entities: Minimal/no caching

## Example Usage
```
/api-endpoint WorkoutTemplate /api/workout-templates GET
/api-endpoint MealPlan /api/meal-plans POST
/api-endpoint ProgressMetric /api/progress-metrics/{id} PUT
```

## Quality Checklist
✓ Controller with proper authorization
✓ Service interface and implementation
✓ Repository interface and implementation
✓ Request/Response DTOs
✓ Input validation
✓ Unit tests (>80% coverage)
✓ Integration tests
✓ Swagger documentation
✓ Error handling
✓ Dependency injection registration

$ARGUMENTS