# Feature: MuscleGroup CRUD Operations

## Overview
Convert the MuscleGroup reference table from read-only to full CRUD operations, allowing Personal Trainers to dynamically manage muscle groups as new exercises and movement patterns are discovered.

## Business Requirements
- Personal Trainers need to add new muscle groups for emerging exercise techniques
- Ability to update muscle group names for clarity or corrections
- Deactivate obsolete muscle groups without breaking existing exercises
- Maintain data integrity with existing exercises that reference muscle groups

## Technical Requirements
- Maintain backward compatibility with existing GET endpoints
- Implement proper authorization (future claim: "ReferenceData-Management")
- Support soft delete to preserve exercise relationships
- Update caching strategy from 1-hour to immediate invalidation on mutations
- Ensure MuscleGroup-BodyPart relationship integrity

## Current State Analysis
- **Entity Type**: Simple entity (not inheriting from ReferenceDataBase)
- **Current Properties**: Id, Name, BodyPartId, BodyPart (navigation)
- **Cache Duration**: 1 hour (dynamic table)
- **Relationships**: 
  - Foreign key to BodyPart
  - Referenced by ExerciseMuscleGroup (many-to-many with Exercise)

## Acceptance Criteria
- [ ] All existing GET endpoints continue to work without changes
- [ ] POST /api/ReferenceTables/MuscleGroups creates new muscle groups
- [ ] PUT /api/ReferenceTables/MuscleGroups/{id} updates existing muscle groups
- [ ] DELETE /api/ReferenceTables/MuscleGroups/{id} soft-deletes (IsActive = false)
- [ ] Duplicate muscle group names are prevented (case-insensitive)
- [ ] BodyPart validation ensures only active body parts can be referenced
- [ ] Cache invalidation triggers on all mutations
- [ ] Deactivated muscle groups remain visible in existing exercises
- [ ] Unit tests achieve 90%+ coverage
- [ ] Integration tests verify all CRUD operations
- [ ] API documentation includes examples for all operations

## Out of Scope
- Bulk operations (will be addressed separately if needed)
- Cascade delete of related exercises
- Historical tracking of changes
- Authorization implementation (claim not yet defined)

## Risk Assessment
- **Low Risk**: Soft delete ensures no data loss
- **Medium Risk**: Cache invalidation must be thoroughly tested
- **Low Risk**: Existing endpoints remain unchanged

## Dependencies
- Existing BodyPart reference table (for foreign key validation)
- Cache infrastructure (already in place)
- Unit of Work pattern (already implemented)

## Success Metrics
- Zero breaking changes to existing API consumers
- Personal Trainers can manage muscle groups without developer intervention
- System performance remains consistent (caching still effective for reads)