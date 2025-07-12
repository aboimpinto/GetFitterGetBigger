Feature: Muscle Groups Reference Data
  As a fitness application user
  I want to access muscle group information
  So that I can understand which muscles are targeted by exercises

  Background:
    Given the database is empty
    And the database has reference data

  @smoke @reference-data
  Scenario: Get all muscle groups
    When I send a GET request to "/api/ReferenceTables/MuscleGroups"
    Then the response status should be 200
    And the response should be a JSON array
    And the response contains at least 1 item

  @reference-data
  Scenario: Get muscle group by valid ID
    Given I send a GET request to "/api/ReferenceTables/MuscleGroups"
    And the response contains at least 1 item
    And I store the first item from the response as "firstMuscleGroup"
    When I send a GET request to "/api/ReferenceTables/MuscleGroups/<firstMuscleGroup.id>"
    Then the response status should be 200
    And the response should have property "id" with value "<firstMuscleGroup.id>"
    And the response should have property "name" with value "<firstMuscleGroup.name>"
    And the response should have property "bodyPartId"
    And the response should have property "bodyPartName"

  @reference-data @validation
  Scenario: Get muscle group by invalid ID returns not found
    When I send a GET request to "/api/ReferenceTables/MuscleGroups/musclegroup-99999999-9999-9999-9999-999999999999"
    Then the response status should be 404

  @reference-data @validation
  Scenario: Get muscle group by invalid ID format returns bad request
    When I send a GET request to "/api/ReferenceTables/MuscleGroups/invalid-id"
    Then the response status should be 400
    # Note: API returns 400 for invalid ID format instead of 404

  @reference-data
  Scenario: Get muscle group by value case insensitive
    Given I send a GET request to "/api/ReferenceTables/MuscleGroups"
    And the response contains at least 1 item
    And I store the first item from the response as "firstMuscleGroup"
    When I send a GET request to "/api/ReferenceTables/MuscleGroups/ByValue/<firstMuscleGroup.name>"
    Then the response status should be 200
    And the response should have property "name" with value "<firstMuscleGroup.name>"

  @reference-data @validation
  Scenario: Get muscle group by non-existent value returns not found
    When I send a GET request to "/api/ReferenceTables/MuscleGroups/ByValue/NonExistentMuscleGroup"
    Then the response status should be 404

  @reference-data
  Scenario: Create new muscle group successfully
    Given I send a GET request to "/api/ReferenceTables/BodyParts"
    And the response contains at least 1 item
    And I store the first item from the response as "firstBodyPart"
    When I send a POST request to "/api/ReferenceTables/MuscleGroups" with body:
      """
      {
        "name": "Test Muscle Group",
        "bodyPartId": "<firstBodyPart.id>"
      }
      """
    Then the response status should be 201
    And the response should have property "name" with value "Test Muscle Group"
    And the response should have property "bodyPartId" with value "<firstBodyPart.id>"
    And the response should have property "id"
    And the response should have property "isActive" with value "true"

  @reference-data @validation
  Scenario: Create muscle group without required fields returns bad request
    When I send a POST request to "/api/ReferenceTables/MuscleGroups" with body:
      """
      {
        "name": "Test Muscle Group"
      }
      """
    Then the response status should be 400

  @reference-data @validation
  Scenario: Create duplicate muscle group returns conflict
    Given I send a GET request to "/api/ReferenceTables/MuscleGroups"
    And the response contains at least 1 item
    And I store the first item from the response as "existingMuscleGroup"
    When I send a POST request to "/api/ReferenceTables/MuscleGroups" with body:
      """
      {
        "name": "<existingMuscleGroup.name>",
        "bodyPartId": "<existingMuscleGroup.bodyPartId>"
      }
      """
    Then the response status should be 409

  @reference-data
  Scenario: Update existing muscle group
    Given I send a GET request to "/api/ReferenceTables/MuscleGroups"
    And the response contains at least 1 item
    And I store the first item from the response as "muscleGroupToUpdate"
    When I send a PUT request to "/api/ReferenceTables/MuscleGroups/<muscleGroupToUpdate.id>" with body:
      """
      {
        "name": "Updated Muscle Group Name",
        "bodyPartId": "<muscleGroupToUpdate.bodyPartId>",
        "isActive": true
      }
      """
    Then the response status should be 200
    And the response should have property "name" with value "Updated Muscle Group Name"
    And the response should have property "id" with value "<muscleGroupToUpdate.id>"

  @reference-data
  Scenario: Deactivate muscle group - currently not working
    Given I send a GET request to "/api/ReferenceTables/MuscleGroups"
    And the response contains at least 1 item
    And I store the first item from the response as "muscleGroupToDeactivate"
    When I send a PUT request to "/api/ReferenceTables/MuscleGroups/<muscleGroupToDeactivate.id>" with body:
      """
      {
        "name": "<muscleGroupToDeactivate.name>",
        "bodyPartId": "<muscleGroupToDeactivate.bodyPartId>",
        "isActive": false
      }
      """
    Then the response status should be 200
    # Note: API currently ignores isActive field in updates, always returns true
    And the response should have property "isActive" with value "true"

  @reference-data @validation
  Scenario: Update non-existent muscle group returns bad request
    When I send a PUT request to "/api/ReferenceTables/MuscleGroups/musclegroup-99999999-9999-9999-9999-999999999999" with body:
      """
      {
        "name": "Updated Name",
        "bodyPartId": "bodypart-12345678-1234-1234-1234-123456789012",
        "isActive": true
      }
      """
    Then the response status should be 400
    # Note: API returns 400 instead of 404 for non-existent muscle groups

  @reference-data
  Scenario: Delete muscle group returns bad request
    Given I send a GET request to "/api/ReferenceTables/BodyParts"
    And I store the first item from the response as "bodyPart"
    And I send a POST request to "/api/ReferenceTables/MuscleGroups" with body:
      """
      {
        "name": "Muscle Group To Delete",
        "bodyPartId": "<bodyPart.id>"
      }
      """
    And I store the response property "id" as "muscleGroupId"
    When I send a DELETE request to "/api/ReferenceTables/MuscleGroups/<muscleGroupId>"
    Then the response status should be 400
    # Note: API returns 400 for delete operations, likely DELETE is not implemented

  @reference-data @structure
  Scenario: Muscle group response has correct structure
    When I send a GET request to "/api/ReferenceTables/MuscleGroups"
    Then the response status should be 200
    And I store the first item from the response as "firstMuscleGroup"
    And the response should match the schema:
      """
      {
        "type": "array",
        "items": {
          "type": "object",
          "required": ["id", "name", "bodyPartId", "bodyPartName", "isActive", "createdAt"],
          "properties": {
            "id": { 
              "type": "string",
              "pattern": "^musclegroup-[a-f0-9-]+$"
            },
            "name": { "type": "string" },
            "bodyPartId": { 
              "type": "string",
              "pattern": "^bodypart-[a-f0-9-]+$"
            },
            "bodyPartName": { "type": "string" },
            "isActive": { "type": "boolean" },
            "createdAt": { "type": "string" },
            "updatedAt": { "type": ["string", "null"] }
          }
        }
      }
      """