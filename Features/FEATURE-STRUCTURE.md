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

### Creating New Features
1. Create feature directory: `/Features/[FeatureName]/`
2. Copy FEATURE-TEMPLATE.md to `README.md`
3. Fill in all required sections
4. Create subdirectories as needed
5. Add feature to `/Features/README.md` index

## Maintenance Rules

### DO NOT
- Modify FEATURE-TEMPLATE.md
- Modify FEATURE-STRUCTURE.md
- Change established directory structure
- Remove required sections from feature README

### DO
- Add new optional sections to feature documentation
- Create feature-specific subdirectories as needed
- Update version and changelog regularly
- Keep cross-references up to date

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

Last Updated: 2025-07-10
Version: 1.1.0