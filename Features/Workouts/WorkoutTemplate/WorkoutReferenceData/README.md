# Workout Reference Data

## Metadata
- **Feature ID**: FEAT-025
- **Status**: PLANNING
- **Created**: 2025-01-12
- **Last Updated**: 2025-01-12
- **Version**: 1.0.0
- **Owner**: GetFitterGetBigger Team
- **Projects Affected**: API | Admin | Clients

## Overview

### Business Purpose
The Workout Reference Data feature establishes the foundational reference tables and categorization system that enables intuitive workout discovery and organization within the GetFitterGetBigger ecosystem. This feature addresses the critical need for standardized workout classification, objective-based training guidance, and execution protocol definitions that support both casual fitness enthusiasts and professional trainers.

By providing read-only reference data for workout objectives, categories, and execution protocols, this feature creates the metadata infrastructure necessary for effective workout template management, discovery, and execution across all client platforms. The standardized approach ensures consistency in training terminology and methodology while supporting advanced filtering and recommendation systems.

### Target Users
- **Primary**: Personal Trainers creating and organizing workout templates with proper categorization and objective alignment
- **Secondary**: Fitness enthusiasts browsing and filtering workouts based on goals, muscle groups, and training preferences

### Success Metrics
- Reference data adoption rate of 95%+ across all workout templates
- Average workout discovery time reduced by 60% through improved categorization
- User engagement increase of 40% through better workout-goal alignment

## Technical Specification

### Data Model
```json
{
  "WorkoutObjective": {
    "workoutObjectiveId": "string (guid)",
    "value": "string",
    "description": "string",
    "displayOrder": "number",
    "isActive": "boolean"
  },
  "WorkoutCategory": {
    "workoutCategoryId": "string (guid)",
    "value": "string", 
    "description": "string",
    "icon": "string",
    "color": "string",
    "primaryMuscleGroups": "string",
    "displayOrder": "number",
    "isActive": "boolean"
  },
  "ExecutionProtocol": {
    "executionProtocolId": "string (guid)",
    "code": "string",
    "value": "string",
    "description": "string",
    "timeBase": "boolean",
    "repBase": "boolean", 
    "restPattern": "string",
    "intensityLevel": "string",
    "displayOrder": "number",
    "isActive": "boolean"
  },
  "WorkoutMuscles": {
    "workoutTemplateId": "string (guid)",
    "muscleId": "string (guid)",
    "engagementLevel": "string",
    "estimatedLoad": "number"
  }
}
```

### API Endpoints
| Method | Endpoint | Purpose | Auth Required | Claims |
|--------|----------|---------|---------------|--------|
| GET | /api/workout-objectives | Retrieve all workout objectives | Yes | Free-Tier |
| GET | /api/workout-objectives/{id} | Retrieve specific objective | Yes | Free-Tier |
| GET | /api/workout-categories | Retrieve all workout categories | Yes | Free-Tier |
| GET | /api/workout-categories/{id} | Retrieve specific category | Yes | Free-Tier |
| GET | /api/execution-protocols | Retrieve all execution protocols | Yes | Free-Tier |
| GET | /api/execution-protocols/{id} | Retrieve specific protocol | Yes | Free-Tier |

### Business Rules
1. All reference tables are read-only through API endpoints
2. WorkoutObjective values must align with exercise science principles
3. WorkoutCategory icons and colors must be unique across all categories
4. ExecutionProtocol codes must be unique and follow UPPERCASE_UNDERSCORE format
5. DisplayOrder values must be sequential and unique within each table
6. IsActive flag controls visibility but maintains data integrity

### Validation Rules
- **Value**: Required, maximum 100 characters, must be unique within table
- **Description**: Required, maximum 500 characters
- **DisplayOrder**: Required, positive integer, unique within table
- **Code**: Required for ExecutionProtocol, maximum 50 characters, uppercase with underscores
- **Icon**: Required for WorkoutCategory, maximum 50 characters
- **Color**: Required for WorkoutCategory, valid hex color code

## Implementation Details

### API Project
- **Endpoints Implemented**: 6 GET endpoints for read-only access to reference tables
- **Data Models**: WorkoutObjective, WorkoutCategory, ExecutionProtocol, WorkoutMuscles entities
- **Database Changes**: 3 new reference tables following standard ReferenceDataBase pattern
- **Business Logic**: Reference table validation, caching strategies, and hierarchical data retrieval

### Admin Project
- **UI Components**: Reference table listing components integrated into existing ReferenceTable menu
- **Routes**: /reference-tables/workout-objectives, /reference-tables/workout-categories, /reference-tables/execution-protocols
- **User Workflows**: Browse and view reference data for workout template creation context
- **UI Requirements**: Responsive list views with filtering and search capabilities

### Clients Project
- **Platforms**: Web | Mobile (Android/iOS) | Desktop
- **Implementation Status**:
  - Web: Not Started
  - Android: Not Started
  - iOS: Not Started
  - Desktop: Not Started
- **Platform-Specific Considerations**: Mobile apps require optimized caching for offline reference data access

## Request/Response Examples

### Example 1: Get All Workout Objectives
**Request**:
```http
GET /api/workout-objectives
Authorization: Bearer [token]
Accept: application/json
```

**Success Response (200 OK)**:
```json
[
  {
    "workoutObjectiveId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "value": "Muscular Strength",
    "description": "Develops maximum force production capabilities. Typical programming includes 1-5 reps per set, 3-5 sets total, 3-5 minute rest periods between sets, and 85-100% intensity of 1RM.",
    "displayOrder": 1,
    "isActive": true
  },
  {
    "workoutObjectiveId": "7b8c9d12-3456-789a-bcde-f012345678ab",
    "value": "Hypertrophy", 
    "description": "Promotes muscle size increase through controlled volume and moderate intensity. Typical programming includes 6-12 reps per set, 3-4 sets total, 1-3 minute rest periods, and 65-85% intensity of 1RM.",
    "displayOrder": 2,
    "isActive": true
  }
]
```

**Error Response (401 Unauthorized)**:
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authentication required to access reference data"
}
```

### Example 2: Get Execution Protocols
**Request**:
```http
GET /api/execution-protocols
Authorization: Bearer [token]
Accept: application/json
```

**Success Response (200 OK)**:
```json
[
  {
    "executionProtocolId": "9e8d7c6b-5a49-3827-1605-948372615049",
    "code": "STANDARD",
    "value": "Standard",
    "description": "Traditional sets and repetitions with prescribed rest periods. Perform the specified reps, then rest for the prescribed time before the next set.",
    "timeBase": false,
    "repBase": true,
    "restPattern": "Fixed",
    "intensityLevel": "Medium",
    "displayOrder": 1,
    "isActive": true
  }
]
```

## Error Handling

### Error Codes
| Code | Meaning | User Message |
|------|---------|--------------|
| 401 | Unauthorized | Authentication required to access reference data |
| 403 | Forbidden | Insufficient permissions to access reference tables |
| 404 | Not Found | Requested reference item does not exist |
| 500 | Internal Server Error | Unable to retrieve reference data at this time |

## Security Considerations
- **Authentication**: Required for all endpoints using JWT bearer tokens
- **Authorization**: Free-Tier claim minimum for read access to reference data
- **Data Privacy**: No sensitive data in reference tables, publicly shareable fitness information
- **Audit Trail**: Reference data access logging for usage analytics

## Dependencies

### External Dependencies
- None

### Internal Dependencies
- Exercise Management Feature (muscle group relationships)
- Authentication System (JWT token validation)
- Caching Infrastructure (Redis for reference data caching)

### Reference Data
- Muscle Groups (existing reference table)
- Difficulty Levels (existing reference table) 
- Body Parts (existing reference table)

## Migration Plan
1. Create database tables following ReferenceDataBase pattern with proper indexes
2. Implement data seeding with standard fitness industry values for all three tables
3. Deploy API endpoints with comprehensive caching strategy
4. Add Admin UI components to existing ReferenceTable menu structure
5. Validate integration with existing muscle and exercise systems

## Testing Requirements

### Unit Tests
- Reference table entity validation and business rules
- API controller endpoint responses and error handling
- Caching strategy effectiveness and cache invalidation
- Data seeding accuracy and completeness

### Integration Tests
- End-to-end reference data retrieval workflows
- Cross-table relationship validation (WorkoutMuscles with existing Muscle table)
- Authentication and authorization enforcement
- Database performance with reference table queries

### E2E Tests
- Admin user browsing reference tables through UI
- Client application consuming reference data for workout discovery
- Reference data consistency across all platforms

## Documentation

### User Documentation
- Admin guide for understanding reference table organization
- Trainer guide for workout objective and category selection best practices

### Developer Documentation
- API documentation with complete endpoint specifications
- Database schema documentation with relationship diagrams

## Future Enhancements
- **Phase 2**: Advanced filtering combinations and smart recommendations
- **Phase 3**: User-customizable categories and objective definitions
- **Phase 4**: Machine learning-based workout categorization suggestions

## Related Features
- [Exercise Management](../../ExerciseManagement/README.md) - Provides muscle group and exercise data foundation
- [Workout Template Core](../WorkoutTemplateCore/README.md) - Consumes reference data for template creation
- [Exercise Weight Type](../../ReferenceData/ExerciseWeightType/README.md) - Related reference data pattern

## Changelog
| Version | Date | Changes | Author |
|---------|------|---------|--------|
| 1.0.0 | 2025-01-12 | Initial feature specification with three reference tables | GetFitterGetBigger Team |

## Notes
This feature establishes the foundation for sophisticated workout organization and discovery. The reference tables follow established project patterns with standard Value/Description/DisplayOrder structure while adding business-specific fields where needed.

The ExecutionProtocol table includes programmatic codes following the ExerciseWeightType pattern, enabling both human-readable displays and system integration. The WorkoutMuscles relationship table bridges to existing muscle infrastructure while supporting future workout template functionality.

### Technology Stack Reference
While this feature documentation remains technology-agnostic, the current implementation uses:
- API: C# Minimal API
- Admin: C# Blazor  
- Clients: C# Avalonia (Android, iOS, Desktop, Web)

Note: Feature documentation should focus on business requirements and use JSON for data models to maintain technology independence.