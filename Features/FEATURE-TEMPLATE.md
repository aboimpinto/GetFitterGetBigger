# Feature Documentation Template

This is a standardized template for documenting features in the GetFitterGetBigger ecosystem.
DO NOT MODIFY THIS TEMPLATE. Copy it to create new feature documentation.

---

# [Feature Name]

## Metadata
- **Feature ID**: FEAT-XXX
- **Status**: PLANNING | IN_PROGRESS | COMPLETED | DEPRECATED
- **Created**: YYYY-MM-DD
- **Last Updated**: YYYY-MM-DD
- **Version**: X.Y.Z
- **Owner**: [Team/Person Name]
- **Projects Affected**: API | Admin | Clients

## Overview

### Business Purpose
[2-3 paragraphs explaining the business value and problem this feature solves]

### Target Users
- **Primary**: [User type and their needs]
- **Secondary**: [User type and their needs]

### Success Metrics
- [Metric 1 with target]
- [Metric 2 with target]
- [Metric 3 with target]

## Technical Specification

### Data Model
```json
{
  "EntityName": {
    "id": "string (guid)",
    "property1": "type",
    "property2": "type",
    "relationships": {
      "relatedEntity": "EntityName2"
    }
  }
}
```

### API Endpoints
| Method | Endpoint | Purpose | Auth Required | Claims |
|--------|----------|---------|---------------|--------|
| GET    | /api/... | ...     | Yes/No        | ...    |

### Business Rules
1. [Rule 1]
2. [Rule 2]
3. [Rule 3]

### Validation Rules
- **Field 1**: [Validation requirements]
- **Field 2**: [Validation requirements]

## Implementation Details

### API Project
- **Endpoints Implemented**: [List of endpoints]
- **Data Models**: [List of models/DTOs]
- **Database Changes**: [Schema changes in JSON format]
- **Business Logic**: [Key business rules implemented]

### Admin Project
- **UI Components**: [List of conceptual components]
- **Routes**: [List of admin routes]
- **User Workflows**: [Key workflows enabled]
- **UI Requirements**: [Responsive design, accessibility needs]

### Clients Project
- **Platforms**: Web | Mobile (Android/iOS) | Desktop
- **Implementation Status**:
  - Web: [Status]
  - Android: [Status]
  - iOS: [Status]
  - Desktop: [Status]
- **Platform-Specific Considerations**: [Any platform limitations or optimizations]

## Request/Response Examples

### Example 1: [Operation Name]
**Request**:
```http
POST /api/endpoint
Authorization: Bearer [token]
Content-Type: application/json

{
  "field1": "value1",
  "field2": "value2"
}
```

**Success Response (200 OK)**:
```json
{
  "id": "...",
  "field1": "value1",
  "field2": "value2"
}
```

**Error Response (400 Bad Request)**:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation failed",
  "status": 400,
  "errors": {
    "field1": ["Error message"]
  }
}
```

## Error Handling

### Error Codes
| Code | Meaning | User Message |
|------|---------|--------------|
| 400  | ...     | ...          |
| 401  | ...     | ...          |
| 403  | ...     | ...          |
| 404  | ...     | ...          |

## Security Considerations
- **Authentication**: [Requirements]
- **Authorization**: [Claims and permissions]
- **Data Privacy**: [Sensitive data handling]
- **Audit Trail**: [What is logged]

## Dependencies

### External Dependencies
- [Service/API 1]
- [Service/API 2]

### Internal Dependencies
- [Feature 1]
- [Feature 2]

### Reference Data
- [Reference Table 1]
- [Reference Table 2]

## Migration Plan
1. [Step 1]
2. [Step 2]
3. [Step 3]

## Testing Requirements

### Unit Tests
- [Test scenario 1]
- [Test scenario 2]

### Integration Tests
- [Test scenario 1]
- [Test scenario 2]

### E2E Tests
- [User flow 1]
- [User flow 2]

## Documentation

### User Documentation
- [Link to user guide]
- [Link to FAQ]

### Developer Documentation
- [Link to API docs]
- [Link to architecture docs]

## Future Enhancements
- **Phase 2**: [Enhancement description]
- **Phase 3**: [Enhancement description]

## Related Features
- [Feature Name 1] - [How it relates]
- [Feature Name 2] - [How it relates]

## Changelog
| Version | Date | Changes | Author |
|---------|------|---------|--------|
| 1.0.0   | YYYY-MM-DD | Initial release | ... |

## Notes
[Any additional information not covered above]

### Technology Stack Reference
While this feature documentation remains technology-agnostic, the current implementation uses:
- API: C# Minimal API
- Admin: C# Blazor  
- Clients: C# Avalonia (Android, iOS, Desktop, Web)

Note: Feature documentation should focus on business requirements and use JSON for data models to maintain technology independence.