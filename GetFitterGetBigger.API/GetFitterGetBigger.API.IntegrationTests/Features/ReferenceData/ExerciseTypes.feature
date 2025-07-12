Feature: Exercise Types Reference Data
  As a system user
  I want to access exercise types reference data
  So that I can use them in exercise management

  Background:
    Given the database is empty
    And the database has reference data

  @reference-data @smoke
  Scenario: Get all exercise types
    When I send a GET request to "/api/ReferenceTables/ExerciseTypes"
    Then the response status should be 200
    And the response should be a JSON array

  @reference-data
  Scenario: Get exercise type by valid ID
    Given I send a GET request to "/api/ReferenceTables/ExerciseTypes"
    And the response contains at least 1 item
    And I store the first item from the response as "firstExerciseType"
    When I send a GET request to "/api/ReferenceTables/ExerciseTypes/<firstExerciseType.id>"
    Then the response status should be 200
    And the response should have property "id" with value "<firstExerciseType.id>"
    And the response should have property "value" with value "<firstExerciseType.value>"

  @reference-data @validation
  Scenario: Get exercise type by invalid ID format returns not found
    When I send a GET request to "/api/ReferenceTables/ExerciseTypes/invalid-id"
    Then the response status should be 404

  @reference-data @validation
  Scenario: Get exercise type by non-existent ID returns not found
    When I send a GET request to "/api/ReferenceTables/ExerciseTypes/exercisetype-99999999-9999-9999-9999-999999999999"
    Then the response status should be 404

  @reference-data
  Scenario: Get exercise type by valid value
    Given I send a GET request to "/api/ReferenceTables/ExerciseTypes"
    And the response contains at least 1 item
    And I store the first item from the response as "firstExerciseType"
    When I send a GET request to "/api/ReferenceTables/ExerciseTypes/ByValue/<firstExerciseType.value>"
    Then the response status should be 200
    And the response should have property "value" with value "<firstExerciseType.value>"

  @reference-data @validation
  Scenario: Get exercise type by non-existent value returns not found
    When I send a GET request to "/api/ReferenceTables/ExerciseTypes/ByValue/InvalidType"
    Then the response status should be 404

  @reference-data @validation
  Scenario Outline: Get exercise type by value with different casing
    Given I send a GET request to "/api/ReferenceTables/ExerciseTypes"
    And the response contains an item with value "Warmup"
    When I send a GET request to "/api/ReferenceTables/ExerciseTypes/ByValue/<casing>"
    Then the response status should be 200
    And the response should have property "value" with value "Warmup"

    Examples:
      | casing |
      | Warmup |
      | warmup |
      | WARMUP |
      | WaRmUp |