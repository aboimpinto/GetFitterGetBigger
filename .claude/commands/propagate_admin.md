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
description: Propagate a feature from the Features directory to the Admin project
argument-hint: <feature-name>
---

# Propagate Feature to Admin Project

This command propagates a feature from the central Features directory to the GetFitterGetBigger.Admin project following the established propagation process.

## Purpose
Transforms technology-agnostic feature specifications into properly structured Admin implementation plans, creating a new feature in the Admin project's memory-bank for development.

## Prerequisites
- Feature must exist in `/Features/` directory
- Feature should have been refined with `/refine_feature` command
- Feature must have a `README.md` file (created by /refine_feature)
- Feature should have an `admin/` folder with:
  - `components.md` - UI component specifications
  - `workflows.md` - Personal Trainer workflow descriptions
- Feature should have a `tests/` folder with:
  - `unit-tests.md` - Unit test specifications (for validation logic)
  - `integration-tests.md` - Integration test specifications
  - `e2e-tests.md` - End-to-end test scenarios (highly relevant for UI)
- If `README.md`, `admin/` or `tests/` folders don't exist, run `/refine_feature` first
- API endpoints should already be implemented

## Process Overview
1. Validate feature exists in Features directory
2. Check if `admin/` folder exists with required documentation
3. Check if `tests/` folder exists with test specifications
4. If no `admin/` or `tests/` folder, prompt user to run `/refine_feature` command
5. Analyze the feature's documentation:
   - Use `README.md` as comprehensive feature reference
   - Use `_RAW.md` as additional business context (if exists)
   - Primary source: `admin/` folder contents
   - Test specifications: `tests/` folder contents (especially e2e-tests.md)
   - Extract Admin-specific requirements and UI acceptance criteria
6. Follow the FEATURE-TO-ADMIN-PROPAGATION-PROCESS.md guidelines
7. Create comprehensive Admin feature specification in `/GetFitterGetBigger.Admin/memory-bank/features/0-SUBMITTED/`

## What Gets Created

### In Admin Memory-Bank (0-SUBMITTED)
A new feature folder containing:
- **feature-description.md**: Complete Admin feature specification with:
  - UI component requirements
  - Personal Trainer workflows
  - State management needs
  - Integration with existing Admin patterns
  - Accessibility requirements
- **implementation-plan.md**: Technical implementation details
- **ui-mockups.md**: Visual specifications (if applicable)
- **bdd-scenarios.md**: BDD test scenarios for UI workflows (derived from e2e-tests.md)
- **ui-test-scenarios.md**: Component and interaction test scenarios
- **validation-test-scenarios.md**: Client-side validation tests (from unit-tests.md)

### What Will Be Implemented (after refinement)
- **Pages**: Interactive Blazor pages for CRUD operations
- **Components**: Reusable UI components (selectors, badges, etc.)
- **Services**: API client services and state management
- **Models**: DTOs matching API contracts
- **Validation**: Client-side validation rules
- **Tests**: Component and integration tests

## Example Usage
```
/propagate_admin WorkoutTemplate
/propagate_admin ExerciseWeightType
```

## Input Validation Process
1. Check if feature exists in `/Features/[Category]/[FeatureName]/`
2. Verify presence of feature documentation:
   - `README.md` (mandatory - refined feature documentation)
   - `[FeatureName]_RAW.md` (optional - original context)
3. Check for `admin/` folder with required files:
   - `components.md` (mandatory)
   - `workflows.md` (mandatory)
4. Check for `tests/` folder with required files:
   - `unit-tests.md` (for validation logic)
   - `integration-tests.md` (for workflow tests)
   - `e2e-tests.md` (critical for UI testing)
5. If `README.md`, `admin/` or `tests/` folders are missing or incomplete:
   ```
   ERROR: Feature documentation, Admin specifications, or test specifications not found.
   Please run: /refine_feature [FeatureName]
   This will create the necessary documentation structure.
   ```

## Workflow
1. **Input**: Feature name (e.g., "WorkoutTemplate")
2. **Read** (in priority order): 
   - `/Features/[Category]/[FeatureName]/README.md` (comprehensive feature specification)
   - `/Features/[Category]/[FeatureName]/admin/components.md` (primary Admin source)
   - `/Features/[Category]/[FeatureName]/admin/workflows.md` (primary Admin source)
   - `/Features/[Category]/[FeatureName]/tests/e2e-tests.md` (for UI acceptance criteria)
   - `/Features/[Category]/[FeatureName]/tests/integration-tests.md` (for workflow validation)
   - `/Features/[Category]/[FeatureName]/tests/unit-tests.md` (for validation rules)
   - `/Features/[Category]/[FeatureName]/[FeatureName]_RAW.md` (if exists, for additional context)
3. **Extract**: Key information from README.md:
   - Data models and entity relationships
   - Business rules and workflows
   - User roles and permissions
   - API contracts (for service integration)
   - Success metrics and UI requirements
   - Error handling patterns
4. **Create**: New feature in `/GetFitterGetBigger.Admin/memory-bank/features/0-SUBMITTED/`
5. **Transform**: Test specifications into UI-focused test scenarios
6. **Next Step**: Run `/refine_feature` on the created Admin feature

## Information Extracted from README.md
The README.md provides the complete refined feature specification and is used to extract Admin-relevant information:

1. **Business Context**:
   - Target users (especially Personal Trainers)
   - Business purpose and workflows
   - Success metrics for UI design

2. **Data Models for UI**:
   - Entity structures for form design
   - Field types and constraints for input controls
   - Relationships for navigation and data display

3. **Validation Requirements**:
   - Field-level validation rules for client-side implementation
   - Business rule validation for UI feedback
   - Error messages and user guidance

4. **API Integration Details**:
   - Endpoint specifications for service calls
   - Request/response formats for DTOs
   - Error handling for user notifications

5. **UI/UX Guidance**:
   - Workflow descriptions informing page flow
   - State management requirements
   - Performance considerations for responsive UI

## Test Transformation Process
When propagating tests from the feature's `tests/` folder to Admin:

1. **E2E Tests** → Primary source for BDD scenarios:
   - Personal Trainer workflows
   - Complete UI interactions
   - Multi-step processes
   - Error handling flows

2. **Integration Tests** → UI integration scenarios:
   - Component interactions
   - API service integration
   - State management tests
   - Navigation flows

3. **Unit Tests** → Validation and component logic:
   - Client-side validation rules
   - Component behavior tests
   - Form validation
   - Business logic in UI services

## Important Notes
- Always check if the feature already exists in Admin to avoid duplication
- Follow the project's established patterns for services and components
- Ensure proper error handling and loading states
- Update navigation menu if new pages are created
- Reference existing Admin patterns (ReferenceTable structure, etc.)
- Include accessibility requirements (WCAG compliance)
- BDD scenarios should focus on UI workflows (extracted from e2e tests)
- All test specifications should be transformed to focus on UI/UX aspects

Feature to propagate: $ARGUMENTS