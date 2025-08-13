Feature: WorkoutTemplate Count Query Optimization
  As a fitness application
  I want count queries to be optimized and ignore includes
  So that count operations are performed efficiently

  Background:
    Given I am authenticated as a user
    And the database has reference data
    And the following workout templates exist for count tests:
      | Name           | Description                  | CategoryName       | DifficultyName | Duration | Tags          |
      | Test Workout 1 | Description for test workout | Upper Body - Push  | Beginner       | 50       | test,workout1 |
      | Test Workout 2 | Description for test workout | Upper Body - Push  | Intermediate   | 55       | test,workout2 |
      | Test Workout 3 | Description for test workout | Lower Body         | Beginner       | 60       | test,workout3 |
      | Test Workout 4 | Description for test workout | Lower Body         | Advanced       | 65       | test,workout4 |
      | Test Workout 5 | Description for test workout | Upper Body - Push  | Intermediate   | 70       | test,workout5 |

  @workouttemplate @count @performance @ignore
  Scenario: Count query ignores includes and executes simple count
    When I send a GET request to "/api/workout-templates"
    Then the response status should be 200
    And the response should have property "totalCount" with value "5"
    # The actual data with includes should also be returned
    And the response should have property "items" as array
    And the response should have property "items" as array with 5 items

  @workouttemplate @count @performance @ignore
  Scenario: Complex query with multiple filters executes count efficiently
    When I send a GET request to "/api/workout-templates?namePattern=Test&sortBy=name&sortOrder=asc"
    Then the response status should be 200
    And the response should have property "totalCount" with value "5"
    And the response should have property "items" as array with 5 items
    And the response items should be sorted by name ascending

  @workouttemplate @count @ignore
  Scenario: Count is accurate with pagination
    When I send a GET request to "/api/workout-templates?page=1&pageSize=2"
    Then the response status should be 200
    And the response should have property "totalCount" with value "5"
    And the response should have property "items" as array with 2 items
    
    When I send a GET request to "/api/workout-templates?page=3&pageSize=2"
    Then the response status should be 200
    And the response should have property "totalCount" with value "5"
    And the response should have property "items" as array with 1 items

  @workouttemplate @count @ignore
  Scenario: Count with no results returns zero
    When I send a GET request to "/api/workout-templates?namePattern=NonExistentWorkout"
    Then the response status should be 200
    And the response should have property "totalCount" with value "0"
    And the response should have property "items" as empty array

