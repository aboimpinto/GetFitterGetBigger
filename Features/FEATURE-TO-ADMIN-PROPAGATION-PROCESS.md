## Target Project Validation
**CRITICAL**: This process is ONLY for the Admin project (/GetFitterGetBigger.Admin/).

Before proceeding, verify:
- [ ] You are propagating to the Admin project specifically
- [ ] You will create features in `/GetFitterGetBigger.Admin/memory-bank/features/`
- [ ] You are focusing on UI components, user workflows, and frontend implementation
- [ ] You are NOT implementing API endpoints or database schemas

If propagating to API or Clients projects, stop and use the appropriate process document.
 

# Feature to Admin Propagation Process

## Overview

This document provides a comprehensive process for propagating features from the main Features folder to the Admin project. It ensures that business requirements captured in RAW files are properly translated into actionable implementation tasks for the Admin web application.

### Purpose

- Transform technology-independent feature specifications into Admin-specific implementation plans
- Ensure consistency in feature propagation across the ecosystem
- Maintain traceability from business requirements to technical implementation
- Provide clear guidelines for AI assistants and developers

### When to Use This Process

Use this process when:
- A new feature is defined in the Features folder with a RAW file
- The feature requires Admin interface implementation
- API changes need to be integrated into the Admin application
- Existing features need updates in the Admin project

### Key Principles

1. **Technology Independence**: RAW files describe WHAT, not HOW
2. **Clear Contracts**: API specifications must be complete and unambiguous
3. **UI/UX Focus**: Admin documentation emphasizes user interface and workflows
4. **Traceability**: Every propagated feature references its source
5. **Independent Numbering**: Each project maintains its own feature sequence

## Pre-Propagation Checklist

Before starting propagation, verify:

- [ ] RAW file exists in Features folder
- [ ] RAW file follows technology-independent guidelines
- [ ] Admin folder exists with relevant documentation (if applicable)
- [ ] Business requirements are clear and complete
- [ ] Dependencies are identified (API features, reference data)
- [ ] Feature is ready for frontend implementation

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
- Entities and relationships
- Business rules and validations
- Operations/capabilities needed

#### 1.3 Identify Admin-Specific Requirements
- Which users will use this feature (PT-Tier, Admin-Tier)
- UI components needed
- Workflows and user journeys
- Data entry/management needs

### Step 2: Verify Admin Documentation

#### 2.1 Check for Admin-Specific Files
Look for files in the feature's admin folder:
- `[Feature]_admin.md` - UI/UX requirements
- `components.md` - Component specifications
- `workflows.md` - User workflows
- `mockups/` - Visual designs

#### 2.2 Validate Documentation Completeness
Ensure documentation covers:
- [ ] User interface descriptions
- [ ] Component hierarchy
- [ ] User workflows
- [ ] Form validations
- [ ] Error handling
- [ ] Success states
- [ ] Mobile responsiveness

#### 2.3 Identify Missing Information
If admin documentation is incomplete:
1. Note gaps for the implementation team
2. Infer reasonable UI patterns from similar features
3. Document assumptions clearly

### Step 3: Create API Summary (if applicable)

#### 3.1 Document Endpoints
For each endpoint the Admin will consume:
```markdown
### [Operation Name]
- **Method**: GET/POST/PUT/DELETE
- **Path**: /api/[resource]
- **Authentication**: Required/Optional
- **Authorization**: PT-Tier, Admin-Tier
- **Purpose**: [What it does]
```

#### 3.2 Specify Request/Response Formats
Include clear examples:
```json
// Request
{
  "field1": "value",
  "field2": 123
}

// Response
{
  "id": "resource-123...",
  "field1": "value",
  "field2": 123
}
```

#### 3.3 Note Integration Requirements
- Caching strategies
- Error handling patterns
- Pagination requirements
- Real-time updates needed

### Step 4: Prepare Admin Feature

#### 4.1 Get Next Feature ID
```bash
cd /GetFitterGetBigger.Admin/memory-bank/features/
cat NEXT_FEATURE_ID.txt  # e.g., "018"
```

#### 4.2 Create Feature Structure
```bash
mkdir -p 0-SUBMITTED/FEAT-018-[feature-name]/
cd 0-SUBMITTED/FEAT-018-[feature-name]/
```

#### 4.3 Write feature-description.md
Use the template below:
```markdown
# Feature: [Feature Name]

## Feature ID: FEAT-XXX
## Created: [YYYY-MM-DD]
## Status: SUBMITTED
## Source: Features/[Category]/[Feature]/

## Summary
[2-3 sentence overview of what needs to be implemented in Admin]

## Business Context
[Reference the RAW file and explain the business need]
This feature addresses the need for [business requirement] as defined in 
the source RAW file at Features/[Category]/[Feature]/[Feature]_RAW.md.

## User Stories
As a [Personal Trainer/Admin]:
- I want to [action] so that [benefit]
- I want to [action] so that [benefit]

## UI/UX Requirements

### Page/Component Structure
[Describe the main pages or components needed]

### User Workflows
1. [Primary workflow description]
2. [Secondary workflow description]

### Forms and Validations
- [Field]: [Validation rules]
- [Field]: [Validation rules]

### Visual Design Notes
[Any specific design requirements or patterns to follow]

## Technical Requirements

### API Integration
[List endpoints to consume with brief descriptions]

### State Management
[Describe any complex state management needs]

### Component Dependencies
[List any shared components or libraries needed]

## Acceptance Criteria
- [ ] User can [action]
- [ ] System validates [rule]
- [ ] UI displays [information]
- [ ] Error states show [behavior]
- [ ] Success states show [behavior]

## Dependencies
- API Feature: [FEAT-XXX if applicable]
- Reference Data: [List any reference tables needed]
- Other Features: [List related features]

## Implementation Notes
[Any specific technical considerations for the Admin team]
```

#### 4.4 Create Implementation Plan
Create `[feature-name].md`:
```markdown
# FEAT-XXX: [Feature Name] Implementation Plan

## Overview
Implementation plan for [feature description].

## Tasks

### 1. API Integration Layer
- [ ] Create API service for [endpoints]
- [ ] Add TypeScript interfaces
- [ ] Implement error handling

### 2. Component Development
- [ ] Create [Component1] component
- [ ] Create [Component2] component
- [ ] Add form validations

### 3. State Management
- [ ] Set up Redux/Context for [feature]
- [ ] Create actions and reducers
- [ ] Connect components to state

### 4. UI Implementation
- [ ] Implement responsive design
- [ ] Add loading states
- [ ] Create error boundaries

### 5. Testing
- [ ] Unit tests for components
- [ ] Integration tests for API calls
- [ ] E2E tests for workflows

## Technical Decisions
[Document any technical choices made during planning]

## Open Questions
[List any questions for the team to resolve]
```

#### 4.5 Update Project Files
1. Increment `NEXT_FEATURE_ID.txt`
2. Update `feature-status.md` (if exists)

### Step 5: Document Integration Points

#### 5.1 API Endpoints Documentation
Create clear documentation of all endpoints:
- Full URL paths
- Request/response examples
- Error scenarios
- Rate limiting considerations

#### 5.2 Reference Data Dependencies
List all reference tables needed:
- ExerciseTypes
- DifficultyLevels
- Equipment
- [Any new reference data]

#### 5.3 State Management Needs
Document complex state requirements:
- Form state with multi-step workflows
- Real-time data updates
- Cached data management
- User preferences

#### 5.4 Component Requirements
Identify reusable components:
- Existing components to reuse
- New shared components needed
- Third-party libraries required

### Step 6: Update Tracking

#### 6.1 Update feature-propagation-log.md
Add entry to `/api-docs/feature-propagation-log.md`:
```markdown
#### X. [Feature Name]
- **Source**: Features/[Category]/[Feature]/
- **Admin Feature ID**: FEAT-XXX
- **Created**: [Date]
- **Status**: Propagated to 0-SUBMITTED
- **Summary**: [Brief description]
```

#### 6.2 Commit Changes
```bash
git add .
git commit -m "feat: propagate [feature] to Admin project

- Created FEAT-XXX in Admin 0-SUBMITTED
- Documented UI/UX requirements
- Specified API integration needs
- Added implementation plan"
```

## Quality Assurance Checklist

Before considering propagation complete:

### Documentation Quality
- [ ] Feature description is complete and clear
- [ ] UI/UX requirements are specific
- [ ] API integration is well-documented
- [ ] Dependencies are identified
- [ ] Acceptance criteria are measurable

### Technical Completeness
- [ ] All endpoints documented with examples
- [ ] Request/response formats specified
- [ ] Validation rules clearly stated
- [ ] Error scenarios covered
- [ ] Authentication requirements noted

### Process Compliance
- [ ] Used correct feature numbering
- [ ] Created in 0-SUBMITTED state
- [ ] Referenced source RAW file
- [ ] Updated tracking documents
- [ ] Followed naming conventions

## Common Mistakes to Avoid

1. **❌ Copying Technical Details from RAW**
   - RAW files should be technology-independent
   - Don't propagate implementation specifics

2. **❌ Incomplete UI Specifications**
   - Always describe user workflows
   - Include form validations
   - Specify error and success states

3. **❌ Missing API Examples**
   - Provide complete request/response examples
   - Include error response formats
   - Document all status codes

4. **❌ Forgetting Dependencies**
   - List all reference data needed
   - Note related features
   - Identify API prerequisites

5. **❌ Skipping Tracking Updates**
   - Always update propagation log
   - Increment feature counters
   - Maintain traceability

## Example: ExerciseWeightType Propagation

### Source Analysis
```
Features/ReferenceData/ExerciseWeightType/ExerciseWeightType_reference_RAW.md
```
- Defines 5 weight types for exercises
- Specifies validation rules
- Describes UI behavior guidelines

### Admin Requirements Extracted
1. **Exercise Form Enhancement**
   - Add weight type dropdown
   - Dynamic weight field visibility
   - Validation based on type

2. **Reference Data Management**
   - Read-only view of weight types
   - Integration with exercise CRUD

3. **User Workflows**
   - Creating exercises with weight types
   - Editing existing exercises
   - Bulk updates for migration

### Resulting Admin Feature
Created: `FEAT-025-exercise-weight-type/`
- Comprehensive UI specifications
- Form behavior documentation
- API integration details
- Migration workflow

## Templates

### Admin Feature Description Template
[Included in Step 4.3 above]

### API Integration Documentation Template
```markdown
## API Integration

### Endpoint: [Name]
- **Method**: [HTTP Method]
- **Path**: `/api/[path]`
- **Purpose**: [What it does]
- **Authentication**: Bearer token required
- **Authorization**: [Claims needed]

#### Request
```json
{
  // Request body example
}
```

#### Response (Success - 200)
```json
{
  // Response body example
}
```

#### Response (Error - 400)
```json
{
  "error": "Validation failed",
  "details": ["Field X is required"]
}
```

#### Integration Notes
- [Caching strategy]
- [Error handling approach]
- [Retry logic if applicable]
```

### Validation Rules Template
```markdown
## Validation Rules

### [Form/Entity Name]

| Field | Type | Required | Validation Rules |
|-------|------|----------|------------------|
| name | string | Yes | Max 100 chars, unique |
| description | string | No | Max 500 chars |
| weight | number | Conditional | > 0 when required |
| isActive | boolean | Yes | Default: true |

### Business Rules
1. [Rule description and implementation]
2. [Rule description and implementation]

### Error Messages
- `FIELD_REQUIRED`: "[Field] is required"
- `FIELD_INVALID`: "[Field] must be [criteria]"
- `BUSINESS_RULE_VIOLATION`: "[Specific message]"
```

## Post-Propagation

After completing propagation:

1. **Notify Admin Team**
   - Feature is available in 0-SUBMITTED
   - Highlight any open questions
   - Point to related API features

2. **Monitor Progress**
   - Check when moved to READY_TO_DEVELOP
   - Answer questions during implementation
   - Update documentation as needed

3. **Maintain Alignment**
   - Keep documentation synchronized
   - Update if requirements change
   - Track implementation status

## Appendix: File Structure Reference

### Features Folder Structure
```
Features/
├── [Category]/
│   └── [Feature]/
│       ├── [Feature]_RAW.md          # Business requirements
│       ├── admin/                    # Admin-specific docs
│       │   ├── components.md
│       │   ├── workflows.md
│       │   └── mockups/
│       ├── api/                      # API specifications
│       │   ├── endpoints.md
│       │   └── models.md
│       └── assets/                   # Supporting materials
```

### Admin Memory Bank Structure
```
GetFitterGetBigger.Admin/memory-bank/
├── features/
│   ├── 0-SUBMITTED/                  # New features start here
│   │   └── FEAT-XXX-[name]/
│   │       ├── feature-description.md
│   │       └── [name].md
│   ├── 1-READY_TO_DEVELOP/
│   ├── 2-IN_PROGRESS/
│   ├── 3-COMPLETED/
│   └── NEXT_FEATURE_ID.txt
```

---

Remember: The goal is to provide the Admin team with everything they need to implement the feature without requiring access to the original RAW files or API documentation.