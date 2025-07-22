# Workout Template Core - Unit Tests

## Overview
This document specifies unit tests for the Workout Template Core feature. Tests are described in a technology-agnostic manner focusing on business logic validation.

## Entity Validation Tests

### WorkoutTemplate Entity Tests

#### Test: Create Valid Workout Template
**Given**: Valid workout template data
**When**: Creating a new workout template
**Then**: 
- Template is created successfully
- All required fields are populated
- Default values are set correctly
- Created timestamp is set
- Initial state is DRAFT
- Version is set to "1.0.0"

#### Test: Reject Invalid Template Name
**Given**: Template with invalid name (empty, too short, too long)
**When**: Attempting to create template
**Then**: 
- Creation fails with validation error
- Error specifies name requirements
- No template is created

#### Test: Validate Duration Range
**Given**: Template with duration outside valid range
**When**: Attempting to set duration
**Then**: 
- Validation error for durations < 5 or > 300 minutes
- Error message specifies valid range

#### Test: Validate Difficulty Level
**Given**: Template with invalid difficulty level
**When**: Setting difficulty level
**Then**: 
- Only accepts: Beginner, Intermediate, Advanced
- Rejects any other value
- Error specifies valid options

### WorkoutTemplateExercise Entity Tests

#### Test: Add Exercise to Zone
**Given**: Valid exercise and zone
**When**: Adding exercise to template
**Then**: 
- Exercise is added to correct zone
- Sequence order is set correctly
- Exercise count is updated

#### Test: Validate Zone Values
**Given**: Exercise with invalid zone
**When**: Attempting to add exercise
**Then**: 
- Only accepts: Warmup, Main, Cooldown
- Rejects invalid zone values
- Error specifies valid zones

#### Test: Enforce Unique Sequence Order
**Given**: Existing exercise with sequence order 1 in Main zone
**When**: Adding another exercise with sequence order 1 to Main zone
**Then**: 
- Validation error occurs
- Error indicates duplicate sequence order
- Suggests next available sequence number

#### Test: Validate Exercise Notes Length
**Given**: Exercise with notes exceeding maximum length
**When**: Setting exercise notes
**Then**: 
- Validation error for notes > 500 characters
- Current length and limit displayed

### SetConfiguration Entity Tests

#### Test: Create Valid Set Configuration
**Given**: Valid configuration data
**When**: Creating set configuration
**Then**: 
- Configuration created successfully
- All fields validated
- Associated with correct exercise

#### Test: Validate Target Sets Range
**Given**: Configuration with invalid set count
**When**: Setting target sets
**Then**: 
- Only accepts 1-100 sets
- Rejects values outside range
- Error specifies valid range

#### Test: Validate Rep Format
**Given**: Various rep format inputs
**When**: Setting target reps
**Then**: 
- Accepts single numbers: "12"
- Accepts ranges: "8-12"
- Accepts special values: "AMRAP"
- Rejects invalid formats

#### Test: Validate Duration Range
**Given**: Configuration with time-based exercise
**When**: Setting target duration
**Then**: 
- Accepts 1-3600 seconds
- Rejects negative values
- Rejects values > 1 hour

## Business Rule Tests

### State Management Tests

#### Test: Initial State is DRAFT
**Given**: New workout template
**When**: Template is created
**Then**: 
- State is set to DRAFT
- All DRAFT permissions are available
- Modification is allowed

#### Test: DRAFT to PRODUCTION Transition
**Given**: Template in DRAFT state
**When**: Transitioning to PRODUCTION
**Then**: 
- State changes to PRODUCTION
- All test execution logs are deleted
- Template becomes publicly available
- Version increments

#### Test: Block PRODUCTION to DRAFT with Logs
**Given**: PRODUCTION template with execution logs
**When**: Attempting to rollback to DRAFT
**Then**: 
- Transition is blocked
- Error indicates execution logs exist
- Suggests archiving instead

#### Test: Allow PRODUCTION to DRAFT without Logs
**Given**: PRODUCTION template with no execution logs
**When**: Rolling back to DRAFT
**Then**: 
- State changes to DRAFT
- Template removed from public access
- Modification enabled

#### Test: Archive Any State
**Given**: Template in any state
**When**: Archiving template
**Then**: 
- State changes to ARCHIVED
- Template becomes read-only
- Historical data preserved
- No new executions allowed

### Exercise Organization Tests

#### Test: Maintain Zone Sequence
**Given**: Exercises in different zones
**When**: Validating workout structure
**Then**: 
- Warmup exercises logically precede Main
- Cooldown exercises logically follow Main
- Each zone maintains independent sequencing

#### Test: Auto-suggest Warmup Exercises
**Given**: Main exercise with associated warmups
**When**: Adding main exercise
**Then**: 
- Associated warmups are suggested
- Suggestions appear in warmup zone
- User can accept or decline

#### Test: Warn on Warmup Removal
**Given**: Warmup exercise linked to main exercise
**When**: Removing warmup exercise
**Then**: 
- Warning displayed about association
- User can acknowledge and proceed
- Warning logged in template notes

### Equipment Management Tests

#### Test: Aggregate Equipment from Exercises
**Given**: Multiple exercises with equipment
**When**: Calculating template equipment
**Then**: 
- All unique equipment listed
- No duplicates in list
- Updates when exercises change

#### Test: Handle No Equipment Exercises
**Given**: Template with bodyweight exercises only
**When**: Checking equipment requirements
**Then**: 
- Equipment list shows "None" or "Bodyweight"
- Clear indication no equipment needed

### Template Modification Tests

#### Test: Allow Edit in DRAFT State
**Given**: Template in DRAFT state
**When**: Modifying any field
**Then**: 
- All modifications allowed
- Version remains same
- Last modified timestamp updates

#### Test: Restrict Edit in PRODUCTION State
**Given**: Template in PRODUCTION state
**When**: Attempting major modifications
**Then**: 
- Only minor edits allowed (notes, tags)
- Structure changes blocked
- Suggests creating new version

#### Test: Block Edit in ARCHIVED State
**Given**: Template in ARCHIVED state
**When**: Attempting any modification
**Then**: 
- All modifications blocked
- Read-only access enforced
- Error suggests duplicating template

## Data Integrity Tests

### Reference Data Tests

#### Test: Validate Category Reference
**Given**: Template with category ID
**When**: Validating references
**Then**: 
- Category must exist in reference table
- Invalid reference rejected
- Error identifies missing reference

#### Test: Validate Objective Reference
**Given**: Template with objective ID
**When**: Validating references
**Then**: 
- Objective must exist in reference table
- Invalid reference rejected

#### Test: Validate Protocol Reference
**Given**: Template with protocol ID
**When**: Validating references
**Then**: 
- Protocol must exist in reference table
- Invalid reference rejected

### Cascade Delete Tests

#### Test: Delete Template Cascades
**Given**: Template with exercises and configurations
**When**: Deleting template
**Then**: 
- All associated exercises deleted
- All set configurations deleted
- No orphan records remain

#### Test: Prevent Delete with Execution Logs
**Given**: Template with execution history
**When**: Attempting to delete
**Then**: 
- Deletion blocked
- Error indicates logs exist
- Suggests archiving instead

## Permission Tests

### Creator Permission Tests

#### Test: Creator Can Modify Own Template
**Given**: User who created template
**When**: Attempting modifications
**Then**: 
- All permitted operations succeed
- Based on template state rules

#### Test: Non-Creator Cannot Modify
**Given**: User who didn't create template
**When**: Attempting modifications
**Then**: 
- Modifications blocked
- Error indicates permission denied
- Can still view if public

### Role-Based Tests

#### Test: Personal Trainer Can Create
**Given**: User with Personal Trainer role
**When**: Creating new template
**Then**: 
- Creation succeeds
- User set as creator
- All features available

#### Test: Regular User Cannot Create
**Given**: User without Personal Trainer role
**When**: Attempting to create template
**Then**: 
- Creation blocked
- Error indicates insufficient role
- Suggests contacting trainer

## Edge Case Tests

### Concurrency Tests

#### Test: Handle Concurrent Modifications
**Given**: Two users editing same template
**When**: Both save changes
**Then**: 
- Last write wins
- Version conflict detected
- User notified of overwrite

#### Test: Prevent Duplicate State Transitions
**Given**: Template transitioning states
**When**: Another transition attempted
**Then**: 
- Second transition blocked
- Current state verified
- Appropriate error returned

### Data Limit Tests

#### Test: Maximum Exercises per Template
**Given**: Template approaching exercise limit
**When**: Adding exercises beyond reasonable limit (e.g., 100)
**Then**: 
- Warning about template complexity
- Performance implications noted
- Still allows if user confirms

#### Test: Maximum Tags per Template
**Given**: Template with multiple tags
**When**: Exceeding tag limit (e.g., 10)
**Then**: 
- Additional tags rejected
- Error specifies limit
- Suggests removing existing tags

## Performance Tests

### Query Performance Tests

#### Test: Efficient Template List Query
**Given**: Large number of templates
**When**: Querying with filters
**Then**: 
- Results returned within 500ms
- Proper indexes utilized
- Pagination works correctly

#### Test: Efficient Equipment Aggregation
**Given**: Template with many exercises
**When**: Calculating equipment list
**Then**: 
- Calculation completes quickly
- No duplicate processing
- Cached appropriately