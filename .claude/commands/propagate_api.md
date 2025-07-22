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
description: Propagate a feature from the Features directory to the API project
argument-hint: <feature-name>
---

# Propagate Feature to API Project

This command propagates a feature from the central Features directory to the GetFitterGetBigger.API project following the clean architecture pattern.

## Purpose
Transforms technology-agnostic feature specifications into properly structured API implementations with controllers, services, repositories, and comprehensive tests.

## Prerequisites
- Feature must exist in `/Features/` directory
- Feature should have complete _RAW.md documentation
- Database models should be defined

## Process Overview
1. Validate feature exists in Features directory
2. Analyze the feature's _RAW.md documentation
3. Follow the FEATURE-TO-API-PROPAGATION-PROCESS.md guidelines
4. Implement API components following clean architecture:
   - Controllers with proper authorization
   - Service interfaces and implementations
   - Repository interfaces and implementations
   - DTOs for requests and responses
   - Entity models and migrations
   - Comprehensive unit and integration tests

## What Gets Created

### Core Implementation
- **Controllers**: RESTful endpoints with Swagger documentation
- **Services**: Business logic with ServiceResult pattern
- **Repositories**: Data access with Entity Framework Core
- **DTOs**: Request/Response models with validation
- **Entities**: Database models with proper relationships
- **Migrations**: EF Core database migrations

### Testing
- **Unit Tests**: Service layer tests with mocking
- **Integration Tests**: Full API endpoint tests
- **Test Builders**: Reusable test data builders
- **BDD Tests**: Gherkin scenarios (if applicable)

## Architecture Patterns
- Clean Architecture separation
- Repository + Unit of Work pattern
- ServiceResult for error handling
- Dependency Injection throughout
- SOLID principles

## Example Usage
```
/propagate_api WorkoutTemplate
/propagate_api ExerciseWeightType
/propagate_api MealPlanning
```

## Important Considerations
- Check for existing implementations to avoid duplication
- Follow established naming conventions
- Implement proper caching strategies based on entity tier
- Include comprehensive error handling
- Add appropriate authorization attributes
- Create both unit and integration tests

Feature to propagate: $ARGUMENTS