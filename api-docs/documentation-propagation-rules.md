# Documentation Propagation Rules

This document defines the rules and responsibilities for propagating API documentation to client projects in the GetFitterGetBigger ecosystem.

## Purpose

When API endpoints are created or modified, the relevant information must be propagated to client projects so they can implement the necessary connections and functionality. This ensures all projects have the information needed without requiring access to the API project's memory-bank or internal documentation.

## Project Structure

The ecosystem consists of:
- **API Project**: The backend service providing all endpoints
- **Admin Project**: Web application for Personal Trainers to manage exercises, workouts, and clients
- **Clients Project**: Contains mobile (Android, iOS), web, and desktop applications for end users

## Propagation Targets

When propagating API documentation, update only these two memory-banks:
1. `/GetFitterGetBigger.Admin/memory-bank/`
2. `/GetFitterGetBigger.Clients/memory-bank/`

**Important**: Do NOT update individual sub-project memory-banks. The Clients memory-bank serves all client platforms (Android, iOS, Web, Desktop).

## Documentation Content Requirements

When propagating API information, include:

### 1. Endpoint Overview
- Purpose and functionality description
- Which user roles can access the endpoint
- Business context and use cases

### 2. Technical Specifications
- Complete endpoint URLs
- HTTP methods
- Authentication requirements
- Authorization requirements (specific claims needed)
- Request/response content types

### 3. Request Details
- Path parameters with types and constraints
- Query parameters with defaults and limits
- Request body structure with validation rules
- Required vs optional fields
- Field constraints (max length, format, etc.)

### 4. Response Details
- Success response status codes
- Response body structure with examples
- Pagination format if applicable
- Error response formats and common status codes

### 5. Implementation Guidelines
- Authentication flow specifics
- Error handling recommendations
- Performance considerations
- Platform-specific notes where relevant

### 6. Dependencies
- Reference data requirements (e.g., reference tables needed)
- Related endpoints that must be implemented together
- Order of implementation if dependencies exist

## File Naming and Structure Convention

When creating propagation documents in memory-banks:

### For API Project (uses FEAT-XXX format):
1. Check `NEXT_FEATURE_ID.txt` for the next number
2. Create folder: `memory-bank/features/0-SUBMITTED/FEAT-XXX-{feature-name}/`
3. Create `feature-description.md` inside the folder
4. Update `NEXT_FEATURE_ID.txt`

### For Admin Project (also uses FEAT-XXX format):
1. Check `NEXT_FEATURE_ID.txt` for the next number
2. Create folder: `memory-bank/features/0-SUBMITTED/FEAT-XXX-{feature-name}/`
3. Create `feature-description.md` (API integration details)
4. Create `{feature-name}.md` (implementation plan with FEAT-XXX header)
5. Update `NEXT_FEATURE_ID.txt` to next number

### For Clients Project (no numbering system):
1. Create folder: `memory-bank/features/0-SUBMITTED/{feature-name}/`
2. Create `feature-description.md` (API integration details)
3. Create `{feature-name}.md` (implementation plan)
4. Use descriptive folder names like:
   - `equipment-reference-data`
   - `musclegroup-reference-data`
   - `rest-exercise-optional-musclegroups`

### Important Notes:
- API documentation should be in `feature-description.md` (API integration details)
- Feature implementation plan should be in `{feature-name}.md` (no FEAT prefix in filename)
- Both files go in the same feature folder within `memory-bank/features/0-SUBMITTED/`
- Folder names: Admin uses `FEAT-XXX-{name}`, Clients uses `{name}` only
- ALL features MUST start in `0-SUBMITTED` state
- No files should be placed in a root-level `0-SUBMITTED` folder

## Propagation Process

1. **Analyze**: Read the API implementation details from API memory-bank
2. **Summarize**: Create comprehensive summary in `/api-docs/`
3. **Propagate**: Create integration documents in both Admin and Clients memory-banks
   - Place features in `0-SUBMITTED` folder for project teams to refine
   - Focus on API contract details, not implementation tasks
4. **Verify**: Ensure all necessary information is included for independent implementation

## Information Isolation

Client projects should be able to implement features using ONLY:
- Their own memory-bank documentation
- The propagated integration documents
- NO access to API memory-bank or internal API documentation needed

## Update Triggers

Propagation should occur when:
- New API endpoints are created
- Existing endpoints have breaking changes
- Authentication/authorization requirements change
- New business rules affect client implementation
- Response formats are modified

## Quality Checklist

Before completing propagation, verify:
- [ ] All endpoints are documented with full URLs
- [ ] Authentication/authorization is clearly specified
- [ ] Request/response examples are provided
- [ ] Validation rules are documented
- [ ] Error scenarios are covered
- [ ] Platform-specific considerations are noted
- [ ] Dependencies on reference data are listed
- [ ] Implementation order is clear if dependencies exist