Feature: Database Operations
  As a developer
  I want to ensure database operations work correctly
  So that the application can persist and retrieve data reliably

  @database @infrastructure
  Scenario: Database connection is available
    When I check database connectivity
    Then the database should be connected

  @database @migration
  Scenario: Database migrations run successfully on startup
    Given the database is empty
    When I check database connectivity
    Then the database should be connected
    And the database should have the expected schema

  @database @persistence
  Scenario: Data persists across multiple requests
    Given the database is empty
    And the database has reference data
    When I send a GET request to "/api/ReferenceTables/DifficultyLevels"
    Then the response status should be 200
    And I store the first item from the response as "firstItem"
    When I send a GET request to "/api/ReferenceTables/DifficultyLevels/<firstItem.id>"
    Then the response status should be 200
    And the response should have property "value" with value "<firstItem.value>"
    # This proves data persists between requests

  @database @reference-data
  Scenario: Reference data is properly seeded
    Given the database is empty
    And the database has reference data
    When I send a GET request to "/api/ReferenceTables/DifficultyLevels"
    Then the response contains at least 1 item
    When I send a GET request to "/api/ReferenceTables/BodyParts"
    Then the response contains at least 1 item
    When I send a GET request to "/api/ReferenceTables/ExerciseTypes"
    Then the response contains at least 1 item
    When I send a GET request to "/api/ReferenceTables/MuscleRoles"
    Then the response contains at least 1 item

  @database @query
  Scenario: Database queries work correctly
    Given the database is empty
    And the database has reference data
    When I send a GET request to "/api/ReferenceTables/MuscleGroups"
    Then the response status should be 200
    And the response should be a JSON array
    And the response should match the schema:
      """
      {
        "type": "array",
        "items": {
          "type": "object",
          "required": ["id", "name", "bodyPartId"],
          "properties": {
            "id": { "type": "string" },
            "name": { "type": "string" },
            "bodyPartId": { "type": "string" }
          }
        }
      }
      """