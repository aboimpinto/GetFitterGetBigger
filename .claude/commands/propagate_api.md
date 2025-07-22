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
Transforms technology-agnostic feature specifications into properly structured API implementation plans, creating a new feature in the API project's memory-bank for development.

## Prerequisites
- Feature must exist in `/Features/` directory
- Feature should have been refined with `/refine_feature` command
- Feature should have an `api/` folder with:
  - `endpoints.md` - API endpoint specifications
  - `models.md` - Data model definitions
  - (Optional) `business-rules.md` - Service logic requirements
- If `api/` folder doesn't exist, run `/refine_feature` first

## Process Overview
1. Validate feature exists in Features directory
2. Check if `api/` folder exists with required documentation
3. If no `api/` folder, prompt user to run `/refine_feature` command
4. Analyze the feature's documentation:
   - Use `_RAW.md` as business context reference
   - Primary source: `api/` folder contents
   - Extract API-specific requirements
5. Follow the FEATURE-TO-API-PROPAGATION-PROCESS.md guidelines
6. Create comprehensive API feature specification in `/GetFitterGetBigger.API/memory-bank/features/0-SUBMITTED/`

## What Gets Created

### In API Memory-Bank (0-SUBMITTED)
A new feature folder containing:
- **feature-description.md**: Complete API feature specification with:
  - Data model requirements
  - API endpoints and contracts
  - Business rules and validation
  - Authorization requirements
  - Caching strategies
- **api-endpoints-spec.md**: Detailed endpoint specifications
- **implementation-plan.md**: Technical implementation details
- **bdd-scenarios.md**: BDD test scenarios (mandatory)

### What Will Be Implemented (after refinement)
- **Controllers**: RESTful endpoints with Swagger documentation
- **Services**: Business logic with ServiceResult pattern
- **Repositories**: Data access with Entity Framework Core
- **DTOs**: Request/Response models with validation
- **Entities**: Database models with proper relationships
- **Migrations**: EF Core database migrations
- **Unit Tests**: Service layer tests with mocking
- **Integration Tests**: Full API endpoint tests
- **Test Builders**: Reusable test data builders

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

## Input Validation Process
1. Check if feature exists in `/Features/[Category]/[FeatureName]/`
2. Verify presence of `[FeatureName]_RAW.md` file
3. Check for `api/` folder with required files:
   - `endpoints.md` (mandatory)
   - `models.md` (mandatory)
   - `business-rules.md` (optional)
4. If `api/` folder is missing or incomplete:
   ```
   ERROR: Feature API documentation not found.
   Please run: /refine_feature [FeatureName]
   This will create the necessary api/ folder structure.
   ```

## Workflow
1. **Input**: Feature name (e.g., "WorkoutTemplate")
2. **Read**: 
   - `/Features/[Category]/[FeatureName]/[FeatureName]_RAW.md` (for context)
   - `/Features/[Category]/[FeatureName]/api/endpoints.md` (primary source)
   - `/Features/[Category]/[FeatureName]/api/models.md` (primary source)
   - `/Features/[Category]/[FeatureName]/api/business-rules.md` (if exists)
3. **Create**: New feature in `/GetFitterGetBigger.API/memory-bank/features/0-SUBMITTED/`
4. **Next Step**: Run `/refine_feature` on the created API feature

## Important Considerations
- Check for existing implementations to avoid duplication
- Follow established naming conventions
- Implement proper caching strategies based on entity tier
- Include comprehensive error handling
- Add appropriate authorization attributes
- BDD test scenarios are mandatory
- Reference ServiceResult pattern for all service methods

Feature to propagate: $ARGUMENTS