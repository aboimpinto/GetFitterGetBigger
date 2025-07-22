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
Ensures that features defined in the technology-agnostic Features directory are properly implemented in the Admin (Blazor) project with appropriate UI components, services, and state management.

## Prerequisites
- Feature must exist in `/Features/` directory
- Feature should have proper _RAW.md documentation
- API endpoints should already be implemented

## Process Overview
1. Validate feature exists in Features directory
2. Read and analyze the feature's _RAW.md file
3. Follow the FEATURE-TO-ADMIN-PROPAGATION-PROCESS.md guidelines
4. Create/update Admin project components:
   - Pages and UI components
   - Service interfaces and implementations
   - State management services
   - DTOs and models
   - Validation logic

## What Gets Created
- **Pages**: Interactive Blazor pages for CRUD operations
- **Components**: Reusable UI components (selectors, badges, etc.)
- **Services**: API client services and state management
- **Models**: DTOs matching API contracts
- **Validation**: Client-side validation rules

## Example Usage
```
/propagate_admin WorkoutTemplate
/propagate_admin ExerciseWeightType
```

## Important Notes
- Always check if the feature already exists in Admin to avoid duplication
- Follow the project's established patterns for services and components
- Ensure proper error handling and loading states
- Update navigation menu if new pages are created

Feature to propagate: $ARGUMENTS