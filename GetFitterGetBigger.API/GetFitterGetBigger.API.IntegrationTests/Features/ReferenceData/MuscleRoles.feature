Feature: Muscle Roles Reference Data
  As a system user
  I want to access muscle roles reference data
  So that I can use them in exercise management

  Background:
    Given the database is empty
    And the database has reference data

  @reference-data @smoke
  Scenario: Get all muscle roles
    When I send a GET request to "/api/ReferenceTables/MuscleRoles"
    Then the response status should be 200
    And the response should be a JSON array

  @reference-data
  Scenario: Get muscle role by valid ID
    Given I send a GET request to "/api/ReferenceTables/MuscleRoles"
    And the response contains at least 1 item
    And I store the first item from the response as "firstMuscleRole"
    When I send a GET request to "/api/ReferenceTables/MuscleRoles/<firstMuscleRole.id>"
    Then the response status should be 200
    And the response should have property "id" with value "<firstMuscleRole.id>"
    And the response should have property "value" with value "<firstMuscleRole.value>"

  @reference-data @validation
  Scenario: Get muscle role by invalid ID format returns bad request
    When I send a GET request to "/api/ReferenceTables/MuscleRoles/abcdef12-3456-7890-abcd-ef1234567890"
    Then the response status should be 400
    And the response should contain "Invalid ID format"
    And the response should contain "Expected format: 'musclerole-{guid}'"

  @reference-data @validation
  Scenario: Get muscle role by empty GUID returns bad request
    When I send a GET request to "/api/ReferenceTables/MuscleRoles/musclerole-00000000-0000-0000-0000-000000000000"
    Then the response status should be 400

  @reference-data @validation
  Scenario: Get muscle role by non-existent ID returns not found
    When I send a GET request to "/api/ReferenceTables/MuscleRoles/musclerole-11111111-1111-1111-1111-111111111111"
    Then the response status should be 404

  @reference-data
  Scenario: Get muscle role by valid value
    Given I send a GET request to "/api/ReferenceTables/MuscleRoles"
    And the response contains at least 1 item
    And I store the first item from the response as "firstMuscleRole"
    When I send a GET request to "/api/ReferenceTables/MuscleRoles/ByValue/<firstMuscleRole.value>"
    Then the response status should be 200
    And the response should have property "value" with value "<firstMuscleRole.value>"

  @reference-data @validation
  Scenario: Get muscle role by non-existent value returns not found
    When I send a GET request to "/api/ReferenceTables/MuscleRoles/ByValue/NonExistentMuscleRole"
    Then the response status should be 404

  @reference-data @validation
  Scenario Outline: Get muscle role by value with different casing
    Given I send a GET request to "/api/ReferenceTables/MuscleRoles"
    And the response contains an item with value "Primary"
    When I send a GET request to "/api/ReferenceTables/MuscleRoles/ByValue/<casing>"
    Then the response status should be 200
    And the response should have property "value" with value "Primary"

    Examples:
      | casing  |
      | Primary |
      | primary |
      | PRIMARY |
      | PrImArY |