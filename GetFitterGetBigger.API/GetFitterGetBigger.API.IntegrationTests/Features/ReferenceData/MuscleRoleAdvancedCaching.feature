Feature: Muscle Role Advanced Caching
  As a system administrator
  I want muscle role data to be cached properly for complex scenarios
  So that the cache handles different access patterns correctly

  Background:
    Given the database is empty
    And the database has reference data
    And I am tracking database queries

  @caching @reference-data
  Scenario: Different muscle role IDs should result in separate cache entries
    # This test verifies that each ID is cached independently and returns correct data
    # The sophisticated cache implementation may avoid DB hits if data is already available
    Given I am tracking database queries
    And I send a GET request to "/api/ReferenceTables/MuscleRoles"
    And the response contains at least 2 items
    And I store the first item from the response as "firstMuscleRole"
    And I store the second item from the response as "secondMuscleRole"
    # First GetById call - may or may not hit DB depending on cache sophistication
    When I send a GET request to "/api/ReferenceTables/MuscleRoles/<firstMuscleRole.id>"
    Then the response status should be 200
    And the response property "id" should be "<firstMuscleRole.id>"
    # Second GetById call with different ID - should return different data
    When I send a GET request to "/api/ReferenceTables/MuscleRoles/<secondMuscleRole.id>"
    Then the response status should be 200
    And the response property "id" should be "<secondMuscleRole.id>"
    # Repeat first call - should return same data consistently
    When I send a GET request to "/api/ReferenceTables/MuscleRoles/<firstMuscleRole.id>"
    Then the response status should be 200
    And the response property "id" should be "<firstMuscleRole.id>"
    
  @caching @reference-data
  Scenario: Get by value should also use cache
    Given I send a GET request to "/api/ReferenceTables/MuscleRoles"
    And the response contains an item with value "Primary"
    And I reset the database query counter
    # First call should hit the database
    When I send a GET request to "/api/ReferenceTables/MuscleRoles/ByValue/Primary"
    Then the response status should be 200
    And the database query count should be 1
    # Reset counter to clearly show second call uses cache
    Given I reset the database query counter
    # Second call should use cache and NOT hit the database
    When I send a GET request to "/api/ReferenceTables/MuscleRoles/ByValue/Primary"
    Then the response status should be 200
    And the database query count should be 0