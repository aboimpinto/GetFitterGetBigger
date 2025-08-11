Feature: MovementPattern Caching
  As a system administrator
  I want movement pattern data to be cached
  So that repeated requests don't hit the database unnecessarily

  Background:
    Given the database is empty
    And the database has reference data
    And I am tracking database queries

  @caching @reference-data
  Scenario: Calling get all movement patterns twice should only hit database once
    # First call should hit the database
    When I send a GET request to "/api/ReferenceTables/MovementPatterns"
    Then the response status should be 200
    And the database query count should be 1
    # Reset counter to clearly show second call uses cache
    Given I reset the database query counter
    # Second call should use cache and NOT hit the database
    When I send a GET request to "/api/ReferenceTables/MovementPatterns"
    Then the response status should be 200
    And the database query count should be 0
    
  @caching @reference-data
  Scenario: Calling get movement pattern by ID twice should only hit database once
    Given I send a GET request to "/api/ReferenceTables/MovementPatterns"
    And the response contains at least 1 item
    And I store the first item from the response as "firstMovementPattern"
    And I reset the database query counter
    # First GetById call should hit the database
    When I send a GET request to "/api/ReferenceTables/MovementPatterns/<firstMovementPattern.id>"
    Then the response status should be 200
    And the database query count should be 1
    # Reset counter to clearly show second call uses cache
    Given I reset the database query counter
    # Second GetById call should use cache and NOT hit the database
    When I send a GET request to "/api/ReferenceTables/MovementPatterns/<firstMovementPattern.id>"
    Then the response status should be 200
    And the database query count should be 0