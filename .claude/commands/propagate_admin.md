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
- Feature should have an `admin/` folder with:
  - `components.md` - UI component specifications
  - `workflows.md` - Personal Trainer workflow descriptions
- If `admin/` folder doesn't exist, run `/refine_feature` first
- API endpoints should already be implemented

## Process Overview
1. Validate feature exists in Features directory
2. Check if `admin/` folder exists with required documentation
3. If no `admin/` folder, prompt user to run `/refine_feature` command
4. Analyze the feature's documentation:
   - Use `_RAW.md` as business context reference
   - Primary source: `admin/` folder contents
   - Extract Admin-specific requirements
5. Follow the FEATURE-TO-ADMIN-PROPAGATION-PROCESS.md guidelines
6. Create comprehensive Admin feature specification in `/GetFitterGetBigger.Admin/memory-bank/features/0-SUBMITTED/`

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
- **bdd-scenarios.md**: BDD test scenarios for UI workflows

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
2. Verify presence of `[FeatureName]_RAW.md` file
3. Check for `admin/` folder with required files:
   - `components.md` (mandatory)
   - `workflows.md` (mandatory)
4. If `admin/` folder is missing or incomplete:
   ```
   ERROR: Feature Admin documentation not found.
   Please run: /refine_feature [FeatureName]
   This will create the necessary admin/ folder structure.
   ```

## Workflow
1. **Input**: Feature name (e.g., "WorkoutTemplate")
2. **Read**: 
   - `/Features/[Category]/[FeatureName]/[FeatureName]_RAW.md` (for context)
   - `/Features/[Category]/[FeatureName]/admin/components.md` (primary source)
   - `/Features/[Category]/[FeatureName]/admin/workflows.md` (primary source)
3. **Create**: New feature in `/GetFitterGetBigger.Admin/memory-bank/features/0-SUBMITTED/`
4. **Next Step**: Run `/refine_feature` on the created Admin feature

## Important Notes
- Always check if the feature already exists in Admin to avoid duplication
- Follow the project's established patterns for services and components
- Ensure proper error handling and loading states
- Update navigation menu if new pages are created
- Reference existing Admin patterns (ReferenceTable structure, etc.)
- Include accessibility requirements (WCAG compliance)
- BDD scenarios should focus on UI workflows

Feature to propagate: $ARGUMENTS