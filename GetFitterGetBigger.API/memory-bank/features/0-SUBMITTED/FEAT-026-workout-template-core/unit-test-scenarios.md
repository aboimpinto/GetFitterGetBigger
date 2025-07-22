# Workout Template Core - Unit Test Scenarios

## Entity Validation Tests

### WorkoutTemplate Entity Tests

#### Test: Create Valid Workout Template
- **Given**: Valid workout template data
- **When**: Creating a new workout template
- **Then**: Template is created successfully with all required fields populated, default values set correctly, created timestamp set, initial state is DRAFT, version is "1.0.0"

#### Test: Reject Invalid Template Name
- **Given**: Template with invalid name (empty, too short <3 chars, too long >100 chars)
- **When**: Attempting to create template
- **Then**: Creation fails with validation error specifying name requirements

#### Test: Validate Duration Range
- **Given**: Template with duration outside valid range
- **When**: Attempting to set duration
- **Then**: Validation error for durations < 5 or > 300 minutes with error message specifying valid range

#### Test: Validate Difficulty Level
- **Given**: Template with invalid difficulty level
- **When**: Setting difficulty level
- **Then**: Only accepts "Beginner", "Intermediate", "Advanced" and rejects any other value

### WorkoutTemplateExercise Entity Tests

#### Test: Add Exercise to Zone
- **Given**: Valid exercise and zone
- **When**: Adding exercise to template
- **Then**: Exercise is added to correct zone with correct sequence order and exercise count is updated

#### Test: Validate Zone Values
- **Given**: Exercise with invalid zone
- **When**: Attempting to add exercise
- **Then**: Only accepts "Warmup", "Main", "Cooldown" and rejects invalid zone values

#### Test: Enforce Unique Sequence Order
- **Given**: Existing exercise with sequence order 1 in Main zone
- **When**: Adding another exercise with sequence order 1 to Main zone
- **Then**: Validation error occurs indicating duplicate sequence order

#### Test: Validate Exercise Notes Length
- **Given**: Exercise with notes exceeding maximum length
- **When**: Setting exercise notes
- **Then**: Validation error for notes > 500 characters

### SetConfiguration Entity Tests

#### Test: Create Valid Set Configuration
- **Given**: Valid configuration data
- **When**: Creating set configuration
- **Then**: Configuration created successfully with all fields validated

#### Test: Validate Target Sets Range
- **Given**: Configuration with invalid set count
- **When**: Setting target sets
- **Then**: Only accepts 1-100 sets and rejects values outside range

#### Test: Validate Rep Format
- **Given**: Various rep format inputs
- **When**: Setting target reps
- **Then**: Accepts single numbers ("12"), ranges ("8-12"), special values ("AMRAP"), rejects invalid formats

#### Test: Validate Duration Range
- **Given**: Configuration with time-based exercise
- **When**: Setting target duration
- **Then**: Accepts 1-3600 seconds, rejects negative values and values > 1 hour

## Service Layer Tests

### WorkoutTemplateService Tests

#### Test: Create Template with Service
- **Given**: Valid template data and authenticated Personal Trainer
- **When**: Creating template through service
- **Then**: ServiceResult.Success with created template, audit fields populated, state set to DRAFT

#### Test: Update Template Respects State
- **Given**: Template in various states (DRAFT, PRODUCTION, ARCHIVED)
- **When**: Attempting updates
- **Then**: DRAFT allows all updates, PRODUCTION allows limited updates, ARCHIVED blocks all updates

#### Test: Delete Template with Cascade
- **Given**: Template with exercises and configurations
- **When**: Deleting template
- **Then**: All related entities deleted, no orphan records, ServiceResult.Success

#### Test: Prevent Delete with Execution Logs
- **Given**: Template with execution history
- **When**: Attempting to delete
- **Then**: ServiceResult.Failure with appropriate error message

### State Management Service Tests

#### Test: Initial State is DRAFT
- **Given**: New workout template
- **When**: Template is created
- **Then**: State is set to DRAFT with all DRAFT permissions available

#### Test: DRAFT to PRODUCTION Transition
- **Given**: Template in DRAFT state
- **When**: Transitioning to PRODUCTION
- **Then**: State changes to PRODUCTION, test logs deleted, template publicly available, version increments

#### Test: Block PRODUCTION to DRAFT with Logs
- **Given**: PRODUCTION template with execution logs
- **When**: Attempting rollback to DRAFT
- **Then**: Transition blocked with error indicating execution logs exist

#### Test: Allow PRODUCTION to DRAFT without Logs
- **Given**: PRODUCTION template with no execution logs
- **When**: Rolling back to DRAFT
- **Then**: State changes to DRAFT, template removed from public access

#### Test: Archive Any State
- **Given**: Template in any state
- **When**: Archiving template
- **Then**: State changes to ARCHIVED, template becomes read-only, historical data preserved

### Exercise Management Service Tests

#### Test: Add Exercise with Validation
- **Given**: Template and exercise data
- **When**: Adding exercise through service
- **Then**: Exercise added with proper zone ordering, equipment list updated, ServiceResult.Success

#### Test: Reorder Exercises Within Zone
- **Given**: Multiple exercises in same zone
- **When**: Updating sequence orders
- **Then**: Orders updated correctly, no conflicts, other zones unaffected

#### Test: Auto-suggest Warmup Exercises
- **Given**: Main exercise with associated warmups
- **When**: Adding main exercise
- **Then**: Associated warmups suggested in response

#### Test: Aggregate Equipment from Exercises
- **Given**: Multiple exercises with equipment
- **When**: Calculating template equipment
- **Then**: All unique equipment listed, no duplicates, updates when exercises change

### SetConfiguration Service Tests

#### Test: Create Configuration with Validation
- **Given**: Valid configuration data
- **When**: Creating through service
- **Then**: Configuration created, all validations pass, ServiceResult.Success

#### Test: Update Configuration
- **Given**: Existing configuration
- **When**: Updating values
- **Then**: Updates applied, validations enforced, ServiceResult.Success

#### Test: Delete Configuration
- **Given**: Existing configuration
- **When**: Deleting
- **Then**: Configuration removed, no orphan data, ServiceResult.Success

## Repository Tests

### WorkoutTemplateRepository Tests

#### Test: GetById with Includes
- **Given**: Template ID
- **When**: Fetching with includes
- **Then**: Template returned with all navigation properties loaded

#### Test: GetAll with Filtering
- **Given**: Filter criteria (category, objective, state)
- **When**: Querying templates
- **Then**: Only matching templates returned, pagination applied correctly

#### Test: Specification Pattern Support
- **Given**: Complex query specification
- **When**: Applying specification
- **Then**: Correct results returned, efficient query generated

### Transaction Tests

#### Test: Atomic Template Creation
- **Given**: Complex template with many exercises
- **When**: Creation fails partway
- **Then**: Entire operation rolled back, no partial data

#### Test: Concurrent Access Handling
- **Given**: Multiple simultaneous updates
- **When**: Saving changes
- **Then**: Optimistic concurrency handled, version conflicts detected

## Validation Tests

### Input Validation Tests

#### Test: Name Validation Rules
- **Given**: Various name inputs
- **When**: Validating
- **Then**: 3-100 chars allowed, special chars limited to spaces/hyphens

#### Test: Tag Validation
- **Given**: Tag inputs
- **When**: Validating
- **Then**: Max 10 tags, each 2-30 chars, alphanumeric only

#### Test: Reference ID Validation
- **Given**: Reference IDs (category, objective, protocol)
- **When**: Validating
- **Then**: Must exist in reference tables, proper error messages

### Business Rule Validation Tests

#### Test: Zone Order Validation
- **Given**: Exercises in different zones
- **When**: Validating workout structure
- **Then**: Warmup precedes Main, Cooldown follows Main

#### Test: Equipment Requirement Validation
- **Given**: Exercises with equipment
- **When**: Validating template
- **Then**: Equipment list accurate and complete

#### Test: State Transition Rules
- **Given**: Various state transitions
- **When**: Validating transitions
- **Then**: Only valid transitions allowed, proper error messages

## Performance Tests

### Query Performance Tests

#### Test: Efficient Template List Query
- **Given**: Large dataset (10,000+ templates)
- **When**: Querying with filters
- **Then**: Results in < 500ms, proper indexes used

#### Test: Efficient Equipment Aggregation
- **Given**: Template with many exercises
- **When**: Calculating equipment
- **Then**: Calculation completes quickly, no duplicate processing

### Caching Tests

#### Test: Reference Data Caching
- **Given**: WorkoutState queries
- **When**: Multiple requests
- **Then**: First from database, subsequent from cache

#### Test: Cache Invalidation
- **Given**: Cached template data
- **When**: Template modified
- **Then**: Cache entry invalidated, fresh data on next request

## Error Handling Tests

### Service Error Tests

#### Test: Handle Repository Exceptions
- **Given**: Repository throws exception
- **When**: Service method executes
- **Then**: ServiceResult.Failure returned, exception logged, no unhandled exceptions

#### Test: Validation Error Aggregation
- **Given**: Multiple validation errors
- **When**: Validating input
- **Then**: All errors collected and returned together

### Controller Error Tests

#### Test: ServiceResult to ActionResult
- **Given**: Various ServiceResult outcomes
- **When**: Converting to ActionResult
- **Then**: Appropriate HTTP status codes, proper error formatting

## Security Tests

### Authorization Tests

#### Test: Personal Trainer Can Create
- **Given**: User with Personal Trainer role
- **When**: Creating template
- **Then**: Operation succeeds

#### Test: Regular User Cannot Create
- **Given**: User without Personal Trainer role
- **When**: Attempting creation
- **Then**: 403 Forbidden returned

#### Test: Owner Can Modify
- **Given**: Template creator
- **When**: Modifying template
- **Then**: Operation succeeds based on state rules

#### Test: Non-Owner Cannot Modify
- **Given**: Different user
- **When**: Attempting modification
- **Then**: 403 Forbidden returned