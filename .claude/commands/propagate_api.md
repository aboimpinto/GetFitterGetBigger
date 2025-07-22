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
- Feature must have a `README.md` file (created by /refine_feature)
- Feature should have an `api/` folder with:
  - `endpoints.md` - API endpoint specifications
  - `models.md` - Data model definitions
  - (Optional) `business-rules.md` - Service logic requirements
- Feature should have a `tests/` folder with:
  - `unit-tests.md` - Unit test specifications
  - `integration-tests.md` - Integration test specifications
  - `e2e-tests.md` - End-to-end test scenarios (optional)
- If `README.md`, `api/` or `tests/` folders don't exist, run `/refine_feature` first

## Process Overview
1. Validate feature exists in Features directory
2. Check if `api/` folder exists with required documentation
3. Check if `tests/` folder exists with test specifications
4. If no `api/` or `tests/` folder, prompt user to run `/refine_feature` command
5. Analyze the feature's documentation:
   - Use `README.md` as comprehensive feature reference
   - Use `_RAW.md` as additional business context (if exists)
   - Primary source: `api/` folder contents
   - Test specifications: `tests/` folder contents
   - Extract API-specific requirements and acceptance criteria
6. Follow the FEATURE-TO-API-PROPAGATION-PROCESS.md guidelines
7. Create comprehensive API feature specification in `/GetFitterGetBigger.API/memory-bank/features/0-SUBMITTED/`

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
- **bdd-scenarios.md**: BDD test scenarios (mandatory, derived from tests/ folder)
- **unit-test-scenarios.md**: Unit test specifications from source feature
- **integration-test-scenarios.md**: Integration test specifications from source feature

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
2. Verify presence of feature documentation:
   - `README.md` (mandatory - refined feature documentation)
   - `[FeatureName]_RAW.md` (optional - original context)
3. Check for `api/` folder with required files:
   - `endpoints.md` (mandatory)
   - `models.md` (mandatory)
   - `business-rules.md` (optional)
4. Check for `tests/` folder with required files:
   - `unit-tests.md` (mandatory)
   - `integration-tests.md` (mandatory)
   - `e2e-tests.md` (optional but recommended)
5. If `README.md`, `api/` or `tests/` folders are missing or incomplete:
   ```
   ERROR: Feature documentation, API specifications, or test specifications not found.
   Please run: /refine_feature [FeatureName]
   This will create the necessary documentation structure.
   ```

## Workflow
1. **Input**: Feature name (e.g., "WorkoutTemplate")
2. **Read** (in priority order): 
   - `/Features/[Category]/[FeatureName]/README.md` (comprehensive feature specification)
   - `/Features/[Category]/[FeatureName]/api/endpoints.md` (primary API source)
   - `/Features/[Category]/[FeatureName]/api/models.md` (primary API source)
   - `/Features/[Category]/[FeatureName]/api/business-rules.md` (if exists)
   - `/Features/[Category]/[FeatureName]/tests/unit-tests.md` (for acceptance criteria)
   - `/Features/[Category]/[FeatureName]/tests/integration-tests.md` (for API test scenarios)
   - `/Features/[Category]/[FeatureName]/tests/e2e-tests.md` (if exists, for workflow validation)
   - `/Features/[Category]/[FeatureName]/[FeatureName]_RAW.md` (if exists, for additional context)
3. **Extract**: Key information from README.md:
   - Technical specifications and data models
   - Business rules and validation requirements
   - API endpoint details and contracts
   - Error handling specifications
   - Security considerations
   - Dependencies and relationships
4. **Create**: New feature in `/GetFitterGetBigger.API/memory-bank/features/0-SUBMITTED/`
5. **Transform**: Test specifications into BDD scenarios and test plans
6. **Next Step**: Run `/refine_feature` on the created API feature

## Information Extracted from README.md
The README.md provides the complete refined feature specification and is the primary source for:

1. **Metadata and Context**:
   - Feature ID, status, version
   - Business purpose and target users
   - Success metrics

2. **Technical Specifications**:
   - Complete data model in JSON format
   - Entity relationships
   - Field specifications and constraints

3. **API Details** (cross-referenced with api/ folder):
   - Endpoint table with methods, paths, purposes
   - Request/response examples
   - Error handling patterns

4. **Business and Validation Rules**:
   - Comprehensive business logic
   - Field validation requirements
   - State transitions and workflows

5. **Implementation Guidance**:
   - Database schema requirements
   - Caching strategies
   - Security considerations
   - Dependencies and integrations

## Test Transformation Process
When propagating tests from the feature's `tests/` folder:

1. **Unit Tests** → Unit test scenarios focusing on:
   - Service layer business logic
   - Validation rules
   - Edge cases and error handling
   - Mocking of dependencies

2. **Integration Tests** → API integration test scenarios:
   - Full request/response cycle
   - Database interactions
   - Multiple component interactions
   - Error response validation

3. **E2E Tests** → BDD scenarios for user workflows:
   - Complete user journeys
   - Cross-feature interactions
   - Real-world usage patterns

## Important Considerations
- Check for existing implementations to avoid duplication
- Follow established naming conventions
- Implement proper caching strategies based on entity tier
- Include comprehensive error handling
- Add appropriate authorization attributes
- BDD test scenarios are mandatory (derived from tests folder)
- Reference ServiceResult pattern for all service methods
- All test specifications should be technology-agnostic

Feature to propagate: $ARGUMENTS