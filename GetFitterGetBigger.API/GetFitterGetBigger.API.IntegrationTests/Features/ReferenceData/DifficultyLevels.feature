Feature: Difficulty Levels Reference Data
  As a system user
  I want to access difficulty levels reference data
  So that I can use them in exercise management

  Background:
    Given the database is empty
    And the database has reference data

  @reference-data @smoke
  Scenario: Get all difficulty levels
    When I send a GET request to "/api/ReferenceTables/DifficultyLevels"
    Then the response status should be 200
    And the response should be a JSON array

  @reference-data
  Scenario: Get difficulty level by valid ID
    Given I send a GET request to "/api/ReferenceTables/DifficultyLevels"
    And the response contains at least 1 item
    And I store the first item from the response as "firstDifficultyLevel"
    When I send a GET request to "/api/ReferenceTables/DifficultyLevels/<firstDifficultyLevel.Id>"
    Then the response status should be 200
    And the response should have property "id" with value "<firstDifficultyLevel.Id>"
    And the response should have property "value" with value "<firstDifficultyLevel.Value>"

  @reference-data @validation
  Scenario: Get difficulty level by invalid ID format returns bad request
    When I send a GET request to "/api/ReferenceTables/DifficultyLevels/8a8adb1d-24d2-4979-a5a6-0d760e6da24b"
    Then the response status should be 400
    And the response should contain "Invalid ID format"
    And the response should contain "Expected format: 'difficultylevel-{guid}'"

  @reference-data @validation
  Scenario: Get difficulty level by non-existent ID returns not found
    When I send a GET request to "/api/ReferenceTables/DifficultyLevels/difficultylevel-00000000-0000-0000-0000-000000000000"
    Then the response status should be 404

  @reference-data
  Scenario: Get difficulty level by valid value
    Given I send a GET request to "/api/ReferenceTables/DifficultyLevels"
    And the response contains at least 1 item
    And I store the first item from the response as "firstDifficultyLevel"
    When I send a GET request to "/api/ReferenceTables/DifficultyLevels/ByValue/<firstDifficultyLevel.Value>"
    Then the response status should be 200
    And the response should have property "value" with value "<firstDifficultyLevel.Value>"

  @reference-data @validation
  Scenario: Get difficulty level by non-existent value returns not found
    When I send a GET request to "/api/ReferenceTables/DifficultyLevels/ByValue/NonExistentDifficultyLevel"
    Then the response status should be 404

  @reference-data @validation
  Scenario Outline: Get difficulty level by value with different casing
    Given I send a GET request to "/api/ReferenceTables/DifficultyLevels"
    And the response contains an item with value "Beginner"
    When I send a GET request to "/api/ReferenceTables/DifficultyLevels/ByValue/<casing>"
    Then the response status should be 200
    And the response should have property "value" with value "Beginner"

    Examples:
      | casing   |
      | Beginner |
      | beginner |
      | BEGINNER |
      | BeGiNnEr |