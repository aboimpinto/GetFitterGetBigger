Feature: Exercise Weight Types Reference Data
  As a fitness application user
  I want to access exercise weight type information
  So that I can properly configure exercises with appropriate weight requirements

  Background:
    Given the database is empty
    And the database has reference data

  @smoke @reference-data
  Scenario: Get all exercise weight types
    When I send a GET request to "/api/ReferenceTables/ExerciseWeightTypes"
    Then the response status should be 200
    And the response should be a JSON array
    And the response should be an array with 5 items
    And the response should contain "Bodyweight Only"
    And the response should contain "Bodyweight Optional"
    And the response should contain "Weight Required"
    And the response should contain "Machine Weight"
    And the response should contain "No Weight"

  @reference-data
  Scenario: Get exercise weight type by valid ID
    Given I send a GET request to "/api/ReferenceTables/ExerciseWeightTypes"
    And the response contains at least 1 item
    And I store the first item from the response as "firstWeightType"
    When I send a GET request to "/api/ReferenceTables/ExerciseWeightTypes/<firstWeightType.id>"
    Then the response status should be 200
    And the response should have property "id" with value "<firstWeightType.id>"
    And the response should have property "value" with value "<firstWeightType.value>"
    And the response should have property "description"

  @reference-data @validation
  Scenario: Get exercise weight type by invalid ID returns not found
    When I send a GET request to "/api/ReferenceTables/ExerciseWeightTypes/exerciseweighttype-99999999-9999-9999-9999-999999999999"
    Then the response status should be 404

  @reference-data @validation
  Scenario: Get exercise weight type by invalid ID format returns bad request
    When I send a GET request to "/api/ReferenceTables/ExerciseWeightTypes/invalid-id"
    Then the response status should be 400

  @reference-data
  Scenario: Get exercise weight type by value - Bodyweight Only
    When I send a GET request to "/api/ReferenceTables/ExerciseWeightTypes/ByValue/Bodyweight Only"
    Then the response status should be 200
    And the response should have property "value" with value "Bodyweight Only"
    And the response should contain "cannot have external weight added"

  @reference-data
  Scenario: Get exercise weight type by value case insensitive
    When I send a GET request to "/api/ReferenceTables/ExerciseWeightTypes/ByValue/WEIGHT REQUIRED"
    Then the response status should be 200
    And the response should have property "value" with value "Weight Required"

  @reference-data @validation
  Scenario: Get exercise weight type by invalid value returns not found
    When I send a GET request to "/api/ReferenceTables/ExerciseWeightTypes/ByValue/NonExistentWeightType"
    Then the response status should be 404

  @reference-data
  Scenario: Get exercise weight type by URL encoded value
    When I send a GET request to "/api/ReferenceTables/ExerciseWeightTypes/ByValue/Machine%20Weight"
    Then the response status should be 200
    And the response should have property "value" with value "Machine Weight"

  @reference-data
  Scenario: Get exercise weight type by code - BODYWEIGHT_ONLY
    When I send a GET request to "/api/ReferenceTables/ExerciseWeightTypes/ByCode/BODYWEIGHT_ONLY"
    Then the response status should be 200
    And the response should have property "value" with value "Bodyweight Only"
    And the response should contain "cannot have external weight added"

  @reference-data
  Scenario Outline: Get exercise weight type by all valid codes
    When I send a GET request to "/api/ReferenceTables/ExerciseWeightTypes/ByCode/<code>"
    Then the response status should be 200
    And the response should have property "value" with value "<expectedValue>"

    Examples:
      | code                | expectedValue        |
      | BODYWEIGHT_ONLY     | Bodyweight Only      |
      | BODYWEIGHT_OPTIONAL | Bodyweight Optional  |
      | WEIGHT_REQUIRED     | Weight Required      |
      | MACHINE_WEIGHT      | Machine Weight       |
      | NO_WEIGHT           | No Weight            |

  @reference-data @validation
  Scenario: Get exercise weight type by code is case sensitive
    When I send a GET request to "/api/ReferenceTables/ExerciseWeightTypes/ByCode/bodyweight_only"
    Then the response status should be 404

  @reference-data @validation
  Scenario: Get exercise weight type by invalid code returns not found
    When I send a GET request to "/api/ReferenceTables/ExerciseWeightTypes/ByCode/INVALID_CODE"
    Then the response status should be 404

  @reference-data @structure
  Scenario: Exercise weight type response has correct structure
    When I send a GET request to "/api/ReferenceTables/ExerciseWeightTypes"
    Then the response status should be 200
    And I store the first item from the response as "firstWeightType"
    And the response should match the schema:
      """
      {
        "type": "array",
        "items": {
          "type": "object",
          "required": ["id", "value"],
          "properties": {
            "id": { 
              "type": "string",
              "pattern": "^exerciseweighttype-[a-f0-9-]+$"
            },
            "value": { "type": "string" },
            "description": { "type": ["string", "null"] }
          }
        }
      }
      """