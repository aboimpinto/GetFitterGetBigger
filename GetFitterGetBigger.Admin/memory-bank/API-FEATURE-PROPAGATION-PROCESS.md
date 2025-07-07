# API Feature Propagation Process

This document outlines the process for propagating API features (especially endpoint changes or new endpoints) to the Admin and Client projects.

## Overview

When API features modify or add endpoints, these changes need to be propagated to frontend projects (Admin and Clients) to ensure they can consume the new functionality. This is a structured process that maintains consistency across the ecosystem.

## Process Steps

### 1. Identify API Changes Requiring Propagation

API features that typically require propagation include:
- New endpoints added
- Existing endpoint modifications (request/response changes)
- New fields added to DTOs
- Validation rule changes
- New reference data types

### 2. Create Feature in Frontend Projects

When creating a propagated feature in Admin or Client projects:

#### Step 1: Navigate to the Project
```bash
cd /path/to/GetFitterGetBigger.Admin  # or .Clients
```

#### Step 2: Create Feature in 0-SUBMITTED State
All propagated features start in the `0-SUBMITTED` state:

```bash
# Create feature directory with project-specific ID
mkdir -p memory-bank/features/0-SUBMITTED/FEAT-XXX-feature-name
```

**IMPORTANT**: 
- Use the project's own feature numbering (check `NEXT_FEATURE_ID.txt`)
- Do NOT reuse the API's feature ID
- Each project maintains independent feature numbering

#### Step 3: Create Feature Description
Create `feature-description.md` in the feature directory:

```markdown
# Feature: [Feature Name]

## Feature ID: FEAT-XXX
## Created: [Date]
## Status: SUBMITTED

## Summary
[Brief description of what needs to be implemented]

## Background
The API has been updated (API's FEAT-YYY) to [describe changes]. 
[Explain why these changes were made]

## Requirements
[List specific implementation requirements for this project]

## API Changes Reference
- Endpoint: [GET/POST/PUT/DELETE] /api/[endpoint]
- New fields: [list any new fields]
- Modified fields: [list any modified fields]
- Validation changes: [list any validation changes]

## Dependencies
- API's FEAT-YYY must be completed and deployed
- [Any other dependencies]
```

#### Step 4: Update Project Files

1. **Update feature-status.md**:
   Add the new feature to the status dashboard

2. **Increment NEXT_FEATURE_ID.txt**:
   Update to the next available number

### 3. Feature Refinement Process

The propagated feature will remain in `0-SUBMITTED` until the team is ready to implement it. At that point:

1. Review the API changes in detail
2. Create implementation tasks specific to the project
3. Add `feature-tasks.md` with UI/UX specific tasks
4. Move to `1-READY_TO_DEVELOP` when planning is complete

### 4. Update API Documentation

After propagating a feature, update the central API documentation:

1. **Update feature-propagation-log.md** in `/api-docs/`:
   - Record which projects received the feature
   - Note the feature IDs in each project
   - Track propagation status

2. **Update the specific API documentation** if needed:
   - Add notes about frontend implementation requirements
   - Document any special considerations for UI/UX

## Example: FEAT-019 Kinetic Chain Propagation

### API Project (Origin)
- **Feature ID**: FEAT-019
- **Changes**: Added `kineticChain` field to Exercise endpoints
- **Status**: IN_PROGRESS

### Admin Project (Propagated)
- **Feature ID**: FEAT-017 (Admin's numbering)
- **Status**: SUBMITTED
- **Location**: `memory-bank/features/0-SUBMITTED/FEAT-017-exercise-kinetic-chain/`

### Clients Project (To be propagated)
- **Feature ID**: TBD (will use Clients' NEXT_FEATURE_ID)
- **Status**: Not yet created

## Best Practices

1. **Independent Numbering**: Each project maintains its own feature numbering sequence
2. **Reference Original**: Always reference the API's feature ID in the description
3. **Complete Information**: Include all endpoint changes and field modifications
4. **No Auto-Implementation**: Features stay in SUBMITTED until explicitly planned
5. **Track Everything**: Update propagation logs to maintain visibility

## Common Pitfalls to Avoid

1. ❌ **Don't reuse API feature IDs** - Each project has its own sequence
2. ❌ **Don't create in READY_TO_DEVELOP** - Always start in SUBMITTED
3. ❌ **Don't skip documentation** - Always create feature-description.md
4. ❌ **Don't forget to update NEXT_FEATURE_ID.txt** - Keep the counter accurate
5. ❌ **Don't implement immediately** - Wait for team planning and prioritization

## Tracking Propagation Status

To check propagation status across projects:
1. Check `/api-docs/feature-propagation-log.md` for the central record
2. Search for the API feature ID reference in each project's memory-bank
3. Use the feature-status.md dashboards in each project

## When NOT to Propagate

Some API changes don't require frontend propagation:
- Internal API refactoring with no contract changes
- Performance optimizations
- Bug fixes that don't change behavior
- Documentation-only changes

Only propagate when frontend code needs to change to accommodate the API updates.