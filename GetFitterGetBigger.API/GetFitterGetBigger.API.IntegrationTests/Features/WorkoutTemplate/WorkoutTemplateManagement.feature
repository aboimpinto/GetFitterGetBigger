@WorkoutTemplate @Management
Feature: Workout Template Management
    As a Personal Trainer
    I want to create and manage workout templates
    So that I can provide structured training programs to my clients

Background:
    Given the database is initialized with test data
    And the following workout states exist:
        | Value       | Description              | DisplayOrder |
        | DRAFT       | Template under construction | 1          |
        | PRODUCTION  | Active template for use     | 2          |
        | ARCHIVED    | Retired template           | 3          |
    And the following workout categories exist:
        | Name             | Description                                          |
        | Upper Body - Push| Push exercises targeting chest, shoulders, triceps   |
        | Upper Body - Pull| Pull exercises targeting back and biceps             |
        | Lower Body       | Lower body exercises for legs and glutes             |
        | Core             | Core stability and strength exercises                |
        | Full Body        | Compound exercises engaging multiple muscle groups   |
    And the following difficulty levels exist:
        | Name         | Description        | DisplayOrder |
        | Beginner     | For beginners      | 1           |
        | Intermediate | For intermediate   | 2           |
        | Advanced     | For advanced       | 3           |
    And the following workout objectives exist:
        | Name                 | Description                    |
        | Muscular Strength    | Build strength                 |
        | Muscular Hypertrophy | Build muscle mass              |
        | Muscular Endurance   | Improve endurance              |

# Basic CRUD Operations

Scenario: Create a new workout template
    Given I am a Personal Trainer with ID "user-01000001-0000-0000-0000-000000000001"
    When I create a new workout template with:
        | Field                     | Value                  |
        | Name                      | Full Body Strength Day |
        | Description               | Focus on compound movements |
        | CategoryId                | Full Body              |
        | DifficultyId              | Intermediate           |
        | EstimatedDurationMinutes  | 60                     |
        | IsPublic                  | true                   |
    Then the workout template should be created successfully
    And the template state should be "DRAFT"
    And the response should contain:
        | Field                     | Value                  |
        | Name                      | Full Body Strength Day |
        | WorkoutState              | DRAFT                  |
        | EstimatedDurationMinutes  | 60                     |

Scenario: Get workout template by ID
    Given a workout template exists with ID "workouttemplate-03000001-0000-0000-0000-000000000001"
    When I request the workout template by ID
    Then the response status should be OK
    And the response should include navigation properties:
        | Property    |
        | Category    |
        | Difficulty  |
        | WorkoutState |
        | Exercises   |
        | Objectives  |

Scenario: Update an existing workout template
    Given I am a Personal Trainer with ID "user-01000001-0000-0000-0000-000000000001"
    And I have created a workout template in DRAFT state
    When I update the workout template with:
        | Field        | Value                    |
        | Name         | Updated Strength Day     |
        | Description  | Updated description      |
    Then the workout template should be updated successfully

Scenario: Delete a workout template
    Given I am a Personal Trainer with ID "user-01000001-0000-0000-0000-000000000001"
    And I have created a workout template in DRAFT state
    When I delete the workout template
    Then the template should be soft deleted
    And the template should not appear in active template lists

# State Transitions

Scenario: Transition template from DRAFT to PRODUCTION
    Given I am a Personal Trainer with ID "user-01000001-0000-0000-0000-000000000001"
    And I have created a complete workout template in DRAFT state
    When I change the template state to "PRODUCTION"
    Then the state should change to "PRODUCTION"
    And the template should be available for execution

# TODO: Uncomment when execution logs are implemented
#Scenario: Block invalid state transition with execution logs
#    Given I am a Personal Trainer with ID "user-01000001-0000-0000-0000-000000000001"
#    And I have a workout template in PRODUCTION state
#    And execution logs exist for this template
#    When I attempt to change the state to "DRAFT"
#    Then the operation should fail with status "Conflict"
#    And the template should remain in "PRODUCTION" state

Scenario: Archive a workout template
    Given I am a Personal Trainer with ID "user-01000001-0000-0000-0000-000000000001"
    And I have a workout template in PRODUCTION state
    When I change the template state to "ARCHIVED"
    Then the state should change to "ARCHIVED"
    And the template should not appear in active template lists

# Filtering and Search

Scenario: Get paged workout templates
    Given I am a Personal Trainer with ID "user-01000001-0000-0000-0000-000000000001"
    And I have created 15 workout templates
    When I request my templates with page size 10
    Then I should receive 10 templates in the first page
    And the total count should be 15

Scenario: Search templates by name pattern
    Given the following workout templates exist:
        | Name                  |
        | Upper Body Power      |
        | Lower Body Strength   |
        | Upper Body Endurance  |
    When I search for templates with name containing "Upper"
    Then I should receive 2 templates
    And all template names should contain "Upper"

Scenario: Filter templates by category
    Given workout templates exist in different categories
    When I filter templates by category "Full Body"
    Then all returned templates should have category "Full Body"

Scenario: Filter templates by difficulty
    Given workout templates exist with different difficulty levels
    When I filter templates by difficulty "Intermediate"
    Then all returned templates should have difficulty "Intermediate"

# TODO: Uncomment when workout template objectives linking is implemented
#Scenario: Filter templates by objective
#    Given workout templates exist with different objectives
#    When I filter templates by objective "Muscular Hypertrophy"
#    Then all returned templates should have objective "Muscular Hypertrophy"

# Validation

Scenario Outline: Validate template name length
    Given I am a Personal Trainer
    When I create a workout template with name of <length> characters
    Then the operation should <result>

    Examples:
        | length | result |
        | 2      | fail   |
        | 3      | pass   |
        | 100    | pass   |
        | 101    | fail   |

Scenario Outline: Validate duration range
    Given I am a Personal Trainer
    When I create a workout template with duration <duration> minutes
    Then the operation should <result>

    Examples:
        | duration | result |
        | 4        | fail   |
        | 5        | pass   |
        | 300      | pass   |
        | 301      | fail   |

Scenario: Prevent duplicate template names globally
    Given I am a Personal Trainer with ID "user-01000001-0000-0000-0000-000000000001"
    And I have created a template named "My Workout"
    When I try to create another template named "My Workout"
    Then the operation should fail with status "Conflict"

# Access Control
# TODO: Uncomment these scenarios when authorization is implemented

#Scenario: Creator can modify own template
#    Given I am a Personal Trainer with ID "user-01000001-0000-0000-0000-000000000001"
#    And I have created a workout template in DRAFT state
#    When I update the template
#    Then the operation should succeed

#Scenario: Non-creator cannot modify template
#    Given a workout template exists created by another user
#    When I attempt to update the template
#    Then the operation should fail with status "Forbidden"

#Scenario: View public templates
#    Given a public workout template exists
#    When I request the template as any user
#    Then I should be able to view the template details

#Scenario: Cannot view private templates of others
#    Given a private workout template exists created by another user
#    When I request the template
#    Then the operation should fail with status "Forbidden"

# Template Duplication

Scenario: Duplicate a workout template
    Given I am a Personal Trainer with ID "user-01000001-0000-0000-0000-000000000001"
    And I have an existing workout template "Original Full Body Workout"
    When I duplicate the template with name "Duplicated Full Body Workout"
    Then a new template should be created in DRAFT state
    And all exercises and configurations should be copied
    And the original template should remain unchanged

# Performance

Scenario: Handle large number of templates efficiently
    Given 1000 workout templates exist in the system
    When I request templates with pagination (page 1, size 20)
    Then the response should return within 500ms
    And I should receive exactly 20 templates
    And the total count should be 1000