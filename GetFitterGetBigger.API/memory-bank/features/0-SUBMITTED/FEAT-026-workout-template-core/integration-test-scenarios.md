# Workout Template Core - Integration Test Scenarios

## API Integration Tests

### Template CRUD Operations

#### Test: Complete Template Creation Flow
- **Given**: Personal Trainer user authenticated
- **When**: Creating template through API with valid data
- **Then**: 
  - POST /api/workout-templates returns 201 Created
  - Response includes generated ID and Location header
  - GET /api/workout-templates/{id} returns created template
  - Template appears in list endpoint with correct data
  - All reference data properly linked and populated

#### Test: Update Template with Exercises
- **Given**: Existing template in DRAFT state
- **When**: Adding exercises and configurations through API
- **Then**:
  - POST /api/workout-templates/{id}/exercises returns 201
  - Exercise appears in template details with full data
  - Equipment list updates automatically
  - POST /api/workout-templates/{id}/exercises/{exerciseId}/configurations succeeds
  - Complete template structure maintained in database

#### Test: Delete Template Cascade
- **Given**: Template with exercises and configurations
- **When**: DELETE /api/workout-templates/{id}
- **Then**:
  - Template marked as deleted (soft delete)
  - All exercises removed from database
  - All configurations removed
  - No orphan records remain
  - 404 returned on subsequent GET requests

### State Transition Integration

#### Test: DRAFT to PRODUCTION Flow
- **Given**: DRAFT template with complete structure
- **When**: PUT /api/workout-templates/{id}/state with PRODUCTION state
- **Then**:
  - State transition succeeds with 200 OK
  - Any test execution logs are deleted
  - Template appears in public listings
  - State change logged in audit trail
  - Cache invalidated for template

#### Test: Block Invalid State Transitions
- **Given**: PRODUCTION template with execution logs
- **When**: Attempting PUT /api/workout-templates/{id}/state to DRAFT
- **Then**:
  - Request returns 409 Conflict
  - Error details execution log count
  - Template remains in PRODUCTION state
  - Audit log shows failed attempt
  - No data corruption occurs

### Exercise Management Integration

#### Test: Add Exercise with Auto-Associations
- **Given**: Exercise with warmup/cooldown associations exists
- **When**: Adding exercise to Main zone via API
- **Then**:
  - Main exercise added successfully
  - GET /api/workout-templates/{id}/exercise-suggestions includes associations
  - Associated exercises flagged in response
  - Suggestion reasons provided
  - Equipment requirements updated in real-time

#### Test: Reorder Exercises Within Zone
- **Given**: Multiple exercises in same zone
- **When**: Updating sequence orders via PUT requests
- **Then**:
  - All PUT requests succeed
  - No sequence conflicts occur
  - GET template shows correct order
  - Other zones remain unaffected
  - Database consistency maintained

### Reference Data Integration

#### Test: Reference Data Caching
- **Given**: Fresh application start
- **When**: Accessing reference endpoints
- **Then**:
  - GET /api/reference-tables/workout-states loads from database first time
  - Subsequent requests served from cache
  - Cache headers set for 365 days
  - ETags properly configured
  - 304 Not Modified responses for unchanged data

#### Test: Invalid Reference Validation
- **Given**: Template with invalid reference IDs
- **When**: Attempting to create/update via API
- **Then**:
  - API returns 400 Bad Request
  - Error identifies all invalid references
  - No partial data saved to database
  - Clear error messages returned
  - Valid alternatives suggested if available

## Cross-Feature Integration Tests

### Exercise Library Integration

#### Test: Exercise Availability Check
- **Given**: Template referencing specific exercises
- **When**: Exercise is deactivated in exercise library
- **Then**:
  - Existing templates remain valid and functional
  - Exercise cannot be added to new templates
  - Warning shown in template details API response
  - Alternative exercises suggested
  - Historical data preserved correctly

#### Test: Exercise Update Propagation
- **Given**: Exercise used in multiple templates
- **When**: Exercise details updated (name, equipment, muscles)
- **Then**:
  - All templates show updated information immediately
  - Equipment changes reflected in template equipment list
  - Category changes handled properly
  - No template modifications required
  - Cache invalidation occurs for affected templates

### User Management Integration

#### Test: Creator Permissions Verified
- **Given**: Template with specific creator
- **When**: Different users attempt various operations
- **Then**:
  - Creator has full permissions based on state
  - Other trainers can view public templates only
  - Regular users cannot modify any templates
  - Admin users have override access
  - All access attempts logged in audit trail

#### Test: Role Change Impact
- **Given**: User with Personal Trainer role who owns templates
- **When**: Role is revoked
- **Then**:
  - Cannot create new templates (403 Forbidden)
  - Can still modify existing owned templates in DRAFT
  - Templates remain accessible to authorized users
  - Appropriate error messages shown
  - Graceful permission degradation

## Data Consistency Tests

### Concurrent Access Tests

#### Test: Simultaneous Template Updates
- **Given**: Multiple users editing same template
- **When**: Concurrent save operations via API
- **Then**:
  - Optimistic locking prevents conflicts
  - First save succeeds with 200 OK
  - Second save gets 409 Conflict
  - Version numbers prevent overwrites
  - Users receive clear conflict message

#### Test: Race Condition Prevention
- **Given**: Rapid sequential API calls
- **When**: Adding/removing same exercise quickly
- **Then**:
  - Operations processed in order
  - Final state is consistent
  - No duplicate exercises in database
  - Proper error handling for conflicts
  - Audit trail shows accurate sequence

### Transaction Integrity Tests

#### Test: Atomic Template Creation
- **Given**: Complex template with many exercises
- **When**: Creation fails partway (e.g., invalid exercise ID)
- **Then**:
  - Entire operation rolled back
  - No partial data in database
  - Clear error returned via API
  - Can retry operation immediately
  - No data corruption or orphan records

#### Test: Bulk Operation Atomicity
- **Given**: Multiple templates selected for bulk update
- **When**: Bulk state change fails for one template
- **Then**:
  - Successful changes committed
  - Failed changes rolled back
  - Detailed results returned per template
  - Can retry failed items
  - Database in consistent state

## Performance Integration Tests

### Load Testing

#### Test: High Volume Template Queries
- **Given**: Database with 10,000+ templates
- **When**: Querying with complex filters via API
- **Then**:
  - Response time < 1 second
  - Proper pagination works correctly
  - Database indexes utilized efficiently
  - No memory leaks detected
  - Consistent performance across pages

#### Test: Concurrent User Load
- **Given**: 100 concurrent API users
- **When**: Mixed read/write operations
- **Then**:
  - All requests handled successfully
  - No deadlocks occur
  - Response times remain acceptable
  - Database connections managed properly
  - Graceful degradation under load

### Caching Integration

#### Test: Multi-Level Cache Coordination
- **Given**: Cached data at multiple levels (Redis, CDN, client)
- **When**: Template is modified via API
- **Then**:
  - API cache invalidated immediately
  - CDN cache purged if configured
  - Cache headers updated
  - Reference data cache retained
  - Consistent data served to all clients

#### Test: Cache Warmup Strategy
- **Given**: Cold start scenario
- **When**: First requests arrive after startup
- **Then**:
  - Critical reference data loaded first
  - Popular templates cached progressively
  - Reference data cached eternally
  - Lazy loading for remaining data
  - No timeout errors during warmup

## Error Handling Integration

### API Error Propagation

#### Test: Nested Operation Failures
- **Given**: Template operation with multiple steps
- **When**: Step 3 of 5 fails
- **Then**:
  - Previous steps rolled back cleanly
  - Specific error returned via API
  - Recovery suggestions provided
  - State remains consistent
  - Can resume operation if applicable

#### Test: External Service Failures
- **Given**: Dependency on exercise service
- **When**: Exercise service unavailable
- **Then**:
  - Graceful degradation occurs
  - Cached data used if available
  - Clear error message to user
  - Retry mechanism activated
  - Circuit breaker pattern implemented

### Validation Chain Tests

#### Test: Multi-Level Validation
- **Given**: Complex template submission via API
- **When**: Multiple validation rules apply
- **Then**:
  - All validations run completely
  - All errors collected and returned
  - Single response with all issues
  - Errors properly categorized
  - Field-level error mapping correct

#### Test: Business Rule Enforcement
- **Given**: Valid data but business rule violation
- **When**: Attempting operation via API
- **Then**:
  - Technical validation passes
  - Business rule check fails appropriately
  - Specific rule violation identified
  - Suggestion for resolution provided
  - No data corruption occurs

## Security Integration Tests

### Permission Chain Tests

#### Test: Multi-Level Permission Check
- **Given**: Nested resource access (template -> exercise -> configuration)
- **When**: Accessing configuration via API
- **Then**:
  - User authentication verified
  - Template access permission checked
  - Exercise access verified
  - Configuration permission validated
  - Complete audit trail created

#### Test: API Rate Limiting
- **Given**: Rate limits configured (1000/hour, 100/minute)
- **When**: Exceeding rate limits
- **Then**:
  - 429 Too Many Requests returned
  - Retry-After header included
  - Rate limit headers show remaining quota
  - Limits reset appropriately
  - No service disruption

## Monitoring Integration

### Metrics Collection

#### Test: End-to-End Metrics
- **Given**: Complete template workflow
- **When**: User creates and publishes template via API
- **Then**:
  - API response time metrics recorded
  - Business metrics updated (templates created)
  - Performance data collected per endpoint
  - Error rates tracked accurately
  - User journey analytics captured

#### Test: Alert Triggering
- **Given**: Monitoring thresholds configured
- **When**: Error rate exceeds threshold
- **Then**:
  - Alert generated in monitoring system
  - Relevant logs aggregated
  - Incident ticket created if configured
  - Notification sent to on-call
  - Auto-scaling triggered if applicable

## Database Integration Tests

### Migration Tests

#### Test: Database Migration Success
- **Given**: Clean database
- **When**: Running migrations
- **Then**:
  - All tables created correctly
  - Indexes properly configured
  - Foreign keys established
  - Reference data seeded
  - Rollback scripts work correctly

#### Test: Data Integrity Constraints
- **Given**: Database with constraints
- **When**: Attempting invalid operations
- **Then**:
  - Foreign key constraints enforced
  - Unique constraints respected
  - Check constraints validated
  - Appropriate errors returned
  - No constraint violations possible