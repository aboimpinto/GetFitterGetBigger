# Workout Template Core - Integration Tests

## Overview
This document specifies integration tests for the Workout Template Core feature, focusing on component interactions and API endpoint testing.

## API Integration Tests

### Template CRUD Operations

#### Test: Complete Template Creation Flow
**Given**: Personal Trainer user
**When**: Creating template through API
**Then**:
1. POST /api/workout-templates returns 201
2. Response includes generated ID
3. GET /api/workout-templates/{id} returns created template
4. Template appears in list endpoint
5. All reference data properly linked

**Test Data**:
```json
{
  "name": "Integration Test Template",
  "workoutCategoryId": "[valid-category-id]",
  "workoutObjectiveId": "[valid-objective-id]",
  "executionProtocolId": "[valid-protocol-id]",
  "estimatedDuration": 45,
  "difficultyLevel": "Intermediate"
}
```

#### Test: Update Template with Exercises
**Given**: Existing template in DRAFT state
**When**: Adding exercises and configurations
**Then**:
1. POST /api/workout-templates/{id}/exercises succeeds
2. Exercise appears in template details
3. Equipment list updates automatically
4. POST /api/workout-templates/{id}/exercises/{exerciseId}/configurations succeeds
5. Complete template structure maintained

#### Test: Delete Template Cascade
**Given**: Template with exercises and configurations
**When**: DELETE /api/workout-templates/{id}
**Then**:
1. Template marked as deleted
2. All exercises removed
3. All configurations removed
4. No orphan records in database
5. 404 on subsequent GET requests

### State Transition Integration

#### Test: DRAFT to PRODUCTION Flow
**Given**: DRAFT template with complete structure
**When**: Transitioning to PRODUCTION
**Then**:
1. PUT /api/workout-templates/{id}/state succeeds
2. Any test logs are deleted
3. Template appears in public listings
4. State change logged in audit trail
5. Notification sent to creator

#### Test: Block Invalid State Transitions
**Given**: PRODUCTION template with execution logs
**When**: Attempting rollback to DRAFT
**Then**:
1. PUT /api/workout-templates/{id}/state returns 409
2. Error details execution log count
3. Template remains in PRODUCTION
4. Audit log shows failed attempt

### Exercise Management Integration

#### Test: Add Exercise with Auto-Associations
**Given**: Exercise with warmup/cooldown associations
**When**: Adding to Main zone
**Then**:
1. Main exercise added successfully
2. GET /api/workout-templates/{id}/exercise-suggestions includes associations
3. Associated exercises flagged in response
4. User prompted to add associations
5. Equipment requirements updated

#### Test: Reorder Exercises Within Zone
**Given**: Multiple exercises in same zone
**When**: Updating sequence orders
**Then**:
1. PUT requests succeed for each exercise
2. No sequence conflicts occur
3. GET template shows correct order
4. Other zones unaffected
5. No data corruption

### Reference Data Integration

#### Test: Reference Data Caching
**Given**: Fresh application start
**When**: Accessing reference endpoints
**Then**:
1. GET /api/reference-tables/workout-states loads from database
2. Subsequent requests served from cache
3. Cache headers set for 365 days
4. ETags properly configured
5. 304 responses for unchanged data

#### Test: Invalid Reference Validation
**Given**: Template with invalid reference IDs
**When**: Attempting to create/update
**Then**:
1. API returns 400 Bad Request
2. Error identifies invalid references
3. No partial data saved
4. Clear error messages returned
5. Valid alternatives suggested

## Cross-Feature Integration Tests

### Exercise Library Integration

#### Test: Exercise Availability Check
**Given**: Template referencing exercises
**When**: Exercise is deactivated in library
**Then**:
1. Existing templates remain valid
2. Exercise cannot be added to new templates
3. Warning shown in template details
4. Suggest alternative exercises
5. Historical data preserved

#### Test: Exercise Update Propagation
**Given**: Exercise used in multiple templates
**When**: Exercise details updated
**Then**:
1. All templates show updated information
2. Equipment changes reflected
3. Category changes handled
4. No template modifications required
5. Cache invalidation occurs

### User Management Integration

#### Test: Creator Permissions Verified
**Given**: Template with specific creator
**When**: Different users attempt access
**Then**:
1. Creator has full permissions
2. Other trainers can view public templates
3. Regular users cannot modify
4. Admin users have override access
5. Audit trail tracks all access

#### Test: Role Change Impact
**Given**: User with Personal Trainer role
**When**: Role revoked
**Then**:
1. Cannot create new templates
2. Can still modify existing owned templates
3. Templates remain accessible
4. Appropriate error messages shown
5. Graceful permission degradation

## Data Consistency Tests

### Concurrent Access Tests

#### Test: Simultaneous Template Updates
**Given**: Multiple users editing same template
**When**: Concurrent save operations
**Then**:
1. Optimistic locking prevents conflicts
2. First save succeeds
3. Second save gets conflict error
4. Version numbers prevent overwrites
5. Users prompted to refresh

#### Test: Race Condition Prevention
**Given**: Rapid sequential API calls
**When**: Adding/removing same exercise
**Then**:
1. Operations processed in order
2. Final state is consistent
3. No duplicate exercises
4. Proper error handling
5. Audit trail accurate

### Transaction Integrity Tests

#### Test: Atomic Template Creation
**Given**: Complex template with many exercises
**When**: Creation fails partway
**Then**:
1. Entire operation rolled back
2. No partial data in database
3. Clear error returned
4. Can retry operation
5. No data corruption

#### Test: Bulk Operation Atomicity
**Given**: Multiple templates selected
**When**: Bulk state change fails for one
**Then**:
1. Successful changes committed
2. Failed changes rolled back
3. Detailed results returned
4. Can retry failed items
5. Consistent final state

## Performance Integration Tests

### Load Testing

#### Test: High Volume Template Queries
**Given**: 10,000+ templates in system
**When**: Querying with complex filters
**Then**:
1. Response time < 1 second
2. Proper pagination works
3. Database indexes utilized
4. No memory leaks
5. Consistent performance

#### Test: Concurrent User Load
**Given**: 100 concurrent users
**When**: Mixed read/write operations
**Then**:
1. All requests handled
2. No deadlocks occur
3. Response times acceptable
4. Database connections managed
5. Graceful degradation

### Caching Integration

#### Test: Multi-Level Cache Coordination
**Given**: Cached data at multiple levels
**When**: Template is modified
**Then**:
1. API cache invalidated
2. CDN cache purged
3. Client caches notified
4. Reference data cache retained
5. Consistent data served

#### Test: Cache Warmup Strategy
**Given**: Cold start scenario
**When**: First requests arrive
**Then**:
1. Critical data loaded first
2. Popular templates cached
3. Reference data cached eternally
4. Lazy loading for rest
5. No timeout errors

## Error Handling Integration

### API Error Propagation

#### Test: Nested Operation Failures
**Given**: Template operation with multiple steps
**When**: Step 3 of 5 fails
**Then**:
1. Previous steps rolled back
2. Specific error returned
3. Recovery suggestions provided
4. State remains consistent
5. Can resume from checkpoint

#### Test: External Service Failures
**Given**: Dependency on exercise service
**When**: Exercise service unavailable
**Then**:
1. Graceful degradation
2. Cached data used if available
3. Clear error to user
4. Retry mechanism activated
5. Circuit breaker pattern

### Validation Chain Tests

#### Test: Multi-Level Validation
**Given**: Complex template submission
**When**: Multiple validation rules apply
**Then**:
1. All validations run
2. All errors collected
3. Single response with all issues
4. Errors properly categorized
5. Field-level error mapping

#### Test: Business Rule Enforcement
**Given**: Valid data but business rule violation
**When**: Attempting operation
**Then**:
1. Technical validation passes
2. Business rule check fails
3. Specific rule violation identified
4. Suggestion for resolution
5. No data corruption

## Security Integration Tests

### Permission Chain

#### Test: Multi-Level Permission Check
**Given**: Nested resource access
**When**: Accessing template exercise configuration
**Then**:
1. User permissions verified
2. Template access checked
3. Exercise access verified
4. Configuration permission validated
5. Audit trail complete

## Monitoring Integration

### Metrics Collection

#### Test: End-to-End Metrics
**Given**: Complete template workflow
**When**: User creates and publishes template
**Then**:
1. API metrics recorded
2. Business metrics updated
3. Performance data collected
4. Error rates tracked
5. User journey analyzed

#### Test: Alert Triggering
**Given**: Monitoring thresholds set
**When**: Error rate exceeds threshold
**Then**:
1. Alert generated
2. Relevant logs aggregated
3. Incident ticket created
4. Notification sent
5. Auto-scaling triggered