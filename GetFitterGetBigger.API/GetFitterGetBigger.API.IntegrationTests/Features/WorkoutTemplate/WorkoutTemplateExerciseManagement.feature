Feature: Workout Template Exercise Management
    As a fitness system
    I want to manage exercises in workout templates
    So that I can create structured workout plans

Background:
    Given the system has been initialized with seed data

@workout_template @exercises @integration
Scenario: Add exercise to workout template
    Given a workout template "Leg Burning I" exists with ExecutionProtocol "REPS_AND_SETS"
    And a workout exercise "Barbell Squat" exists
    When I add exercise "Barbell Squat" to template "Leg Burning I" with phase "Workout" and round 1
    Then the response status should be 201
    And the exercise should be added to the template

@workout_template @exercises @integration
Scenario: Remove exercise from workout template
    Given a workout template "Test Template" exists with ExecutionProtocol "REPS_AND_SETS"
    And a workout exercise "Barbell Squat" exists
    And the exercise "Barbell Squat" has been added to the template
    When I remove the exercise "Barbell Squat" from the template
    Then the response status should be 200
    And the exercise should be removed from the template

@workout_template @exercises @integration
Scenario: Get template exercises
    Given a workout template "Complex Template" exists with ExecutionProtocol "REPS_AND_SETS"
    And the template has multiple exercises added
    When I get all exercises for the template
    Then the response status should be 200
    And the response should contain all template exercises

@workout_template @exercises @integration @validation
Scenario: Adding nonexistent exercise should fail
    Given a workout template "Test Template" exists with ExecutionProtocol "REPS_AND_SETS"
    When I add exercise "exercise-99999999-9999-9999-9999-999999999999" to template "Test Template" with phase "Workout" and round 1
    Then the response status should be 404
    And the error should contain "not found"