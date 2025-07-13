## Target Project Validation
**CRITICAL**: This process is ONLY for the API project (/GetFitterGetBigger.API/).

Before proceeding, verify:
- [ ] You are propagating to the API project specifically
- [ ] You will create features in `/GetFitterGetBigger.API/memory-bank/features/`
- [ ] You are focusing on backend data models, endpoints, and business logic
- [ ] You are NOT implementing UI components or frontend workflows

# Feature to API Propagation Process

## Overview

This document provides a comprehensive process for propagating features from the main Features folder to the API project. It ensures that business requirements captured in RAW files are properly translated into actionable implementation tasks for the GetFitterGetBigger API backend.

### Purpose

- Transform technology-independent feature specifications into API-specific implementation plans
- Ensure consistency in feature propagation across the ecosystem
- Maintain traceability from business requirements to technical implementation
- Provide clear guidelines for AI assistants and developers
- Focus on backend data model, business logic, and endpoint design

### When to Use This Process

Use this process when:
- A new feature is defined in the Features folder with a RAW file
- The feature requires API backend implementation
- New endpoints, data models, or business logic need to be created
- Existing features need updates in the API project
- UI projects request specific API functionality

### Key Principles

1. **Technology Independence**: RAW files describe WHAT, not HOW
2. **API-First Design**: Define clear contracts and data flows
3. **Database Schema Focus**: Emphasize data modeling and relationships
4. **Business Logic Clarity**: Specify validation rules and business processes
5. **Traceability**: Every propagated feature references its source
6. **Independent Numbering**: Each project maintains its own feature sequence
7. **BDD-First Testing**: All features must include comprehensive BDD scenarios

## Pre-Propagation Checklist

Before starting propagation, verify:

- [ ] RAW file exists in Features folder
- [ ] RAW file follows technology-independent guidelines
- [ ] API folder exists with relevant documentation (if applicable)
- [ ] Business requirements are clear and complete
- [ ] Dependencies are identified (reference data, other features)
- [ ] Feature is ready for backend implementation
- [ ] Data model requirements are understood
- [ ] Authentication/authorization requirements are clear

## Step-by-Step Process

### Step 1: Analyze RAW File

#### 1.1 Locate and Read RAW File
```
Features/
  └── [Category]/
      └── [Feature]/
          └── [Feature]_RAW.md
```

#### 1.2 Extract Key Information
- Business problem being solved
- User stories and scenarios
- **Data entities and relationships** (critical for API)
- **Business rules and validations** (service layer logic)
- **Operations/capabilities needed** (endpoints)
- **Authentication/authorization requirements**
- **Performance and caching considerations**

#### 1.3 Identify API-Specific Requirements
- Which endpoints need to be created (CRUD operations)
- Database schema changes required
- Business logic validation rules
- Data transformation requirements
- Integration with existing API components
- Service dependencies and repository patterns

### Step 2: Verify API Documentation

#### 2.1 Check for API-Specific Files
Look for files in the feature's api folder:
- `[Feature]_api.md` - API-specific requirements
- `endpoints.md` - Endpoint specifications
- `models.md` - Data model definitions
- `business-rules.md` - Service logic requirements

#### 2.2 Validate Documentation Completeness
Ensure documentation covers:
- [ ] Endpoint specifications (request/response formats)
- [ ] Data model definitions
- [ ] Business rule implementations
- [ ] Validation requirements
- [ ] Authentication/authorization rules
- [ ] Error handling scenarios
- [ ] Performance requirements
- [ ] Database schema changes

#### 2.3 Identify Missing Information
If API documentation is incomplete:
1. Note gaps for the implementation team
2. Infer reasonable patterns from similar features
3. Document assumptions clearly
4. Create tasks to clarify ambiguities

### Step 3: Design API Contract

#### 3.1 Define Endpoints
For each operation the feature requires:
```markdown
### [Operation Name]
- **Method**: GET/POST/PUT/DELETE
- **Path**: `/api/[resource]`
- **Authentication**: Bearer token required
- **Authorization**: PT-Tier, Admin-Tier, Free-Tier
- **Purpose**: [What it does]
- **Business Rules**: [Key validation/business logic]
```

#### 3.2 Specify Request/Response Formats
Include clear examples:
```json
// Request (POST /api/exercises)
{
  "name": "Push-up",
  "difficultyLevelId": "difficulty-001",
  "exerciseTypeIds": ["type-001", "type-002"],
  "muscleGroupIds": ["muscle-001"],
  "equipmentIds": ["equipment-001"]
}

// Response (201 Created)
{
  "id": "exercise-123...",
  "name": "Push-up",
  "isActive": true,
  "createdAt": "2025-01-15T10:30:00Z",
  "difficultyLevel": {
    "id": "difficulty-001",
    "name": "Beginner"
  }
}

// Error Response (400 Bad Request)
{
  "errors": {
    "name": ["The name field is required."],
    "difficultyLevelId": ["Invalid difficulty level."]
  }
}
```

#### 3.3 Note Implementation Requirements
- **Repository patterns**: ReadOnlyUnitOfWork vs WritableUnitOfWork usage
- **Service layer logic**: Business rule implementations
- **Validation patterns**: Data annotation vs custom validators
- **Caching strategies**: Which data should be cached
- **Migration requirements**: Database schema changes

### Step 4: Prepare API Feature

#### 4.1 Get Next Feature ID
```bash
cd /GetFitterGetBigger.API/memory-bank/features/
cat NEXT_FEATURE_ID.txt  # e.g., "025"
```

#### 4.2 Create Feature Structure
```bash
mkdir -p 0-SUBMITTED/FEAT-025-[feature-name]/
cd 0-SUBMITTED/FEAT-025-[feature-name]/
```

#### 4.3 Write feature-description.md
Use the template below:
```markdown
# Feature: [Feature Name]

## Feature ID: FEAT-XXX
## Created: [YYYY-MM-DD]
## Status: SUBMITTED
## Source: Features/[Category]/[Feature]/
## Target PI: [PI-YYYY-QX]

## Summary
[2-3 sentence overview of what needs to be implemented in API]

## Business Context
[Reference the RAW file and explain the business need]
This feature addresses the need for [business requirement] as defined in 
the source RAW file at Features/[Category]/[Feature]/[Feature]_RAW.md.

## Data Model Requirements

### New Entities
- **[EntityName]**: [Purpose and key fields]
  - Key relationships: [Related entities]
  - Business rules: [Validation requirements]

### Entity Relationships
```
[EntityA] ←→ [EntityB] (one-to-many)
[EntityB] ←→ [EntityC] (many-to-many)
```

### Database Schema Changes
- [ ] New tables: [List tables]
- [ ] Modified tables: [List changes]
- [ ] New indexes: [Performance requirements]
- [ ] Migration scripts: [Data migration needs]

## API Endpoints

### [Resource] Management
```
GET    /api/[resources]           # List with pagination
POST   /api/[resources]           # Create new
GET    /api/[resources]/{id}      # Get by ID
PUT    /api/[resources]/{id}      # Update existing
DELETE /api/[resources]/{id}      # Soft delete
```

### Business Operations
```
POST   /api/[resources]/{id}/[action]  # Business operation
GET    /api/[resources]/[query]        # Special queries
```

## Business Rules & Validation

### Core Business Rules
1. **[Rule Name]**: [Description and implementation requirement]
2. **[Validation Rule]**: [Field validation and error handling]
3. **[Authorization Rule]**: [Who can access what]

### Validation Requirements
| Field | Type | Required | Validation Rules | Error Message |
|-------|------|----------|------------------|---------------|
| name | string | Yes | Max 100 chars, unique | "Name is required and must be unique" |
| description | string | No | Max 500 chars | "Description too long" |
| isActive | boolean | Yes | Default: true | N/A |

## Service Layer Requirements

### [Name]Service Interface
```csharp
public interface I[Name]Service
{
    Task<ServiceResult<[Name]Dto>> CreateAsync(Create[Name]Request request);
    Task<ServiceResult<[Name]Dto>> GetByIdAsync([Name]Id id);
    Task<ServiceResult<PagedResponse<[Name]Dto>>> GetAllAsync(PaginationParams pagination);
    Task<ServiceResult<[Name]Dto>> UpdateAsync(Update[Name]Request request);
    Task<ServiceResult> DeleteAsync([Name]Id id);
}
```

### Repository Requirements
- **ReadOnlyUnitOfWork**: For all query operations and validation
- **WritableUnitOfWork**: Only for Create, Update, Delete operations
- **Includes**: Specify navigation properties to load
- **Caching**: Which operations should be cached

## Authentication & Authorization

### Required Claims
- **Create/Update/Delete**: PT-Tier or Admin-Tier
- **Read Operations**: Free-Tier, PT-Tier, Admin-Tier
- **Special Operations**: [Specify claim requirements]

### Authorization Rules
- Users can only access their own data (where applicable)
- PT-Tier users can manage client-related data
- Admin-Tier users have full access

## BDD Test Scenarios (MANDATORY)

### Scenario 1: Create [Resource] - Happy Path
```gherkin
Given I am authenticated as "PT-Tier"
And I have valid [resource] data
When I send a POST request to "/api/[resources]"
Then the response status should be 201
And the response should contain the created [resource]
And the [resource] should be persisted in the database
```

### Scenario 2: Create [Resource] - Validation Error
```gherkin
Given I am authenticated as "PT-Tier"
And I have invalid [resource] data with missing required field
When I send a POST request to "/api/[resources]"
Then the response status should be 400
And the response should contain validation errors
```

### Scenario 3: Get [Resource] - Success
```gherkin
Given I am authenticated as "PT-Tier"
And a [resource] with id "{id}" exists
When I send a GET request to "/api/[resources]/{id}"
Then the response status should be 200
And the response should contain the [resource] details
```

### Scenario 4: Update [Resource] - Success
```gherkin
Given I am authenticated as "PT-Tier"
And a [resource] with id "{id}" exists
When I send a PUT request to "/api/[resources]/{id}"
Then the response status should be 200
And the [resource] should be updated in the database
```

### Scenario 5: Delete [Resource] - Success
```gherkin
Given I am authenticated as "PT-Tier"
And a [resource] with id "{id}" exists
When I send a DELETE request to "/api/[resources]/{id}"
Then the response status should be 204
And the [resource] should be deactivated in the database
```

### Scenario 6: Authorization Failure
```gherkin
Given I am authenticated as "Free-Tier"
When I send a POST request to "/api/[resources]"
Then the response status should be 403
And the response should contain authorization error
```

### Edge Cases & Error Scenarios
- [ ] Duplicate creation attempts
- [ ] Concurrent modification conflicts
- [ ] Invalid foreign key references
- [ ] Large payload handling
- [ ] Rate limiting scenarios

## Performance Considerations

### Caching Strategy
- **Cache Keys**: [Specify cache key patterns]
- **Cache Duration**: [Specify TTL for different data types]
- **Cache Invalidation**: [When to invalidate cache]

### Query Optimization
- **Indexes**: Required database indexes
- **Pagination**: Default page sizes and limits
- **Filtering**: Supported filter parameters
- **Sorting**: Available sort options

## Technical Dependencies

### Internal Dependencies
- **Entities**: [List required entity models]
- **Services**: [List service dependencies]
- **Repositories**: [List repository dependencies]
- **Reference Data**: [List reference tables needed]

### External Dependencies
- **Database**: [Schema changes required]
- **Authentication**: [JWT configuration]
- **Caching**: [Memory cache configuration]

## Migration Strategy

### Database Migrations
- [ ] Create entity tables
- [ ] Add foreign key constraints
- [ ] Create indexes for performance
- [ ] Seed reference data
- [ ] Data migration scripts (if needed)

### Backward Compatibility
- [ ] No breaking changes to existing endpoints
- [ ] Maintain existing response formats
- [ ] Handle legacy data gracefully

## Acceptance Criteria

- [ ] All API endpoints respond correctly
- [ ] Database schema matches requirements
- [ ] Business rules are enforced
- [ ] Validation works as specified
- [ ] Authorization rules are implemented
- [ ] BDD scenarios pass completely
- [ ] Performance meets requirements
- [ ] Documentation is complete

## Dependencies

- **API Features**: [List any API feature dependencies]
- **Reference Data**: [List reference tables needed]
- **Other Systems**: [List external system dependencies]

## Implementation Notes

### Repository Pattern Requirements
- Use `ReadOnlyUnitOfWork` for all validation and query operations
- Use `WritableUnitOfWork` ONLY for actual data modifications
- Implement proper Include() statements for navigation properties
- Follow existing repository patterns for consistency

### Service Layer Guidelines
- Implement business logic in service layer, not controllers
- Use ServiceResult pattern for error handling
- Validate all inputs before processing
- Check authorization at service level
- Implement proper caching where beneficial

### Controller Implementation
- Keep controllers thin - delegate to services
- Use proper HTTP status codes
- Implement consistent error response format
- Add proper OpenAPI documentation attributes
- Follow existing endpoint patterns

### Testing Requirements
- Unit tests: Mock ALL dependencies, test business logic
- Integration tests: Use BDD format with real database
- Test all BDD scenarios defined above
- Achieve high test coverage (>80%)
```

#### 4.4 Update Project Files
1. Increment `NEXT_FEATURE_ID.txt`
2. Update `feature-status.md` (if exists)

### Step 5: Document Implementation Plan

#### 5.1 Create Detailed Task Breakdown
Create implementation plan following API Feature Implementation Process:
```markdown
# FEAT-XXX: [Feature Name] Implementation Plan

## Overview
Implementation plan for [feature description].

## Pre-Implementation Requirements
- [ ] Review `/memory-bank/systemPatterns.md` - Architecture rules
- [ ] Review `/memory-bank/unitOfWorkPattern.md` - ReadOnly vs Writable patterns
- [ ] Review `/memory-bank/common-implementation-pitfalls.md` - Common mistakes
- [ ] Review `/memory-bank/UNIT-VS-INTEGRATION-TESTS.md` - Test separation rules

## Implementation Categories

### 1. Database & Entity Model (Est: 2-3h)
- [ ] Create entity models with proper relationships
- [ ] Add entity configurations for EF Core
- [ ] Create database migration
- [ ] Add seed data if required
- [ ] Write entity unit tests

### 2. Repository Layer (Est: 1-2h)
- [ ] Create repository interface
- [ ] Implement repository with ReadOnlyUnitOfWork/WritableUnitOfWork
- [ ] Add specialized query methods
- [ ] Write repository unit tests (all dependencies mocked)

### 3. Service Layer (Est: 3-4h)
- [ ] Create service interface
- [ ] Implement business logic with proper validation
- [ ] Add caching where appropriate
- [ ] Handle authorization rules
- [ ] Write service unit tests (all dependencies mocked)

### 4. Controller Layer (Est: 2-3h)
- [ ] Create controller with CRUD endpoints
- [ ] Add request/response DTOs
- [ ] Implement proper error handling
- [ ] Add OpenAPI documentation
- [ ] Write controller unit tests (service mocked)

### 5. BDD Integration Tests (Est: 2-4h)
- [ ] Create BDD feature file with all scenarios
- [ ] Implement step definitions for happy paths
- [ ] Implement step definitions for error cases
- [ ] Add edge case scenarios
- [ ] Ensure all scenarios pass

### 6. Documentation & Finalization (Est: 1h)
- [ ] Update API documentation
- [ ] Create usage examples
- [ ] Document configuration changes
- [ ] Update deployment notes
```

#### 5.2 Specify Testing Strategy
- **Unit Tests**: All components tested in isolation with mocked dependencies
- **Integration Tests**: BDD scenarios testing complete workflows with real database
- **Test Data**: Seed data and test fixtures approach
- **Coverage Goals**: Minimum 80% code coverage

### Step 6: Update Tracking

#### 6.1 Update feature-propagation-log.md
Add entry to `/api-docs/feature-propagation-log.md`:
```markdown
#### X. [Feature Name]
- **Source**: Features/[Category]/[Feature]/
- **API Feature ID**: FEAT-XXX
- **Created**: [Date]
- **Status**: Propagated to 0-SUBMITTED
- **Summary**: [Brief description]
- **Endpoints**: [List of endpoints to be created]
- **Dependencies**: [List any dependencies]
```

#### 6.2 Commit Changes
```bash
git add .
git commit -m "feat: propagate [feature] to API project

- Created FEAT-XXX in API 0-SUBMITTED
- Documented endpoints and data model requirements
- Specified BDD test scenarios
- Added implementation plan with time estimates"
```

## Quality Assurance Checklist

Before considering propagation complete:

### Documentation Quality
- [ ] Feature description is complete and clear
- [ ] Data model requirements are specific
- [ ] API endpoints are well-documented
- [ ] BDD scenarios cover all business rules
- [ ] Dependencies are identified
- [ ] Acceptance criteria are measurable

### Technical Completeness
- [ ] All endpoints documented with request/response examples
- [ ] Database schema changes specified
- [ ] Business rules clearly stated
- [ ] Validation requirements detailed
- [ ] Authentication/authorization requirements noted
- [ ] Performance considerations addressed

### API-Specific Requirements
- [ ] Repository pattern usage specified (ReadOnly vs Writable)
- [ ] Service layer responsibilities defined
- [ ] Caching strategy documented
- [ ] Migration requirements identified
- [ ] BDD test scenarios are comprehensive

### Process Compliance
- [ ] Used correct feature numbering
- [ ] Created in 0-SUBMITTED state
- [ ] Referenced source RAW file
- [ ] Updated tracking documents
- [ ] Followed naming conventions

## Common Mistakes to Avoid

1. **❌ Copying UI-Specific Details from RAW**
   - RAW files should be technology-independent
   - Don't propagate frontend implementation specifics

2. **❌ Incomplete Data Model Specifications**
   - Always define entity relationships clearly
   - Include validation rules and business constraints
   - Specify migration requirements

3. **❌ Missing BDD Scenarios**
   - Every business rule must have a BDD scenario
   - Include both happy path and error scenarios
   - Don't forget authorization test cases

4. **❌ Incorrect UnitOfWork Usage**
   - ReadOnlyUnitOfWork for validation and queries
   - WritableUnitOfWork ONLY for data modifications
   - Don't mix patterns within the same method

5. **❌ Forgetting Performance Considerations**
   - Specify caching requirements
   - Note pagination needs
   - Identify potential bottlenecks

6. **❌ Skipping Authorization Design**
   - Always specify claim requirements
   - Document permission boundaries
   - Include authorization test scenarios

## Templates

### API Endpoint Documentation Template
```markdown
## [Resource] Endpoints

### Create [Resource]
```http
POST /api/[resources]
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "string",
  "field": "value"
}
```

**Business Rules:**
- Field X must be unique
- User must have PT-Tier or Admin-Tier claims
- [Additional rules]

**Response (201 Created):**
```json
{
  "id": "guid",
  "name": "string",
  "field": "value",
  "createdAt": "datetime",
  "isActive": true
}
```

**Error Responses:**
- `400`: Validation errors
- `401`: Unauthorized
- `403`: Insufficient permissions
- `409`: Duplicate resource

### Get [Resource]
```http
GET /api/[resources]/{id}
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "id": "guid",
  "name": "string",
  "field": "value",
  "relatedEntities": []
}
```

### Update [Resource]
```http
PUT /api/[resources]/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "field": "updated value"
}
```

### Delete [Resource]
```http
DELETE /api/[resources]/{id}
Authorization: Bearer {token}
```

**Response (204 No Content)**
```

### BDD Scenario Template
```gherkin
Feature: [Resource] Management
  As a [user type]
  I want to [action]
  So that [benefit]

Background:
  Given the application is running
  And the database is clean

Scenario: Create [resource] with valid data
  Given I am authenticated as "PT-Tier"
  And I have valid [resource] data
    | field1 | field2 |
    | value1 | value2 |
  When I send a POST request to "/api/[resources]"
  Then the response status should be 201
  And the response should contain the created [resource]
  And the [resource] should be persisted in the database

Scenario: Create [resource] with invalid data
  Given I am authenticated as "PT-Tier"
  And I have invalid [resource] data with missing required field
  When I send a POST request to "/api/[resources]"
  Then the response status should be 400
  And the response should contain validation errors
  And no [resource] should be created in the database

Scenario: Unauthorized access attempt
  Given I am not authenticated
  When I send a GET request to "/api/[resources]"
  Then the response status should be 401
  And the response should contain authentication error
```

### Service Interface Template
```csharp
public interface I[Name]Service
{
    /// <summary>
    /// Creates a new [resource] with validation
    /// </summary>
    Task<ServiceResult<[Name]Dto>> CreateAsync(Create[Name]Request request);
    
    /// <summary>
    /// Gets [resource] by ID with related data
    /// </summary>
    Task<ServiceResult<[Name]Dto>> GetByIdAsync([Name]Id id);
    
    /// <summary>
    /// Gets paginated list of [resources] with filtering
    /// </summary>
    Task<ServiceResult<PagedResponse<[Name]Dto>>> GetAllAsync(
        [Name]FilterParams filters, 
        PaginationParams pagination);
    
    /// <summary>
    /// Updates existing [resource] with validation
    /// </summary>
    Task<ServiceResult<[Name]Dto>> UpdateAsync(Update[Name]Request request);
    
    /// <summary>
    /// Soft deletes [resource] (sets IsActive = false)
    /// </summary>
    Task<ServiceResult> DeleteAsync([Name]Id id);
}
```

## Example: ExerciseWeightType Propagation

### Source Analysis
```
Features/ReferenceData/ExerciseWeightType/ExerciseWeightType_reference_RAW.md
```
- Defines 5 weight types for exercises
- Specifies validation rules for exercise weight fields
- Describes business logic for weight type behavior

### API Requirements Extracted
1. **Data Model**
   - ExerciseWeightType reference table
   - Relationship to Exercise entity
   - Validation rules for weight types

2. **Endpoints**
   - GET /api/exerciseweighttypes (reference data)
   - Integration with exercise CRUD operations

3. **Business Logic**
   - Weight field visibility based on type
   - Validation rules per weight type
   - Migration strategy for existing data

### Resulting API Feature
Created: `FEAT-023-exercise-weight-type/`
- Comprehensive data model specifications
- Endpoint definitions with examples
- BDD scenarios for all business rules
- Migration strategy documentation

## Post-Propagation

After completing propagation:

1. **Notify API Team**
   - Feature is available in 0-SUBMITTED
   - Highlight any open questions
   - Point to related features or dependencies

2. **Monitor Progress**
   - Check when moved to 1-READY_TO_DEVELOP
   - Answer questions during planning
   - Update documentation as needed

3. **Maintain Alignment**
   - Keep documentation synchronized
   - Update if business requirements change
   - Track implementation status

## Integration with API Workflow

This propagation process integrates with the API's existing workflow:

1. **0-SUBMITTED State**: Features start here after propagation
2. **Planning Phase**: Move to 1-READY_TO_DEVELOP with detailed task breakdown
3. **Implementation**: Follow FEATURE_IMPLEMENTATION_PROCESS.md
4. **Testing**: Mandatory BDD scenarios defined during propagation
5. **Completion**: Move to 3-COMPLETED with all reports

The propagation process ensures that when features reach the API team, they have:
- Clear business requirements
- Defined data models
- Specified endpoints
- Comprehensive BDD scenarios
- Implementation guidance
- Performance considerations

---

Remember: The goal is to provide the API team with everything they need to implement the feature without requiring access to the original RAW files or extensive business analysis.