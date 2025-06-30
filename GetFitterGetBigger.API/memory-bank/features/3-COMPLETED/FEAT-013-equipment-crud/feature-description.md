# Feature Request: Equipment CRUD Operations

## Feature ID: FEAT-013
## Status: COMPLETED
## Completed: 2025-01-30
## Accepted By: Paulo Aboim Pinto (Product Owner)

## Summary
Extend the existing Equipment reference table to support full Create, Read, Update, and Delete (CRUD) operations. Currently, Equipment only supports read operations, but full CRUD functionality is needed for comprehensive equipment management in the fitness tracking system.

## Business Value
- **Admin users** need to manage equipment dynamically without database migrations
- **Flexibility** to add new equipment types as gyms acquire new machines
- **Data integrity** through proper validation and soft deletes
- **Consistency** with other reference tables like MuscleGroups that already have full CRUD

## Current State
Equipment currently has:
- Read-only controller with GET endpoints
- Simple entity with Id and Name
- Read-only repository methods
- Basic caching for read operations
- Many-to-many relationship with Exercise

## Desired State
Equipment should have:
- Full CRUD endpoints (POST, PUT, DELETE in addition to existing GET)
- Soft delete capability with IsActive flag
- Timestamps for audit trail (CreatedAt, UpdatedAt)
- Proper DTOs for each operation
- Validation for duplicate names
- Usage checking before deactivation
- Cache invalidation on write operations

## Technical Requirements

### 1. Entity Updates
- Add `IsActive` property for soft deletes
- Add `CreatedAt` and `UpdatedAt` timestamps
- Add update factory method

### 2. DTOs
- `EquipmentDto` - Full equipment details
- `CreateEquipmentDto` - Creation request
- `UpdateEquipmentDto` - Update request

### 3. Repository Updates
- Add to `IEquipmentRepository`:
  - `CreateAsync(Equipment equipment)`
  - `UpdateAsync(Equipment equipment)`
  - `DeactivateAsync(EquipmentId id)`
  - `ExistsAsync(string name)`
  - `IsInUseAsync(EquipmentId id)`

### 4. Controller Updates
- Add POST endpoint for creation
- Add PUT endpoint for updates
- Add DELETE endpoint for soft delete
- Implement proper validation
- Add cache invalidation

### 5. Database Migration
- Update Equipment table schema
- Set default values for existing records

## Acceptance Criteria
1. Admin users can create new equipment with unique names
2. Admin users can update equipment names
3. Admin users can deactivate equipment (soft delete)
4. System prevents deactivation of equipment in use by exercises
5. System prevents duplicate equipment names
6. All operations invalidate relevant caches
7. All operations return appropriate HTTP status codes
8. Unit tests cover all new functionality
9. Integration tests verify end-to-end behavior

## Dependencies
- None - Equipment is a standalone reference table

## Risk Assessment
- **Low Risk**: Following established patterns from MuscleGroups
- **Migration Required**: Existing data needs IsActive=true and timestamps

## Implementation Pattern Reference
Follow the implementation pattern from `MuscleGroupsController` which already implements full CRUD for a reference table.

## Priority
Medium - This enhances admin functionality but doesn't block client features

## Estimated Effort
- 2-3 days for full implementation including tests