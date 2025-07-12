Feature: Movement Patterns Reference Data
  As a system user
  I want to access movement patterns reference data
  So that I can use them in exercise management

  Background:
    Given the database is empty
    And the database has reference data

  @reference-data @smoke
  Scenario: Get all movement patterns
    When I send a GET request to "/api/ReferenceTables/MovementPatterns"
    Then the response status should be 200
    And the response should be a JSON array

  @reference-data
  Scenario: Get movement pattern by valid ID
    Given I send a GET request to "/api/ReferenceTables/MovementPatterns"
    And the response contains at least 1 item
    And I store the first item from the response as "firstMovementPattern"
    When I send a GET request to "/api/ReferenceTables/MovementPatterns/<firstMovementPattern.id>"
    Then the response status should be 200
    And the response should have property "id" with value "<firstMovementPattern.id>"
    And the response should have property "value" with value "<firstMovementPattern.value>"

  @reference-data @validation
  Scenario: Get movement pattern by invalid ID format returns bad request
    When I send a GET request to "/api/ReferenceTables/MovementPatterns/abcdef12-3456-7890-abcd-ef1234567890"
    Then the response status should be 400
    And the response should contain "Invalid ID format"
    And the response should contain "Expected format: 'movementpattern-{guid}'"

  @reference-data @validation
  Scenario: Get movement pattern by non-existent ID returns not found
    When I send a GET request to "/api/ReferenceTables/MovementPatterns/movementpattern-00000000-0000-0000-0000-000000000000"
    Then the response status should be 404

  @reference-data
  Scenario: Get movement pattern by valid value
    Given I send a GET request to "/api/ReferenceTables/MovementPatterns"
    And the response contains at least 1 item
    And I store the first item from the response as "firstMovementPattern"
    When I send a GET request to "/api/ReferenceTables/MovementPatterns/ByValue/<firstMovementPattern.value>"
    Then the response status should be 200
    And the response should have property "value" with value "<firstMovementPattern.value>"

  @reference-data @validation
  Scenario: Get movement pattern by non-existent value returns not found
    When I send a GET request to "/api/ReferenceTables/MovementPatterns/ByValue/NonExistentMovementPattern"
    Then the response status should be 404

