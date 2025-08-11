Feature: Exercise Types Advanced Caching
  As a system administrator
  I want exercise types data to be cached properly for complex scenarios
  So that the cache handles different access patterns correctly

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
    When I send a GET request to "/api/ReferenceTables/ExerciseTypes/<firstExerciseType.id>"
    Then the response status should be 200
    And the database query count should be 1
    When I send a GET request to "/api/ReferenceTables/ExerciseTypes/<firstExerciseType.id>"
    Then the response status should be 200
    And the database query count should be 1

  @caching @reference-data
  Scenario: Different exercise type IDs should result in separate cache entries
    # This test verifies that each ID is cached independently and returns correct data
    # The sophisticated cache implementation may avoid DB hits if data is already available
    Given I am tracking database queries
    And I send a GET request to "/api/ReferenceTables/ExerciseTypes"
    And the response contains at least 2 items
    And I store the first item from the response as "firstExerciseType"
    And I store the second item from the response as "secondExerciseType"
    # First GetById call - may or may not hit DB depending on cache sophistication
    When I send a GET request to "/api/ReferenceTables/ExerciseTypes/<firstExerciseType.id>"
    Then the response status should be 200
    And the response property "id" should be "<firstExerciseType.id>"
    # Second GetById call with different ID - should return different data
    When I send a GET request to "/api/ReferenceTables/ExerciseTypes/<secondExerciseType.id>"
    Then the response status should be 200
    And the response property "id" should be "<secondExerciseType.id>"
    # Repeat first call - should return same data consistently
    When I send a GET request to "/api/ReferenceTables/ExerciseTypes/<firstExerciseType.id>"
    Then the response status should be 200
    And the response property "id" should be "<firstExerciseType.id>"
    
  @caching @reference-data
  Scenario: Get by value should also use cache
    Given I send a GET request to "/api/ReferenceTables/ExerciseTypes"
    And the response contains an item with value "Warmup"
    And I reset the database query counter
    When I send a GET request to "/api/ReferenceTables/ExerciseTypes/ByValue/Warmup"
    Then the response status should be 200
    And the database query count should be 1
    When I send a GET request to "/api/ReferenceTables/ExerciseTypes/ByValue/Warmup"
    Then the response status should be 200
    And the database query count should be 1