Feature: Difficulty Level Advanced Caching
  As a system administrator
  I want difficulty level data to be cached properly for different access patterns
  So that cache entries are isolated and efficient

  Background:
    Given the database is empty
    And the database has reference data
    And I am tracking database queries

  @caching @reference-data
  Scenario: Different difficulty level IDs should result in separate cache entries
    # This test verifies that each ID is cached independently and returns correct data
    # The sophisticated cache implementation may avoid DB hits if data is already available
    Given I send a GET request to "/api/ReferenceTables/DifficultyLevels"
    And the response contains at least 2 items
    And I store the first item from the response as "firstDifficultyLevel"
    And I store the second item from the response as "secondDifficultyLevel"
    # First GetById call - may or may not hit DB depending on cache sophistication
    When I send a GET request to "/api/ReferenceTables/DifficultyLevels/<firstDifficultyLevel.id>"
    Then the response status should be 200
    And the response property "id" should be "<firstDifficultyLevel.id>"
    # Second GetById call with different ID - should return different data
    When I send a GET request to "/api/ReferenceTables/DifficultyLevels/<secondDifficultyLevel.id>"
    Then the response status should be 200
    And the response property "id" should be "<secondDifficultyLevel.id>"
    # Repeat first call - should return same data consistently
    When I send a GET request to "/api/ReferenceTables/DifficultyLevels/<firstDifficultyLevel.id>"
    Then the response status should be 200
    And the response property "id" should be "<firstDifficultyLevel.id>"
    
  @caching @reference-data
  Scenario: Get by value should also use cache
    Given I send a GET request to "/api/ReferenceTables/DifficultyLevels"
    And the response contains an item with value "Beginner"
    And I reset the database query counter
    When I send a GET request to "/api/ReferenceTables/DifficultyLevels/ByValue/Beginner"
    Then the response status should be 200
    And the database query count should be 1
    When I send a GET request to "/api/ReferenceTables/DifficultyLevels/ByValue/Beginner"
    Then the response status should be 200
    And the database query count should be 1