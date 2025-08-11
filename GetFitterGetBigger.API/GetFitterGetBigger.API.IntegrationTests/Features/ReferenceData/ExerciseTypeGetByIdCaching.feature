Feature: ExerciseType GetById Caching
  As a system administrator
  I want exercise type GetById to be cached properly
  So that repeated calls for the same ID only hit the database once

  Background:
    Given the database is empty
    And the database has reference data
    And I am tracking database queries

  @caching @reference-data
  Scenario: Calling get exercise type by ID twice should only hit database once
    Given I send a GET request to "/api/ReferenceTables/ExerciseTypes"
    And the response contains at least 1 item
    And I store the first item from the response as "firstExerciseType"
    And I reset the database query counter
    # First call should hit the database
    When I send a GET request to "/api/ReferenceTables/ExerciseTypes/<firstExerciseType.id>"
    Then the response status should be 200
    And the database query count should be 1
    # Reset counter to clearly show second call uses cache
    Given I reset the database query counter
    # Second call should use cache and NOT hit the database
    When I send a GET request to "/api/ReferenceTables/ExerciseTypes/<firstExerciseType.id>"
    Then the response status should be 200
    And the database query count should be 0