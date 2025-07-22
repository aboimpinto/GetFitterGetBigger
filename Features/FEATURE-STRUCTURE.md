# Feature Documentation Structure

This document defines the immutable structure for organizing feature documentation in the GetFitterGetBigger project.

## Directory Structure

```
/Features/
├── FEATURE-TEMPLATE.md          # DO NOT MODIFY - Template for all features
├── FEATURE-STRUCTURE.md         # DO NOT MODIFY - This file
├── README.md                    # Feature index and overview
│
├── [FeatureName]/               # One directory per feature
│   ├── README.md                # Feature overview using template
│   ├── [FeatureName]_RAW.md    # Optional: Unstructured documentation to be refined
│   ├── api/                     # API-related documentation
│   │   ├── endpoints.md         # Endpoint specifications
│   │   ├── models.md            # Data models and DTOs
│   │   └── migrations/          # Database migration scripts
│   ├── admin/                   # Admin project documentation
│   │   ├── components.md        # UI components
│   │   ├── workflows.md         # User workflows
│   │   └── mockups/             # UI mockups and designs
│   ├── clients/                 # Client applications documentation
│   │   ├── web.md               # Web-specific implementation
│   │   ├── mobile.md            # Mobile-specific implementation
│   │   └── desktop.md           # Desktop-specific implementation
│   ├── tests/                   # Test documentation
│   │   ├── unit-tests.md        # Unit test specifications
│   │   ├── integration-tests.md # Integration test specifications
│   │   └── e2e-tests.md         # End-to-end test scenarios
│   └── assets/                  # Images, diagrams, etc.
│       ├── diagrams/
│       └── screenshots/
```

## File Naming Conventions

### Required Files (Must exist for each feature)
- `README.md` - Main feature documentation using FEATURE-TEMPLATE.md
- `api/endpoints.md` - API endpoint specifications
- `api/models.md` - Data models and structures

### Optional Files (Create as needed)
- `admin/components.md` - Admin UI components
- `admin/workflows.md` - Admin user workflows
- `clients/[platform].md` - Platform-specific implementations
- `tests/[type]-tests.md` - Test specifications
- `CHANGELOG.md` - Detailed version history (if README changelog insufficient)

## Content Standards

### Technology-Agnostic Approach
- **MUST NOT** include technology-specific code examples
- **MUST** use JSON serialization for all data models
- **MUST** focus on business logic and requirements, not implementation
- **MAY** reference current technology stack in Notes section only
- **SHOULD** ensure documentation remains valid regardless of technology changes

### 1. Feature README.md
- MUST follow FEATURE-TEMPLATE.md structure exactly
- MUST include all sections from template
- MAY add additional sections at the end under "Notes"

### 2. API Documentation (api/)
- MUST include all endpoints with full REST specifications
- MUST include request/response examples using JSON format
- MUST document all error responses
- MUST specify authorization requirements
- MUST NOT include language-specific code snippets

### 3. Admin Documentation (admin/)
- MUST include component hierarchy (conceptual, not framework-specific)
- MUST document user workflows with steps
- SHOULD include mockups or screenshots
- MUST specify responsive design requirements
- MUST NOT reference specific UI framework components

### 4. Client Documentation (clients/)
- MUST be platform-specific for behavior only
- MUST note platform limitations
- MUST include implementation status
- SHOULD include platform-specific optimizations
- MUST NOT include platform-specific code examples

### 5. Test Documentation (tests/)
- MUST include test scenarios
- MUST specify test data requirements
- SHOULD include performance benchmarks
- MUST document edge cases
- MUST use language-agnostic test descriptions

## Cross-Feature References

### Linking Between Features
- Use relative paths: `../FeatureName/README.md`
- Always link to README.md first, then specific sections
- Use anchors for specific sections: `../FeatureName/README.md#technical-specification`

### Dependency Documentation
- List all dependencies in the main README.md
- Create a `dependencies.md` file for complex dependencies
- Include both upstream and downstream dependencies

## Version Control

### Feature Versioning
- Use semantic versioning: MAJOR.MINOR.PATCH
- MAJOR: Breaking changes
- MINOR: New functionality (backward compatible)
- PATCH: Bug fixes (backward compatible)

### Change Documentation
- Update version in README.md metadata
- Add entry to Changelog section
- For major changes, create separate CHANGELOG.md

## Migration Guidelines

### From Existing Documentation
1. Copy FEATURE-TEMPLATE.md to create new feature directory
2. Extract relevant content from existing docs
3. Organize content according to template sections
4. Add cross-references to related features
5. Archive or deprecate old documentation

### From RAW Documentation Files
1. **Identify the RAW file location** (e.g., `/Features/Category/SubCategory/FeatureName/FeatureName_RAW.md`)
2. **Create refined structure in the SAME directory** as the RAW file
3. **Do NOT create a new parallel directory**
4. Extract and organize content from RAW file into proper structure
5. Preserve the RAW file or rename with `_ARCHIVE` suffix
6. Ensure all RAW content is incorporated into refined documentation

### Creating New Features
1. Create feature directory: `/Features/[FeatureName]/`
2. Copy FEATURE-TEMPLATE.md to `README.md`
3. Fill in all required sections
4. Create subdirectories as needed
5. Add feature to `/Features/README.md` index

## Refining RAW Documentation

### Purpose
RAW documentation files (`*_RAW.md`) contain unstructured feature information that needs to be refined into the standard feature structure. When refining these files, the refined structure must be created in the **same directory** as the RAW file.

### Refinement Process
1. **Locate the RAW file** (e.g., `WorkoutTemplateCore_RAW.md`)
2. **Create refined structure in the same directory**
3. **Preserve the RAW file** for reference and history
4. **Do NOT create a new parallel directory**

### Example: Correct Refinement
```
/Features/Workouts/WorkoutTemplate/WorkoutTemplateCore/
├── WorkoutTemplateCore_RAW.md     # Original RAW documentation (preserved)
├── README.md                       # Refined feature documentation
├── api/                           # API documentation (created during refinement)
│   ├── endpoints.md
│   └── models.md
├── admin/                         # Admin documentation (created during refinement)
│   ├── components.md
│   └── workflows.md
└── tests/                         # Test documentation (created during refinement)
```

### Example: Incorrect Refinement (DO NOT DO THIS)
```
/Features/Workouts/WorkoutTemplate/
├── WorkoutTemplateCore/
│   └── WorkoutTemplateCore_RAW.md  # RAW file here
└── WorkoutTemplateCore/            # WRONG: Creates parallel directory
    ├── README.md
    └── api/
```

### RAW File Guidelines
- RAW files should be preserved after refinement
- RAW files can be archived by adding `_ARCHIVE` suffix once refinement is complete
- Multiple RAW files in a directory should be consolidated during refinement
- RAW file content should be fully incorporated into the refined structure

## Maintenance Rules

### DO NOT
- Modify FEATURE-TEMPLATE.md
- Modify FEATURE-STRUCTURE.md
- Change established directory structure
- Remove required sections from feature README
- Create parallel directories when refining RAW files

### DO
- Add new optional sections to feature documentation
- Create feature-specific subdirectories as needed
- Update version and changelog regularly
- Keep cross-references up to date
- Refine RAW files in their current directory

## Example Features

### Well-Structured Features (Reference These)
- `/Features/ExerciseManagement/` - Complex feature with multiple components
- `/Features/Authentication/` - Security-focused feature
- `/Features/WorkoutBuilder/` - UI-heavy feature

### Feature Categories
1. **Core Features** - Essential system functionality
2. **Enhancement Features** - Improvements to existing features
3. **Integration Features** - Third-party integrations
4. **Utility Features** - Helper functionality

---

This structure is designed to be:
- **Consistent** - Same structure for all features
- **Scalable** - Handles simple to complex features
- **Maintainable** - Clear organization and standards
- **Discoverable** - Easy to find and navigate

## Current Technology Stack

This section documents the current technology choices for reference. Note that feature documentation should remain technology-agnostic.

### Current Implementation (as of 2025-07-10)
- **API**: C# Minimal API
- **Admin**: C# Blazor
- **Clients**: C# Avalonia (compiles to Android, iOS, Desktop, and Web)

### Technology Independence Principle
Feature documentation focuses on business requirements and data models using JSON serialization. This approach ensures that features remain valid regardless of technology changes. Implementation details are kept in project-specific repositories, not in feature documentation.

Last Updated: 2025-07-22
Version: 1.2.0