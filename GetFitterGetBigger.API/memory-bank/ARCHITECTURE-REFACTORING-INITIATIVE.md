# Architecture Refactoring Initiative

## Overview
This document tracks the architectural refactoring initiative to enforce proper separation of concerns in the GetFitterGetBigger API. The goal is to ensure that controllers never directly access repositories or UnitOfWork, and instead communicate only through a service layer.

## Architectural Rules (MANDATORY)

### Controller Layer Rules
1. **Controllers MUST NOT directly access repositories** - This is FORBIDDEN
2. **Controllers MUST NOT directly access UnitOfWork (ReadOnly or Writable)** - This is FORBIDDEN
3. **Controllers MUST ONLY communicate with Service layer**

### Service Layer Rules (NEW - Implemented in FEAT-020)
1. **Single Repository Rule**: Each service MUST only access its own repository directly
   - Example: MuscleGroupService can only access IMuscleGroupRepository directly
2. **Service-to-Service Communication**: When a service needs data from another entity, it MUST call the corresponding service
   - Example: If MuscleGroupService needs to validate a BodyPart exists, it must call IBodyPartService.ExistsAsync()
3. **Transactional Pattern**: For operations across multiple repositories:
   - The same WritableUnitOfWork MUST be passed between services
   - Services accepting UnitOfWork parameters MUST use the provided instance
   - Example: AuthService passes its UnitOfWork to ClaimService.CreateUserClaimAsync()
4. **Read-Only Operations**: For read-only cross-service operations:
   - Services should NOT pass UnitOfWork between them
   - Each service creates its own ReadOnlyUnitOfWork as needed

## Current State Analysis (as of 2025-01-07)

### Compliance Status
- **Total Controllers**: 10 (excluding base controller)
- **Compliant**: 2 (20%)
  - ✅ AuthController
  - ✅ ExercisesController
- **Non-Compliant**: 8 (80%)
  - ❌ EquipmentController
  - ❌ MuscleGroupsController
  - ❌ BodyPartsController
  - ❌ DifficultyLevelsController
  - ❌ ExerciseTypesController
  - ❌ KineticChainTypesController
  - ❌ MetricTypesController
  - ❌ MovementPatternsController
  - ❌ MuscleRolesController
  - ❌ ReferenceTablesBaseController (base class)

## Refactoring Features Created

### Phase 1: Critical Controllers
1. **FEAT-015**: Refactor Equipment Controller to Use Service Layer
   - Status: SUBMITTED
   - Priority: High (has CRUD operations)

2. **FEAT-016**: Refactor MuscleGroups Controller to Use Service Layer
   - Status: SUBMITTED
   - Priority: High (complex business logic)

3. **FEAT-017**: Refactor ReferenceTablesBase Controller and Create Generic Service Pattern
   - Status: SUBMITTED
   - Priority: Critical (affects 7 other controllers)

### Phase 2: Reference Table Controllers
These controllers will be refactored as part of FEAT-017:
- BodyPartsController
- DifficultyLevelsController
- ExerciseTypesController
- KineticChainTypesController
- MetricTypesController
- MovementPatternsController
- MuscleRolesController

## Implementation Strategy

### Recommended Order
1. **Start with FEAT-017** - This creates the generic pattern that other controllers will use
2. **Then FEAT-015** - Equipment Controller as a concrete example
3. **Finally FEAT-016** - MuscleGroups Controller with complex logic

### Pattern to Follow
Use the existing compliant controllers as examples:
- **AuthController** → uses IAuthService
- **ExercisesController** → uses IExerciseService

### Service Layer Responsibilities
- Create and manage UnitOfWork instances
- Access repositories
- Implement business logic
- Handle transactions (CommitAsync)
- Manage caching

### Controller Responsibilities
- HTTP request/response handling
- Input validation via attributes
- Authorization checks
- Call service methods
- Map service results to HTTP responses

## Success Metrics
- 100% of controllers compliant with architectural rules
- All tests passing after refactoring
- No direct repository access in any controller
- No UnitOfWork usage in any controller
- Consistent service layer pattern across all controllers

## Long-term Benefits
1. **Maintainability**: Clear separation of concerns
2. **Testability**: Easier to mock services than repositories
3. **Flexibility**: Can change data access strategy without touching controllers
4. **Consistency**: All controllers follow the same pattern
5. **Transaction Management**: Centralized in service layer
6. **Business Logic**: Properly encapsulated in services

## Recent Architecture Improvements (2025-07-18)

### BUG-009: Enhanced Reference Service Architecture Flaw
**Status**: FIXED (Commit: 11fb3734)

#### Problems Addressed
1. **Duplicate LoadEntityByIdAsync Methods**: Services were forced to implement two identical versions (ReadOnly and Writable)
2. **Single UnitOfWork Violation**: Base class mixed read/write operations in single methods
3. **Null Returns**: Methods returned null instead of using Empty pattern
4. **Poor Method Organization**: Large methods violating single responsibility

#### Solutions Implemented
1. **Unified LoadEntityByIdAsync**: Single method that creates its own ReadOnlyUnitOfWork
2. **Proper UnitOfWork Separation**: 
   - `LoadEntityByIdAsync` - Uses ReadOnlyUnitOfWork for queries
   - `LoadEntityByIdForUpdateAsync` - Accepts WritableUnitOfWork for updates only
3. **Empty Pattern**: All methods return Empty objects instead of null
4. **Method Decomposition**: GetByIdAsync refactored into smaller, focused methods:
   - `ValidateAndParseId` - ID validation
   - `LoadEntityAsync` - Orchestration
   - `TryLoadFromCacheAsync` - Cache operations
   - `LoadEntityFromDatabaseAsync` - Database operations
   - `IsEntityValidForReturn` - Business validation
   - `MapAndCacheEntityAsync` - DTO mapping and caching

#### Pattern Improvements
- **Pattern Matching**: Replaced if-else chains with switch expressions
- **ServiceError Support**: ValidationResult now supports structured errors
- **Consistent Error Handling**: Proper error propagation throughout the stack

## Notes
- This refactoring does not change any external API contracts
- All existing endpoints remain the same
- Only the internal architecture is being improved
- This aligns with industry best practices for layered architecture