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
description: Refine a feature following the established template and structure
argument-hint: <feature-name>
---

# Refine Feature Documentation

This command refines a feature in the Features directory to ensure it follows the established template and structure for the GetFitterGetBigger project.

## Purpose
Standardizes feature documentation to maintain consistency across all features, ensuring they are technology-agnostic and properly structured for implementation across API, Admin, and Client projects.

## Prerequisites
- Feature directory must exist in `/Features/`
- Initial feature documentation should be present
- Understanding of the feature's business requirements

## Process Overview
1. Locate the feature in the Features directory
2. Review existing documentation against FEATURE-TEMPLATE.md
3. Apply FEATURE-STRUCTURE.md organizational patterns
4. Ensure technology-agnostic approach
5. Create or update:
   - Core feature description (_RAW.md file)
   - API specifications
   - Admin UI requirements
   - Client implementation details
   - Asset requirements
   - Test scenarios

## Documentation Structure

### Core Components
- **Feature Overview**: Business purpose and value proposition
- **Entity Definitions**: Core data models and relationships
- **Business Rules**: Validation and logic requirements
- **User Workflows**: Step-by-step interaction flows
- **API Contracts**: Endpoint specifications
- **UI/UX Requirements**: Interface guidelines

### Supporting Documentation
- **README.md**: Quick reference and navigation
- **admin/**: Admin-specific implementation details
- **api/**: API endpoint and model documentation
- **clients/**: Mobile/desktop app requirements
- **assets/**: Media and resource specifications
- **tests/**: Test scenarios and acceptance criteria

## Quality Checklist
✓ Technology-agnostic language
✓ Clear business requirements
✓ Complete entity relationships
✓ Defined validation rules
✓ User journey documentation
✓ API endpoint specifications
✓ UI/UX wireframes or descriptions
✓ Test scenarios defined

## Example Usage
```
/refine_feature WorkoutTemplate
/refine_feature MealPlanning
/refine_feature ProgressTracking
```

## Important Guidelines
- Remove any technology-specific implementation details
- Focus on "what" not "how"
- Include all user roles and their interactions
- Define clear acceptance criteria
- Ensure consistency with existing features
- Document edge cases and error scenarios

Feature to refine: $ARGUMENTS