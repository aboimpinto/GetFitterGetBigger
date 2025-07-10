# GetFitterGetBigger Features Documentation

This directory contains comprehensive documentation for all features in the GetFitterGetBigger ecosystem.

## Documentation Structure

All features follow a standardized structure defined in:
- `FEATURE-TEMPLATE.md` - Template for creating new feature documentation
- `FEATURE-STRUCTURE.md` - Directory structure and organization rules

## Feature Index

### Core Features

#### Exercise Management
- **Status**: COMPLETED
- **Projects**: API, Admin, Clients
- **Description**: Comprehensive exercise creation, management, and categorization system
- **Sub-features**:
  - Exercise CRUD operations
  - Kinetic Chain categorization
  - Exercise Linking (warmup/cooldown relationships)

#### Authentication & Authorization
- **Status**: COMPLETED
- **Projects**: API, Admin, Clients
- **Description**: JWT-based authentication with role-based access control
- **Key Claims**: PT-Tier, Admin-Tier, Free-Tier

#### Reference Tables
- **Status**: COMPLETED
- **Projects**: API, Admin, Clients
- **Description**: System-wide reference data management
- **Tables**: Difficulty levels, muscle groups, equipment, body parts, etc.

### Feature Features (In Progress)

#### Workout Builder
- **Status**: IN_PROGRESS
- **Projects**: API, Admin
- **Description**: Create and manage workout templates and instances

#### User Management
- **Status**: IN_PROGRESS
- **Projects**: API, Admin
- **Description**: Client and trainer profile management

### Planned Features

#### Diet Planning
- **Status**: PLANNING
- **Projects**: API, Admin, Clients
- **Description**: Nutrition tracking and meal planning

#### Progress Tracking
- **Status**: PLANNING
- **Projects**: API, Clients
- **Description**: Client progress monitoring and analytics

## How to Use This Documentation

### For Developers
1. Start with the feature's README.md for overview
2. Check `/api/` subdirectory for endpoint specifications
3. Review `/admin/` or `/clients/` for UI implementation
4. Consult `/tests/` for testing requirements

### For Product Managers
1. Read feature README.md for business purpose and metrics
2. Review workflows in `/admin/workflows.md`
3. Check implementation status across projects

### For New Features
1. Copy `FEATURE-TEMPLATE.md` to start documentation
2. Follow structure defined in `FEATURE-STRUCTURE.md`
3. Add feature to this index when ready

## Documentation Standards

### Version Control
- Each feature maintains its own version
- Use semantic versioning (MAJOR.MINOR.PATCH)
- Document all changes in changelog

### Cross-References
- Use relative paths for internal links
- Always link to feature README first
- Maintain bidirectional references

### Quality Checklist
- [ ] All required sections filled
- [ ] API examples provided
- [ ] Error cases documented
- [ ] Security considerations addressed
- [ ] Test scenarios defined
- [ ] Cross-references updated

## Feature Status Definitions

- **PLANNING**: Requirements being gathered
- **IN_PROGRESS**: Active development
- **COMPLETED**: Fully implemented and tested
- **DEPRECATED**: No longer supported

## Contributing

When adding or updating feature documentation:
1. Follow the FEATURE-TEMPLATE.md structure
2. Ensure all projects (API, Admin, Clients) are covered
3. Update this index file
4. Add appropriate cross-references
5. Include examples and diagrams where helpful

## Quick Links

- [API Documentation Standards](/api-docs/README.md)
- [Admin Project Overview](/GetFitterGetBigger.Admin/README.md)
- [Clients Project Overview](/GetFitterGetBigger.Clients/README.md)
- [Database Schema](/database/README.md)

---

Last Updated: 2025-07-10
Maintained by: Development Team