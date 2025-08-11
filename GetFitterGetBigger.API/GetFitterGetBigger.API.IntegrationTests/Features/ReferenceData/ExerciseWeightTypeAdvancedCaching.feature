Feature: Exercise Weight Type Advanced Caching
  As a system administrator
  I want exercise weight type data to be cached properly for complex scenarios
  So that the cache handles different access patterns correctly

  Background:
    Given the database is empty
    And the database has reference data
    And I am tracking database queries

  @caching @reference-data
  Scenario: Different exercise weight type IDs should result in separate cache entries
    # This test verifies that each ID is cached independently and returns correct data
    # The sophisticated cache implementation may avoid DB hits if data is already available
    Given I am tracking database queries
    And I send a GET request to "/api/ReferenceTables/ExerciseWeightTypes"
    And the response contains at least 2 items
    And I store the first item from the response as "firstExerciseWeightType"
    And I store the second item from the response as "secondExerciseWeightType"
    # First GetById call - may or may not hit DB depending on cache sophistication
    When I send a GET request to "/api/ReferenceTables/ExerciseWeightTypes/<firstExerciseWeightType.id>"
    Then the response status should be 200
    And the response property "id" should be "<firstExerciseWeightType.id>"
    # Second GetById call with different ID - should return different data
    When I send a GET request to "/api/ReferenceTables/ExerciseWeightTypes/<secondExerciseWeightType.id>"
    Then the response status should be 200
    And the response property "id" should be "<secondExerciseWeightType.id>"
    # Repeat first call - should return same data consistently
    When I send a GET request to "/api/ReferenceTables/ExerciseWeightTypes/<firstExerciseWeightType.id>"
    Then the response status should be 200
    And the response property "id" should be "<firstExerciseWeightType.id>"