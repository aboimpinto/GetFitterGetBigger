# API Documentation Process

## Overview
This document describes the process for documenting API endpoints in the GetFitterGetBigger project.

## Documentation Location
All API documentation should be stored within the feature folder in the memory-bank, NOT in a separate api-docs directory.

## Process

### 1. When to Create API Documentation
Create API documentation when:
- Implementing new endpoints
- Modifying existing endpoints
- Changing request/response formats
- Adding new business rules or validation

### 2. Documentation Structure

For each feature with API endpoints, create the following files in the feature folder:

#### `api-endpoints-documentation.md`
Should contain:
- Overview of the endpoints
- Business rules and constraints
- Each endpoint with:
  - HTTP method and path
  - Path parameters
  - Query parameters
  - Request body schema and examples
  - Response schemas and examples
  - All possible error responses
  - Usage examples
- TypeScript interfaces for all DTOs
- Notes for implementation

#### `propagation-notes.md`
Should contain:
- What files to propagate
- Admin-specific implementation notes
- Client-specific implementation notes
- Common considerations
- Testing recommendations

### 3. Documentation Template

```markdown
# [Feature Name] API Endpoints

## Overview
[Brief description of the endpoints and their purpose]

### Business Rules
- [List all business rules]
- [Include validation constraints]

## Endpoints

### 1. [Endpoint Name]

[Description of what the endpoint does]

**Endpoint:** `[METHOD] /api/[path]`

**Path Parameters:**
- `param` (type): Description

**Query Parameters:**
- `param` (type, optional/required): Description

**Request Body:**
```json
{
  "field": "value"
}
```

**Response:** `[Status Code]`
```json
{
  "field": "value"
}
```

**Error Responses:**
- `400 Bad Request`: [When this happens]
  ```json
  {
    "error": "Error message"
  }
  ```

## Data Models

### [DTO Name]
```typescript
interface DtoName {
  field: type;
}
```

## Usage Examples
[Provide real-world usage examples]

## Notes for Implementation
[Any special considerations]
```

### 4. Propagation Process

The documentation propagator will:
1. Read API documentation from the feature folder
2. Copy relevant files to Admin and Clients memory-banks
3. Frontend developers use this documentation to implement features

### 5. Best Practices

1. **Be Comprehensive**: Include all possible responses, including errors
2. **Use Real Examples**: Provide actual JSON that can be used for testing
3. **Document Constraints**: Clearly state min/max values, required fields, etc.
4. **TypeScript Ready**: Provide interfaces that can be copy-pasted
5. **Consider Both Projects**: Think about how Admin and Clients will use the endpoints differently

### 6. What NOT to Do

- Don't create a separate `/api-docs` directory
- Don't document endpoints outside the feature folder
- Don't forget error responses
- Don't use generic examples - use realistic data

## Example

See `/memory-bank/features/2-IN_PROGRESS/FEAT-022-exercise-linking/` for a complete example of API documentation done correctly.

## Benefits

- All feature documentation is in one place
- Easy to find related docs when working on a feature
- Propagation is simpler
- Version control tracks documentation with implementation