Feature: Exercise Weight Type Caching
  As a system administrator
  I want exercise weight type data to be cached
  So that repeated requests don't hit the database unnecessarily

  Background:
    Given the database is empty
    And the database has reference data
    And I am tracking database queries

  @caching @reference-data
  Scenario: Calling get all exercise weight types twice should only hit database once
    # First call should hit the database
    When I send a GET request to "/api/ReferenceTables/ExerciseWeightTypes"
    Then the response status should be 200
    And the database query count should be 1
    # Reset counter to clearly show second call uses cache
    Given I reset the database query counter
    # Second call should use cache and NOT hit the database
    When I send a GET request to "/api/ReferenceTables/ExerciseWeightTypes"
    Then the response status should be 200
    And the database query count should be 0
    
  @caching @reference-data
  Scenario: Calling get exercise weight type by ID twice should only hit database once
    Given I send a GET request to "/api/ReferenceTables/ExerciseWeightTypes"
    And the response contains at least 1 item
    And I store the first item from the response as "firstExerciseWeightType"
    And I reset the database query counter
    # First call should hit the database
    When I send a GET request to "/api/ReferenceTables/ExerciseWeightTypes/<firstExerciseWeightType.id>"
    Then the response status should be 200
    And the database query count should be 1
    # Reset counter to clearly show second call uses cache
    Given I reset the database query counter
    # Second call should use cache and NOT hit the database
    When I send a GET request to "/api/ReferenceTables/ExerciseWeightTypes/<firstExerciseWeightType.id>"
    Then the response status should be 200
    And the database query count should be 0
    
  @caching @reference-data
  Scenario: Get by value should also use cache
    Given I send a GET request to "/api/ReferenceTables/ExerciseWeightTypes"
    And the response contains an item with value "Weight Required"
    And I reset the database query counter
    # First call should hit the database
    When I send a GET request to "/api/ReferenceTables/ExerciseWeightTypes/ByValue/Weight Required"
    Then the response status should be 200
    And the database query count should be 1
    # Reset counter to clearly show second call uses cache
    Given I reset the database query counter
    # Second call should use cache and NOT hit the database
    When I send a GET request to "/api/ReferenceTables/ExerciseWeightTypes/ByValue/Weight Required"
    Then the response status should be 200
    And the database query count should be 0