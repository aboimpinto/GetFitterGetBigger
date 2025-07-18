# BUG-009: Enhanced Reference Service Architecture Flaw - Tasks

## Overview
The EnhancedReferenceService base class has a fundamental design flaw requiring duplicate implementations of LoadEntityByIdAsync and violating the Single UnitOfWork pattern.

## Tasks

### 1. Refactor EnhancedReferenceService Base Class
- [ ] Remove the duplicate LoadEntityByIdAsync abstract methods
- [ ] Implement single LoadEntityByIdAsync without UnitOfWork parameter
- [ ] Update GetByIdAsync to use the new pattern
- [ ] Refactor UpdateAsync to use proper UnitOfWork separation
- [ ] Ensure all methods return Empty instead of null

### 2. Update All Derived Services
- [ ] EquipmentService - Remove duplicate LoadEntityByIdAsync
- [ ] ExerciseTypeService - Remove duplicate LoadEntityByIdAsync
- [ ] DifficultyLevelService - Remove duplicate LoadEntityByIdAsync
- [ ] KineticChainTypeService - Remove duplicate LoadEntityByIdAsync
- [ ] MuscleRoleService - Remove duplicate LoadEntityByIdAsync
- [ ] MovementPatternService - Remove duplicate LoadEntityByIdAsync
- [ ] MetricTypeService - Remove duplicate LoadEntityByIdAsync
- [ ] ExerciseWeightTypeService - Remove duplicate LoadEntityByIdAsync
- [ ] WorkoutObjectiveService - Remove duplicate LoadEntityByIdAsync
- [ ] WorkoutCategoryService - Remove duplicate LoadEntityByIdAsync
- [ ] ExecutionProtocolService - Remove duplicate LoadEntityByIdAsync

### 3. Update PureReferenceService Base Class
- [ ] Apply same architectural fixes as EnhancedReferenceService
- [ ] Ensure consistency across all base service classes

### 4. Testing
- [ ] Run all unit tests
- [ ] Run all integration tests
- [ ] Verify no performance regression

### 5. Documentation
- [ ] Update ARCHITECTURE-REFACTORING-INITIATIVE.md
- [ ] Update service implementation examples
- [ ] Add to LESSONS-LEARNED.md

## Priority
**HIGH** - This affects all reference data services and enforces bad patterns

## Estimated Effort
- Base class refactoring: 2-3 hours
- Update all services: 2-3 hours  
- Testing: 1 hour
- Total: ~6 hours