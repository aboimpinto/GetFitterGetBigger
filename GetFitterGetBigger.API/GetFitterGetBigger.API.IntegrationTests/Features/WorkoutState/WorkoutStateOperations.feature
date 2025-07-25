Feature: Workout State Reference Data
  As a system user
  I want to access workout state reference data
  So that I can use them in workout template management

  Background:
    Given the database is empty
    And the database has reference data

  @reference-data @smoke
  Scenario: Get all workout states
    When I send a GET request to "/api/workout-states"
    Then the response status should be 200
    And the response should be a JSON array
    And the response should be an array with 3 items

  @reference-data
  Scenario: Get workout state by valid ID
    Given I send a GET request to "/api/workout-states"
    And the response contains at least 1 item
    And I store the first item from the response as "firstWorkoutState"
    When I send a GET request to "/api/workout-states/<firstWorkoutState.id>"
    Then the response status should be 200
    And the response should have property "id" with value "<firstWorkoutState.id>"
    And the response should have property "value" with value "<firstWorkoutState.value>"

  @reference-data @validation
  Scenario: Get workout state by invalid ID format returns bad request
    When I send a GET request to "/api/workout-states/7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c"
    Then the response status should be 400

  @reference-data @validation
  Scenario: Get workout state by empty GUID returns not found
    When I send a GET request to "/api/workout-states/workoutstate-00000000-0000-0000-0000-000000000000"
    Then the response status should be 400

  @reference-data @validation
  Scenario: Get workout state by non-existent ID returns not found
    When I send a GET request to "/api/workout-states/workoutstate-11111111-1111-1111-1111-111111111111"
    Then the response status should be 404

  @reference-data
  Scenario: Get workout state by valid value
    Given I send a GET request to "/api/workout-states"
    And the response contains at least 1 item
    And I store the first item from the response as "firstWorkoutState"
    When I send a GET request to "/api/workout-states/ByValue/<firstWorkoutState.value>"
    Then the response status should be 200
    And the response should have property "value" with value "<firstWorkoutState.value>"

  @reference-data @validation
  Scenario: Get workout state by non-existent value returns not found
    When I send a GET request to "/api/workout-states/ByValue/NonExistentState"
    Then the response status should be 404

  @reference-data
  Scenario Outline: Get workout state by known values
    When I send a GET request to "/api/workout-states/ByValue/<value>"
    Then the response status should be 200
    And the response should have property "value" with value "<value>"
    And the response should have property "description" with value "<description>"

    Examples:
      | value      | description                  |
      | DRAFT      | Template under construction  |
      | PRODUCTION | Active template for use      |
      | ARCHIVED   | Retired template             |

  @reference-data
  Scenario: Verify all seeded workout states exist
    When I send a GET request to "/api/workout-states"
    Then the response status should be 200
    And the response should be a JSON array
    And the response contains an item with value "DRAFT"
    And the response contains an item with value "PRODUCTION"
    And the response contains an item with value "ARCHIVED"