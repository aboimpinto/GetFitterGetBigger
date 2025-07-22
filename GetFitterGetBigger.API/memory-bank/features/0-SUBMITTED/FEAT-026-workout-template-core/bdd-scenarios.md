# Workout Template Core - BDD Scenarios

## Feature: Workout Template Management
As a Personal Trainer
I want to create and manage workout templates
So that I can provide structured training programs to my clients

### Scenario: Create a new workout template
Given I am a Personal Trainer
And I have valid workout reference data (category, objective, protocol)
When I create a new workout template with:
  | Field              | Value                    |
  | Name              | Upper Body Strength Day   |
  | Category          | Upper Body               |
  | Objective         | Muscular Strength        |
  | Protocol          | Standard                 |
  | Duration          | 60                       |
  | Difficulty        | Intermediate             |
  | IsPublic          | true                     |
Then the template should be created successfully
And the template state should be "DRAFT"
And the version should be "1.0.0"
And I should be set as the creator

### Scenario: Add exercises to workout template
Given I have a workout template in DRAFT state
And exercises exist in the system
When I add the following exercises:
  | Exercise        | Zone    | Sequence | Notes                          |
  | Arm Circles     | Warmup  | 1        | 30 seconds each direction      |
  | Bench Press     | Main    | 1        | Focus on controlled movement   |
  | Chest Stretch   | Cooldown| 1        | Hold for 30 seconds           |
Then all exercises should be added successfully
And the equipment list should show "Barbell, Bench"
And the exercise count should be 3

### Scenario: Configure sets for exercises
Given I have a workout template with exercises
When I configure sets for "Bench Press" with:
  | Sets | Reps  | Intensity      |
  | 3    | 8-12  | RPE 7-8        |
Then the configuration should be saved
And the exercise should show the target sets and reps

### Scenario: Transition template from DRAFT to PRODUCTION
Given I have a complete workout template in DRAFT state
And some test execution logs exist
When I transition the template to PRODUCTION state
Then the state should change to "PRODUCTION"
And all test execution logs should be deleted
And the template should be publicly available
And the version should increment

### Scenario: Block invalid state transition
Given I have a workout template in PRODUCTION state
And execution logs exist for this template
When I attempt to transition back to DRAFT state
Then the transition should fail with error "Cannot change state due to existing execution logs"
And the template should remain in PRODUCTION state

### Scenario: Archive a workout template
Given I have a workout template in any state
When I transition the template to ARCHIVED state
Then the state should change to "ARCHIVED"
And the template should become read-only
And historical execution logs should be preserved
And no new executions should be allowed

## Feature: Exercise Organization
As a Personal Trainer
I want to organize exercises by zones
So that workouts follow proper warm-up, main, and cool-down structure

### Scenario: Enforce unique sequence order within zone
Given I have a workout template
And an exercise exists in Main zone with sequence order 1
When I try to add another exercise to Main zone with sequence order 1
Then the operation should fail with error "Sequence order must be unique within zone"

### Scenario: Suggest associated warmup exercises
Given I have a workout template
And I add "Bench Press" to the Main zone
And "Bench Press" has associated warmup exercises
Then the system should suggest adding "Chest Stretch" and "Arm Circles" to Warmup zone

### Scenario: Reorder exercises within zone
Given I have multiple exercises in the Main zone
When I update the sequence orders to:
  | Exercise      | New Order |
  | Bench Press   | 2         |
  | Squats        | 1         |
  | Deadlifts     | 3         |
Then the exercises should be reordered correctly
And other zones should remain unaffected

## Feature: Template Duplication
As a Personal Trainer
I want to duplicate existing workout templates
So that I can create variations efficiently

### Scenario: Duplicate a workout template
Given I have an existing workout template "Original Upper Body"
When I duplicate it with name "Modified Upper Body"
Then a new template should be created in DRAFT state
And all exercises and configurations should be copied
And I should be set as the creator of the new template
And the original template should remain unchanged

## Feature: Equipment Management
As a system
I want to automatically aggregate equipment requirements
So that users know what equipment is needed

### Scenario: Aggregate equipment from exercises
Given I have a workout template with exercises:
  | Exercise        | Equipment               |
  | Bench Press     | Barbell, Bench         |
  | Dumbbell Curls  | Dumbbells              |
  | Pull-ups        | Pull-up Bar            |
When I view the template
Then the equipment list should show "Barbell, Bench, Dumbbells, Pull-up Bar"
And duplicates should be removed

### Scenario: Handle bodyweight exercises
Given I have a workout template with only bodyweight exercises
When I view the equipment requirements
Then the equipment list should show "None" or "Bodyweight"

## Feature: Access Control
As a system
I want to enforce proper access control
So that only authorized users can modify templates

### Scenario: Creator can modify own template
Given I created a workout template
When I attempt to modify the template
Then the modification should succeed
Based on the template state rules

### Scenario: Non-creator cannot modify template
Given a workout template created by another user
When I attempt to modify the template
Then the operation should fail with error "You don't have permission to perform this action"

### Scenario: View public templates
Given a public workout template exists
When I request the template as any user
Then I should be able to view the template details

### Scenario: Cannot view private templates
Given a private workout template created by another user
When I request the template
Then the operation should fail with 403 Forbidden

## Feature: Data Validation
As a system
I want to validate all input data
So that only valid workout templates are created

### Scenario: Validate template name length
Given I am creating a workout template
When I provide a name with <length> characters
Then the validation should <result>

Examples:
  | length | result  |
  | 2      | fail    |
  | 3      | pass    |
  | 100    | pass    |
  | 101    | fail    |

### Scenario: Validate duration range
Given I am creating a workout template
When I set the duration to <duration> minutes
Then the validation should <result>

Examples:
  | duration | result  |
  | 4        | fail    |
  | 5        | pass    |
  | 300      | pass    |
  | 301      | fail    |

### Scenario: Validate difficulty level
Given I am creating a workout template
When I set the difficulty to "<level>"
Then the validation should <result>

Examples:
  | level         | result  |
  | Beginner      | pass    |
  | Intermediate  | pass    |
  | Advanced      | pass    |
  | Expert        | fail    |

## Feature: Reference Data Validation
As a system
I want to validate all reference data
So that templates use valid categories, objectives, and protocols

### Scenario: Validate workout category exists
Given I am creating a workout template
When I provide an invalid workout category ID
Then the operation should fail with error "Invalid workout category"

### Scenario: Validate workout objective exists
Given I am creating a workout template
When I provide an invalid workout objective ID
Then the operation should fail with error "Invalid workout objective"

### Scenario: Validate execution protocol exists
Given I am creating a workout template
When I provide an invalid execution protocol ID
Then the operation should fail with error "Invalid execution protocol"

## Feature: Concurrent Access
As a system
I want to handle concurrent modifications
So that data integrity is maintained

### Scenario: Handle concurrent template updates
Given two users are editing the same template
When both users save changes simultaneously
Then the first save should succeed
And the second save should fail with a version conflict error
And the user should be prompted to refresh and retry

## Feature: Performance and Limits
As a system
I want to enforce reasonable limits
So that system performance is maintained

### Scenario: Warn about excessive exercises
Given I have a workout template
When I add more than 50 exercises
Then a warning should be displayed about template complexity
But the operation should still be allowed if confirmed

### Scenario: Limit tags per template
Given I have a workout template with 10 tags
When I try to add another tag
Then the operation should fail with error "Maximum 10 tags allowed"