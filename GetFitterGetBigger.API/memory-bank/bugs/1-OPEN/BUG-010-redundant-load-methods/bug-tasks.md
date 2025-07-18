# Bug Tasks: Redundant Load Methods

## Investigation Tasks

- [ ] Analyze all services inheriting from EnhancedReferenceService
- [ ] Document all redundant method patterns
- [ ] Identify which methods actually need Entity vs DTO returns
- [ ] Check if any service overrides GetByIdAsync behavior

## Design Tasks

- [ ] Design unified loading architecture
- [ ] Determine if services should work with Entities or DTOs internally
- [ ] Create migration strategy for existing services
- [ ] Consider impact on caching strategy

## Implementation Tasks

- [ ] Refactor base service to eliminate redundancy
- [ ] Update all derived services
- [ ] Update unit tests
- [ ] Update integration tests
- [ ] Performance test to ensure no regression

## Validation Tasks

- [ ] Verify no breaking changes to public API
- [ ] Ensure caching still works correctly
- [ ] Confirm Empty pattern is maintained
- [ ] Check that all tests pass